# Runtime vs Management APIs in CloudflareFS

## Architecture Overview

CloudflareFS implements a **dual-layer API architecture** that separates runtime operations from management operations, reflecting Cloudflare's own platform design.

```
CloudflareFS/
└── src/
   ├── Runtime/           # In-Worker APIs (JavaScript interop)
   │   ├── CloudFlare.D1/
   │   ├── CloudFlare.R2/
   │   ├── CloudFlare.KV/
   │   └── CloudFlare.Worker.Context/
   │
   └── Management/        # REST APIs (HTTP clients)
       ├── CloudFlare.Management.Workers/
       ├── CloudFlare.Management.D1/
       ├── CloudFlare.Management.R2/
       ├── CloudFlare.Management.Analytics/
       └── CloudFlare.Management.Queues/
```

## Runtime APIs (In-Worker)

**Source**: TypeScript definitions from `@cloudflare/workers-types`
**Generation**: Glutinum (TypeScript → F#)
**Usage**: F# code running **inside** Cloudflare Workers
**Protocol**: Direct JavaScript interop
**Authentication**: Via Worker bindings in `wrangler.toml`

### Example: Runtime D1 Operations
```fsharp
// Inside a Worker - using Runtime API
open CloudFlare.D1

let handleRequest (request: Request) (env: Env) =
    async {
        // env.DATABASE is a D1Database binding from wrangler.toml
        let db = env.DATABASE

        // Direct database operations via JavaScript interop
        let! result =
            db.prepare("SELECT * FROM users WHERE id = ?")
              .bind(userId)
              .first<User>()

        match result with
        | Some user -> return Response.json(user)
        | None -> return Response.error("User not found", 404)
    }
```

### Runtime API Characteristics:
- Zero-latency access to platform services
- No HTTP overhead
- Automatic authentication via bindings
- Limited to operations available in Worker context
- Cannot create/delete resources, only use them

## Management APIs (External)

**Source**: Cloudflare OpenAPI specifications
**Generation**: Hawaii (OpenAPI → F#)
**Usage**: F# applications **outside** Workers (deployment tools, CLIs, management apps)
**Protocol**: HTTPS REST API
**Authentication**: API tokens/keys

### Example: Management D1 Operations
```fsharp
// External application - using Management API
open CloudFlare.Management.D1
open System.Net.Http

let provisionDatabase (accountId: string) (apiToken: string) =
    async {
        let httpClient = new HttpClient()
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}")

        let client = D1ManagementClient(httpClient)

        // Create a new D1 database via REST API
        let! database =
            client.CreateDatabase(
                accountId = accountId,
                name = "production-db",
                primaryLocationHint = Some "wnam"
            )

        printfn $"Created database: {database.uuid}"

        // List all databases
        let! databases = client.ListDatabases(accountId)
        return databases
    }
```

### Management API Characteristics:

- Full CRUD operations on resources
- Account-level administration
- Billing and usage information
- Cross-region configuration
- Requires API authentication

## Use Case Comparison

| Operation | Runtime API | Management API |
|-----------|-------------|----------------|
| Query D1 database | ✅ `db.prepare(...).all()` | ❌ Not available |
| Create D1 database | ❌ Not available | ✅ `client.CreateDatabase(...)` |
| Read KV value | ✅ `env.KV.get(key)` | ✅ `client.GetValue(...)` (slower) |
| Create KV namespace | ❌ Not available | ✅ `client.CreateNamespace(...)` |
| Stream R2 object | ✅ `env.BUCKET.get(key)` | ✅ `client.GetObject(...)` (different) |
| Create R2 bucket | ❌ Not available | ✅ `client.CreateBucket(...)` |
| Execute Worker | ✅ Direct invocation | ✅ Via HTTP trigger |
| Deploy Worker | ❌ Not available | ✅ `client.UpdateWorkerScript(...)` |

## Workflow Options

This process is still under review, but it's worth noting the "head space" that various approaches require as design moves toward implementation.

**Important**: CloudflareFS's goal is to make F# .fsx configuration of solutions a first-class consideration. All infrastructure should be defined in code, not configuration files. While we may export wrangler.toml for compatibility with existing Cloudflare tooling, it is an express goal of this framework to operate code-first.

1. **Infrastructure Setup** (Management API)

   ```fsharp
   // CLI tool or deployment script - pure F# code
   let! database = managementClient.CreateDatabase(accountId, "app-db")
   let! kvNamespace = managementClient.CreateNamespace(accountId, "cache")
   let! r2Bucket = managementClient.CreateBucket(accountId, "assets")
   ```

2. **Configure Bindings** (F# code, NOT TOML)

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

3. **Runtime Operations** (Runtime API)

   ```fsharp
   // Inside Worker
   let handleRequest (env: Env) =
       let! data = env.DATABASE.prepare("SELECT * FROM data").all()
       let! cached = env.CACHE.get("key")
       let! asset = env.ASSETS.get("logo.png")
       // ... process and respond
   ```

## Generation Pipeline

### Runtime APIs (Glutinum)

```bash
# TypeScript → F#
npx @glutinum/cli generate ./node_modules/@cloudflare/workers-types/index.d.ts \
    --output ./src/Runtime/CloudFlare.Worker.Context/Generated.fs \
    --namespace CloudFlare.Worker
```

### Management APIs (Hawaii)

```bash
# OpenAPI → F#
hawaii --config ./generators/hawaii/d1-hawaii.json
# Generates: src/Management/CloudFlare.Management.D1/
```

## Key Benefits

1. **Type Safety**: Both layers are fully typed in F#
2. **No Duplication**: Each API serves distinct purposes
3. **Complete Coverage**: Can manage infrastructure AND run applications
4. **Familiar Patterns**: Management follows REST conventions, Runtime follows platform conventions
5. **Tool Flexibility**: Can build CLIs, deployment tools, and Workers all in F#

## Future: The `cfs` CLI Tool

The CloudflareFS CLI will leverage both API layers:

```fsharp
// cfs deploy command - uses both APIs
let deploy (config: DeployConfig) =
    async {
        // Management API: Ensure infrastructure exists
        let! database = ensureDatabase config.database
        let! kvNamespace = ensureKVNamespace config.kv

        // Management API: Deploy Worker code
        let! worker = deployWorkerScript config.script

        // Runtime API validation could happen here
        // by invoking the Worker and checking health

        return DeploymentResult.Success
    }
```

This dual-layer approach would provide the flexibility to build any Cloudflare tool or application entirely in F#.
