module R2WebDAV.CLI.Commands.AddUser

open System
open System.Net.Http
open System.Text.Json
open R2WebDAV.CLI.Config
open R2WebDAV.CLI.Naming
open R2WebDAV.CLI.R2Client
open R2WebDAV.CLI.WorkersClient
open Spectre.Console

let execute (config: CloudflareConfig) (username: string) (password: string) : Async<Result<unit, string>> =
    async {
        // Validate username
        match validateUsername username with
        | Error err -> return Error err
        | Ok validUsername ->

        let bucketName = getBucketName validUsername
        let secretName = getSecretName validUsername

        AnsiConsole.MarkupLine($"[blue]Creating WebDAV user:[/] [yellow]{validUsername}[/]")
        AnsiConsole.WriteLine()

        // Step 1: Create R2 bucket using Management API (idempotent)
        let! bucketResult =
            AnsiConsole.Status()
                .StartAsync("Checking/creating R2 bucket...", fun ctx ->
                    async {
                        let r2 = R2Operations(config)

                        // Check if bucket already exists
                        let! existingBuckets = r2.ListBuckets()
                        match existingBuckets with
                        | Ok buckets when buckets |> List.exists (fun b -> b.Name = bucketName) ->
                            AnsiConsole.MarkupLine($"[yellow]⚠[/] Bucket [cyan]{bucketName}[/] already exists, skipping creation")
                            return Ok ()
                        | Ok _ ->
                            // Bucket doesn't exist, create it
                            let! result = r2.CreateBucket(bucketName)
                            match result with
                            | Ok () ->
                                AnsiConsole.MarkupLine($"[green]✓[/] Created bucket: [cyan]{bucketName}[/]")
                                return Ok ()
                            | Error err ->
                                let escaped = Markup.Escape(err)
                                AnsiConsole.MarkupLine($"[red]✗[/] Failed to create bucket: {escaped}")
                                return Error err
                        | Error err ->
                            let escaped = Markup.Escape(err)
                            AnsiConsole.MarkupLine($"[red]✗[/] Failed to list buckets: {escaped}")
                            return Error err
                    } |> Async.StartAsTask)
                |> Async.AwaitTask

        match bucketResult with
        | Error err -> return Error err
        | Ok () ->

        AnsiConsole.WriteLine()
        AnsiConsole.MarkupLine("[green]✓ R2 bucket provisioned via Management API![/]")
        AnsiConsole.WriteLine()

        // Step 2: Store/update password secret using Workers Management API (idempotent)
        let! secretResult =
            AnsiConsole.Status()
                .StartAsync("Setting password secret...", fun ctx ->
                    async {
                        let workers = WorkersOperations(config)
                        let! result = workers.PutSecret(config.WorkerName, secretName, password)

                        match result with
                        | Ok () ->
                            AnsiConsole.MarkupLine($"[green]✓[/] Set secret: [cyan]{secretName}[/]")
                            AnsiConsole.MarkupLine($"   [dim]Note: This will update password if user exists[/]")
                            return Ok ()
                        | Error err ->
                            let escaped = Markup.Escape(err)
                            AnsiConsole.MarkupLine($"[red]✗[/] Failed to set secret: {escaped}")
                            return Error err
                    } |> Async.StartAsTask)
                |> Async.AwaitTask

        match secretResult with
        | Error err -> return Error err
        | Ok () ->

        // Step 3: Configure R2 bucket binding using Workers Management API
        let! bindingResult =
            AnsiConsole.Status()
                .StartAsync("Configuring R2 bucket binding...", fun ctx ->
                    async {
                        let workers = WorkersOperations(config)

                        // Create R2 bucket binding for the new user
                        let bindingName = getBindingName validUsername
                        let newR2Binding =
                            CloudFlare.Api.Compute.Workers.Types.workersbindingkindr2bucket.Create(
                                bucketName,
                                bindingName,
                                CloudFlare.Api.Compute.Workers.Types.workersbindingkindr2bucketType.R2_bucket
                            )
                        let newBinding = CloudFlare.Api.Compute.Workers.Types.workersbindingitem.R2bucket newR2Binding

                        // Get current worker settings to preserve existing bindings
                        let httpClient = new HttpClient()
                        httpClient.BaseAddress <- Uri("https://api.cloudflare.com/client/v4")
                        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiToken}")

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

                        // Extract existing R2 bindings, excluding any that match our new binding name
                        let existingBindings =
                            let mutable resultProp = Unchecked.defaultof<JsonElement>
                            if settingsJson.TryGetProperty("result", &resultProp) then
                                let mutable bindingsProp = Unchecked.defaultof<JsonElement>
                                if resultProp.TryGetProperty("bindings", &bindingsProp) && bindingsProp.ValueKind = JsonValueKind.Array then
                                    bindingsProp.EnumerateArray()
                                    |> Seq.choose (fun binding ->
                                        let mutable typeProp = Unchecked.defaultof<JsonElement>
                                        let mutable nameProp = Unchecked.defaultof<JsonElement>
                                        let mutable bucketNameProp = Unchecked.defaultof<JsonElement>

                                        if binding.TryGetProperty("type", &typeProp) &&
                                           typeProp.GetString() = "r2_bucket" &&
                                           binding.TryGetProperty("name", &nameProp) &&
                                           binding.TryGetProperty("bucket_name", &bucketNameProp) then
                                            let existingBindingName = nameProp.GetString()
                                            // Skip if this is the binding we're adding/updating
                                            if existingBindingName = bindingName then
                                                None
                                            else
                                                // Recreate the binding
                                                let r2Binding =
                                                    CloudFlare.Api.Compute.Workers.Types.workersbindingkindr2bucket.Create(
                                                        bucketNameProp.GetString(),
                                                        existingBindingName,
                                                        CloudFlare.Api.Compute.Workers.Types.workersbindingkindr2bucketType.R2_bucket
                                                    )
                                                Some (CloudFlare.Api.Compute.Workers.Types.workersbindingitem.R2bucket r2Binding)
                                        else
                                            None)
                                    |> List.ofSeq
                                else
                                    []
                            else
                                []

                        // Combine existing bindings with the new one
                        let allBindings = newBinding :: existingBindings

                        let! result = workers.UpdateWorkerBindings(config.WorkerName, allBindings)

                        match result with
                        | Ok () ->
                            AnsiConsole.MarkupLine($"[green]✓[/] Configured R2 binding: [cyan]{bindingName}[/] → [cyan]{bucketName}[/]")
                            return Ok ()
                        | Error err ->
                            let escaped = Markup.Escape(err)
                            AnsiConsole.MarkupLine($"[red]✗[/] Failed to configure binding: {escaped}")
                            return Error err
                    } |> Async.StartAsTask)
                |> Async.AwaitTask

        match bindingResult with
        | Error err -> return Error err
        | Ok () ->

        AnsiConsole.WriteLine()
        AnsiConsole.MarkupLine("[green]✓ User fully configured via Management API![/]")
        AnsiConsole.WriteLine()
        AnsiConsole.MarkupLine("[dim]This command is idempotent - run it multiple times to update passwords or bindings[/]")

        return Ok ()
    }
