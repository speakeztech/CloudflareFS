# CloudflareFS Binding Strategy

## Two-Layer Architecture

### Layer 1: Runtime Bindings (In-Worker APIs)

**Tool**: Glutinum (TypeScript → F#) + Manual bindings
**Location**: `src/Runtime/`

These APIs execute inside the Cloudflare Worker V8 isolate:

| API | Status | Binding Method |
|-----|--------|---------------|
| Request/Response | ✅ Complete | Manual F# |
| Headers | ✅ Complete | Manual F# |
| KV Namespace | ✅ Complete | Manual F# |
| D1 Database | ✅ Complete | Manual F# |
| R2 Storage | ✅ Complete | Manual F# |
| Durable Objects | 🔄 Planned | Glutinum/Manual |
| Queues | 🔄 Planned | Glutinum/Manual |
| Analytics Engine | 🔄 Planned | Glutinum/Manual |

**Key Points**:
- JavaScript interop via Fable
- No HTTP calls - direct V8 bindings
- Configured via wrangler.toml bindings
- Used during request handling

### Layer 2: Management Bindings (REST APIs)

**Tool**: Hawaii (OpenAPI → F#)
**Location**: `src/Management/`

These APIs are called over HTTP to manage Cloudflare resources:

| API | Status | Operations |
|-----|--------|-----------|
| KV Management | 🔄 Planned | Create/delete namespaces |
| R2 Management | 🔄 Planned | Create/delete buckets |
| D1 Management | 🔄 Planned | Create/delete databases |
| Workers Management | 🔄 Planned | Deploy/update scripts |
| DNS Management | 🔄 Planned | CRUD operations |
| Account Management | 🔄 Planned | Settings, billing |

**Key Points**:
- HTTP REST calls
- API token authentication
- Can be called from anywhere
- Used for deployment/configuration

## Why This Split?

| Aspect | Runtime (Layer 1) | Management (Layer 2) |
|--------|------------------|---------------------|
| **Execution** | Inside Worker | External HTTP |
| **Authentication** | Wrangler bindings | API tokens |
| **Use case** | Handle requests | Deploy/configure |
| **Type source** | TypeScript defs | OpenAPI spec |
| **Tool** | Glutinum/Manual | Hawaii |
| **F# output** | Fable interop | Async workflows |

## Implementation Phases

### Phase 1: Runtime Bindings ✅ (Mostly Complete)
- Core Worker types
- Storage bindings (KV, D1, R2)
- Next: Durable Objects, Queues

### Phase 2: Management APIs 🔄 (Starting)
- Set up Hawaii generator
- Generate from Cloudflare OpenAPI
- Create unified client interface

### Phase 3: Integration
- Unified CloudflareFS module
- Combined runtime + management
- Developer tooling (CLI)

## Code Examples

### Runtime API (runs in Worker)
```fsharp
// This code executes INSIDE the Worker
let handleRequest (req: Request) (env: Env) =
    task {
        // Direct binding to KV
        let! value = env.KV.get "key"

        // Direct binding to D1
        let! users = env.DB.prepare("SELECT * FROM users").all()

        return Response.json {| kv = value; users = users |}
    }
```

### Management API (runs anywhere)
```fsharp
// This code runs on developer machine or CI/CD
let deployInfrastructure accountId = async {
    let client = CloudflareClient(apiToken = config.ApiToken)

    // Create KV namespace via REST API
    let! kvResult = client.KV.CreateNamespace(accountId, "prod-cache")

    // Create D1 database via REST API
    let! dbResult = client.D1.CreateDatabase(accountId, "prod-db")

    // Deploy Worker script
    let! workerResult = client.Workers.Deploy(accountId, script)

    return {| kv = kvResult; db = dbResult; worker = workerResult |}
}
```

## Tooling Summary

- **Glutinum**: TypeScript → F# for Worker runtime APIs
- **Hawaii**: OpenAPI → F# for management REST APIs
- **Fable**: F# → JavaScript compilation for Workers
- **Wrangler**: Cloudflare deployment and local dev

This architecture provides:
1. Type-safe Worker development
2. Type-safe infrastructure automation
3. Complete platform coverage
4. Clear separation of concerns