module R2WebDAV.CLI.WorkersClient

open System
open System.Net.Http
open System.Text.Json
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
                use jsonDoc = JsonDocument.Parse(patchContent)
                let json = jsonDoc.RootElement
                let mutable successProp = Unchecked.defaultof<JsonElement>
                if json.TryGetProperty("success", &successProp) && successProp.GetBoolean() then
                    return Ok ()
                else
                    let mutable errorsProp = Unchecked.defaultof<JsonElement>
                    if json.TryGetProperty("errors", &errorsProp) && errorsProp.ValueKind = JsonValueKind.Array then
                        let errorMessages =
                            errorsProp.EnumerateArray()
                            |> Seq.choose (fun e ->
                                let mutable msgProp = Unchecked.defaultof<JsonElement>
                                if e.TryGetProperty("message", &msgProp) then
                                    Some (msgProp.GetString())
                                else
                                    None)
                            |> String.concat "; "
                        return Error errorMessages
                    else
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

                // Parse the JSON response using System.Text.Json
                use jsonDoc = JsonDocument.Parse(content)
                let json = jsonDoc.RootElement

                let mutable successProp = Unchecked.defaultof<JsonElement>
                if json.TryGetProperty("success", &successProp) && successProp.GetBoolean() then
                    return Ok ()
                else
                    // Extract error messages if present
                    let mutable errorsProp = Unchecked.defaultof<JsonElement>
                    if json.TryGetProperty("errors", &errorsProp) && errorsProp.ValueKind = JsonValueKind.Array then
                        let errorMessages =
                            errorsProp.EnumerateArray()
                            |> Seq.choose (fun e ->
                                let mutable msgProp = Unchecked.defaultof<JsonElement>
                                if e.TryGetProperty("message", &msgProp) then
                                    Some (msgProp.GetString())
                                else
                                    None)
                            |> String.concat "; "
                        return Error errorMessages
                    else
                        return Error content
            with ex ->
                return Error $"Exception setting secret: {ex.Message}"
        }

    member this.DeleteSecret(scriptName: string, secretName: string) : Async<Result<unit, string>> =
        async {
            try
                let requestParts = [
                    Http.RequestPart.path("account_id", config.AccountId)
                    Http.RequestPart.path("script_name", scriptName)
                    Http.RequestPart.path("secret_name", secretName)
                ]

                let! (status, content) =
                    Http.OpenApiHttp.deleteAsync
                        httpClient
                        "/accounts/{account_id}/workers/scripts/{script_name}/secrets/{secret_name}"
                        requestParts
                        None

                use jsonDoc = JsonDocument.Parse(content)
                let json = jsonDoc.RootElement

                let mutable successProp = Unchecked.defaultof<JsonElement>
                if json.TryGetProperty("success", &successProp) && successProp.GetBoolean() then
                    return Ok ()
                else
                    let mutable errorsProp = Unchecked.defaultof<JsonElement>
                    if json.TryGetProperty("errors", &errorsProp) && errorsProp.ValueKind = JsonValueKind.Array then
                        let errorMessages =
                            errorsProp.EnumerateArray()
                            |> Seq.choose (fun e ->
                                let mutable msgProp = Unchecked.defaultof<JsonElement>
                                if e.TryGetProperty("message", &msgProp) then
                                    Some (msgProp.GetString())
                                else
                                    None)
                            |> String.concat "; "
                        return Error errorMessages
                    else
                        return Error content
            with ex ->
                return Error $"Exception deleting secret: {ex.Message}"
        }

    member this.RemoveBinding(scriptName: string, bindingName: string) : Async<Result<unit, string>> =
        async {
            try
                // Get current settings to find existing bindings
                let! settingsResult =
                    Http.OpenApiHttp.getAsync
                        httpClient
                        "/accounts/{account_id}/workers/scripts/{script_name}/settings"
                        [Http.RequestPart.path("account_id", config.AccountId)
                         Http.RequestPart.path("script_name", scriptName)]
                        None

                let (_, settingsContent) = settingsResult
                use settingsJsonDoc = JsonDocument.Parse(settingsContent)
                let settingsJson = settingsJsonDoc.RootElement

                // Extract current bindings and filter out the one to remove
                let filteredBindingsJson =
                    let mutable resultProp = Unchecked.defaultof<JsonElement>
                    if settingsJson.TryGetProperty("result", &resultProp) then
                        let mutable bindingsProp = Unchecked.defaultof<JsonElement>
                        if resultProp.TryGetProperty("bindings", &bindingsProp) && bindingsProp.ValueKind = JsonValueKind.Array then
                            bindingsProp.EnumerateArray()
                            |> Seq.filter (fun binding ->
                                let mutable nameProp = Unchecked.defaultof<JsonElement>
                                if binding.TryGetProperty("name", &nameProp) then
                                    nameProp.GetString() <> bindingName
                                else
                                    true)
                            |> Seq.map (fun b -> b.GetRawText())
                            |> String.concat ","
                        else
                            ""
                    else
                        ""

                // PATCH settings with filtered bindings
                let settingsBody = sprintf """{"bindings":[%s]}""" filteredBindingsJson

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

                use jsonDoc = JsonDocument.Parse(patchContent)
                let json = jsonDoc.RootElement
                let mutable successProp = Unchecked.defaultof<JsonElement>
                if json.TryGetProperty("success", &successProp) && successProp.GetBoolean() then
                    return Ok ()
                else
                    let mutable errorsProp = Unchecked.defaultof<JsonElement>
                    if json.TryGetProperty("errors", &errorsProp) && errorsProp.ValueKind = JsonValueKind.Array then
                        let errorMessages =
                            errorsProp.EnumerateArray()
                            |> Seq.choose (fun e ->
                                let mutable msgProp = Unchecked.defaultof<JsonElement>
                                if e.TryGetProperty("message", &msgProp) then
                                    Some (msgProp.GetString())
                                else
                                    None)
                            |> String.concat "; "
                        return Error errorMessages
                    else
                        return Error patchContent
            with ex ->
                return Error $"Exception removing binding: {ex.Message}"
        }
