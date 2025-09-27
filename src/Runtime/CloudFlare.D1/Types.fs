namespace CloudFlare.D1

open Fable.Core
open Fable.Core.JsInterop
open System

[<AllowNullLiteral>]
[<Interface>]
type D1Database =
    abstract member prepare: query: string -> D1PreparedStatement
    abstract member batch<'T> : statements: ResizeArray<D1PreparedStatement> -> JS.Promise<ResizeArray<D1Result<'T>>>
    abstract member exec: query: string -> JS.Promise<D1ExecResult>

and [<AllowNullLiteral>] D1PreparedStatement =
    abstract member bind: [<ParamArray>] values: obj[] -> D1PreparedStatement
    abstract member first<'T> : ?colName: string -> JS.Promise<'T option>
    abstract member run<'T> : unit -> JS.Promise<D1Result<'T>>
    abstract member all<'T> : unit -> JS.Promise<D1Result<'T>>
    abstract member raw<'T> : unit -> JS.Promise<ResizeArray<'T>>

and [<AllowNullLiteral>] D1Result<'T> =
    abstract member results: ResizeArray<'T> option with get
    abstract member success: bool with get
    abstract member error: string option with get
    abstract member meta: obj with get

and [<AllowNullLiteral>] D1ExecResult =
    abstract member count: float with get
    abstract member duration: float with get