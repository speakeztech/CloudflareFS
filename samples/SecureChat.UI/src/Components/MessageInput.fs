module SecureChat.UI.Components.MessageInput

open Feliz

[<ReactComponent>]
let MessageInput (onSend: string -> bool -> unit) =
    let message, setMessage = React.useState("")
    let encrypt, setEncrypt = React.useState(false)

    let handleSubmit (e: Browser.Types.Event) =
        e.preventDefault()
        if message.Trim() <> "" then
            onSend message encrypt
            setMessage ""

    Html.form [
        prop.onSubmit handleSubmit
        prop.className "border-t dark:border-gray-700 p-4"
        prop.children [
            Html.div [
                prop.className "flex items-center space-x-2"
                prop.children [
                    Html.input [
                        prop.type'.text
                        prop.value message
                        prop.onChange setMessage
                        prop.placeholder "Type a message..."
                        prop.className "flex-1 px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
                    ]

                    Html.label [
                        prop.className "flex items-center space-x-2 cursor-pointer"
                        prop.children [
                            Html.input [
                                prop.type'.checkbox
                                prop.checked encrypt
                                prop.onChange (fun _ -> setEncrypt (not encrypt))
                                prop.className "w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500 dark:focus:ring-blue-600 dark:ring-offset-gray-800 focus:ring-2 dark:bg-gray-700 dark:border-gray-600"
                            ]
                            Html.span [
                                prop.className "text-sm text-gray-700 dark:text-gray-300"
                                prop.text "Encrypt"
                            ]
                        ]
                    ]

                    Html.button [
                        prop.type'.submit
                        prop.disabled (message.Trim() = "")
                        prop.className (
                            if message.Trim() = "" then
                                "px-6 py-2 bg-gray-400 text-white rounded-lg font-medium cursor-not-allowed"
                            else
                                "px-6 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors"
                        )
                        prop.text "Send"
                    ]
                ]
            ]
        ]
    ]