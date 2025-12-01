namespace AskAI.CLI.Commands

open System
open System.IO
open System.Net.Http
open System.Diagnostics
open System.Security.Cryptography
open System.Text
open AskAI.CLI

/// Deploy the Ask AI worker to Cloudflare
/// Demonstrates Fable compilation and Management API deployment
module Deploy =

    /// Compute hash of source files for change detection
    let private computeSourceHash (workerDir: string) : string =
        let files =
            Directory.GetFiles(workerDir, "*.fs", SearchOption.AllDirectories)
            |> Array.sort

        use sha = SHA256.Create()
        for file in files do
            let content = File.ReadAllBytes(file)
            sha.TransformBlock(content, 0, content.Length, null, 0) |> ignore
        sha.TransformFinalBlock([||], 0, 0) |> ignore
        BitConverter.ToString(sha.Hash).Replace("-", "").ToLowerInvariant()

    /// Run external process and capture output
    let private runProcess (command: string) (args: string) (workingDir: string) : Async<Result<string, string>> =
        async {
            let psi = ProcessStartInfo(
                FileName = command,
                Arguments = args,
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            )

            use proc = Process.Start(psi)
            let! stdout = proc.StandardOutput.ReadToEndAsync() |> Async.AwaitTask
            let! stderr = proc.StandardError.ReadToEndAsync() |> Async.AwaitTask
            proc.WaitForExit()

            if proc.ExitCode = 0 then
                return Ok stdout
            else
                return Error $"Command failed (exit {proc.ExitCode}): {stderr}"
        }

    /// Deploy worker via Management API
    let private deployWorker
        (httpClient: HttpClient)
        (accountId: string)
        (scriptName: string)
        (jsFiles: (string * byte[]) list)
        (d1DatabaseId: string)
        : Async<Result<string, string>> =
        async {
            let url = $"https://api.cloudflare.com/client/v4/accounts/{accountId}/workers/scripts/{scriptName}"

            // Build metadata JSON
            let resources = Config.defaultResourceNames
            let metadataJson = $"""{{
                "main_module": "Main.js",
                "compatibility_date": "2024-11-01",
                "compatibility_flags": ["nodejs_compat"],
                "bindings": [
                    {{ "type": "d1", "name": "DB", "id": "{d1DatabaseId}" }},
                    {{ "type": "ai", "name": "AI" }},
                    {{ "type": "plain_text", "name": "ALLOWED_ORIGIN", "text": "*" }},
                    {{ "type": "plain_text", "name": "AUTORAG_NAME", "text": "{resources.AutoRAGName}" }}
                ]
            }}"""

            // Build multipart form
            use form = new MultipartFormDataContent()
            form.Add(new StringContent(metadataJson), "metadata")

            for (filename, content) in jsFiles do
                let fileContent = new ByteArrayContent(content)
                fileContent.Headers.ContentType <- Headers.MediaTypeHeaderValue("application/javascript+module")
                form.Add(fileContent, filename, filename)

            let! response = httpClient.PutAsync(url, form) |> Async.AwaitTask
            let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask

            if response.IsSuccessStatusCode then
                // Get subdomain
                let subdomainUrl = $"https://api.cloudflare.com/client/v4/accounts/{accountId}/workers/subdomain"
                let! subResponse = httpClient.GetAsync(subdomainUrl) |> Async.AwaitTask
                let! subBody = subResponse.Content.ReadAsStringAsync() |> Async.AwaitTask

                let workerUrl =
                    if subResponse.IsSuccessStatusCode then
                        try
                            use doc = System.Text.Json.JsonDocument.Parse(subBody)
                            let subdomain = doc.RootElement.GetProperty("result").GetProperty("subdomain").GetString()
                            $"https://{scriptName}.{subdomain}.workers.dev"
                        with _ ->
                            $"https://{scriptName}.workers.dev"
                    else
                        $"https://{scriptName}.workers.dev"

                return Ok workerUrl
            else
                return Error $"Deployment failed: {body}"
        }

    /// Execute the deploy command
    let execute
        (config: Config.CloudflareConfig)
        (workerDir: string option)
        (force: bool)
        : Async<Result<unit, string>> =
        async {
            let workerPath = workerDir |> Option.defaultValue "../AskAI/Worker"
            let mutable state = Config.loadState()

            // Check if resources are provisioned
            match state.D1DatabaseId with
            | None ->
                return Error "Resources not provisioned. Run 'provision' first."
            | Some d1Id ->

            // Check for source changes
            let currentHash = computeSourceHash workerPath
            if not force && state.LastDeployHash = Some currentHash then
                printfn "No changes detected. Use --force to redeploy."
                match state.WorkerUrl with
                | Some url -> printfn "Current URL: %s" url
                | None -> ()
                return Ok ()
            else

            printfn "Deploying Ask AI worker..."
            printfn ""

            // 1. Install npm dependencies
            let nodeModulesPath = Path.Combine(workerPath, "node_modules")
            if not (Directory.Exists(nodeModulesPath)) then
                printfn "  [1/4] Installing npm dependencies..."
                match! runProcess "npm" "install" workerPath with
                | Error e -> return Error e
                | Ok _ -> printfn "        Done"
            else
                printfn "  [1/4] npm dependencies already installed"

            // 2. Compile F# to JavaScript via Fable
            printfn "  [2/4] Compiling F# to JavaScript..."
            match! runProcess "dotnet" "fable src -o dist --noCache" workerPath with
            | Error e -> return Error e
            | Ok _ -> printfn "        Done"

            // 3. Gather compiled JS files
            printfn "  [3/4] Gathering compiled modules..."
            let distDir = Path.Combine(workerPath, "dist")
            let jsFiles =
                Directory.GetFiles(distDir, "*.js", SearchOption.AllDirectories)
                |> Array.map (fun path ->
                    let relativePath = Path.GetRelativePath(distDir, path).Replace("\\", "/")
                    let content = File.ReadAllBytes(path)
                    (relativePath, content))
                |> List.ofArray

            printfn "        Found %d JavaScript modules" jsFiles.Length

            // 4. Deploy via Management API
            printfn "  [4/4] Uploading to Cloudflare..."
            use httpClient = new HttpClient()
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiToken}")

            let resources = Config.defaultResourceNames
            match! deployWorker httpClient config.AccountId resources.WorkerName jsFiles d1Id with
            | Error e -> return Error e
            | Ok workerUrl ->

            // Update state
            state <- {
                state with
                    WorkerDeployed = true
                    WorkerUrl = Some workerUrl
                    LastDeployHash = Some currentHash
            }
            Config.saveState state

            printfn ""
            printfn "Deployment complete!"
            printfn ""
            printfn "  Worker URL: %s" workerUrl

            return Ok ()
        }
