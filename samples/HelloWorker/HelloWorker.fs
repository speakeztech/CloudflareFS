// Example Cloudflare Worker using CloudflareFS bindings
module HelloWorker

open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.Worker.Context
open CloudFlare.Worker.Context.Globals
open CloudFlare.Worker.Context.Helpers
open CloudFlare.Worker.Context.Helpers.ResponseBuilder

// Main fetch handler
let fetch (request: Request) (env: Env) (ctx: ExecutionContext) =
    // Parse the request using wrapper functions
    let path = Request.getPath request
    let method = Request.getMethod request

    // Route handling
    let response =
        match method, path with
        | "GET", "/" ->
            // Simple response using helper
            ok "Hello from CloudflareFS! The wrapper functions hide the backticked bindings."

        | "GET", "/json" ->
            // JSON response using helper
            json {|
                message = "Hello from F# + Fable + Cloudflare Workers"
                timestamp = System.DateTime.Now
                path = path
            |} 200

        | "GET", "/headers" ->
            // Custom headers using computation expression
            let hdrs =
                headers {
                    contentType "application/json"
                    cors
                    set "X-Powered-By" "CloudflareFS"
                    set "X-Custom-Header" "F# Wrapper Functions"
                }
            Response.Create(U2.Case1 """{"message": "Headers example"}""", jsOptions(fun o ->
                o.headers <- Some (U2.Case2 hdrs)
                o.status <- Some 200.0
            ))

        | "GET", "/redirect" ->
            // Redirect example
            redirect "https://github.com/speakez/CloudflareFS"

        | "GET", "/request-info" ->
            // Show request information
            let url = Request.getUrl request
            let info = {|
                method = method
                url = url
                path = path
                headers = request.headers?get("User-Agent") |> Option.ofObj
            |}
            json info 200

        | _ ->
            // 404 for unknown routes
            let notFoundInfo = {|
                error = "Not Found"
                path = path
                availableRoutes = ["/"; "/json"; "/headers"; "/redirect"; "/request-info"]
            |}
            json notFoundInfo 404

    response

// Export the handler using ExportDefault attribute
[<ExportDefault>]
let handler: obj = {| fetch = fetch |} :> obj