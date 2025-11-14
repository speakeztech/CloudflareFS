module LaptopDealAgent.NotificationChannels

open System
open Fable.Core
open Fable.Core.JsInterop
open LaptopDealAgent.NotificationTypes

/// Send notification via Pushover (recommended for phone notifications)
/// Pushover provides iOS/Android apps with reliable push notifications
/// Sign up at: https://pushover.net/
let sendPushover (apiToken: string) (userKey: string) (message: NotificationMessage) : JS.Promise<bool> =
    promise {
        try
            let priority =
                match message.Priority with
                | Low -> 0
                | Normal -> 0
                | High -> 1
                | Emergency -> 2  // Requires confirmation

            let payload = createObj [
                "token" ==> apiToken
                "user" ==> userKey
                "message" ==> message.Body
                "title" ==> message.Title
                "priority" ==> priority
                "url" ==> (message.Url |> Option.defaultValue "")
                "url_title" ==> "View Deal"
                "sound" ==> (if message.Priority = Emergency then "cashregister" else "pushover")
            ]

            let! response =
                Fetch.fetch("https://api.pushover.net/1/messages.json", jsOptions(fun o ->
                    o.method <- Some "POST"
                    o.headers <- Some (U2.Case1 (createObj [
                        "Content-Type" ==> "application/json"
                    ]))
                    o.body <- Some (U2.Case1 (JS.JSON.stringify(payload)))
                ))

            let! json = response.json() |> unbox<JS.Promise<obj>>
            let status = json?status |> unbox<int>

            if status = 1 then
                printfn "✓ Pushover notification sent successfully"
                return true
            else
                printfn "✗ Pushover notification failed: %A" json
                return false

        with
        | ex ->
            printfn "✗ Pushover error: %s" ex.Message
            return false
    }

/// Send notification via Telegram Bot
/// Create a bot: https://t.me/botfather
/// Get your chat ID: https://api.telegram.org/bot<TOKEN>/getUpdates
let sendTelegram (botToken: string) (chatId: string) (message: NotificationMessage) : JS.Promise<bool> =
    promise {
        try
            let emoji =
                match message.Priority with
                | Emergency -> "🔥🎉"
                | High -> "🎯"
                | Normal -> "💰"
                | Low -> "ℹ️"

            let formattedMessage =
                $"{emoji} *{message.Title}*\n\n{message.Body}"

            let url = $"https://api.telegram.org/bot{botToken}/sendMessage"

            let payload = createObj [
                "chat_id" ==> chatId
                "text" ==> formattedMessage
                "parse_mode" ==> "Markdown"
                "disable_web_page_preview" ==> false
            ]

            let! response =
                Fetch.fetch(url, jsOptions(fun o ->
                    o.method <- Some "POST"
                    o.headers <- Some (U2.Case1 (createObj [
                        "Content-Type" ==> "application/json"
                    ]))
                    o.body <- Some (U2.Case1 (JS.JSON.stringify(payload)))
                ))

            let! json = response.json() |> unbox<JS.Promise<obj>>
            let ok = json?ok |> unbox<bool>

            if ok then
                printfn "✓ Telegram notification sent successfully"
                return true
            else
                printfn "✗ Telegram notification failed: %A" json
                return false

        with
        | ex ->
            printfn "✗ Telegram error: %s" ex.Message
            return false
    }

