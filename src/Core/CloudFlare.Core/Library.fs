namespace CloudFlare.Core

open System

module Environment =
    let isDevelopment () =
        let env = System.Environment.GetEnvironmentVariable("ENVIRONMENT")
        env = "development" || env = "dev" || String.IsNullOrEmpty(env)

    let isProduction () =
        let env = System.Environment.GetEnvironmentVariable("ENVIRONMENT")
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
