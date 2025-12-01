namespace AskAI.CLI.Commands

open System
open System.Net.Http
open System.Text
open System.Text.Json
open AskAI.CLI

/// Provision Cloudflare resources for Ask AI
/// Demonstrates using Hawaii-generated Management API bindings
module Provision =

    /// D1 schema for query analytics
    let private d1Schema = """
        CREATE TABLE IF NOT EXISTS query_log (
            id TEXT PRIMARY KEY,
            query_text TEXT NOT NULL,
            timestamp TEXT NOT NULL,
            response_latency_ms INTEGER,
            source_urls TEXT,
            source_count INTEGER
        );

        CREATE INDEX IF NOT EXISTS idx_query_log_timestamp ON query_log(timestamp);

        CREATE TABLE IF NOT EXISTS query_patterns (
            pattern_hash TEXT PRIMARY KEY,
            canonical_query TEXT NOT NULL,
            frequency INTEGER DEFAULT 1,
            last_seen TEXT NOT NULL,
            avg_latency_ms REAL
        );
    """

    /// Create R2 bucket via Management API
    let private createR2Bucket
        (httpClient: HttpClient)
        (accountId: string)
        (bucketName: string)
        : Async<Result<unit, string>> =
        async {
            let url = $"https://api.cloudflare.com/client/v4/accounts/{accountId}/r2/buckets"
            let payload = JsonSerializer.Serialize({| name = bucketName |})
            use content = new StringContent(payload, Encoding.UTF8, "application/json")

            let! response = httpClient.PostAsync(url, content) |> Async.AwaitTask
            let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask

            if response.IsSuccessStatusCode then
                return Ok ()
            elif body.Contains("already exists") then
                return Ok ()  // Idempotent
            else
                return Error $"R2 bucket creation failed: {body}"
        }

    /// Create D1 database via Management API
    let private createD1Database
        (httpClient: HttpClient)
        (accountId: string)
        (dbName: string)
        : Async<Result<string, string>> =
        async {
            let url = $"https://api.cloudflare.com/client/v4/accounts/{accountId}/d1/database"
            let payload = JsonSerializer.Serialize({| name = dbName |})
            use content = new StringContent(payload, Encoding.UTF8, "application/json")

            let! response = httpClient.PostAsync(url, content) |> Async.AwaitTask
            let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask

            if response.IsSuccessStatusCode then
                use doc = JsonDocument.Parse(body)
                let uuid = doc.RootElement.GetProperty("result").GetProperty("uuid").GetString()
                return Ok uuid
            elif body.Contains("already exists") then
                // Get existing database ID
                let! listResponse = httpClient.GetAsync(url) |> Async.AwaitTask
                let! listBody = listResponse.Content.ReadAsStringAsync() |> Async.AwaitTask
                use doc = JsonDocument.Parse(listBody)
                let db =
                    doc.RootElement.GetProperty("result").EnumerateArray()
                    |> Seq.tryFind (fun d -> d.GetProperty("name").GetString() = dbName)
                match db with
                | Some d -> return Ok (d.GetProperty("uuid").GetString())
                | None -> return Error "Database exists but could not find ID"
            else
                return Error $"D1 database creation failed: {body}"
        }

    /// Execute SQL on D1 database
    let private executeD1SQL
        (httpClient: HttpClient)
        (accountId: string)
        (databaseId: string)
        (sql: string)
        : Async<Result<unit, string>> =
        async {
            let url = $"https://api.cloudflare.com/client/v4/accounts/{accountId}/d1/database/{databaseId}/query"
            let payload = JsonSerializer.Serialize({| sql = sql |})
            use content = new StringContent(payload, Encoding.UTF8, "application/json")

            let! response = httpClient.PostAsync(url, content) |> Async.AwaitTask

            if response.IsSuccessStatusCode then
                return Ok ()
            else
                let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                return Error $"SQL execution failed: {body}"
        }

    /// Execute the provision command
    let execute (config: Config.CloudflareConfig) : Async<Result<unit, string>> =
        async {
            use httpClient = new HttpClient()
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiToken}")

            let resources = Config.defaultResourceNames
            let mutable state = Config.loadState()

            printfn "Provisioning Cloudflare resources for Ask AI..."
            printfn ""

            // 1. Create R2 bucket
            printfn "  [1/2] Creating R2 bucket: %s" resources.R2BucketName
            match! createR2Bucket httpClient config.AccountId resources.R2BucketName with
            | Error e -> return Error e
            | Ok () ->
                state <- { state with R2BucketCreated = true }
                printfn "        Done"

            // 2. Create D1 database
            printfn "  [2/2] Creating D1 database: %s" resources.D1DatabaseName
            match! createD1Database httpClient config.AccountId resources.D1DatabaseName with
            | Error e -> return Error e
            | Ok dbId ->
                state <- { state with D1DatabaseId = Some dbId }
                printfn "        Database ID: %s" dbId

                // Initialize schema
                printfn "        Initializing schema..."
                match! executeD1SQL httpClient config.AccountId dbId d1Schema with
                | Error e -> return Error e
                | Ok () ->
                    printfn "        Done"

            Config.saveState state

            printfn ""
            printfn "Provisioning complete!"
            printfn ""
            printfn "  R2 Bucket:    %s" resources.R2BucketName
            printfn "  D1 Database:  %s" resources.D1DatabaseName
            match state.D1DatabaseId with
            | Some id -> printfn "  D1 ID:        %s" id
            | None -> ()

            return Ok ()
        }
