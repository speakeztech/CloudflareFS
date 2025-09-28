namespace rec CloudFlare.Management.Queues.Types

type ``mqapi-v4-errorArrayItem`` =
    { code: int
      message: string }
    ///Creates an instance of mqapi-v4-errorArrayItem with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``mqapi-v4-errorArrayItem`` = { code = code; message = message }

type ``mqapi-v4-error`` = list<``mqapi-v4-errorArrayItem``>
type ``mqapi-v4-message`` = list<string>
type ``mqbatch-size`` = float
///A Resource identifier.
type mqidentifier = string
///An ID that represents an "in-flight" message that has been pulled from a Queue. You must hold on to this ID and use it to acknowledge this message.
type ``mqlease-id`` = string
type ``mqmax-concurrency`` = float
type ``mqmax-retries`` = float
type ``mqmax-wait-time`` = float
type ``mqqueue-name`` = string

type ``mqqueue-pull-batchArrayItem`` =
    { attempts: Option<float>
      body: Option<string>
      id: Option<string>
      ///An ID that represents an "in-flight" message that has been pulled from a Queue. You must hold on to this ID and use it to acknowledge this message.
      lease_id: Option<``mqlease-id``>
      metadata: Option<Newtonsoft.Json.Linq.JObject>
      timestamp_ms: Option<float> }
    ///Creates an instance of mqqueue-pull-batchArrayItem with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``mqqueue-pull-batchArrayItem`` =
        { attempts = None
          body = None
          id = None
          lease_id = None
          metadata = None
          timestamp_ms = None }

type ``mqqueue-pull-batch`` = list<``mqqueue-pull-batchArrayItem``>
type ``mqretry-delay`` = float
///Name of a Worker
type ``mqscript-name`` = string
type ``mqvisibility-timeout`` = float

type Errors =
    { code: int
      message: string }
    ///Creates an instance of Errors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): Errors = { code = code; message = message }

type ``mqapi-v4-failure`` =
    { errors: Option<list<Errors>>
      messages: Option<``mqapi-v4-message``>
      ///Indicates if the API call was successful or not.
      success: Option<bool> }
    ///Creates an instance of mqapi-v4-failure with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``mqapi-v4-failure`` =
        { errors = None
          messages = None
          success = None }

type ``mqapi-v4-successErrors`` =
    { code: int
      message: string }
    ///Creates an instance of mqapi-v4-successErrors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``mqapi-v4-successErrors`` = { code = code; message = message }

type ``mqapi-v4-success`` =
    { errors: Option<list<``mqapi-v4-successErrors``>>
      messages: Option<``mqapi-v4-message``>
      ///Indicates if the API call was successful or not.
      success: Option<bool> }
    ///Creates an instance of mqapi-v4-success with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``mqapi-v4-success`` =
        { errors = None
          messages = None
          success = None }

type Settings =
    { ///The maximum number of messages to include in a batch.
      batch_size: Option<``mqbatch-size``>
      ///The maximum number of retries
      max_retries: Option<``mqmax-retries``>
      ///The number of seconds to delay before making the message available for another attempt.
      retry_delay: Option<``mqretry-delay``>
      ///The number of milliseconds that a message is exclusively leased. After the timeout, the message becomes available for another attempt.
      visibility_timeout_ms: Option<``mqvisibility-timeout``> }
    ///Creates an instance of Settings with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): Settings =
        { batch_size = None
          max_retries = None
          retry_delay = None
          visibility_timeout_ms = None }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type Type =
    | [<CompiledName "http_pull">] Http_pull
    member this.Format() =
        match this with
        | Http_pull -> "http_pull"

type ``mqhttp-consumer`` =
    { ///A Resource identifier.
      consumer_id: Option<mqidentifier>
      created_on: Option<string>
      ///A Resource identifier.
      queue_id: Option<mqidentifier>
      settings: Option<Settings>
      ``type``: Option<Type> }
    ///Creates an instance of mqhttp-consumer with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``mqhttp-consumer`` =
        { consumer_id = None
          created_on = None
          queue_id = None
          settings = None
          ``type`` = None }

