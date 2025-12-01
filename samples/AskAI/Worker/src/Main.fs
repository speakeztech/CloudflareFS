namespace AskAI.Worker

open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.Worker.Context

/// Main entry point for the Ask AI worker
/// Routes requests to appropriate handlers
module Main =

    /// URL helper to parse request path
    [<Global>]
    type URL
        [<Emit("new URL($0)")>]
        (url: string) =
        member _.pathname: string = jsNative
        member _.searchParams: obj = jsNative

    /// Main fetch handler - entry point for the Cloudflare Worker
    [<ExportDefault>]
    let handler =
        createObj [
            "fetch" ==> fun (request: Request) (env: WorkerEnv) (ctx: ExecutionContext) ->
                promise {
                    let url = URL(request.url)
                    let path = url.pathname
                    let method = request.method

                    // Get Origin header for CORS handling
                    let origin =
                        let headers: Headers = request?headers
                        let originValue = headers.get("Origin")
                        if isNullOrUndefined originValue then None else Some (string originValue)

                    try
                        match method, path with
                        | "OPTIONS", _ ->
                            // CORS preflight
                            return Handlers.handleOptionsWithOrigin env origin

                        | "POST", "/ask" ->
                            // Main Ask AI endpoint (non-streaming)
                            let! response = Handlers.handleAskRequest request env ctx
                            return Handlers.withCORSForOrigin env origin response

                        | "POST", "/ask-stream" ->
                            // Streaming Ask AI endpoint (SSE)
                            let! response = Handlers.handleAskStreamRequest request env ctx
                            return Handlers.withCORSForOrigin env origin response

                        | "GET", "/health" ->
                            // Health check
                            return Handlers.handleHealth ()

                        | "GET", "/" ->
                            // Root - return API info
                            let info = createObj [
                                "name" ==> "Ask AI Worker"
                                "version" ==> "1.0.0"
                                "description" ==> "RAG-powered question answering with ranked sources"
                                "endpoints" ==> [|
                                    createObj [ "method" ==> "POST"; "path" ==> "/ask"; "description" ==> "Ask a question (JSON response)" ]
                                    createObj [ "method" ==> "POST"; "path" ==> "/ask-stream"; "description" ==> "Ask a question (SSE streaming)" ]
                                    createObj [ "method" ==> "GET"; "path" ==> "/health"; "description" ==> "Health check" ]
                                |]
                            ]
                            return Handlers.jsonResponse info 200

                        | _ ->
                            return Handlers.handleNotFound ()

                    with ex ->
                        let error = createObj [
                            "error" ==> ex.Message
                        ]
                        return Handlers.jsonResponse error 500
                }
        ]
