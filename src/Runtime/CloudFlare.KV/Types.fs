namespace CloudFlare.KV

open Fable.Core
open Fable.Core.JsInterop
open System

[<AllowNullLiteral>]
[<Interface>]
type KVNamespace =
    abstract member get: key: string * ?``type``: string -> JS.Promise<string option>
    [<Emit("$0.get($1, 'json')")>]
    abstract member getJson<'T> : key: string -> JS.Promise<'T option>
    [<Emit("$0.get($1, 'arrayBuffer')")>]
    abstract member getArrayBuffer: key: string -> JS.Promise<JS.ArrayBuffer option>
    abstract member put: key: string * value: U2<string, JS.ArrayBuffer> * ?options: KVPutOptions -> JS.Promise<unit>
    abstract member delete: key: string -> JS.Promise<unit>
    abstract member list: ?options: KVListOptions -> JS.Promise<KVListResult>

and [<AllowNullLiteral>] KVPutOptions =
    abstract member expiration: float option with get, set
    abstract member expirationTtl: float option with get, set
    abstract member metadata: obj option with get, set

and [<AllowNullLiteral>] KVListOptions =
    abstract member limit: float option with get, set
    abstract member prefix: string option with get, set
    abstract member cursor: string option with get, set

and [<AllowNullLiteral>] KVListResult =
    abstract member keys: ResizeArray<KVKey> with get, set
    abstract member list_complete: bool with get, set
    abstract member cursor: string option with get, set

and [<AllowNullLiteral>] KVKey =
    abstract member name: string with get, set
    abstract member expiration: float option with get, set
    abstract member metadata: obj option with get, set