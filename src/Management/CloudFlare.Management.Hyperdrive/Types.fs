namespace rec CloudFlare.Management.Hyperdrive.Types

type ``hyperdrivehyperdrive-name`` = string
type ``hyperdrivehyperdrive-origin-connection-limit`` = int

///Specifies the URL scheme used to connect to your origin database.
[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type ``hyperdrivehyperdrive-scheme`` =
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

type ``hyperdriveapi-response-collection`` =
    { errors: Option<list<Errors>>
      messages: Option<list<Messages>>
      result: Option<Newtonsoft.Json.Linq.JObject>
      ///Return the status of the API call success.
      success: Option<bool>
      result_info: Option<hyperdriveresultinfo> }
    ///Creates an instance of hyperdriveapi-response-collection with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``hyperdriveapi-response-collection`` =
        { errors = None
          messages = None
          result = None
          success = None
          result_info = None }

type ``hyperdriveapi-response-commonErrors`` =
    { code: int
      message: string }
    ///Creates an instance of hyperdriveapi-response-commonErrors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``hyperdriveapi-response-commonErrors`` =
        { code = code; message = message }

type ``hyperdriveapi-response-commonMessages`` =
    { code: int
      message: string }
    ///Creates an instance of hyperdriveapi-response-commonMessages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``hyperdriveapi-response-commonMessages`` =
        { code = code; message = message }

type ``hyperdriveapi-response-common`` =
    { errors: list<``hyperdriveapi-response-commonErrors``>
      messages: list<``hyperdriveapi-response-commonMessages``>
      result: Newtonsoft.Json.Linq.JObject
      ///Return the status of the API call success.
      success: bool }
    ///Creates an instance of hyperdriveapi-response-common with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: list<``hyperdriveapi-response-commonErrors``>,
                          messages: list<``hyperdriveapi-response-commonMessages``>,
                          result: Newtonsoft.Json.Linq.JObject,
                          success: bool): ``hyperdriveapi-response-common`` =
        { errors = errors
          messages = messages
          result = result
          success = success }

type ``hyperdriveapi-response-common-failure`` =
    { errors: Newtonsoft.Json.Linq.JToken
      messages: Newtonsoft.Json.Linq.JToken
      result: Newtonsoft.Json.Linq.JObject
      ///Return the status of the API call success.
      success: bool }
    ///Creates an instance of hyperdriveapi-response-common-failure with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: Newtonsoft.Json.Linq.JToken,
                          messages: Newtonsoft.Json.Linq.JToken,
                          result: Newtonsoft.Json.Linq.JObject,
                          success: bool): ``hyperdriveapi-response-common-failure`` =
        { errors = errors
          messages = messages
          result = result
          success = success }

type ``hyperdriveapi-response-singleErrors`` =
    { code: int
      message: string }
    ///Creates an instance of hyperdriveapi-response-singleErrors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``hyperdriveapi-response-singleErrors`` =
        { code = code; message = message }

type ``hyperdriveapi-response-singleMessages`` =
    { code: int
      message: string }
    ///Creates an instance of hyperdriveapi-response-singleMessages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``hyperdriveapi-response-singleMessages`` =
        { code = code; message = message }

type ``hyperdriveapi-response-single`` =
    { errors: Option<list<``hyperdriveapi-response-singleErrors``>>
      messages: Option<list<``hyperdriveapi-response-singleMessages``>>
      result: Option<Newtonsoft.Json.Linq.JObject>
      ///Return the status of the API call success.
      success: Option<bool> }
    ///Creates an instance of hyperdriveapi-response-single with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``hyperdriveapi-response-single`` =
        { errors = None
          messages = None
          result = None
          success = None }

type ``hyperdrivehyperdrive-caching-common`` =
    { ///Set to true to disable caching of SQL responses. Default is false.
      disabled: Option<bool> }
    ///Creates an instance of hyperdrivehyperdrive-caching-common with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``hyperdrivehyperdrive-caching-common`` = { disabled = None }

type ``hyperdrivehyperdrive-caching-disabled`` =
    { ///Set to true to disable caching of SQL responses. Default is false.
      disabled: Option<bool> }
    ///Creates an instance of hyperdrivehyperdrive-caching-disabled with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``hyperdrivehyperdrive-caching-disabled`` = { disabled = None }

type ``hyperdrivehyperdrive-caching-enabled`` =
    { ///Set to true to disable caching of SQL responses. Default is false.
      disabled: Option<bool>
      ///Specify the maximum duration items should persist in the cache. Not returned if set to the default (60).
      max_age: Option<int>
      ///Specify the number of seconds the cache may serve a stale response. Omitted if set to the default (15).
      stale_while_revalidate: Option<int> }
    ///Creates an instance of hyperdrivehyperdrive-caching-enabled with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``hyperdrivehyperdrive-caching-enabled`` =
        { disabled = None
          max_age = None
          stale_while_revalidate = None }

type ``hyperdrivehyperdrive-config`` =
    { caching: Option<Newtonsoft.Json.Linq.JObject>
      ///Defines the creation time of the Hyperdrive configuration.
      created_on: Option<System.DateTimeOffset>
      ///Define configurations using a unique string identifier.
      id: hyperdriveidentifier
      ///Defines the last modified time of the Hyperdrive configuration.
      modified_on: Option<System.DateTimeOffset>
      mtls: Option<``hyperdrivehyperdrive-mtls``>
      name: ``hyperdrivehyperdrive-name``
      origin: Newtonsoft.Json.Linq.JObject
      ///The (soft) maximum number of connections the Hyperdrive is allowed to make to the origin database.
      origin_connection_limit: Option<``hyperdrivehyperdrive-origin-connection-limit``> }
    ///Creates an instance of hyperdrivehyperdrive-config with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (id: hyperdriveidentifier,
                          name: ``hyperdrivehyperdrive-name``,
                          origin: Newtonsoft.Json.Linq.JObject): ``hyperdrivehyperdrive-config`` =
        { caching = None
          created_on = None
          id = id
          modified_on = None
          mtls = None
          name = name
          origin = origin
          origin_connection_limit = None }

