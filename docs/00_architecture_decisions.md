# CloudflareFS Architecture Decisions

## Executive Summary

CloudflareFS implements a **dual-layer architecture** that separates Runtime APIs (in-Worker JavaScript interop) from Management APIs (external REST operations), providing complete type-safe F# coverage of the Cloudflare platform.

## Current Implementation Status

### ✅ Completed Runtime Bindings (Layer 1)
- **CloudFlare.Worker.Context**: Core Worker types (Request, Response, Headers)
- **CloudFlare.KV**: Key-Value storage operations
- **CloudFlare.R2**: Object storage operations
- **CloudFlare.D1**: Database query operations
- **CloudFlare.AI**: Workers AI service bindings

### ✅ Completed Management APIs (Layer 2)
- **CloudFlare.Management.Workers**: Worker deployment and configuration (Hawaii-generated with post-processing)
- **CloudFlare.Management.R2**: R2 bucket management (Hawaii-generated)
- **CloudFlare.Management.D1**: D1 database management (Hawaii-generated)
- **CloudFlare.Management.Analytics**: Analytics API (Hawaii-generated)
- **CloudFlare.Management.Queues**: Queue management (Hawaii-generated)
- **CloudFlare.Management.Vectorize**: Vector index management V2 (Hawaii-generated)
- **CloudFlare.Management.Hyperdrive**: Connection config management (Hawaii-generated)
- **CloudFlare.Management.DurableObjects**: Namespace management (Hawaii-generated)

### 🔄 In Progress
- **CloudFlare.Management.KV**: KV namespace management (Hawaii complex schema issues)
- **CloudFlare.Management.Logs**: Logs API (extraction patterns pending)

## The Two-Layer Architecture

### Layer 1: Runtime APIs (JavaScript Interop)

