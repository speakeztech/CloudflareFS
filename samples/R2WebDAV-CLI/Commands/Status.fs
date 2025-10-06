module R2WebDAV.CLI.Commands.Status

open R2WebDAV.CLI.Config
open R2WebDAV.CLI.R2Client
open Spectre.Console

let execute (config: CloudflareConfig) : Async<Result<unit, string>> =
    async {
        let rule = Rule("[bold blue]R2WebDAV Deployment Status[/]")
        rule.Justification <- Justify.Left
        AnsiConsole.Write(rule)
        AnsiConsole.WriteLine()

        // Worker Deployment Info
        AnsiConsole.MarkupLine("[bold]Worker Deployment[/]")
        AnsiConsole.MarkupLine($"  Name: [cyan]{config.WorkerName}[/]")
        AnsiConsole.MarkupLine($"  Status: [yellow]⚠ Check manually with wrangler[/]")
        AnsiConsole.WriteLine()

        // R2 Buckets
        AnsiConsole.MarkupLine("[bold]R2 Buckets[/]")

        let r2 = R2Operations(config)
        let! result = r2.ListBuckets()

        match result with
        | Error err ->
            AnsiConsole.MarkupLine($"  [red]Error:[/] {err}")
            return Error err

        | Ok buckets ->
            if List.isEmpty buckets then
                AnsiConsole.MarkupLine("  [yellow]No buckets found[/]")
            else
                AnsiConsole.MarkupLine($"  [green]Total:[/] {List.length buckets} bucket(s)")
                for bucket in buckets do
                    AnsiConsole.MarkupLine($"  • [cyan]{bucket.Name}[/]")

            AnsiConsole.WriteLine()

            // Configured Users
            let userBuckets =
                buckets
                |> List.filter (fun b -> b.Name.EndsWith("-webdav-bucket"))

            AnsiConsole.MarkupLine("[bold]Configured Users[/]")
            if List.isEmpty userBuckets then
                AnsiConsole.MarkupLine("  [yellow]No users configured[/]")
            else
                let usernames =
                    userBuckets
                    |> List.map (fun b -> b.Name.Substring(0, b.Name.Length - "-webdav-bucket".Length))
                    |> String.concat ", "

                AnsiConsole.MarkupLine($"  [green]{List.length userBuckets}[/] user(s): [cyan]{usernames}[/]")

            return Ok ()
    }
