module LaptopDealAgent.SearchActor

open System
open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.Worker.Context
open CloudFlare.Worker.Context.Globals
open CloudFlare.DurableObjects
open LaptopDealAgent.Types
open LaptopDealAgent.AIAnalyzer

/// Search Actor - Durable Object for parallel search execution with idempotency
[<AllowNullLiteral>]
type SearchActorDO(state: DurableObjectState<obj>, env: obj) =

    let mutable actorState: SearchActorState option = None

    /// Initialize state from storage
    member this.InitializeAsync() : JS.Promise<unit> =
        promise {
            try
                let! storedState = state.storage.get("state") |> unbox<JS.Promise<obj>>

                if not (isNull storedState) then
                    actorState <- Some (storedState |> unbox<SearchActorState>)
                    let st = actorState.Value
                    printfn "[SearchActor %s] Loaded state: %d tracked URLs" st.Model.ModelNumber st.TrackedUrls.Count
                else
                    printfn "[SearchActor] No existing state"
            with ex ->
                printfn "[SearchActor] Error loading state: %s" ex.Message
        }

    /// Save state to storage
    member this.SaveStateAsync() : JS.Promise<unit> =
        promise {
            match actorState with
            | Some st ->
                do! state.storage.put("state", st) |> unbox<JS.Promise<unit>>
                printfn "[SearchActor %s] State saved" st.Model.ModelNumber
            | None -> ()
        }

    /// Check if URL should be processed (new, price drop, or stock drop)
    member this.ShouldProcessUrl(url: string, newPrice: decimal, newQuantity: int option) : bool * string =
        match actorState with
        | None -> (true, "No state initialized")
        | Some st ->
            match st.TrackedUrls.TryFind url with
            | None ->
                (true, "New URL")
            | Some tracked ->
                // Check for price drop
                let priceDropped = newPrice < tracked.LastPrice

                // Check for quantity drop (stock running out)
                let quantityDropped =
                    match tracked.LastQuantity, newQuantity with
                    | Some oldQty, Some newQty when newQty < oldQty ->
                        true
                    | _ -> false

                if priceDropped then
                    let drop = tracked.LastPrice - newPrice
                    let percentDrop = (drop / tracked.LastPrice) * 100M
                    let dropStr = drop.ToString("F2")
                    let percentStr = percentDrop.ToString("F1")
                    (true, $"Price drop: ${dropStr} ({percentStr}%%)")
                elif quantityDropped then
                    match tracked.LastQuantity, newQuantity with
                    | Some oldQty, Some newQty ->
                        let qtyDrop = oldQty - newQty
                        (true, $"Stock drop: {qtyDrop} units ({oldQty} → {newQty})")
                    | _ -> (false, "No changes")
                else
                    let lastPriceStr = tracked.LastPrice.ToString("F2")
                    (false, $"Already tracked at ${lastPriceStr}")

    /// Update tracked URL with new price and quantity
    member this.UpdateTrackedUrl(url: string, price: decimal, quantity: int option) =
        match actorState with
        | None -> ()
        | Some st ->
            let now = DateTime.UtcNow

            let updated =
                match st.TrackedUrls.TryFind url with
                | None ->
                    // New URL
                    {
                        Url = url
                        LastPrice = price
                        LastQuantity = quantity
                        LastSeen = now
                        FirstSeen = now
                        PriceHistory = [(now, price)]
                        QuantityHistory =
                            match quantity with
                            | Some qty -> [(now, qty)]
                            | None -> []
                    }
                | Some tracked ->
                    // Update existing
                    let newQuantityHistory =
                        match quantity with
                        | Some qty ->
                            (now, qty) :: tracked.QuantityHistory
                            |> List.take (min 50 (tracked.QuantityHistory.Length + 1))
                        | None -> tracked.QuantityHistory

                    {
                        tracked with
                            LastPrice = price
                            LastQuantity = quantity
                            LastSeen = now
                            PriceHistory = (now, price) :: tracked.PriceHistory |> List.take (min 50 (tracked.PriceHistory.Length + 1))
                            QuantityHistory = newQuantityHistory
                    }

            actorState <- Some {
                st with
                    TrackedUrls = st.TrackedUrls.Add(url, updated)
                    TotalDealsFound = st.TotalDealsFound + 1
                    LastRun = Some now
            }

    /// Execute search for a specific model
    member this.ExecuteSearch(searchQuery: string, maxResults: int) : JS.Promise<PriceInfo list> =
        promise {
            try
                do! this.InitializeAsync()

                let model =
                    match actorState with
                    | Some st -> st.Model
                    | None -> failwith "State not initialized"

                printfn "[SearchActor %s] 🔍 Executing search: %s" model.ModelNumber searchQuery

                // Perform web search (simplified - in production use actual search API)
                let! searchResults = this.PerformWebSearch(searchQuery, maxResults)

                printfn "[SearchActor %s] Found %d search results" model.ModelNumber searchResults.Length

                // Deep analyze each URL in parallel
                let! analyzedResults =
                    searchResults
                    |> List.map (fun url ->
                        promise {
                            printfn "[SearchActor %s] Analyzing: %s" model.ModelNumber url

                            let ai = env?AI

                            let! priceInfoOpt = deepAnalyzeUrl ai url model

                            match priceInfoOpt with
                            | None ->
                                printfn "[SearchActor %s] ✗ Not a valid deal: %s" model.ModelNumber url
                                return None

                            | Some priceInfo ->
                                match priceInfo.Price with
                                | None ->
                                    return None
                                | Some price ->
                                    // Check if we should process this URL (checks for price drop or stock drop)
                                    let shouldProcess, reason = this.ShouldProcessUrl(url, price, priceInfo.Quantity)

                                    if shouldProcess then
                                        printfn "[SearchActor %s] ✓ Processing: %s - %s" model.ModelNumber url reason
                                        this.UpdateTrackedUrl(url, price, priceInfo.Quantity)
                                        return Some priceInfo
                                    else
                                        printfn "[SearchActor %s] ⊘ Skipping: %s - %s" model.ModelNumber url reason
                                        return None
                        }
                    )
                    |> Promise.all

                let validDeals = analyzedResults |> Array.choose id |> Array.toList

                printfn "[SearchActor %s] ✓ Found %d valid deals" model.ModelNumber validDeals.Length

                // Save updated state
                do! this.SaveStateAsync()

                return validDeals

            with ex ->
                printfn "[SearchActor] Error in search execution: %s" ex.Message
                return []
        }

    /// Perform web search (placeholder - integrate with real search API)
    member this.PerformWebSearch(query: string, maxResults: int) : JS.Promise<string list> =
        promise {
            // In production, integrate with:
            // - Google Custom Search API
            // - Bing Web Search API
            // - SerpApi
            // - etc.

            // For now, return known retailer URLs as examples
            let model =
                match actorState with
                | Some st -> st.Model
                | None -> failwith "State not initialized"

            let baseUrls = [
                $"https://www.bestbuy.com/site/asus-rog-flow-z13-{model.ModelNumber.ToLowerInvariant()}"
                $"https://www.amazon.com/dp/ASUS-ROG-Flow-Z13-{model.RAMSize}GB"
                $"https://www.newegg.com/asus-rog-flow-z13-{model.RAMSize}gb"
                $"https://www.bhphotovideo.com/c/product/asus-rog-flow-z13-{model.ModelNumber}"
                $"https://www.microcenter.com/product/asus-rog-flow-z13-{model.RAMSize}gb"
            ]

            return baseUrls |> List.take (min maxResults baseUrls.Length)
        }

    /// Get current state
    member this.GetState() : JS.Promise<SearchActorState option> =
        promise {
            do! this.InitializeAsync()
            return actorState
        }

    /// Reset state (for testing)
    member this.ResetState() : JS.Promise<unit> =
        promise {
            match actorState with
            | Some st ->
                actorState <- Some (defaultSearchActorState st.Model)
                do! this.SaveStateAsync()
            | None -> ()
        }

    /// Handle HTTP requests to the Durable Object
    interface DurableObject with
        member this.fetch(request: Request) : U2<Response, JS.Promise<Response>> =
            let handleRequest() =
                promise {
                    try
                        let! body = request.json() |> unbox<JS.Promise<obj>>

                        let action = body?action |> unbox<string>

                        match action with
                        | "init" ->
                            let modelStr = body?model |> unbox<string>
                            let model =
                                if modelStr.Contains("R9641TB") then GZ302EA_R9641TB
                                else GZ302EA_XS99

                            actorState <- Some (defaultSearchActorState model)
                            do! this.SaveStateAsync()

                            return Response.json({| success = true |})

                        | "search" ->
                            let searchQuery = body?query |> unbox<string>
                            let maxResults = body?maxResults |> unbox<int>

                            let! results = this.ExecuteSearch(searchQuery, maxResults)

                            let response = createObj [
                                "success" ==> true
                                "results" ==> results
                                "count" ==> results.Length
                            ]

                            return Response.json(response)

                        | "getState" ->
                            let! state = this.GetState()

                            return Response.json(state)

                        | "reset" ->
                            do! this.ResetState()

                            return Response.json({| success = true |})

                        | _ ->
                            let init : ResponseInit = createObj [ "status" ==> 400.0 ] |> unbox
                            return Response.Create(U2.Case1 $"Unknown action: {action}", init)

                    with ex ->
                        printfn "[SearchActor] Error in fetch: %s" ex.Message
                        let init : ResponseInit = createObj [ "status" ==> 500.0 ] |> unbox
                        return Response.Create(U2.Case1 $"Error: {ex.Message}", init)
                }
            U2.Case2 (handleRequest())

        member _.alarm() = JS.Constructors.Promise.Create(fun resolve _ -> resolve())
        member _.webSocketMessage(ws, message) = JS.Constructors.Promise.Create(fun resolve _ -> resolve())
        member _.webSocketClose(ws, code, reason, wasClean) = JS.Constructors.Promise.Create(fun resolve _ -> resolve())
        member _.webSocketError(ws, error) = JS.Constructors.Promise.Create(fun resolve _ -> resolve())

