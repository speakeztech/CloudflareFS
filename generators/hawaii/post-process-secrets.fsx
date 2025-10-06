#!/usr/bin/env dotnet fsi

open System
open System.IO
open System.Text.RegularExpressions

let clientFile = "Generated-Workers/Client.fs"

if not (File.Exists clientFile) then
    printfn "❌ Error: %s not found" clientFile
    exit 1

let content = File.ReadAllText(clientFile)

// Step 1: Add body parameter to method signature
let signaturePattern = @"member this\.WorkerPutScriptSecret\(accountId: string, scriptName: string, \?cancellationToken: CancellationToken\)"
let signatureReplacement = "member this.WorkerPutScriptSecret(accountId: string, scriptName: string, body: Types.workersbindingkindsecrettext, ?cancellationToken: CancellationToken)"

let mutable updated =
    if Regex.IsMatch(content, signaturePattern) then
        Regex.Replace(content, signaturePattern, signatureReplacement)
    else
        content

// Step 2: Add RequestPart.jsonContent body to requestParts list
// Match the specific requestParts list for this method
let requestPartsPattern = @"(member this\.WorkerPutScriptSecret\([^)]+\) =\s+async \{\s+let requestParts =\s+\[ RequestPart\.path \(""account_id"", accountId\)\s+RequestPart\.path \(""script_name"", scriptName\) \])"

let requestPartsReplacement = @"member this.WorkerPutScriptSecret(accountId: string, scriptName: string, body: Types.workersbindingkindsecrettext, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path (""account_id"", accountId)
                  RequestPart.path (""script_name"", scriptName)
                  RequestPart.jsonContent body ]"

updated <- Regex.Replace(updated, requestPartsPattern, requestPartsReplacement, RegexOptions.Singleline)

if updated <> content then
    File.WriteAllText(clientFile, updated)
    printfn "✓ Successfully added body parameter to WorkerPutScriptSecret"
else
    printfn "⚠ No changes made - method might already be fixed"
