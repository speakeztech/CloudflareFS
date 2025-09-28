module CloudFlare.DurableObjects.Helpers

open CloudFlare.DurableObjects
open CloudFlare.Worker.Context
open CloudFlare.Worker.Context.Globals
open Fable.Core
open Fable.Core.JsInterop
open System

/// Create list options with a prefix
let listOptions (prefix: string) =
    let mutable _start = None
    let mutable _startAfter = None
    let mutable _end = None
    let mutable _prefix = Some prefix
    let mutable _reverse = None
    let mutable _limit = None
    let mutable _allowConcurrency = None
    let mutable _noCache = None
    { new DurableObjectListOptions with
        member _.start with get() = _start and set(v) = _start <- v
        member _.startAfter with get() = _startAfter and set(v) = _startAfter <- v
        member _.``end`` with get() = _end and set(v) = _end <- v
        member _.prefix with get() = _prefix and set(v) = _prefix <- v
        member _.reverse with get() = _reverse and set(v) = _reverse <- v
        member _.limit with get() = _limit and set(v) = _limit <- v
        member _.allowConcurrency with get() = _allowConcurrency and set(v) = _allowConcurrency <- v
        member _.noCache with get() = _noCache and set(v) = _noCache <- v }

/// Create list options with limit
let listOptionsWithLimit (limit: int) =
    let mutable _start = None
    let mutable _startAfter = None
    let mutable _end = None
    let mutable _prefix = None
    let mutable _reverse = None
    let mutable _limit = Some limit
    let mutable _allowConcurrency = None
    let mutable _noCache = None
    { new DurableObjectListOptions with
        member _.start with get() = _start and set(v) = _start <- v
        member _.startAfter with get() = _startAfter and set(v) = _startAfter <- v
        member _.``end`` with get() = _end and set(v) = _end <- v
        member _.prefix with get() = _prefix and set(v) = _prefix <- v
        member _.reverse with get() = _reverse and set(v) = _reverse <- v
        member _.limit with get() = _limit and set(v) = _limit <- v
        member _.allowConcurrency with get() = _allowConcurrency and set(v) = _allowConcurrency <- v
        member _.noCache with get() = _noCache and set(v) = _noCache <- v }

/// Get options allowing concurrency
let getOptionsAllowConcurrency =
    let mutable _allowConcurrency = Some true
    let mutable _noCache = None
    { new DurableObjectGetOptions with
        member _.allowConcurrency with get() = _allowConcurrency and set(v) = _allowConcurrency <- v
        member _.noCache with get() = _noCache and set(v) = _noCache <- v }

/// Put options allowing unconfirmed writes
let putOptionsAllowUnconfirmed =
    let mutable _allowConcurrency = None
    let mutable _allowUnconfirmed = Some true
    let mutable _noCache = None
    { new DurableObjectPutOptions with
        member _.allowConcurrency with get() = _allowConcurrency and set(v) = _allowConcurrency <- v
        member _.allowUnconfirmed with get() = _allowUnconfirmed and set(v) = _allowUnconfirmed <- v
        member _.noCache with get() = _noCache and set(v) = _noCache <- v }

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
let namespaceBuilder (ns: DurableObjectNamespace) = NamespaceBuilder(ns)

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
                        return Response.json({| counter = value |}, null)
                    elif url.Contains("/decrement") then
                        let! value = this.Decrement()
                        return Response.json({| counter = value |}, null)
                    else
                        let! value = this.GetValue()
                        return Response.json({| counter = value |}, null)
                }
            handleRequest() |> Async.StartAsPromise |> U2.Case2

        member _.alarm() = JS.Constructors.Promise.Create(fun resolve _ -> resolve())
        member _.webSocketMessage(ws, message) = JS.Constructors.Promise.Create(fun resolve _ -> resolve())
        member _.webSocketClose(ws, code, reason, wasClean) = JS.Constructors.Promise.Create(fun resolve _ -> resolve())
        member _.webSocketError(ws, error) = JS.Constructors.Promise.Create(fun resolve _ -> resolve())

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
            tx.get<int>(key)
            |> unbox<JS.Promise<int option>>
            |> fun p -> p?``then``(fun current ->
                let value = defaultArg current 0
                let newValue = value + 1
                tx.put(key, newValue)
                |> unbox<JS.Promise<unit>>
                |> fun p2 -> p2?``then``(fun () -> newValue))
            |> unbox<JS.Promise<int>>
        ) |> Async.AwaitPromise

    /// Batch operations
    let putMany<'T> (storage: DurableObjectStorage) (items: (string * 'T) list) =
        let map = items |> List.fold (fun (m:Map<string,'T>) (k,v) -> m.Add(k,v)) Map.empty
        storage.put(map) |> Async.AwaitPromise

/// WebSocket helpers
module WebSocket =
    /// Broadcast message to all connected clients
    let broadcast (state: DurableObjectState<obj>) (message: string) =
        let sockets = state.getWebSockets()
        for socket in sockets do
            socket?send(message)

    /// Broadcast to tagged clients
    let broadcastToTag (state: DurableObjectState<obj>) (tag: string) (message: string) =
        let sockets = state.getWebSockets(tag)
        for socket in sockets do
            socket?send(message)

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