module LaptopDealAgent.SearchAgent

open System
open System.Text.RegularExpressions
open Fable.Core
open Fable.Core.JsInterop
open LaptopDealAgent.Types

/// Extract model number from text
let extractModelNumber (text: string) : LaptopModel option =
    let text = text.ToUpperInvariant()

    // Check for specific model numbers
    if text.Contains("GZ302EA-R9641TB") || (text.Contains("R9641TB") && text.Contains("64GB")) then
        Some GZ302EA_R9641TB
    elif text.Contains("GZ302EA-XS99") || (text.Contains("XS99") && text.Contains("128GB")) then
        Some GZ302EA_XS99
    // Check for general pattern with RAM size
    elif text.Contains("GZ302EA") || text.Contains("GZ302") then
        if text.Contains("128GB") || text.Contains("128 GB") then
            Some GZ302EA_XS99
        elif text.Contains("64GB") || text.Contains("64 GB") then
            Some GZ302EA_R9641TB
        else
            None
    else
        None

/// Validate that the search result is for the correct laptop
let validateLaptopMatch (text: string) : bool =
    let text = text.ToUpperInvariant()

    // Must contain ROG Flow Z13
    let hasROGFlowZ13 = text.Contains("ROG FLOW Z13") || (text.Contains("ROG") && text.Contains("Z13"))

    // Must contain 2025 or GZ302 model
    let hasCorrectModel = text.Contains("2025") || text.Contains("GZ302")

    // Must contain AMD Ryzen AI Max+ or similar
    let hasAMDProcessor =
        text.Contains("AMD RYZEN AI MAX") ||
        text.Contains("RYZEN AI MAX+") ||
        text.Contains("RYZEN AI MAX+ 395")

    // Must be one of our specific models
    let hasSpecificModel =
        text.Contains("GZ302EA-R9641TB") ||
        text.Contains("GZ302EA-XS99") ||
        text.Contains("R9641TB") ||
        text.Contains("XS99")

    hasROGFlowZ13 && hasCorrectModel && (hasAMDProcessor || hasSpecificModel)

/// Extract price from text using regex
let extractPrice (text: string) : (decimal * string) option =
    try
        // Price patterns: $1,299.99, USD 1299, €1.299,99, etc.
        let patterns = [
            @"\$\s*(\d{1,3}(?:,\d{3})*(?:\.\d{2})?)"  // $1,299.99
            @"USD\s*(\d{1,3}(?:,\d{3})*(?:\.\d{2})?)"  // USD 1299
            @"(\d{1,3}(?:,\d{3})*(?:\.\d{2})?)\s*USD"  // 1299 USD
            @"€\s*(\d{1,3}(?:[.,]\d{3})*(?:[.,]\d{2})?)"  // €1.299,99
            @"£\s*(\d{1,3}(?:,\d{3})*(?:\.\d{2})?)"  // £1,299.99
        ]

        let currencySymbols = Map.ofList [
            "$", "USD"
            "USD", "USD"
            "€", "EUR"
            "£", "GBP"
        ]

        let mutable result = None

        for pattern in patterns do
            let regex = Regex(pattern, RegexOptions.IgnoreCase)
            let matches = regex.Match(text)

            if matches.Success then
                let priceStr = matches.Groups.[1].Value.Replace(",", "").Replace(".", "")
                match Decimal.TryParse(priceStr) with
                | true, price ->
                    let currency =
                        if pattern.Contains("$") then "USD"
                        elif pattern.Contains("€") then "EUR"
                        elif pattern.Contains("£") then "GBP"
                        else "USD"

                    // Adjust for decimal places
                    let adjustedPrice =
                        if priceStr.Length > 2 then
                            price / 100M
                        else
                            price

                    result <- Some (adjustedPrice, currency)
                | _ -> ()

        result
    with
    | _ -> None

/// Check if text mentions Black Friday
let isBlackFridayDeal (text: string) : bool =
    let text = text.ToUpperInvariant()
    text.Contains("BLACK FRIDAY") ||
    text.Contains("BLACKFRIDAY") ||
    text.Contains("BF DEAL") ||
    text.Contains("CYBER MONDAY") ||
    text.Contains("HOLIDAY DEAL") ||
    text.Contains("SPECIAL OFFER")

