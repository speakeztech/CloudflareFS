module LaptopDealAgent.NotificationManager

open System
open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.DurableObjects
open LaptopDealAgent.Types
open LaptopDealAgent.NotificationTypes
open LaptopDealAgent.NotificationChannels

/// Durable Object that manages notification state and prevents spam
[<AllowNullLiteral>]
type NotificationManagerDO(state: DurableObjectState, env: obj) =
    inherit DurableObject(state, env)

    let mutable notificationState = defaultNotificationState

    /// Initialize state from storage
    member this.InitializeAsync() : JS.Promise<unit> =
        promise {
            try
                let! storedState = state.storage.get("state") |> unbox<JS.Promise<obj>>

                if not (isNull storedState) then
                    notificationState <- storedState |> unbox<NotificationState>
                    printfn "Loaded notification state: %d notifications in history" notificationState.History.Length
                else
                    printfn "Initialized new notification state"
            with
            | ex ->
                printfn "Error loading state: %s" ex.Message
        }

    /// Save state to storage
    member this.SaveStateAsync() : JS.Promise<unit> =
        promise {
            do! state.storage.put("state", notificationState) |> unbox<JS.Promise<unit>>
        }

    /// Reset daily counter if it's a new day
    member this.ResetDailyCounterIfNeeded() =
        let today = DateTime.UtcNow.Date

        if notificationState.LastResetDate < today then
            notificationState <- {
                notificationState with
                    NotificationsToday = 0
                    LastResetDate = today
            }
            printfn "Daily notification counter reset"

    /// Check if we should notify based on rules
    member this.ShouldNotify(event: NotificationEvent, rules: NotificationRule) : bool =
        // Reset daily counter if needed
        this.ResetDailyCounterIfNeeded()

        // Check daily limit
        if notificationState.NotificationsToday >= rules.MaxNotificationsPerDay then
            printfn "Daily notification limit reached (%d/%d)" notificationState.NotificationsToday rules.MaxNotificationsPerDay
            false

        // Check minimum time between notifications
        elif notificationState.LastNotificationTime.IsSome then
            let timeSinceLastNotification = DateTime.UtcNow - notificationState.LastNotificationTime.Value
            let minHours = TimeSpan.FromHours(rules.MinHoursBetweenNotifications)

            if timeSinceLastNotification < minHours then
                printfn "Too soon since last notification (%.1f hours < %.1f hours required)"
                    timeSinceLastNotification.TotalHours
                    rules.MinHoursBetweenNotifications
                false
            else
                true
        else
            true

    /// Check if this specific deal has already been notified
    member this.IsAlreadyNotified(event: NotificationEvent) : bool =
        // Check if we've notified about this exact deal recently (within 24 hours)
        let recentThreshold = DateTime.UtcNow.AddHours(-24.0)

        notificationState.History
        |> List.exists (fun h ->
            h.Model = event.Model &&
            h.Retailer = event.Retailer &&
            h.Price >= event.Price * 0.99M &&  // Allow 1% variance
            h.Price <= event.Price * 1.01M &&
            h.NotifiedAt > recentThreshold
        )

    /// Check if deal meets notification rules
    member this.MeetsRules(event: NotificationEvent, rules: NotificationRule) : bool =
        // Check model filter
        let modelMatch =
            rules.ModelsToWatch.IsEmpty ||
            rules.ModelsToWatch |> List.contains event.Model

        // Check retailer filter
        let retailerMatch =
            rules.RetailersToWatch.IsEmpty ||
            rules.RetailersToWatch |> List.exists (fun r ->
                event.Retailer.ToLowerInvariant().Contains(r.ToLowerInvariant())
            )

        // Check Black Friday filter
        let blackFridayMatch =
            not rules.OnlyBlackFriday || event.IsBlackFridayDeal

        // Check historical low filter
        let historicalLowMatch =
            not rules.OnlyHistoricalLow ||
            (event.PreviousLowestPrice.IsSome && event.Price <= event.PreviousLowestPrice.Value)

        // Check price drop percentage
        let priceDropMatch =
            match rules.MinPriceDropPercent, event.PreviousLowestPrice with
            | Some minDrop, Some prevPrice ->
                let actualDrop = ((prevPrice - event.Price) / prevPrice) * 100M
                actualDrop >= decimal minDrop
            | None, _ -> true
            | Some _, None -> true  // No previous price, allow it

        modelMatch && retailerMatch && blackFridayMatch && historicalLowMatch && priceDropMatch

    /// Record notification in history
    member this.RecordNotification(event: NotificationEvent, channel: string) =
        let entry = {
            EventId = event.EventId
            Model = event.Model
            Retailer = event.Retailer
            Price = event.Price
            NotifiedAt = DateTime.UtcNow
            Channel = channel
        }

        // Keep only last 100 notifications
        let history =
            entry :: notificationState.History
            |> List.take (min 100 (notificationState.History.Length + 1))

        notificationState <- {
            notificationState with
                History = history
                LastNotificationTime = Some DateTime.UtcNow
                NotificationsToday = notificationState.NotificationsToday + 1
        }

    /// Process notification event
    member this.ProcessNotification(event: NotificationEvent, rules: NotificationRule, channels: NotificationChannel list, dashboardUrl: string option) : JS.Promise<bool> =
        promise {
            try
                do! this.InitializeAsync()

                printfn "Processing notification for %s at $%.2f" event.Model.ModelNumber event.Price

                // Check if already notified
                if this.IsAlreadyNotified(event) then
                    printfn "Already notified about this deal recently"
                    return false

                // Check rules
                elif not (this.MeetsRules event rules) then
                    printfn "Deal doesn't meet notification rules"
                    return false

                // Check rate limiting
                elif not (this.ShouldNotify event rules) then
                    printfn "Rate limited or daily limit reached"
                    return false

                else
                    // Send notifications to all channels
                    let message = createDealNotification event dashboardUrl

                    printfn "Sending notification: %s" message.Title

                    let! results =
                        channels
                        |> List.map (fun channel ->
                            promise {
                                let! success = sendNotification channel message

                                if success then
                                    this.RecordNotification(event, channel.ToString())

                                return success
                            }
                        )
                        |> Promise.all

                    let anySuccess = results |> Array.exists id

                    if anySuccess then
                        do! this.SaveStateAsync()
                        printfn "✓ Notification sent successfully"
                        return true
                    else
                        printfn "✗ All notification channels failed"
                        return false

            with
            | ex ->
                printfn "Error processing notification: %s" ex.Message
                return false
        }

    /// Handle HTTP requests to the Durable Object
    override this.fetch(request: obj) : JS.Promise<obj> =
        promise {
            try
                let req = request |> unbox<CloudFlare.Worker.Context.Request>
                let url = req.url
                let method = req.method

                match method with
                | "POST" ->
                    // Process notification request
                    let! body = req.json() |> unbox<JS.Promise<obj>>

                    let event = body?event |> unbox<NotificationEvent>
                    let rules = body?rules |> unbox<NotificationRule>
                    let channels = body?channels |> unbox<NotificationChannel list>
                    let dashboardUrl = body?dashboardUrl |> Option.ofObj |> Option.map unbox<string>

                    let! success = this.ProcessNotification(event, rules, channels, dashboardUrl)

                    let response = createObj [
                        "success" ==> success
                        "state" ==> notificationState
                    ]

                    return CloudFlare.Worker.Context.Response.json(response) :> obj

                | "GET" ->
                    // Get current state
                    do! this.InitializeAsync()

                    let response = createObj [
                        "state" ==> notificationState
                    ]

                    return CloudFlare.Worker.Context.Response.json(response) :> obj

                | "DELETE" ->
                    // Clear history
                    notificationState <- defaultNotificationState
                    do! this.SaveStateAsync()

                    return CloudFlare.Worker.Context.Response.json({| message = "History cleared" |}) :> obj

                | _ ->
                    return CloudFlare.Worker.Context.Response.create("Method not allowed", 405.0) :> obj

            with
            | ex ->
                printfn "Error in Durable Object fetch: %s" ex.Message
                return CloudFlare.Worker.Context.Response.create($"Error: {ex.Message}", 500.0) :> obj
        }

