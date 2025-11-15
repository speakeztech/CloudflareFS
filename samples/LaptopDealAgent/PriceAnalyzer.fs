module LaptopDealAgent.PriceAnalyzer

open System
open Fable.Core
open Fable.Core.JsInterop
open LaptopDealAgent.Types

/// Calculate average price from price history
let calculateAveragePrice (history: PriceHistoryEntry list) : decimal option =
    if history.IsEmpty then
        None
    else
        let sum = history |> List.sumBy (fun h -> h.Price)
        Some (sum / decimal history.Length)

/// Find lowest historical price
let findLowestPrice (history: PriceHistoryEntry list) : decimal option =
    if history.IsEmpty then
        None
    else
        history
        |> List.map (fun h -> h.Price)
        |> List.min
        |> Some

/// Determine price trend
let analyzePriceTrend (history: PriceHistoryEntry list) : string =
    if history.Length < 2 then
        "insufficient data"
    else
        let sortedHistory = history |> List.sortBy (fun h -> h.Timestamp)
        let recentPrices = sortedHistory |> List.rev |> List.take (min 5 sortedHistory.Length)

        if recentPrices.Length < 2 then
            "stable"
        else
            let first = recentPrices |> List.last
            let last = recentPrices |> List.head

            let percentageChange = ((last.Price - first.Price) / first.Price) * 100M

            if percentageChange > 5M then
                "increasing"
            elif percentageChange < -5M then
                "decreasing"
            else
                "stable"

/// Generate recommendation based on analysis
let generateRecommendation (analysis: DealAnalysis) : string =
    match analysis.CurrentBestPrice, analysis.LowestHistoricalPrice with
    | None, _ ->
        "No current prices available. Check back later."

    | Some currentPrice, None ->
        $"Current best price: ${currentPrice:F2}. No historical data for comparison."

    | Some currentPrice, Some lowestPrice ->
        let savings = lowestPrice - currentPrice
        let percentSavings = (savings / lowestPrice) * 100M
        let currentPriceStr = currentPrice.ToString("F2")
        let lowestPriceStr = lowestPrice.ToString("F2")
        let percentStr = percentSavings.ToString("F1")

        if currentPrice <= lowestPrice then
            $"🎉 BEST PRICE EVER! Current price ${currentPriceStr} is at or below historical low. Strong buy recommendation!"
        elif percentSavings > 15M then
            $"Great Deal! Current price ${currentPriceStr} is {percentStr}%% below historical low (${lowestPriceStr}). Recommended purchase."
        elif percentSavings > 5M then
            $"Good Deal. Current price ${currentPriceStr} is {percentStr}%% above historical low but still competitive."
        elif analysis.PriceTrend = "decreasing" then
            $"Prices are trending down. Current: ${currentPriceStr}. Consider waiting a few days."
        elif analysis.PriceTrend = "increasing" then
            $"⚠️ Prices are rising. Current: ${currentPriceStr}. May want to purchase soon."
        else
            $"Current price ${currentPriceStr}. Historical low: ${lowestPriceStr}. Price is stable."

/// Store price information in KV
let storePriceHistory (kv: obj) (priceInfo: PriceInfo) : JS.Promise<unit> =
    promise {
        try
            let timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss")
            let key = $"price_history_{priceInfo.Model}_{priceInfo.Retailer}_{timestamp}"

            let entry: PriceHistoryEntry = {
                Model = priceInfo.Model
                Price = priceInfo.Price |> Option.defaultValue 0M
                Currency = priceInfo.Currency
                Retailer = priceInfo.Retailer
                Url = priceInfo.Url
                Timestamp = DateTime.UtcNow
                IsBlackFridayDeal = priceInfo.IsBlackFridayDeal
            }

            let json = JS.JSON.stringify(entry)
            do! kv?put(key, json) |> unbox<JS.Promise<unit>>

            printfn $"Stored price history: {key}"
        with
        | ex ->
            printfn $"Error storing price history: {ex.Message}"
    }

