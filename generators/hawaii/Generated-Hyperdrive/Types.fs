namespace rec CloudFlare.Management.Hyperdrive.Types

type hyperdrivehyperdrivename = string
type hyperdrivehyperdriveoriginconnectionlimit = int

///Specifies the URL scheme used to connect to your origin database.
[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type hyperdrivehyperdrivescheme =
    | [<CompiledName "postgres">] Postgres
    | [<CompiledName "postgresql">] Postgresql
    | [<CompiledName "mysql">] Mysql
    member this.Format() =
        match this with
        | Postgres -> "postgres"
        | Postgresql -> "postgresql"
        | Mysql -> "mysql"

///Define configurations using a unique string identifier.
type hyperdriveidentifier = string

type hyperdrivemessagesArrayItem =
    { code: int
      message: string }
    ///Creates an instance of hyperdrivemessagesArrayItem with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): hyperdrivemessagesArrayItem = { code = code; message = message }

type hyperdrivemessages = list<hyperdrivemessagesArrayItem>

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

type hyperdriveapiresponsecollection =
    { errors: Option<list<Errors>>
      messages: Option<list<Messages>>
      result: Option<System.Text.Json.JsonElement>
      ///Return the status of the API call success.
      success: Option<bool>
      result_info: Option<hyperdriveresultinfo> }
    ///Creates an instance of hyperdriveapiresponsecollection with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): hyperdriveapiresponsecollection =
        { errors = None
          messages = None
          result = None
          success = None
          result_info = None }

type hyperdriveapiresponsecommonErrors =
    { code: int
      message: string }
    ///Creates an instance of hyperdriveapiresponsecommonErrors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): hyperdriveapiresponsecommonErrors =
        { code = code; message = message }

type hyperdriveapiresponsecommonMessages =
    { code: int
      message: string }
    ///Creates an instance of hyperdriveapiresponsecommonMessages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): hyperdriveapiresponsecommonMessages =
        { code = code; message = message }

type hyperdriveapiresponsecommon =
    { errors: list<hyperdriveapiresponsecommonErrors>
      messages: list<hyperdriveapiresponsecommonMessages>
      result: System.Text.Json.JsonElement
      ///Return the status of the API call success.
      success: bool }
    ///Creates an instance of hyperdriveapiresponsecommon with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: list<hyperdriveapiresponsecommonErrors>,
                          messages: list<hyperdriveapiresponsecommonMessages>,
                          result: System.Text.Json.JsonElement,
                          success: bool): hyperdriveapiresponsecommon =
        { errors = errors
          messages = messages
          result = result
          success = success }

type hyperdriveapiresponsecommonfailure =
    { errors: System.Text.Json.JsonElement
      messages: System.Text.Json.JsonElement
      result: System.Text.Json.JsonElement
      ///Return the status of the API call success.
      success: bool }
    ///Creates an instance of hyperdriveapiresponsecommonfailure with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: System.Text.Json.JsonElement,
                          messages: System.Text.Json.JsonElement,
                          result: System.Text.Json.JsonElement,
                          success: bool): hyperdriveapiresponsecommonfailure =
        { errors = errors
          messages = messages
          result = result
          success = success }

type hyperdriveapiresponsesingleErrors =
    { code: int
      message: string }
    ///Creates an instance of hyperdriveapiresponsesingleErrors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): hyperdriveapiresponsesingleErrors =
        { code = code; message = message }

type hyperdriveapiresponsesingleMessages =
    { code: int
      message: string }
    ///Creates an instance of hyperdriveapiresponsesingleMessages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): hyperdriveapiresponsesingleMessages =
        { code = code; message = message }

type hyperdriveapiresponsesingle =
    { errors: Option<list<hyperdriveapiresponsesingleErrors>>
      messages: Option<list<hyperdriveapiresponsesingleMessages>>
      result: Option<System.Text.Json.JsonElement>
      ///Return the status of the API call success.
      success: Option<bool> }
    ///Creates an instance of hyperdriveapiresponsesingle with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): hyperdriveapiresponsesingle =
        { errors = None
          messages = None
          result = None
          success = None }

type hyperdrivehyperdrivecachingcommon =
    { ///Set to true to disable caching of SQL responses. Default is false.
      disabled: Option<bool> }
    ///Creates an instance of hyperdrivehyperdrivecachingcommon with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): hyperdrivehyperdrivecachingcommon = { disabled = None }

