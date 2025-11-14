module LaptopDealAgent.UI.Components.Header

open Feliz
open System

[<ReactComponent>]
let Header (lastUpdated: DateTime option) (onRefresh: unit -> unit) (isLoading: bool) =
    Html.div [
        prop.className "navbar bg-base-200 shadow-lg rounded-box mb-6"
        prop.children [
            Html.div [
                prop.className "flex-1"
                prop.children [
                    Html.div [
                        prop.className "flex items-center gap-3"
                        prop.children [
                            Html.div [
                                prop.className "text-4xl"
                                prop.text "🎮"
                            ]

                            Html.div [
                                prop.children [
                                    Html.h1 [
                                        prop.className "text-2xl font-bold text-primary"
                                        prop.text "ROG Flow Z13 Deal Tracker"
                                    ]
                                    Html.p [
                                        prop.className "text-sm opacity-70"
                                        prop.text "Black Friday 2025 Laptop Deals"
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]

            Html.div [
                prop.className "flex-none gap-2"
                prop.children [
                    // Last Updated
                    match lastUpdated with
                    | Some dt ->
                        Html.div [
                            prop.className "hidden sm:flex flex-col items-end mr-4"
                            prop.children [
                                Html.div [
                                    prop.className "text-xs opacity-70"
                                    prop.text "Last Updated"
                                ]
                                Html.div [
                                    prop.className "text-sm font-semibold"
                                    prop.text (dt.ToString("MMM dd, HH:mm"))
                                ]
                            ]
                        ]
                    | None -> Html.none

                    // Refresh Button
                    Html.button [
                        prop.className $"btn btn-primary {if isLoading then "loading" else ""}"
                        prop.onClick (fun _ -> onRefresh())
                        prop.disabled isLoading
                        prop.children [
                            if not isLoading then
                                Html.text "🔄 Refresh"
                            else
                                Html.text "Loading..."
                        ]
                    ]
                ]
            ]
        ]
    ]
