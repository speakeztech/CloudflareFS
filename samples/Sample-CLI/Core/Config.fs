module Sample.CLI.Config

open System

type CloudflareConfig = {
    ApiToken: string
    AccountId: string
    WorkerName: string
}

let loadConfig () : Result<CloudflareConfig, string> =
    // Load from environment variables
    let getEnv name =
        match Environment.GetEnvironmentVariable(name) with
        | null | "" -> None
        | value -> Some value

    match getEnv "CLOUDFLARE_API_TOKEN", getEnv "CLOUDFLARE_ACCOUNT_ID" with
    | Some token, Some accountId ->
        let workerName =
            getEnv "CLOUDFLARE_WORKER_NAME"
            |> Option.defaultValue "r2-webdav-fsharp"

        Ok {
            ApiToken = token
            AccountId = accountId
            WorkerName = workerName
        }
    | None, _ -> Error "CLOUDFLARE_API_TOKEN environment variable not set"
    | _, None -> Error "CLOUDFLARE_ACCOUNT_ID environment variable not set"
