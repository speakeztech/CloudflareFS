module R2WebDAV.Main

open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.Worker.Context
open CloudFlare.Worker.Context.Globals
open CloudFlare.Worker.Context.Helpers
open R2WebDAV.Auth
open R2WebDAV.WebDav

[<Emit("new URL($0)")>]
let createURL(url: string) : obj = jsNative

/// Add CORS headers to response
let addCorsHeaders (response: Response) : Response =
    response.headers.set("Access-Control-Allow-Origin", "*")
    response.headers.set("Access-Control-Allow-Methods", "GET, HEAD, PUT, DELETE, OPTIONS, PROPFIND, MKCOL, COPY, MOVE")
    response.headers.set("Access-Control-Allow-Headers", "authorization, content-type, depth, overwrite, destination, range")
    response.headers.set("Access-Control-Expose-Headers", "content-type, content-length, dav, etag, last-modified, location, date, content-range")
    response.headers.set("Access-Control-Allow-Credentials", "false")
    response.headers.set("Access-Control-Max-Age", "86400")
    response

/// Main fetch handler
let fetch (request: Request) (env: Env) (ctx: ExecutionContext) : JS.Promise<Response> =
    promise {
        // Check if request is for the WebDAV API path
        let url = Request.getUrl request
        let urlObj: obj = createURL(url)
        let pathname: string = urlObj?pathname

        if API_PREFIX <> "" && not (pathname.StartsWith(API_PREFIX)) then
            // Return helpful info page for root path
            let html = """
<!DOCTYPE html>
<html style="background: #000;">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>R2 WebDAV Server</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        html, body { background: #000; color: #e0e0e0; }
        body { font-family: system-ui, -apple-system, sans-serif; max-width: 800px; margin: 50px auto; padding: 0 20px; line-height: 1.6; }
        h1 { color: #f38020; margin-bottom: 10px; }
        h2 { color: #e0e0e0; margin-top: 30px; margin-bottom: 15px; }
        p { margin: 10px 0; }
        code { background: #1a1a1a; color: #f38020; padding: 2px 8px; border-radius: 4px; border: 1px solid #333; }
        .info { background: #0d1b2a; padding: 15px; border-radius: 5px; border-left: 4px solid #2196F3; margin: 20px 0; }
        .example { background: #1a1a1a; padding: 15px; border-radius: 5px; margin: 10px 0; border: 1px solid #333; }
        .example strong { color: #f38020; }
        ul { margin: 15px 0; padding-left: 25px; }
        li { margin: 8px 0; }
        a { color: #2196F3; text-decoration: none; }
        a:hover { text-decoration: underline; }
        .footer { color: #666; font-size: 0.9em; margin-top: 40px; padding-top: 20px; border-top: 1px solid #333; }
    </style>
</head>
<body>
    <h1>🚀 R2 WebDAV Server</h1>
    <p>This is a CloudFlare Workers-based WebDAV server backed by R2 storage.</p>

    <div class="info">
        <strong>⚠️ WebDAV Endpoint:</strong> This worker serves WebDAV at <code>/webdav</code>
    </div>

    <h2>Configuration</h2>
    <p>To connect from Super Productivity or any WebDAV client:</p>
    <div class="example">
        <strong>Server URL:</strong> <code>""" + url.TrimEnd('/') + """/webdav</code><br>
        <strong>Username:</strong> Your configured username<br>
        <strong>Password:</strong> Your configured password
    </div>

    <h2>Features</h2>
    <ul>
        <li>Per-user R2 bucket isolation</li>
        <li>Basic authentication</li>
        <li>Full WebDAV protocol support (PROPFIND, GET, PUT, DELETE, MKCOL, COPY, MOVE)</li>
        <li>CORS enabled for web applications</li>
    </ul>

    <div class="footer">
        Powered by <a href="https://github.com/speakeztech/CloudflareFS">CloudflareFS</a> - F# bindings for Cloudflare Workers
    </div>
</body>
</html>"""
            let headers = Headers.Create()
            headers.set("Content-Type", "text/html; charset=utf-8")
            let init = createObj [
                "status" ==> 200
                "headers" ==> headers
            ]
            return Response.Create(U2.Case1 html, unbox init)
        else
            let method = Request.getMethod request

            // CORS preflight doesn't need auth
            if method = "OPTIONS" then
                let response = handleOptions ()
                return addCorsHeaders response
            else
                // Get authorization header
                let authHeader = request.headers.get("Authorization")

                // Parse and verify auth
                let! authResult = parseAndVerifyAuth authHeader env

                match authResult with
                | None ->
                    let response = unauthorizedResponse ()
                    return addCorsHeaders response
                | Some (username, bucket) ->
                    // Process WebDAV request
                    let! response = dispatchHandler request bucket
                    return addCorsHeaders response
    }

/// Export the handler
[<ExportDefault>]
let handler: obj = {| fetch = fetch |} :> obj
