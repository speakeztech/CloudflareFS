module R2WebDAV.CLI.Program

open System
open Argu
open Spectre.Console
open R2WebDAV.CLI.Config
open R2WebDAV.CLI.Commands

type AddUserArgs =
    | [<Mandatory; Unique>] Username of string
    | [<Mandatory; Unique>] Password of string
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Username _ -> "WebDAV username"
            | Password _ -> "WebDAV password"

type RemoveUserArgs =
    | [<Mandatory; Unique>] Username of string
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Username _ -> "WebDAV username to remove"

type ListUsersArgs =
    | [<Hidden>] Dummy
    interface IArgParserTemplate with
        member this.Usage = ""

type StatusArgs =
    | [<Hidden>] Dummy
    interface IArgParserTemplate with
        member this.Usage = ""

type DeployArgs =
    | [<AltCommandLine("-p")>] Worker_Path of string
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Worker_Path _ -> "Path to worker project directory (defaults to ../R2WebDAV)"

type CliCommand =
    | [<CliPrefix(CliPrefix.None)>] Add_User of ParseResults<AddUserArgs>
    | [<CliPrefix(CliPrefix.None)>] Remove_User of ParseResults<RemoveUserArgs>
    | [<CliPrefix(CliPrefix.None)>] List_Users of ParseResults<ListUsersArgs>
    | [<CliPrefix(CliPrefix.None)>] Status of ParseResults<StatusArgs>
    | [<CliPrefix(CliPrefix.None)>] Deploy of ParseResults<DeployArgs>
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Add_User _ -> "Add a new WebDAV user"
            | Remove_User _ -> "Remove a WebDAV user and all associated resources"
            | List_Users _ -> "List all configured WebDAV users"
            | Status _ -> "Show deployment status"
            | Deploy _ -> "Build and deploy the worker using Fable and Management API"

[<EntryPoint>]
let main argv =
    try
        let parser = ArgumentParser.Create<CliCommand>(programName = "r2webdav")

        let results = parser.ParseCommandLine(argv)

        // Load configuration
        let config =
            match loadConfig() with
            | Ok cfg -> cfg
            | Error err ->
                AnsiConsole.MarkupLine($"[red]Configuration Error:[/] {err}")
                AnsiConsole.WriteLine()
                AnsiConsole.MarkupLine("Required environment variables:")
                AnsiConsole.MarkupLine("  [cyan]CLOUDFLARE_API_TOKEN[/]")
                AnsiConsole.MarkupLine("  [cyan]CLOUDFLARE_ACCOUNT_ID[/]")
                AnsiConsole.MarkupLine("  [cyan]CLOUDFLARE_WORKER_NAME[/] (optional, defaults to 'r2-webdav-fsharp')")
                exit 1

        // Execute command
        let result =
            match results.GetSubCommand() with
            | Add_User args ->
                let username = args.GetResult AddUserArgs.Username
                let password = args.GetResult AddUserArgs.Password
                AddUser.execute config username password
                |> Async.RunSynchronously

            | Remove_User args ->
                let username = args.GetResult RemoveUserArgs.Username
                RemoveUser.execute config username
                |> Async.RunSynchronously

            | List_Users _ ->
                ListUsers.execute config
                |> Async.RunSynchronously

            | Status _ ->
                Status.execute config
                |> Async.RunSynchronously

            | Deploy args ->
                let workerPath = args.TryGetResult DeployArgs.Worker_Path
                Deploy.execute config workerPath
                |> Async.RunSynchronously

        match result with
        | Ok () -> 0
        | Error err ->
            let escaped = Markup.Escape(err)
            AnsiConsole.MarkupLine($"[red]Error:[/] {escaped}")
            1

    with
    | :? ArguParseException as ex ->
        printfn "%s" ex.Message
        1
    | ex ->
        printfn "Unexpected error: %s" ex.Message
        1
