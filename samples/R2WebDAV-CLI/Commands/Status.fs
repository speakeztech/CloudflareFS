module R2WebDAV.CLI.Commands.Status

open System
open System.Net.Http
open System.Text.Json
open R2WebDAV.CLI.Config
open R2WebDAV.CLI.R2Client
open R2WebDAV.CLI.WorkersClient
open R2WebDAV.CLI.Naming
open Spectre.Console

let execute (config: CloudflareConfig) : Async<Result<unit, string>> =
    async {
        let rule = Rule($"[bold blue]R2WebDAV Status: {config.WorkerName}[/]")
        rule.Justification <- Justify.Left
        AnsiConsole.Write(rule)
        AnsiConsole.WriteLine()

        // Worker Deployment Status
        AnsiConsole.MarkupLine("[bold]Worker Deployment[/]")
        AnsiConsole.MarkupLine($"  Name: [cyan]{config.WorkerName}[/]")

        let httpClient = new HttpClient()
        httpClient.BaseAddress <- Uri("https://api.cloudflare.com/client/v4")
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiToken}")

        // Check if worker exists by trying to get its settings
        try
            let! settingsCheckResult =
                CloudFlare.Api.Compute.Workers.Http.OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/workers/scripts/{script_name}/settings"
                    [CloudFlare.Api.Compute.Workers.Http.RequestPart.path("account_id", config.AccountId)
                     CloudFlare.Api.Compute.Workers.Http.RequestPart.path("script_name", config.WorkerName)]
                    None

            let (statusCode, _) = settingsCheckResult
            if statusCode = System.Net.HttpStatusCode.OK then
                AnsiConsole.MarkupLine($"  Status: [green]✓ Deployed[/]")
            else
                AnsiConsole.MarkupLine($"  Status: [yellow]⚠ Not found[/]")
        with ex ->
            AnsiConsole.MarkupLine($"  Status: [yellow]⚠ Unable to check status[/]")

        AnsiConsole.WriteLine()

        // Get worker settings to find bound buckets
        let! settingsResult =
            CloudFlare.Api.Compute.Workers.Http.OpenApiHttp.getAsync
                httpClient
                "/accounts/{account_id}/workers/scripts/{script_name}/settings"
                [CloudFlare.Api.Compute.Workers.Http.RequestPart.path("account_id", config.AccountId)
                 CloudFlare.Api.Compute.Workers.Http.RequestPart.path("script_name", config.WorkerName)]
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

        // R2 Buckets (scoped to this worker's WebDAV instance)
        AnsiConsole.MarkupLine("[bold]R2 Buckets (WebDAV)[/]")

        // Filter to only buckets that are actually bound to this worker
        let webdavBuckets =
            boundBucketNames
            |> Set.filter (fun name -> name.EndsWith("-webdav-bucket"))
            |> Set.toList
            |> List.sort

        if List.isEmpty webdavBuckets then
            AnsiConsole.MarkupLine("  [yellow]No WebDAV buckets bound to this worker[/]")
        else
            AnsiConsole.MarkupLine($"  [green]Total:[/] {List.length webdavBuckets} bucket(s)")
            for bucketName in webdavBuckets do
                let username = bucketName.Substring(0, bucketName.Length - "-webdav-bucket".Length)
                AnsiConsole.MarkupLine($"  • [cyan]{bucketName}[/] (user: [yellow]{username}[/])")

        AnsiConsole.WriteLine()

        // Configured Users (derived from bound WebDAV buckets)
        AnsiConsole.MarkupLine("[bold]Configured Users[/]")
        if List.isEmpty webdavBuckets then
            AnsiConsole.MarkupLine("  [yellow]No users configured for this worker[/]")
        else
            let usernames =
                webdavBuckets
                |> List.map (fun bucketName -> bucketName.Substring(0, bucketName.Length - "-webdav-bucket".Length))

            AnsiConsole.MarkupLine($"  [green]{List.length usernames}[/] user(s) configured for [cyan]{config.WorkerName}[/]:")
            for username in usernames do
                let bucketName = getBucketName username
                let bindingName = getBindingName username
                let secretName = getSecretName username
                AnsiConsole.MarkupLine($"  • [yellow]{username}[/]")
                AnsiConsole.MarkupLine($"    Bucket: [dim]{bucketName}[/]")
                AnsiConsole.MarkupLine($"    Binding: [dim]{bindingName}[/]")
                AnsiConsole.MarkupLine($"    Secret: [dim]{secretName}[/]")

        return Ok ()
    }