type mqqueue =
    { consumers: Option<list<mqconsumer>>
      consumers_total_count: Option<float>
      created_on: Option<string>
      modified_on: Option<string>
      producers: Option<list<mqproducer>>
      producers_total_count: Option<float>
      queue_id: Option<string>
      queue_name: Option<``mqqueue-name``>
      settings: Option<``mqqueue-settings``> }
    ///Creates an instance of mqqueue with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): mqqueue =
        { consumers = None
          consumers_total_count = None
          created_on = None
          modified_on = None
          producers = None
          producers_total_count = None
          queue_id = None
          queue_name = None
          settings = None }

type ``mqqueue-batch`` =
    { ///The number of seconds to wait for attempting to deliver this batch to consumers
      delay_seconds: Option<float>
      messages: Option<list<``mqqueue-message``>> }
    ///Creates an instance of mqqueue-batch with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``mqqueue-batch`` =
        { delay_seconds = None
          messages = None }

type ``mqqueue-message`` =
    { ///The number of seconds to wait for attempting to deliver this message to consumers
      delay_seconds: Option<float> }
    ///Creates an instance of mqqueue-message with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``mqqueue-message`` = { delay_seconds = None }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type Contenttype =
    | [<CompiledName "json">] Json
    member this.Format() =
        match this with
        | Json -> "json"

type ``mqqueue-message-json`` =
    { body: Option<Newtonsoft.Json.Linq.JObject>
      content_type: Option<Contenttype> }
    ///Creates an instance of mqqueue-message-json with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``mqqueue-message-json`` = { body = None; content_type = None }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type ``mqqueue-message-textContenttype`` =
    | [<CompiledName "text">] Text
    member this.Format() =
        match this with
        | Text -> "text"

type ``mqqueue-message-text`` =
    { body: Option<string>
      content_type: Option<``mqqueue-message-textContenttype``> }
    ///Creates an instance of mqqueue-message-text with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``mqqueue-message-text`` = { body = None; content_type = None }

type ``mqqueue-settings`` =
    { ///Number of seconds to delay delivery of all messages to consumers.
      delivery_delay: Option<float>
      ///Indicates if message delivery to consumers is currently paused.
      delivery_paused: Option<bool>
      ///Number of seconds after which an unconsumed message will be delayed.
      message_retention_period: Option<float> }
    ///Creates an instance of mqqueue-settings with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``mqqueue-settings`` =
        { delivery_delay = None
          delivery_paused = None
          message_retention_period = None }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type ``mqr2-producerType`` =
    | [<CompiledName "r2_bucket">] R2_bucket
    member this.Format() =
        match this with
        | R2_bucket -> "r2_bucket"

type ``mqr2-producer`` =
    { bucket_name: Option<string>
      ``type``: Option<``mqr2-producerType``> }
    ///Creates an instance of mqr2-producer with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``mqr2-producer`` = { bucket_name = None; ``type`` = None }

type Script = Map<string, Newtonsoft.Json.Linq.JToken>
type Scriptname = Map<string, Newtonsoft.Json.Linq.JToken>

type ``mqworker-consumerSettings`` =
    { ///The maximum number of messages to include in a batch.
      batch_size: Option<``mqbatch-size``>
      ///Maximum number of concurrent consumers that may consume from this Queue. Set to `null` to automatically opt in to the platform's maximum (recommended).
      max_concurrency: Option<``mqmax-concurrency``>
      ///The maximum number of retries
      max_retries: Option<``mqmax-retries``>
      ///The number of milliseconds to wait for a batch to fill up before attempting to deliver it
      max_wait_time_ms: Option<``mqmax-wait-time``>
      ///The number of seconds to delay before making the message available for another attempt.
      retry_delay: Option<``mqretry-delay``> }
    ///Creates an instance of mqworker-consumerSettings with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``mqworker-consumerSettings`` =
        { batch_size = None
          max_concurrency = None
          max_retries = None
          max_wait_time_ms = None
          retry_delay = None }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type ``mqworker-consumerType`` =
    | [<CompiledName "worker">] Worker
    member this.Format() =
        match this with
        | Worker -> "worker"

