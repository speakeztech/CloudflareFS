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
            let headers = Headers.Create()
            let init = createObj [
                "status" ==> 404
                "headers" ==> headers
            ]
            return Response.Create(U2.Case1 "Not Found", unbox init)
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