**Purpose**: In-Worker APIs that execute inside the V8 isolate
**Source**: TypeScript definitions from `@cloudflare/workers-types`
**Generation**: Glutinum (TypeScript → F#) or manual bindings
**Location**: `src/Runtime/`

```fsharp
// Runs INSIDE a Worker
type D1Database =
    abstract member prepare: query: string -> D1PreparedStatement
    abstract member batch: statements: ResizeArray<D1PreparedStatement> -> JS.Promise<ResizeArray<D1Result>>
```

### Layer 2: Management APIs (REST/HTTP)

**Purpose**: External APIs for infrastructure provisioning and management
**Source**: Cloudflare OpenAPI specifications
**Generation**: Hawaii (OpenAPI → F#)
**Location**: `src/Management/`

```fsharp
// Runs OUTSIDE Workers (any platform: browser, native, or .NET)
type D1ManagementClient =
    member this.CreateDatabase: accountId: string * name: string ->
        Async<Result<D1Database, ApiError>>
    member this.ListDatabases: accountId: string ->
        Async<Result<D1DatabaseList, ApiError>>
```

## Key Architectural Decisions

### Decision 1: Separate Runtime from Management ✅

**Rationale**: These APIs serve fundamentally different purposes and have different execution contexts.

| Aspect | Runtime APIs | Management APIs |
|--------|--------------|-----------------|
| Execution Context | Inside Worker (V8) | External (any platform) |
| Protocol | JavaScript interop | HTTP/REST |
| Authentication | Worker bindings | API tokens |
| Latency | Microseconds | Network RTT |
| Use Case | Data operations | Infrastructure setup |
| Compilation | Fable only | Fable, Fidelity, or .NET |

### Decision 2: Use Hawaii for OpenAPI Generation ✅

**Rationale**: Hawaii provides idiomatic F# client generation from OpenAPI specs.

**Implementation**:
1. Segment massive Cloudflare OpenAPI spec (15.5MB) into service-specific chunks
2. Generate F# clients using Hawaii
3. Organize in parallel structure to Runtime APIs

### Decision 3: Project Organization by Service ✅

**Rationale**: Each Cloudflare service gets its own project for better modularity.

```
CloudflareFS/
├── src/
│   ├── Runtime/                    # In-Worker APIs
│   │   ├── CloudFlare.Worker.Context/
│   │   ├── CloudFlare.D1/
│   │   ├── CloudFlare.R2/
│   │   ├── CloudFlare.KV/
│   │   └── CloudFlare.AI/
│   │
│   └── Management/                 # REST APIs
│       ├── CloudFlare.Management.D1/
│       ├── CloudFlare.Management.R2/
│       ├── CloudFlare.Management.KV/
│       └── CloudFlare.Management.Analytics/
```

### Decision 4: Pure F# Portability for Management APIs ✅

**Rationale**: Management APIs must be compilable via multiple F# toolchains for maximum portability.

**Principle**: Use only F#-native constructs to enable:
- **Fable**: Compile to JavaScript (browser-based management tools)
- **Fidelity**: Compile to native via MLIR/LLVM (zero runtime dependencies)
- **.NET**: Traditional compilation as fallback option

**Implementation Requirements**:
- Use `async { }` computational expressions, NOT `Task<T>`
- Return `Async<Result<'T, 'Error>>` for all operations
- No System.Net.Http or other .NET-specific types
- Pure functional error handling (Result/Option types)
- Immutable records and discriminated unions only

```fsharp
// ✅ CORRECT: Pure F# that compiles everywhere
type D1ManagementClient =
    member this.CreateDatabase: accountId: string * name: string ->
        Async<Result<D1Database, ApiError>>

// ❌ WRONG: .NET-specific, won't compile with Fable/Fidelity
type D1ManagementClient =
    member this.CreateDatabase: accountId: string * name: string ->
        Task<CloudFlareResult<D1Database>>  // Uses Task and custom .NET types
```

**Contrast with Other Libraries**:
- CloudFlare.Client (C#) uses `Task<T>` - locked to .NET only
- CloudflareFS uses `async { }` - portable to any target

### Decision 5: OpenAPI Segmentation Pipeline ✅

**Problem**: Cloudflare's OpenAPI spec is 15.5MB, causing tool failures.

**Solution**: F# script (`extract-services.fsx`) that:
1. Parses the full OpenAPI spec
2. Extracts service-specific paths and schemas
3. Creates focused specs (45KB - 217KB each)
4. Preserves all dependencies and references

## Implementation Pipeline

### Runtime API Generation (Glutinum)
```bash
# TypeScript definitions → F# bindings
npx @glutinum/cli generate
    ./node_modules/@cloudflare/workers-types/index.d.ts \
    --output ./src/Runtime/CloudFlare.Worker.Context/Generated.fs
```

### Management API Generation (Hawaii)
```bash
# 1. Segment OpenAPI spec
dotnet fsi generators/hawaii/extract-services.fsx

# 2. Generate F# clients
hawaii --config generators/hawaii/d1-hawaii.json
# Output: src/Management/CloudFlare.Management.D1/
```

## Usage Patterns

### Complete Workflow Example

```fsharp
// 1. Infrastructure Setup (Management API)
let provisionInfrastructure (accountId: string) = async {
    let client = D1ManagementClient()
    let! result = client.CreateDatabase(accountId, "app-db", Some "wnam")
    match result with
    | Ok database -> return database.uuid
    | Error err -> return failwith $"Failed: {err}"
}

// 2. Configure Bindings (wrangler.toml)
[[d1_databases]]
binding = "DATABASE"
database_id = "generated-uuid-here"

// 3. Runtime Operations (Runtime API)
[<Export>]
let fetch (request: Request) (env: Env) =
    async {
        let db = env.DATABASE // D1Database from Runtime API
        let! result = db.prepare("SELECT * FROM users").all()
        return Response.json(result)
    }
```

## Future Architectural Considerations

### CloudflareFS CLI Tool (`cfs`)

Will leverage both API layers:
```fsharp
// Deploy command uses both APIs
let deploy (config: DeployConfig) = async {
    // Management API: Create resources
    let! database = ensureDatabase config.database
    let! kvNamespace = ensureKVNamespace config.kv

    // Management API: Deploy Worker
    let! worker = deployWorkerScript config.script

    // Could validate via Runtime API invocation
    return DeploymentResult.Success
}
```

### Firetower Monitoring Tool

Desktop/web monitoring application:
- **Management APIs**: Query metrics, logs, usage
- **Runtime APIs**: Direct Worker invocation for health checks
- **Real-time**: WebSocket connections for live data

## Lessons Learned

1. **Hawaii Limitations**: Some complex OpenAPI structures cause null reference exceptions (KV specs remain problematic)
2. **OpenAPI Size**: Large specs need segmentation for tooling compatibility
3. **Namespace Standardization**: Consistent `CloudFlare.Management.*` naming eliminates confusion and improves discoverability
4. **Post-Processing Pipeline**: Automated discriminated union generation and System.Text.Json migration are essential for production-ready clients
5. **Dual Benefits**: Separation enables both infrastructure-as-code AND runtime operations in F#
6. **Portability Matters**: Avoiding .NET-specific patterns (Task, HttpClient) enables compilation via Fable and Fidelity
7. **F# Native Patterns**: Using async workflows and Result types maintains compatibility across all F# toolchains
8. **Discriminator Support**: Hawaii doesn't natively support OpenAPI discriminator schemas; post-processing successfully bridges this gap

## Next Steps

1. **Fix Hawaii Issues**: Debug null reference exceptions for KV specs
2. **Complete Management APIs**: Logs extraction and generation
3. **Expand Management APIs**: DNS, Zero Trust, additional services
4. **Build CLI Tool**: Implement `cfs` leveraging both API layers
5. **Create Firetower**: Monitoring tool using Management APIs
6. **Tool Contributions**: Submit post-processing patterns back to Hawaii for native discriminator support

## Conclusion

The dual-layer architecture successfully provides:
- **Complete Coverage**: Both runtime operations and infrastructure management
- **Type Safety**: Full F# typing across all Cloudflare services
- **Clear Separation**: No confusion between runtime and management concerns
- **Future Flexibility**: Foundation for CLI tools, monitoring, and automation

This architecture positions CloudflareFS as the comprehensive F# solution for the entire Cloudflare platform.