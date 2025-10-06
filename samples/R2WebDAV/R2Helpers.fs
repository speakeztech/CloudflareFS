module R2WebDAV.R2Helpers

open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.R2

/// List all objects in a bucket with optional prefix
let listAll (bucket: R2Bucket) (prefix: string) (isRecursive: bool) : JS.Promise<R2Object array> =
    promise {
        let results = ResizeArray<R2Object>()
        let mutable cursor: string option = None
        let mutable truncated = true

        while truncated do
            let options =
                createObj [
                    "prefix" ==> prefix
                    "delimiter" ==> (if isRecursive then null else "/")
                    "cursor" ==> (cursor |> Option.toObj)
                    "include" ==> [| "httpMetadata"; "customMetadata" |]
                ]
                |> unbox<R2ListOptions>

            let! listResult = bucket.list(options)

            for obj in listResult.objects do
                results.Add(obj)

            truncated <- listResult.truncated
            cursor <- listResult.cursor

        return results.ToArray()
    }

/// Get object metadata without downloading body
let headObject (bucket: R2Bucket) (key: string) : JS.Promise<R2Object option> =
    bucket.head(key)

/// Check if object exists
let exists (bucket: R2Bucket) (key: string) : JS.Promise<bool> =
    promise {
        let! obj = bucket.head(key)
        return obj.IsSome
    }

/// Delete multiple objects matching a prefix (for directory deletion)
let deletePrefix (bucket: R2Bucket) (prefix: string) : JS.Promise<unit> =
    promise {
        let! objects = listAll bucket prefix true
        for obj in objects do
            do! bucket.delete(obj.key)
    }
