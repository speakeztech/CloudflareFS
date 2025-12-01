namespace AskAI.Worker

open System
open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.Worker.Context
open CloudFlare.D1
open CloudFlare.AI.Generated

/// Request handlers for the Ask AI worker
/// Demonstrates streaming SSE responses, AutoRAG integration, and D1 logging
module Handlers =

    /// Build context prefix based on user persona and interests
    /// This prepends context to the query to steer the LLM response
    let buildContextPrefix (persona: string option) (interests: string array) : string =
        let personaText =
            match persona |> Option.defaultValue "engineer" with
            | "business" -> "I am a business leader."
            | "academic" -> "I am an academic."
            | "security" -> "I am a security professional."
            | _ -> "I am an engineer."

        let interestText =
            let interestNames =
                interests
                |> Array.choose (function
                    | "fidelity" -> Some "the Fidelity framework"
                    | "cloudflarefs" -> Some "CloudflareFS"
                    | _ -> None)
            if interestNames.Length > 0 then
                " I am principally interested in " + (String.concat " and " interestNames) + "."
            else
                ""

        personaText + interestText

    /// Convert slug to Title Case with special word handling
    let private slugToTitleCase (slug: string) : string =
        slug
            .Replace("-", " ")
            .Split(' ')
            |> Array.map (fun word ->
                match word.ToLowerInvariant() with
                | "fsharp" -> "F#"
                | "csharp" -> "C#"
                | "ai" -> "AI"
                | "ui" -> "UI"
                | "api" -> "API"
                | "llm" -> "LLM"
                | "rag" -> "RAG"
                | "cpu" -> "CPU"
                | "gpu" -> "GPU"
                | w when w.Length > 0 ->
                    string (Char.ToUpperInvariant(w.[0])) + w.Substring(1)
                | w -> w
            )
            |> String.concat " "

    /// Convert filename to Title Case with section as parenthetical
    /// e.g., "blog--unexpected-fusion.md" -> "Unexpected Fusion (blog)"
    let filenameToTitle (filename: string) : string =
        let withoutExt = filename.Replace(".md", "")
        if withoutExt.Contains("--") then
            let parts = withoutExt.Split([|"--"|], StringSplitOptions.None)
            let section = parts.[0]
            let slug = parts.[1]
            let title = slugToTitleCase slug
            $"{title} ({section})"
        else
            slugToTitleCase withoutExt

    /// Log query to D1 for analytics
    let logQuery (db: D1Database) (entry: QueryLogEntry) : JS.Promise<unit> =
        promise {
            let sql = """
                INSERT INTO query_log (
                    id, query_text, timestamp,
                    response_latency_ms, source_urls, source_count
                ) VALUES (?, ?, ?, ?, ?, ?)
            """
            let stmt = db.prepare(sql)
            let bound = stmt.bind(
                entry.Id,
                entry.QueryText,
                entry.Timestamp.ToString("o"),
                entry.ResponseLatencyMs,
                entry.SourceUrls,
                entry.SourceCount
            )
            let! _ = bound.run<obj>()
            return ()
        }

    /// Create JSON response
    let jsonResponse (data: obj) (status: int) : Response =
        Globals.Response.json(data, !!createObj [ "status" ==> status ])

    /// Extract sources from AutoRAG search response data
    let private extractSources (data: ResizeArray<AutoRagSearchResponse.data>) : SourceReference array =
        data.ToArray()
        |> Array.map (fun item ->
            let attrs = item.attributes
            let titleVal: obj = attrs.["title"]
            let urlVal: obj = attrs.["url"]

            let title =
                if isNullOrUndefined titleVal then filenameToTitle item.filename
                else string titleVal

            // Generate clean URL from filename if url metadata missing
            // Filename format: "section--slug.md" (e.g., "blog--my-post.md")
            let url =
                if isNullOrUndefined urlVal then
                    let filename = item.filename.Replace(".md", "")
                    if filename.Contains("--") then
                        let parts = filename.Split([|"--"|], StringSplitOptions.None)
                        let section = parts.[0]
                        let slug = parts.[1]
                        $"/{section}/{slug}/"
                    else
                        $"/blog/{filename}/"
                else string urlVal

            { title = title; url = url; relevance = item.score }
        )
        |> Array.distinctBy (fun s -> s.url)
        |> Array.sortByDescending (fun s -> s.relevance)

    /// Handle the main /ask POST endpoint (non-streaming)
    let handleAskRequest (request: Request) (env: WorkerEnv) (ctx: ExecutionContext) : JS.Promise<Response> =
        promise {
            let startTime = DateTime.UtcNow
            let queryId = Guid.NewGuid().ToString()

            let! body = request.json<AskRequest>()
            let question = body.question.Trim()

            if String.IsNullOrWhiteSpace(question) then
                return jsonResponse { error = "Question is required" } 400
            else

            // Build context prefix from persona and interests
            let interests = if isNullOrUndefined body.interests then [||] else body.interests
            let contextPrefix = buildContextPrefix body.persona interests
            let fullQuery = if String.IsNullOrEmpty(contextPrefix) then question else $"{contextPrefix} {question}"

            // Call AutoRAG
            let autorag = env.AI.autorag(env.AUTORAG_NAME)

            let searchRequest: AutoRagAiSearchRequest = !!createObj [
                "query" ==> fullQuery
                "max_num_results" ==> 5
            ]

            let! autoragResult = autorag.aiSearch(searchRequest)

            // Check if we got a structured response
            let aiResponse: AutoRagAiSearchResponse option =
                let resultObj: obj = autoragResult :> obj
                if not (isNullOrUndefined resultObj?response) then
                    Some (resultObj :?> AutoRagAiSearchResponse)
                else
                    None

            let response =
                match aiResponse with
                | Some resp ->
                    let sources = extractSources resp.data
                    { answer = resp.response; sources = sources; phase = "1" }
                | None ->
                    { answer = "Streaming responses not yet supported"; sources = [||]; phase = "1" }

            let latencyMs = int (DateTime.UtcNow - startTime).TotalMilliseconds

            let logEntry = {
                Id = queryId
                QueryText = question
                Timestamp = startTime
                ResponseLatencyMs = latencyMs
                SourceUrls = response.sources |> Array.map (fun s -> s.url) |> String.concat ","
                SourceCount = response.sources.Length
            }
            ctx.waitUntil(logQuery env.DB logEntry |> unbox)

            return jsonResponse response 200
        }

    /// Check if an origin is allowed based on ALLOWED_ORIGIN env var
    let isOriginAllowed (env: WorkerEnv) (origin: string option) : string option =
        match origin with
        | None -> None
        | Some requestOrigin ->
            if env.ALLOWED_ORIGIN = "*" then
                Some requestOrigin
            else
                let allowedOrigins = env.ALLOWED_ORIGIN.Split(',') |> Array.map (fun s -> s.Trim())
                if allowedOrigins |> Array.contains requestOrigin then
                    Some requestOrigin
                else
                    None

    /// Handle CORS preflight OPTIONS requests
    let handleOptionsWithOrigin (env: WorkerEnv) (origin: string option) : Response =
        let headers = Globals.Headers.Create()
        match isOriginAllowed env origin with
        | Some allowedOrigin ->
            headers.set("Access-Control-Allow-Origin", allowedOrigin)
        | None -> ()
        headers.set("Access-Control-Allow-Methods", "POST, OPTIONS")
        headers.set("Access-Control-Allow-Headers", "Content-Type")
        headers.set("Access-Control-Max-Age", "86400")

        Globals.Response.Create(U2.Case1 "", !!createObj [
            "status" ==> 204
            "headers" ==> headers
        ])

    /// Add CORS headers to a response
    let withCORSForOrigin (env: WorkerEnv) (origin: string option) (response: Response) : Response =
        let headers: Headers = response?headers
        match isOriginAllowed env origin with
        | Some allowedOrigin ->
            headers.set("Access-Control-Allow-Origin", allowedOrigin)
        | None -> ()
        response

    /// Handle streaming /ask-stream POST endpoint
    /// Returns SSE with sources first, then streams AI response chunks
    let handleAskStreamRequest (request: Request) (env: WorkerEnv) (ctx: ExecutionContext) : JS.Promise<Response> =
        promise {
            let startTime = DateTime.UtcNow
            let queryId = Guid.NewGuid().ToString()

            let! body = request.json<AskRequest>()
            let question = body.question.Trim()

            if String.IsNullOrWhiteSpace(question) then
                return jsonResponse { error = "Question is required" } 400
            else

            // Build context prefix from persona and interests
            let interests = if isNullOrUndefined body.interests then [||] else body.interests
            let contextPrefix = buildContextPrefix body.persona interests
            let fullQuery = if String.IsNullOrEmpty(contextPrefix) then question else $"{contextPrefix} {question}"

            let autorag = env.AI.autorag(env.AUTORAG_NAME)

            // Step 1: Get sources via search (no LLM, fast)
            let searchRequest: AutoRagSearchRequest = !!createObj [
                "query" ==> fullQuery
                "max_num_results" ==> 5
            ]
            let! searchResult = autorag.search(searchRequest)
            let sources = extractSources searchResult.data

            // Step 2: Get streaming AI response
            let streamRequest: AutoRagAiSearchRequestStreaming = !!createObj [
                "query" ==> fullQuery
                "max_num_results" ==> 5
                "stream" ==> true
            ]
            let! streamResponse = autorag.aiSearch(streamRequest)

            // Create a TransformStream to build our SSE response
            let transformStream: obj = emitJsExpr () "new TransformStream()"
            let readable: obj = transformStream?readable
            let writable: obj = transformStream?writable
            let writer: obj = writable?getWriter()
            let encoder: obj = emitJsExpr () "new TextEncoder()"

            // Helper to write SSE event
            let writeEvent (eventType: string) (data: obj) : JS.Promise<unit> =
                let json = JS.JSON.stringify(data)
                let sseData = $"event: {eventType}\ndata: {json}\n\n"
                let encoded: obj = encoder?encode(sseData)
                writer?write(encoded) |> unbox

            // Parse AutoRAG SSE chunk to extract response text
            let parseAutoRagChunk (chunk: string) : string option =
                let lines = chunk.Split('\n')
                lines
                |> Array.choose (fun line ->
                    if line.StartsWith("data: ") then
                        try
                            let json = line.Substring(6)
                            let parsed: obj = JS.JSON.parse(json)
                            let response: obj = parsed?response
                            if not (isNullOrUndefined response) then
                                Some (string response)
                            else None
                        with _ -> None
                    else None)
                |> Array.tryHead

            // Start async processing in background
            let processStream () : JS.Promise<unit> =
                promise {
                    try
                        // Send sources event first
                        do! writeEvent "sources" {| sources = sources |}

                        // Read and forward the AI stream
                        let body: obj = streamResponse?body
                        if not (isNullOrUndefined body) then
                            let reader: obj = body?getReader()
                            let decoder: obj = emitJsExpr () "new TextDecoder()"
                            let mutable isDone = false
                            let mutable fullText = ""
                            let mutable buffer = ""

                            while not isDone do
                                let! result = reader?read() |> unbox<JS.Promise<obj>>
                                let done': bool = result?``done`` |> unbox
                                let value: obj = result?value

                                if done' then
                                    isDone <- true
                                else if not (isNullOrUndefined value) then
                                    let chunk: string = decoder?decode(value) |> unbox
                                    buffer <- buffer + chunk

                                    // Process complete SSE messages
                                    let parts = buffer.Split([|"\n\n"|], StringSplitOptions.None)
                                    if parts.Length > 1 then
                                        for i in 0 .. parts.Length - 2 do
                                            match parseAutoRagChunk (parts.[i] + "\n\n") with
                                            | Some text ->
                                                fullText <- fullText + text
                                                do! writeEvent "chunk" {| text = text |}
                                            | None -> ()
                                        buffer <- parts.[parts.Length - 1]

                            // Send done event
                            do! writeEvent "done" {| complete = true; fullText = fullText |}

                            // Log query analytics
                            let latencyMs = int (DateTime.UtcNow - startTime).TotalMilliseconds
                            let logEntry = {
                                Id = queryId
                                QueryText = question
                                Timestamp = startTime
                                ResponseLatencyMs = latencyMs
                                SourceUrls = sources |> Array.map (fun s -> s.url) |> String.concat ","
                                SourceCount = sources.Length
                            }
                            ctx.waitUntil(logQuery env.DB logEntry |> unbox)

                            do! writer?close() |> unbox<JS.Promise<unit>>
                        else
                            do! writeEvent "error" {| message = "No response body" |}
                            do! writer?close() |> unbox<JS.Promise<unit>>
                    with ex ->
                        do! writeEvent "error" {| message = ex.Message |}
                        do! writer?close() |> unbox<JS.Promise<unit>>
                }

            // Start processing (don't await - let it run in background)
            processStream () |> ignore

            // Return SSE response immediately with the readable stream
            let headers = Globals.Headers.Create()
            headers.set("Content-Type", "text/event-stream")
            headers.set("Cache-Control", "no-cache")
            headers.set("Connection", "keep-alive")

            return Globals.Response.Create(!!readable, !!createObj [
                "status" ==> 200
                "headers" ==> headers
            ])
        }

    /// Handle health check endpoint
    let handleHealth () : Response =
        jsonResponse { status = "ok"; phase = "1" } 200

    /// Handle 404 Not Found
    let handleNotFound () : Response =
        Globals.Response.Create(U2.Case1 "Not Found", !!createObj [ "status" ==> 404 ])
