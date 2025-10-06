module R2WebDAV.WebDav

open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.Worker.Context
open CloudFlare.Worker.Context.Globals
open CloudFlare.Worker.Context.Helpers
open CloudFlare.R2
open R2WebDAV.Types
open R2WebDAV.R2Helpers

let API_PREFIX = "/webdav"

[<Emit("new URL($0)")>]
let createURL(url: string) : obj = jsNative

/// Extract resource path from request URL
let makeResourcePath (request: Request) : string =
    let url = Request.getUrl request
    let urlObj: obj = createURL(url)
    let mutable path: string = urlObj?pathname

    // Remove API prefix
    if API_PREFIX <> "" && path.StartsWith(API_PREFIX) then
        path <- path.Substring(API_PREFIX.Length)

    // Remove leading slash
    if path.StartsWith("/") then
        path <- path.Substring(1)

    // Remove trailing slash
    if path.EndsWith("/") && path.Length > 0 then
        path <- path.Substring(0, path.Length - 1)

    path

/// Create response with headers
let createResponse (body: string) (status: int) (contentType: string option) : Response =
    let headers = Headers.Create()
    match contentType with
    | Some ct -> headers.set("Content-Type", ct)
    | None -> ()

    let init = createObj [
        "status" ==> status
        "headers" ==> headers
    ]

    Response.Create(U2.Case1 body, unbox init)

/// Handle OPTIONS request (CORS preflight and WebDAV discovery)
let handleOptions () : Response =
    let headers = Headers.Create()
    headers.set("Allow", "GET, HEAD, PUT, DELETE, OPTIONS, PROPFIND, MKCOL, COPY, MOVE")
    headers.set("DAV", "1, 2")

    let init = createObj [
        "status" ==> 200
        "headers" ==> headers
    ]

    Response.Create(U2.Case1 "", unbox init)

/// Handle GET request
let rec handleGet (request: Request) (bucket: R2Bucket) : JS.Promise<Response> =
    promise {
        let resourcePath = makeResourcePath request
        let url = Request.getUrl request

        if url.EndsWith("/") then
            // Directory listing
            let prefix = if resourcePath = "" then "" else resourcePath + "/"
            let! objects = listAll bucket prefix false

            let mutable page = ""
            if resourcePath <> "" then
                page <- page + """<a href="../">..</a><br>"""

            for obj in objects do
                let name = obj.key.Substring(prefix.Length)
                let displayName = if name.EndsWith("/") || obj.size = 0.0 then name + "/" else name
                page <- page + $"""<a href="{name}">{displayName}</a><br>"""

            return createResponse page 200 (Some "text/html")
        else
            // File download
            let! obj = bucket.get(resourcePath)

            match obj with
            | None ->
                return createResponse "Not Found" 404 None
            | Some r2obj ->
                let headers = Headers.Create()
                match r2obj.httpMetadata with
                | Some meta ->
                    match meta.contentType with
                    | Some ct -> headers.set("Content-Type", ct)
                    | None -> ()
                | None -> ()

                headers.set("ETag", r2obj.etag)
                headers.set("Last-Modified", toRfc1123 r2obj.uploaded)

                let init = createObj [
                    "status" ==> 200
                    "headers" ==> headers
                ]

                // R2ObjectBody.body is ReadableStream, need to wrap in U2
                let streamBody: obj = r2obj.body
                return Response.Create(unbox streamBody, unbox init)
    }

/// Handle HEAD request
let handleHead (request: Request) (bucket: R2Bucket) : JS.Promise<Response> =
    promise {
        let! response = handleGet request bucket

        let init = createObj [
            "status" ==> response.status
            "statusText" ==> response.statusText
            "headers" ==> response.headers
        ]

        return Response.Create(U2.Case1 null, unbox init)
    }

/// Handle PUT request (file upload)
let handlePut (request: Request) (bucket: R2Bucket) : JS.Promise<Response> =
    promise {
        let resourcePath = makeResourcePath request

        // Check if parent directory exists
        let parts = resourcePath.Split('/')
        if parts.Length > 1 then
            let parentPath = String.concat "/" parts.[0..parts.Length-2]
            let! parentExists = exists bucket parentPath

            if not parentExists then
                return createResponse "Conflict" 409 None
            else
                let! body = request.arrayBuffer()
                let contentType = request.headers.get("Content-Type")

                let putOptions =
                    match contentType with
                    | Some ct ->
                        createObj [
                            "httpMetadata" ==> createObj ["contentType" ==> ct]
                        ] |> unbox<R2PutOptions>
                    | None ->
                        createObj [] |> unbox<R2PutOptions>

                let! _ = bucket.put(resourcePath, U3.Case1 body, putOptions)
                return createResponse "" 201 None
        else
            let! body = request.arrayBuffer()
            let contentType = request.headers.get("Content-Type")

            let putOptions =
                match contentType with
                | Some ct ->
                    createObj [
                        "httpMetadata" ==> createObj ["contentType" ==> ct]
                    ] |> unbox<R2PutOptions>
                | None ->
                    createObj [] |> unbox<R2PutOptions>

            let! _ = bucket.put(resourcePath, U3.Case1 body, putOptions)
            return createResponse "" 201 None
    }

/// Handle DELETE request
let handleDelete (request: Request) (bucket: R2Bucket) : JS.Promise<Response> =
    promise {
        let resourcePath = makeResourcePath request
        let! obj = bucket.head(resourcePath)

        match obj with
        | None ->
            return createResponse "Not Found" 404 None
        | Some r2obj ->
            // Check if it's a directory
            let isDirectory =
                match r2obj.customMetadata with
                | Some meta ->
                    let rt: string option = meta?resourcetype
                    rt = Some "<collection />"
                | None -> false

            if isDirectory then
                // Delete all objects with this prefix
                do! deletePrefix bucket (resourcePath + "/")
                do! bucket.delete(resourcePath)
            else
                do! bucket.delete(resourcePath)

            return createResponse "" 204 None
    }

