module CloudFlare.Hyperdrive.Helpers

open CloudFlare.Hyperdrive
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
    { new HyperdriveConfig with
        member val id = "" with get, set
        member val name = name with get, set
        member val origin =
            { new HyperdriveOrigin with
                member val host = host with get, set
                member val port = port with get, set
                member val database = database with get, set
                member val user = user with get, set
                member val password = password with get, set
                member val scheme = "postgresql" with get, set } with get, set
        member val caching = None with get, set }

/// Create caching configuration
let createCaching (maxAge: int) (staleWhileRevalidate: int) =
    { new HyperdriveCaching with
        member val disabled = Some false with get, set
        member val maxAge = Some maxAge with get, set
        member val staleWhileRevalidate = Some staleWhileRevalidate with get, set }

/// Disable caching
let disableCaching() =
    { new HyperdriveCaching with
        member val disabled = Some true with get, set
        member val maxAge = None with get, set
        member val staleWhileRevalidate = None with get, set }

/// Connection pool configuration helper
let createPoolSettings (min: int) (max: int) =
    { new ConnectionPoolSettings with
        member val minConnections = Some min with get, set
        member val maxConnections = Some max with get, set
        member val connectionTimeout = Some 30000 with get, set // 30 seconds
        member val idleTimeout = Some 60000 with get, set } // 60 seconds

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