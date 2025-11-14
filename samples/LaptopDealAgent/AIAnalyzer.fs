module LaptopDealAgent.AIAnalyzer

open System
open Fable.Core
open Fable.Core.JsInterop
open LaptopDealAgent.Types

/// Reputable retailers we trust
let reputableRetailers = [
    "Best Buy"; "bestbuy.com"
    "Amazon"; "amazon.com"
    "Newegg"; "newegg.com"
    "Micro Center"; "microcenter.com"
    "B&H Photo"; "bhphotovideo.com"
    "ASUS"; "asus.com"
    "Microsoft Store"; "microsoft.com"
    "Walmart"; "walmart.com"
    "Target"; "target.com"
    "Costco"; "costco.com"
]

/// Check if retailer is reputable
let isReputableRetailer (url: string) (retailerName: string) : bool =
    let urlLower = url.ToLowerInvariant()
    let nameLower = retailerName.ToLowerInvariant()

    reputableRetailers
    |> List.exists (fun retailer ->
        let retailerLower = retailer.ToLowerInvariant()
        urlLower.Contains(retailerLower) || nameLower.Contains(retailerLower)
    )

/// Analyze page content using Cloudflare AI
let analyzePageWithAI (ai: obj) (url: string) (htmlContent: string) (targetModel: LaptopModel) : JS.Promise<PriceInfo option> =
    promise {
        try
            let modelNumber = targetModel.ModelNumber
            let ramSize = targetModel.RAMSize
            let maxPrice = if ramSize = 64 then 2000M else 2300M

            let prompt = $"""
Analyze this product listing page for the ASUS ROG Flow Z13 laptop.

TARGET DEVICE:
- Model: {modelNumber}
- RAM: {ramSize}GB
- Max acceptable price: ${maxPrice}

IMPORTANT VALIDATION:
1. Device MUST be the exact model: {modelNumber}
2. Device MUST be "New" or "Certified Refurbished" condition
3. Device MUST be in stock or available for purchase
4. Price MUST be under ${maxPrice} USD
5. Retailer must be reputable (Best Buy, Amazon, Newegg, B&H, ASUS, etc.)

EXTRACT THE FOLLOWING:
- Exact product title
- Model number (verify it matches {modelNumber})
- RAM size (verify it's {ramSize}GB)
- Current price in USD
- Original price (if discounted)
- Condition (New, Certified Refurbished, Used, etc.)
- Stock status
- Quantity available (if listed, e.g., "Only 3 left", "12 in stock")
- Retailer name
- Is this a Black Friday or special promotion deal?

PAGE CONTENT:
{htmlContent.Substring(0, Math.Min(8000, htmlContent.Length))}

Return ONLY a JSON object with this exact structure (no markdown, no explanation):
{{
  "isMatch": true/false,
  "modelNumber": "GZ302EA-XS99",
  "ramSize": 128,
  "price": 1999.99,
  "originalPrice": 2499.99,
  "condition": "New",
  "inStock": true,
  "quantity": 5,
  "retailer": "Best Buy",
  "isBlackFriday": true,
  "stockText": "Only 5 left in stock",
  "title": "ASUS ROG Flow Z13..."
}}

If this is NOT the correct device or doesn't meet criteria, return: {{"isMatch": false}}
"""

            // Use Cloudflare AI for analysis
            let! response =
                ai?run(
                    "@cf/meta/llama-3-8b-instruct",
                    createObj [
                        "prompt" ==> prompt
                        "max_tokens" ==> 500
                    ]
                ) |> unbox<JS.Promise<obj>>

            let responseText = response?response |> unbox<string>

            printfn "AI Response: %s" (responseText.Substring(0, Math.Min(200, responseText.Length)))

            // Parse AI response
            try
                let result = JS.JSON.parse(responseText)

                let isMatch = result?isMatch |> unbox<bool>

                if not isMatch then
                    printfn "AI determined this is not a match"
                    return None
                else
                    let price = result?price |> unbox<float> |> decimal
                    let originalPrice = result?originalPrice |> Option.ofObj |> Option.map (unbox<float> >> decimal)
                    let quantity = result?quantity |> Option.ofObj |> Option.map unbox<int>
                    let condition = result?condition |> unbox<string>
                    let inStock = result?inStock |> unbox<bool>
                    let retailer = result?retailer |> unbox<string>

                    // Validate condition
                    let isValidCondition =
                        condition.ToLowerInvariant().Contains("new") ||
                        condition.ToLowerInvariant().Contains("certified refurbished")

                    if not isValidCondition then
                        printfn "Invalid condition: %s (must be New or Certified Refurbished)" condition
                        return None

                    // Validate price threshold
                    if price > maxPrice then
                        printfn "Price $%.2f exceeds max $%.2f" price maxPrice
                        return None

                    // Validate retailer
                    if not (isReputableRetailer url retailer) then
                        printfn "Not a reputable retailer: %s" retailer
                        return None

                    // Calculate discount
                    let discountPercentage =
                        match originalPrice with
                        | Some orig when orig > price ->
                            Some (float ((orig - price) / orig * 100M))
                        | _ -> None

                    let priceInfo = {
                        Model = modelNumber
                        Price = Some price
                        Currency = "USD"
                        Retailer = retailer
                        Url = url
                        InStock = inStock
                        IsBlackFridayDeal = result?isBlackFriday |> unbox<bool>
                        DiscountPercentage = discountPercentage
                        OriginalPrice = originalPrice
                        DetectedAt = DateTime.UtcNow
                        Condition = Some condition
                        Quantity = quantity
                        StockText = result?stockText |> Option.ofObj |> Option.map unbox<string>
                        Title = result?title |> unbox<string>
                    }

                    printfn "✓ Valid deal found: $%.2f for %s at %s (Qty: %A)"
                        price modelNumber retailer quantity

                    return Some priceInfo
            with ex ->
                printfn "Error parsing AI response: %s" ex.Message
                return None

        with ex ->
            printfn "Error in AI analysis: %s" ex.Message
            return None
    }

/// Fetch page content
let fetchPageContent (url: string) : JS.Promise<string option> =
    promise {
        try
            printfn "Fetching page: %s" url

            let! response = Fetch.fetch url []

            if response.Ok then
                let! html = response.text()
                return Some html
            else
                printfn "Failed to fetch page: HTTP %d" response.Status
                return None
        with ex ->
            printfn "Error fetching page: %s" ex.Message
            return None
    }

/// Deep analyze a URL with AI
let deepAnalyzeUrl (ai: obj) (url: string) (targetModel: LaptopModel) : JS.Promise<PriceInfo option> =
    promise {
        printfn "🤖 Deep analyzing URL: %s" url

        let! htmlOpt = fetchPageContent url

        match htmlOpt with
        | None ->
            return None
        | Some html ->
            return! analyzePageWithAI ai url html targetModel
    }
