namespace rec CloudFlare.Management.Analytics.Types

///Identifier
type argoanalyticsidentifier = string

type argoanalyticsmessagesArrayItem =
    { code: int
      message: string }
    ///Creates an instance of argoanalyticsmessagesArrayItem with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): argoanalyticsmessagesArrayItem =
        { code = code; message = message }

type argoanalyticsmessages = list<argoanalyticsmessagesArrayItem>

type Errors =
    { code: int
      message: string }
    ///Creates an instance of Errors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): Errors = { code = code; message = message }

type Messages =
    { code: int
      message: string }
    ///Creates an instance of Messages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): Messages = { code = code; message = message }

type argoanalyticsapiresponsecommon =
    { errors: list<Errors>
      messages: list<Messages>
      result: System.Text.Json.JsonElement
      ///Whether the API call was successful
      success: bool }
    ///Creates an instance of argoanalyticsapiresponsecommon with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: list<Errors>,
                          messages: list<Messages>,
                          result: System.Text.Json.JsonElement,
                          success: bool): argoanalyticsapiresponsecommon =
        { errors = errors
          messages = messages
          result = result
          success = success }

type argoanalyticsapiresponsecommonfailure =
    { errors: System.Text.Json.JsonElement
      messages: System.Text.Json.JsonElement
      result: System.Text.Json.JsonElement
      ///Whether the API call was successful
      success: bool }
    ///Creates an instance of argoanalyticsapiresponsecommonfailure with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: System.Text.Json.JsonElement,
                          messages: System.Text.Json.JsonElement,
                          result: System.Text.Json.JsonElement,
                          success: bool): argoanalyticsapiresponsecommonfailure =
        { errors = errors
          messages = messages
          result = result
          success = success }

type argoanalyticsapiresponsesingleErrors =
    { code: int
      message: string }
    ///Creates an instance of argoanalyticsapiresponsesingleErrors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): argoanalyticsapiresponsesingleErrors =
        { code = code; message = message }

type argoanalyticsapiresponsesingleMessages =
    { code: int
      message: string }
    ///Creates an instance of argoanalyticsapiresponsesingleMessages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): argoanalyticsapiresponsesingleMessages =
        { code = code; message = message }

type argoanalyticsapiresponsesingle =
    { errors: Option<list<argoanalyticsapiresponsesingleErrors>>
      messages: Option<list<argoanalyticsapiresponsesingleMessages>>
      result: Option<System.Text.Json.JsonElement>
      ///Whether the API call was successful
      success: Option<bool> }
    ///Creates an instance of argoanalyticsapiresponsesingle with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): argoanalyticsapiresponsesingle =
        { errors = None
          messages = None
          result = None
          success = None }

type argoanalyticsresponsesingleErrors =
    { code: int
      message: string }
    ///Creates an instance of argoanalyticsresponsesingleErrors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): argoanalyticsresponsesingleErrors =
        { code = code; message = message }

type argoanalyticsresponsesingleMessages =
    { code: int
      message: string }
    ///Creates an instance of argoanalyticsresponsesingleMessages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): argoanalyticsresponsesingleMessages =
        { code = code; message = message }

type argoanalyticsresponsesingle =
    { errors: Option<list<argoanalyticsresponsesingleErrors>>
      messages: Option<list<argoanalyticsresponsesingleMessages>>
      result: Option<System.Text.Json.JsonElement>
      ///Whether the API call was successful
      success: Option<bool> }
    ///Creates an instance of argoanalyticsresponsesingle with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): argoanalyticsresponsesingle =
        { errors = None
          messages = None
          result = None
          success = None }

[<RequireQualifiedAccess>]
type ArgoAnalyticsForZoneArgoAnalyticsForAZone =
    ///Argo Analytics for a zone response
    | OK of payload: argoanalyticsresponsesingle
