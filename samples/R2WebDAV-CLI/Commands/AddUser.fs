module R2WebDAV.CLI.Commands.AddUser

open System
open R2WebDAV.CLI.Config
open R2WebDAV.CLI.Naming
open R2WebDAV.CLI.R2Client
open Spectre.Console

let execute (config: CloudflareConfig) (username: string) (password: string) : Async<Result<unit, string>> =
    async {
        // Validate username
        match validateUsername username with
        | Error err -> return Error err
        | Ok validUsername ->

        let bucketName = getBucketName validUsername
        let secretName = getSecretName validUsername
        let bindingName = getBindingName validUsername

        AnsiConsole.MarkupLine($"[blue]Creating WebDAV user:[/] [yellow]{validUsername}[/]")
        AnsiConsole.WriteLine()

        // Step 1: Create R2 bucket
        AnsiConsole.Status()
            .Start("Creating R2 bucket...", fun ctx ->
                async {
                    let r2 = R2Operations(config)
                    let! result = r2.CreateBucket(bucketName)

                    match result with
                    | Ok () ->
                        AnsiConsole.MarkupLine($"[green]✓[/] Created bucket: [cyan]{bucketName}[/]")
                        return Ok ()
                    | Error err ->
                        AnsiConsole.MarkupLine($"[red]✗[/] Failed to create bucket: {err}")
                        return Error err
                } |> Async.RunSynchronously
            ) |> ignore

        // Step 2: Store secret (TODO: Use Management API when available)
        AnsiConsole.MarkupLine($"[yellow]⚠[/] Secret storage via wrangler CLI (Management API pending)")
        AnsiConsole.MarkupLine($"   Run: [cyan]wrangler secret put {secretName}[/]")

        // Step 3: Update wrangler.toml (TODO: Implement TOML manipulation)
        AnsiConsole.MarkupLine($"[yellow]⚠[/] TOML update pending")
        AnsiConsole.MarkupLine($"   Add to wrangler.toml:")
        AnsiConsole.MarkupLine($"   [cyan][[r2_buckets]][/]")
        AnsiConsole.MarkupLine($"   [cyan]binding = \"{bindingName}\"[/]")
        AnsiConsole.MarkupLine($"   [cyan]bucket_name = \"{bucketName}\"[/]")

        AnsiConsole.WriteLine()
        AnsiConsole.MarkupLine("[green]User creation initiated![/]")
        AnsiConsole.MarkupLine("Complete the manual steps above to finish setup.")

        return Ok ()
    }
