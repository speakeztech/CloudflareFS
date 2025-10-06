#!/usr/bin/env dotnet fsi

// Post-processor to fix JObject multipart form data
// This bridges the gap until Hawaii supports JObject in multipart requests

open System
open System.IO
open System.Text.RegularExpressions

let fixJObjectMultipart (clientContent: string) =
    // Find: RequestPart.multipartFormData ("metadata", metadata)
    // Where metadata is JObject
    // Replace with: RequestPart.multipartFormData ("metadata", metadata.ToString(Newtonsoft.Json.Formatting.None))

    let pattern = @"RequestPart\.multipartFormData\s*\(\s*""metadata"",\s*metadata\s*\)"
    let replacement = "RequestPart.multipartFormData (\"metadata\", metadata.ToString(Newtonsoft.Json.Formatting.None))"

    if Regex.IsMatch(clientContent, pattern) then
        let updated = Regex.Replace(clientContent, pattern, replacement)
        (true, updated)
    else
        (false, clientContent)

let processWorkersClient (clientFilePath: string) =
    printfn "Processing: %s" clientFilePath

    if not (File.Exists clientFilePath) then
        printfn "Error: File not found: %s" clientFilePath
        exit 1

    let content = File.ReadAllText(clientFilePath)

    // Check if already processed
    if content.Contains("metadata.ToString(Newtonsoft.Json.Formatting.None)") then
        printfn "Already processed - JObject fix found"
        exit 0

    let (changed, updatedContent) = fixJObjectMultipart content

    if changed then
        File.WriteAllText(clientFilePath, updatedContent)
        printfn "✓ Successfully fixed JObject multipart form data"
    else
        printfn "No JObject multipart issues found"

// Main
let args = fsi.CommandLineArgs |> Array.tail

if args.Length = 0 then
    printfn "Usage: dotnet fsi post-process-jobject.fsx <path-to-Client.fs>"
    exit 1

let clientFile = args.[0]
processWorkersClient clientFile
