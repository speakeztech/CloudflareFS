namespace AskAI.Worker

open System
open Fable.Core
open Fable.Core.JsInterop
open CloudFlare.Worker.Context
open CloudFlare.D1
open CloudFlare.AI.Generated

/// Types for the Ask AI worker
/// Demonstrates CloudflareFS runtime bindings for AI Search (AutoRAG)
[<AutoOpen>]
module Types =

    /// Incoming question request
    type AskRequest = {
        question: string
        persona: string option      // "business" | "engineer" | "academic" | "security"
        interests: string array     // ["fidelity", "cloudflarefs"]
    }

    /// Source reference from RAG results
    type SourceReference = {
        title: string
        url: string
        relevance: float
    }

    /// Response to the Ask AI query
    type AskResponse = {
        answer: string
        sources: SourceReference array
        phase: string
    }

    /// Query log entry for D1 analytics
    type QueryLogEntry = {
        Id: string
        QueryText: string
        Timestamp: DateTime
        ResponseLatencyMs: int
        SourceUrls: string
        SourceCount: int
    }

    /// Worker environment bindings
    /// These are populated by Cloudflare at runtime based on worker bindings
    [<AllowNullLiteral>]
    [<Interface>]
    type WorkerEnv =
        inherit Env
        /// D1 database for query logging and analytics
        abstract member DB: D1Database with get
        /// AI binding providing access to Workers AI and AutoRAG
        abstract member AI: Ai<obj> with get
        /// Allowed origins for CORS (comma-separated or "*")
        abstract member ALLOWED_ORIGIN: string with get
        /// Name of the AutoRAG instance to query
        abstract member AUTORAG_NAME: string with get

    /// Error response
    type ErrorResponse = {
        error: string
    }

    /// Health check response
    type HealthResponse = {
        status: string
        phase: string
    }
