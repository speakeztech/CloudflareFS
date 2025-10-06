#r "nuget: Newtonsoft.Json, 13.0.3"
#r "nuget: FSharp.Data, 6.3.0"

open System
open System.IO
open System.Text.RegularExpressions
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open FSharp.Data

// Configuration for service extraction
type ServiceConfig = {
    Name: string
    PathPatterns: string list
    OperationPrefix: string
    Description: string
}

let services = [
    { Name = "KV"
      PathPatterns = [
          "/accounts/{account_id}/storage/kv/namespaces"
          "/accounts/{account_id}/storage/kv/namespaces/{namespace_id}"
          "/accounts/{account_id}/storage/kv/namespaces/{namespace_id}/bulk"
          "/accounts/{account_id}/storage/kv/namespaces/{namespace_id}/keys"
          "/accounts/{account_id}/storage/kv/namespaces/{namespace_id}/metadata/{key_name}"
          "/accounts/{account_id}/storage/kv/namespaces/{namespace_id}/values/{key_name}"
      ]
      OperationPrefix = "kv"
      Description = "KV Storage Management API" }

    { Name = "R2"
      PathPatterns = [
          "/accounts/{account_id}/r2/buckets"
          "/accounts/{account_id}/r2/buckets/{bucket_name}"
          "/accounts/{account_id}/r2/buckets/{bucket_name}/objects"
          "/accounts/{account_id}/r2/buckets/{bucket_name}/sippy"
          "/accounts/{account_id}/r2/buckets/{bucket_name}/usage"
      ]
      OperationPrefix = "r2"
      Description = "R2 Object Storage Management API" }

    { Name = "D1"
      PathPatterns = [
          "/accounts/{account_id}/d1/database"
          "/accounts/{account_id}/d1/database/{database_id}"
          "/accounts/{account_id}/d1/database/{database_id}/backup"
          "/accounts/{account_id}/d1/database/{database_id}/query"
          "/accounts/{account_id}/d1/database/{database_id}/raw"
      ]
      OperationPrefix = "d1"
      Description = "D1 Database Management API" }

    { Name = "Workers"
      PathPatterns = [
          "/accounts/{account_id}/workers/scripts"
          "/accounts/{account_id}/workers/scripts/{script_name}"
          "/accounts/{account_id}/workers/scripts/{script_name}/content"
          "/accounts/{account_id}/workers/scripts/{script_name}/schedules"
          "/accounts/{account_id}/workers/scripts/{script_name}/secrets"
          "/accounts/{account_id}/workers/scripts/{script_name}/tails"
          "/accounts/{account_id}/workers/scripts/{script_name}/usage-model"
          "/accounts/{account_id}/workers/scripts/{script_name}/versions"
          "/accounts/{account_id}/workers/scripts/{script_name}/versions/{version_id}"
          "/accounts/{account_id}/workers/services"
          "/accounts/{account_id}/workers/subdomain"
      ]
      OperationPrefix = "workers"
      Description = "Workers Script Management API" }

    { Name = "Analytics"
      PathPatterns = [
          "/accounts/{account_id}/analytics"
          "/zones/{zone_id}/analytics/dashboard"
          "/zones/{zone_id}/analytics/latency"
          "/accounts/{account_id}/workers/scripts/{script_name}/analytics"
      ]
      OperationPrefix = "analytics"
      Description = "Analytics API" }

    { Name = "Logs"
      PathPatterns = [
          "/zones/{zone_id}/logs/control/retention/flag"
          "/zones/{zone_id}/logs/rayids/{ray_id}"
          "/zones/{zone_id}/logs/received"
          "/zones/{zone_id}/logs/received/fields"
          "/accounts/{account_id}/logs/control/cmb/config"
          "/accounts/{account_id}/logpull/datasets/{dataset}/fields"
          "/accounts/{account_id}/logpull/datasets/{dataset}/jobs"
          "/accounts/{account_id}/logpush/datasets/{dataset}/fields"
          "/accounts/{account_id}/logpush/datasets/{dataset}/jobs"
      ]
      OperationPrefix = "logs"
      Description = "Logs API" }

    { Name = "Queues"
      PathPatterns = [
          "/accounts/{account_id}/queues"
          "/accounts/{account_id}/queues/{queue_id}"
          "/accounts/{account_id}/queues/{queue_id}/consumers"
          "/accounts/{account_id}/queues/{queue_id}/consumers/{consumer_id}"
          "/accounts/{account_id}/queues/{queue_id}/messages"
          "/accounts/{account_id}/queues/{queue_id}/messages/ack"
          "/accounts/{account_id}/queues/{queue_id}/messages/batch"
          "/accounts/{account_id}/queues/{queue_id}/messages/pull"
          "/accounts/{account_id}/queues/{queue_id}/purge"
      ]
      OperationPrefix = "queues"
      Description = "Message Queue Management API" }

    { Name = "Vectorize"
      PathPatterns = [
          "/accounts/{account_id}/vectorize/v2/indexes"
          "/accounts/{account_id}/vectorize/v2/indexes/{index_name}"
          "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/delete_by_ids"
          "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/get_by_ids"
          "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/info"
          "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/insert"
          "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/list"
          "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/metadata_index/create"
          "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/metadata_index/delete"
          "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/metadata_index/list"
          "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/query"
          "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/upsert"
      ]
      OperationPrefix = "vectorize"
      Description = "Vector Database Management API (V2)" }

    { Name = "Hyperdrive"
      PathPatterns = [
          "/accounts/{account_id}/hyperdrive/configs"
          "/accounts/{account_id}/hyperdrive/configs/{hyperdrive_id}"
      ]
      OperationPrefix = "hyperdrive"
      Description = "Database Connection Pooling API" }

    { Name = "DurableObjects"
      PathPatterns = [
          "/accounts/{account_id}/workers/durable_objects/namespaces"
          "/accounts/{account_id}/workers/durable_objects/namespaces/{id}/objects"
      ]
      OperationPrefix = "durable-objects"
      Description = "Durable Objects Management API" }
]

