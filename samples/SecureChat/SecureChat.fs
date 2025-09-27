// Secure Chat application using Cloudflare Workers
module SecureChat

open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.Worker.Context.Generated
open CloudFlare.Worker.Context.Helpers
open CloudFlare.Worker.Context.Helpers.ResponseBuilder
open CloudFlare.Worker.Context.D1
open System

// Message types
type ChatMessage = {
    id: string
    userId: string
    content: string
    timestamp: DateTime
    encrypted: bool
}

type ChatRoom = {
    id: string
    name: string
    created: DateTime
    participants: string[]
}

// Security helpers using Web Crypto API
module Security =
    // Timing-safe comparison for passwords
    let timingSafeEqual (a: string) (b: string) : bool =
        emitJsExpr (a, b) """
            const encoder = new TextEncoder();
            const aBytes = encoder.encode($0);
            const bBytes = encoder.encode($1);
            return aBytes.byteLength === bBytes.byteLength &&
                   crypto.subtle.timingSafeEqual(aBytes, bBytes)
        """

    let hashPassword (password: string) : JS.Promise<string> =
        emitJsExpr password """
            crypto.subtle.digest('SHA-256', new TextEncoder().encode($0))
                .then(buf => Array.from(new Uint8Array(buf))
                .map(b => b.toString(16).padStart(2, '0'))
                .join(''))
        """

    let generateSessionToken () : string =
        emitJsExpr () "crypto.randomUUID()"

    let validateToken (token: string) (kv: KVNamespace) = async {
        let! session = KV.get $"session:{token}" kv
        return session.IsSome
    }

// Database operations
module Database =
    let initSchema (db: D1Database) =
        let sql = """
            CREATE TABLE IF NOT EXISTS sessions (
                id TEXT PRIMARY KEY,
                username TEXT NOT NULL,
                created_at INTEGER NOT NULL,
                expires_at INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS messages (
                id TEXT PRIMARY KEY,
                room_id TEXT NOT NULL,
                username TEXT NOT NULL,
                content TEXT NOT NULL,
                timestamp INTEGER NOT NULL,
                encrypted INTEGER DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS rooms (
                id TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                created_at INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS participants (
                room_id TEXT NOT NULL,
                username TEXT NOT NULL,
                joined_at INTEGER NOT NULL,
                PRIMARY KEY (room_id, user_id)
            );
        """
        db.exec(sql) |> promiseToAsync

    let createSession (username: string) (db: D1Database) = async {
        let sessionId = Security.generateSessionToken()
        let expiresAt = DateTime.UtcNow.AddHours(24.0).Ticks
        let stmt =
            db.prepare("INSERT INTO sessions (id, username, created_at, expires_at) VALUES (?, ?, ?, ?)")
              .bind(sessionId, username, DateTime.UtcNow.Ticks, expiresAt)
        let! result = stmt.run() |> promiseToAsync
        return if result.success then Some sessionId else None
    }

    let getSession (sessionId: string) (db: D1Database) = async {
        let stmt = db.prepare("SELECT * FROM sessions WHERE id = ? AND expires_at > ?").bind(sessionId, DateTime.UtcNow.Ticks)
        let! result = stmt.first<obj>() |> promiseToAsync
        return result
    }

    let createRoom (name: string) (creatorUsername: string) (db: D1Database) = async {
        let roomId = Security.generateSessionToken()
        let! _ =
            db.prepare("INSERT INTO rooms (id, name, created_at) VALUES (?, ?, ?)")
              .bind(roomId, name, DateTime.Now.Ticks)
              .run() |> promiseToAsync

        let! _ =
            db.prepare("INSERT INTO participants (room_id, username, joined_at) VALUES (?, ?, ?)")
              .bind(roomId, creatorUsername, DateTime.Now.Ticks)
              .run() |> promiseToAsync

        return roomId
    }

    let saveMessage (roomId: string) (username: string) (content: string) (encrypted: bool) (db: D1Database) = async {
        let messageId = Security.generateSessionToken()
        let stmt =
            db.prepare("INSERT INTO messages (id, room_id, username, content, timestamp, encrypted) VALUES (?, ?, ?, ?, ?, ?)")
              .bind(messageId, roomId, username, content, DateTime.Now.Ticks, if encrypted then 1 else 0)
        let! result = stmt.run() |> promiseToAsync
        return if result.success then Some messageId else None
    }

    let getMessages (roomId: string) (limit: int) (db: D1Database) = async {
        let stmt =
            db.prepare("SELECT * FROM messages WHERE room_id = ? ORDER BY timestamp DESC LIMIT ?")
              .bind(roomId, limit)
        let! result = stmt.all<obj>() |> promiseToAsync
        return result.results |> Option.defaultValue (ResizeArray())
    }