/// Send notification via Discord webhook
/// Create webhook in Discord server settings → Integrations → Webhooks
let sendDiscord (webhookUrl: string) (message: NotificationMessage) : JS.Promise<bool> =
    promise {
        try
            let color =
                match message.Priority with
                | Emergency -> 0xFF0000  // Red
                | High -> 0xFFA500       // Orange
                | Normal -> 0x00FF00     // Green
                | Low -> 0x0000FF        // Blue

            let embed = createObj [
                "title" ==> message.Title
                "description" ==> message.Body
                "color" ==> color
                "timestamp" ==> DateTime.UtcNow.ToString("o")
                "url" ==> (message.Url |> Option.defaultValue "")
            ]

            let payload = createObj [
                "embeds" ==> [| embed |]
            ]

            let! response =
                Fetch.fetch(webhookUrl, jsOptions(fun o ->
                    o.method <- Some "POST"
                    o.headers <- Some (U2.Case1 (createObj [
                        "Content-Type" ==> "application/json"
                    ]))
                    o.body <- Some (U2.Case1 (JS.JSON.stringify(payload)))
                ))

            if response.ok then
                printfn "✓ Discord notification sent successfully"
                return true
            else
                printfn "✗ Discord notification failed: %d" response.status
                return false

        with
        | ex ->
            printfn "✗ Discord error: %s" ex.Message
            return false
    }

/// Send notification via Twilio SMS
/// Sign up at: https://www.twilio.com/
let sendTwilioSMS (accountSid: string) (authToken: string) (fromNumber: string) (toNumber: string) (message: NotificationMessage) : JS.Promise<bool> =
    promise {
        try
            let smsBody = $"{message.Title}\n\n{message.Body}"

            let url = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json"

            let formData =
                [
                    "From", fromNumber
                    "To", toNumber
                    "Body", smsBody
                ]
                |> List.map (fun (k, v) -> $"{k}={JS.encodeURIComponent(v)}")
                |> String.concat "&"

            let authHeader =
                let credentials = $"{accountSid}:{authToken}"
                let encoded = JS.btoa(credentials)
                $"Basic {encoded}"

            let! response =
                Fetch.fetch(url, jsOptions(fun o ->
                    o.method <- Some "POST"
                    o.headers <- Some (U2.Case1 (createObj [
                        "Content-Type" ==> "application/x-www-form-urlencoded"
                        "Authorization" ==> authHeader
                    ]))
                    o.body <- Some (U2.Case1 formData)
                ))

            if response.ok then
                printfn "✓ Twilio SMS sent successfully"
                return true
            else
                printfn "✗ Twilio SMS failed: %d" response.status
                return false

        with
        | ex ->
            printfn "✗ Twilio error: %s" ex.Message
            return false
    }

/// Send notification to any channel
let sendNotification (channel: NotificationChannel) (message: NotificationMessage) : JS.Promise<bool> =
    match channel with
    | Pushover (apiToken, userKey) ->
        sendPushover apiToken userKey message

    | Telegram (botToken, chatId) ->
        sendTelegram botToken chatId message

    | Discord webhookUrl ->
        sendDiscord webhookUrl message

    | Twilio (accountSid, authToken, fromNumber, toNumber) ->
        sendTwilioSMS accountSid authToken fromNumber toNumber message

    | Email (apiKey, toEmail) ->
        // Could integrate with SendGrid, Mailgun, etc.
        promise {
            printfn "Email notifications not yet implemented"
            return false
        }

    | Slack webhookUrl ->
        // Similar to Discord webhook
        promise {
            printfn "Slack notifications not yet implemented"
            return false
        }

/// Create a notification message from a deal
let createDealNotification (event: NotificationEvent) : NotificationMessage =
    let priceDropInfo =
        match event.PreviousLowestPrice with
        | Some prevPrice ->
            let savings = prevPrice - event.Price
            let percentSavings = (savings / prevPrice) * 100M
            $"Price dropped ${savings:F2} ({percentSavings:F1}%) from ${prevPrice:F2}"
        | None ->
            "New deal found!"

    let dealType =
        if event.IsBlackFridayDeal then " 🔥 BLACK FRIDAY DEAL" else ""

    let title = $"{event.Model.ModelNumber} - ${event.Price:F2}{dealType}"

    let body =
        $"""{event.Model.FullName}

💰 Price: ${event.Price:F2}
🏪 Retailer: {event.Retailer}
📊 {priceDropInfo}

{event.Recommendation}

Act fast - deals can expire quickly!"""

    {
        Title = title
        Body = body
        Url = Some event.Url
        Priority = event.Priority
    }
