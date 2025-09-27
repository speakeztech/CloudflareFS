module SecureChat.UI.App

open Feliz
open Browser.Dom
open SecureChat.UI.ThemeContext
open SecureChat.UI.Api
open SecureChat.UI.Components

[<ReactComponent>]
let App () =
    let theme, setTheme = React.useState(getStoredTheme())
    let user, setUser = React.useState(None)

    // Apply theme on mount
    React.useEffectOnce(fun () ->
        applyTheme theme
    )

    let handleLogin (response: LoginResponse) =
        setUser (Some response)

    let handleLogout () =
        setUser None

    React.contextProvider(themeContext, (theme, setTheme), [
        Html.div [
            match user with
            | None ->
                Login.Login handleLogin
            | Some u ->
                ChatRoom.ChatRoom u.token u.username handleLogout
        ]
    ])

// Mount the app
ReactDOM.createRoot(document.getElementById("root"))
    .render(App())