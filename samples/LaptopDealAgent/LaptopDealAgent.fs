module LaptopDealAgent.Main

open System
open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.Worker.Context
open CloudFlare.Worker.Context.Globals
open CloudFlare.Worker.Context.Helpers
open CloudFlare.Worker.Context.Helpers.ResponseBuilder
open LaptopDealAgent.Types
open LaptopDealAgent.AIAnalyzer
open LaptopDealAgent.SearchActor
open LaptopDealAgent.SearchOrchestrator
open LaptopDealAgent.SearchAgent
open LaptopDealAgent.PriceAnalyzer

/// Environment bindings
[<AllowNullLiteral>]
type LaptopAgentEnv =
    inherit Env
    abstract PRICE_HISTORY: obj
    abstract AI: obj
    abstract SEARCH_ACTOR: obj

/// Store current deals in KV for API access
let storeCurrentDeals (kv: obj) (deals: PriceInfo list) : JS.Promise<unit> =
    promise {
        try
            let dealsData = {|
                deals = deals
                lastUpdated = DateTime.UtcNow
                totalDeals = deals.Length
            |}

            let json = JS.JSON.stringify(dealsData)
            do! kv?put("current_deals", json) |> unbox<JS.Promise<unit>>

            printfn "Stored %d current deals" deals.Length
        with
        | ex ->
            printfn "Error storing current deals: %s" ex.Message
    }

/// Store current analyses in KV for API access
let storeCurrentAnalyses (kv: obj) (analyses: DealAnalysis list) : JS.Promise<unit> =
    promise {
        try
            let analysisData = {|
                analyses = analyses
                generatedAt = DateTime.UtcNow
            |}

            let json = JS.JSON.stringify(analysisData)
            do! kv?put("current_analyses", json) |> unbox<JS.Promise<unit>>

            printfn "Stored %d analyses" analyses.Length
        with
        | ex ->
            printfn "Error storing analyses: %s" ex.Message
    }

/// Run the scheduled agent task
let runScheduledTask (env: LaptopAgentEnv) : JS.Promise<string> =
    promise {
        printfn "🤖 Starting laptop deal agent..."

        try
            // Execute parallel AI-powered searches using actor system
            printfn "🔍 Orchestrating parallel AI searches with idempotency..."
            let! priceInfos = orchestrateParallelSearches env

            printfn $"✅ Found {priceInfos.Length} valid deals from parallel actors"

            // Store price information
            printfn "💾 Storing price history..."
            do!
                priceInfos
                |> List.map (storePriceHistory env.PRICE_HISTORY)
                |> Promise.all
                |> Promise.map ignore

            // Store current deals for API access
            do! storeCurrentDeals env.PRICE_HISTORY priceInfos

            // Analyze prices for each model
            printfn "📊 Analyzing prices..."
            let! analyses =
                [ GZ302EA_R9641TB; GZ302EA_XS99 ]
                |> List.map (analyzePrices env.PRICE_HISTORY priceInfos)
                |> Promise.all

            let analysisList = analyses |> Array.toList

            // Store current analyses for API access
            do! storeCurrentAnalyses env.PRICE_HISTORY analysisList

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
            // Get current deals
            try
                let! dealsJson = env.PRICE_HISTORY?get("current_deals") |> unbox<JS.Promise<string>>

                if isNull dealsJson || dealsJson = "" then
                    return json {|
                        deals = []
                        lastUpdated = DateTime.UtcNow
                        totalDeals = 0
                    |} 200
                else
                    let dealsData = JS.JSON.parse(dealsJson)
                    return Response.json(dealsData)
            with
            | ex ->
                return json {| error = ex.Message |} 500

        | "GET", "/api/analysis" ->
            // Get current analyses
            try
                let! analysisJson = env.PRICE_HISTORY?get("current_analyses") |> unbox<JS.Promise<string>>

                if isNull analysisJson || analysisJson = "" then
                    return json {|
                        analyses = []
                        generatedAt = DateTime.UtcNow
                    |} 200
                else
                    let analysisData = JS.JSON.parse(analysisJson)
                    return Response.json(analysisData)
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
            // Get price history for a specific model
            try
                let! history64GB = retrievePriceHistory env.PRICE_HISTORY GZ302EA_R9641TB
                let! history128GB = retrievePriceHistory env.PRICE_HISTORY GZ302EA_XS99

                let result = {|
                    GZ302EA_R9641TB = history64GB |> List.take (min 10 history64GB.Length)
                    GZ302EA_XS99 = history128GB |> List.take (min 10 history128GB.Length)
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
