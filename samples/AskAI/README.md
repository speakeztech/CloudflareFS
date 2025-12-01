# Ask AI Sample

A complete example demonstrating CloudflareFS for building a RAG-powered question answering system using Cloudflare's AI Search (AutoRAG).

## Overview

This sample shows both sides of the CloudflareFS dual-layer architecture:

- **Worker/** - Runtime code that executes in Cloudflare Workers, using Glutinum-generated bindings
- **CLI/** - Management code that provisions resources and deploys workers, using Hawaii-generated bindings

## Features

- **RAG with Ranked Sources**: Questions return both synthesized answers and ranked source documents
- **Streaming SSE**: Real-time response streaming with sources delivered first
- **Persona Context**: Responses adapt based on user persona (engineer, business, academic, security)
- **Analytics Logging**: Query patterns stored in D1 for analysis
- **Content Sync**: Automated synchronization of markdown content to R2 for indexing

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         User Question                           │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                     ask-ai Worker (F#/Fable)                    │
├─────────────────────────────────────────────────────────────────┤
│  Bindings: AI (AutoRAG), D1 (logging)                          │
│  Routes: POST /ask, POST /ask-stream, GET /health              │
└─────────────────────────────────────────────────────────────────┘
                              │
              ┌───────────────┴───────────────┐
              ▼                               ▼
┌──────────────────────┐         ┌──────────────────────┐
│   AI Search (AutoRAG) │         │   D1 Database        │
│   - Vector search     │         │   - Query logging    │
│   - LLM synthesis     │         │   - Analytics        │
└──────────────────────┘         └──────────────────────┘
              │
              ▼
┌──────────────────────┐
│   R2 Bucket          │
│   - Markdown content │
│   - Custom metadata  │
└──────────────────────┘
              ▲
              │
┌─────────────────────────────────────────────────────────────────┐
│                     CLI Tool (F#/.NET)                          │
├─────────────────────────────────────────────────────────────────┤
│  Commands: provision, deploy, sync, status                      │
│  - Provisions R2, D1 via Management API                         │
│  - Compiles F# to JS via Fable                                  │
│  - Deploys worker via Management API                            │
│  - Syncs content to R2                                          │
└─────────────────────────────────────────────────────────────────┘
```

## Quick Start

### Prerequisites

- .NET 8.0 SDK
- Node.js 18+
- Cloudflare account with API token

### Environment Setup

```bash
export CLOUDFLARE_API_TOKEN="your-api-token"
export CLOUDFLARE_ACCOUNT_ID="your-account-id"
```

### Provision Resources

```bash
cd CLI
dotnet run -- provision
```

This creates:
- R2 bucket for content storage
- D1 database for query analytics

### Deploy Worker

```bash
dotnet run -- deploy
```

This:
1. Compiles F# to JavaScript via Fable
2. Uploads worker to Cloudflare via Management API
3. Configures bindings for AI, D1, and environment variables

### Sync Content

```bash
dotnet run -- sync --content-dir /path/to/markdown
```

This:
1. Reads markdown files from specified directory
2. Extracts frontmatter metadata
3. Uploads to R2 with custom metadata
4. Triggers AutoRAG index refresh

## Worker API

### POST /ask

Non-streaming question endpoint.

```json
{
  "question": "What is CloudflareFS?",
  "persona": "engineer",
  "interests": ["cloudflarefs"]
}
```

Response:
```json
{
  "answer": "CloudflareFS is a collection of F# bindings...",
  "sources": [
    { "title": "CloudflareFS Introduction", "url": "/blog/cloudflarefs/", "relevance": 0.92 }
  ],
  "phase": "1"
}
```

### POST /ask-stream

Streaming SSE endpoint. Returns events:
- `sources` - Ranked source documents (sent immediately)
- `chunk` - Incremental response text
- `done` - Completion with full text
- `error` - Error details if any

## Key Implementation Details

### Persona Context

The worker prepends context based on user persona to steer LLM responses:

```fsharp
let buildContextPrefix (persona: string option) (interests: string array) =
    let personaText =
        match persona |> Option.defaultValue "engineer" with
        | "business" -> "I am a business leader."
        | "academic" -> "I am an academic."
        | "security" -> "I am a security professional."
        | _ -> "I am an engineer."
    // ...
```

### Source Extraction

Sources are extracted from AutoRAG responses with URL generation from filenames:

```fsharp
// Filename format: "section--slug.md" -> "/section/slug/"
let url =
    if filename.Contains("--") then
        let parts = filename.Split([|"--"|], StringSplitOptions.None)
        $"/{parts.[0]}/{parts.[1].Replace(".md", "")}/"
    else
        $"/blog/{filename.Replace(".md", "")}/"
```

### Two-Phase Streaming

The streaming endpoint uses a two-phase approach:
1. Fast vector search to get sources (no LLM)
2. Streaming LLM response

This lets users see sources immediately while the answer generates.

## Project Structure

```
AskAI/
├── Worker/                 # Cloudflare Worker (Fable/JS)
│   ├── src/
│   │   ├── Types.fs       # Request/response types, env bindings
│   │   ├── Handlers.fs    # Route handlers, SSE streaming
│   │   └── Main.fs        # Entry point, routing
│   ├── AskAI.Worker.fsproj
│   └── package.json
│
├── CLI/                    # Management CLI (.NET)
│   ├── Core/
│   │   ├── Config.fs      # Configuration, state management
│   │   └── ContentSync.fs # R2 sync, AutoRAG triggering
│   ├── Commands/
│   │   ├── Provision.fs   # Create R2, D1 resources
│   │   ├── Deploy.fs      # Fable compile, worker deploy
│   │   ├── Sync.fs        # Content sync command
│   │   └── Status.fs      # Show deployment status
│   ├── Program.fs         # CLI entry point
│   └── AskAI.CLI.fsproj
│
└── README.md
```

## Extending This Sample

### Adding New Personas

Edit `Handlers.fs` to add new persona types:

```fsharp
| "researcher" -> "I am a research scientist."
```

### Custom Metadata

Modify `ContentSync.fs` to extract additional frontmatter fields and attach them as R2 custom metadata.

### Analytics Queries

The D1 schema supports analytics queries:

```sql
-- Most frequent queries
SELECT canonical_query, frequency
FROM query_patterns
ORDER BY frequency DESC
LIMIT 10;

-- Average latency by hour
SELECT strftime('%H', timestamp) as hour, avg(response_latency_ms)
FROM query_log
GROUP BY hour;
```

## Related Documentation

- [CloudflareFS GitHub](https://github.com/speakeztech/CloudflareFS)
- [Cloudflare AI Search (AutoRAG)](https://developers.cloudflare.com/ai-gateway/providers/autorag/)
- [Fable Compiler](https://fable.io/)
- [Hawaii OpenAPI Generator](https://github.com/Zaid-Ajaj/Hawaii)
- [Glutinum TypeScript Bindings](https://github.com/glutinum-org/Glutinum)
