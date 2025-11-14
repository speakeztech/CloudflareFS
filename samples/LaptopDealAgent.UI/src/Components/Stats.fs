module LaptopDealAgent.UI.Components.Stats

open Feliz
open LaptopDealAgent.UI.Types

[<ReactComponent>]
let Stats (deals: PriceInfo list) (analyses: DealAnalysis list) =
    let totalDeals = deals.Length

    let blackFridayDeals =
        deals |> List.filter (fun d -> d.IsBlackFridayDeal) |> List.length

    let inStockDeals =
        deals |> List.filter (fun d -> d.InStock) |> List.length

    let bestPrice =
        analyses
        |> List.choose (fun a -> a.CurrentBestPrice)
        |> function
            | [] -> None
            | prices -> Some (List.min prices)

    Html.div [
        prop.className "stats stats-vertical lg:stats-horizontal shadow bg-base-200 w-full"
        prop.children [
            // Total Deals
            Html.div [
                prop.className "stat"
                prop.children [
                    Html.div [
                        prop.className "stat-figure text-secondary"
                        prop.children [
                            Html.text "📊"
                        ]
                    ]
                    Html.div [
                        prop.className "stat-title"
                        prop.text "Total Deals"
                    ]
                    Html.div [
                        prop.className "stat-value text-primary"
                        prop.text $"{totalDeals}"
                    ]
                    Html.div [
                        prop.className "stat-desc"
                        prop.text "tracked listings"
                    ]
                ]
            ]

            // Black Friday Deals
            Html.div [
                prop.className "stat"
                prop.children [
                    Html.div [
                        prop.className "stat-figure text-secondary"
                        prop.children [
                            Html.text "🔥"
                        ]
                    ]
                    Html.div [
                        prop.className "stat-title"
                        prop.text "Black Friday"
                    ]
                    Html.div [
                        prop.className "stat-value text-error"
                        prop.text $"{blackFridayDeals}"
                    ]
                    Html.div [
                        prop.className "stat-desc"
                        prop.text "special offers"
                    ]
                ]
            ]

            // In Stock
            Html.div [
                prop.className "stat"
                prop.children [
                    Html.div [
                        prop.className "stat-figure text-secondary"
                        prop.children [
                            Html.text "✓"
                        ]
                    ]
                    Html.div [
                        prop.className "stat-title"
                        prop.text "In Stock"
                    ]
                    Html.div [
                        prop.className "stat-value text-success"
                        prop.text $"{inStockDeals}"
                    ]
                    Html.div [
                        prop.className "stat-desc"
                        prop.text "available now"
                    ]
                ]
            ]

            // Best Price
            Html.div [
                prop.className "stat"
                prop.children [
                    Html.div [
                        prop.className "stat-figure text-secondary"
                        prop.children [
                            Html.text "💰"
                        ]
                    ]
                    Html.div [
                        prop.className "stat-title"
                        prop.text "Best Price"
                    ]
                    Html.div [
                        prop.className "stat-value text-accent"
                        prop.text (
                            match bestPrice with
                            | Some p -> $"${p:F0}"
                            | None -> "N/A"
                        )
                    ]
                    Html.div [
                        prop.className "stat-desc"
                        prop.text "lowest found"
                    ]
                ]
            ]
        ]
    ]
