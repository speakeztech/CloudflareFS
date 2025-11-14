module LaptopDealAgent.UI.App

open Feliz
open Browser.Dom
open LaptopDealAgent.UI.Types
open LaptopDealAgent.UI.Api
open LaptopDealAgent.UI.Components

[<ReactComponent>]
let App () =
    let deals, setDeals = React.useState<PriceInfo list>([])
    let analyses, setAnalyses = React.useState<DealAnalysis list>([])
    let lastUpdated, setLastUpdated = React.useState<System.DateTime option>(None)
    let isLoading, setIsLoading = React.useState(false)
    let error, setError = React.useState<string option>(None)

    let loadData () =
        promise {
            setIsLoading true
            setError None

            try
                // Load deals and analyses in parallel
                let! dealsResponse = getDeals()
                let! analysisResponse = getAnalysis()

                setDeals dealsResponse.Deals
                setAnalyses analysisResponse.Analyses
                setLastUpdated (Some dealsResponse.LastUpdated)

                setIsLoading false
            with ex ->
                console.error("Error loading data:", ex)
                setError (Some ex.Message)
                setIsLoading false
        }

    let handleRefresh () =
        promise {
            try
                // Trigger search on the backend
                do! triggerSearch()

                // Wait a bit for the search to complete
                do! Promise.sleep 2000

                // Reload data
                do! loadData()
            with ex ->
                console.error("Error refreshing:", ex)
                setError (Some "Failed to trigger refresh")
        }
        |> Promise.start

    // Load data on mount
    React.useEffectOnce(fun () ->
        loadData() |> Promise.start
    )

    // Auto-refresh every 5 minutes
    React.useEffect((fun () ->
        let interval = JS.setInterval (fun () -> loadData() |> Promise.start) 300000.0 // 5 minutes

        React.createDisposable(fun () -> JS.clearInterval interval)
    ), [||])

    Html.div [
        prop.className "min-h-screen bg-base-100"
        prop.children [
            Html.div [
                prop.className "container mx-auto px-4 py-8 max-w-7xl"
                prop.children [
                    // Header
                    Header.Header lastUpdated handleRefresh isLoading

                    // Error Alert
                    match error with
                    | Some err ->
                        Html.div [
                            prop.className "alert alert-error shadow-lg mb-6"
                            prop.children [
                                Html.div [
                                    prop.children [
                                        Html.text "⚠️ "
                                        Html.span [ prop.text err ]
                                    ]
                                ]
                                Html.button [
                                    prop.className "btn btn-sm"
                                    prop.onClick (fun _ -> setError None)
                                    prop.text "Dismiss"
                                ]
                            ]
                        ]
                    | None -> Html.none

                    // Loading State
                    if isLoading && deals.IsEmpty then
                        Html.div [
                            prop.className "flex justify-center items-center h-96"
                            prop.children [
                                Html.div [
                                    prop.className "text-center"
                                    prop.children [
                                        Html.div [
                                            prop.className "loading loading-spinner loading-lg text-primary mb-4"
                                        ]
                                        Html.p [
                                            prop.className "text-xl"
                                            prop.text "Loading deals..."
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    else
                        Html.div [
                            prop.className "space-y-6"
                            prop.children [
                                // Stats
                                Stats.Stats deals analyses

                                // Deal List
                                DealList.DealList deals
                            ]
                        ]

                    // Footer
                    Html.footer [
                        prop.className "mt-12 text-center py-6 border-t border-base-300"
                        prop.children [
                            Html.p [
                                prop.className "text-sm opacity-70"
                                prop.children [
                                    Html.text "Powered by "
                                    Html.a [
                                        prop.className "link link-primary"
                                        prop.href "https://github.com/speakeztech/CloudflareFS"
                                        prop.target "_blank"
                                        prop.text "CloudflareFS"
                                    ]
                                    Html.text " - Built with F#, Fable, and DaisyUI"
                                ]
                            ]
                            Html.p [
                                prop.className "text-xs opacity-50 mt-2"
                                prop.text "Prices and availability are subject to change. Always verify on retailer's website."
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

// Mount the app
ReactDOM.createRoot(document.getElementById("root"))
    .render(App())
