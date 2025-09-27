#!/usr/bin/env -S dotnet fsi

(*
    CloudflareFS Binding Generation Script

    This script automates the generation of F# bindings from Cloudflare TypeScript definitions
    using the Glutinum CLI tool.
*)

open System
open System.IO
open System.Diagnostics

let projectRoot = Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, ".."))
let nodeModules = Path.Combine(projectRoot, "node_modules")
let srcRuntime = Path.Combine(projectRoot, "src", "Runtime")

// Configuration for each package to generate
type BindingConfig = {
    SourcePackage: string
    SourceFile: string
    OutputDirectory: string
    OutputFile: string
}

let bindings = [
    // Core Worker types - this is the main one
    {
        SourcePackage = "@cloudflare/workers-types"
        SourceFile = "index.d.ts"
        OutputDirectory = Path.Combine(srcRuntime, "CloudFlare.Worker.Context")
        OutputFile = "Generated.fs"
    }

    // AI bindings
    {
        SourcePackage = "@cloudflare/ai"
        SourceFile = Path.Combine("dist", "index.d.ts")
        OutputDirectory = Path.Combine(srcRuntime, "CloudFlare.AI")
        OutputFile = "Generated.fs"
    }
]

// Run Glutinum for a specific binding configuration
let generateBinding (config: BindingConfig) =
    let sourcePath = Path.Combine(nodeModules, config.SourcePackage, config.SourceFile)
    let outputPath = Path.Combine(config.OutputDirectory, config.OutputFile)

    if not (File.Exists(sourcePath)) then
        printfn "⚠️  Source file not found: %s" sourcePath
        printfn "   Make sure to run 'npm install' first"
        false
    else
        printfn "📦 Generating bindings for %s..." config.SourcePackage
        printfn "   Source: %s" sourcePath
        printfn "   Output: %s" outputPath

        // Ensure output directory exists
        Directory.CreateDirectory(config.OutputDirectory) |> ignore

        // Build the Glutinum command
        let glutinumCmd = sprintf "npx @glutinum/cli \"%s\" --out-file \"%s\"" sourcePath outputPath

        // Execute Glutinum
        let psi = ProcessStartInfo("cmd.exe", sprintf "/c %s" glutinumCmd)
        psi.WorkingDirectory <- projectRoot
        psi.RedirectStandardOutput <- true
        psi.RedirectStandardError <- true
        psi.UseShellExecute <- false

        use proc = Process.Start(psi)
        let output = proc.StandardOutput.ReadToEnd()
        let error = proc.StandardError.ReadToEnd()
        proc.WaitForExit()

        if proc.ExitCode = 0 then
            printfn "✅ Successfully generated %s" config.OutputFile
            if not (String.IsNullOrWhiteSpace(output)) then
                printfn "   Output: %s" output
            true
        else
            printfn "❌ Failed to generate bindings for %s" config.SourcePackage
            if not (String.IsNullOrWhiteSpace(error)) then
                printfn "   Error: %s" error
            false

// Main execution
printfn "========================================="
printfn "CloudflareFS Binding Generation"
printfn "========================================="
printfn ""

let results = bindings |> List.map generateBinding

let successful = results |> List.filter id |> List.length
let failed = results |> List.filter (not) |> List.length

printfn ""
printfn "========================================="
printfn "Generation Complete"
printfn "✅ Successful: %d" successful
if failed > 0 then
    printfn "❌ Failed: %d" failed
printfn "========================================="

// Return exit code
if failed > 0 then 1 else 0