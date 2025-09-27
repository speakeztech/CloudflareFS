// Example Cloudflare Worker using CloudflareFS bindings
module HelloWorker

open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.Worker.Context.Generated
open CloudFlare.Worker.Context.Helpers
open CloudFlare.Worker.Context.Helpers.ResponseBuilder

// Example KV operations
let handleKV (kv: KVNamespace) (key: string) = async {
    // Get value from KV
    let! value = KV.get key kv

    match value with
    | Some v ->
        return ok $"Found value: {v}"
    | None ->
        // Store a new value
        let newValue = $"Created at {System.DateTime.Now}"
        do! KV.putWithTtl key newValue 3600 kv
        return ok $"Stored new value: {newValue}"
}

// Main fetch handler
let fetch (request: Request) (env: Env) (ctx: ExecutionContext) =
    // Parse the request
    let path = Request.getPath request
    let method = Request.getMethod request

    // Route handling
    let response =
        match method, path with
        | "GET", "/" ->
            // Simple response
            ok "Hello from CloudflareFS!"

        | "GET", "/json" ->
            // JSON response
            json {| message = "Hello"; timestamp = System.DateTime.Now |} 200

        | "GET", "/headers" ->
            // Custom headers
            let hdrs =
                headers {
                    contentType "text/plain"
                    cors
                    set "X-Custom-Header" "CloudflareFS"
                }
            Response.Create(U2.Case1 "Headers example", jsOptions(fun o ->
                o.headers <- Some (U2.Case2 hdrs)
                o.status <- Some 200.0
            ))

        | "GET", path when path.StartsWith("/kv/") ->
            // KV example (if MY_KV binding exists)
            match env?MY_KV with
            | null -> serverError "KV namespace not configured"
            | kv ->
                let key = path.Substring(4)
                let promise = handleKV (kv :?> KVNamespace) key |> Async.StartAsPromise
                ctx.waitUntil(promise |> unbox)
                promise |> unbox

        | "GET", "/redirect" ->
            // Redirect example
            redirect "https://github.com/speakeztech/CloudflareFS"

        | _ ->
            // 404 for unknown routes
            notFound()

    response

// Export the handler
[<Export("default")>]
let handler = {| fetch = fetch |}