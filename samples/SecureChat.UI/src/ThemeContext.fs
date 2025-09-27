module SecureChat.UI.ThemeContext

open Fable.Core
open Feliz

type Theme = Light | Dark

let private themeKey = "securechat-theme"

let getStoredTheme () =
    Browser.Dom.window.localStorage.getItem(themeKey)
    |> Option.ofObj
    |> Option.map (function
        | "light" -> Light
        | _ -> Dark)
    |> Option.defaultValue Dark

let storeTheme (theme: Theme) =
    let value = match theme with Light -> "light" | Dark -> "dark"
    Browser.Dom.window.localStorage.setItem(themeKey, value)

let applyTheme (theme: Theme) =
    let classList = Browser.Dom.document.documentElement.classList
    match theme with
    | Dark ->
        classList.add("dark")
    | Light ->
        classList.remove("dark")

let themeContext = React.createContext<Theme * (Theme -> unit)>(Dark, ignore)