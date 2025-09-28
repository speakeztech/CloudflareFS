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
- **CloudFlare.Management.R2**: R2 bucket management (Hawaii-generated)
- **CloudFlare.Management.D1**: D1 database management (Hawaii-generated)
- **CloudFlare.Management.Analytics**: Analytics API (Hawaii-generated)

### 🔄 In Progress
- **CloudFlare.Management.KV**: KV namespace management (Hawaii issues)
- **CloudFlare.Management.Workers**: Worker deployment (Hawaii issues)

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
// Runs OUTSIDE Workers (CLI tools, deployment scripts)
type D1ManagementClient(httpClient: HttpClient) =
    member this.CreateDatabase: accountId: string * name: string -> Async<D1Database>
    member this.ListDatabases: accountId: string -> Async<D1DatabaseList>
```

## Key Architectural Decisions

### Decision 1: Separate Runtime from Management ✅

**Rationale**: These APIs serve fundamentally different purposes and have different execution contexts.

| Aspect | Runtime APIs | Management APIs |
|--------|--------------|-----------------|
| Execution Context | Inside Worker (V8) | External (any .NET app) |
| Protocol | JavaScript interop | HTTP/REST |
| Authentication | Worker bindings | API tokens |
| Latency | Microseconds | Network RTT |
| Use Case | Data operations | Infrastructure setup |

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

### Decision 4: OpenAPI Segmentation Pipeline ✅

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
    let client = D1ManagementClient(httpClient)
    let! database = client.CreateDatabase(accountId, "app-db", Some "wnam")
    return database.uuid
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

1. **Hawaii Limitations**: Some complex OpenAPI structures cause null reference exceptions (KV, Workers specs)
2. **OpenAPI Size**: Large specs need segmentation for tooling compatibility
3. **Namespace Consistency**: Generated code needs namespace updates to match project structure
4. **Dual Benefits**: Separation enables both infrastructure-as-code AND runtime operations in F#

## Next Steps

1. **Fix Hawaii Issues**: Debug null reference exceptions for KV/Workers specs
2. **Complete Runtime Bindings**: Durable Objects, Queues, Vectorize
3. **Expand Management APIs**: DNS, Zero Trust, Workers deployment
4. **Build CLI Tool**: Implement `cfs` leveraging both API layers
5. **Create Firetower**: Monitoring tool using Management APIs

## Conclusion

The dual-layer architecture successfully provides:
- **Complete Coverage**: Both runtime operations and infrastructure management
- **Type Safety**: Full F# typing across all Cloudflare services
- **Clear Separation**: No confusion between runtime and management concerns
- **Future Flexibility**: Foundation for CLI tools, monitoring, and automation

This architecture positions CloudflareFS as the comprehensive F# solution for the entire Cloudflare platform.