type ``mqworker-consumer`` =
    { ///A Resource identifier.
      consumer_id: Option<mqidentifier>
      created_on: Option<string>
      ///A Resource identifier.
      queue_id: Option<mqidentifier>
      script: Option<Script>
      script_name: Option<Scriptname>
      settings: Option<``mqworker-consumerSettings``>
      ``type``: Option<``mqworker-consumerType``> }
    ///Creates an instance of mqworker-consumer with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``mqworker-consumer`` =
        { consumer_id = None
          created_on = None
          queue_id = None
          script = None
          script_name = None
          settings = None
          ``type`` = None }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type ``mqworker-producerType`` =
    | [<CompiledName "worker">] Worker
    member this.Format() =
        match this with
        | Worker -> "worker"

type ``mqworker-producer`` =
    { script: Option<string>
      ``type``: Option<``mqworker-producerType``> }
    ///Creates an instance of mqworker-producer with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``mqworker-producer`` = { script = None; ``type`` = None }

type QueuesList_OKErrors = { code: int; message: string }

type Resultinfo =
    { ///Total number of queues
      count: Option<float>
      ///Current page within paginated list of queues
      page: Option<float>
      ///Number of queues per page
      per_page: Option<float>
      ///Total queues available without any search parameters
      total_count: Option<float>
      ///Total pages available without any search parameters
      total_pages: Option<float> }

type QueuesList_OK =
    { errors: Option<list<QueuesList_OKErrors>>
      messages: Option<``mqapi-v4-message``>
      ///Indicates if the API call was successful or not.
      success: Option<bool>
      result: Option<list<mqqueue>>
      result_info: Option<Resultinfo> }

[<RequireQualifiedAccess>]
type QueuesList =
    ///List of all Queues that belong to this account
    | OK of payload: QueuesList_OK

type QueuesCreatePayload =
    { queue_name: ``mqqueue-name`` }
    ///Creates an instance of QueuesCreatePayload with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (queue_name: ``mqqueue-name``): QueuesCreatePayload = { queue_name = queue_name }

type QueuesCreate_OKErrors = { code: int; message: string }

type QueuesCreate_OK =
    { errors: Option<list<QueuesCreate_OKErrors>>
      messages: Option<``mqapi-v4-message``>
      ///Indicates if the API call was successful or not.
      success: Option<bool>
      result: Option<mqqueue> }

[<RequireQualifiedAccess>]
type QueuesCreate =
    ///Created Queue
    | OK of payload: QueuesCreate_OK

[<RequireQualifiedAccess>]
type QueuesDelete =
    ///Successful delete
    | OK of payload: ``mqapi-v4-success``

type QueuesGet_OKErrors = { code: int; message: string }

type QueuesGet_OK =
    { errors: Option<list<QueuesGet_OKErrors>>
      messages: Option<``mqapi-v4-message``>
      ///Indicates if the API call was successful or not.
      success: Option<bool>
      result: Option<mqqueue> }

[<RequireQualifiedAccess>]
type QueuesGet =
    ///Details of the requested Queue
    | OK of payload: QueuesGet_OK

type QueuesUpdatePartial_OKErrors = { code: int; message: string }

type Result =
    { consumers: Option<list<mqconsumer>>
      consumers_total_count: Option<float>
      created_on: Option<string>
      modified_on: Option<string>
      producers: Option<list<mqproducer>>
      producers_total_count: Option<float>
      queue_id: Option<string>
      queue_name: Option<``mqqueue-name``>
      settings: Option<``mqqueue-settings``> }

type QueuesUpdatePartial_OK =
    { errors: Option<list<QueuesUpdatePartial_OKErrors>>
      messages: Option<``mqapi-v4-message``>
      ///Indicates if the API call was successful or not.
      success: Option<bool>
      result: Option<Result> }

[<RequireQualifiedAccess>]
type QueuesUpdatePartial =
    ///Updated Queue
    | OK of payload: QueuesUpdatePartial_OK

type QueuesUpdate_OKErrors = { code: int; message: string }

type QueuesUpdate_OKResult =
    { consumers: Option<list<mqconsumer>>
      consumers_total_count: Option<float>
      created_on: Option<string>
      modified_on: Option<string>
      producers: Option<list<mqproducer>>
      producers_total_count: Option<float>
      queue_id: Option<string>
      queue_name: Option<``mqqueue-name``>
      settings: Option<``mqqueue-settings``> }