// Load the OpenAPI specification
let loadOpenApiSpec (filePath: string) =
    printfn "Loading OpenAPI specification from %s..." filePath
    let fileInfo = FileInfo(filePath)
    let sizeMB = float fileInfo.Length / (1024.0 * 1024.0)
    printfn "File size: %.2f MB" sizeMB

    let json = File.ReadAllText(filePath)
    JObject.Parse(json)

// Extract paths for a service
let extractServicePaths (spec: JObject) (service: ServiceConfig) =
    let paths = spec.["paths"] :?> JObject
    let mutable extractedPaths = JObject()
    let mutable pathCount = 0

    for pathProp in paths.Properties() do
        let path = pathProp.Name
        let isMatch =
            service.PathPatterns
            |> List.exists (fun pattern -> path = pattern)

        if isMatch then
            extractedPaths.Add(pathProp.Name, pathProp.Value)
            pathCount <- pathCount + 1
            printfn "  Found: %s" path

    printfn "  Extracted %d paths for %s" pathCount service.Name
    extractedPaths, pathCount

// Collect schema references from a JSON token
let rec collectSchemaRefs (token: JToken) (refs: Set<string> ref) =
    match token.Type with
    | JTokenType.Object ->
        let obj = token :?> JObject
        for prop in obj.Properties() do
            if prop.Name = "$ref" then
                let refValue = prop.Value.ToString()
                if refValue.StartsWith("#/components/schemas/") then
                    let schemaName = refValue.Replace("#/components/schemas/", "")
                    refs := Set.add schemaName !refs
            else
                collectSchemaRefs prop.Value refs
    | JTokenType.Array ->
        let arr = token :?> JArray
        for item in arr do
            collectSchemaRefs item refs
    | _ -> ()

