namespace rec CloudFlare.Management.Analytics.Types

///Identifier
type ``argo-analyticsidentifier`` = string

type ``argo-analyticsmessagesArrayItem`` =
    { code: int
      message: string }
    ///Creates an instance of argo-analyticsmessagesArrayItem with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``argo-analyticsmessagesArrayItem`` =
        { code = code; message = message }

type ``argo-analyticsmessages`` = list<``argo-analyticsmessagesArrayItem``>

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

type ``argo-analyticsapi-response-common`` =
    { errors: list<Errors>
      messages: list<Messages>
      result: Newtonsoft.Json.Linq.JToken
      ///Whether the API call was successful
      success: bool }
    ///Creates an instance of argo-analyticsapi-response-common with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: list<Errors>,
                          messages: list<Messages>,
                          result: Newtonsoft.Json.Linq.JToken,
                          success: bool): ``argo-analyticsapi-response-common`` =
        { errors = errors
          messages = messages
          result = result
          success = success }

type ``argo-analyticsapi-response-common-failure`` =
    { errors: Newtonsoft.Json.Linq.JToken
      messages: Newtonsoft.Json.Linq.JToken
      result: Newtonsoft.Json.Linq.JObject
      ///Whether the API call was successful
      success: bool }
    ///Creates an instance of argo-analyticsapi-response-common-failure with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: Newtonsoft.Json.Linq.JToken,
                          messages: Newtonsoft.Json.Linq.JToken,
                          result: Newtonsoft.Json.Linq.JObject,
                          success: bool): ``argo-analyticsapi-response-common-failure`` =
        { errors = errors
          messages = messages
          result = result
          success = success }

type ``argo-analyticsapi-response-singleErrors`` =
    { code: int
      message: string }
    ///Creates an instance of argo-analyticsapi-response-singleErrors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``argo-analyticsapi-response-singleErrors`` =
        { code = code; message = message }

type ``argo-analyticsapi-response-singleMessages`` =
    { code: int
      message: string }
    ///Creates an instance of argo-analyticsapi-response-singleMessages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``argo-analyticsapi-response-singleMessages`` =
        { code = code; message = message }

type ``argo-analyticsapi-response-single`` =
    { errors: Option<list<``argo-analyticsapi-response-singleErrors``>>
      messages: Option<list<``argo-analyticsapi-response-singleMessages``>>
      result: Option<Newtonsoft.Json.Linq.JToken>
      ///Whether the API call was successful
      success: Option<bool> }
    ///Creates an instance of argo-analyticsapi-response-single with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``argo-analyticsapi-response-single`` =
        { errors = None
          messages = None
          result = None
          success = None }

type ``argo-analyticsresponsesingleErrors`` =
    { code: int
      message: string }
    ///Creates an instance of argo-analyticsresponsesingleErrors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``argo-analyticsresponsesingleErrors`` =
        { code = code; message = message }

type ``argo-analyticsresponsesingleMessages`` =
    { code: int
      message: string }
    ///Creates an instance of argo-analyticsresponsesingleMessages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``argo-analyticsresponsesingleMessages`` =
        { code = code; message = message }

type ``argo-analyticsresponsesingle`` =
    { errors: Option<list<``argo-analyticsresponsesingleErrors``>>
      messages: Option<list<``argo-analyticsresponsesingleMessages``>>
      result: Option<Newtonsoft.Json.Linq.JToken>
      ///Whether the API call was successful
      success: Option<bool> }
    ///Creates an instance of argo-analyticsresponsesingle with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``argo-analyticsresponsesingle`` =
        { errors = None
          messages = None
          result = None
          success = None }

[<RequireQualifiedAccess>]
type ArgoAnalyticsForZoneArgoAnalyticsForAZone =
    ///Argo Analytics for a zone response
    | OK of payload: ``argo-analyticsresponsesingle``
