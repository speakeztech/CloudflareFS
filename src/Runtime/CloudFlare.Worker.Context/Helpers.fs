// F# idiomatic helpers and computation expressions for Cloudflare Workers
module CloudFlare.Worker.Context.Helpers

open Fable.Core
open Fable.Core.JsInterop
open System
open CloudFlare.Worker.Context
open CloudFlare.Worker.Context.Globals

// Convert JS promises to F# async
let inline promiseToAsync (p: JS.Promise<'T>) : Async<'T> =
    Async.AwaitPromise p

// Response builder helpers
module ResponseBuilder =
    let json (data: 'T) (status: int) =
        let init : ResponseInit = jsOptions(fun o -> o.status <- Some (float status))
        Response.json(data, init)

    let text (content: string) (status: int) =
        let init : ResponseInit = jsOptions(fun o -> o.status <- Some (float status))
        Response.Create(U2.Case1 content, init)

    let redirect (url: string) =
        Response.redirect(url, 302.0)

    let ok (content: string) =
        text content 200

    let notFound () =
        text "Not Found" 404

    let serverError (message: string) =
        text message 500

// Headers builder
type HeadersBuilder() =
    member _.Yield(_) = Headers.Create()

    [<CustomOperation("set")>]
    member _.Set(headers: Headers, name: string, value: string) =
        headers.set(name, value)
        headers

    [<CustomOperation("contentType")>]
    member _.ContentType(headers: Headers, value: string) =
        headers.set("Content-Type", value)
        headers

    [<CustomOperation("cors")>]
    member _.Cors(headers: Headers) =
        headers.set("Access-Control-Allow-Origin", "*")
        headers

let headers = HeadersBuilder()

// Request helpers
module Request =
    let getHeader (name: string) (req: Request) =
        req.headers.get(name)

    let getMethod (req: Request) =
        req.``method``

    let getUrl (req: Request) =
        req.url

    let getPath (req: Request) =
        emitJsExpr (req.url) "new URL($0).pathname"

    let getQuery (req: Request) =
        emitJsExpr req.url "Object.fromEntries(new URL($0).searchParams.entries())"

// Async computation expression extensions for Workers
type AsyncBuilder with
    member x.Source(p: JS.Promise<'T>) = promiseToAsync p