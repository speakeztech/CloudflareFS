#r "nuget: Newtonsoft.Json, 13.0.3"

open Newtonsoft.Json.Linq
open System.IO

let json = File.ReadAllText(@"temp\Workers-openapi.json")
let spec = JObject.Parse(json)

let schemas = spec.["components"].["schemas"] :?> JObject

if schemas.Property("workers_secret") <> null then
    printfn "✅ workers_secret schema found"
    let schema = schemas.["workers_secret"]
    printfn "%s" (schema.ToString(Newtonsoft.Json.Formatting.Indented))
else
    printfn "❌ workers_secret schema NOT found"

// Also check the binding types
printfn "\nChecking binding types:"
if schemas.Property("workers_binding_kind_secret_text") <> null then
    printfn "✅ workers_binding_kind_secret_text found"
else
    printfn "❌ workers_binding_kind_secret_text NOT found"
