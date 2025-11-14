# Notification System Design

This document explains the notification architecture for the Laptop Deal Agent.

## Architecture Overview

The notification system uses a **Durable Object singleton pattern** to manage notifications intelligently:

```
┌─────────────────────────────────────────────────────────────┐
│                   LaptopDealAgent (Cron)                     │
│                   Runs every hour                            │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ Finds deals
                     ▼
┌─────────────────────────────────────────────────────────────┐
│          NotificationManager (Durable Object)                │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │ Stateful Singleton (one per user)                  │    │
│  │                                                     │    │
│  │ • Deduplication (same deal not notified twice)     │    │
│  │ • Rate limiting (max N per day, min hours between) │    │
│  │ • Rule evaluation (price thresholds, BF only, etc) │    │
│  │ • History tracking (what's been sent)              │    │
│  │ • Multi-channel routing                            │    │
│  └────────────────────────────────────────────────────┘    │
└────────────────────┬────────────────────────────────────────┘
                     │
                     │ Sends to channels
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                   Notification Channels                      │
│                                                              │
│  • Pushover (iOS/Android push notifications) ← RECOMMENDED  │
│  • Telegram (Bot messages)                                  │
│  • Discord (Webhook)                                        │
│  • Twilio (SMS)                                             │
│  • Email (future)                                           │
│  • Slack (future)                                           │
└─────────────────────────────────────────────────────────────┘
```

## Why Durable Objects?

**Durable Objects** are perfect for this because they:

1. **Maintain State** - Remember what you've already been notified about
2. **Singleton Pattern** - One instance per user, preventing race conditions
3. **Persistent Storage** - State survives across invocations
4. **Low Latency** - Colocated with your worker for fast access
5. **Transactional** - Atomic operations on state

### Alternative: Cloudflare Queues

For simpler use cases without complex state management:
```
Agent → Queue → Consumer Worker → Send Notification
```

This works but:
- ❌ No deduplication (could send same deal multiple times)
- ❌ No rate limiting
- ❌ No history tracking
- ✅ Simple to set up
- ✅ Good for fire-and-forget notifications

## Recommended Channel: Pushover

**Pushover** is the best option for phone notifications because:

- ✅ **Reliable push notifications** to iOS and Android
- ✅ **One-time purchase** ($5 per platform, no monthly fees)
- ✅ **Priority levels** (Low, Normal, High, Emergency with sounds)
- ✅ **Rich notifications** with URLs, titles, custom sounds
- ✅ **No rate limits** for reasonable use
- ✅ **Simple API** - just HTTP POST
- ✅ **Offline queuing** - delivers when phone comes online

