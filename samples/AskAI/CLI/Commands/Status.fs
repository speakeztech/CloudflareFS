namespace AskAI.CLI.Commands

open System
open AskAI.CLI

/// Show deployment status
module Status =

    /// Execute the status command
    let execute (config: Config.CloudflareConfig) : Async<Result<unit, string>> =
        async {
            let state = Config.loadState()
            let resources = Config.defaultResourceNames

            printfn "Ask AI Deployment Status"
            printfn "========================"
            printfn ""

            printfn "Configuration:"
            printfn "  Account ID:     %s...%s" (config.AccountId.Substring(0, 4)) (config.AccountId.Substring(config.AccountId.Length - 4))
            printfn ""

            printfn "Resources:"
            printfn "  R2 Bucket:      %s %s"
                resources.R2BucketName
                (if state.R2BucketCreated then "[created]" else "[not created]")

            printfn "  D1 Database:    %s %s"
                resources.D1DatabaseName
                (match state.D1DatabaseId with Some id -> $"[{id.Substring(0, 8)}...]" | None -> "[not created]")

            printfn "  AutoRAG:        %s" resources.AutoRAGName
            printfn ""

            printfn "Worker:"
            printfn "  Name:           %s" resources.WorkerName
            printfn "  Deployed:       %s" (if state.WorkerDeployed then "Yes" else "No")
            match state.WorkerUrl with
            | Some url -> printfn "  URL:            %s" url
            | None -> ()
            match state.LastDeployHash with
            | Some hash -> printfn "  Last Hash:      %s..." (hash.Substring(0, 16))
            | None -> ()
            printfn ""

            printfn "Content Sync:"
            match state.LastSyncTimestamp with
            | Some ts -> printfn "  Last Sync:      %s" (ts.ToString("yyyy-MM-dd HH:mm:ss UTC"))
            | None -> printfn "  Last Sync:      Never"

            match state.LastDeployedCommit with
            | Some commit -> printfn "  Last Commit:    %s" commit
            | None -> ()

            return Ok ()
        }
