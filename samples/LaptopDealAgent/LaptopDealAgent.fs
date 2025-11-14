module LaptopDealAgent.Main

open System
open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.Worker.Context
open CloudFlare.Worker.Context.Globals
open CloudFlare.Worker.Context.Helpers
open CloudFlare.Worker.Context.Helpers.ResponseBuilder
open CloudFlare.D1
open LaptopDealAgent.Types
open LaptopDealAgent.AIAnalyzer
open LaptopDealAgent.SearchActor
open LaptopDealAgent.SearchOrchestrator
open LaptopDealAgent.SearchAgent
open LaptopDealAgent.PriceAnalyzer
open LaptopDealAgent.D1Storage

/// Environment bindings
[<AllowNullLiteral>]
type LaptopAgentEnv =
    inherit Env
    abstract DB: D1Database
    abstract AI: obj
    abstract SEARCH_ACTOR: obj

/// Run the scheduled agent task
let runScheduledTask (env: LaptopAgentEnv) : JS.Promise<string> =
    promise {
        printfn "🤖 Starting laptop deal agent..."

        try
            // Execute parallel AI-powered searches using actor system
            printfn "🔍 Orchestrating parallel AI searches with idempotency..."
            let! priceInfos = orchestrateParallelSearches env

            printfn $"✅ Found {priceInfos.Length} valid deals from parallel actors"

            // Store deals in D1 database
            printfn "💾 Storing deals in D1 database..."
            do!
                priceInfos
                |> List.map (upsertDeal env.DB)
                |> Promise.all
                |> Promise.map ignore

            // Get analysis from D1
            printfn "📊 Analyzing prices from D1..."
            let! analysisList = getAnalysis env.DB

            // Generate report
            printfn "📝 Generating report..."
            let report = formatAnalysisReport analysisList

            // Print summary to logs
            for analysis in analysisList do
                printfn $"\n{'='|> String.replicate 60}"
                printfn $"Model: {analysis.Model.FullName}"
                printfn $"Current Best: {analysis.CurrentBestPrice |> Option.map (sprintf \"$%.2f\") |> Option.defaultValue \"N/A\"}"
                printfn $"Retailer: {analysis.BestRetailer |> Option.defaultValue \"N/A\"}"
                printfn $"Trend: {analysis.PriceTrend}"
                printfn $"Recommendation: {analysis.Recommendation}"
                printfn $"{'='|> String.replicate 60}\n"

            printfn "✅ Laptop deal agent completed successfully"
            return report

        with
        | ex ->
            let errorMsg = $"❌ Error in scheduled task: {ex.Message}"
            printfn "%s" errorMsg
            return errorMsg
    }

/// Handle scheduled execution
let handleScheduled (event: obj) (env: LaptopAgentEnv) (ctx: ExecutionContext) =
    promise {
        printfn "⏰ Scheduled event triggered"
        let! result = runScheduledTask env
        printfn "📋 Result: %s" (if result.Contains("Error") then "Failed" else "Success")
    }

/// Handle HTTP requests (for manual triggers and viewing reports)
let fetch (request: Request) (env: LaptopAgentEnv) (ctx: ExecutionContext) =
    promise {
        let path = Request.getPath request
        let method = Request.getMethod request

        match method, path with
        | "GET", "/" ->
            // Run the agent and return the report
            let! report = runScheduledTask env

            return Response.Create(U2.Case1 report, jsOptions(fun o ->
                o.headers <- Some (U2.Case1 (createObj ["Content-Type" ==> "text/html; charset=utf-8"]))
                o.status <- Some 200.0
            ))

        | "GET", "/api/deals" ->
            // Get current deals from D1
            try
                let! deals = getAllDeals env.DB

                return json {|
                    deals = deals
                    lastUpdated = DateTime.UtcNow
                    totalDeals = deals.Length
                |} 200
            with
            | ex ->
                return json {| error = ex.Message |} 500

        | "GET", "/api/analysis" ->
            // Get current analyses from D1
            try
                let! analyses = getAnalysis env.DB

                return json {|
                    analyses = analyses
                    generatedAt = DateTime.UtcNow
                |} 200
            with
            | ex ->
                return json {| error = ex.Message |} 500

        | "GET", "/status" ->
            // Return status information
            let status = {|
                status = "running"
                timestamp = DateTime.UtcNow
                models = [|
                    GZ302EA_R9641TB.FullName
                    GZ302EA_XS99.FullName
                |]
            |}

            return json status 200

        | "GET", "/history" | "GET", "/api/history" ->
            // Get price history for deals
            try
                // For now, return all deals with their current prices
                // Could be extended to query price_history table for specific URLs
                let! deals64GB = getDealsByModel env.DB "GZ302EA-R9641TB"
                let! deals128GB = getDealsByModel env.DB "GZ302EA-XS99"

                let result = {|
                    GZ302EA_R9641TB = deals64GB
                    GZ302EA_XS99 = deals128GB
                |}

                return json result 200
            with
            | ex ->
                return json {| error = ex.Message |} 500

        | "POST", "/trigger" | "POST", "/api/trigger" ->
            // Manual trigger
            ctx.waitUntil(runScheduledTask env |> Promise.map ignore)

            return json {| message = "Agent triggered"; status = "running" |} 202

        | _ ->
            // 404 for unknown routes
            let notFoundInfo = {|
                error = "Not Found"
                path = path
                availableRoutes = ["/"; "/api/deals"; "/api/analysis"; "/api/history"; "/status"; "/trigger"]
            |}

            return json notFoundInfo 404
    }

/// Export the handlers
[<ExportDefault>]
let handler: obj =
    {|
        fetch = fetch
        scheduled = handleScheduled
    |} :> obj

// Export the Durable Object classes
[<ExportNamed("NotificationManagerDO")>]
let notificationManagerDO = LaptopDealAgent.NotificationManager.NotificationManagerDO

[<ExportNamed("SearchActorDO")>]
let searchActorDO = LaptopDealAgent.SearchActor.SearchActorDO
