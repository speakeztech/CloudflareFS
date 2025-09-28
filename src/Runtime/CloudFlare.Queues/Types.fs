namespace CloudFlare.Queues

open Fable.Core
open Fable.Core.JsInterop
open System

/// Message send request for queue operations
[<AllowNullLiteral>]
type MessageSendRequest<'Body> =
    abstract member body: 'Body with get, set
    abstract member id: string option with get, set
    abstract member delaySeconds: float option with get, set

/// Options for sending a single message
[<AllowNullLiteral>]
type QueueSendOptions =
    abstract member contentType: string option with get, set
    abstract member delaySeconds: float option with get, set

/// Options for sending batch messages
[<AllowNullLiteral>]
type QueueSendBatchOptions =
    abstract member delaySeconds: float option with get, set

/// Queue interface for sending messages
[<AllowNullLiteral>]
[<Interface>]
type Queue<'Body> =
    /// Send a single message to the queue
    abstract member send: body: 'Body * ?options: QueueSendOptions -> JS.Promise<unit>

    /// Send multiple messages to the queue
    abstract member sendBatch: messages: ResizeArray<MessageSendRequest<'Body>> * ?options: QueueSendBatchOptions -> JS.Promise<unit>

/// Message received from a queue
[<AllowNullLiteral>]
type Message<'Body> =
    abstract member id: string with get
    abstract member timestamp: DateTime with get
    abstract member body: 'Body with get
    abstract member attempts: int with get

/// Retry options for message processing
[<AllowNullLiteral>]
type QueueRetryOptions =
    abstract member delaySeconds: float option with get, set

/// Batch of messages for processing
[<AllowNullLiteral>]
type MessageBatch<'Body> =
    abstract member queue: string with get
    abstract member messages: ResizeArray<Message<'Body>> with get
    abstract member ackAll: unit -> unit
    abstract member retryAll: ?options: QueueRetryOptions -> unit

/// Queue event for Workers
[<AllowNullLiteral>]
type QueueEvent<'Body> =
    inherit ExtendableEvent
    abstract member batch: MessageBatch<'Body> with get

/// Queue consumer interface for processing messages
[<AllowNullLiteral>]
[<Interface>]
type QueueConsumer<'Body> =
    /// Process a batch of messages from the queue
    abstract member queue: batch: MessageBatch<'Body> -> JS.Promise<unit>

/// Environment with Queue bindings
[<AllowNullLiteral>]
type IQueuesEnvironment =
    /// Access a Queue binding by name
    [<Emit("$0[$1]")>]
    abstract member getQueue: name: string -> Queue<obj>

/// Extension methods for typed Queue access
module QueueExtensions =
    type IQueuesEnvironment with
        /// Get a typed queue binding
        member inline this.GetQueue<'T>(name: string) : Queue<'T> =
            unbox(this.getQueue(name))