/// Retrieve price history from KV
let retrievePriceHistory (kv: obj) (model: LaptopModel) : JS.Promise<PriceHistoryEntry list> =
    promise {
        try
            let prefix = $"price_history_{model.ModelNumber}_"

            // List all keys with the prefix
            let! listResult = kv?list({| prefix = prefix |}) |> unbox<JS.Promise<obj>>

            let keys = listResult?keys |> unbox<obj array>

            // Retrieve all values
            let! values =
                keys
                |> Array.map (fun key ->
                    promise {
                        let keyName = key?name |> unbox<string>
                        let! value = kv?get(keyName) |> unbox<JS.Promise<string>>

                        if isNull value || value = "" then
                            return None
                        else
                            try
                                let entry = JS.JSON.parse(value) |> unbox<PriceHistoryEntry>
                                return Some entry
                            with
                            | _ -> return None
                    }
                )
                |> Promise.all

            let history =
                values
                |> Array.choose id
                |> Array.toList
                |> List.sortByDescending (fun h -> h.Timestamp)

            return history
        with
        | ex ->
            printfn $"Error retrieving price history: {ex.Message}"
            return []
    }

/// Analyze current prices against historical data
let analyzePrices (kv: obj) (currentPrices: PriceInfo list) (model: LaptopModel) : JS.Promise<DealAnalysis> =
    promise {
        // Get price history
        let! history = retrievePriceHistory kv model

        // Find current best price
        let modelNumber = model.ModelNumber
        let currentBestPrice =
            currentPrices
            |> List.choose (fun p -> if p.Model = modelNumber then p.Price else None)
            |> function
                | [] -> None
                | prices -> Some (List.min prices)

        let bestRetailer =
            currentPrices
            |> List.filter (fun p -> p.Model = modelNumber && p.Price = currentBestPrice)
            |> List.tryHead
            |> Option.map (fun p -> p.Retailer)

        let averagePrice = calculateAveragePrice history
        let lowestPrice = findLowestPrice history
        let trend = analyzePriceTrend history

        let analysis = {
            Model = model
            CurrentBestPrice = currentBestPrice
            BestRetailer = bestRetailer
            PriceHistory = history |> List.take (min 10 history.Length)
            AveragePrice = averagePrice
            LowestHistoricalPrice = lowestPrice
            PriceTrend = trend
            Recommendation = ""  // Will be set next
        }

        let recommendation = generateRecommendation analysis

        return { analysis with Recommendation = recommendation }
    }

