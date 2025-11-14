module LaptopDealAgent.UI.Api

open Fable.Core
open Fetch
open Fable.Core.JsInterop
open LaptopDealAgent.UI.Types

let private baseUrl = "/api"

let private defaultProps =
    [ RequestProperties.Credentials RequestCredentials.Sameorigin ]

let private handleResponse<'T> (response: Response) : JS.Promise<'T> =
    promise {
        if response.Ok then
            let! json = response.text()
            return JS.JSON.parse json |> unbox<'T>
        else
            return failwith $"HTTP {response.Status}: {response.StatusText}"
    }

/// Get all current deals
let getDeals () : JS.Promise<DealsResponse> =
    promise {
        let! response = fetch $"{baseUrl}/deals" defaultProps
        return! handleResponse response
    }

/// Get price history for all models
let getHistory () : JS.Promise<PriceHistoryEntry list> =
    promise {
        let! response = fetch $"{baseUrl}/history" defaultProps
        return! handleResponse response
    }

/// Get analysis for all models
let getAnalysis () : JS.Promise<AnalysisResponse> =
    promise {
        let! response = fetch $"{baseUrl}/analysis" defaultProps
        return! handleResponse response
    }

/// Trigger manual search
let triggerSearch () : JS.Promise<unit> =
    promise {
        let! response =
            fetch
                $"{baseUrl}/trigger"
                [ RequestProperties.Method HttpMethod.POST
                  yield! defaultProps ]

        if not response.Ok then
            return failwith $"Failed to trigger search: {response.StatusText}"
    }
