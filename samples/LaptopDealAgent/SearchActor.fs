module LaptopDealAgent.SearchActor

open System
open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.DurableObjects
open LaptopDealAgent.Types
open LaptopDealAgent.AIAnalyzer

/// Search Actor - Durable Object for parallel search execution with idempotency
[<AllowNullLiteral>]
type SearchActorDO(state: DurableObjectState, env: obj) =
    inherit DurableObject(state, env)

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

    /// Check if URL should be processed (new or price drop)
    member this.ShouldProcessUrl(url: string, newPrice: decimal) : bool * string =
        match actorState with
        | None -> (true, "No state initialized")
        | Some st ->
            match st.TrackedUrls.TryFind url with
            | None ->
                (true, "New URL")
            | Some tracked ->
                if newPrice < tracked.LastPrice then
                    let drop = tracked.LastPrice - newPrice
                    let percentDrop = (drop / tracked.LastPrice) * 100M
                    (true, $"Price drop: ${drop:F2} ({percentDrop:F1}%)")
                else
                    (false, $"Already tracked at ${tracked.LastPrice:F2}")

    /// Update tracked URL with new price
    member this.UpdateTrackedUrl(url: string, price: decimal) =
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
                        LastSeen = now
                        FirstSeen = now
                        PriceHistory = [(now, price)]
                    }
                | Some tracked ->
                    // Update existing
                    {
                        tracked with
                            LastPrice = price
                            LastSeen = now
                            PriceHistory = (now, price) :: tracked.PriceHistory |> List.take (min 50 (tracked.PriceHistory.Length + 1))
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
                                    // Check if we should process this URL
                                    let shouldProcess, reason = this.ShouldProcessUrl(url, price)

                                    if shouldProcess then
                                        printfn "[SearchActor %s] ✓ Processing: %s - %s" model.ModelNumber url reason
                                        this.UpdateTrackedUrl(url, price)
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
    override this.fetch(request: obj) : JS.Promise<obj> =
        promise {
            try
                let req = request |> unbox<CloudFlare.Worker.Context.Request>
                let! body = req.json() |> unbox<JS.Promise<obj>>

                let action = body?action |> unbox<string>

                match action with
                | "init" ->
                    let modelStr = body?model |> unbox<string>
                    let model =
                        if modelStr.Contains("R9641TB") then GZ302EA_R9641TB
                        else GZ302EA_XS99

                    actorState <- Some (defaultSearchActorState model)
                    do! this.SaveStateAsync()

                    return CloudFlare.Worker.Context.Response.json({| success = true |}) :> obj

                | "search" ->
                    let searchQuery = body?query |> unbox<string>
                    let maxResults = body?maxResults |> unbox<int>

                    let! results = this.ExecuteSearch(searchQuery, maxResults)

                    let response = createObj [
                        "success" ==> true
                        "results" ==> results
                        "count" ==> results.Length
                    ]

                    return CloudFlare.Worker.Context.Response.json(response) :> obj

                | "getState" ->
                    let! state = this.GetState()

                    return CloudFlare.Worker.Context.Response.json(state) :> obj

                | "reset" ->
                    do! this.ResetState()

                    return CloudFlare.Worker.Context.Response.json({| success = true |}) :> obj

                | _ ->
                    return CloudFlare.Worker.Context.Response.create($"Unknown action: {action}", 400.0) :> obj

            with ex ->
                printfn "[SearchActor] Error in fetch: %s" ex.Message
                return CloudFlare.Worker.Context.Response.create($"Error: {ex.Message}", 500.0) :> obj
        }

/// Helper to get SearchActor instance
let getSearchActor (env: obj) (model: LaptopModel) : obj =
    let namespace = env?SEARCH_ACTOR

    // Generate stable ID based on model
    let id = namespace?idFromName(model.ModelNumber)

    // Get the Durable Object stub
    namespace?get(id)

/// Initialize a search actor with model
let initializeSearchActor (env: obj) (model: LaptopModel) : JS.Promise<unit> =
    promise {
        try
            let actor = getSearchActor env model

            let payload = createObj [
                "action" ==> "init"
                "model" ==> model.ModelNumber
            ]

            let requestInit = jsOptions(fun o ->
                o.method <- Some "POST"
                o.headers <- Some (U2.Case1 (createObj [
                    "Content-Type" ==> "application/json"
                ]))
                o.body <- Some (U2.Case1 (JS.JSON.stringify(payload)))
            )

            let! response = actor?fetch("https://fake-host/", requestInit) |> unbox<JS.Promise<obj>>

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

            let requestInit = jsOptions(fun o ->
                o.method <- Some "POST"
                o.headers <- Some (U2.Case1 (createObj [
                    "Content-Type" ==> "application/json"
                ]))
                o.body <- Some (U2.Case1 (JS.JSON.stringify(payload)))
            )

            let! response = actor?fetch("https://fake-host/", requestInit) |> unbox<JS.Promise<obj>>

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
