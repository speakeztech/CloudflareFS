module LaptopDealAgent.D1Storage

open System
open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.D1
open LaptopDealAgent.Types

/// Database record types that match our schema

type ModelRecord = {
    id: int
    model_number: string
    ram_size: int
    full_name: string
    max_price: decimal
}

type RetailerRecord = {
    id: int
    name: string
    domain: string
    is_reputable: bool
}

type DealRecord = {
    id: int
    model_id: int
    retailer_id: int
    url: string
    price: decimal option
    quantity: int option
    stock_text: string option
    condition: string option
    in_stock: bool
    is_black_friday_deal: bool
    discount_percentage: float option
    original_price: decimal option
    title: string
    first_seen: string
    last_seen: string
}

type PriceHistoryRecord = {
    id: int
    deal_id: int
    price: decimal
    quantity: int option
    timestamp: string
}

/// Get model ID from model number
let getModelId (db: D1Database) (modelNumber: string) : JS.Promise<int option> =
    promise {
        try
            let! result =
                db.prepare("SELECT id FROM models WHERE model_number = ?")
                    .bind(modelNumber)
                    .first<ModelRecord>()

            return result |> Option.map (fun r -> r.id)
        with ex ->
            printfn "Error getting model ID: %s" ex.Message
            return None
    }

/// Get or create retailer ID from domain
let getOrCreateRetailerId (db: D1Database) (retailerName: string) (domain: string) : JS.Promise<int option> =
    promise {
        try
            // Try to find existing retailer by domain
            let! existing =
                db.prepare("SELECT id FROM retailers WHERE domain = ?")
                    .bind(domain)
                    .first<RetailerRecord>()

            match existing with
            | Some r -> return Some r.id
            | None ->
                // Insert new retailer (will be marked as not reputable by default for unknown retailers)
                let! result =
                    db.prepare("INSERT INTO retailers (name, domain, is_reputable) VALUES (?, ?, 0) RETURNING id")
                        .bind(retailerName, domain)
                        .first<{| id: int |}>()

                return result |> Option.map (fun r -> r.id)
        with ex ->
            printfn "Error getting/creating retailer: %s" ex.Message
            return None
    }

/// Extract domain from URL
let extractDomain (url: string) : string =
    try
        let uri = Uri(url)
        uri.Host.ToLowerInvariant()
            .Replace("www.", "")
    with _ ->
        "unknown.com"

/// Insert or update a deal
let upsertDeal (db: D1Database) (priceInfo: PriceInfo) : JS.Promise<int option> =
    promise {
        try
            // Get model ID
            let! modelIdOpt = getModelId db priceInfo.Model

            match modelIdOpt with
            | None ->
                printfn "Model not found: %s" priceInfo.Model
                return None
            | Some modelId ->
                // Get or create retailer
                let domain = extractDomain priceInfo.Url
                let! retailerIdOpt = getOrCreateRetailerId db priceInfo.Retailer domain

                match retailerIdOpt with
                | None ->
                    printfn "Could not get retailer ID for: %s" priceInfo.Retailer
                    return None
                | Some retailerId ->
                    // Check if deal already exists
                    let! existingDeal =
                        db.prepare("SELECT id, price, quantity FROM deals WHERE url = ? AND model_id = ?")
                            .bind(priceInfo.Url, modelId)
                            .first<{| id: int; price: decimal option; quantity: int option |}>()

                    match existingDeal with
                    | Some existing ->
                        // Update existing deal
                        let! _ =
                            db.prepare("""
                                UPDATE deals
                                SET price = ?, quantity = ?, stock_text = ?, condition = ?,
                                    in_stock = ?, is_black_friday_deal = ?, discount_percentage = ?,
                                    original_price = ?, title = ?, last_seen = CURRENT_TIMESTAMP,
                                    last_updated = CURRENT_TIMESTAMP
                                WHERE id = ?
                            """)
                                .bind(
                                    priceInfo.Price |> Option.toObj,
                                    priceInfo.Quantity |> Option.toObj,
                                    priceInfo.StockText |> Option.toObj,
                                    priceInfo.Condition |> Option.toObj,
                                    priceInfo.InStock,
                                    priceInfo.IsBlackFridayDeal,
                                    priceInfo.DiscountPercentage |> Option.toObj,
                                    priceInfo.OriginalPrice |> Option.toObj,
                                    priceInfo.Title,
                                    existing.id
                                )
                                .run()

                        // If price or quantity changed, add to history
                        let priceChanged =
                            match priceInfo.Price, existing.price with
                            | Some newPrice, Some oldPrice -> newPrice <> oldPrice
                            | _ -> false

                        let quantityChanged =
                            match priceInfo.Quantity, existing.quantity with
                            | Some newQty, Some oldQty -> newQty <> oldQty
                            | Some _, None -> true
                            | _ -> false

                        if priceChanged || quantityChanged then
                            let! _ =
                                db.prepare("INSERT INTO price_history (deal_id, price, quantity) VALUES (?, ?, ?)")
                                    .bind(
                                        existing.id,
                                        priceInfo.Price |> Option.defaultValue 0M,
                                        priceInfo.Quantity |> Option.toObj
                                    )
                                    .run()
                            ()

                        printfn "✓ Updated deal ID %d" existing.id
                        return Some existing.id

                    | None ->
                        // Insert new deal
                        let! result =
                            db.prepare("""
                                INSERT INTO deals (
                                    model_id, retailer_id, url, price, quantity, stock_text,
                                    condition, in_stock, is_black_friday_deal, discount_percentage,
                                    original_price, title
                                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
                                RETURNING id
                            """)
                                .bind(
                                    modelId,
                                    retailerId,
                                    priceInfo.Url,
                                    priceInfo.Price |> Option.toObj,
                                    priceInfo.Quantity |> Option.toObj,
                                    priceInfo.StockText |> Option.toObj,
                                    priceInfo.Condition |> Option.toObj,
                                    priceInfo.InStock,
                                    priceInfo.IsBlackFridayDeal,
                                    priceInfo.DiscountPercentage |> Option.toObj,
                                    priceInfo.OriginalPrice |> Option.toObj,
                                    priceInfo.Title
                                )
                                .first<{| id: int |}>()

                        match result with
                        | Some r ->
                            // Add initial price history entry
                            let! _ =
                                db.prepare("INSERT INTO price_history (deal_id, price, quantity) VALUES (?, ?, ?)")
                                    .bind(
                                        r.id,
                                        priceInfo.Price |> Option.defaultValue 0M,
                                        priceInfo.Quantity |> Option.toObj
                                    )
                                    .run()

                            printfn "✓ Inserted new deal ID %d" r.id
                            return Some r.id
                        | None ->
                            printfn "Failed to insert deal"
                            return None

        with ex ->
            printfn "Error upserting deal: %s" ex.Message
            return None
    }

