module LaptopDealAgent.SearchOrchestrator

open System
open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.Worker.Context
open LaptopDealAgent.Types
open LaptopDealAgent.SearchActor

/// Orchestrate parallel searches across multiple actors
let orchestrateParallelSearches (env: obj) : JS.Promise<PriceInfo list> =
    promise {
        printfn "🎬 Starting parallel search orchestration..."

        try
            // Define search queries for each model
            let searchTasks = [
                (GZ302EA_R9641TB, "ASUS ROG Flow Z13 2025 GZ302EA-R9641TB 64GB Black Friday deal under $2000", 10)
                (GZ302EA_XS99, "ASUS ROG Flow Z13 2025 GZ302EA-XS99 128GB Black Friday deal under $2300", 10)
            ]

            printfn "🚀 Spawning %d parallel search actors..." searchTasks.Length

            // Execute searches in parallel using actor system
            let! results =
                searchTasks
                |> List.map (fun (model, query, maxResults) ->
                    promise {
                        printfn "[Orchestrator] Launching actor for %s" model.ModelNumber

                        // Each actor runs independently in parallel
                        let! deals = executeSearch env model query maxResults

                        printfn "[Orchestrator] Actor %s completed: %d deals" model.ModelNumber deals.Length

                        return deals
                    }
                )
                |> Promise.all

            // Aggregate results from all actors
            let allDeals = results |> Array.collect List.toArray |> Array.toList

            printfn "✅ Orchestration complete: %d total deals from %d actors"
                allDeals.Length searchTasks.Length

            // Log summary
            for (model, _, _) in searchTasks do
                let modelDeals = allDeals |> List.filter (fun d -> d.Model = model.ModelNumber)
                printfn "   - %s: %d deals" model.ModelNumber modelDeals.Length

            return allDeals

        with ex ->
            printfn "❌ Error in orchestration: %s" ex.Message
            return []
    }

/// Get status of all search actors
let getActorStatuses (env: obj) : JS.Promise<Map<string, SearchActorState option>> =
    promise {
        try
            let models = [GZ302EA_R9641TB; GZ302EA_XS99]

            let! statuses =
                models
                |> List.map (fun model ->
                    promise {
                        let actor = getSearchActor env model

                        let payload = createObj [
                            "action" ==> "getState"
                        ]

                        let requestInit = createObj [
                            "method" ==> "POST"
                            "headers" ==> createObj [
                                "Content-Type" ==> "application/json"
                            ]
                            "body" ==> JS.JSON.stringify(payload)
                        ]

                        let! response = actor?fetch("https://fake-host/", requestInit) |> unbox<JS.Promise<Response>>

                        if response?ok |> unbox<bool> then
                            let! state = response?json() |> unbox<JS.Promise<SearchActorState option>>
                            return (model.ModelNumber, state)
                        else
                            return (model.ModelNumber, None)
                    }
                )
                |> Promise.all

            return statuses |> Array.toList |> Map.ofList

        with ex ->
            printfn "Error getting actor statuses: %s" ex.Message
            return Map.empty
    }

/// Reset all search actors (for testing)
let resetAllActors (env: obj) : JS.Promise<unit> =
    promise {
        try
            let models = [GZ302EA_R9641TB; GZ302EA_XS99]

            do!
                models
                |> List.map (fun model ->
                    promise {
                        let actor = getSearchActor env model

                        let payload = createObj [
                            "action" ==> "reset"
                        ]

                        let requestInit = createObj [
                            "method" ==> "POST"
                            "headers" ==> createObj [
                                "Content-Type" ==> "application/json"
                            ]
                            "body" ==> JS.JSON.stringify(payload)
                        ]

                        let! response = actor?fetch("https://fake-host/", requestInit) |> unbox<JS.Promise<Response>>

                        if response?ok |> unbox<bool> then
                            printfn "✓ Reset actor for %s" model.ModelNumber
                        else
                            printfn "✗ Failed to reset actor for %s" model.ModelNumber
                    }
                )
                |> Promise.all
                |> Promise.map ignore

            printfn "✓ All actors reset"

        with ex ->
            printfn "Error resetting actors: %s" ex.Message
    }
