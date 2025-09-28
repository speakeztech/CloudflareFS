module CloudFlare.Hyperdrive.Helpers

open CloudFlare.Hyperdrive
open Fable.Core
open Fable.Core.JsInterop
open System

/// Create a connection string from Hyperdrive instance
let getConnectionString (hyperdrive: Hyperdrive) =
    hyperdrive.connectionString

/// Create PostgreSQL connection URL
let postgresUrl (hyperdrive: Hyperdrive) =
    $"postgresql://{hyperdrive.user}:{hyperdrive.password}@{hyperdrive.host}:{hyperdrive.port}/{hyperdrive.database}"

/// Create MySQL connection URL
let mysqlUrl (hyperdrive: Hyperdrive) =
    $"mysql://{hyperdrive.user}:{hyperdrive.password}@{hyperdrive.host}:{hyperdrive.port}/{hyperdrive.database}"

/// Connect with retry logic
let connectWithRetry (hyperdrive: Hyperdrive) (maxRetries: int) =
    let rec attemptConnect (retriesLeft: int) =
        async {
            try
                let socket = hyperdrive.connect()
                return Ok socket
            with
            | ex when retriesLeft > 0 ->
                do! Async.Sleep 1000 // Wait 1 second before retry
                return! attemptConnect (retriesLeft - 1)
            | ex ->
                return Error ex.Message
        }
    attemptConnect maxRetries

/// Hyperdrive builder for database operations
type HyperdriveBuilder(hyperdrive: Hyperdrive) =
    member _.Hyperdrive = hyperdrive

    member _.Connect() =
        hyperdrive.connect()

    member _.ConnectAsync() =
        async { return hyperdrive.connect() }

    member _.ConnectionString =
        hyperdrive.connectionString

    member _.GetSocket() =
        try
            Ok (hyperdrive.connect())
        with
        | ex -> Error ex.Message

/// Create a Hyperdrive builder
let hyperdrive (h: Hyperdrive) = HyperdriveBuilder(h)

/// Create Hyperdrive configuration
let createConfig (name: string) (host: string) (port: int) (database: string) (user: string) (password: string) =
    let mutable _id = ""
    let mutable _name = name
    let mutable _caching = None
    let mutable _origin =
        let mutable _host = host
        let mutable _port = port
        let mutable _database = database
        let mutable _user = user
        let mutable _password = password
        let mutable _scheme = "postgresql"
        { new HyperdriveOrigin with
            member _.host with get() = _host and set(v) = _host <- v
            member _.port with get() = _port and set(v) = _port <- v
            member _.database with get() = _database and set(v) = _database <- v
            member _.user with get() = _user and set(v) = _user <- v
            member _.password with get() = _password and set(v) = _password <- v
            member _.scheme with get() = _scheme and set(v) = _scheme <- v }
    { new HyperdriveConfig with
        member _.id with get() = _id and set(v) = _id <- v
        member _.name with get() = _name and set(v) = _name <- v
        member _.origin with get() = _origin and set(v) = _origin <- v
        member _.caching with get() = _caching and set(v) = _caching <- v }

/// Create caching configuration
let createCaching (maxAge: int) (staleWhileRevalidate: int) =
    let mutable _disabled = Some false
    let mutable _maxAge = Some maxAge
    let mutable _staleWhileRevalidate = Some staleWhileRevalidate
    { new HyperdriveCaching with
        member _.disabled with get() = _disabled and set(v) = _disabled <- v
        member _.maxAge with get() = _maxAge and set(v) = _maxAge <- v
        member _.staleWhileRevalidate with get() = _staleWhileRevalidate and set(v) = _staleWhileRevalidate <- v }

/// Disable caching
let disableCaching() =
    let mutable _disabled = Some true
    let mutable _maxAge = None
    let mutable _staleWhileRevalidate = None
    { new HyperdriveCaching with
        member _.disabled with get() = _disabled and set(v) = _disabled <- v
        member _.maxAge with get() = _maxAge and set(v) = _maxAge <- v
        member _.staleWhileRevalidate with get() = _staleWhileRevalidate and set(v) = _staleWhileRevalidate <- v }

/// Connection pool configuration helper
let createPoolSettings (min: int) (max: int) =
    let mutable _minConnections = Some min
    let mutable _maxConnections = Some max
    let mutable _connectionTimeout = Some 30000 // 30 seconds
    let mutable _idleTimeout = Some 60000 // 60 seconds
    { new ConnectionPoolSettings with
        member _.minConnections with get() = _minConnections and set(v) = _minConnections <- v
        member _.maxConnections with get() = _maxConnections and set(v) = _maxConnections <- v
        member _.connectionTimeout with get() = _connectionTimeout and set(v) = _connectionTimeout <- v
        member _.idleTimeout with get() = _idleTimeout and set(v) = _idleTimeout <- v }

/// Database connection wrapper for common libraries
module DatabaseAdapters =
    /// Create connection config for node-postgres (pg)
    let toPgConfig (hyperdrive: Hyperdrive) =
        createObj [
            "host" ==> hyperdrive.host
            "port" ==> hyperdrive.port
            "database" ==> hyperdrive.database
            "user" ==> hyperdrive.user
            "password" ==> hyperdrive.password
            "ssl" ==> false // Hyperdrive handles SSL
        ]

    /// Create connection config for mysql2
    let toMysqlConfig (hyperdrive: Hyperdrive) =
        createObj [
            "host" ==> hyperdrive.host
            "port" ==> hyperdrive.port
            "database" ==> hyperdrive.database
            "user" ==> hyperdrive.user
            "password" ==> hyperdrive.password
        ]

/// Health check for database connection
let healthCheck (hyperdrive: Hyperdrive) =
    async {
        try
            let socket = hyperdrive.connect()
            do! socket.close() |> Async.AwaitPromise
            return Ok "Connection successful"
        with
        | ex -> return Error ex.Message
    }

/// Active pattern for database type detection
let (|PostgreSQL|MySQL|Unknown|) (connectionString: string) =
    if connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://") then
        PostgreSQL
    elif connectionString.StartsWith("mysql://") then
        MySQL
    else
        Unknown