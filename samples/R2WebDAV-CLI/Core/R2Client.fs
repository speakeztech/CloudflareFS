module R2WebDAV.CLI.R2Client

open System
open System.Net.Http
open R2WebDAV.CLI.Config

type BucketInfo = {
    Name: string
    CreationDate: DateTimeOffset
}

type R2Operations(config: CloudflareConfig) =
    let httpClient = new HttpClient()

    do
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiToken}")

    member this.CreateBucket(bucketName: string) : Async<Result<unit, string>> =
        async {
            try
                let url = $"https://api.cloudflare.com/client/v4/accounts/{config.AccountId}/r2/buckets"
                let content = new StringContent($"{{\"name\":\"{bucketName}\"}}", Text.Encoding.UTF8, "application/json")

                let! response = httpClient.PostAsync(url, content) |> Async.AwaitTask
                let! responseText = response.Content.ReadAsStringAsync() |> Async.AwaitTask

                if response.IsSuccessStatusCode then
                    return Ok ()
                else
                    return Error $"Failed to create bucket: {responseText}"
            with ex ->
                return Error $"Exception creating bucket: {ex.Message}"
        }

    member this.ListBuckets() : Async<Result<BucketInfo list, string>> =
        async {
            try
                let url = $"https://api.cloudflare.com/client/v4/accounts/{config.AccountId}/r2/buckets"

                let! response = httpClient.GetAsync(url) |> Async.AwaitTask
                let! responseText = response.Content.ReadAsStringAsync() |> Async.AwaitTask

                if response.IsSuccessStatusCode then
                    // TODO: Parse JSON response properly
                    // For now, return empty list as placeholder
                    return Ok []
                else
                    return Error $"Failed to list buckets: {responseText}"
            with ex ->
                return Error $"Exception listing buckets: {ex.Message}"
        }

    member this.DeleteBucket(bucketName: string) : Async<Result<unit, string>> =
        async {
            try
                let url = $"https://api.cloudflare.com/client/v4/accounts/{config.AccountId}/r2/buckets/{bucketName}"

                let! response = httpClient.DeleteAsync(url) |> Async.AwaitTask
                let! responseText = response.Content.ReadAsStringAsync() |> Async.AwaitTask

                if response.IsSuccessStatusCode then
                    return Ok ()
                else
                    return Error $"Failed to delete bucket: {responseText}"
            with ex ->
                return Error $"Exception deleting bucket: {ex.Message}"
        }
