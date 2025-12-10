# Architecture Principles

## Core Design Decisions

### Decision 1: Separate Runtime from Management
These APIs serve fundamentally different purposes with different execution contexts:

| Aspect | Runtime APIs | Management APIs |
|--------|--------------|-----------------|
| Execution Context | Inside Worker (V8) | External (any platform) |
| Protocol | JavaScript interop | HTTP/REST |
| Authentication | Worker bindings | API tokens |
| Latency | Microseconds | Network RTT |
| Use Case | Data operations | Infrastructure setup |
| Compilation | Fable only | Fable, Fidelity, or .NET |

### Decision 2: Use Different Generation Tools Per Layer
- **Runtime**: Glutinum (TypeScript to F#) for JavaScript interop
- **Management**: Hawaii (OpenAPI to F#) for REST clients

### Decision 3: Project Organization by Service
Each Cloudflare service gets its own F# project for modularity:
- `CloudFlare.{Service}` for Runtime APIs
- `CloudFlare.Management.{Service}` for Management APIs

### Decision 4: Pure F# Portability for Management APIs
Management APIs must be compilable via multiple F# toolchains:

**Implementation Requirements**:
- Use `async { }` computational expressions, NOT `Task<T>`
- Return `Async<Result<'T, 'Error>>` for functional error handling
- Use `FSharp.SystemTextJson` for serialization (not `Newtonsoft.Json`)
- Immutable records and discriminated unions only

```fsharp
// CORRECT: Pure F# that compiles everywhere
type D1ManagementClient =
    member this.CreateDatabase: accountId: string * name: string ->
        Async<Result<D1Database, ApiError>>

// WRONG: .NET-specific, won't compile with Fable/Fidelity
type D1ManagementClient =
    member this.CreateDatabase: accountId: string * name: string ->
        Task<CloudFlareResult<D1Database>>
```

### Decision 5: OpenAPI Segmentation Pipeline
The Cloudflare OpenAPI spec (15.5MB) is too large for tools:
1. F# script (`extract-services.fsx`) segments into service-specific specs
2. Each service gets focused spec (45KB - 250KB)
3. Hawaii generates per-service clients

## Layer Separation Principles

### What Runtime APIs DO:
- Zero-latency access to platform services
- Direct database operations, storage access
- Automatic authentication via bindings

### What Runtime APIs DO NOT:
- Create/delete resources
- Account-level administration
- Cross-region configuration

### What Management APIs DO:
- Full CRUD operations on resources
- Account-level administration
- Billing and usage information
- Infrastructure provisioning

### What Management APIs DO NOT:
- Execute inside Workers
- Provide microsecond latency
- Direct data operations (use for setup only)

## Code-First Philosophy
CloudflareFS's goal is to make F# `.fsx` configuration first-class:

```fsharp
// CloudflareFS code-first approach - bindings defined in F#
let workerBindings = [
    D1Database("DATABASE", databaseId = "abc-123-def")
    KVNamespace("CACHE", namespaceId = "xyz-456-789")
    R2Bucket("ASSETS", bucketName = "assets")
]

// Optional: Export to wrangler.toml for compatibility
let exportToWranglerToml() =
    workerBindings |> WranglerCompat.exportBindings "wrangler.toml"
```

While we may export wrangler.toml for compatibility, the express goal is code-first infrastructure definition.
