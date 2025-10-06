module R2WebDAV.CLI.WorkersClient

open System
open System.Net.Http
open FSharp.Data
open CloudFlare.Api.Compute.Workers
open R2WebDAV.CLI.Config

type WorkersOperations(config: CloudflareConfig) =
    let httpClient = new HttpClient()

    do
        httpClient.BaseAddress <- Uri("https://api.cloudflare.com/client/v4")
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiToken}")

    member this.UpdateWorkerBindings(scriptName: string, bindings: Types.workersbindingitem list) : Async<Result<unit, string>> =
        async {
            try
                // Build bindings JSON - manually construct to avoid discriminated union serialization issues
                let bindingsJson =
                    bindings
                    |> List.map (fun binding ->
                        match binding with
                        | Types.workersbindingitem.R2bucket r2 ->
                            let jurisdictionPart =
                                match r2.jurisdiction with
                                | Some j -> sprintf ""","jurisdiction":"%s" """ (j.Format())
                                | None -> ""
                            sprintf """{"type":"%s","name":"%s","bucket_name":"%s"%s}"""
                                (r2.``type``.Format()) r2.name r2.bucket_name jurisdictionPart
                        | _ -> failwith "Only R2 bucket bindings are currently supported")
                    |> String.concat ","

                // PATCH settings with new bindings using multipart/form-data
                let settingsBody = sprintf """{"bindings":[%s]}""" bindingsJson

                let requestParts = [
                    Http.RequestPart.path("account_id", config.AccountId)
                    Http.RequestPart.path("script_name", scriptName)
                    Http.RequestPart.multipartFormData("settings", settingsBody)
                ]

                let! (patchStatus, patchContent) =
                    Http.OpenApiHttp.patchAsync
                        httpClient
                        "/accounts/{account_id}/workers/scripts/{script_name}/settings"
                        requestParts
                        None

                // Parse response
                let json = JsonValue.Parse(patchContent)
                match json.TryGetProperty("success") with
                | Some success when success.AsBoolean() ->
                    return Ok ()
                | _ ->
                    match json.TryGetProperty("errors") with
                    | Some (JsonValue.Array errors) when errors.Length > 0 ->
                        let errorMessages =
                            errors
                            |> Array.choose (fun e ->
                                match e.TryGetProperty("message") with
                                | Some msg -> Some (msg.AsString())
                                | None -> None)
                            |> String.concat "; "
                        return Error errorMessages
                    | _ ->
                        return Error patchContent
            with ex ->
                return Error $"Exception updating worker bindings: {ex.Message}"
        }

    member this.PutSecret(scriptName: string, secretName: string, secretValue: string) : Async<Result<unit, string>> =
        async {
            try
                // Create the secret body - use secret_text type
                let secretBody =
                    Types.workersbindingkindsecrettext.Create(
                        secretName,
                        secretValue,
                        Types.workersbindingkindsecrettextType.Secret_text
                    )

                // Call the API directly to avoid the broken Serializer.deserialize<string> issue
                let requestParts = [
                    Http.RequestPart.path("account_id", config.AccountId)
                    Http.RequestPart.path("script_name", scriptName)
                    Http.RequestPart.jsonContent secretBody
                ]

                let! (status, content) =
                    Http.OpenApiHttp.putAsync
                        httpClient
                        "/accounts/{account_id}/workers/scripts/{script_name}/secrets"
                        requestParts
                        None

                // Parse the JSON response using FSharp.Data
                let json = JsonValue.Parse(content)

                match json.TryGetProperty("success") with
                | Some success when success.AsBoolean() ->
                    return Ok ()
                | _ ->
                    // Extract error messages if present
                    match json.TryGetProperty("errors") with
                    | Some (JsonValue.Array errors) when errors.Length > 0 ->
                        let errorMessages =
                            errors
                            |> Array.choose (fun e ->
                                match e.TryGetProperty("message") with
                                | Some msg -> Some (msg.AsString())
                                | None -> None)
                            |> String.concat "; "
                        return Error errorMessages
                    | _ ->
                        return Error content
            with ex ->
                return Error $"Exception setting secret: {ex.Message}"
        }
