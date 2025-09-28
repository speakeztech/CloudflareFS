module CloudFlare.Queues.Helpers

open CloudFlare.Queues
open Fable.Core.JsInterop
open System

/// Create a message send request
let createMessage<'T> (body: 'T) =
    { new MessageSendRequest<'T> with
        member val body = body with get, set
        member val id = None with get, set
        member val delaySeconds = None with get, set }

/// Create a message with ID
let createMessageWithId<'T> (id: string) (body: 'T) =
    { new MessageSendRequest<'T> with
        member val body = body with get, set
        member val id = Some id with get, set
        member val delaySeconds = None with get, set }

/// Create a delayed message
let createDelayedMessage<'T> (delaySeconds: float) (body: 'T) =
    { new MessageSendRequest<'T> with
        member val body = body with get, set
        member val id = None with get, set
        member val delaySeconds = Some delaySeconds with get, set }

/// Queue computation expression for F# async workflows
type QueueBuilder<'T>(queue: Queue<'T>) =
    member _.Queue = queue

    member _.Send(body: 'T) =
        queue.send(body) |> Async.AwaitPromise

    member _.SendDelayed(body: 'T, delaySeconds: float) =
        let options =
            { new QueueSendOptions with
                member val contentType = None with get, set
                member val delaySeconds = Some delaySeconds with get, set }
        queue.send(body, options) |> Async.AwaitPromise

    member _.SendBatch(messages: 'T list) =
        let msgs =
            messages
            |> List.map createMessage
            |> ResizeArray
        queue.sendBatch(msgs) |> Async.AwaitPromise

    member _.SendBatchDelayed(messages: 'T list, delaySeconds: float) =
        let msgs =
            messages
            |> List.map (createDelayedMessage delaySeconds)
            |> ResizeArray
        queue.sendBatch(msgs) |> Async.AwaitPromise

/// Create a queue builder
let queue<'T> (q: Queue<'T>) = QueueBuilder(q)

/// Process messages with automatic acknowledgment
let processMessages<'T> (handler: Message<'T> -> Async<unit>) (batch: MessageBatch<'T>) =
    async {
        let mutable allSucceeded = true

        for message in batch.messages do
            try
                do! handler message
            with
            | ex ->
                allSucceeded <- false
                eprintfn $"Failed to process message {message.id}: {ex.Message}"

        if allSucceeded then
            batch.ackAll()
        else
            batch.retryAll()
    }

/// Process messages individually with selective acknowledgment
let processMessagesIndividually<'T> (handler: Message<'T> -> Async<bool>) (batch: MessageBatch<'T>) =
    async {
        let failed = ResizeArray<Message<'T>>()

        for message in batch.messages do
            try
                let! success = handler message
                if not success then
                    failed.Add(message)
            with
            | ex ->
                failed.Add(message)
                eprintfn $"Failed to process message {message.id}: {ex.Message}"

        // In real implementation, would need individual ack/retry
        // For now, use batch operations
        if failed.Count = 0 then
            batch.ackAll()
        else
            batch.retryAll()
    }

/// Active pattern for message content type detection
let (|JsonMessage|TextMessage|BinaryMessage|) (message: Message<obj>) =
    match message.body with
    | :? string as s when s.StartsWith("{") || s.StartsWith("[") -> JsonMessage s
    | :? string as s -> TextMessage s
    | _ -> BinaryMessage message.body