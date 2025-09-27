module SecureChat.UI.Components.ChatRoom

open Feliz
open SecureChat.UI.Api
open SecureChat.UI.Components

[<ReactComponent>]
let ChatRoom (token: string) (username: string) (onLogout: unit -> unit) =
    let messages, setMessages = React.useState([||])
    let currentRoom, setCurrentRoom = React.useState("general")
    let rooms, setRooms = React.useState([
        { id = "general"; name = "General" }
        { id = "tech"; name = "Tech" }
        { id = "random"; name = "Random" }
    ])

    let loadMessages () =
        promise {
            let! result = getMessages token currentRoom
            match result with
            | Ok msgs -> setMessages msgs
            | Error _ -> ()
        }
        |> Promise.start

    let sendMessage (content: string) (encrypted: bool) =
        promise {
            let! success = sendMessage token currentRoom content encrypted
            if success then
                loadMessages()
        }
        |> Promise.start

    let createNewRoom () =
        let roomName = Browser.Dom.window.prompt("Enter room name:")
        if roomName <> null && roomName.Trim() <> "" then
            promise {
                let! result = createRoom token roomName
                match result with
                | Ok room ->
                    setRooms (Array.append rooms [| room |])
                    setCurrentRoom room.id
                | Error _ -> ()
            }
            |> Promise.start

    // Load messages on mount and when room changes
    React.useEffect((fun () ->
        loadMessages()
        let interval = Browser.Dom.window.setInterval(loadMessages, 3000)
        React.createDisposable(fun () ->
            Browser.Dom.window.clearInterval(interval)
        )
    ), [| currentRoom |])

    Html.div [
        prop.className "flex h-screen bg-gray-50 dark:bg-gray-900"
        prop.children [
            // Sidebar
            Html.div [
                prop.className "w-64 bg-white dark:bg-gray-800 border-r dark:border-gray-700"
                prop.children [
                    Html.div [
                        prop.className "p-4 border-b dark:border-gray-700"
                        prop.children [
                            Html.h2 [
                                prop.className "text-lg font-semibold text-gray-900 dark:text-white"
                                prop.text "Rooms"
                            ]
                            Html.button [
                                prop.onClick (fun _ -> createNewRoom())
                                prop.className "mt-2 w-full px-3 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium transition-colors"
                                prop.text "+ New Room"
                            ]
                        ]
                    ]

                    Html.div [
                        prop.className "p-2"
                        prop.children [
                            for room in rooms do
                                Html.button [
                                    prop.onClick (fun _ -> setCurrentRoom room.id)
                                    prop.className (
                                        let baseClasses = "w-full px-3 py-2 rounded-lg text-left transition-colors"
                                        if room.id = currentRoom then
                                            $"{baseClasses} bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-400"
                                        else
                                            $"{baseClasses} hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300"
                                    )
                                    prop.text $"# {room.name}"
                                ]
                        ]
                    ]
                ]
            ]

            // Chat area
            Html.div [
                prop.className "flex-1 flex flex-col"
                prop.children [
                    // Header
                    Html.div [
                        prop.className "bg-white dark:bg-gray-800 border-b dark:border-gray-700 px-6 py-4"
                        prop.children [
                            Html.div [
                                prop.className "flex items-center justify-between"
                                prop.children [
                                    Html.h1 [
                                        prop.className "text-xl font-semibold text-gray-900 dark:text-white"
                                        prop.text (
                                            let room = rooms |> Array.tryFind (fun r -> r.id = currentRoom)
                                            match room with
                                            | Some r -> $"# {r.name}"
                                            | None -> "# general"
                                        )
                                    ]
                                    Html.div [
                                        prop.className "flex items-center space-x-4"
                                        prop.children [
                                            Html.span [
                                                prop.className "text-sm text-gray-600 dark:text-gray-400"
                                                prop.text $"Logged in as {username}"
                                            ]
                                            ThemeToggle.ThemeToggle()
                                            Html.button [
                                                prop.onClick (fun _ -> onLogout())
                                                prop.className "px-4 py-2 bg-gray-200 dark:bg-gray-700 hover:bg-gray-300 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-300 rounded-lg text-sm font-medium transition-colors"
                                                prop.text "Logout"
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]

                    // Messages
                    Html.div [
                        prop.className "flex-1 bg-white dark:bg-gray-800"
                        prop.children [
                            MessageList.MessageList messages username
                        ]
                    ]

                    // Input
                    MessageInput.MessageInput sendMessage
                ]
            ]
        ]
    ]