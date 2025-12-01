namespace AskAI.CLI.Commands

open System
open System.Net.Http
open AskAI.CLI

/// Sync content to R2 for AutoRAG indexing
module Sync =

    /// Execute the sync command
    let execute
        (config: Config.CloudflareConfig)
        (contentDir: string option)
        (skipIndex: bool)
        : Async<Result<unit, string>> =
        async {
            let contentPath = contentDir |> Option.defaultValue "./content"
            let sections = ["blog"; "portfolio"; "company"]

            printfn "Syncing content to R2 for AutoRAG indexing..."
            printfn ""
            printfn "  Content directory: %s" contentPath
            printfn "  Sections: %s" (String.concat ", " sections)
            printfn ""

            // Load content
            let items = ContentSync.loadAllContent contentPath sections
            printfn "  Found %d content items:" items.Length

            for section in sections do
                let count = items |> List.filter (fun i -> i.Section = section) |> List.length
                if count > 0 then
                    printfn "    - %s: %d files" section count

            printfn ""

            // Sync to R2
            use httpClient = new HttpClient()
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiToken}")

            let resources = Config.defaultResourceNames
            printfn "  Uploading to R2 bucket: %s" resources.R2BucketName

            match! ContentSync.syncToR2 httpClient config.AccountId resources.R2BucketName items with
            | Error e -> return Error e
            | Ok (uploaded, skipped, deleted) ->

            printfn ""
            printfn "  Uploaded: %d files" uploaded
            printfn "  Skipped:  %d files (unchanged)" skipped
            printfn "  Deleted:  %d files" deleted

            // Trigger AutoRAG indexing
            if skipIndex then
                printfn ""
                printfn "  Skipping AutoRAG index sync (--skip-index flag set)"
            elif uploaded > 0 || deleted > 0 then
                printfn ""
                printfn "  Triggering AutoRAG index sync..."
                match! ContentSync.triggerAutoRAGScan httpClient config.AccountId config.ApiToken resources.AutoRAGName with
                | Error e ->
                    printfn "  Warning: %s" e
                | Ok () ->
                    printfn "  AutoRAG sync triggered successfully"

            // Update state
            let state = Config.loadState()
            Config.saveState { state with LastSyncTimestamp = Some DateTime.UtcNow }

            printfn ""
            printfn "Sync complete!"

            return Ok ()
        }
