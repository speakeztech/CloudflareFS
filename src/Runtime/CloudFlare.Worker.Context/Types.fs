namespace CloudFlare.Worker.Context

open Fable.Core
open Fable.Core.JsInterop
open System

[<AllowNullLiteral>]
[<Interface>]
type Headers =
    abstract member append: name: string * value: string -> unit
    abstract member delete: name: string -> unit
    abstract member get: name: string -> string option
    abstract member has: name: string -> bool
    abstract member set: name: string * value: string -> unit
    abstract member forEach: callback: (string -> string -> unit) -> unit

[<AllowNullLiteral>]
[<Interface>]
type Request =
    abstract member ``method``: string with get
    abstract member url: string with get
    abstract member headers: Headers with get
    abstract member clone: unit -> Request
    abstract member text: unit -> JS.Promise<string>
    abstract member json<'T> : unit -> JS.Promise<'T>
    abstract member arrayBuffer: unit -> JS.Promise<JS.ArrayBuffer>
    abstract member cf: obj option with get

[<AllowNullLiteral>]
[<Interface>]
type Response =
    abstract member status: float with get
    abstract member statusText: string with get
    abstract member ok: bool with get
    abstract member headers: Headers with get
    abstract member clone: unit -> Response
    abstract member text: unit -> JS.Promise<string>
    abstract member json<'T> : unit -> JS.Promise<'T>
    abstract member arrayBuffer: unit -> JS.Promise<JS.ArrayBuffer>

[<AllowNullLiteral>]
[<Interface>]
type ResponseInit =
    abstract member status: float option with get, set
    abstract member statusText: string option with get, set
    abstract member headers: U2<obj, Headers> option with get, set

[<AllowNullLiteral>]
[<Interface>]
type ExecutionContext =
    abstract member waitUntil: promise: JS.Promise<obj> -> unit
    abstract member passThroughOnException: unit -> unit

[<AllowNullLiteral>]
[<Interface>]
type Env =
    [<EmitIndexer>]
    abstract member Item: key: string -> obj with get, set

type FetchHandler = Request -> Env -> ExecutionContext -> U2<Response, JS.Promise<Response>>

// Constructors
[<Global>]
[<AllowNullLiteral>]
type HeadersConstructor =
    [<Emit("new $0($1...)")>]
    abstract Create: ?init: obj -> Headers

[<Global>]
[<AllowNullLiteral>]
type ResponseConstructor =
    [<Emit("new $0($1...)")>]
    abstract Create: ?body: U2<string, JS.ArrayBuffer> * ?init: ResponseInit -> Response
    abstract member json: obj * ?init: ResponseInit -> Response
    abstract member redirect: url: string * ?status: float -> Response

[<Global>]
let Headers: HeadersConstructor = jsNative

[<Global>]
let Response: ResponseConstructor = jsNative