type QueuesUpdate_OK =
    { errors: Option<list<QueuesUpdate_OKErrors>>
      messages: Option<``mqapi-v4-message``>
      ///Indicates if the API call was successful or not.
      success: Option<bool>
      result: Option<QueuesUpdate_OKResult> }

[<RequireQualifiedAccess>]
type QueuesUpdate =
    ///Updated Queue
    | OK of payload: QueuesUpdate_OK

type QueuesListConsumers_OKErrors = { code: int; message: string }

type QueuesListConsumers_OK =
    { errors: Option<list<QueuesListConsumers_OKErrors>>
      messages: Option<``mqapi-v4-message``>
      ///Indicates if the API call was successful or not.
      success: Option<bool>
      result: Option<list<mqconsumer>> }

[<RequireQualifiedAccess>]
type QueuesListConsumers =
    ///All consumers attached to this Queue
    | OK of payload: QueuesListConsumers_OK

type QueuesCreateConsumer_OKErrors = { code: int; message: string }

type QueuesCreateConsumer_OK =
    { errors: Option<list<QueuesCreateConsumer_OKErrors>>
      messages: Option<``mqapi-v4-message``>
      ///Indicates if the API call was successful or not.
      success: Option<bool>
      result: Option<Newtonsoft.Json.Linq.JObject> }

[<RequireQualifiedAccess>]
type QueuesCreateConsumer =
    ///Create Queue Consumer response.
    | OK of payload: QueuesCreateConsumer_OK

[<RequireQualifiedAccess>]
type QueuesDeleteConsumer =
    ///Successful consumer delete
    | OK of payload: ``mqapi-v4-success``

type QueuesGetConsumer_OKErrors = { code: int; message: string }

type QueuesGetConsumer_OK =
    { errors: Option<list<QueuesGetConsumer_OKErrors>>
      messages: Option<``mqapi-v4-message``>
      ///Indicates if the API call was successful or not.
      success: Option<bool>
      result: Option<Newtonsoft.Json.Linq.JObject> }

[<RequireQualifiedAccess>]
type QueuesGetConsumer =
    ///Get Queue Consumer response.
    | OK of payload: QueuesGetConsumer_OK

type QueuesUpdateConsumer_OKErrors = { code: int; message: string }

type QueuesUpdateConsumer_OK =
    { errors: Option<list<QueuesUpdateConsumer_OKErrors>>
      messages: Option<``mqapi-v4-message``>
      ///Indicates if the API call was successful or not.
      success: Option<bool>
      result: Option<Newtonsoft.Json.Linq.JObject> }

[<RequireQualifiedAccess>]
type QueuesUpdateConsumer =
    ///Update Queue Consumer response.
    | OK of payload: QueuesUpdateConsumer_OK

[<RequireQualifiedAccess>]
type QueuesPushMessage =
    ///Successful message ingestion
    | OK of payload: ``mqapi-v4-success``

type Acks =
    { ///An ID that represents an "in-flight" message that has been pulled from a Queue. You must hold on to this ID and use it to acknowledge this message.
      lease_id: Option<``mqlease-id``> }
    ///Creates an instance of Acks with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): Acks = { lease_id = None }

type Retries =
    { ///The number of seconds to delay before making the message available for another attempt.
      delay_seconds: Option<``mqretry-delay``>
      ///An ID that represents an "in-flight" message that has been pulled from a Queue. You must hold on to this ID and use it to acknowledge this message.
      lease_id: Option<``mqlease-id``> }
    ///Creates an instance of Retries with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): Retries =
        { delay_seconds = None
          lease_id = None }

type QueuesAckMessagesPayload =
    { acks: Option<list<Acks>>
      retries: Option<list<Retries>> }
    ///Creates an instance of QueuesAckMessagesPayload with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): QueuesAckMessagesPayload = { acks = None; retries = None }

type QueuesAckMessages_OKErrors = { code: int; message: string }