// Extract schemas and their dependencies
let extractSchemas (spec: JObject) (paths: JObject) (service: ServiceConfig) =
    let schemas = spec.["components"].["schemas"] :?> JObject
    let mutable refs = ref Set.empty

    // Collect references from paths
    collectSchemaRefs paths refs

    // Add schemas matching service prefix
    for schemaProp in schemas.Properties() do
        if schemaProp.Name.StartsWith(service.OperationPrefix + "_") ||
           schemaProp.Name.StartsWith(service.OperationPrefix + "-") then
            refs := Set.add schemaProp.Name !refs

    // Add common schemas
    let commonSchemas = [
        "api-response-common"
        "api-response-single"
        "api-response-collection"
        "messages"
        "result_info"
        "schemas-errors"
    ]

    for common in commonSchemas do
        if schemas.Property(common) <> null then
            refs := Set.add common !refs

    // Recursively add dependencies
    let rec addDependencies (schemaName: string) =
        if schemas.Property(schemaName) <> null then
            let schema = schemas.[schemaName]
            let mutable schemaRefs = ref Set.empty
            collectSchemaRefs schema schemaRefs
            for depRef in !schemaRefs do
                if not (Set.contains depRef !refs) then
                    refs := Set.add depRef !refs
                    addDependencies depRef

    let initialRefs = !refs
    for schemaName in initialRefs do
        addDependencies schemaName

    // Create filtered schemas object
    let mutable filteredSchemas = JObject()
    for schemaName in !refs do
        if schemas.Property(schemaName) <> null then
            filteredSchemas.Add(schemaName, schemas.[schemaName])

    printfn "  Extracted %d schemas" (Set.count !refs)
    filteredSchemas

// Create filtered OpenAPI spec for a service
let createServiceSpec (spec: JObject) (service: ServiceConfig) =
    printfn "\nProcessing %s..." service.Name

    let paths, pathCount = extractServicePaths spec service

    if pathCount = 0 then
        printfn "  WARNING: No paths found for %s" service.Name
        None
    else
        let schemas = extractSchemas spec paths service

        let filtered = JObject()
        filtered.Add("openapi", spec.["openapi"])

        let info = JObject()
        info.Add("title", JValue($"Cloudflare {service.Name} API"))
        info.Add("description", JValue(service.Description))
        info.Add("version", spec.["info"].["version"])
        filtered.Add("info", info)

        filtered.Add("servers", spec.["servers"])
        filtered.Add("paths", paths)

        let components = JObject()
        components.Add("schemas", schemas)
        components.Add("parameters", JObject())
        components.Add("responses", JObject())
        components.Add("examples", JObject())
        components.Add("requestBodies", JObject())
        components.Add("headers", JObject())

        if spec.["components"].["securitySchemes"] <> null then
            components.Add("securitySchemes", spec.["components"].["securitySchemes"])

        filtered.Add("components", components)

        Some filtered

// Main execution
let inputFile = @"D:\repos\Cloudflare\api-schemas\openapi.json"
let outputDir = @".\temp"

// Ensure output directory exists
Directory.CreateDirectory(outputDir) |> ignore

try
    printfn "CloudflareFS OpenAPI Service Extractor"
    printfn "======================================="

    let spec = loadOpenApiSpec inputFile

    for service in services do
        match createServiceSpec spec service with
        | Some filtered ->
            let outputFile = Path.Combine(outputDir, $"{service.Name}-openapi.json")
            let json = filtered.ToString(Formatting.Indented)
            File.WriteAllText(outputFile, json)

            let fileInfo = FileInfo(outputFile)
            let sizeKB = float fileInfo.Length / 1024.0
            printfn "  Saved to %s (%.2f KB)" outputFile sizeKB

        | None ->
            printfn "  Skipped %s (no paths found)" service.Name

    printfn "\n✅ Extraction complete!"
    printfn "   Output directory: %s" outputDir
with
| ex ->
    printfn "❌ Error: %s" ex.Message
    printfn "%s" ex.StackTrace