**Setup:**
1. Go to [https://pushover.net/](https://pushover.net/)
2. Create account ($5 one-time for iOS or Android)
3. Note your **User Key**
4. Create an application, note the **API Token**
5. Install Pushover app on your phone
6. Done! Notifications will appear instantly.

## Configuration

### 1. Update `wrangler.toml`

Add Durable Object binding:

```toml
[[durable_objects.bindings]]
name = "NOTIFICATION_MANAGER"
class_name = "NotificationManagerDO"
script_name = "laptop-deal-agent"

[[migrations]]
tag = "v1"
new_classes = ["NotificationManagerDO"]

# Add your notification credentials as secrets
# Don't put them in wrangler.toml directly!
```

### 2. Set Secrets

Store credentials securely:

```bash
# Pushover (recommended)
wrangler secret put PUSHOVER_API_TOKEN
wrangler secret put PUSHOVER_USER_KEY

# Or Telegram
wrangler secret put TELEGRAM_BOT_TOKEN
wrangler secret put TELEGRAM_CHAT_ID

# Or Discord
wrangler secret put DISCORD_WEBHOOK_URL

# Or Twilio
wrangler secret put TWILIO_ACCOUNT_SID
wrangler secret put TWILIO_AUTH_TOKEN
wrangler secret put TWILIO_FROM_NUMBER
wrangler secret put TWILIO_TO_NUMBER
```

### 3. Configure Notification Rules

In your main agent, set up rules:

```fsharp
// Conservative (only best deals)
let notificationRules = {
    MinPriceDropPercent = Some 15.0      // 15% off minimum
    OnlyHistoricalLow = true             // Only if at/below historical low
    OnlyBlackFriday = false              // Any deal
    MaxNotificationsPerDay = 3           // Max 3 notifications per day
    MinHoursBetweenNotifications = 6.0   // 6 hours between notifications
    ModelsToWatch = []                   // All models
    RetailersToWatch = []                // All retailers
}

// Aggressive (all good deals)
let notificationRules = {
    MinPriceDropPercent = Some 5.0       // 5% off minimum
    OnlyHistoricalLow = false
    OnlyBlackFriday = false
    MaxNotificationsPerDay = 10
    MinHoursBetweenNotifications = 1.0
    ModelsToWatch = []
    RetailersToWatch = []
}

// Black Friday only
let notificationRules = {
    MinPriceDropPercent = None
    OnlyHistoricalLow = false
    OnlyBlackFriday = true               // Only Black Friday deals
    MaxNotificationsPerDay = 20
    MinHoursBetweenNotifications = 0.5
    ModelsToWatch = []
    RetailersToWatch = []
}
```

### 4. Set Up Notification Channels

```fsharp
let channels = [
    // Pushover (recommended)
    Pushover(
        apiToken = env.PUSHOVER_API_TOKEN,
        userKey = env.PUSHOVER_USER_KEY
    )

    // Or Telegram
    Telegram(
        botToken = env.TELEGRAM_BOT_TOKEN,
        chatId = env.TELEGRAM_CHAT_ID
    )

    // Or Discord
    Discord(webhookUrl = env.DISCORD_WEBHOOK_URL)

    // Can use multiple!
]
```

## Usage in Your Agent

Update `LaptopDealAgent.fs` to integrate notifications:

```fsharp
// After analyzing prices, check for deals worth notifying
let notifyIfDealFound (env: LaptopAgentEnv) (analysis: DealAnalysis) =
    promise {
        // Determine if this is a notification-worthy deal
        match analysis.CurrentBestPrice, analysis.LowestHistoricalPrice with
        | Some currentPrice, Some historicalLow when currentPrice <= historicalLow ->
            // Create notification event
            let event = {
                EventId = Guid.NewGuid().ToString()
                Model = analysis.Model
                Price = currentPrice
                PreviousLowestPrice = Some historicalLow
                Retailer = analysis.BestRetailer |> Option.defaultValue "Unknown"
                Url = "https://example.com"  // Get from price info
                IsBlackFridayDeal = true  // Determine from search
                DiscountPercentage = None
                Priority = Emergency  // Historical low!
                Timestamp = DateTime.UtcNow
                Recommendation = analysis.Recommendation
            }

            // Send notification via Durable Object
            let! notified = notifyDeal
                env
                "user123"  // Your user ID
                event
                conservativeNotificationRules
                channels

            if notified then
                printfn "✓ Deal notification sent!"
            else
                printfn "✗ Notification not sent (rate limited or duplicate)"

        | _ -> ()
    }
```

## Notification Priority Levels

The system supports 4 priority levels:

1. **Low** - Good deal, informational
   - Price drop 5-10%
   - In stock at regular price

2. **Normal** - Notable price drop
   - Price drop 10-15%
   - Good deal from trusted retailer

3. **High** - Significant savings
   - Price drop 15-25%
   - Black Friday deal
   - Special on Pushover (different sound)

4. **Emergency** - Best price ever!
   - At or below historical low
   - Price drop >25%
   - Requires acknowledgment on Pushover
   - Special "cash register" sound

## Deduplication Logic

The Durable Object prevents spam by:

1. **Exact match deduplication** - Same model + retailer + price (±1%) within 24 hours
2. **Time-based rate limiting** - Minimum hours between ANY notifications
3. **Daily limits** - Maximum notifications per day
4. **Event ID tracking** - Prevents processing same event twice

## State Management

The Durable Object stores:

```fsharp
type NotificationState = {
    History: NotificationHistoryEntry list      // Last 100 notifications
    LastNotificationTime: DateTime option       // For rate limiting
    NotificationsToday: int                     // Daily counter
    LastResetDate: DateTime                     // For resetting daily counter
}
```

This state persists across:
- Worker restarts
- Cron executions
- Manual triggers

## Testing Notifications

### Test a single notification

```bash
# Send test notification to Durable Object
curl -X POST https://your-worker.workers.dev/test-notification \
  -H "Content-Type: application/json" \
  -d '{
    "model": "GZ302EA-R9641TB",
    "price": 1299.99,
    "retailer": "Best Buy"
  }'
```

### Check notification history

```bash
curl https://your-worker.workers.dev/notification-history
```

### Reset notification state

```bash
curl -X DELETE https://your-worker.workers.dev/notification-history
```

## Channel-Specific Setup

### Pushover Setup

1. Create account at [pushover.net](https://pushover.net/)
2. Purchase app ($5 one-time)
3. Create application
4. Copy API Token and User Key
5. Set secrets:
   ```bash
   wrangler secret put PUSHOVER_API_TOKEN
   wrangler secret put PUSHOVER_USER_KEY
   ```

### Telegram Setup

1. Create bot with [@BotFather](https://t.me/botfather)
2. Get bot token
3. Start conversation with your bot
4. Get chat ID:
   ```bash
   curl https://api.telegram.org/bot<TOKEN>/getUpdates
   ```
5. Set secrets:
   ```bash
   wrangler secret put TELEGRAM_BOT_TOKEN
   wrangler secret put TELEGRAM_CHAT_ID
   ```

### Discord Setup

1. Open Discord server settings
2. Go to Integrations → Webhooks
3. Create webhook, copy URL
4. Set secret:
   ```bash
   wrangler secret put DISCORD_WEBHOOK_URL
   ```

### Twilio Setup

1. Sign up at [twilio.com](https://www.twilio.com/)
2. Get Account SID and Auth Token
3. Get a phone number
4. Set secrets:
   ```bash
   wrangler secret put TWILIO_ACCOUNT_SID
   wrangler secret put TWILIO_AUTH_TOKEN
   wrangler secret put TWILIO_FROM_NUMBER  # +1234567890
   wrangler secret put TWILIO_TO_NUMBER    # Your phone
   ```

## Cost Considerations

| Channel | Cost | Notes |
|---------|------|-------|
| **Pushover** | $5 one-time | Best value, unlimited notifications |
| **Telegram** | Free | Unlimited, but need to manage bot |
| **Discord** | Free | Unlimited webhooks |
| **Twilio** | ~$0.0075/SMS | Can get expensive with many notifications |
| **Email** | Free-$0.001 | Via SendGrid, Mailgun, etc. |

**Recommendation**: Use Pushover for reliability and simplicity.

## Best Practices

1. **Start Conservative** - Use conservative notification rules initially
2. **Test Thoroughly** - Send test notifications before relying on them
3. **Multiple Channels** - Use 2+ channels for redundancy (Pushover + Discord)
4. **Monitor Daily Limits** - Check logs to see if you're hitting limits
5. **Adjust Thresholds** - Fine-tune based on deal frequency
6. **Use Emergency Wisely** - Reserve for truly exceptional deals

## Troubleshooting

### No notifications received

1. Check Durable Object logs:
   ```bash
   wrangler tail laptop-deal-agent
   ```

2. Verify secrets are set:
   ```bash
   wrangler secret list
   ```

3. Test notification manually via `/test-notification` endpoint

4. Check notification history to see if it was sent but blocked

### Too many notifications

1. Increase `MinHoursBetweenNotifications`
2. Decrease `MaxNotificationsPerDay`
3. Increase `MinPriceDropPercent`
4. Enable `OnlyHistoricalLow = true`

### Duplicate notifications

- Should not happen with Durable Object
- Check logs for Durable Object errors
- Verify state is being saved correctly

## Advanced: Multi-User Support

To support multiple users:

```fsharp
// Each user gets their own Durable Object instance
let userId = "user@example.com"

let! notified = notifyDeal env userId event rules channels
```

The Durable Object uses `idFromName(userId)` to create a stable, unique instance per user.

## Future Enhancements

- [ ] Web dashboard to view notification history
- [ ] SMS fallback if push fails
- [ ] Notification scheduling (quiet hours)
- [ ] Geo-based notifications (deals in your region)
- [ ] Price prediction ML integration
- [ ] Notification summaries (digest mode)

---

**Bottom Line**: The Durable Object pattern provides robust, stateful notification management that prevents spam while ensuring you never miss a great deal. Combined with Pushover, you'll get instant, reliable notifications on your phone whenever the agent finds a deal matching your criteria.