type hyperdrivehyperdrivecachingdisabled =
    { ///Set to true to disable caching of SQL responses. Default is false.
      disabled: Option<bool> }
    ///Creates an instance of hyperdrivehyperdrivecachingdisabled with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): hyperdrivehyperdrivecachingdisabled = { disabled = None }

type hyperdrivehyperdrivecachingenabled =
    { ///Set to true to disable caching of SQL responses. Default is false.
      disabled: Option<bool>
      ///Specify the maximum duration items should persist in the cache. Not returned if set to the default (60).
      max_age: Option<int>
      ///Specify the number of seconds the cache may serve a stale response. Omitted if set to the default (15).
      stale_while_revalidate: Option<int> }
    ///Creates an instance of hyperdrivehyperdrivecachingenabled with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): hyperdrivehyperdrivecachingenabled =
        { disabled = None
          max_age = None
          stale_while_revalidate = None }

type hyperdrivehyperdriveconfig =
    { caching: Option<System.Text.Json.JsonElement>
      ///Defines the creation time of the Hyperdrive configuration.
      created_on: Option<System.DateTimeOffset>
      ///Define configurations using a unique string identifier.
      id: hyperdriveidentifier
      ///Defines the last modified time of the Hyperdrive configuration.
      modified_on: Option<System.DateTimeOffset>
      mtls: Option<hyperdrivehyperdrivemtls>
      name: hyperdrivehyperdrivename
      origin: System.Text.Json.JsonElement
      ///The (soft) maximum number of connections the Hyperdrive is allowed to make to the origin database.
      origin_connection_limit: Option<hyperdrivehyperdriveoriginconnectionlimit> }
    ///Creates an instance of hyperdrivehyperdriveconfig with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (id: hyperdriveidentifier, name: hyperdrivehyperdrivename, origin: System.Text.Json.JsonElement): hyperdrivehyperdriveconfig =
        { caching = None
          created_on = None
          id = id
          modified_on = None
          mtls = None
          name = name
          origin = origin
          origin_connection_limit = None }

type Origin = Map<string, System.Text.Json.JsonElement>

type hyperdrivehyperdriveconfigpatch =
    { caching: Option<System.Text.Json.JsonElement>
      mtls: Option<hyperdrivehyperdrivemtls>
      name: Option<hyperdrivehyperdrivename>
      origin: Option<Origin>
      ///The (soft) maximum number of connections the Hyperdrive is allowed to make to the origin database.
      origin_connection_limit: Option<hyperdrivehyperdriveoriginconnectionlimit> }
    ///Creates an instance of hyperdrivehyperdriveconfigpatch with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): hyperdrivehyperdriveconfigpatch =
        { caching = None
          mtls = None
          name = None
          origin = None
          origin_connection_limit = None }

type hyperdrivehyperdriveconfigresponse =
    { caching: System.Text.Json.JsonElement
      ///Defines the creation time of the Hyperdrive configuration.
      created_on: Option<System.DateTimeOffset>
      ///Define configurations using a unique string identifier.
      id: Option<hyperdriveidentifier>
      ///Defines the last modified time of the Hyperdrive configuration.
      modified_on: Option<System.DateTimeOffset>
      mtls: Option<hyperdrivehyperdrivemtls>
      name: Option<hyperdrivehyperdrivename>
      origin: Option<System.Text.Json.JsonElement>
      ///The (soft) maximum number of connections the Hyperdrive is allowed to make to the origin database.
      origin_connection_limit: Option<hyperdrivehyperdriveoriginconnectionlimit> }
    ///Creates an instance of hyperdrivehyperdriveconfigresponse with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (caching: System.Text.Json.JsonElement): hyperdrivehyperdriveconfigresponse =
        { caching = caching
          created_on = None
          id = None
          modified_on = None
          mtls = None
          name = None
          origin = None
          origin_connection_limit = None }

type hyperdrivehyperdrivedatabase =
    { ///Set the name of your origin database.
      database: Option<string>
      ///Set the password needed to access your origin database. The API never returns this write-only value.
      password: Option<string>
      ///Specifies the URL scheme used to connect to your origin database.
      scheme: Option<hyperdrivehyperdrivescheme>
      ///Set the user of your origin database.
      user: Option<string> }
    ///Creates an instance of hyperdrivehyperdrivedatabase with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): hyperdrivehyperdrivedatabase =
        { database = None
          password = None
          scheme = None
          user = None }

