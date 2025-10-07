module Sample.CLI.Commands.ListUsers

open System
open System.Net.Http
open System.Text.Json
open Sample.CLI.Config
open Sample.CLI.R2Client
open Spectre.Console

let execute (config: CloudflareConfig) : Async<Result<unit, string>> =
    async {
        // Get worker settings to find bound buckets
        let httpClient = new HttpClient()
        httpClient.BaseAddress <- Uri("https://api.cloudflare.com/client/v4")
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiToken}")

        let! settingsResult =
            CloudFlare.Management.Workers.Http.OpenApiHttp.getAsync
                httpClient
                "/accounts/{account_id}/workers/scripts/{script_name}/settings"
                [CloudFlare.Management.Workers.Http.RequestPart.path("account_id", config.AccountId)
                 CloudFlare.Management.Workers.Http.RequestPart.path("script_name", config.WorkerName)]
                None

        let (_, settingsContent) = settingsResult
        use settingsJsonDoc = JsonDocument.Parse(settingsContent)
        let settingsJson = settingsJsonDoc.RootElement

        // Extract bucket names from R2 bindings
        let boundBucketNames =
            let mutable resultProp = Unchecked.defaultof<JsonElement>
            if settingsJson.TryGetProperty("result", &resultProp) then
                let mutable bindingsProp = Unchecked.defaultof<JsonElement>
                if resultProp.TryGetProperty("bindings", &bindingsProp) && bindingsProp.ValueKind = JsonValueKind.Array then
                    bindingsProp.EnumerateArray()
                    |> Seq.choose (fun binding ->
                        let mutable typeProp = Unchecked.defaultof<JsonElement>
                        let mutable bucketNameProp = Unchecked.defaultof<JsonElement>
                        if binding.TryGetProperty("type", &typeProp) &&
                           typeProp.GetString() = "r2_bucket" &&
                           binding.TryGetProperty("bucket_name", &bucketNameProp) then
                            Some (bucketNameProp.GetString())
                        else
                            None)
                    |> Set.ofSeq
                else
                    Set.empty
            else
                Set.empty

        // Filter to only WebDAV buckets bound to this worker
        let webdavBucketNames =
            boundBucketNames
            |> Set.filter (fun name -> name.EndsWith("-webdav-bucket"))

        if Set.isEmpty webdavBucketNames then
            AnsiConsole.MarkupLine("[yellow]No WebDAV users configured for this worker[/]")
            return Ok ()
        else
            // Get bucket details (creation dates)
            let r2 = R2Operations(config)
            let! bucketsResult = r2.ListBuckets()

            match bucketsResult with
            | Error err ->
                let escaped = Markup.Escape(err)
                AnsiConsole.MarkupLine($"[red]Error listing buckets:[/] {escaped}")
                return Error err

            | Ok allBuckets ->
                // Filter to only buckets that are bound to this worker
                let boundBuckets =
                    allBuckets
                    |> List.filter (fun b -> webdavBucketNames.Contains(b.Name))
                    |> List.sortBy (fun b -> b.Name)

                let table = Table()
                table.AddColumn("Username") |> ignore
                table.AddColumn("Bucket Name") |> ignore
                table.AddColumn("Created") |> ignore

                for bucket in boundBuckets do
                    // Extract username from bucket name (remove -webdav-bucket suffix)
                    let username = bucket.Name.Substring(0, bucket.Name.Length - "-webdav-bucket".Length)

                    table.AddRow(
                        username,
                        bucket.Name,
                        bucket.CreationDate.ToString("yyyy-MM-dd HH:mm:ss")
                    ) |> ignore

                AnsiConsole.Write(table)

                return Ok ()
    }
