namespace CloudFlare.Hyperdrive

open Fable.Core
open Fable.Core.JsInterop
open System

/// TCP Socket interface for database connections
[<AllowNullLiteral>]
[<Interface>]
type Socket =
    /// Readable stream
    abstract member readable: obj with get

    /// Writable stream
    abstract member writable: obj with get

    /// Connection closed state
    abstract member closed: JS.Promise<unit> with get

    /// Close the socket
    abstract member close: unit -> JS.Promise<unit>

    /// Start TLS
    abstract member startTls: unit -> Socket

/// Hyperdrive database connection interface
[<AllowNullLiteral>]
[<Interface>]
type Hyperdrive =
    /// Connect directly to Hyperdrive as if it's your database, returning a TCP socket
    abstract member connect: unit -> Socket

    /// Connection string for the Hyperdrive instance
    abstract member connectionString: string with get

    /// Hostname for connecting to Hyperdrive
    abstract member host: string with get

    /// Port number for Hyperdrive connection
    abstract member port: int with get

    /// Username for database authentication
    abstract member user: string with get

    /// Password for database authentication
    abstract member password: string with get

    /// Database name
    abstract member database: string with get

/// Environment with Hyperdrive bindings
[<AllowNullLiteral>]
type IHyperdriveEnvironment =
    /// Access a Hyperdrive binding by name
    [<Emit("$0[$1]")>]
    abstract member getHyperdrive: name: string -> Hyperdrive

/// Hyperdrive configuration
[<AllowNullLiteral>]
type HyperdriveConfig =
    abstract member id: string with get, set
    abstract member name: string with get, set
    abstract member origin: HyperdriveOrigin with get, set
    abstract member caching: HyperdriveCaching option with get, set

/// Database origin configuration
and [<AllowNullLiteral>] HyperdriveOrigin =
    abstract member host: string with get, set
    abstract member port: int with get, set
    abstract member database: string with get, set
    abstract member user: string with get, set
    abstract member password: string with get, set
    abstract member scheme: string with get, set

/// Caching configuration
and [<AllowNullLiteral>] HyperdriveCaching =
    abstract member disabled: bool option with get, set
    abstract member maxAge: int option with get, set
    abstract member staleWhileRevalidate: int option with get, set

/// Database type
type DatabaseType =
    | PostgreSQL
    | MySQL
    member this.ToString() =
        match this with
        | PostgreSQL -> "postgresql"
        | MySQL -> "mysql"

/// Connection pool settings
[<AllowNullLiteral>]
type ConnectionPoolSettings =
    abstract member minConnections: int option with get, set
    abstract member maxConnections: int option with get, set
    abstract member connectionTimeout: int option with get, set
    abstract member idleTimeout: int option with get, set