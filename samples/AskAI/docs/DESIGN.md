# Ask AI: Design Document

## Architecture and Implementation Design

---

## Executive Summary

This document describes the architecture for an AI-powered query system that allows visitors to ask questions about a content corpus and receive grounded, contextually-relevant answers with proper source attribution. The system is built on Cloudflare's infrastructure using CloudflareFS bindings, demonstrating both the practical utility of the framework and the potential for sophisticated retrieval-augmented generation.

The design follows a phased approach. **Phase 1** (implemented in this sample) establishes a functional baseline using Cloudflare's managed AI Search (AutoRAG) service, proving rapid deployment capability and providing immediate value. Future phases introduce progressively more sophisticated features: section-aware chunking, knowledge graph enhancement, hybrid response composition, and automated webhook-triggered ingestion.

---

## Vision and Strategic Goals

### Why Build This

A technical blog represents years of writing covering diverse topics. This corpus has internal coherence; posts reference each other, build on shared concepts, and form an interconnected body of knowledge. A visitor arriving with a specific question must currently navigate manually, scanning titles and skimming content to find relevant material.

An intelligent query system transforms passive content into an active knowledge resource. But the implementation matters. A naive RAG system that simply retrieves chunks and asks an LLM to synthesize an answer has several problems: it loses the author's voice by paraphrasing everything, it treats all retrieved chunks as equal regardless of their structural role, and it incurs unnecessary inference costs by regenerating explanations that already exist in well-written form.

### Strategic Objectives

1. **Rapid Deployment**: Cloudflare's AI Search provides a managed pipeline that can be deployed quickly using CloudflareFS bindings.

2. **Architectural Sophistication**: The phased approach demonstrates that F# can support advanced AI application patterns beyond basic integrations.

3. **Cost Optimization**: By using managed services initially and progressively adding custom logic, the system optimizes for both development velocity and operational cost.

4. **Foundation for Extension**: The structural analysis required for advanced phases creates the foundation for learning path recommendations, prerequisite detection, and conceptual navigation.

---

## Phase 1 Architecture (This Sample)

