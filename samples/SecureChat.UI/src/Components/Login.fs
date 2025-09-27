module SecureChat.UI.Components.Login

open Feliz
open SecureChat.UI.Api

[<ReactComponent>]
let Login (onLogin: LoginResponse -> unit) =
    let username, setUsername = React.useState("")
    let password, setPassword = React.useState("")
    let error, setError = React.useState(None)
    let loading, setLoading = React.useState(false)

    let handleSubmit (e: Browser.Types.Event) =
        e.preventDefault()
        setLoading true
        setError None

        promise {
            let! result = login { username = username; password = password }
            match result with
            | Ok response ->
                onLogin response
            | Error msg ->
                setError (Some msg)
                setLoading false
        }
        |> Promise.start

    Html.div [
        prop.className "min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800"
        prop.children [
            Html.div [
                prop.className "bg-white dark:bg-gray-800 p-8 rounded-xl shadow-2xl w-full max-w-md"
                prop.children [
                    Html.h1 [
                        prop.className "text-3xl font-bold text-center mb-8 text-gray-900 dark:text-white"
                        prop.text "SecureChat"
                    ]

                    Html.form [
                        prop.onSubmit handleSubmit
                        prop.className "space-y-6"
                        prop.children [
                            Html.div [
                                Html.label [
                                    prop.className "block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2"
                                    prop.text "Username"
                                ]
                                Html.input [
                                    prop.type'.text
                                    prop.value username
                                    prop.onChange setUsername
                                    prop.required true
                                    prop.className "w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
                                    prop.placeholder "Enter your username"
                                ]
                            ]

                            Html.div [
                                Html.label [
                                    prop.className "block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2"
                                    prop.text "Password"
                                ]
                                Html.input [
                                    prop.type'.password
                                    prop.value password
                                    prop.onChange setPassword
                                    prop.required true
                                    prop.className "w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
                                    prop.placeholder "Enter your password"
                                ]
                            ]

                            match error with
                            | Some msg ->
                                Html.div [
                                    prop.className "bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 text-red-600 dark:text-red-400 px-4 py-3 rounded-lg"
                                    prop.text msg
                                ]
                            | None -> Html.none

                            Html.button [
                                prop.type'.submit
                                prop.disabled loading
                                prop.className (
                                    if loading then
                                        "w-full bg-gray-400 text-white py-3 px-4 rounded-lg font-medium cursor-not-allowed"
                                    else
                                        "w-full bg-blue-600 hover:bg-blue-700 text-white py-3 px-4 rounded-lg font-medium transition-colors"
                                )
                                prop.text (if loading then "Logging in..." else "Login")
                            ]
                        ]
                    ]

                    Html.p [
                        prop.className "mt-6 text-center text-sm text-gray-600 dark:text-gray-400"
                        prop.text "Users must be created by administrators"
                    ]
                ]
            ]
        ]
    ]