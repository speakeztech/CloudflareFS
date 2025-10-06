module R2WebDAV.Types

open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.Worker.Context
open CloudFlare.R2

/// WebDAV properties for a resource
type DavProperties = {
    CreationDate: string option
    DisplayName: string option
    ContentLanguage: string option
    ContentLength: string option
    ContentType: string option
    ETag: string option
    LastModified: string option
    ResourceType: string
}

[<Emit("new Date()")>]
let jsDateNow() : JS.Date = jsNative

/// Convert JS Date to RFC 1123 string
let toRfc1123 (date: JS.Date) : string =
    date.toUTCString()

/// Convert R2 object to WebDAV properties
let fromR2Object (obj: R2Object option) : DavProperties =
    match obj with
    | None ->
        {
            CreationDate = Some (jsDateNow() |> toRfc1123)
            DisplayName = None
            ContentLanguage = None
            ContentLength = Some "0"
            ContentType = None
            ETag = None
            LastModified = Some (jsDateNow() |> toRfc1123)
            ResourceType = "<collection />"
        }
    | Some r2obj ->
        let httpMeta = r2obj.httpMetadata
        let customMeta = r2obj.customMetadata

        {
            CreationDate = Some (toRfc1123 r2obj.uploaded)
            DisplayName = httpMeta |> Option.bind (fun m -> m.contentDisposition)
            ContentLanguage = httpMeta |> Option.bind (fun m -> m.contentLanguage)
            ContentLength = Some (string r2obj.size)
            ContentType = httpMeta |> Option.bind (fun m -> m.contentType)
            ETag = Some r2obj.etag
            LastModified = Some (toRfc1123 r2obj.uploaded)
            ResourceType =
                match customMeta with
                | Some meta ->
                    let rt: string option = meta?resourcetype
                    rt |> Option.defaultValue ""
                | None -> ""
        }
