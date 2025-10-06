module Sample.CLI.Commands.Deploy

open System
open System.IO
open System.Diagnostics
open System.Net.Http
open System.Text.Json
open Sample.CLI.Config
open Spectre.Console

let execute (config: CloudflareConfig) (workerPath: string option) (force: bool) : Async<Result<unit, string>> =
    async {
        // Default to the R2WebDAV sample if no path provided
        let workerDir =
            match workerPath with
            | Some path -> path
            | None ->
                // Assume CLI is in samples/Sample-CLI, worker is in samples/R2WebDAV
                let cliDir = Directory.GetCurrentDirectory()
                Path.Combine(cliDir, "..", "R2WebDAV")

        let workerDirFull = Path.GetFullPath(workerDir)

        if not (Directory.Exists(workerDirFull)) then
            return Error $"Worker directory not found: {workerDirFull}"
        else

        // Detect the main F# file to determine the entry point
        let fsprojFiles = Directory.GetFiles(workerDirFull, "*.fsproj")
        if fsprojFiles.Length = 0 then
            return Error $"No .fsproj file found in {workerDirFull}"
        else

        let fsprojPath = fsprojFiles.[0]
        let projectName = Path.GetFileNameWithoutExtension(fsprojPath)

        // Parse fsproj to find the last Compile Include file (which is the entry point)
        let fsprojContent = File.ReadAllText(fsprojPath)
        let compileIncludes =
            System.Text.RegularExpressions.Regex.Matches(fsprojContent, "<Compile Include=\"([^\"]+)\" />")
            |> Seq.cast<System.Text.RegularExpressions.Match>
            |> Seq.map (fun m -> m.Groups.[1].Value)
            |> Seq.toList

        if compileIncludes.IsEmpty then
            return Error $"No Compile Include entries found in {fsprojPath}"
        else

        let entryPointFile = compileIncludes |> List.last
        let entryPointName = Path.GetFileNameWithoutExtension(entryPointFile)
        let mainFsFile = Path.Combine(workerDirFull, entryPointFile)

        if not (File.Exists(mainFsFile)) then
            return Error $"Entry point file not found: {mainFsFile}"
        else

        // Use project name as worker name (convert to lowercase with hyphens)
        let workerName = projectName.ToLowerInvariant()

        AnsiConsole.MarkupLine($"[blue]Deploying:[/] {workerName}")
        AnsiConsole.MarkupLine($"[dim]Source:[/] {workerDirFull}")
        AnsiConsole.MarkupLine($"[dim]Entry point:[/] {entryPointName}.js")

        // Step 1: Install npm dependencies if needed
        let packageJsonPath = Path.Combine(workerDirFull, "package.json")
        let nodeModulesPath = Path.Combine(workerDirFull, "node_modules")

        // Install npm dependencies if needed
        let! npmCheckResult =
            async {
                if File.Exists(packageJsonPath) && not (Directory.Exists(nodeModulesPath)) then
                    let! npmInstallResult =
                        AnsiConsole.Status()
                            .StartAsync("Installing npm dependencies...", fun ctx ->
                                async {
                                    try
                                        let psi = ProcessStartInfo()
                                        psi.FileName <- "npm"
                                        psi.Arguments <- "install"
                                        psi.WorkingDirectory <- workerDirFull
                                        psi.RedirectStandardOutput <- true
                                        psi.RedirectStandardError <- true
                                        psi.UseShellExecute <- false
                                        psi.CreateNoWindow <- true

                                        use proc = Process.Start(psi)
                                        let! _ = proc.WaitForExitAsync() |> Async.AwaitTask

                                        if proc.ExitCode = 0 then
                                            AnsiConsole.MarkupLine("[green]✓[/] npm install succeeded")
                                            return Ok ()
                                        else
                                            let stderr = proc.StandardError.ReadToEnd()
                                            let escaped = Markup.Escape(stderr)
                                            AnsiConsole.MarkupLine($"[red]✗[/] npm install failed: {escaped}")
                                            return Error $"npm install failed: {stderr}"
                                    with ex ->
                                        let escaped = Markup.Escape(ex.Message)
                                        AnsiConsole.MarkupLine($"[red]✗[/] Error running npm: {escaped}")
                                        return Error $"Error running npm: {ex.Message}"
                                } |> Async.StartAsTask)
                            |> Async.AwaitTask
                    return npmInstallResult
                else
                    return Ok ()
            }

        match npmCheckResult with
        | Error err -> return Error err
        | Ok () ->

        // Step 2: Check if source has changed since last deploy
        let stateFilePath = Path.Combine(workerDirFull, ".deploy-state")
        let sourceFiles = Directory.GetFiles(workerDirFull, "*.fs", SearchOption.AllDirectories)

        // Compute hash of all source files
        let computeSourceHash () =
            sourceFiles
            |> Array.map File.ReadAllText
            |> String.concat ""
            |> fun s ->
                use sha = System.Security.Cryptography.SHA256.Create()
                s |> Text.Encoding.UTF8.GetBytes |> sha.ComputeHash |> Convert.ToHexString

        let currentHash = computeSourceHash()

        // Check if we can skip deployment (unless force is specified)
        let shouldDeploy =
            if force then
                true
            elif File.Exists(stateFilePath) then
                let lastState = File.ReadAllText(stateFilePath)
                let lastHash =
                    if lastState.Contains("|") then lastState.Split('|').[0]
                    else ""
                lastHash <> currentHash
            else
                true

        if not shouldDeploy then
            AnsiConsole.MarkupLine("[yellow]⊘[/] No changes detected since last deployment. Skipping.")
            AnsiConsole.MarkupLine("[dim]Use --force to deploy anyway[/]")
            return Ok ()
        else

        if force then
            AnsiConsole.MarkupLine("[yellow]⚡[/] Force deployment requested")
            AnsiConsole.WriteLine()

        // Step 3: Incremental build with Fable (no clean - let Fable handle it)

        let! fableResult =
            AnsiConsole.Status()
                .StartAsync("Compiling F# to JavaScript with Fable...", fun ctx ->
                    async {
                        try
                            let psi = ProcessStartInfo()
                            psi.FileName <- "fable"
                            psi.Arguments <- ". --outDir dist"
                            psi.WorkingDirectory <- workerDirFull
                            psi.RedirectStandardOutput <- true
                            psi.RedirectStandardError <- true
                            psi.UseShellExecute <- false
                            psi.CreateNoWindow <- true

                            use proc = Process.Start(psi)
                            let! _ = proc.WaitForExitAsync() |> Async.AwaitTask

                            if proc.ExitCode = 0 then
                                return Ok ()
                            else
                                let stderr = proc.StandardError.ReadToEnd()
                                return Error $"Fable compilation failed: {stderr}"
                        with ex ->
                            return Error $"Error running Fable: {ex.Message}"
                    } |> Async.StartAsTask)
                |> Async.AwaitTask

        match fableResult with
        | Error err -> return Error err
        | Ok () ->

        // Step 2: Find all compiled JavaScript files
        let distPath = Path.Combine(workerDirFull, "dist")
        let mainJsPath = Path.Combine(distPath, $"{entryPointName}.js")

        if not (File.Exists(mainJsPath)) then
            return Error $"Compiled worker not found: {mainJsPath}"
        else

        // Get all .js files in dist directory (including subdirectories for fable_modules)
        let jsFiles = Directory.GetFiles(distPath, "*.js", SearchOption.AllDirectories)

        // Step 3: Get current bindings to preserve them
        let httpClient = new HttpClient()
        httpClient.BaseAddress <- Uri("https://api.cloudflare.com/client/v4")
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiToken}")

        let! existingBindings =
            async {
                try
                    let! settingsResult =
                        CloudFlare.Api.Compute.Workers.Http.OpenApiHttp.getAsync
                            httpClient
                            "/accounts/{account_id}/workers/scripts/{script_name}/settings"
                            [CloudFlare.Api.Compute.Workers.Http.RequestPart.path("account_id", config.AccountId)
                             CloudFlare.Api.Compute.Workers.Http.RequestPart.path("script_name", workerName)]
                            None

                    let (_, settingsContent) = settingsResult
                    use settingsJsonDoc = JsonDocument.Parse(settingsContent)
                    let settingsJson = settingsJsonDoc.RootElement

                    let mutable resultProp = Unchecked.defaultof<JsonElement>
                    if settingsJson.TryGetProperty("result", &resultProp) then
                        let mutable bindingsProp = Unchecked.defaultof<JsonElement>
                        if resultProp.TryGetProperty("bindings", &bindingsProp) then
                            return bindingsProp.GetRawText()
                        else
                            return "[]"
                    else
                        return "[]"
                with
                | _ -> return "[]"
            }

        // Step 4: Create metadata for worker version upload matching wrangler's format
        // See: https://github.com/cloudflare/workers-sdk/blob/main/packages/wrangler/src/deployment-bundle/create-worker-upload-form.ts
        // Note: For POST /versions, we include bindings. For PUT to existing script, bindings come from settings.
        let metadata =
            JsonSerializer.Serialize(
                {|
                    main_module = $"{entryPointName}.js"
                    compatibility_date = "2024-01-01"
                    compatibility_flags = [| "nodejs_compat" |]
                    bindings = JsonSerializer.Deserialize<JsonElement>(existingBindings)
                |},
                JsonSerializerOptions(WriteIndented = false)
            )

        // Step 5: Upload worker to Cloudflare
        use formData = new MultipartFormDataContent()
        formData.Add(new StringContent(metadata), "metadata")

        // Add all JavaScript files with relative paths from dist directory
        for jsFile in jsFiles do
            let relativePath = Path.GetRelativePath(distPath, jsFile).Replace("\\", "/")
            let scriptBytes = File.ReadAllBytes(jsFile)
            let scriptContent = new ByteArrayContent(scriptBytes)
            scriptContent.Headers.ContentType <- Headers.MediaTypeHeaderValue("application/javascript+module")
            formData.Add(scriptContent, relativePath, relativePath)

        let absoluteUrl = $"https://api.cloudflare.com/client/v4/accounts/{config.AccountId}/workers/scripts/{workerName}"

        let! (response, content) =
            AnsiConsole.Status()
                .StartAsync("Uploading worker to Cloudflare...", fun ctx ->
                    async {
                        let! resp = httpClient.PutAsync(absoluteUrl, formData) |> Async.AwaitTask
                        let! cont = resp.Content.ReadAsStringAsync() |> Async.AwaitTask
                        return (resp, cont)
                    } |> Async.StartAsTask)
                |> Async.AwaitTask

        let! uploadResult =
            async {
                try
                    use jsonDoc = JsonDocument.Parse(content)
                    let json = jsonDoc.RootElement

                    let mutable successProp = Unchecked.defaultof<JsonElement>
                    if json.TryGetProperty("success", &successProp) && successProp.GetBoolean() then
                        return Ok ()
                    else
                        let mutable errorsProp = Unchecked.defaultof<JsonElement>
                        if json.TryGetProperty("errors", &errorsProp) && errorsProp.ValueKind = JsonValueKind.Array then
                            let errors =
                                errorsProp.EnumerateArray()
                                |> Seq.map (fun e ->
                                    let mutable code = Unchecked.defaultof<JsonElement>
                                    let mutable msg = Unchecked.defaultof<JsonElement>
                                    let codeStr = if e.TryGetProperty("code", &code) then $"[{code.GetInt32()}]" else ""
                                    let msgStr = if e.TryGetProperty("message", &msg) then msg.GetString() else "Unknown error"
                                    $"{codeStr} {msgStr}")
                                |> String.concat("\n")
                            return Error errors
                        else
                            return Error content
                with ex ->
                    return Error $"HTTP request failed: {ex.Message}"
            }

        match uploadResult with
        | Ok () ->
            // Save deployment state for idempotency
            File.WriteAllText(stateFilePath, $"{currentHash}|{DateTime.UtcNow:O}")
            AnsiConsole.MarkupLine("[green]✓[/] Deployed successfully!")

            // Get account subdomain and enable worker on workers.dev
            let! subdomainResult =
                async {
                    try
                        // Get the account subdomain
                        let getSubdomainUrl = $"https://api.cloudflare.com/client/v4/accounts/{config.AccountId}/workers/subdomain"
                        let! subdomainResponse = httpClient.GetAsync(getSubdomainUrl) |> Async.AwaitTask
                        let! subdomainContent = subdomainResponse.Content.ReadAsStringAsync() |> Async.AwaitTask

                        use jsonDoc = JsonDocument.Parse(subdomainContent)
                        let json = jsonDoc.RootElement
                        let mutable resultProp = Unchecked.defaultof<JsonElement>
                        let mutable subdomainProp = Unchecked.defaultof<JsonElement>

                        if json.TryGetProperty("result", &resultProp) &&
                           resultProp.TryGetProperty("subdomain", &subdomainProp) then
                            let subdomain = subdomainProp.GetString()

                            // Enable the worker on workers.dev by updating settings
                            let enableUrl = $"https://api.cloudflare.com/client/v4/accounts/{config.AccountId}/workers/scripts/{workerName}/subdomain"
                            let enableBody = """{"enabled":true}"""
                            use enableContent = new StringContent(enableBody, Text.Encoding.UTF8, "application/json")
                            let! enableResponse = httpClient.PostAsync(enableUrl, enableContent) |> Async.AwaitTask
                            let! enableResponseBody = enableResponse.Content.ReadAsStringAsync() |> Async.AwaitTask

                            return Ok subdomain
                        else
                            return Error "Could not get account subdomain"
                    with ex ->
                        return Error $"Error enabling workers.dev: {ex.Message}"
                }

            match subdomainResult with
            | Ok subdomain ->
                let workerUrl = $"https://{workerName}.{subdomain}.workers.dev"
                AnsiConsole.WriteLine()
                AnsiConsole.MarkupLine($"[cyan]Worker URL:[/] {workerUrl}")
                AnsiConsole.MarkupLine($"[dim]Click the URL above to open in browser[/]")
                return Ok ()
            | Error err ->
                // Still show a URL even if we couldn't enable it
                AnsiConsole.WriteLine()
                AnsiConsole.MarkupLine($"[yellow]⚠[/] Could not enable workers.dev subdomain: {err}")
                let workerUrl = $"https://{workerName}.{config.AccountId}.workers.dev"
                AnsiConsole.MarkupLine($"[dim]Worker URL (may not be active):[/] {workerUrl}")
                return Ok ()
        | Error err ->
            return Error err
    }
