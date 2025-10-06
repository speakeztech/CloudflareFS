namespace rec CloudFlare.Management.Queues

open System.Net
open System.Net.Http
open System.Text
open System.Threading
open CloudFlare.Management.Queues.Types
open CloudFlare.Management.Queues.Http

///Message Queue Management API
type QueuesClient(httpClient: HttpClient) =
    ///<summary>
    ///Returns the queues owned by an account.
    ///</summary>
    member this.QueuesList(accountId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.getAsync httpClient "/accounts/{account_id}/queues" requestParts cancellationToken

            return QueuesList.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Create a new queue
    ///</summary>
    member this.QueuesCreate(accountId: string, body: QueuesCreatePayload, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync httpClient "/accounts/{account_id}/queues" requestParts cancellationToken

            return QueuesCreate.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Deletes a queue
    ///</summary>
    member this.QueuesDelete(queueId: string, accountId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("queue_id", queueId)
                  RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.deleteAsync
                    httpClient
                    "/accounts/{account_id}/queues/{queue_id}"
                    requestParts
                    cancellationToken

            return QueuesDelete.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Get details about a specific queue.
    ///</summary>
    member this.QueuesGet(queueId: string, accountId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("queue_id", queueId)
                  RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/queues/{queue_id}"
                    requestParts
                    cancellationToken

            return QueuesGet.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Updates a Queue.
    ///</summary>
    member this.QueuesUpdatePartial
        (
            queueId: string,
            accountId: string,
            body: mqqueue,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("queue_id", queueId)
                  RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.patchAsync
                    httpClient
                    "/accounts/{account_id}/queues/{queue_id}"
                    requestParts
                    cancellationToken

            return QueuesUpdatePartial.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Updates a Queue. Note that this endpoint does not support partial updates. If successful, the Queue's configuration is overwritten with the supplied configuration.
    ///</summary>
    member this.QueuesUpdate(queueId: string, accountId: string, body: mqqueue, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("queue_id", queueId)
                  RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.putAsync
                    httpClient
                    "/accounts/{account_id}/queues/{queue_id}"
                    requestParts
                    cancellationToken

            return QueuesUpdate.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Returns the consumers for a Queue
    ///</summary>
    member this.QueuesListConsumers(queueId: string, accountId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("queue_id", queueId)
                  RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/queues/{queue_id}/consumers"
                    requestParts
                    cancellationToken

            return QueuesListConsumers.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Creates a new consumer for a Queue
    ///</summary>
    member this.QueuesCreateConsumer
        (
            queueId: string,
            accountId: string,
            body: System.Text.Json.JsonElement,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("queue_id", queueId)
                  RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/queues/{queue_id}/consumers"
                    requestParts
                    cancellationToken

            return QueuesCreateConsumer.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Deletes the consumer for a queue.
    ///</summary>
    member this.QueuesDeleteConsumer
        (
            consumerId: string,
            queueId: string,
            accountId: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("consumer_id", consumerId)
                  RequestPart.path ("queue_id", queueId)
                  RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.deleteAsync
                    httpClient
                    "/accounts/{account_id}/queues/{queue_id}/consumers/{consumer_id}"
                    requestParts
                    cancellationToken

            return QueuesDeleteConsumer.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Fetches the consumer for a queue by consumer id
    ///</summary>
    member this.QueuesGetConsumer
        (
            consumerId: string,
            queueId: string,
            accountId: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("consumer_id", consumerId)
                  RequestPart.path ("queue_id", queueId)
                  RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/queues/{queue_id}/consumers/{consumer_id}"
                    requestParts
                    cancellationToken

            return QueuesGetConsumer.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Updates the consumer for a queue, or creates one if it does not exist.
    ///</summary>
    member this.QueuesUpdateConsumer
        (
            consumerId: string,
            queueId: string,
            accountId: string,
            body: System.Text.Json.JsonElement,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("consumer_id", consumerId)
                  RequestPart.path ("queue_id", queueId)
                  RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.putAsync
                    httpClient
                    "/accounts/{account_id}/queues/{queue_id}/consumers/{consumer_id}"
                    requestParts
                    cancellationToken

            return QueuesUpdateConsumer.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Push a message to a Queue
    ///</summary>
    member this.QueuesPushMessage
        (
            queueId: string,
            accountId: string,
            body: mqqueuemessage,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("queue_id", queueId)
                  RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/queues/{queue_id}/messages"
                    requestParts
                    cancellationToken

            return QueuesPushMessage.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Acknowledge + Retry messages from a Queue
    ///</summary>
    member this.QueuesAckMessages
        (
            queueId: string,
            accountId: string,
            body: QueuesAckMessagesPayload,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("queue_id", queueId)
                  RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/queues/{queue_id}/messages/ack"
                    requestParts
                    cancellationToken

            return QueuesAckMessages.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Push a batch of message to a Queue
    ///</summary>
    member this.QueuesPushMessages
        (
            queueId: string,
            accountId: string,
            body: mqqueuebatch,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("queue_id", queueId)
                  RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/queues/{queue_id}/messages/batch"
                    requestParts
                    cancellationToken

            return QueuesPushMessages.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Pull a batch of messages from a Queue
    ///</summary>
    member this.QueuesPullMessages
        (
            queueId: string,
            accountId: string,
            body: QueuesPullMessagesPayload,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("queue_id", queueId)
                  RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/queues/{queue_id}/messages/pull"
                    requestParts
                    cancellationToken

            return QueuesPullMessages.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Get details about a Queue's purge status.
    ///</summary>
    member this.QueuesPurgeGet(queueId: string, accountId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("queue_id", queueId)
                  RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/queues/{queue_id}/purge"
                    requestParts
                    cancellationToken

            return QueuesPurgeGet.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Deletes all messages from the Queue.
    ///</summary>
    member this.QueuesPurge
        (
            queueId: string,
            accountId: string,
            body: QueuesPurgePayload,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("queue_id", queueId)
                  RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/queues/{queue_id}/purge"
                    requestParts
                    cancellationToken

            return QueuesPurge.OK(Serializer.deserialize content)
        }
