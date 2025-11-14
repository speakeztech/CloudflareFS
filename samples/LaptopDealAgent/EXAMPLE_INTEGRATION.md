# Example: Integrating Notifications into LaptopDealAgent

This document shows how to integrate the notification system into your main `LaptopDealAgent.fs`.

## Complete Integration Example

```fsharp
module LaptopDealAgent.Main

open System
open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.Worker.Context
open CloudFlare.Worker.Context.Globals
open CloudFlare.Worker.Context.Helpers
open CloudFlare.Worker.Context.Helpers.ResponseBuilder
open LaptopDealAgent.Types
open LaptopDealAgent.SearchAgent
open LaptopDealAgent.PriceAnalyzer
open LaptopDealAgent.NotificationTypes
open LaptopDealAgent.NotificationChannels
open LaptopDealAgent.NotificationManager

/// Environment bindings with notification secrets
[<AllowNullLiteral>]
type LaptopAgentEnv =
    inherit Env
    abstract PRICE_HISTORY: obj
    abstract AI: obj
    abstract NOTIFICATION_MANAGER: obj

    // Notification credentials (from secrets)
    abstract PUSHOVER_API_TOKEN: string
    abstract PUSHOVER_USER_KEY: string
    abstract TELEGRAM_BOT_TOKEN: string
    abstract TELEGRAM_CHAT_ID: string
    abstract DISCORD_WEBHOOK_URL: string

/// Get notification channels from environment
let getNotificationChannels (env: LaptopAgentEnv) : NotificationChannel list =
    [
        // Pushover (recommended)
        if not (String.IsNullOrEmpty env.PUSHOVER_API_TOKEN) then
            yield Pushover(env.PUSHOVER_API_TOKEN, env.PUSHOVER_USER_KEY)

        // Telegram
        if not (String.IsNullOrEmpty env.TELEGRAM_BOT_TOKEN) then
            yield Telegram(env.TELEGRAM_BOT_TOKEN, env.TELEGRAM_CHAT_ID)

        // Discord
        if not (String.IsNullOrEmpty env.DISCORD_WEBHOOK_URL) then
            yield Discord(env.DISCORD_WEBHOOK_URL)
    ]

/// Check if a deal is notification-worthy and send notification
let checkAndNotifyDeal
    (env: LaptopAgentEnv)
    (analysis: DealAnalysis)
    (currentPrices: PriceInfo list)
    : JS.Promise<unit> =
    promise {
        match analysis.CurrentBestPrice, analysis.LowestHistoricalPrice with
        | Some currentPrice, lowestPrice ->
            // Determine notification priority
            let priority =
                match lowestPrice with
                | Some low when currentPrice <= low ->
                    Emergency  // At or below historical low!
                | Some low when currentPrice <= low * 1.05M ->
                    High  // Within 5% of historical low
                | Some low when currentPrice <= low * 1.15M ->
                    Normal  // Within 15% of historical low
                | _ ->
                    Low  // Just informational

            // Only notify for Normal priority or higher
            if priority.ToInt() >= Normal.ToInt() then
                // Find the specific price info
                let priceInfo =
                    currentPrices
                    |> List.tryFind (fun p ->
                        p.Model = analysis.Model &&
                        p.Price = Some currentPrice
                    )

                match priceInfo with
                | Some info ->
                    // Create notification event
                    let event = {
                        EventId = Guid.NewGuid().ToString()
                        Model = analysis.Model
                        Price = currentPrice
                        PreviousLowestPrice = lowestPrice
                        Retailer = info.Retailer
                        Url = info.Url
                        IsBlackFridayDeal = info.IsBlackFridayDeal
                        DiscountPercentage = info.DiscountPercentage
                        Priority = priority
                        Timestamp = DateTime.UtcNow
                        Recommendation = analysis.Recommendation
                    }

                    // Get notification rules (conservative by default)
                    let rules = conservativeNotificationRules

                    // Get notification channels
                    let channels = getNotificationChannels env

                    if channels.IsEmpty then
                        printfn "⚠️  No notification channels configured"
                    else
                        // Send notification via Durable Object
                        // Dashboard URL will be the link in the notification
                        let dashboardUrl = Some "https://your-dashboard.pages.dev"

                        printfn "📱 Attempting to send notification..."
                        let! notified = notifyDeal
                            env
                            "default-user"  // Could be configurable
                            event
                            rules
                            channels
                            dashboardUrl  // Link to React dashboard

                        if notified then
                            printfn "✓ Deal notification sent successfully!"
                        else
                            printfn "ℹ️  Notification not sent (rate limited, duplicate, or filtered)"

                | None ->
                    printfn "No price info found for notification"

        | _ ->
            printfn "No current price available for notification"
    }

/// Run the scheduled agent task with notifications
let runScheduledTask (env: LaptopAgentEnv) : JS.Promise<string> =
    promise {
        printfn "🤖 Starting laptop deal agent..."

        try
            // Configuration
            let config = {
                SearchKeywords = [
                    "ASUS ROG Flow Z13 2025 GZ302EA-R9641TB 64GB Black Friday"
                    "ASUS ROG Flow Z13 2025 GZ302EA-XS99 128GB Black Friday"
                    "ROG Flow Z13 GZ302 AMD Ryzen AI Max+ 395 price"
                    "ASUS ROG Flow Z13 2025 Black Friday deal"
                ]
                MaxSearchResults = 20
                MinPriceConfidence = 0.7
                EnableNotifications = true
            }

            // Search for deals
            printfn "🔍 Searching for laptop deals..."
            let! priceInfos = searchForDeals env.AI config

            printfn $"Found {priceInfos.Length} price listings"

            // Store price information
            printfn "💾 Storing price history..."
            do!
                priceInfos
                |> List.map (storePriceHistory env.PRICE_HISTORY)
                |> Promise.all
                |> Promise.map ignore

            // Analyze prices for each model
            printfn "📊 Analyzing prices..."
            let! analyses =
                [ GZ302EA_R9641TB; GZ302EA_XS99 ]
                |> List.map (analyzePrices env.PRICE_HISTORY priceInfos)
                |> Promise.all

            let analysisList = analyses |> Array.toList

            // Check for notification-worthy deals
            printfn "📱 Checking for deals to notify..."
            do!
                analysisList
                |> List.map (fun analysis ->
                    checkAndNotifyDeal env analysis priceInfos
                )
                |> Promise.all
                |> Promise.map ignore

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

/// Handle test notification endpoint
let handleTestNotification (env: LaptopAgentEnv) : JS.Promise<Response> =
    promise {
        try
            // Create a test notification event
            let testEvent = {
                EventId = Guid.NewGuid().ToString()
                Model = GZ302EA_R9641TB
                Price = 1299.99M
                PreviousLowestPrice = Some 1599.99M
                Retailer = "Best Buy"
                Url = "https://www.bestbuy.com/test"
                IsBlackFridayDeal = true
                DiscountPercentage = Some 18.75
                Priority = High
                Timestamp = DateTime.UtcNow
                Recommendation = "Great deal! This is a test notification."
            }

            let channels = getNotificationChannels env

            if channels.IsEmpty then
                return json {|
                    success = false
                    error = "No notification channels configured"
                |} 400
            else
                // Send using Durable Object
                let! sent = notifyDeal
                    env
                    "default-user"
                    testEvent
                    aggressiveNotificationRules  // Use aggressive for testing
                    channels

                return json {|
                    success = sent
                    channels = channels.Length
                    message = if sent then "Test notification sent" else "Notification filtered or rate limited"
                |} 200

        with
        | ex ->
            return json {| error = ex.Message |} 500
    }

/// Handle scheduled execution
let handleScheduled (event: obj) (env: LaptopAgentEnv) (ctx: ExecutionContext) =
    promise {
        printfn "⏰ Scheduled event triggered"
        let! result = runScheduledTask env
        printfn "📋 Result: %s" (if result.Contains("Error") then "Failed" else "Success")
    }

/// Handle HTTP requests
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

        | "GET", "/status" ->
            // Return status information
            let channels = getNotificationChannels env
            let status = {|
                status = "running"
                timestamp = DateTime.UtcNow
                models = [|
                    GZ302EA_R9641TB.FullName
                    GZ302EA_XS99.FullName
                |]
                notificationChannels = channels.Length
                channelTypes = channels |> List.map (fun c -> c.ToString())
            |}

            return json status 200

        | "POST", "/test-notification" ->
            // Test notification system
            return! handleTestNotification env

        | "GET", "/notification-history" ->
            // Get notification history from Durable Object
            try
                let manager = getNotificationManager env "default-user"
                let! response = manager?fetch("https://fake-host/") |> unbox<JS.Promise<obj>>
                let! result = response?json() |> unbox<JS.Promise<obj>>

                return json result 200
            with
            | ex ->
                return json {| error = ex.Message |} 500

        | "DELETE", "/notification-history" ->
            // Clear notification history
            try
                let manager = getNotificationManager env "default-user"

                let requestInit = jsOptions(fun o ->
                    o.method <- Some "DELETE"
                )

                let! response = manager?fetch("https://fake-host/", requestInit) |> unbox<JS.Promise<obj>>
                let! result = response?json() |> unbox<JS.Promise<obj>>

                return json result 200
            with
            | ex ->
                return json {| error = ex.Message |} 500

        | "GET", "/history" ->
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

        | "POST", "/trigger" ->
            // Manual trigger
            ctx.waitUntil(runScheduledTask env |> Promise.map ignore)

            return json {| message = "Agent triggered"; status = "running" |} 202

        | _ ->
            // 404 for unknown routes
            let notFoundInfo = {|
                error = "Not Found"
                path = path
                availableRoutes = [
                    "/"
                    "/status"
                    "/history"
                    "/trigger"
                    "/test-notification"
                    "/notification-history"
                ]
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

// Export the Durable Object class
[<ExportNamed("NotificationManagerDO")>]
let notificationManagerDO = NotificationManagerDO
```

