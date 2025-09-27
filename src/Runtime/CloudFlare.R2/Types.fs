namespace CloudFlare.R2

open Fable.Core
open Fable.Core.JsInterop
open System

[<AllowNullLiteral>]
[<Interface>]
type R2Bucket =
    abstract member head: key: string -> JS.Promise<R2Object option>
    abstract member get: key: string -> JS.Promise<R2ObjectBody option>
    abstract member put: key: string * value: U3<JS.ArrayBuffer, string, ReadableStream> * ?options: R2PutOptions -> JS.Promise<R2Object>
    abstract member delete: key: string -> JS.Promise<unit>
    abstract member list: ?options: R2ListOptions -> JS.Promise<R2Objects>

and [<AllowNullLiteral>] R2Object =
    abstract member key: string with get
    abstract member version: string with get
    abstract member size: float with get
    abstract member etag: string with get
    abstract member httpEtag: string with get
    abstract member uploaded: JS.Date with get
    abstract member httpMetadata: R2HTTPMetadata option with get
    abstract member customMetadata: obj option with get

and [<AllowNullLiteral>] R2ObjectBody =
    inherit R2Object
    abstract member body: ReadableStream with get
    abstract member text: unit -> JS.Promise<string>
    abstract member json<'T> : unit -> JS.Promise<'T>
    abstract member arrayBuffer: unit -> JS.Promise<JS.ArrayBuffer>

and [<AllowNullLiteral>] R2PutOptions =
    abstract member httpMetadata: R2HTTPMetadata option with get, set
    abstract member customMetadata: obj option with get, set

and [<AllowNullLiteral>] R2HTTPMetadata =
    abstract member contentType: string option with get, set
    abstract member contentLanguage: string option with get, set
    abstract member contentDisposition: string option with get, set
    abstract member contentEncoding: string option with get, set
    abstract member cacheControl: string option with get, set
    abstract member cacheExpiry: JS.Date option with get, set

and [<AllowNullLiteral>] R2ListOptions =
    abstract member limit: float option with get, set
    abstract member prefix: string option with get, set
    abstract member cursor: string option with get, set
    abstract member delimiter: string option with get, set

and [<AllowNullLiteral>] R2Objects =
    abstract member objects: ResizeArray<R2Object> with get
    abstract member truncated: bool with get
    abstract member cursor: string option with get
    abstract member delimitedPrefixes: ResizeArray<string> with get

and [<AllowNullLiteral>] ReadableStream =
    abstract member locked: bool with get
    abstract member cancel: ?reason: obj -> JS.Promise<unit>