type hyperdrivehyperdrivedatabasefull =
    { ///Set the name of your origin database.
      database: string
      ///Set the password needed to access your origin database. The API never returns this write-only value.
      password: string
      ///Specifies the URL scheme used to connect to your origin database.
      scheme: hyperdrivehyperdrivescheme
      ///Set the user of your origin database.
      user: string }
    ///Creates an instance of hyperdrivehyperdrivedatabasefull with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (database: string, password: string, scheme: hyperdrivehyperdrivescheme, user: string): hyperdrivehyperdrivedatabasefull =
        { database = database
          password = password
          scheme = scheme
          user = user }

type hyperdrivehyperdrivemtls =
    { ///Define CA certificate ID obtained after uploading CA cert.
      ca_certificate_id: Option<string>
      ///Define mTLS certificate ID obtained after uploading client cert.
      mtls_certificate_id: Option<string>
      ///Set SSL mode to 'require', 'verify-ca', or 'verify-full' to verify the CA.
      sslmode: Option<string> }
    ///Creates an instance of hyperdrivehyperdrivemtls with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): hyperdrivehyperdrivemtls =
        { ca_certificate_id = None
          mtls_certificate_id = None
          sslmode = None }

type hyperdrivehyperdriveorigin =
    { ///Set the name of your origin database.
      database: Option<string>
      ///Set the password needed to access your origin database. The API never returns this write-only value.
      password: Option<string>
      ///Specifies the URL scheme used to connect to your origin database.
      scheme: Option<hyperdrivehyperdrivescheme>
      ///Set the user of your origin database.
      user: Option<string> }
    ///Creates an instance of hyperdrivehyperdriveorigin with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): hyperdrivehyperdriveorigin =
        { database = None
          password = None
          scheme = None
          user = None }

type hyperdriveinternetorigin =
    { ///Defines the host (hostname or IP) of your origin database.
      host: string
      ///Defines the port (default: 5432 for Postgres) of your origin database.
      port: int }
    ///Creates an instance of hyperdriveinternetorigin with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (host: string, port: int): hyperdriveinternetorigin = { host = host; port = port }

type hyperdriveoveraccessorigin =
    { ///Defines the Client ID of the Access token to use when connecting to the origin database.
      access_client_id: string
      ///Defines the Client Secret of the Access Token to use when connecting to the origin database. The API never returns this write-only value.
      access_client_secret: string
      ///Defines the host (hostname or IP) of your origin database.
      host: string }
    ///Creates an instance of hyperdriveoveraccessorigin with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (access_client_id: string, access_client_secret: string, host: string): hyperdriveoveraccessorigin =
        { access_client_id = access_client_id
          access_client_secret = access_client_secret
          host = host }

type hyperdriveresultinfo =
    { ///Defines the total number of results for the requested service.
      count: Option<float>
      ///Defines the current page within paginated list of results.
      page: Option<float>
      ///Defines the number of results per page of results.
      per_page: Option<float>
      ///Defines the total results available without any search parameters.
      total_count: Option<float> }
    ///Creates an instance of hyperdriveresultinfo with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): hyperdriveresultinfo =
        { count = None
          page = None
          per_page = None
          total_count = None }

[<RequireQualifiedAccess>]
type ListHyperdrive =
    ///List Hyperdrives Response.
    | OK of payload: string

[<RequireQualifiedAccess>]
type CreateHyperdrive =
    ///Create Hyperdrive Response.
    | OK of payload: string

[<RequireQualifiedAccess>]
type DeleteHyperdrive =
    ///Delete Hyperdrive Response.
    | OK of payload: string

[<RequireQualifiedAccess>]
type GetHyperdrive =
    ///Get Hyperdrive Response.
    | OK of payload: string

[<RequireQualifiedAccess>]
type PatchHyperdrive =
    ///Patch Hyperdrive Response.
    | OK of payload: string

[<RequireQualifiedAccess>]
type UpdateHyperdrive =
    ///Update Hyperdrive Response.
    | OK of payload: string
