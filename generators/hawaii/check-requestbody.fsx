#r "nuget: Newtonsoft.Json, 13.0.3"

open Newtonsoft.Json.Linq
open System.IO

let json = File.ReadAllText(@"temp\Workers-openapi.json")
let spec = JObject.Parse(json)

let paths = spec.["paths"] :?> JObject
let secretsPath = paths.["/accounts/{account_id}/workers/scripts/{script_name}/secrets"] :?> JObject
let putOperation = secretsPath.["put"] :?> JObject
let requestBody = putOperation.["requestBody"]

if isNull requestBody then
    printfn "❌ No requestBody found in PUT /secrets endpoint"
else
    printfn "✅ requestBody found:"
    printfn "%s" (requestBody.ToString(Newtonsoft.Json.Formatting.Indented))
