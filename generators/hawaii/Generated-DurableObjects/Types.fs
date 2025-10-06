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
type workersschemasid = string

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

type workersapiresponsecollection =
    { errors: Option<list<Errors>>
      messages: Option<list<Messages>>
      ///Whether the API call was successful.
      success: Option<bool>
      result_info: Option<Resultinfo> }
    ///Creates an instance of workersapiresponsecollection with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): workersapiresponsecollection =
        { errors = None
          messages = None
          success = None
          result_info = None }

type workersapiresponsecommonErrorsSource =
    { pointer: Option<string> }
    ///Creates an instance of workersapiresponsecommonErrorsSource with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): workersapiresponsecommonErrorsSource = { pointer = None }

type workersapiresponsecommonErrors =
    { code: int
      documentation_url: Option<string>
      message: string
      source: Option<workersapiresponsecommonErrorsSource> }
    ///Creates an instance of workersapiresponsecommonErrors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): workersapiresponsecommonErrors =
        { code = code
          documentation_url = None
          message = message
          source = None }

type workersapiresponsecommonMessagesSource =
    { pointer: Option<string> }
    ///Creates an instance of workersapiresponsecommonMessagesSource with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): workersapiresponsecommonMessagesSource = { pointer = None }

type workersapiresponsecommonMessages =
    { code: int
      documentation_url: Option<string>
      message: string
      source: Option<workersapiresponsecommonMessagesSource> }
    ///Creates an instance of workersapiresponsecommonMessages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): workersapiresponsecommonMessages =
        { code = code
          documentation_url = None
          message = message
          source = None }

type workersapiresponsecommon =
    { errors: list<workersapiresponsecommonErrors>
      messages: list<workersapiresponsecommonMessages>
      ///Whether the API call was successful.
      success: bool }
    ///Creates an instance of workersapiresponsecommon with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: list<workersapiresponsecommonErrors>,
                          messages: list<workersapiresponsecommonMessages>,
                          success: bool): workersapiresponsecommon =
        { errors = errors
          messages = messages
          success = success }

type workersapiresponsecommonfailure =
    { errors: System.Text.Json.JsonElement
      messages: System.Text.Json.JsonElement
      result: System.Text.Json.JsonElement
      ///Whether the API call was successful.
      success: bool }
    ///Creates an instance of workersapiresponsecommonfailure with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: System.Text.Json.JsonElement,
                          messages: System.Text.Json.JsonElement,
                          result: System.Text.Json.JsonElement,
                          success: bool): workersapiresponsecommonfailure =
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
