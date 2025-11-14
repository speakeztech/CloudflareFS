module LaptopDealAgent.UI.Components.DealCard

open Feliz
open System
open LaptopDealAgent.UI.Types

[<ReactComponent>]
let DealCard (deal: PriceInfo) =
    let modelInfo = LaptopModel.FromString deal.Model

    let badgeColor =
        if deal.IsBlackFridayDeal then "badge-error"
        elif deal.InStock then "badge-success"
        else "badge-warning"

    let priceDisplay =
        match deal.Price with
        | Some p -> $"${p:F2}"
        | None -> "Price N/A"

    let discountBadge =
        match deal.DiscountPercentage with
        | Some discount when discount > 0.0 ->
            Html.div [
                prop.className "badge badge-secondary badge-lg"
                prop.children [
                    Html.text $"%.0f{discount}%% OFF"
                ]
            ]
        | _ -> Html.none

    Html.article [
        prop.className "card bg-base-200 shadow-xl deal-card animate-fade-in"
        prop.children [
            Html.div [
                prop.className "card-body"
                prop.children [
                    // Header with badges
                    Html.div [
                        prop.className "flex justify-between items-start mb-2"
                        prop.children [
                            Html.h3 [
                                prop.className "card-title text-lg"
                                prop.text (
                                    match modelInfo with
                                    | Some m -> m.FullName
                                    | None -> "ASUS ROG Flow Z13 (2025)"
                                )
                            ]

                            Html.div [
                                prop.className "flex gap-2"
                                prop.children [
                                    if deal.IsBlackFridayDeal then
                                        Html.div [
                                            prop.className "badge badge-error gap-1"
                                            prop.children [
                                                Html.text "🔥 BLACK FRIDAY"
                                            ]
                                        ]

                                    Html.div [
                                        prop.className $"badge {badgeColor}"
                                        prop.text (if deal.InStock then "In Stock" else "Out of Stock")
                                    ]
                                ]
                            ]
                        ]
                    ]

                    // Model Number
                    Html.div [
                        prop.className "text-sm opacity-70 mb-4"
                        prop.text $"Model: {deal.Model}"
                    ]

                    // Price Section
                    Html.div [
                        prop.className "stats shadow bg-base-300 mb-4"
                        prop.children [
                            Html.div [
                                prop.className "stat place-items-center"
                                prop.children [
                                    Html.div [
                                        prop.className "stat-title"
                                        prop.text "Current Price"
                                    ]
                                    Html.div [
                                        prop.className "stat-value text-primary text-3xl"
                                        prop.text priceDisplay
                                    ]
                                    discountBadge
                                ]
                            ]
                        ]
                    ]

                    // Retailer Info
                    Html.div [
                        prop.className "grid grid-cols-2 gap-4 mb-4 text-sm"
                        prop.children [
                            Html.div [
                                prop.children [
                                    Html.div [
                                        prop.className "opacity-70"
                                        prop.text "Retailer"
                                    ]
                                    Html.div [
                                        prop.className "font-semibold"
                                        prop.text deal.Retailer
                                    ]
                                ]
                            ]

                            Html.div [
                                prop.children [
                                    Html.div [
                                        prop.className "opacity-70"
                                        prop.text "Detected"
                                    ]
                                    Html.div [
                                        prop.className "font-semibold"
                                        prop.text (deal.DetectedAt.ToString("MMM dd, HH:mm"))
                                    ]
                                ]
                            ]
                        ]
                    ]

                    // Action Button
                    Html.div [
                        prop.className "card-actions justify-end"
                        prop.children [
                            Html.a [
                                prop.className "btn btn-primary"
                                prop.href deal.Url
                                prop.target "_blank"
                                prop.rel "noopener noreferrer"
                                prop.children [
                                    Html.text "View Deal "
                                    Html.text "→"
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
