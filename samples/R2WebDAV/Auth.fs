module R2WebDAV.Auth

open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.Worker.Context
open CloudFlare.Worker.Context.Globals
open CloudFlare.R2

[<Emit("new TextEncoder()")>]
let createTextEncoder() : obj = jsNative

[<Emit("new Uint8Array($0)")>]
let createUint8Array(buffer: obj) : JS.Uint8Array = jsNative

[<Emit("crypto.subtle.importKey($0, $1, $2, $3, $4)")>]
let importKey(format: string, keyData: obj, algorithm: obj, extractable: bool, keyUsages: string array) : JS.Promise<obj> = jsNative

[<Emit("crypto.subtle.sign($0, $1, $2)")>]
let sign(algorithm: string, key: obj, data: obj) : JS.Promise<JS.ArrayBuffer> = jsNative

[<Emit("atob($0)")>]
let atob(encoded: string) : string = jsNative

/// Timing-safe string comparison using SubtleCrypto
let timingSafeEqual (a: string) (b: string) : JS.Promise<bool> =
    promise {
        if a.Length <> b.Length then
            return false
        else
            let encoder = createTextEncoder()
            let aBytes: JS.Uint8Array = encoder?encode(a)
            let bBytes: JS.Uint8Array = encoder?encode(b)

            // Use SubtleCrypto for timing-safe comparison
            let algorithm = createObj ["name" ==> "HMAC"; "hash" ==> "SHA-256"]
            let! aKey = importKey("raw", aBytes, algorithm, false, [| "sign" |])

            let! aSignature = sign("HMAC", aKey, bBytes)
            let! bSignature = sign("HMAC", aKey, aBytes)

            let aView = createUint8Array(aSignature)
            let bView = createUint8Array(bSignature)

            let mutable equal = true
            for i in 0 .. int aView.length - 1 do
                if aView.[i] <> bView.[i] then
                    equal <- false

            return equal
    }

/// Parse Basic Auth header and verify credentials
let parseAndVerifyAuth (authHeader: string option) (env: Env) : JS.Promise<(string * R2Bucket) option> =
    promise {
        match authHeader with
        | None -> return None
        | Some header ->
            if not (header.StartsWith("Basic ")) then
                return None
            else
                let base64 = header.Substring(6)
                let decoded = atob(base64)
                let parts = decoded.Split(':')

                if parts.Length <> 2 then
                    return None
                else
                    let username = parts.[0]
                    let password = parts.[1]

                    // Construct deterministic names
                    let bucketBinding = $"{username}_webdav_sync"
                    let passwordSecret = $"USER_{username.ToUpper()}_PASSWORD"

                    // Get stored password from env
                    let storedPassword: string option = env.[passwordSecret] |> Option.ofObj |> Option.map string

                    match storedPassword with
                    | None -> return None
                    | Some stored ->
                        let! passwordMatch = timingSafeEqual password stored

                        if not passwordMatch then
                            return None
                        else
                            // Get bucket binding
                            let bucket: R2Bucket option = env.[bucketBinding] |> Option.ofObj |> Option.map unbox

                            match bucket with
                            | None -> return None
                            | Some b -> return Some (username, b)
    }

/// Create 401 Unauthorized response
let unauthorizedResponse () =
    let headers = Headers.Create()
    headers.set("WWW-Authenticate", "Basic realm=\"webdav\"")

    let init = createObj [
        "status" ==> 401
        "headers" ==> headers
    ]

    Response.Create(U2.Case1 "Unauthorized", unbox init)
