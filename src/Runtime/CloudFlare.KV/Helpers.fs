module CloudFlare.KV.Helpers

open CloudFlare.KV
open Fable.Core
open Fable.Core.JsInterop

/// Get a string value from KV
let getString (kv: KVNamespace) (key: string) =
    kv.get(key)

/// Get a JSON value from KV
let getJson<'T> (kv: KVNamespace) (key: string) =
    kv.getJson<'T>(key)

/// Put a string value to KV
let putString (kv: KVNamespace) (key: string) (value: string) =
    kv.put(key, U2.Case1 value)

/// Put a JSON value to KV
let putJson (kv: KVNamespace) (key: string) (value: 'T) =
    let json = JS.JSON.stringify(value)
    kv.put(key, U2.Case1 json)

/// Put with expiration (Unix timestamp)
let putWithExpiration (kv: KVNamespace) (key: string) (value: string) (expiration: float) =
    let options = createEmpty<KVPutOptions>
    options.expiration <- Some expiration
    kv.put(key, U2.Case1 value, options)

/// Put with TTL (seconds)
let putWithTtl (kv: KVNamespace) (key: string) (value: string) (ttl: float) =
    let options = createEmpty<KVPutOptions>
    options.expirationTtl <- Some ttl
    kv.put(key, U2.Case1 value, options)

/// Put with metadata
let putWithMetadata (kv: KVNamespace) (key: string) (value: string) (metadata: obj) =
    let options = createEmpty<KVPutOptions>
    options.metadata <- Some metadata
    kv.put(key, U2.Case1 value, options)

/// List keys with prefix
let listWithPrefix (kv: KVNamespace) (prefix: string) =
    let options = createEmpty<KVListOptions>
    options.prefix <- Some prefix
    kv.list(options)

/// List all keys (paginated)
let rec listAll (kv: KVNamespace) (cursor: string option) = promise {
    let options = createEmpty<KVListOptions>
    options.cursor <- cursor
    let! result = kv.list(options)

    if result.list_complete then
        return result.keys
    else
        let! nextKeys = listAll kv result.cursor
        result.keys.AddRange(nextKeys)
        return result.keys
}

/// Delete if exists
let deleteIfExists (kv: KVNamespace) (key: string) = promise {
    let! existing = kv.get(key)
    match existing with
    | Some _ ->
        do! kv.delete(key)
        return true
    | None ->
        return false
}