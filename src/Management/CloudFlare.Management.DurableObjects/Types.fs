namespace rec CloudFlare.Management.DurableObjects.Types

///Opaque token indicating the position from which to continue when requesting the next set of records. A valid value for the cursor can be obtained from the cursors object in the result_info structure.
type workerscursor = string
///Identifier.
type workersidentifier = string

type Source =
    { pointer: Option<string> }
    ///Creates an instance of Source with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): Source = { pointer = None }

type workersmessagesArrayItem =
    { code: int
      documentation_url: Option<string>
      message: string
      source: Option<Source> }
    ///Creates an instance of workersmessagesArrayItem with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): workersmessagesArrayItem =
        { code = code
          documentation_url = None
          message = message
          source = None }

type workersmessages = list<workersmessagesArrayItem>
///ID of the namespace.
type ``workersschemas-id`` = string

type ErrorsSource =
    { pointer: Option<string> }
    ///Creates an instance of ErrorsSource with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ErrorsSource = { pointer = None }

type Errors =
    { code: int
      documentation_url: Option<string>
      message: string
      source: Option<ErrorsSource> }
    ///Creates an instance of Errors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): Errors =
        { code = code
          documentation_url = None
          message = message
          source = None }

type MessagesSource =
    { pointer: Option<string> }
    ///Creates an instance of MessagesSource with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): MessagesSource = { pointer = None }

type Messages =
    { code: int
      documentation_url: Option<string>
      message: string
      source: Option<MessagesSource> }
    ///Creates an instance of Messages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): Messages =
        { code = code
          documentation_url = None
          message = message
          source = None }

type Resultinfo =
    { ///Total number of results for the requested service.
      count: Option<float>
      ///Current page within paginated list of results.
      page: Option<float>
      ///Number of results per page of results.
      per_page: Option<float>
      ///Total results available without any search parameters.
      total_count: Option<float> }
    ///Creates an instance of Resultinfo with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): Resultinfo =
        { count = None
          page = None
          per_page = None
          total_count = None }

type ``workersapi-response-collection`` =
    { errors: Option<list<Errors>>
      messages: Option<list<Messages>>
      ///Whether the API call was successful.
      success: Option<bool>
      result_info: Option<Resultinfo> }
    ///Creates an instance of workersapi-response-collection with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``workersapi-response-collection`` =
        { errors = None
          messages = None
          success = None
          result_info = None }

type ``workersapi-response-commonErrorsSource`` =
    { pointer: Option<string> }
    ///Creates an instance of workersapi-response-commonErrorsSource with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``workersapi-response-commonErrorsSource`` = { pointer = None }

type ``workersapi-response-commonErrors`` =
    { code: int
      documentation_url: Option<string>
      message: string
      source: Option<``workersapi-response-commonErrorsSource``> }
    ///Creates an instance of workersapi-response-commonErrors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``workersapi-response-commonErrors`` =
        { code = code
          documentation_url = None
          message = message
          source = None }

type ``workersapi-response-commonMessagesSource`` =
    { pointer: Option<string> }
    ///Creates an instance of workersapi-response-commonMessagesSource with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``workersapi-response-commonMessagesSource`` = { pointer = None }

type ``workersapi-response-commonMessages`` =
    { code: int
      documentation_url: Option<string>
      message: string
      source: Option<``workersapi-response-commonMessagesSource``> }
    ///Creates an instance of workersapi-response-commonMessages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``workersapi-response-commonMessages`` =
        { code = code
          documentation_url = None
          message = message
          source = None }

type ``workersapi-response-common`` =
    { errors: list<``workersapi-response-commonErrors``>
      messages: list<``workersapi-response-commonMessages``>
      ///Whether the API call was successful.
      success: bool }
    ///Creates an instance of workersapi-response-common with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: list<``workersapi-response-commonErrors``>,
                          messages: list<``workersapi-response-commonMessages``>,
                          success: bool): ``workersapi-response-common`` =
        { errors = errors
          messages = messages
          success = success }

type ``workersapi-response-common-failure`` =
    { errors: Newtonsoft.Json.Linq.JToken
      messages: Newtonsoft.Json.Linq.JToken
      result: Newtonsoft.Json.Linq.JObject
      ///Whether the API call was successful.
      success: bool }
    ///Creates an instance of workersapi-response-common-failure with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: Newtonsoft.Json.Linq.JToken,
                          messages: Newtonsoft.Json.Linq.JToken,
                          result: Newtonsoft.Json.Linq.JObject,
                          success: bool): ``workersapi-response-common-failure`` =
        { errors = errors
          messages = messages
          result = result
          success = success }

type workersnamespace =
    { ``class``: Option<string>
      id: Option<string>
      name: Option<string>
      script: Option<string>
      use_sqlite: Option<bool> }
    ///Creates an instance of workersnamespace with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): workersnamespace =
        { ``class`` = None
          id = None
          name = None
          script = None
          use_sqlite = None }

type workersobject =
    { ///Whether the Durable Object has stored data.
      hasStoredData: Option<bool>
      ///ID of the Durable Object.
      id: Option<string> }
    ///Creates an instance of workersobject with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): workersobject = { hasStoredData = None; id = None }

[<RequireQualifiedAccess>]
type DurableObjectsNamespaceListNamespaces =
    ///List Namespaces response.
    | OK of payload: string

[<RequireQualifiedAccess>]
type DurableObjectsNamespaceListObjects =
    ///List Objects response.
    | OK of payload: string
