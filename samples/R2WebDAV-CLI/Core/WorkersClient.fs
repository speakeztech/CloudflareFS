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
                // Serialize bindings to JSON
                // Note: We have to manually serialize because Hawaii's generated DUs don't serialize correctly
                // This is a known limitation - Hawaii doesn't support OpenAPI discriminator schemas yet
                let bindingsJson =
                    bindings
                    |> List.map (fun binding ->
                        match binding with
                        | Types.workersbindingitem.R2bucket r2 ->
                            // R2 buckets need bucket_name, name, and type fields
                            let jurisdictionPart =
                                match r2.jurisdiction with
                                | Some j -> sprintf ",\"jurisdiction\":\"%s\"" (j.ToString().ToLowerInvariant())
                                | None -> ""
                            sprintf "{\"bucket_name\":\"%s\",\"name\":\"%s\",\"type\":\"%s\"%s}"
                                r2.bucket_name r2.name (r2.``type``.Format()) jurisdictionPart
                        | _ ->
                            // For other types, this is a TODO - we'd need similar handling
                            // For now, just fail with a clear message
                            failwith $"Binding type {binding} not yet supported - only R2 buckets are currently implemented")
                    |> String.concat ","

                // PATCH settings with new bindings using multipart/form-data
                let settingsBody = sprintf """{"bindings":[%s]}""" bindingsJson
                let patchUrl = $"https://api.cloudflare.com/client/v4/accounts/{config.AccountId}/workers/scripts/{scriptName}/settings"

                use formData = new MultipartFormDataContent()
                formData.Add(new StringContent(settingsBody), "settings")

                let patchRequest = new HttpRequestMessage(new HttpMethod("PATCH"), patchUrl)
                patchRequest.Content <- formData

                let! patchResponse = httpClient.SendAsync(patchRequest) |> Async.AwaitTask
                let! patchContent = patchResponse.Content.ReadAsStringAsync() |> Async.AwaitTask

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
                // Create the secret body with correct field names
                let secretBodyJson = sprintf """{"name":"%s","text":"%s","type":"secret_text"}""" secretName secretValue

                // Use absolute URL to avoid BaseAddress issues
                let url = $"https://api.cloudflare.com/client/v4/accounts/{config.AccountId}/workers/scripts/{scriptName}/secrets"

                use requestContent = new StringContent(secretBodyJson, Text.Encoding.UTF8, "application/json")
                let! response = httpClient.PutAsync(url, requestContent) |> Async.AwaitTask
                let! responseBody = response.Content.ReadAsStringAsync() |> Async.AwaitTask

                // Parse the JSON response using System.Text.Json
                use jsonDoc = JsonDocument.Parse(responseBody)
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
                        return Error responseBody
            with ex ->
                return Error $"Exception setting secret: {ex.Message}"
        }

    member this.DeleteSecret(scriptName: string, secretName: string) : Async<Result<unit, string>> =
        async {
            try
                let url = $"https://api.cloudflare.com/client/v4/accounts/{config.AccountId}/workers/scripts/{scriptName}/secrets/{secretName}"

                let! response = httpClient.DeleteAsync(url) |> Async.AwaitTask
                let! responseBody = response.Content.ReadAsStringAsync() |> Async.AwaitTask

                use jsonDoc = JsonDocument.Parse(responseBody)
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
                        return Error responseBody
            with ex ->
                return Error $"Exception deleting secret: {ex.Message}"
        }

    member this.RemoveBinding(scriptName: string, bindingName: string) : Async<Result<unit, string>> =
        async {
            try
                // Get current settings to find existing bindings
                let getUrl = $"https://api.cloudflare.com/client/v4/accounts/{config.AccountId}/workers/scripts/{scriptName}/settings"
                let! getResponse = httpClient.GetAsync(getUrl) |> Async.AwaitTask
                let! settingsContent = getResponse.Content.ReadAsStringAsync() |> Async.AwaitTask

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
                let patchUrl = $"https://api.cloudflare.com/client/v4/accounts/{config.AccountId}/workers/scripts/{scriptName}/settings"

                use formData = new MultipartFormDataContent()
                formData.Add(new StringContent(settingsBody), "settings")

                let patchRequest = new HttpRequestMessage(new HttpMethod("PATCH"), patchUrl)
                patchRequest.Content <- formData

                let! patchResponse = httpClient.SendAsync(patchRequest) |> Async.AwaitTask
                let! patchContent = patchResponse.Content.ReadAsStringAsync() |> Async.AwaitTask

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