/// Helper to get Durable Object instance
let getNotificationManager (env: obj) (userId: string) : obj =
    let namespace = env?NOTIFICATION_MANAGER

    // Generate stable ID based on user ID
    let id = namespace?idFromName(userId)

    // Get the Durable Object stub
    namespace?get(id)

/// Send notification via Durable Object
let notifyDeal (env: obj) (userId: string) (event: NotificationEvent) (rules: NotificationRule) (channels: NotificationChannel list) (dashboardUrl: string option) : JS.Promise<bool> =
    promise {
        try
            let manager = getNotificationManager env userId

            let payload = createObj [
                "event" ==> event
                "rules" ==> rules
                "channels" ==> channels
                "dashboardUrl" ==> (dashboardUrl |> Option.toObj)
            ]

            let requestInit = jsOptions(fun o ->
                o.method <- Some "POST"
                o.headers <- Some (U2.Case1 (createObj [
                    "Content-Type" ==> "application/json"
                ]))
                o.body <- Some (U2.Case1 (JS.JSON.stringify(payload)))
            )

            let! response = manager?fetch("https://fake-host/notify", requestInit) |> unbox<JS.Promise<obj>>

            let! result = response?json() |> unbox<JS.Promise<obj>>
            let success = result?success |> unbox<bool>

            return success

        with
        | ex ->
            printfn "Error calling Durable Object: %s" ex.Message
            return false
    }