## Quick Start

1. **Set up notification channel** (Pushover recommended):
   ```bash
   wrangler secret put PUSHOVER_API_TOKEN
   wrangler secret put PUSHOVER_USER_KEY
   ```

2. **Deploy**:
   ```bash
   npm run build
   npm run deploy
   ```

3. **Test**:
   ```bash
   curl -X POST https://your-worker.workers.dev/test-notification
   ```

   You should receive a notification on your phone!

4. **Check history**:
   ```bash
   curl https://your-worker.workers.dev/notification-history
   ```

## Customizing Notification Rules

Edit the rules in `checkAndNotifyDeal`:

```fsharp
// Only notify for historical lows
let rules = {
    MinPriceDropPercent = Some 15.0
    OnlyHistoricalLow = true
    OnlyBlackFriday = false
    MaxNotificationsPerDay = 3
    MinHoursBetweenNotifications = 6.0
    ModelsToWatch = []
    RetailersToWatch = []
}

// Or use predefined rules
let rules = conservativeNotificationRules
let rules = aggressiveNotificationRules
```

## Monitoring

View real-time logs:
```bash
wrangler tail laptop-deal-agent
```

Look for:
- ✓ `Deal notification sent successfully!`
- ℹ️ `Notification not sent (rate limited...)`
- ⚠️ `No notification channels configured`

---

That's it! Your laptop deal agent will now send you notifications whenever it finds a great deal.
