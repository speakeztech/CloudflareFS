module CloudFlare.DurableObjects.Helpers

open CloudFlare.DurableObjects
open CloudFlare.Worker.Context
open Fable.Core
open Fable.Core.JsInterop
open System

/// Create list options with a prefix
let listOptions (prefix: string) =
    { new DurableObjectListOptions with
        member val start = None with get, set
        member val startAfter = None with get, set
        member val ``end`` = None with get, set
        member val prefix = Some prefix with get, set
        member val reverse = None with get, set
        member val limit = None with get, set
        member val allowConcurrency = None with get, set
        member val noCache = None with get, set }

/// Create list options with limit
let listOptionsWithLimit (limit: int) =
    { new DurableObjectListOptions with
        member val start = None with get, set
        member val startAfter = None with get, set
        member val ``end`` = None with get, set
        member val prefix = None with get, set
        member val reverse = None with get, set
        member val limit = Some limit with get, set
        member val allowConcurrency = None with get, set
        member val noCache = None with get, set }

/// Get options allowing concurrency
let getOptionsAllowConcurrency =
    { new DurableObjectGetOptions with
        member val allowConcurrency = Some true with get, set
        member val noCache = None with get, set }

/// Put options allowing unconfirmed writes
let putOptionsAllowUnconfirmed =
    { new DurableObjectPutOptions with
        member val allowConcurrency = None with get, set
        member val allowUnconfirmed = Some true with get, set
        member val noCache = None with get, set }

/// Durable Object builder for simplified operations
type DurableObjectBuilder(state: DurableObjectState<obj>) =
    member _.State = state
    member _.Storage = state.storage

    member _.Get<'T>(key: string) =
        state.storage.get<'T>(key) |> Async.AwaitPromise

    member _.Put<'T>(key: string, value: 'T) =
        state.storage.put(key, value) |> Async.AwaitPromise

    member _.Delete(key: string) =
        state.storage.delete(key) |> Async.AwaitPromise

    member _.List<'T>(?prefix: string) =
        let options =
            match prefix with
            | Some p -> listOptions p
            | None -> null
        state.storage.list<'T>(options) |> Async.AwaitPromise

    member _.SetAlarm(delayMs: int) =
        let time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + int64 delayMs
        state.storage.setAlarm(float time) |> Async.AwaitPromise

    member _.Transaction<'T>(operation: DurableObjectTransaction -> Async<'T>) =
        state.storage.transaction(fun tx ->
            operation tx |> Async.StartAsPromise
        ) |> Async.AwaitPromise

/// Create a Durable Object builder
let durableObject (state: DurableObjectState<obj>) = DurableObjectBuilder(state)

/// Namespace builder for Durable Object operations
type NamespaceBuilder(ns: DurableObjectNamespace) =
    member _.Namespace = ns

    member _.NewUniqueId() =
        ns.newUniqueId()

    member _.IdFromName(name: string) =
        ns.idFromName(name)

    member _.IdFromString(id: string) =
        ns.idFromString(id)

    member _.Get(id: DurableObjectId) =
        ns.get(id)

    member _.GetByName(name: string) =
        ns.get(ns.idFromName(name))

    member _.FetchByName(name: string, request: Request) =
        let stub = ns.get(ns.idFromName(name))
        stub.fetch(request) |> Async.AwaitPromise

    member _.FetchById(id: DurableObjectId, request: Request) =
        let stub = ns.get(id)
        stub.fetch(request) |> Async.AwaitPromise

/// Create a namespace builder
let namespace (ns: DurableObjectNamespace) = NamespaceBuilder(ns)

/// Create a simple counter Durable Object
[<AbstractClass>]
type CounterDurableObject(state: DurableObjectState<obj>, env: obj) =
    let storage = state.storage

    member _.Increment() =
        async {
            let! current = storage.get<int>("counter") |> Async.AwaitPromise
            let value = defaultArg current 0
            let newValue = value + 1
            do! storage.put("counter", newValue) |> Async.AwaitPromise
            return newValue
        }

    member _.Decrement() =
        async {
            let! current = storage.get<int>("counter") |> Async.AwaitPromise
            let value = defaultArg current 0
            let newValue = value - 1
            do! storage.put("counter", newValue) |> Async.AwaitPromise
            return newValue
        }

    member _.GetValue() =
        async {
            let! current = storage.get<int>("counter") |> Async.AwaitPromise
            return defaultArg current 0
        }

    interface DurableObject with
        member this.fetch(request: Request) =
            let handleRequest() =
                async {
                    let url = request.url
                    if url.Contains("/increment") then
                        let! value = this.Increment()
                        return Response.json({| counter = value |})
                    elif url.Contains("/decrement") then
                        let! value = this.Decrement()
                        return Response.json({| counter = value |})
                    else
                        let! value = this.GetValue()
                        return Response.json({| counter = value |})
                }
            handleRequest() |> Async.StartAsPromise |> U2.Case2

        member _.alarm() = Promise.create(fun resolve reject -> resolve())
        member _.webSocketMessage(ws, message) = Promise.create(fun resolve reject -> resolve())
        member _.webSocketClose(ws, code, reason, wasClean) = Promise.create(fun resolve reject -> resolve())
        member _.webSocketError(ws, error) = Promise.create(fun resolve reject -> resolve())

/// Storage helper for key-value operations
module Storage =
    /// Get or set default value
    let getOrDefault<'T> (storage: DurableObjectStorage) (key: string) (defaultValue: 'T) =
        async {
            let! value = storage.get<'T>(key) |> Async.AwaitPromise
            match value with
            | Some v -> return v
            | None ->
                do! storage.put(key, defaultValue) |> Async.AwaitPromise
                return defaultValue
        }

    /// Atomic increment
    let increment (storage: DurableObjectStorage) (key: string) =
        storage.transaction(fun tx ->
            promise {
                let! current = tx.get<int>(key)
                let value = defaultArg current 0
                let newValue = value + 1
                do! tx.put(key, newValue)
                return newValue
            }
        ) |> Async.AwaitPromise

    /// Batch operations
    let putMany<'T> (storage: DurableObjectStorage) (items: (string * 'T) list) =
        let map = items |> Map.ofList
        storage.put(map) |> Async.AwaitPromise

/// WebSocket helpers
module WebSocket =
    /// Broadcast message to all connected clients
    let broadcast (state: DurableObjectState<obj>) (message: string) =
        let sockets = state.getWebSockets()
        for socket in sockets do
            socket.send(message)

    /// Broadcast to tagged clients
    let broadcastToTag (state: DurableObjectState<obj>) (tag: string) (message: string) =
        let sockets = state.getWebSockets(tag)
        for socket in sockets do
            socket.send(message)

    /// Accept and tag a WebSocket
    let acceptWithTag (state: DurableObjectState<obj>) (ws: WebSocket) (tag: string) =
        state.acceptWebSocket(ws, ResizeArray [tag])

/// Active patterns for Durable Object requests
let (|GetRequest|PostRequest|PutRequest|DeleteRequest|OtherRequest|) (request: Request) =
    match request.method with
    | "GET" -> GetRequest
    | "POST" -> PostRequest
    | "PUT" -> PutRequest
    | "DELETE" -> DeleteRequest
    | _ -> OtherRequest request.method