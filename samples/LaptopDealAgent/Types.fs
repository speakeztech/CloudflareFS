module LaptopDealAgent.Types

open System

/// Specific laptop models we're tracking
type LaptopModel =
    | GZ302EA_R9641TB  // 64GB RAM variant
    | GZ302EA_XS99     // 128GB RAM variant

    member this.ModelNumber =
        match this with
        | GZ302EA_R9641TB -> "GZ302EA-R9641TB"
        | GZ302EA_XS99 -> "GZ302EA-XS99"

    member this.RAMSize =
        match this with
        | GZ302EA_R9641TB -> 64
        | GZ302EA_XS99 -> 128

    member this.FullName =
        $"ASUS ROG Flow Z13 (2025) GZ302 {this.ModelNumber} {this.RAMSize}GB RAM"

/// Search result from web search
type SearchResult = {
    Url: string
    Title: string
    Snippet: string
    Source: string
    Timestamp: DateTime
}

/// Price information extracted from a search result
type PriceInfo = {
    Model: string
    Price: decimal option
    Currency: string
    Retailer: string
    Url: string
    InStock: bool
    IsBlackFridayDeal: bool
    DiscountPercentage: float option
    OriginalPrice: decimal option
    DetectedAt: DateTime
    Condition: string option
    Quantity: int option
    StockText: string option
    Title: string
}

/// Historical price data point
type PriceHistoryEntry = {
    Model: LaptopModel
    Price: decimal
    Currency: string
    Retailer: string
    Url: string
    Timestamp: DateTime
    IsBlackFridayDeal: bool
}

/// Analysis result
type DealAnalysis = {
    Model: LaptopModel
    CurrentBestPrice: decimal option
    BestRetailer: string option
    PriceHistory: PriceHistoryEntry list
    AveragePrice: decimal option
    LowestHistoricalPrice: decimal option
    PriceTrend: string  // "increasing", "decreasing", "stable"
    Recommendation: string
}

/// Agent configuration
type AgentConfig = {
    SearchKeywords: string list
    MaxSearchResults: int
    MinPriceConfidence: float
    EnableNotifications: bool
}

/// Tracked URL with price and quantity for idempotency
type TrackedUrl = {
    Url: string
    LastPrice: decimal
    LastQuantity: int option
    LastSeen: DateTime
    FirstSeen: DateTime
    PriceHistory: (DateTime * decimal) list
    QuantityHistory: (DateTime * int) list
}

/// Search actor state for idempotency
type SearchActorState = {
    Model: LaptopModel
    TrackedUrls: Map<string, TrackedUrl>
    TotalDealsFound: int
    LastRun: DateTime option
}

/// Default state for a search actor
let defaultSearchActorState model = {
    Model = model
    TrackedUrls = Map.empty
    TotalDealsFound = 0
    LastRun = None
}
