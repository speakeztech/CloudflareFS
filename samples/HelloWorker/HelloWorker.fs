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

    let statement = "Hello from F# + Fable + Cloudflare Workers"

    // Route handling
    let response =
        match method, path with
        | "GET", "/" ->
            // HTML info page in dark mode
            let url = Request.getUrl request
            let html = """
<!DOCTYPE html>
<html style="background: #000;">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>HelloWorker - F# + Cloudflare</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        html, body { background: #000; color: #e0e0e0; }
        body { font-family: system-ui, -apple-system, sans-serif; max-width: 800px; margin: 50px auto; padding: 0 20px; line-height: 1.6; }
        h1 { color: #f38020; margin-bottom: 10px; }
        h2 { color: #e0e0e0; margin-top: 30px; margin-bottom: 15px; }
        p { margin: 10px 0; }
        code { background: #1a1a1a; color: #f38020; padding: 2px 8px; border-radius: 4px; border: 1px solid #333; font-family: 'Courier New', monospace; }
        .route { background: #1a1a1a; padding: 12px 15px; border-radius: 5px; margin: 8px 0; border: 1px solid #333; display: flex; align-items: center; }
        .route code { margin: 0 8px 0 0; }
        .route span { color: #999; }
        ul { margin: 15px 0; padding: 0; list-style: none; }
        li { margin: 0; }
        a { color: #2196F3; text-decoration: none; }
        a:hover { text-decoration: underline; }
        .footer { color: #666; font-size: 0.9em; margin-top: 40px; padding-top: 20px; border-top: 1px solid #333; }
    </style>
</head>
<body>
    <h1>👋 HelloWorker</h1>
    <p>A simple Cloudflare Worker written in F# using CloudflareFS bindings.</p>

    <h2>Available Routes</h2>
    <ul>
        <li><div class="route"><code>GET /</code><span>This page</span></div></li>
        <li><div class="route"><code>GET /json</code><span>JSON response with timestamp</span></div></li>
        <li><div class="route"><code>GET /headers</code><span>Custom headers example</span></div></li>
        <li><div class="route"><code>GET /redirect</code><span>Redirect to GitHub</span></div></li>
        <li><div class="route"><code>GET /request-info</code><span>Request information</span></div></li>
    </ul>

    <h2>Features</h2>
    <ul>
        <li><div class="route">✨ F# + Fable compilation to JavaScript</div></li>
        <li><div class="route">⚡ Cloudflare Workers edge computing</div></li>
        <li><div class="route">🔧 Type-safe bindings via CloudflareFS</div></li>
    </ul>

    <div class="footer">
        Powered by <a href="https://github.com/speakeztech/CloudflareFS">CloudflareFS</a> - F# bindings for Cloudflare Workers
    </div>
</body>
</html>"""
            Response.Create(U2.Case1 html, jsOptions(fun o ->
                o.headers <- Some (U2.Case1 (createObj ["Content-Type" ==> "text/html; charset=utf-8"]))
                o.status <- Some 200.0
            ))

        | "GET", "/json" ->
            // JSON response using helper
            json {|
                message = statement
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
            redirect "https://github.com/speakeztech/CloudflareFS"

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