/// Get all current deals
let getAllDeals (db: D1Database) : JS.Promise<PriceInfo list> =
    promise {
        try
            let! result =
                db.prepare("""
                    SELECT
                        d.url, d.price, d.quantity, d.stock_text, d.condition,
                        d.in_stock, d.is_black_friday_deal, d.discount_percentage,
                        d.original_price, d.title, d.last_seen,
                        m.model_number,
                        r.name as retailer_name
                    FROM deals d
                    JOIN models m ON d.model_id = m.id
                    JOIN retailers r ON d.retailer_id = r.id
                    WHERE d.in_stock = 1
                    ORDER BY d.last_seen DESC
                """)
                    .all<{|
                        url: string
                        price: decimal option
                        quantity: int option
                        stock_text: string option
                        condition: string option
                        in_stock: bool
                        is_black_friday_deal: bool
                        discount_percentage: float option
                        original_price: decimal option
                        title: string
                        last_seen: string
                        model_number: string
                        retailer_name: string
                    |}>()

            match result.results with
            | Some deals ->
                return
                    deals
                    |> Seq.map (fun d ->
                        {
                            Model = d.model_number
                            Price = d.price
                            Currency = "USD"
                            Retailer = d.retailer_name
                            Url = d.url
                            InStock = d.in_stock
                            IsBlackFridayDeal = d.is_black_friday_deal
                            DiscountPercentage = d.discount_percentage
                            OriginalPrice = d.original_price
                            DetectedAt = DateTime.Parse(d.last_seen)
                            Condition = d.condition
                            Quantity = d.quantity
                            StockText = d.stock_text
                            Title = d.title
                        }
                    )
                    |> Seq.toList
            | None ->
                return []

        with ex ->
            printfn "Error getting all deals: %s" ex.Message
            return []
    }

