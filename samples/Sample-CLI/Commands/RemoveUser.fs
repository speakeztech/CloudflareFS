module Sample.CLI.Commands.RemoveUser

open System
open Sample.CLI.Config
open Sample.CLI.Naming
open Sample.CLI.R2Client
open Sample.CLI.WorkersClient
open Spectre.Console

let execute (config: CloudflareConfig) (username: string) : Async<Result<unit, string>> =
    async {
        // Validate username
        match validateUsername username with
        | Error err -> return Error err
        | Ok validUsername ->

        let bucketName = getBucketName validUsername
        let secretName = getSecretName validUsername
        let bindingName = getBindingName validUsername

        AnsiConsole.MarkupLine($"[blue]Removing WebDAV user:[/] [yellow]{validUsername}[/]")
        AnsiConsole.WriteLine()

        // Step 1: Remove R2 binding from Worker
        let! bindingResult =
            AnsiConsole.Status()
                .StartAsync("Removing R2 bucket binding...", fun ctx ->
                    async {
                        let workers = WorkersOperations(config)
                        let! result = workers.RemoveBinding(config.WorkerName, bindingName)

                        match result with
                        | Ok () ->
                            AnsiConsole.MarkupLine($"[green]✓[/] Removed R2 binding: [cyan]{bindingName}[/]")
                            return Ok ()
                        | Error err ->
                            let escaped = Markup.Escape(err)
                            AnsiConsole.MarkupLine($"[yellow]⚠[/] Could not remove binding: {escaped}")
                            // Don't fail - continue cleanup
                            return Ok ()
                    } |> Async.StartAsTask)
                |> Async.AwaitTask

        match bindingResult with
        | Error err -> return Error err
        | Ok () ->

        // Step 2: Delete secret from Worker
        let! secretResult =
            AnsiConsole.Status()
                .StartAsync("Removing password secret...", fun ctx ->
                    async {
                        let workers = WorkersOperations(config)
                        let! result = workers.DeleteSecret(config.WorkerName, secretName)

                        match result with
                        | Ok () ->
                            AnsiConsole.MarkupLine($"[green]✓[/] Removed secret: [cyan]{secretName}[/]")
                            return Ok ()
                        | Error err ->
                            let escaped = Markup.Escape(err)
                            AnsiConsole.MarkupLine($"[yellow]⚠[/] Could not remove secret: {escaped}")
                            // Don't fail - continue cleanup
                            return Ok ()
                    } |> Async.StartAsTask)
                |> Async.AwaitTask

        match secretResult with
        | Error err -> return Error err
        | Ok () ->

        // Step 3: Empty and delete R2 bucket
        let! bucketResult =
            AnsiConsole.Status()
                .StartAsync("Deleting R2 bucket and contents...", fun ctx ->
                    async {
                        let r2 = R2Operations(config)

                        // First, try to empty the bucket
                        let! emptyResult = r2.EmptyBucket(bucketName)
                        match emptyResult with
                        | Ok () ->
                            AnsiConsole.MarkupLine($"[green]✓[/] Emptied bucket contents")
                        | Error err ->
                            let escaped = Markup.Escape(err)
                            AnsiConsole.MarkupLine($"[yellow]⚠[/] Could not empty bucket: {escaped}")

                        // Then delete the bucket
                        let! result = r2.DeleteBucket(bucketName)

                        match result with
                        | Ok () ->
                            AnsiConsole.MarkupLine($"[green]✓[/] Deleted bucket: [cyan]{bucketName}[/]")
                            return Ok ()
                        | Error err ->
                            let escaped = Markup.Escape(err)
                            AnsiConsole.MarkupLine($"[red]✗[/] Failed to delete bucket: {escaped}")
                            return Error err
                    } |> Async.StartAsTask)
                |> Async.AwaitTask

        match bucketResult with
        | Error err -> return Error err
        | Ok () ->

        AnsiConsole.WriteLine()
        AnsiConsole.MarkupLine("[green]✓ User fully removed via Management API![/]")

        return Ok ()
    }