type Origin = Map<string, Newtonsoft.Json.Linq.JToken>

type ``hyperdrivehyperdrive-config-patch`` =
    { caching: Option<Newtonsoft.Json.Linq.JObject>
      mtls: Option<``hyperdrivehyperdrive-mtls``>
      name: Option<``hyperdrivehyperdrive-name``>
      origin: Option<Origin>
      ///The (soft) maximum number of connections the Hyperdrive is allowed to make to the origin database.
      origin_connection_limit: Option<``hyperdrivehyperdrive-origin-connection-limit``> }
    ///Creates an instance of hyperdrivehyperdrive-config-patch with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``hyperdrivehyperdrive-config-patch`` =
        { caching = None
          mtls = None
          name = None
          origin = None
          origin_connection_limit = None }

type ``hyperdrivehyperdrive-config-response`` =
    { caching: Newtonsoft.Json.Linq.JObject
      ///Defines the creation time of the Hyperdrive configuration.
      created_on: Option<System.DateTimeOffset>
      ///Define configurations using a unique string identifier.
      id: Option<hyperdriveidentifier>
      ///Defines the last modified time of the Hyperdrive configuration.
      modified_on: Option<System.DateTimeOffset>
      mtls: Option<``hyperdrivehyperdrive-mtls``>
      name: Option<``hyperdrivehyperdrive-name``>
      origin: Option<Newtonsoft.Json.Linq.JObject>
      ///The (soft) maximum number of connections the Hyperdrive is allowed to make to the origin database.
      origin_connection_limit: Option<``hyperdrivehyperdrive-origin-connection-limit``> }
    ///Creates an instance of hyperdrivehyperdrive-config-response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (caching: Newtonsoft.Json.Linq.JObject): ``hyperdrivehyperdrive-config-response`` =
        { caching = caching
          created_on = None
          id = None
          modified_on = None
          mtls = None
          name = None
          origin = None
          origin_connection_limit = None }

type ``hyperdrivehyperdrive-database`` =
    { ///Set the name of your origin database.
      database: Option<string>
      ///Set the password needed to access your origin database. The API never returns this write-only value.
      password: Option<string>
      ///Specifies the URL scheme used to connect to your origin database.
      scheme: Option<``hyperdrivehyperdrive-scheme``>
      ///Set the user of your origin database.
      user: Option<string> }
    ///Creates an instance of hyperdrivehyperdrive-database with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``hyperdrivehyperdrive-database`` =
        { database = None
          password = None
          scheme = None
          user = None }

type ``hyperdrivehyperdrive-database-full`` =
    { ///Set the name of your origin database.
      database: string
      ///Set the password needed to access your origin database. The API never returns this write-only value.
      password: string
      ///Specifies the URL scheme used to connect to your origin database.
      scheme: ``hyperdrivehyperdrive-scheme``
      ///Set the user of your origin database.
      user: string }
    ///Creates an instance of hyperdrivehyperdrive-database-full with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (database: string, password: string, scheme: ``hyperdrivehyperdrive-scheme``, user: string): ``hyperdrivehyperdrive-database-full`` =
        { database = database
          password = password
          scheme = scheme
          user = user }

type ``hyperdrivehyperdrive-mtls`` =
    { ///Define CA certificate ID obtained after uploading CA cert.
      ca_certificate_id: Option<string>
      ///Define mTLS certificate ID obtained after uploading client cert.
      mtls_certificate_id: Option<string>
      ///Set SSL mode to 'require', 'verify-ca', or 'verify-full' to verify the CA.
      sslmode: Option<string> }
    ///Creates an instance of hyperdrivehyperdrive-mtls with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``hyperdrivehyperdrive-mtls`` =
        { ca_certificate_id = None
          mtls_certificate_id = None
          sslmode = None }

type ``hyperdrivehyperdrive-origin`` =
    { ///Set the name of your origin database.
      database: Option<string>
      ///Set the password needed to access your origin database. The API never returns this write-only value.
      password: Option<string>
      ///Specifies the URL scheme used to connect to your origin database.
      scheme: Option<``hyperdrivehyperdrive-scheme``>
      ///Set the user of your origin database.
      user: Option<string> }
    ///Creates an instance of hyperdrivehyperdrive-origin with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``hyperdrivehyperdrive-origin`` =
        { database = None
          password = None
          scheme = None
          user = None }

type ``hyperdriveinternet-origin`` =
    { ///Defines the host (hostname or IP) of your origin database.
      host: string
      ///Defines the port (default: 5432 for Postgres) of your origin database.
      port: int }
    ///Creates an instance of hyperdriveinternet-origin with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (host: string, port: int): ``hyperdriveinternet-origin`` = { host = host; port = port }

type ``hyperdriveover-access-origin`` =
    { ///Defines the Client ID of the Access token to use when connecting to the origin database.
      access_client_id: string
      ///Defines the Client Secret of the Access Token to use when connecting to the origin database. The API never returns this write-only value.
      access_client_secret: string
      ///Defines the host (hostname or IP) of your origin database.
      host: string }
    ///Creates an instance of hyperdriveover-access-origin with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (access_client_id: string, access_client_secret: string, host: string): ``hyperdriveover-access-origin`` =
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
