# CloudflareFS Architecture Decisions

## Current State Analysis

### What We Have

1. **CloudFlare.Worker.Context** (Runtime Bindings)
   - ✅ Core Worker types (Request, Response, Headers)
   - ✅ KV Namespace bindings
   - ✅ D1 Database bindings
   - ✅ R2 Storage bindings
   - ✅ ExecutionContext and Environment

2. **CloudFlare.AI**
   - ✅ AI service bindings (populated from Wrangler)

### The Two-Layer Architecture Question

## Layer 1: Runtime Bindings (JavaScript Interop)

These are **in-Worker** APIs that run in the V8 isolate:
- **KV, D1, R2** - Already bound in Worker.Context
- **Durable Objects** - Need runtime bindings
- **Queues** - Need runtime bindings
- **Analytics Engine** - Need runtime bindings

**Binding Method**: Fable.Core interop with TypeScript definitions

## Layer 2: Management APIs (REST/HTTP)

These are **external** APIs called via HTTP from anywhere:
- **Account Management** - REST API
- **DNS Records** - REST API
- **Workers Management** - REST API
- **R2 Bucket Creation** - REST API
- **KV Namespace Creation** - REST API
- **D1 Database Management** - REST API

**Binding Method**: This is where **Hawaii** comes in!

## The Key Insight

**Runtime APIs** (Layer 1) and **Management APIs** (Layer 2) are fundamentally different:

| Aspect | Runtime APIs | Management APIs |
|--------|--------------|-----------------|
| Where they run | Inside Worker (V8) | External HTTP calls |
| Authentication | Bindings in wrangler.toml | API tokens |
| Type definitions | TypeScript (workers-types) | OpenAPI specs |
| Binding tool | Glutinum/Manual F# | Hawaii |
| Usage | During request handling | During deployment/config |

## Recommended Architecture

```
CloudflareFS/
├── src/
│   ├── Runtime/                    # Layer 1 - Worker Runtime
│   │   ├── CloudFlare.Worker.Context/   ✅ Done
│   │   ├── CloudFlare.DurableObjects/   🔄 TODO
│   │   ├── CloudFlare.Queues/          🔄 TODO
│   │   └── CloudFlare.Analytics/       🔄 TODO
│   │
│   └── Management/                 # Layer 2 - REST APIs
│       ├── CloudFlare.Api.Core/        # Hawaii generated
│       ├── CloudFlare.Api.Accounts/
│       ├── CloudFlare.Api.Workers/
│       ├── CloudFlare.Api.Storage/     # R2, KV, D1 management
│       └── CloudFlare.Api.DNS/
```

## Why Hawaii for Management APIs?

1. **OpenAPI Driven**: Cloudflare publishes OpenAPI specs
2. **F# Idiomatic**: Generates functional F# client code
3. **Automatic Updates**: Regenerate when APIs change
4. **Type Safety**: Full request/response typing with discriminated unions
5. **Async Support**: Native F# async workflows

## Implementation Strategy

### Phase 1: Complete Runtime Bindings (Current Focus)
```fsharp
// These run INSIDE the Worker
type Worker = {
    KV: KVNamespace        // ✅ Done
    R2: R2Bucket          // ✅ Done
    D1: D1Database        // ✅ Done
    DO: DurableObject     // 🔄 TODO
    Queue: Queue          // 🔄 TODO
}
```

### Phase 2: Management APIs via Hawaii
```fsharp
// These call Cloudflare's REST API
type Management = {
    CreateKVNamespace: string -> Async<KVNamespace>
    CreateR2Bucket: string -> Async<R2Bucket>
    CreateD1Database: string -> Async<D1Database>
    DeployWorker: WorkerScript -> Async<Deployment>
}
```

## Decision: Use Hawaii for Management

**YES**, we should use Hawaii, but **ONLY** for Management APIs:

### Runtime APIs (Don't use Hawaii)
- KV operations (get/put/delete) - Already done via Fable
- R2 operations (get/put/head) - Already done via Fable
- D1 queries - Already done via Fable
- These are JavaScript interop, not HTTP

### Management APIs (Use Hawaii)
- Creating KV namespaces
- Creating R2 buckets
- Managing Workers
- DNS operations
- Account management
- These are REST APIs with OpenAPI specs

## Next Steps

1. **Finish Runtime Bindings** (Manual/Glutinum)
   - Durable Objects
   - Queues
   - Analytics Engine

2. **Set up Hawaii** for Management
   - Get Cloudflare's OpenAPI spec
   - Configure Hawaii
   - Generate F# client bindings

3. **Create Unified Interface**
   ```fsharp
   module CloudflareFS =
       // Runtime - runs in Worker
       let kv = KV.get "key"

       // Management - runs anywhere
       let! namespace = Management.createKVNamespace "my-namespace"
   ```

## Example Use Cases

### Runtime (in Worker)
```fsharp
let handleRequest (req: Request) (env: Env) =
    // This runs IN the Worker
    let! value = env.KV.get "key"
    let! data = env.DB.prepare("SELECT *").all()
    Response.json {| kv = value; db = data |}
```

### Management (from CLI/Script)
```fsharp
let deployApp () = async {
    // This runs on developer machine
    let! kv = CloudflareAPI.createKVNamespace "prod-cache"
    let! db = CloudflareAPI.createD1Database "prod-db"
    let! worker = CloudflareAPI.deployWorker script
    return {| kv = kv.id; db = db.id; worker = worker.id |}
}
```

## Conclusion

- **Runtime APIs**: Continue with current Fable/interop approach ✅
- **Management APIs**: Implement with FsAutoComplete.Api 🔄
- **Don't mix them**: They serve different purposes
- **Both are needed**: Complete platform coverage requires both

This gives us:
1. Type-safe Worker development (Runtime)
2. Type-safe infrastructure automation (Management)
3. Complete Cloudflare platform coverage
4. Clear architectural separation