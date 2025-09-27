module SecureChat.UI.Components.MessageList

open Feliz
open SecureChat.UI.Api

[<ReactComponent>]
let MessageList (messages: Message[]) (currentUser: string) =
    let messagesEndRef = React.useRef(None)

    React.useEffect((fun () ->
        messagesEndRef.current
        |> Option.iter (fun el ->
            el?scrollIntoView({| behavior = "smooth" |}) |> ignore
        )
    ), [| messages |])

    Html.div [
        prop.className "flex-1 overflow-y-auto p-4 space-y-4"
        prop.children [
            for msg in messages do
                let isOwn = msg.username = currentUser

                Html.div [
                    prop.className (
                        if isOwn then
                            "flex justify-end"
                        else
                            "flex justify-start"
                    )
                    prop.children [
                        Html.div [
                            prop.className (
                                let baseClasses = "max-w-xs lg:max-w-md px-4 py-2 rounded-lg shadow"
                                if isOwn then
                                    $"{baseClasses} bg-blue-600 text-white"
                                else
                                    $"{baseClasses} bg-gray-200 dark:bg-gray-700 text-gray-900 dark:text-white"
                            )
                            prop.children [
                                Html.div [
                                    prop.className "flex items-center justify-between mb-1"
                                    prop.children [
                                        Html.span [
                                            prop.className (
                                                if isOwn then
                                                    "text-xs text-blue-100"
                                                else
                                                    "text-xs text-gray-500 dark:text-gray-400"
                                            )
                                            prop.text msg.username
                                        ]
                                        if msg.encrypted then
                                            Html.span [
                                                prop.className "ml-2"
                                                prop.title "Encrypted message"
                                                prop.text "🔒"
                                            ]
                                    ]
                                ]
                                Html.p [
                                    prop.className "text-sm"
                                    prop.text msg.content
                                ]
                                Html.div [
                                    prop.className (
                                        if isOwn then
                                            "text-xs text-blue-100 mt-1"
                                        else
                                            "text-xs text-gray-500 dark:text-gray-400 mt-1"
                                    )
                                    prop.text (
                                        let date = System.DateTime(msg.timestamp)
                                        date.ToString("HH:mm")
                                    )
                                ]
                            ]
                        ]
                    ]
                ]

            Html.div [
                prop.ref messagesEndRef
            ]
        ]
    ]