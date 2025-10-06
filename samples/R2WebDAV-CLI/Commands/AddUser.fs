module R2WebDAV.CLI.Commands.AddUser

open System
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

                        // Create R2 bucket binding
                        let bindingName = getBindingName validUsername
                        let r2Binding =
                            CloudFlare.Api.Compute.Workers.Types.workersbindingkindr2bucket.Create(
                                bucketName,
                                bindingName,
                                CloudFlare.Api.Compute.Workers.Types.workersbindingkindr2bucketType.R2_bucket
                            )

                        // Wrap in discriminated union
                        let binding = CloudFlare.Api.Compute.Workers.Types.workersbindingitem.R2bucket r2Binding

                        let! result = workers.UpdateWorkerBindings(config.WorkerName, [binding])

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
