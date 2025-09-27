module CloudFlare.R2.Helpers

open CloudFlare.R2
open Fable.Core
open Fable.Core.JsInterop

/// Get text content from R2
let getText (bucket: R2Bucket) (key: string) = promise {
    let! obj = bucket.get(key)
    match obj with
    | Some body -> return! body.text()
    | None -> return None
}

/// Get JSON content from R2
let getJson<'T> (bucket: R2Bucket) (key: string) = promise {
    let! obj = bucket.get(key)
    match obj with
    | Some body ->
        let! json = body.json<'T>()
        return Some json
    | None -> return None
}

/// Put text content to R2
let putText (bucket: R2Bucket) (key: string) (content: string) =
    bucket.put(key, U3.Case2 content)

/// Put JSON content to R2
let putJson (bucket: R2Bucket) (key: string) (data: 'T) =
    let json = JS.JSON.stringify(data)
    bucket.put(key, U3.Case2 json)

/// Put with content type
let putWithType (bucket: R2Bucket) (key: string) (content: string) (contentType: string) =
    let options = createEmpty<R2PutOptions>
    let metadata = createEmpty<R2HTTPMetadata>
    metadata.contentType <- Some contentType
    options.httpMetadata <- Some metadata
    bucket.put(key, U3.Case2 content, options)

/// List objects with prefix
let listWithPrefix (bucket: R2Bucket) (prefix: string) =
    let options = createEmpty<R2ListOptions>
    options.prefix <- Some prefix
    bucket.list(options)

/// Check if object exists
let exists (bucket: R2Bucket) (key: string) = promise {
    let! obj = bucket.head(key)
    return obj.IsSome
}