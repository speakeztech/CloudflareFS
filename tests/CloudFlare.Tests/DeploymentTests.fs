module CloudFlare.Tests.DeploymentTests

open Expecto
open System
open System.IO
open System.Security.Cryptography

/// IO operations for deployment - following Jordan Marr's pattern
type DeploymentIO = {
    readFile: string -> string
    writeFile: string -> string -> unit
    fileExists: string -> bool
    directoryExists: string -> bool
    getFiles: string -> string -> string[]
    runProcess: string -> string -> int * string * string
}

/// Create test IO with in-memory file system
let createTestIO (files: Map<string, string>) =
    let mutable fileSystem = files

    {
        readFile = fun path ->
            match fileSystem.TryFind path with
            | Some content -> content
            | None -> failwith $"File not found: {path}"

        writeFile = fun path content ->
            fileSystem <- fileSystem.Add(path, content)

        fileExists = fun path ->
            fileSystem.ContainsKey path

        directoryExists = fun path ->
            // Simple simulation: directory exists if any file starts with the path
            fileSystem |> Map.exists (fun k _ -> k.StartsWith(path))

        getFiles = fun dir pattern ->
            fileSystem
            |> Map.keys
            |> Seq.filter (fun k -> k.StartsWith(dir))
            |> Seq.toArray

        runProcess = fun cmd args ->
            // Simulate successful process execution
            (0, "Success", "")
    }

/// Deployment state tracking
module DeploymentState =
    let computeHash (content: string) =
        use sha = SHA256.Create()
        content
        |> Text.Encoding.UTF8.GetBytes
        |> sha.ComputeHash
        |> Convert.ToHexString

    let shouldDeploy (io: DeploymentIO) (stateFile: string) (sourceFiles: string list) (force: bool) =
        if force then
            true
        elif not (io.fileExists stateFile) then
            true
        else
            let currentHash =
                sourceFiles
                |> List.map io.readFile
                |> String.concat ""
                |> computeHash

            let lastState = io.readFile stateFile
            let lastHash = if lastState.Contains("|") then lastState.Split('|').[0] else ""

            lastHash <> currentHash

    let saveState (io: DeploymentIO) (stateFile: string) (sourceFiles: string list) =
        let hash =
            sourceFiles
            |> List.map io.readFile
            |> String.concat ""
            |> computeHash

        let timestamp = DateTime.UtcNow.ToString("O")
        io.writeFile stateFile $"{hash}|{timestamp}"

/// Worker metadata construction
module WorkerMetadata =
    type WorkerConfig = {
        mainModule: string
        compatibilityDate: string
        compatibilityFlags: string list
        bindings: Map<string, obj>
    }

    let create (mainModule: string) (bindings: Map<string, obj>) : WorkerConfig =
        {
            mainModule = mainModule
            compatibilityDate = "2024-01-01"
            compatibilityFlags = ["nodejs_compat"]
            bindings = bindings
        }

    let toJson (config: WorkerConfig) =
        let bindingsJson =
            if config.bindings.IsEmpty then "[]"
            else
                config.bindings
                |> Map.toList
                |> List.map (fun (k, v) -> $"""{{\"name\":\"{k}\",\"value\":\"{v}\"}}""")
                |> String.concat ","
                |> sprintf "[%s]"

        $"""{{
            "main_module": "{config.mainModule}",
            "compatibility_date": "{config.compatibilityDate}",
            "compatibility_flags": [{String.Join(",", config.compatibilityFlags |> List.map (sprintf "\"%s\""))}],
            "bindings": {bindingsJson}
        }}"""