Phase 1 uses discrete components with focused responsibilities:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              User Interface                                 │
│                     (Web page with embedded query UI)                       │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         ask-ai Worker (F#/Fable)                            │
│                        (Query API Endpoint)                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│  Bindings: AI (AutoRAG), D1 (logging)                                       │
│  Routes: POST /ask, POST /ask-stream, GET /health                           │
│  • Receives questions from frontend                                         │
│  • Calls AI Search for RAG-based answers                                    │
│  • Supports streaming SSE responses                                         │
│  • Query logging to D1 for analytics                                        │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                         CLI Tool (F#/.NET)                                  │
│                    (Deployment Orchestration)                               │
├─────────────────────────────────────────────────────────────────────────────┤
│  Commands: provision, deploy, sync, status                                  │
│  • Provisions R2, D1 resources via Management API                           │
│  • Compiles F# to JS via Fable                                              │
│  • Deploys workers via Management API                                       │
│  • Syncs content to R2 with metadata                                        │
└─────────────────────────────────────────────────────────────────────────────┘

┌───────────────────────────────────────┐ ┌───────────────────────────────────┐
│         Logging Layer (D1)            │ │      Content Layer (R2)           │
├───────────────────────────────────────┤ ├───────────────────────────────────┤
│ • Query log                           │ │ • Markdown files                  │
│ • Query patterns                      │ │ • Custom metadata                 │
│ • Analytics                           │ │ • AI Search source                │
└───────────────────────────────────────┘ └───────────────────────────────────┘

Note: AI Search (AutoRAG) provides built-in similarity caching, eliminating the
need for a separate KV cache layer.
```

### Design Rationale

Phase 1 prioritizes time-to-deployment over sophistication. Cloudflare's AI Search service provides a managed RAG pipeline that handles document ingestion, chunking, embedding, storage, and retrieval. By leveraging this service, a functional query feature can be operational quickly.

The limitations of this approach are understood:
- Chunking is not markdown-header-aware (uses token counts, not semantic structure)
- Retrieved chunks lack structural context
- All responses require full LLM generation

These limitations become the motivation for future phases, but they do not prevent Phase 1 from delivering value.

---

## Content Synchronization

Content (markdown files) must be synchronized to an R2 bucket that AI Search monitors for changes. The synchronization process extracts content and uploads it with custom metadata.

### Metadata Flow

Custom metadata attached to R2 objects propagates through AI Search's pipeline:

```fsharp
let metadata = {|
    title = frontMatter.Title
    date = frontMatter.Date.ToString("o")
    url = $"/{section}/{slug}/"
    context = $"Content titled '{frontMatter.Title}' from the {section} section. URL: /{section}/{slug}/"
|}
```

The `context` field is particularly important. AI Search attaches this value to each chunk and includes it in the LLM prompt during generation, enabling proper source attribution.

### R2 Key Convention

Files are stored with a section prefix to enable URL reconstruction:

```
blog--my-post-title.md      -> /blog/my-post-title/
portfolio--product-name.md  -> /portfolio/product-name/
company--about-us.md        -> /company/about-us/
```

---

## Query Processing

### Persona and Interest Context

The worker supports optional persona and interest parameters that adjust how responses are framed:

**Personas:**
- `business` - Emphasize business value, ROI, strategic implications
- `engineer` - Include technical details, code examples (default)
- `academic` - Emphasize theoretical foundations, research connections
- `security` - Highlight security implications, threat models

**Interests:**
- Topic-specific emphasis (e.g., "fidelity", "cloudflarefs")

These parameters prepend context to the query before it reaches AI Search:

```fsharp
let buildContextPrefix (persona: string option) (interests: string array) =
    let personaText =
        match persona |> Option.defaultValue "engineer" with
        | "business" -> "I am a business leader."
        | "academic" -> "I am an academic."
        | "security" -> "I am a security professional."
        | _ -> "I am an engineer."
    // ... combine with interests
```

### Streaming Response Architecture

The streaming endpoint (`/ask-stream`) uses a two-phase approach:

1. **Fast vector search** to get sources (no LLM invocation)
2. **Streaming LLM response** forwarded as SSE events

This lets users see sources immediately while the answer generates:

```
event: sources
data: {"sources": [...]}

event: chunk
data: {"text": "CloudflareFS is"}

event: chunk
data: {"text": " a collection of"}

event: done
data: {"complete": true, "fullText": "..."}
```

### Source Extraction

Sources are extracted from AI Search responses with URL generation from filenames:

```fsharp
// Filename format: "section--slug.md" -> "/section/slug/"
let url =
    if filename.Contains("--") then
        let parts = filename.Split([|"--"|], StringSplitOptions.None)
        $"/{parts.[0]}/{parts.[1].Replace(".md", "")}/"
    else
        $"/blog/{filename.Replace(".md", "")}/"
```

---

## Analytics and Logging

Queries are logged to D1 for analytics:

```sql
CREATE TABLE query_log (
    id TEXT PRIMARY KEY,
    query_text TEXT NOT NULL,
    timestamp TEXT NOT NULL,
    response_latency_ms INTEGER,
    source_urls TEXT,
    source_count INTEGER
);

CREATE TABLE query_patterns (
    pattern_hash TEXT PRIMARY KEY,
    canonical_query TEXT NOT NULL,
    frequency INTEGER DEFAULT 1,
    last_seen TEXT NOT NULL,
    avg_latency_ms REAL
);
```

This enables analysis of:
- Most frequent query patterns
- Average response latencies
- Source utilization patterns

---

## CLI Tool Architecture

The CLI orchestrates all deployment operations using CloudflareFS Management API bindings:

### Commands

| Command | Purpose |
|---------|---------|
| `provision` | Create R2, D1 resources; initialize D1 schema |
| `deploy` | Compile F# to JS via Fable, deploy worker via Management API |
| `sync` | Parse content directory, upload to R2 with metadata, trigger AI Search index |
| `status` | Show current deployment state and resource IDs |

### Deployment State

The CLI maintains deployment state in a local JSON file:

```fsharp
type DeploymentState = {
    R2BucketCreated: bool
    D1DatabaseId: string option
    WorkerDeployed: bool
    WorkerUrl: string option
    LastDeployHash: string option
    LastSyncTimestamp: DateTime option
    LastDeployedCommit: string option
}
```

This enables:
- Idempotent operations (safe to run multiple times)
- Change detection (skip deployment if source unchanged)
- Audit trail for deployments

---

## Phase 1 Deliverables

Upon completion of Phase 1, the system provides:

1. Natural language question answering grounded in content
2. Source attribution with links to relevant documents
3. Streaming responses for responsive UX
4. Persona-aware response tailoring
5. Query analytics for usage patterns
6. Response times appropriate for interactive use (typically 2-5 seconds)

---

## Future Phases (Not Implemented in This Sample)

### Phase 2a: Section-Aware Chunking

Replace token-based chunking with markdown-header-aware chunking. Aggregate chunk relevance scores to document-level scores, enabling better source ranking.

### Phase 2b: Knowledge Graph Enhancement

Build topic analysis and document relationship inference. Enable learning path recommendations and prerequisite detection.

### Phase 2c: Hybrid Response Composition

Embed original content directly rather than regenerating through LLM inference. Reduce token consumption while preserving author voice.

### Phase 2d: Smart Accordion UI

Present answers that blend minimal AI-generated framing with expandable sections of original prose. Deep link to specific document sections.

### Phase 2e: Git-Aware Webhook Ingestion

Automated webhook-triggered ingestion with intelligent cache invalidation. Classify changes (cosmetic vs. substantive) to determine processing scope.

---

## Key Design Decisions

### Why Two-Phase Streaming?

Sending sources before the LLM response provides immediate feedback. Users can start reading source titles while the answer generates, reducing perceived latency.

### Why Persona/Interest Context?

Different audiences have different needs. A business leader asking about a framework cares about ROI; an engineer cares about implementation details. Prepending context steers the LLM without requiring custom prompts.

### Why CLI-Based Deployment?

Using CloudflareFS Management API bindings for deployment (rather than Wrangler) enables:
- Type-safe infrastructure code
- Integration into existing F# build pipelines
- Programmatic deployment for CI/CD

### Why R2 + AI Search Instead of Custom RAG?

AI Search handles chunking, embedding, vector storage, and retrieval as a managed service. For Phase 1, this dramatically reduces implementation complexity while providing a functional baseline.

---

## References

- [Cloudflare AI Search (AutoRAG)](https://developers.cloudflare.com/ai-gateway/providers/autorag/)
- [CloudflareFS GitHub](https://github.com/speakeztech/CloudflareFS)
- [Fable Compiler](https://fable.io/)
- [Hawaii OpenAPI Generator](https://github.com/Zaid-Ajaj/Hawaii)
- [Glutinum TypeScript Bindings](https://github.com/glutinum-org/Glutinum)
