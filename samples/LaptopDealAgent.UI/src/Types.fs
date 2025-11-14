module LaptopDealAgent.UI.Types

open System

type LaptopModel =
    | GZ302EA_R9641TB
    | GZ302EA_XS99

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

    static member FromString (str: string) =
        match str with
        | "GZ302EA-R9641TB" -> Some GZ302EA_R9641TB
        | "GZ302EA-XS99" -> Some GZ302EA_XS99
        | _ -> None

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
}

type PriceHistoryEntry = {
    Model: string
    Price: decimal
    Currency: string
    Retailer: string
    Url: string
    Timestamp: DateTime
    IsBlackFridayDeal: bool
}

type DealAnalysis = {
    Model: string
    CurrentBestPrice: decimal option
    BestRetailer: string option
    PriceHistory: PriceHistoryEntry list
    AveragePrice: decimal option
    LowestHistoricalPrice: decimal option
    PriceTrend: string
    Recommendation: string
}

type DealsResponse = {
    Deals: PriceInfo list
    LastUpdated: DateTime
    TotalDeals: int
}

type AnalysisResponse = {
    Analyses: DealAnalysis list
    GeneratedAt: DateTime
}
