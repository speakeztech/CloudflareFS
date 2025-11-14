module LaptopDealAgent.UI.Components.DealList

open Feliz
open LaptopDealAgent.UI.Types
open LaptopDealAgent.UI.Components.DealCard

type FilterOption =
    | All
    | BlackFridayOnly
    | InStockOnly
    | Model of string

[<ReactComponent>]
let DealList (deals: PriceInfo list) =
    let filter, setFilter = React.useState(All)
    let sortBy, setSortBy = React.useState("date")

    let filteredDeals =
        deals
        |> List.filter (fun deal ->
            match filter with
            | All -> true
            | BlackFridayOnly -> deal.IsBlackFridayDeal
            | InStockOnly -> deal.InStock
            | Model modelNum -> deal.Model = modelNum
        )
        |> List.sortBy (fun deal ->
            match sortBy with
            | "price" -> deal.Price |> Option.defaultValue System.Decimal.MaxValue |> float
            | "date" -> -deal.DetectedAt.Ticks |> float
            | _ -> 0.0
        )

    Html.div [
        prop.className "space-y-4"
        prop.children [
            // Filters and Sort
            Html.div [
                prop.className "card bg-base-200 shadow-lg"
                prop.children [
                    Html.div [
                        prop.className "card-body p-4"
                        prop.children [
                            Html.div [
                                prop.className "flex flex-wrap gap-4 items-center"
                                prop.children [
                                    // Filter Label
                                    Html.div [
                                        prop.className "font-semibold"
                                        prop.text "Filter:"
                                    ]

                                    // Filter Buttons
                                    Html.div [
                                        prop.className "btn-group"
                                        prop.children [
                                            Html.button [
                                                prop.className $"btn btn-sm {if filter = All then "btn-active" else ""}"
                                                prop.onClick (fun _ -> setFilter All)
                                                prop.text "All"
                                            ]
                                            Html.button [
                                                prop.className $"btn btn-sm {if filter = BlackFridayOnly then "btn-active" else ""}"
                                                prop.onClick (fun _ -> setFilter BlackFridayOnly)
                                                prop.text "🔥 Black Friday"
                                            ]
                                            Html.button [
                                                prop.className $"btn btn-sm {if filter = InStockOnly then "btn-active" else ""}"
                                                prop.onClick (fun _ -> setFilter InStockOnly)
                                                prop.text "✓ In Stock"
                                            ]
                                        ]
                                    ]

                                    Html.div [
                                        prop.className "divider divider-horizontal"
                                    ]

                                    // Sort Label
                                    Html.div [
                                        prop.className "font-semibold"
                                        prop.text "Sort:"
                                    ]

                                    // Sort Dropdown
                                    Html.select [
                                        prop.className "select select-bordered select-sm"
                                        prop.value sortBy
                                        prop.onChange (fun value -> setSortBy value)
                                        prop.children [
                                            Html.option [
                                                prop.value "date"
                                                prop.text "Most Recent"
                                            ]
                                            Html.option [
                                                prop.value "price"
                                                prop.text "Lowest Price"
                                            ]
                                        ]
                                    ]

                                    // Results Count
                                    Html.div [
                                        prop.className "ml-auto text-sm opacity-70"
                                        prop.text $"{filteredDeals.Length} deals"
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]

            // Deal Cards Grid
            if filteredDeals.IsEmpty then
                Html.div [
                    prop.className "card bg-base-200 shadow-lg"
                    prop.children [
                        Html.div [
                            prop.className "card-body items-center text-center"
                            prop.children [
                                Html.div [
                                    prop.className "text-6xl mb-4"
                                    prop.text "🔍"
                                ]
                                Html.h3 [
                                    prop.className "text-xl font-bold"
                                    prop.text "No deals found"
                                ]
                                Html.p [
                                    prop.className "opacity-70"
                                    prop.text "Check back later or try different filters"
                                ]
                            ]
                        ]
                    ]
                ]
            else
                Html.div [
                    prop.className "grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
                    prop.children (
                        filteredDeals
                        |> List.map DealCard
                    )
                ]
        ]
    ]