type QueuesAckMessages_OKResult =
    { ///The number of messages that were succesfully acknowledged.
      ackCount: Option<float>
      ///The number of messages that were succesfully retried.
      retryCount: Option<float>
      warnings: Option<list<string>> }

type QueuesAckMessages_OK =
    { errors: Option<list<QueuesAckMessages_OKErrors>>
      messages: Option<``mqapi-v4-message``>
      ///Indicates if the API call was successful or not.
      success: Option<bool>
      result: Option<QueuesAckMessages_OKResult> }

[<RequireQualifiedAccess>]
type QueuesAckMessages =
    ///Details of ACKs and retries
    | OK of payload: QueuesAckMessages_OK

[<RequireQualifiedAccess>]
type QueuesPushMessages =
    ///Successful batch ingestion
    | OK of payload: ``mqapi-v4-success``

type QueuesPullMessagesPayload =
    { ///The maximum number of messages to include in a batch.
      batch_size: Option<``mqbatch-size``>
      ///The number of milliseconds that a message is exclusively leased. After the timeout, the message becomes available for another attempt.
      visibility_timeout_ms: Option<``mqvisibility-timeout``> }
    ///Creates an instance of QueuesPullMessagesPayload with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): QueuesPullMessagesPayload =
        { batch_size = None
          visibility_timeout_ms = None }

type QueuesPullMessages_OKErrors = { code: int; message: string }

type Messages =
    { attempts: Option<float>
      body: Option<string>
      id: Option<string>
      ///An ID that represents an "in-flight" message that has been pulled from a Queue. You must hold on to this ID and use it to acknowledge this message.
      lease_id: Option<``mqlease-id``>
      metadata: Option<Newtonsoft.Json.Linq.JObject>
      timestamp_ms: Option<float> }

type QueuesPullMessages_OKResult =
    { ///The number of unacknowledged messages in the queue
      message_backlog_count: Option<float>
      messages: Option<list<Messages>> }

type QueuesPullMessages_OK =
    { errors: Option<list<QueuesPullMessages_OKErrors>>
      messages: Option<``mqapi-v4-message``>
      ///Indicates if the API call was successful or not.
      success: Option<bool>
      result: Option<QueuesPullMessages_OKResult> }

[<RequireQualifiedAccess>]
type QueuesPullMessages =
    ///A batch of messages in the Queue
    | OK of payload: QueuesPullMessages_OK

type QueuesPurgeGet_OKErrors = { code: int; message: string }

type QueuesPurgeGet_OKResult =
    { ///Indicates if the last purge operation completed successfully.
      completed: Option<string>
      ///Timestamp when the last purge operation started.
      started_at: Option<string> }

type QueuesPurgeGet_OK =
    { errors: Option<list<QueuesPurgeGet_OKErrors>>
      messages: Option<``mqapi-v4-message``>
      ///Indicates if the API call was successful or not.
      success: Option<bool>
      result: Option<QueuesPurgeGet_OKResult> }

[<RequireQualifiedAccess>]
type QueuesPurgeGet =
    ///Details of the requested Queue
    | OK of payload: QueuesPurgeGet_OK

type QueuesPurgePayload =
    { ///Confimation that all messages will be deleted permanently.
      delete_messages_permanently: Option<bool> }
    ///Creates an instance of QueuesPurgePayload with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): QueuesPurgePayload = { delete_messages_permanently = None }

type QueuesPurge_OKErrors = { code: int; message: string }

type QueuesPurge_OKResult =
    { consumers: Option<list<mqconsumer>>
      consumers_total_count: Option<float>
      created_on: Option<string>
      modified_on: Option<string>
      producers: Option<list<mqproducer>>
      producers_total_count: Option<float>
      queue_id: Option<string>
      queue_name: Option<``mqqueue-name``>
      settings: Option<``mqqueue-settings``> }

type QueuesPurge_OK =
    { errors: Option<list<QueuesPurge_OKErrors>>
      messages: Option<``mqapi-v4-message``>
      ///Indicates if the API call was successful or not.
      success: Option<bool>
      result: Option<QueuesPurge_OKResult> }

[<RequireQualifiedAccess>]
type QueuesPurge =
    ///Updated Queue
    | OK of payload: QueuesPurge_OK