/// Helper to get SearchActor instance
let getSearchActor (env: obj) (model: LaptopModel) : obj =
    let ``namespace`` = env?SEARCH_ACTOR

    // Generate stable ID based on model
    let id = ``namespace``?idFromName(model.ModelNumber)

    // Get the Durable Object stub
    ``namespace``?get(id)

/// Initialize a search actor with model
let initializeSearchActor (env: obj) (model: LaptopModel) : JS.Promise<unit> =
    promise {
        try
            let actor = getSearchActor env model

            let payload = createObj [
                "action" ==> "init"
                "model" ==> model.ModelNumber
            ]

            let requestInit = createObj [
                "method" ==> "POST"
                "headers" ==> createObj [
                    "Content-Type" ==> "application/json"
                ]
                "body" ==> JS.JSON.stringify(payload)
            ]

            let! response = actor?fetch("https://fake-host/", requestInit) |> unbox<JS.Promise<Response>>

            if response?ok |> unbox<bool> then
                printfn "✓ SearchActor initialized for %s" model.ModelNumber
            else
                printfn "✗ Failed to initialize SearchActor for %s" model.ModelNumber

        with ex ->
            printfn "Error initializing SearchActor: %s" ex.Message
    }

/// Execute search via actor
let executeSearch (env: obj) (model: LaptopModel) (query: string) (maxResults: int) : JS.Promise<PriceInfo list> =
    promise {
        try
            let actor = getSearchActor env model

            let payload = createObj [
                "action" ==> "search"
                "query" ==> query
                "maxResults" ==> maxResults
            ]

            let requestInit = createObj [
                "method" ==> "POST"
                "headers" ==> createObj [
                    "Content-Type" ==> "application/json"
                ]
                "body" ==> JS.JSON.stringify(payload)
            ]

            let! response = actor?fetch("https://fake-host/", requestInit) |> unbox<JS.Promise<Response>>

            if response?ok |> unbox<bool> then
                let! result = response?json() |> unbox<JS.Promise<obj>>
                let results = result?results |> unbox<PriceInfo list>

                printfn "✓ SearchActor found %d deals for %s" results.Length model.ModelNumber

                return results
            else
                printfn "✗ SearchActor failed for %s" model.ModelNumber
                return []

        with ex ->
            printfn "Error executing search via actor: %s" ex.Message
            return []
    }
