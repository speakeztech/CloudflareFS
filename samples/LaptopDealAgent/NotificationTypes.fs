module LaptopDealAgent.NotificationTypes

open System
open LaptopDealAgent.Types

/// Notification channels
type NotificationChannel =
    | Pushover of apiToken: string * userKey: string
    | Telegram of botToken: string * chatId: string
    | Discord of webhookUrl: string
    | Twilio of accountSid: string * authToken: string * fromNumber: string * toNumber: string
    | Email of apiKey: string * toEmail: string
    | Slack of webhookUrl: string

/// Notification priority levels
type NotificationPriority =
    | Low          // Good deal, but not urgent
    | Normal       // Notable price drop
    | High         // Significant price drop (>15%)
    | Emergency    // Best price ever / Black Friday exclusive

    member this.ToInt() =
        match this with
        | Low -> 0
        | Normal -> 1
        | High -> 2
        | Emergency -> 3

/// Notification trigger rules
type NotificationRule = {
    /// Minimum price drop percentage to trigger notification
    MinPriceDropPercent: float option

    /// Only notify if price is at or below historical low
    OnlyHistoricalLow: bool

    /// Only notify for Black Friday deals
    OnlyBlackFriday: bool

    /// Maximum notifications per day
    MaxNotificationsPerDay: int

    /// Minimum hours between notifications for same model
    MinHoursBetweenNotifications: float

    /// Specific models to notify about (empty = all)
    ModelsToWatch: LaptopModel list

    /// Specific retailers to watch (empty = all)
    RetailersToWatch: string list
}

/// Default conservative notification rules
let defaultNotificationRules = {
    MinPriceDropPercent = Some 10.0  // 10% drop minimum
    OnlyHistoricalLow = false
    OnlyBlackFriday = false
    MaxNotificationsPerDay = 5
    MinHoursBetweenNotifications = 2.0
    ModelsToWatch = []  // All models
    RetailersToWatch = []  // All retailers
}

/// Aggressive notification rules (notify on any good deal)
let aggressiveNotificationRules = {
    MinPriceDropPercent = Some 5.0
    OnlyHistoricalLow = false
    OnlyBlackFriday = false
    MaxNotificationsPerDay = 10
    MinHoursBetweenNotifications = 1.0
    ModelsToWatch = []
    RetailersToWatch = []
}

/// Conservative notification rules (only best deals)
let conservativeNotificationRules = {
    MinPriceDropPercent = Some 15.0
    OnlyHistoricalLow = true
    OnlyBlackFriday = false
    MaxNotificationsPerDay = 3
    MinHoursBetweenNotifications = 6.0
    ModelsToWatch = []
    RetailersToWatch = []
}

/// Notification event
type NotificationEvent = {
    EventId: string
    Model: LaptopModel
    Price: decimal
    PreviousLowestPrice: decimal option
    Retailer: string
    Url: string
    IsBlackFridayDeal: bool
    DiscountPercentage: float option
    Priority: NotificationPriority
    Timestamp: DateTime
    Recommendation: string
}

/// Notification history entry (for deduplication)
type NotificationHistoryEntry = {
    EventId: string
    Model: LaptopModel
    Retailer: string
    Price: decimal
    NotifiedAt: DateTime
    Channel: string
}

/// Notification state (stored in Durable Object)
type NotificationState = {
    History: NotificationHistoryEntry list
    LastNotificationTime: DateTime option
    NotificationsToday: int
    LastResetDate: DateTime
}

/// Default state
let defaultNotificationState = {
    History = []
    LastNotificationTime = None
    NotificationsToday = 0
    LastResetDate = DateTime.UtcNow.Date
}

/// Notification message content
type NotificationMessage = {
    Title: string
    Body: string
    Url: string option
    Priority: NotificationPriority
}