/// Handle MKCOL request (create directory)
let handleMkcol (request: Request) (bucket: R2Bucket) : JS.Promise<Response> =
    promise {
        let resourcePath = makeResourcePath request

        // Check if already exists
        let! existing = bucket.head(resourcePath)

        match existing with
        | Some _ ->
            return createResponse "Method Not Allowed" 405 None
        | None ->
            // Create directory marker
            let options =
                createObj [
                    "customMetadata" ==> createObj ["resourcetype" ==> "<collection />"]
                ]
                |> unbox<R2PutOptions>

            let! _ = bucket.put(resourcePath, U3.Case2 "", options)
            return createResponse "" 201 None
    }

/// Handle PROPFIND request (WebDAV property discovery)
let handlePropfind (request: Request) (bucket: R2Bucket) : JS.Promise<Response> =
    promise {
        let resourcePath = makeResourcePath request
        let url = Request.getUrl request

        if url.EndsWith("/") || resourcePath = "" then
            // Directory propfind
            let prefix = if resourcePath = "" then "" else resourcePath + "/"
            let! objects = listAll bucket prefix false

            let mutable xml = """<?xml version="1.0" encoding="utf-8"?>"""
            xml <- xml + """<D:multistatus xmlns:D="DAV:">"""

            // Add entry for the directory itself
            xml <- xml + """<D:response>"""
            xml <- xml + $"""<D:href>{API_PREFIX}/{resourcePath}/</D:href>"""
            xml <- xml + """<D:propstat><D:prop>"""
            xml <- xml + """<D:resourcetype><D:collection/></D:resourcetype>"""
            xml <- xml + """</D:prop><D:status>HTTP/1.1 200 OK</D:status></D:propstat>"""
            xml <- xml + """</D:response>"""

            // Add entries for contents
            for obj in objects do
                let props = fromR2Object (Some obj)
                let name = obj.key.Substring(prefix.Length)

                xml <- xml + """<D:response>"""
                xml <- xml + $"""<D:href>{API_PREFIX}/{prefix}{name}</D:href>"""
                xml <- xml + """<D:propstat><D:prop>"""

                match props.ContentLength with
                | Some len -> xml <- xml + $"""<D:getcontentlength>{len}</D:getcontentlength>"""
                | None -> ()

                match props.ContentType with
                | Some ct -> xml <- xml + $"""<D:getcontenttype>{ct}</D:getcontenttype>"""
                | None -> ()

                match props.LastModified with
                | Some lm -> xml <- xml + $"""<D:getlastmodified>{lm}</D:getlastmodified>"""
                | None -> ()

                match props.ETag with
                | Some etag -> xml <- xml + $"""<D:getetag>{etag}</D:getetag>"""
                | None -> ()

                if props.ResourceType = "<collection />" then
                    xml <- xml + """<D:resourcetype><D:collection/></D:resourcetype>"""
                else
                    xml <- xml + """<D:resourcetype/>"""

                xml <- xml + """</D:prop><D:status>HTTP/1.1 200 OK</D:status></D:propstat>"""
                xml <- xml + """</D:response>"""

            xml <- xml + """</D:multistatus>"""

            return createResponse xml 207 (Some "application/xml; charset=utf-8")
        else
            // File propfind
            let! obj = bucket.head(resourcePath)

            let mutable xml = """<?xml version="1.0" encoding="utf-8"?>"""
            xml <- xml + """<D:multistatus xmlns:D="DAV:">"""
            xml <- xml + """<D:response>"""
            xml <- xml + $"""<D:href>{API_PREFIX}/{resourcePath}</D:href>"""
            xml <- xml + """<D:propstat><D:prop>"""

            let props = fromR2Object obj

            match props.ContentLength with
            | Some len -> xml <- xml + $"""<D:getcontentlength>{len}</D:getcontentlength>"""
            | None -> ()

            match props.ContentType with
            | Some ct -> xml <- xml + $"""<D:getcontenttype>{ct}</D:getcontenttype>"""
            | None -> ()

            match props.LastModified with
            | Some lm -> xml <- xml + $"""<D:getlastmodified>{lm}</D:getlastmodified>"""
            | None -> ()

            match props.ETag with
            | Some etag -> xml <- xml + $"""<D:getetag>{etag}</D:getetag>"""
            | None -> ()

            if props.ResourceType = "<collection />" then
                xml <- xml + """<D:resourcetype><D:collection/></D:resourcetype>"""
            else
                xml <- xml + """<D:resourcetype/>"""

            xml <- xml + """</D:prop><D:status>HTTP/1.1 200 OK</D:status></D:propstat>"""
            xml <- xml + """</D:response>"""
            xml <- xml + """</D:multistatus>"""

            return createResponse xml 207 (Some "application/xml; charset=utf-8")
    }

/// Dispatch WebDAV request to appropriate handler
let dispatchHandler (request: Request) (bucket: R2Bucket) : JS.Promise<Response> =
    promise {
        let method = Request.getMethod request

        return!
            match method with
            | "OPTIONS" -> Promise.lift (handleOptions ())
            | "HEAD" -> handleHead request bucket
            | "GET" -> handleGet request bucket
            | "PUT" -> handlePut request bucket
            | "DELETE" -> handleDelete request bucket
            | "MKCOL" -> handleMkcol request bucket
            | "PROPFIND" -> handlePropfind request bucket
            | _ -> Promise.lift (createResponse "Method Not Allowed" 405 None)
    }
