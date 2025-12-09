namespace CloudFlare.Core

open System
open Fable.Core
open Fable.Core.JsInterop

module Environment =
    // Access process.env in Node.js/Cloudflare Workers environment
    [<Emit("(typeof process !== 'undefined' && process.env && process.env[$0]) || ''")>]
    let private getEnvVar (name: string) : string = jsNative

    let isDevelopment () =
        let env = getEnvVar "ENVIRONMENT"
        env = "development" || env = "dev" || String.IsNullOrEmpty(env)

    let isProduction () =
        let env = getEnvVar "ENVIRONMENT"
        env = "production" || env = "prod"

module Version =
    let current = "0.1.0"

module Configuration =
    let private config = System.Collections.Generic.Dictionary<string, string>()

    let set (key: string) (value: string) =
        config.[key] <- value

    let tryGet (key: string) =
        match config.TryGetValue(key) with
        | true, value -> Some value
        | false, _ -> None

    let get (key: string) =
        match tryGet key with
        | Some value -> value
        | None -> failwithf "Configuration key '%s' not found" key

    let getOrDefault (key: string) (defaultValue: string) =
        match tryGet key with
        | Some value -> value
        | None -> defaultValue