/// Format analysis as HTML report
let formatAnalysisReport (analyses: DealAnalysis list) : string =
    let now = DateTime.UtcNow

    let formatAnalysis (analysis: DealAnalysis) =
        let priceStr =
            match analysis.CurrentBestPrice with
            | Some price -> $"${price:F2}"
            | None -> "N/A"

        let retailerStr =
            analysis.BestRetailer
            |> Option.defaultValue "Unknown"

        let historyRows =
            analysis.PriceHistory
            |> List.take (min 5 analysis.PriceHistory.Length)
            |> List.map (fun h ->
                let timestamp = h.Timestamp.ToString("yyyy-MM-dd HH:mm")
                let price = h.Price.ToString("F2")
                let blackFridayStatus = if h.IsBlackFridayDeal then "🔥 Yes" else "No"
                $"""<tr>
                    <td>{timestamp}</td>
                    <td>{h.Retailer}</td>
                    <td>${price}</td>
                    <td>{blackFridayStatus}</td>
                </tr>"""
            )
            |> String.concat "\n"

        $"""
        <div class="laptop-section">
            <h2>{analysis.Model.FullName}</h2>
            <div class="price-info">
                <div class="current-price">
                    <h3>Current Best Price</h3>
                    <p class="price">{priceStr}</p>
                    <p class="retailer">at {retailerStr}</p>
                </div>
                <div class="stats">
                    <div class="stat">
                        <span class="label">Price Trend:</span>
                        <span class="value">{analysis.PriceTrend}</span>
                    </div>
                    <div class="stat">
                        <span class="label">Historical Low:</span>
                        <span class="value">{analysis.LowestHistoricalPrice |> Option.map (sprintf "$%.2f") |> Option.defaultValue "N/A"}</span>
                    </div>
                    <div class="stat">
                        <span class="label">Average Price:</span>
                        <span class="value">{analysis.AveragePrice |> Option.map (sprintf "$%.2f") |> Option.defaultValue "N/A"}</span>
                    </div>
                </div>
            </div>
            <div class="recommendation">
                <h3>Recommendation</h3>
                <p>{analysis.Recommendation}</p>
            </div>
            <div class="history">
                <h3>Recent Price History</h3>
                <table>
                    <thead>
                        <tr>
                            <th>Date</th>
                            <th>Retailer</th>
                            <th>Price</th>
                            <th>Black Friday</th>
                        </tr>
                    </thead>
                    <tbody>
                        {historyRows}
                    </tbody>
                </table>
            </div>
        </div>
        """

    let analysisHtml =
        analyses
        |> List.map formatAnalysis
        |> String.concat "\n"

    let lastUpdated = now.ToString("yyyy-MM-dd HH:mm:ss")

    let html =
        """<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>ASUS ROG Flow Z13 Black Friday Deal Tracker</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: system-ui, -apple-system, sans-serif;
            background: #000;
            color: #e0e0e0;
            padding: 20px;
            line-height: 1.6;
        }
        .container {
            max-width: 1200px;
            margin: 0 auto;
        }
        h1 {
            color: #f38020;
            margin-bottom: 10px;
            font-size: 2em;
        }
        .updated {
            color: #999;
            margin-bottom: 30px;
        }
        .laptop-section {
            background: #1a1a1a;
            border: 1px solid #333;
            border-radius: 8px;
            padding: 25px;
            margin-bottom: 30px;
        }
        .laptop-section h2 {
            color: #f38020;
            margin-bottom: 20px;
        }
        .price-info {
            display: grid;
            grid-template-columns: 1fr 2fr;
            gap: 20px;
            margin-bottom: 20px;
        }
        .current-price {
            background: #0a0a0a;
            padding: 20px;
            border-radius: 5px;
            text-align: center;
        }
        .price {
            font-size: 3em;
            color: #4CAF50;
            font-weight: bold;
            margin: 10px 0;
        }
        .retailer {
            color: #999;
        }
        .stats {
            background: #0a0a0a;
            padding: 20px;
            border-radius: 5px;
        }
        .stat {
            margin-bottom: 10px;
            display: flex;
            justify-content: space-between;
            padding: 8px 0;
            border-bottom: 1px solid #222;
        }
        .stat:last-child {
            border-bottom: none;
        }
        .label {
            color: #999;
        }
        .value {
            color: #e0e0e0;
            font-weight: bold;
        }
        .recommendation {
            background: #0a0a0a;
            padding: 20px;
            border-radius: 5px;
            margin-bottom: 20px;
        }
        .recommendation h3 {
            color: #f38020;
            margin-bottom: 10px;
        }
        .recommendation p {
            font-size: 1.1em;
            line-height: 1.5;
        }
        .history {
            margin-top: 20px;
        }
        .history h3 {
            color: #f38020;
            margin-bottom: 15px;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            background: #0a0a0a;
            border-radius: 5px;
            overflow: hidden;
        }
        th {
            background: #222;
            color: #f38020;
            padding: 12px;
            text-align: left;
        }
        td {
            padding: 10px 12px;
            border-bottom: 1px solid #222;
        }
        tr:last-child td {
            border-bottom: none;
        }
        .footer {
            text-align: center;
            color: #666;
            margin-top: 50px;
            padding-top: 20px;
            border-top: 1px solid #333;
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>🎮 ASUS ROG Flow Z13 Black Friday Deal Tracker</h1>
        <p class="updated">Last updated: """ + lastUpdated + """ UTC</p>

        """ + analysisHtml + """

        <div class="footer">
            <p>Powered by CloudflareFS - Automated price monitoring every hour</p>
        </div>
    </div>
</body>
</html>"""
    html
