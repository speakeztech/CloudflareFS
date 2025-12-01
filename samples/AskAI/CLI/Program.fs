module AskAI.CLI.Program

open System
open Argu
open AskAI.CLI
open AskAI.CLI.Commands

/// CLI argument types
type ProvisionArgs =
    | [<Hidden>] Dummy
    interface IArgParserTemplate with
        member this.Usage = ""

type DeployArgs =
    | [<AltCommandLine("-p")>] Worker_Path of string
    | [<AltCommandLine("-f")>] Force
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Worker_Path _ -> "Path to worker project directory"
            | Force -> "Force redeployment even if source hasn't changed"

type SyncArgs =
    | [<AltCommandLine("-c")>] Content_Dir of string
    | [<AltCommandLine("-s")>] Skip_Index
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Content_Dir _ -> "Path to content directory"
            | Skip_Index -> "Skip triggering AutoRAG index after sync"

type StatusArgs =
    | [<Hidden>] Dummy
    interface IArgParserTemplate with
        member this.Usage = ""

type CliCommand =
    | [<CliPrefix(CliPrefix.None)>] Provision of ParseResults<ProvisionArgs>
    | [<CliPrefix(CliPrefix.None)>] Deploy of ParseResults<DeployArgs>
    | [<CliPrefix(CliPrefix.None)>] Sync of ParseResults<SyncArgs>
    | [<CliPrefix(CliPrefix.None)>] Status of ParseResults<StatusArgs>
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Provision _ -> "Create R2 bucket, D1 database, and other resources"
            | Deploy _ -> "Build and deploy the Ask AI worker"
            | Sync _ -> "Sync content to R2 for AutoRAG indexing"
            | Status _ -> "Show deployment status"

[<EntryPoint>]
let main argv =
    try
        let parser = ArgumentParser.Create<CliCommand>(programName = "ask-ai-cli")

        if argv.Length = 0 then
            printfn "%s" (parser.PrintUsage())
            0
        else

        let results = parser.ParseCommandLine(argv)

        // Load configuration
        let config =
            match Config.loadConfig() with
            | Ok cfg -> cfg
            | Error err ->
                printfn "Configuration Error: %s" err
                printfn ""
                printfn "Required environment variables:"
                printfn "  CLOUDFLARE_API_TOKEN"
                printfn "  CLOUDFLARE_ACCOUNT_ID"
                exit 1

        // Execute command
        let result =
            match results.GetSubCommand() with
            | Provision _ ->
                Provision.execute config
                |> Async.RunSynchronously

            | Deploy args ->
                let workerPath = args.TryGetResult DeployArgs.Worker_Path
                let force = args.Contains DeployArgs.Force
                Deploy.execute config workerPath force
                |> Async.RunSynchronously

            | Sync args ->
                let contentDir = args.TryGetResult SyncArgs.Content_Dir
                let skipIndex = args.Contains SyncArgs.Skip_Index
                Sync.execute config contentDir skipIndex
                |> Async.RunSynchronously

            | Status _ ->
                Status.execute config
                |> Async.RunSynchronously

        match result with
        | Ok () -> 0
        | Error err ->
            printfn "Error: %s" err
            1

    with
    | :? ArguParseException as ex ->
        printfn "%s" ex.Message
        1
    | ex ->
        printfn "Unexpected error: %s" ex.Message
        1
