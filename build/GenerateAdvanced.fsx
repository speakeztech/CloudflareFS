#!/usr/bin/env -S dotnet fsi

#r "nuget: FSharp.Data"

open System
open System.IO
open System.Diagnostics
open System.Text.RegularExpressions
open FSharp.Data

let projectRoot = Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, ".."))
let nodeModules = Path.Combine(projectRoot, "node_modules")
let srcRuntime = Path.Combine(projectRoot, "src", "Runtime")
let tempDir = Path.Combine(projectRoot, "temp")

// Ensure temp directory exists
Directory.CreateDirectory(tempDir) |> ignore

type BindingPackage = {
    NpmPackage: string
    InputFile: string
    OutputModule: string
    OutputNamespace: string
    RequiresPreprocessing: bool
}

let packages = [
    { NpmPackage = "@cloudflare/workers-types"
      InputFile = "index.d.ts"
      OutputModule = "CloudFlare.Worker.Context"
      OutputNamespace = "CloudFlare.Worker"
      RequiresPreprocessing = true }

    { NpmPackage = "@cloudflare/ai"
      InputFile = Path.Combine("dist", "index.d.ts")
      OutputModule = "CloudFlare.AI"
      OutputNamespace = "CloudFlare.AI"
      RequiresPreprocessing = false }
]

/// Run a command and return the output
let runCommand (command: string) (args: string) =
    let psi = ProcessStartInfo(command, args)
    psi.WorkingDirectory <- projectRoot
    psi.RedirectStandardOutput <- true
    psi.RedirectStandardError <- true
    psi.UseShellExecute <- false

    use proc = Process.Start(psi)
    let output = proc.StandardOutput.ReadToEnd()
    let error = proc.StandardError.ReadToEnd()
    proc.WaitForExit()

    if proc.ExitCode <> 0 then
        printfn "Error running %s %s" command args
        printfn "Error: %s" error
        None
    else
        Some output

/// Preprocess TypeScript file if needed
let preprocessIfNeeded (package: BindingPackage) =
    let sourcePath = Path.Combine(nodeModules, package.NpmPackage, package.InputFile)

    if package.RequiresPreprocessing then
        let outputPath = Path.Combine(tempDir, sprintf "%s.preprocessed.d.ts" package.OutputModule)
        printfn "🔧 Preprocessing %s..." package.NpmPackage

        let result = runCommand "node" (sprintf "build/preprocessor.js \"%s\" \"%s\"" sourcePath outputPath)
        match result with
        | Some _ ->
            printfn "   ✅ Preprocessing complete"
            outputPath
        | None ->
            printfn "   ⚠️ Preprocessing failed, using original"
            sourcePath
    else
        sourcePath

/// Post-process generated F# bindings
let postProcessBindings (filePath: string) (package: BindingPackage) =
    if not (File.Exists(filePath)) then
        printfn "⚠️ File not found for post-processing: %s" filePath
        false
    else
        printfn "🔧 Post-processing %s..." filePath
        let content = File.ReadAllText(filePath)

        let processed =
            content
            // Fix module declaration
            |> fun s -> Regex.Replace(s, @"module rec Glutinum", sprintf "module rec %s" package.OutputNamespace)

            // Add Fable attributes to types
            |> fun s -> Regex.Replace(s, @"type (\w+) =", "[<AllowNullLiteral>]\ntype $1 =")

            // Convert Promise patterns to JS.Promise
            |> fun s -> Regex.Replace(s, @"Promise<([^>]+)>", "JS.Promise<$1>")

            // Add Import attributes for non-generated types
            |> fun s ->
                if package.NpmPackage.Contains("workers-types") then
                    Regex.Replace(s, @"type (Request|Response|Headers|URL|FormData|WebSocket)\b",
                                  "[<Import(\"$1\", \"@cloudflare/workers-types\")>]\ntype $1")
                else
                    s

            // Fix array types
            |> fun s -> Regex.Replace(s, @"ResizeArray<([^>]+)>", "$1[]")

            // Add namespace opening
            |> fun s ->
                let namespaceOpening = sprintf "namespace %s\n\nopen Fable.Core\nopen Fable.Core.JsInterop\nopen System\nopen Browser.Types\n\n" package.OutputNamespace
                namespaceOpening + s.Replace("open System", "")

        File.WriteAllText(filePath, processed)
        printfn "   ✅ Post-processing complete"
        true

/// Generate bindings for a package
let generateBinding (package: BindingPackage) =
    printfn ""
    printfn "📦 Processing %s" package.NpmPackage
    printfn "=" |> String.replicate 50 |> printfn "%s"

    // Step 1: Preprocess if needed
    let inputPath = preprocessIfNeeded package

    // Step 2: Prepare output path
    let outputDir = Path.Combine(srcRuntime, package.OutputModule)
    Directory.CreateDirectory(outputDir) |> ignore
    let outputPath = Path.Combine(outputDir, "Generated.fs")

    // Step 3: Run Glutinum
    printfn "🔨 Running Glutinum..."
    let glutinumArgs = sprintf "@glutinum/cli \"%s\" --out-file \"%s\"" inputPath outputPath

    let result = runCommand "npx" glutinumArgs

    match result with
    | Some output ->
        if File.Exists(outputPath) then
            printfn "   ✅ Bindings generated"
            // Step 4: Post-process
            postProcessBindings outputPath package
        else
            printfn "   ❌ Glutinum failed to generate output"
            false
    | None ->
        printfn "   ❌ Glutinum execution failed"
        false

/// Create extension modules with F# idioms
let createExtensionModule (package: BindingPackage) =
    let extensionPath = Path.Combine(srcRuntime, package.OutputModule, "Extensions.fs")

    let extensionContent =
        sprintf "namespace %s\n\n" package.OutputNamespace +
        "[<AutoOpen>]\n" +
        "module Extensions =\n" +
        "    open Fable.Core\n" +
        "    open Fable.Core.JsInterop\n" +
        "    open System\n\n" +
        "    /// Convert JS Promise to F# Async\n" +
        "    type JS.Promise<'T> with\n" +
        "        member this.ToAsync() : Async<'T> =\n" +
        "            this |> Async.AwaitPromise\n\n" +
        "    /// Computation expression builder for promises\n" +
        "    type PromiseBuilder() =\n" +
        "        member _.Bind(x: JS.Promise<'a>, f: 'a -> JS.Promise<'b>) =\n" +
        "            x?``then``(f)\n" +
        "        member _.Return(x: 'a) =\n" +
        "            JS.Promise.resolve(x)\n" +
        "        member _.ReturnFrom(x: JS.Promise<'a>) = x\n" +
        "        member _.Zero() = JS.Promise.resolve()\n\n" +
        "    let promise = PromiseBuilder()\n"

    File.WriteAllText(extensionPath, extensionContent)
    printfn "   ✅ Extension module created"

// Main execution
printfn "========================================="
printfn "CloudflareFS Advanced Binding Generation"
printfn "========================================="

let results =
    packages
    |> List.map (fun pkg ->
        let success = generateBinding pkg
        if success then createExtensionModule pkg
        success
    )

let successful = results |> List.filter id |> List.length
let failed = results |> List.filter (not) |> List.length

// Clean up temp directory
if Directory.Exists(tempDir) then
    Directory.Delete(tempDir, true)

printfn ""
printfn "========================================="
printfn "Generation Complete"
printfn "✅ Successful: %d" successful
if failed > 0 then
    printfn "❌ Failed: %d" failed
printfn "========================================="

exit (if failed > 0 then 1 else 0)