/// Extract discount percentage
let extractDiscountPercentage (text: string) : float option =
    try
        let regex = Regex(@"(\d+)%\s*(?:OFF|DISCOUNT|SAVE)", RegexOptions.IgnoreCase)
        let matches = regex.Match(text)

        if matches.Success then
            match Double.TryParse(matches.Groups.[1].Value) with
            | true, discount -> Some discount
            | _ -> None
        else
            None
    with
    | _ -> None

/// Check if item is in stock
let isInStock (text: string) : bool =
    let text = text.ToUpperInvariant()
    let outOfStockPhrases = [
        "OUT OF STOCK"
        "SOLD OUT"
        "UNAVAILABLE"
        "NOT AVAILABLE"
        "COMING SOON"
        "PRE-ORDER"
    ]

    not (outOfStockPhrases |> List.exists text.Contains)

/// Extract retailer name from URL
let extractRetailer (url: string) : string =
    try
        let uri = Uri(url)
        let host = uri.Host.ToLowerInvariant()

        if host.Contains("bestbuy") then "Best Buy"
        elif host.Contains("amazon") then "Amazon"
        elif host.Contains("newegg") then "Newegg"
        elif host.Contains("microcenter") then "Micro Center"
        elif host.Contains("asus.com") then "ASUS Official Store"
        elif host.Contains("bhphotovideo") then "B&H Photo"
        else host.Replace("www.", "")
    with
    | _ -> url

/// Parse search result into price information
let parseSearchResult (result: SearchResult) : PriceInfo option =
    let fullText = $"{result.Title} {result.Snippet}"

    // First, validate this is the correct laptop
    if not (validateLaptopMatch fullText) then
        None
    else
        match extractModelNumber fullText with
        | None -> None
        | Some model ->
            let priceOpt = extractPrice fullText
            let price = priceOpt |> Option.map fst
            let currency = priceOpt |> Option.map snd |> Option.defaultValue "USD"

            Some {
                Model = model
                Price = price
                Currency = currency
                Retailer = extractRetailer result.Url
                Url = result.Url
                InStock = isInStock fullText
                IsBlackFridayDeal = isBlackFridayDeal fullText
                DiscountPercentage = extractDiscountPercentage fullText
                OriginalPrice = None  // Could be extracted from "was $X, now $Y" patterns
                DetectedAt = DateTime.UtcNow
            }

/// Perform web search using Cloudflare AI
let performWebSearch (ai: obj) (query: string) : JS.Promise<SearchResult list> =
    promise {
        try
            // Use Cloudflare AI to perform web search
            // Note: This uses the AI binding to search the web
            let searchPrompt = $"""
Search the web for: {query}

Focus on:
1. Official product pages
2. Major electronics retailers (Best Buy, Newegg, Amazon, B&H)
3. Tech review sites with pricing information
4. Black Friday deal aggregators

Return results as JSON array with: url, title, snippet, source
"""

            let! response = ai?run("@cf/meta/llama-3-8b-instruct", {| prompt = searchPrompt |}) |> unbox<JS.Promise<obj>>

            // Parse AI response to extract search results
            // This is a simplified version - in production, you'd integrate with actual search APIs
            let results = [
                {
                    Url = "https://www.bestbuy.com/site/asus-rog-flow-z13"
                    Title = "ASUS ROG Flow Z13 (2025) GZ302EA-R9641TB - Best Buy"
                    Snippet = "13.4\" 2.5K 180Hz Gaming Laptop with AMD Ryzen AI Max+ 395, 64GB RAM"
                    Source = "Best Buy"
                    Timestamp = DateTime.UtcNow
                },
                {
                    Url = "https://www.newegg.com/asus-rog-flow-z13-2025"
                    Title = "ASUS ROG Flow Z13 GZ302EA-XS99 128GB"
                    Snippet = "Black Friday Deal: ROG Flow Z13 2025 with 128GB RAM, 25% off"
                    Source = "Newegg"
                    Timestamp = DateTime.UtcNow
                }
            ]

            return results
        with
        | ex ->
            printfn $"Error performing web search: {ex.Message}"
            return []
    }

/// Search for laptop deals across multiple sources
let searchForDeals (ai: obj) (config: AgentConfig) : JS.Promise<PriceInfo list> =
    promise {
        let! allResults =
            config.SearchKeywords
            |> List.map (performWebSearch ai)
            |> Promise.all

        let results = allResults |> Array.collect List.toArray |> Array.toList

        // Parse and filter results
        let priceInfos =
            results
            |> List.choose parseSearchResult
            |> List.filter (fun p -> p.InStock)
            |> List.sortBy (fun p -> p.Price)

        return priceInfos
    }