let tests =
    testList "Deployment Logic Tests" [

        testList "Deployment State Management" [
            testCase "Initial deployment when no state exists" <| fun _ ->
                let io = createTestIO (Map [
                    "worker.fs", "let handler = ..."
                ])

                let shouldDeploy = DeploymentState.shouldDeploy io ".deploy-state" ["worker.fs"] false

                Expect.isTrue shouldDeploy "Should deploy when no state file exists"

            testCase "Skip deployment when source unchanged" <| fun _ ->
                let sourceContent = "let handler = ..."
                let hash = DeploymentState.computeHash sourceContent

                let io = createTestIO (Map [
                    "worker.fs", sourceContent
                    ".deploy-state", $"{hash}|2024-01-01T00:00:00Z"
                ])

                let shouldDeploy = DeploymentState.shouldDeploy io ".deploy-state" ["worker.fs"] false

                Expect.isFalse shouldDeploy "Should skip when source unchanged"

            testCase "Deploy when source changed" <| fun _ ->
                let oldHash = DeploymentState.computeHash "old content"

                let io = createTestIO (Map [
                    "worker.fs", "new content"
                    ".deploy-state", $"{oldHash}|2024-01-01T00:00:00Z"
                ])

                let shouldDeploy = DeploymentState.shouldDeploy io ".deploy-state" ["worker.fs"] false

                Expect.isTrue shouldDeploy "Should deploy when source changed"

            testCase "Force deployment ignores state" <| fun _ ->
                let hash = DeploymentState.computeHash "content"

                let io = createTestIO (Map [
                    "worker.fs", "content"
                    ".deploy-state", $"{hash}|2024-01-01T00:00:00Z"
                ])

                let shouldDeploy = DeploymentState.shouldDeploy io ".deploy-state" ["worker.fs"] true

                Expect.isTrue shouldDeploy "Force flag should override state check"

            testCase "Save deployment state with timestamp" <| fun _ ->
                let mutable savedContent = ""
                let io = {
                    createTestIO Map.empty with
                        readFile = fun _ -> "content"
                        writeFile = fun _ content -> savedContent <- content
                }

                DeploymentState.saveState io ".deploy-state" ["worker.fs"]

                Expect.stringContains savedContent "|" "State should include separator"
                let parts = savedContent.Split('|')
                Expect.equal parts.Length 2 "State should have hash and timestamp"
                Expect.isTrue (parts.[0].Length > 0) "Should have hash"
                Expect.isTrue (parts.[1].Length > 0) "Should have timestamp"

            testCase "Hash computation is deterministic" <| fun _ ->
                let content = "test content"
                let hash1 = DeploymentState.computeHash content
                let hash2 = DeploymentState.computeHash content

                Expect.equal hash1 hash2 "Same content should produce same hash"

            testCase "Different content produces different hash" <| fun _ ->
                let hash1 = DeploymentState.computeHash "content 1"
                let hash2 = DeploymentState.computeHash "content 2"

                Expect.notEqual hash1 hash2 "Different content should produce different hash"
        ]

        testList "Worker Metadata Construction" [
            testCase "Create basic metadata" <| fun _ ->
                let metadata = WorkerMetadata.create "worker.js" Map.empty

                Expect.equal metadata.mainModule "worker.js" "Should set main module"
                Expect.equal metadata.compatibilityFlags ["nodejs_compat"] "Should have node compat"
                Expect.isTrue (metadata.bindings.IsEmpty) "Should have empty bindings"

            testCase "Create metadata with bindings" <| fun _ ->
                let bindings = Map [
                    ("DB", box "my-d1-database")
                    ("BUCKET", box "my-r2-bucket")
                ]

                let metadata = WorkerMetadata.create "worker.js" bindings

                Expect.equal metadata.bindings.Count 2 "Should have 2 bindings"
                Expect.isTrue (metadata.bindings.ContainsKey "DB") "Should have DB binding"
                Expect.isTrue (metadata.bindings.ContainsKey "BUCKET") "Should have BUCKET binding"

            testCase "Metadata JSON serialization" <| fun _ ->
                let metadata = WorkerMetadata.create "main.js" Map.empty
                let json = WorkerMetadata.toJson metadata

                Expect.stringContains json "main_module" "Should contain main_module field"
                Expect.stringContains json "main.js" "Should contain module name"
                Expect.stringContains json "compatibility_date" "Should contain compatibility_date"
                Expect.stringContains json "nodejs_compat" "Should contain nodejs_compat flag"

            testCase "Metadata JSON with bindings" <| fun _ ->
                let bindings = Map [("KEY", box "value")]
                let metadata = WorkerMetadata.create "worker.js" bindings
                let json = WorkerMetadata.toJson metadata

                Expect.stringContains json "bindings" "Should contain bindings field"
                Expect.stringContains json "KEY" "Should contain binding name"
                Expect.stringContains json "value" "Should contain binding value"
        ]

        testList "Fable Compilation Simulation" [
            testCase "Successful compilation returns exit code 0" <| fun _ ->
                let io = createTestIO Map.empty
                let (exitCode, stdout, stderr) = io.runProcess "fable" ". --outDir dist"

                Expect.equal exitCode 0 "Successful compilation should return 0"
                Expect.isNotEmpty stdout "Should have stdout"

            testCase "Detect compilation errors in output" <| fun _ ->
                let fableOutput = """Fable 4.25.0: F# to JavaScript compiler
Started Fable compilation...
.\Worker.fs(55,15): (55,24) error FSHARP: The type 'string' does not support the operator
Fable compilation finished in 3619ms"""

                let hasErrors = fableOutput.Contains("error FSHARP:")

                Expect.isTrue hasErrors "Should detect compilation errors"

            testCase "Parse Fable output for file and line information" <| fun _ ->
                let errorLine = @".\Worker.fs(55,15): (55,24) error FSHARP: The type 'string' does not support the operator"

                let containsFile = errorLine.Contains("Worker.fs")
                let containsLineNumber = errorLine.Contains("(55,")
                let containsError = errorLine.Contains("error FSHARP:")

                Expect.isTrue containsFile "Should contain filename"
                Expect.isTrue containsLineNumber "Should contain line number"
                Expect.isTrue containsError "Should contain error marker"
        ]

        testList "Project File Analysis" [
            testCase "Extract project name from fsproj" <| fun _ ->
                let fsprojPath = "MyWorker.fsproj"
                let projectName = Path.GetFileNameWithoutExtension(fsprojPath)

                Expect.equal projectName "MyWorker" "Should extract project name"

            testCase "Parse compile includes from fsproj content" <| fun _ ->
                let fsprojContent = """
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <Compile Include="Types.fs" />
    <Compile Include="Helpers.fs" />
    <Compile Include="Worker.fs" />
  </ItemGroup>
</Project>
                """

                let compileIncludes =
                    System.Text.RegularExpressions.Regex.Matches(fsprojContent, "<Compile Include=\"([^\"]+)\" />")
                    |> Seq.cast<System.Text.RegularExpressions.Match>
                    |> Seq.map (fun m -> m.Groups.[1].Value)
                    |> Seq.toList

                Expect.equal compileIncludes.Length 3 "Should find 3 compile includes"
                Expect.equal (List.last compileIncludes) "Worker.fs" "Last file should be entry point"

            testCase "Convert project name to worker name" <| fun _ ->
                let projectName = "MyWorkerProject"
                let workerName = projectName.ToLowerInvariant()

                Expect.equal workerName "myworkerproject" "Should convert to lowercase"
        ]

        testList "File Bundling Logic" [
            testCase "Collect all JS files for upload" <| fun _ ->
                let io = createTestIO (Map [
                    "dist/worker.js", "export default { fetch() {} }"
                    "dist/utils.js", "export function helper() {}"
                    "dist/fable_modules/module.js", "// fable module"
                ])

                let jsFiles = io.getFiles "dist" "*.js"

                Expect.isGreaterThanOrEqual jsFiles.Length 1 "Should find JS files"

            testCase "Compute relative paths for multipart upload" <| fun _ ->
                let distPath = "D:/project/dist"
                let fullPath = "D:/project/dist/fable_modules/util.js"

                let relativePath =
                    Path.GetRelativePath(distPath, fullPath).Replace("\\", "/")

                Expect.equal relativePath "fable_modules/util.js" "Should compute correct relative path"

            testCase "Main module must be included" <| fun _ ->
                let files = [
                    "dist/Worker.js"
                    "dist/helpers.js"
                ]

                let hasMainModule = files |> List.exists (fun f -> f.Contains("Worker.js"))

                Expect.isTrue hasMainModule "Worker.js must be in bundle"
        ]

        testList "Deployment Idempotency" [
            testCase "Multiple deployments of same code are idempotent" <| fun _ ->
                let mutable deployCount = 0
                let content = "unchanged content"

                let io = createTestIO (Map ["worker.fs", content])

                // First deployment
                if DeploymentState.shouldDeploy io ".deploy-state" ["worker.fs"] false then
                    deployCount <- deployCount + 1
                    DeploymentState.saveState io ".deploy-state" ["worker.fs"]

                // Second deployment attempt
                if DeploymentState.shouldDeploy io ".deploy-state" ["worker.fs"] false then
                    deployCount <- deployCount + 1

                Expect.equal deployCount 1 "Should only deploy once for unchanged code"

            testCase "Deployment proceeds after source change" <| fun _ ->
                let mutable currentContent = "initial"
                let mutable io = createTestIO (Map ["worker.fs", currentContent])

                // First deployment
                DeploymentState.saveState io ".deploy-state" ["worker.fs"]

                // Change source
                currentContent <- "modified"
                io <- createTestIO (Map [
                    "worker.fs", currentContent
                    ".deploy-state", io.readFile ".deploy-state"
                ])

                let shouldRedeploy = DeploymentState.shouldDeploy io ".deploy-state" ["worker.fs"] false

                Expect.isTrue shouldRedeploy "Should redeploy after source change"
        ]
    ]