/// Get deals for a specific model
let getDealsByModel (db: D1Database) (modelNumber: string) : JS.Promise<PriceInfo list> =
    promise {
        try
            let! result =
                db.prepare("""
                    SELECT
                        d.url, d.price, d.quantity, d.stock_text, d.condition,
                        d.in_stock, d.is_black_friday_deal, d.discount_percentage,
                        d.original_price, d.title, d.last_seen,
                        m.model_number,
                        r.name as retailer_name
                    FROM deals d
                    JOIN models m ON d.model_id = m.id
                    JOIN retailers r ON d.retailer_id = r.id
                    WHERE m.model_number = ? AND d.in_stock = 1
                    ORDER BY d.price ASC
                """)
                    .bind(modelNumber)
                    .all<{|
                        url: string
                        price: decimal option
                        quantity: int option
                        stock_text: string option
                        condition: string option
                        in_stock: bool
                        is_black_friday_deal: bool
                        discount_percentage: float option
                        original_price: decimal option
                        title: string
                        last_seen: string
                        model_number: string
                        retailer_name: string
                    |}>()

            match result.results with
            | Some deals ->
                return
                    deals
                    |> Seq.map (fun d ->
                        {
                            Model = d.model_number
                            Price = d.price
                            Currency = "USD"
                            Retailer = d.retailer_name
                            Url = d.url
                            InStock = d.in_stock
                            IsBlackFridayDeal = d.is_black_friday_deal
                            DiscountPercentage = d.discount_percentage
                            OriginalPrice = d.original_price
                            DetectedAt = DateTime.Parse(d.last_seen)
                            Condition = d.condition
                            Quantity = d.quantity
                            StockText = d.stock_text
                            Title = d.title
                        }
                    )
                    |> Seq.toList
            | None ->
                return []

        with ex ->
            printfn "Error getting deals by model: %s" ex.Message
            return []
    }

/// Get price history for a specific deal URL
let getPriceHistory (db: D1Database) (url: string) : JS.Promise<(DateTime * decimal * int option) list> =
    promise {
        try
            let! dealResult =
                db.prepare("SELECT id FROM deals WHERE url = ?")
                    .bind(url)
                    .first<{| id: int |}>()

            match dealResult with
            | None -> return []
            | Some deal ->
                let! historyResult =
                    db.prepare("""
                        SELECT price, quantity, timestamp
                        FROM price_history
                        WHERE deal_id = ?
                        ORDER BY timestamp DESC
                        LIMIT 100
                    """)
                        .bind(deal.id)
                        .all<{| price: decimal; quantity: int option; timestamp: string |}>()

                match historyResult.results with
                | Some history ->
                    return
                        history
                        |> Seq.map (fun h -> (DateTime.Parse(h.timestamp), h.price, h.quantity))
                        |> Seq.toList
                | None ->
                    return []

        with ex ->
            printfn "Error getting price history: %s" ex.Message
            return []
    }

/// Get best current price for a model
let getBestPrice (db: D1Database) (modelNumber: string) : JS.Promise<decimal option> =
    promise {
        try
            let! result =
                db.prepare("""
                    SELECT MIN(d.price) as best_price
                    FROM deals d
                    JOIN models m ON d.model_id = m.id
                    WHERE m.model_number = ? AND d.in_stock = 1 AND d.price IS NOT NULL
                """)
                    .bind(modelNumber)
                    .first<{| best_price: decimal option |}>()

            return result |> Option.bind (fun r -> r.best_price)

        with ex ->
            printfn "Error getting best price: %s" ex.Message
            return None
    }

/// Get analysis for all models
let getAnalysis (db: D1Database) : JS.Promise<DealAnalysis list> =
    promise {
        try
            let models = [GZ302EA_R9641TB; GZ302EA_XS99]

            let! analyses =
                models
                |> List.map (fun model ->
                    promise {
                        let! deals = getDealsByModel db model.ModelNumber
                        let! bestPrice = getBestPrice db model.ModelNumber

                        let prices = deals |> List.choose (fun d -> d.Price)
                        let avgPrice =
                            if prices.Length > 0 then
                                Some (prices |> List.average)
                            else
                                None

                        let lowestPrice =
                            if prices.Length > 0 then
                                Some (prices |> List.min)
                            else
                                None

                        let bestRetailer =
                            deals
                            |> List.filter (fun d -> d.Price = lowestPrice)
                            |> List.tryHead
                            |> Option.map (fun d -> d.Retailer)

                        return {
                            Model = model
                            CurrentBestPrice = bestPrice
                            BestRetailer = bestRetailer
                            PriceHistory = []  // Could populate from price_history table
                            AveragePrice = avgPrice
                            LowestHistoricalPrice = lowestPrice
                            PriceTrend = "stable"  // Could calculate from history
                            Recommendation =
                                match bestPrice with
                                | Some price when price < (decimal model.RAMSize * 20M) ->
                                    $"Excellent deal at ${price:F2}! Consider purchasing."
                                | Some price ->
                                    $"Current price: ${price:F2}. Monitor for price drops."
                                | None ->
                                    "No deals currently available."
                        }
                    }
                )
                |> Promise.all

            return analyses |> Array.toList

        with ex ->
            printfn "Error getting analysis: %s" ex.Message
            return []
    }
