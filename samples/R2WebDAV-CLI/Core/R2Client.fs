module R2WebDAV.CLI.R2Client

open System
open System.Net.Http
open FSharp.Data
open CloudFlare.Management.R2
open R2WebDAV.CLI.Config

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
                let payload = Types.R2CreateBucketPayload.Create(name = bucketName)
                let! result = r2Client.R2CreateBucket(config.AccountId, payload)

                match result with
                | Types.R2CreateBucket.OK jsonPayload ->
                    let json = JsonValue.Parse(jsonPayload)
                    match json.TryGetProperty("success") with
                    | Some success when success.AsBoolean() ->
                        return Ok ()
                    | _ ->
                        return Error jsonPayload
            with ex ->
                return Error $"Exception creating bucket: {ex.Message}"
        }

    member this.ListBuckets() : Async<Result<BucketInfo list, string>> =
        async {
            try
                // Call the API directly to get raw JSON response
                let requestParts = [ Http.RequestPart.path("account_id", config.AccountId) ]
                let! (status, content) = Http.OpenApiHttp.getAsync httpClient "/accounts/{account_id}/r2/buckets" requestParts None

                let json = JsonValue.Parse(content)

                match json.TryGetProperty("success") with
                | Some success when success.AsBoolean() ->
                    match json.TryGetProperty("result") with
                    | Some result ->
                        match result.TryGetProperty("buckets") with
                        | Some (JsonValue.Array buckets) ->
                            let bucketList =
                                buckets
                                |> Array.map (fun bucket ->
                                    { Name = bucket.["name"].AsString()
                                      CreationDate = DateTimeOffset.Parse(bucket.["creation_date"].AsString()) })
                                |> Array.toList
                            return Ok bucketList
                        | _ ->
                            return Ok []
                    | None ->
                        return Ok []
                | _ ->
                    // Extract error messages
                    match json.TryGetProperty("errors") with
                    | Some (JsonValue.Array errors) when errors.Length > 0 ->
                        let errorMessages =
                            errors
                            |> Array.choose (fun e ->
                                match e.TryGetProperty("message") with
                                | Some msg -> Some (msg.AsString())
                                | None -> None)
                            |> String.concat "; "
                        return Error errorMessages
                    | _ ->
                        return Error content
            with ex ->
                return Error $"Exception listing buckets: {ex.Message}"
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
