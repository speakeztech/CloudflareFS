namespace rec CloudFlare.Management.D1.Types

///Account identifier tag.
type d1accountidentifier = string
///Specifies the timestamp the resource was created as an ISO8601 string.
type d1createdat = System.DateTimeOffset
///D1 database identifier (UUID).
type d1databaseidentifier = string
///D1 database name.
type d1databasename = string
type d1databaseversion = string
type d1filesize = float

type d1messagesArrayItem =
    { code: int
      message: string }
    ///Creates an instance of d1messagesArrayItem with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): d1messagesArrayItem = { code = code; message = message }

type d1messages = list<d1messagesArrayItem>
type d1params = list<string>

///Specify the region to create the D1 primary, if available. If this option is omitted, the D1 will be created as close as possible to the current user.
[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type d1primarylocationhint =
    | [<CompiledName "wnam">] Wnam
    | [<CompiledName "enam">] Enam
    | [<CompiledName "weur">] Weur
    | [<CompiledName "eeur">] Eeur
    | [<CompiledName "apac">] Apac
    | [<CompiledName "oc">] Oc
    member this.Format() =
        match this with
        | Wnam -> "wnam"
        | Enam -> "enam"
        | Weur -> "weur"
        | Eeur -> "eeur"
        | Apac -> "apac"
        | Oc -> "oc"

///The read replication mode for the database. Use 'auto' to create replicas and allow D1 automatically place them around the world, or 'disabled' to not use any database replicas (it can take a few hours for all replicas to be deleted).
[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type d1readreplicationmode =
    | [<CompiledName "auto">] Auto
    | [<CompiledName "disabled">] Disabled
    member this.Format() =
        match this with
        | Auto -> "auto"
        | Disabled -> "disabled"

///Region location hint of the database instance that handled the query.
[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type d1servedbyregion =
    | [<CompiledName "WNAM">] WNAM
    | [<CompiledName "ENAM">] ENAM
    | [<CompiledName "WEUR">] WEUR
    | [<CompiledName "EEUR">] EEUR
    | [<CompiledName "APAC">] APAC
    | [<CompiledName "OC">] OC
    member this.Format() =
        match this with
        | WNAM -> "WNAM"
        | ENAM -> "ENAM"
        | WEUR -> "WEUR"
        | EEUR -> "EEUR"
        | APAC -> "APAC"
        | OC -> "OC"

///Your SQL query. Supports multiple statements, joined by semicolons, which will be executed as a batch.
type d1sql = string
type d1tablecount = float

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

type d1apiresponsecommon =
    { errors: list<Errors>
      messages: list<Messages>
      result: Option<System.Text.Json.JsonElement>
      ///Whether the API call was successful
      success: bool }
    ///Creates an instance of d1apiresponsecommon with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: list<Errors>,
                          messages: list<Messages>,
                          success: bool): d1apiresponsecommon =
        { errors = errors
          messages = messages
          result = None
          success = success }

type d1apiresponsecommonfailure =
    { errors: System.Text.Json.JsonElement
      messages: System.Text.Json.JsonElement
      result: System.Text.Json.JsonElement
      ///Whether the API call was successful
      success: bool }
    ///Creates an instance of d1apiresponsecommonfailure with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: System.Text.Json.JsonElement,
                          messages: System.Text.Json.JsonElement,
                          result: System.Text.Json.JsonElement,
                          success: bool): d1apiresponsecommonfailure =
        { errors = errors
          messages = messages
          result = result
          success = success }

///The details of the D1 database.
type d1databasedetailsresponse =
    { ///Specifies the timestamp the resource was created as an ISO8601 string.
      created_at: Option<d1createdat>
      ///The D1 database's size, in bytes.
      file_size: Option<d1filesize>
      ///D1 database name.
      name: Option<d1databasename>
      num_tables: Option<d1tablecount>
      ///Configuration for D1 read replication.
      read_replication: Option<d1readreplicationdetails>
      ///D1 database identifier (UUID).
      uuid: Option<d1databaseidentifier>
      version: Option<d1databaseversion> }
    ///Creates an instance of d1databasedetailsresponse with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): d1databasedetailsresponse =
        { created_at = None
          file_size = None
          name = None
          num_tables = None
          read_replication = None
          uuid = None
          version = None }

type d1databaseresponse =
    { ///Specifies the timestamp the resource was created as an ISO8601 string.
      created_at: Option<d1createdat>
      ///D1 database name.
      name: Option<d1databasename>
      ///D1 database identifier (UUID).
      uuid: Option<d1databaseidentifier>
      version: Option<d1databaseversion> }
    ///Creates an instance of d1databaseresponse with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): d1databaseresponse =
        { created_at = None
          name = None
          uuid = None
          version = None }

///Configuration for D1 read replication.
type Readreplication =
    { ///The read replication mode for the database. Use 'auto' to create replicas and allow D1 automatically place them around the world, or 'disabled' to not use any database replicas (it can take a few hours for all replicas to be deleted).
      mode: d1readreplicationmode }
    ///Creates an instance of Readreplication with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (mode: d1readreplicationmode): Readreplication = { mode = mode }

type d1databaseupdatepartialrequestbody =
    { ///Configuration for D1 read replication.
      read_replication: Option<Readreplication> }
    ///Creates an instance of d1databaseupdatepartialrequestbody with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): d1databaseupdatepartialrequestbody = { read_replication = None }

///Configuration for D1 read replication.
type d1databaseupdaterequestbodyReadreplication =
    { ///The read replication mode for the database. Use 'auto' to create replicas and allow D1 automatically place them around the world, or 'disabled' to not use any database replicas (it can take a few hours for all replicas to be deleted).
      mode: d1readreplicationmode }
    ///Creates an instance of d1databaseupdaterequestbodyReadreplication with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (mode: d1readreplicationmode): d1databaseupdaterequestbodyReadreplication = { mode = mode }

type d1databaseupdaterequestbody =
    { ///Configuration for D1 read replication.
      read_replication: d1databaseupdaterequestbodyReadreplication }
    ///Creates an instance of d1databaseupdaterequestbody with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (read_replication: d1databaseupdaterequestbodyReadreplication): d1databaseupdaterequestbody =
        { read_replication = read_replication }

///Various durations for the query.
type Timings =
    { ///The duration of the SQL query execution inside the database. Does not include any network communication.
      sql_duration_ms: Option<float> }
    ///Creates an instance of Timings with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): Timings = { sql_duration_ms = None }

type d1querymeta =
    { ///Denotes if the database has been altered in some way, like deleting rows.
      changed_db: Option<bool>
      ///Rough indication of how many rows were modified by the query, as provided by SQLite's `sqlite3_total_changes()`.
      changes: Option<float>
      ///The duration of the SQL query execution inside the database. Does not include any network communication.
      duration: Option<float>
      ///The row ID of the last inserted row in a table with an `INTEGER PRIMARY KEY` as provided by SQLite. Tables created with `WITHOUT ROWID` do not populate this.
      last_row_id: Option<float>
      ///Number of rows read during the SQL query execution, including indices (not all rows are necessarily returned).
      rows_read: Option<float>
      ///Number of rows written during the SQL query execution, including indices.
      rows_written: Option<float>
      ///Denotes if the query has been handled by the database primary instance.
      served_by_primary: Option<bool>
      ///Region location hint of the database instance that handled the query.
      served_by_region: Option<d1servedbyregion>
      ///Size of the database after the query committed, in bytes.
      size_after: Option<float>
      ///Various durations for the query.
      timings: Option<Timings> }
    ///Creates an instance of d1querymeta with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): d1querymeta =
        { changed_db = None
          changes = None
          duration = None
          last_row_id = None
          rows_read = None
          rows_written = None
          served_by_primary = None
          served_by_region = None
          size_after = None
          timings = None }

type d1queryresultresponse =
    { meta: Option<d1querymeta>
      results: Option<System.Text.Json.JsonElement>
      success: Option<bool> }
    ///Creates an instance of d1queryresultresponse with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): d1queryresultresponse =
        { meta = None
          results = None
          success = None }

type Results =
    { columns: Option<list<string>>
      rows: Option<list<list<string>>> }
    ///Creates an instance of Results with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): Results = { columns = None; rows = None }

type d1rawresultresponse =
    { meta: Option<d1querymeta>
      results: Option<Results>
      success: Option<bool> }
    ///Creates an instance of d1rawresultresponse with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): d1rawresultresponse =
        { meta = None
          results = None
          success = None }

///Configuration for D1 read replication.
type d1readreplicationdetails =
    { ///The read replication mode for the database. Use 'auto' to create replicas and allow D1 automatically place them around the world, or 'disabled' to not use any database replicas (it can take a few hours for all replicas to be deleted).
      mode: d1readreplicationmode }
    ///Creates an instance of d1readreplicationdetails with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (mode: d1readreplicationmode): d1readreplicationdetails = { mode = mode }

[<RequireQualifiedAccess>]
type CloudflareD1ListDatabases =
    ///List D1 databases response
    | OK of payload: d1apiresponsecommon

type CloudflareD1CreateDatabasePayload =
    { ///D1 database name.
      name: d1databasename
      ///Specify the region to create the D1 primary, if available. If this option is omitted, the D1 will be created as close as possible to the current user.
      primary_location_hint: Option<d1primarylocationhint> }
    ///Creates an instance of CloudflareD1CreateDatabasePayload with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (name: d1databasename): CloudflareD1CreateDatabasePayload =
        { name = name
          primary_location_hint = None }

[<RequireQualifiedAccess>]
type CloudflareD1CreateDatabase =
    ///Returns the created D1 database's metadata
    | OK of payload: d1apiresponsecommon

[<RequireQualifiedAccess>]
type CloudflareD1DeleteDatabase =
    ///Delete D1 database response
    | OK of payload: d1apiresponsecommon

[<RequireQualifiedAccess>]
type CloudflareD1GetDatabase =
    ///Database details response
    | OK of payload: d1apiresponsecommon

[<RequireQualifiedAccess>]
type CloudflareD1UpdatePartialDatabase =
    ///Database details response
    | OK of payload: d1apiresponsecommon

[<RequireQualifiedAccess>]
type CloudflareD1UpdateDatabase =
    ///Database details response
    | OK of payload: d1apiresponsecommon

type CloudflareD1QueryDatabasePayload =
    { ``params``: Option<d1params>
      ///Your SQL query. Supports multiple statements, joined by semicolons, which will be executed as a batch.
      sql: d1sql }
    ///Creates an instance of CloudflareD1QueryDatabasePayload with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (sql: d1sql): CloudflareD1QueryDatabasePayload = { ``params`` = None; sql = sql }

[<RequireQualifiedAccess>]
type CloudflareD1QueryDatabase =
    ///Query response
    | OK of payload: d1apiresponsecommon

type CloudflareD1RawDatabaseQueryPayload =
    { ``params``: Option<d1params>
      ///Your SQL query. Supports multiple statements, joined by semicolons, which will be executed as a batch.
      sql: d1sql }
    ///Creates an instance of CloudflareD1RawDatabaseQueryPayload with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (sql: d1sql): CloudflareD1RawDatabaseQueryPayload = { ``params`` = None; sql = sql }

[<RequireQualifiedAccess>]
type CloudflareD1RawDatabaseQuery =
    ///Raw query response
    | OK of payload: d1apiresponsecommon