// API handlers
module Api =
    let handleRegister (body: obj) = async {
        return json {|
            error = "Registration disabled"
            message = "Users must be created using the add-user.ps1 script"
        |} 403
    }

    let handleLogin (body: obj) (env: Env) (db: D1Database) (kv: KVNamespace) = async {
        let username = body?username :?> string
        let password = body?password :?> string

        if String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password) then
            return json {| error = "Username and password required" |} 400
        else
            // Check if user has a secret configured
            let userSecretKey = $"USER_{username.ToUpper()}_PASSWORD"
            let storedPasswordHash = env?(userSecretKey) :?> string option

            match storedPasswordHash with
            | None ->
                return json {| error = "Invalid credentials" |} 401
            | Some storedHash ->
                // Hash the provided password and compare
                let! providedHash = Security.hashPassword password |> promiseToAsync
                if Security.timingSafeEqual providedHash storedHash then
                    // Create session in database
                    let! sessionId = Database.createSession username db
                    match sessionId with
                    | Some token ->
                        // Also store in KV for fast lookups
                        do! KV.putWithTtl $"session:{token}" username 86400 kv
                        return json {| token = token; username = username |} 200
                    | None ->
                        return serverError "Failed to create session"
                else
            return json {| error = "Invalid credentials" |} 401
    }

    let handleCreateRoom (body: obj) (username: string) (db: D1Database) = async {
        let roomName = body?name :?> string
        if String.IsNullOrWhiteSpace(roomName) then
            return json {| error = "Room name required" |} 400
        else
            let! roomId = Database.createRoom roomName username db
            return json {| roomId = roomId; name = roomName |} 200
    }

    let handleSendMessage (body: obj) (username: string) (db: D1Database) = async {
        let roomId = body?roomId :?> string
        let content = body?content :?> string
        let encrypted = body?encrypted |> Option.ofObj |> Option.defaultValue false :?> bool

        if String.IsNullOrWhiteSpace(content) then
            return json {| error = "Message content required" |} 400
        else
            let! messageId = Database.saveMessage roomId username content encrypted db
            match messageId with
            | Some id ->
                return json {| messageId = id; success = true |} 200
            | None ->
                return serverError "Failed to send message"
    }

    let handleGetMessages (roomId: string) (db: D1Database) = async {
        let! messages = Database.getMessages roomId 50 db
        return json {| messages = messages |} 200
    }


// Main handler
let fetch (request: Request) (env: Env) (ctx: ExecutionContext) =
    let path = Request.getPath request
    let method = Request.getMethod request

    // Get required bindings
    let db = env?DB :?> D1Database
    let kv = env?KV :?> KVNamespace

    // Initialize database schema
    ctx.waitUntil(Database.initSchema db |> Async.StartAsPromise |> unbox)

    // CORS headers for all responses
    let corsHeaders =
        headers {
            cors
            contentType "application/json"
        }

    let handleAuthorized (handler: string -> Async<Response>) = async {
        let token = Request.getHeader "Authorization" request
        match token with
        | Some t when t.StartsWith("Bearer ") ->
            let tokenValue = t.Substring(7)
            let! username = KV.get $"session:{tokenValue}" kv
            match username with
            | Some user ->
                return! handler user
            | None ->
                return json {| error = "Invalid token" |} 401
        | _ ->
            return json {| error = "Authorization required" |} 401
    }

    // Route handling
    let response =
        match method, path with
        | "OPTIONS", _ ->
            // CORS preflight
            Response.Create(null, jsOptions(fun o ->
                o.status <- Some 204.0
                o.headers <- Some (U2.Case2 corsHeaders)
            ))

        | "POST", "/api/register" ->
            Api.handleRegister {||}
            |> Async.StartAsPromise
            |> unbox

        | "POST", "/api/login" ->
            request.json<obj>()
            |> promiseToAsync
            |> fun body -> async {
                let! b = body
                return! Api.handleLogin b env db kv
            }
            |> Async.StartAsPromise
            |> unbox

        | "POST", "/api/rooms" ->
            handleAuthorized (fun username ->
                request.json<obj>()
                |> promiseToAsync
                |> fun body -> async {
                    let! b = body
                    return! Api.handleCreateRoom b username db
                }
            )
            |> Async.StartAsPromise
            |> unbox

        | "POST", "/api/messages" ->
            handleAuthorized (fun username ->
                request.json<obj>()
                |> promiseToAsync
                |> fun body -> async {
                    let! b = body
                    return! Api.handleSendMessage b username db
                }
            )
            |> Async.StartAsPromise
            |> unbox

        | "GET", path when path.StartsWith("/api/messages/") ->
            let roomId = path.Substring(14)
            handleAuthorized (fun _ ->
                Api.handleGetMessages roomId db
            )
            |> Async.StartAsPromise
            |> unbox

        | "GET", "/" ->
            json {|
                name = "SecureChat API"
                version = "1.0.0"
                endpoints = [|
                    "/api/login"
                    "/api/rooms"
                    "/api/messages"
                    "/api/messages/:roomId"
                |]
            |} 200

        | _ ->
            notFound()

    // Add CORS headers to all responses
    emitJsExpr (response, corsHeaders) """
        if ($0 instanceof Response) {
            const newHeaders = new Headers($0.headers);
            $1.forEach((value, key) => newHeaders.set(key, value));
            return new Response($0.body, {
                status: $0.status,
                statusText: $0.statusText,
                headers: newHeaders
            });
        }
        return $0;
    """

// Export the handler
[<Export("default")>]
let handler = {| fetch = fetch |}