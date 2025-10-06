module R2WebDAV.CLI.Commands.ListUsers

open R2WebDAV.CLI.Config
open R2WebDAV.CLI.R2Client
open Spectre.Console

let execute (config: CloudflareConfig) : Async<Result<unit, string>> =
    async {
        let r2 = R2Operations(config)

        let! result = r2.ListBuckets()

        match result with
        | Error err ->
            AnsiConsole.MarkupLine($"[red]Error listing buckets:[/] {err}")
            return Error err

        | Ok buckets ->
            if List.isEmpty buckets then
                AnsiConsole.MarkupLine("[yellow]No WebDAV users configured[/]")
            else
                let table = Table()
                table.AddColumn("Username") |> ignore
                table.AddColumn("Bucket Name") |> ignore
                table.AddColumn("Created") |> ignore

                for bucket in buckets do
                    // Extract username from bucket name (remove -webdav-bucket suffix)
                    let username =
                        if bucket.Name.EndsWith("-webdav-bucket") then
                            bucket.Name.Substring(0, bucket.Name.Length - "-webdav-bucket".Length)
                        else
                            bucket.Name

                    table.AddRow(
                        username,
                        bucket.Name,
                        bucket.CreationDate.ToString("yyyy-MM-dd HH:mm:ss")
                    ) |> ignore

                AnsiConsole.Write(table)

            return Ok ()
    }
