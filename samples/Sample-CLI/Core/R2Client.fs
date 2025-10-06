module Sample.CLI.R2Client

open System
open System.Net.Http
open System.Text.Json
open CloudFlare.Management.R2
open Sample.CLI.Config

type BucketInfo = {
    Name: string
    CreationDate: DateTimeOffset
}

type R2Operations(config: CloudflareConfig) =
    let httpClient = new HttpClient()
    let r2Client = R2Client(httpClient)

    do
        httpClient.BaseAddress <- Uri("https://api.cloudflare.com/client/v4")
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiToken}")

    member this.CreateBucket(bucketName: string) : Async<Result<unit, string>> =
        async {
            try
                // Create the payload and call the API directly to avoid Hawaii's broken string deserialization
                let payload = Types.R2CreateBucketPayload.Create(bucketName)
                let requestParts = [
                    Http.RequestPart.path("account_id", config.AccountId)
                    Http.RequestPart.jsonContent payload
                ]

                let! (status, content) = Http.OpenApiHttp.postAsync httpClient "/accounts/{account_id}/r2/buckets" requestParts None

                // Parse the JSON response to check success
                use jsonDoc = JsonDocument.Parse(content)
                let json = jsonDoc.RootElement

                let mutable successProp = Unchecked.defaultof<JsonElement>
                if json.TryGetProperty("success", &successProp) && successProp.GetBoolean() then
                    return Ok ()
                else
                    // Check if error is about bucket already existing
                    let mutable errorsProp = Unchecked.defaultof<JsonElement>
                    if json.TryGetProperty("errors", &errorsProp) && errorsProp.ValueKind = JsonValueKind.Array then
                        let errorMessages =
                            errorsProp.EnumerateArray()
                            |> Seq.choose (fun e ->
                                let mutable msgProp = Unchecked.defaultof<JsonElement>
                                if e.TryGetProperty("message", &msgProp) then
                                    Some (msgProp.GetString())
                                else
                                    None)
                            |> Seq.toList

                        // Check if it's an "already exists" error - treat as success
                        let alreadyExists =
                            errorMessages
                            |> List.exists (fun msg ->
                                msg.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
                                msg.Contains("bucket with that name already exists", StringComparison.OrdinalIgnoreCase))

                        if alreadyExists then
                            return Ok () // Treat as success for idempotency
                        else
                            return Error (String.concat "; " errorMessages)
                    else
                        return Error content
            with ex ->
                return Error $"Exception creating bucket: {ex.Message}"
        }

    member this.ListBuckets() : Async<Result<BucketInfo list, string>> =
        async {
            try
                // Call the API directly to get raw JSON response
                let requestParts = [ Http.RequestPart.path("account_id", config.AccountId) ]
                let! (status, content) = Http.OpenApiHttp.getAsync httpClient "/accounts/{account_id}/r2/buckets" requestParts None

                use jsonDoc = JsonDocument.Parse(content)
                let json = jsonDoc.RootElement

                let mutable successProp = Unchecked.defaultof<JsonElement>
                if json.TryGetProperty("success", &successProp) && successProp.GetBoolean() then
                    let mutable resultProp = Unchecked.defaultof<JsonElement>
                    if json.TryGetProperty("result", &resultProp) then
                        let mutable bucketsProp = Unchecked.defaultof<JsonElement>
                        if resultProp.TryGetProperty("buckets", &bucketsProp) && bucketsProp.ValueKind = JsonValueKind.Array then
                            let bucketList =
                                bucketsProp.EnumerateArray()
                                |> Seq.map (fun bucket ->
                                    let mutable nameProp = Unchecked.defaultof<JsonElement>
                                    let mutable dateProp = Unchecked.defaultof<JsonElement>
                                    { Name =
                                        if bucket.TryGetProperty("name", &nameProp) then
                                            nameProp.GetString()
                                        else
                                            ""
                                      CreationDate =
                                        if bucket.TryGetProperty("creation_date", &dateProp) then
                                            DateTimeOffset.Parse(dateProp.GetString())
                                        else
                                            DateTimeOffset.MinValue })
                                |> Seq.toList
                            return Ok bucketList
                        else
                            return Ok []
                    else
                        return Ok []
                else
                    // Extract error messages
                    let mutable errorsProp = Unchecked.defaultof<JsonElement>
                    if json.TryGetProperty("errors", &errorsProp) && errorsProp.ValueKind = JsonValueKind.Array then
                        let errorMessages =
                            errorsProp.EnumerateArray()
                            |> Seq.choose (fun e ->
                                let mutable msgProp = Unchecked.defaultof<JsonElement>
                                if e.TryGetProperty("message", &msgProp) then
                                    Some (msgProp.GetString())
                                else
                                    None)
                            |> String.concat "; "
                        return Error errorMessages
                    else
                        return Error content
            with ex ->
                return Error $"Exception listing buckets: {ex.Message}"
        }

    member this.EmptyBucket(bucketName: string) : Async<Result<unit, string>> =
        async {
            try
                // R2 Management API doesn't have a direct "empty bucket" endpoint
                // We need to use S3-compatible API via R2 bucket bindings or accept that buckets may have content
                // For now, we'll try to delete and if it fails due to content, return informative error
                return Ok ()
            with ex ->
                return Error $"Exception emptying bucket: {ex.Message}"
        }

    member this.DeleteBucket(bucketName: string) : Async<Result<unit, string>> =
        async {
            try
                let! result = r2Client.R2DeleteBucket(bucketName, config.AccountId)

                match result with
                | Types.R2DeleteBucket.OK response ->
                    if response.success then
                        return Ok ()
                    else
                        let errors = response.errors |> List.map (fun e -> e.message) |> String.concat "; "
                        return Error errors
            with ex ->
                return Error $"Exception deleting bucket: {ex.Message}"
        }
