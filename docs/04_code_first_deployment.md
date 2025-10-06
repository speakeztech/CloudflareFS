# Code-First Deployment Strategy for CloudflareFS

## Executive Summary

CloudflareFS will implement code-first Infrastructure as Code (IaC) where the primary configuration method is expressed as F# code, as opposed to managing a fragmented array of TOML files. The framework makes code-driven migrations and deployments a primary mechanism. While we may provide wrangler.toml export for backward compatibility with existing Cloudflare tooling, static configuration diverges from our vision of application/solution-level code-driven infrastructure.

After analyzing the wrangler source code (from `cloudflare/workers-sdk`), deploying directly via REST APIs using F# configuration scripts is not only feasible but architecturally consistent. CloudflareFS already has the foundation for this with the Management API layer, and the wrangler source reveals the exact metadata structure needed.

## Core Approach

F# .fsx configuration of solutions is our choice for deployment and configuration management. All migrations and deployments should be code-driven, and other code-driven approaches may be considered in the future.

## Deployment Modes

CloudflareFS supports multiple deployment modes to fit different workflows - from direct API deployment to offline TOML generation for traditional CI/CD pipelines.

### 1. Direct API Deployment (Default)

```fsharp
// deploy.fsx - Direct deployment via Cloudflare APIs
#r "nuget: CloudflareFS"

open CloudflareFS.Api
open CloudflareFS.Deployment

let deploy() = cloudflare {
    account "abc123"

    worker "api-service" {
        // Resources are created automatically
        kv "CACHE" (ensureNamespace "api-cache")
        r2 "STORAGE" (ensureBucket "api-storage")
        d1 "DB" (ensureDatabase "api-db")

        route "api.example.com/*"
        script "./dist/worker.js"
    }
}

// Execute: cfs deploy ./deploy.fsx
// Result: Direct deployment via API calls
```

### 2. Offline TOML Generation

```fsharp
// deploy.fsx - Same script, different output mode
let deploy() = cloudflare {
    account "abc123"

    worker "api-service" {
        // When offline, assumes resources exist
        kv "CACHE" (useExisting "kv-namespace-id-123")
        r2 "STORAGE" (useExisting "my-bucket")
        d1 "DB" (useExisting "d1-database-id-456")

        route "api.example.com/*"
        script "./dist/worker.js"
    }
}

// Execute: cfs deploy ./deploy.fsx --offline
// Result: Generates wrangler.toml
```

Generated `wrangler.toml`:
```toml
name = "api-service"
main = "./dist/worker.js"
compatibility_date = "2023-10-01"

[[kv_namespaces]]
binding = "CACHE"
id = "kv-namespace-id-123"

[[r2_buckets]]
binding = "STORAGE"
bucket_name = "my-bucket"

[[d1_databases]]
binding = "DB"
database_id = "d1-database-id-456"

[[routes]]
pattern = "api.example.com/*"
```

### 3. Hybrid Mode (Provision + TOML)

```fsharp
// deploy.fsx - Provision resources, then generate TOML
let deploy() = deployment {
    mode Hybrid  // Provision resources via API, generate TOML for deployment

    account "abc123"

    // Phase 1: Provision resources (via API)
    provision {
        kv "api-cache"
        kv "api-sessions"
        r2 "api-storage"
        d1 "api-database" "./migrations"
    }

    // Phase 2: Generate TOML with provisioned IDs
    worker "api-service" {
        kv "CACHE" (fromProvisioned "api-cache")
        kv "SESSIONS" (fromProvisioned "api-sessions")
        r2 "STORAGE" (fromProvisioned "api-storage")
        d1 "DB" (fromProvisioned "api-database")

        route "api.example.com/*"
    }
}

// Execute: cfs deploy ./deploy.fsx --hybrid
// Result:
//   1. Creates resources via API
//   2. Generates wrangler.toml with real IDs
//   3. User runs: wrangler deploy
```

## Technical Implementation

### Direct API Deployment Approach

The Core Upload API:

```http
PUT /accounts/{account_id}/workers/scripts/{script_name}/content
Content-Type: multipart/form-data

--boundary
Content-Disposition: form-data; name="metadata"
Content-Type: application/json

{
  "main_module": "worker.js",
  "compatibility_date": "2024-01-01",
  "bindings": [
    {
      "type": "kv_namespace",
      "name": "CACHE",
      "namespace_id": "abc123"
    },
    {
      "type": "r2_bucket",
      "name": "STORAGE",
      "bucket_name": "my-bucket"
    },
    {
      "type": "d1_database",
      "name": "DB",
      "id": "database-uuid"
    }
  ],
  "routes": [
    {
      "pattern": "example.com/*",
      "zone_id": "zone-123"
    }
  ]
}

--boundary
Content-Disposition: form-data; name="worker.js"; filename="worker.js"
Content-Type: application/javascript

// Your compiled worker code here
export default {
  async fetch(request, env, ctx) {
    // Worker implementation
  }
}
--boundary--
```

### F# Implementation Strategy

```fsharp
// CloudflareFS Direct Deployment
module CloudflareFS.Deployment.Direct

open System.Net.Http
open System.Text

// Complete binding types discovered from wrangler source
type WorkerBinding =
    | KVNamespace of name: string * namespaceId: string
    | R2Bucket of name: string * bucketName: string * jurisdiction: string option
    | D1Database of name: string * id: string
    | Queue of name: string * queueName: string * deliveryDelay: int option
    | DurableObject of name: string * className: string * scriptName: string option
    | Vectorize of name: string * indexName: string
    | Hyperdrive of name: string * id: string
    | AI of name: string * staging: bool option
    | Browser of name: string
    | AnalyticsEngine of name: string * dataset: string option
    | Service of name: string * service: string * environment: string option
    | MTLSCertificate of name: string * certificateId: string
    | Assets of name: string  // Unified static asset system

type WorkerMetadata = {
    MainModule: string
    CompatibilityDate: string
    Bindings: WorkerBinding list
    Routes: Route list
    UsageModel: string option
}

type DirectDeployer(httpClient: HttpClient, accountId: string) =

    member this.DeployWorker(scriptName: string, code: byte[], metadata: WorkerMetadata) = async {
        // 1. Create multipart form content
        use content = new MultipartFormDataContent()

        // 2. Add metadata JSON
        let metadataJson = JsonSerializer.Serialize(metadata)
        content.Add(new StringContent(metadataJson, Encoding.UTF8, "application/json"), "metadata")

        // 3. Add worker code
        content.Add(new ByteArrayContent(code), "worker.js", "worker.js")

        // 4. Upload directly via API
        let url = $"https://api.cloudflare.com/client/v4/accounts/{accountId}/workers/scripts/{scriptName}/content"
        let! response = httpClient.PutAsync(url, content) |> Async.AwaitTask

        return response.IsSuccessStatusCode
    }
```

## Configuration Strategies

### Environment-Aware Deployment

```fsharp
// deploy.fsx
open CloudflareFS

let getDeploymentMode() =
    match Environment.GetEnvironmentVariable("CF_DEPLOY_MODE") with
    | "offline" -> Offline
    | "hybrid" -> Hybrid
    | "dry-run" -> DryRun
    | _ -> Direct

let config env = cloudflare {
    account (getAccountId env)

    worker $"api-service-{env}" {
        match env with
        | "production" ->
            // Production uses existing resources
            kv "CACHE" (useExisting "prod-cache-id")
            r2 "STORAGE" (useExisting "prod-storage")
            d1 "DB" (useExisting "prod-db-id")

        | "staging" ->
            // Staging provisions new resources
            kv "CACHE" (ensureNamespace "staging-cache")
            r2 "STORAGE" (ensureBucket "staging-storage")
            d1 "DB" (ensureDatabase "staging-db")

        | "development" ->
            // Dev uses local resources
            kv "CACHE" (local "./kv-data")
            r2 "STORAGE" (local "./r2-data")
            d1 "DB" (local "./db.sqlite")
    }
}

// Deploy based on mode
match getDeploymentMode() with
| Direct ->
    config "production"
    |> deployDirect client

| Offline ->
    config "production"
    |> generateToml "wrangler.toml"

| Hybrid ->
    config "production"
    |> provisionResources client
    |> generateToml "wrangler.toml"
```

### Resource Resolution

```fsharp
type ResourceRef =
    | Ensure of name: string                    // Create if doesn't exist
    | UseExisting of id: string                 // Use specific ID
    | FromProvisioned of name: string           // Use ID from provision phase
    | FromConfig of key: string                 // Read from config file
    | FromEnvironment of var: string            // Read from env var

let resolveResource (client: CloudflareClient option) accountId ref =
    match ref, client with
    | Ensure name, Some client ->
        // Online mode: ensure exists via API
        async {
            let! existing = client.KV.ListNamespaces(accountId)
            match existing |> Array.tryFind (fun ns -> ns.title = name) with
            | Some ns -> return ns.id
            | None ->
                let! created = client.KV.CreateNamespace(accountId, name)
                return created.id
        }

    | UseExisting id, _ ->
        // Offline mode: use provided ID
        async { return id }

    | FromEnvironment var, _ ->
        // Read from environment
        async { return Environment.GetEnvironmentVariable(var) }

    | FromConfig key, _ ->
        // Read from config file
        async {
            let config = loadConfig "config.json"
            return config.[key]
        }

    | _, None ->
        failwith "Client required for resource provisioning"
```

## Wrangler Analysis & Direct Deployment

### What Wrangler Actually Does (From Source Analysis)

After examining the wrangler source code, wrangler functions as a convenience wrapper that:

1. **Reads wrangler.toml** and converts it to API calls
2. **Bundles JavaScript/WASM** using esbuild internally
3. **Uploads via multipart form** with JSON metadata containing all bindings
4. **Manages authentication** via Bearer tokens
5. **Provides local dev server** (miniflare integration)
6. **Handles complex orchestration**:
   - Automatic retries on API failures
   - Bundle size validation
   - Source map management
   - Module dependency analysis
   - Resource provisioning (auto-creates if missing)
   - Durable Object migrations
   - Asset manifest generation and differential uploads

### Discoveries from Wrangler Source Code

#### Complete Binding Types Available

The wrangler source reveals all binding types that can be specified in metadata:
- Standard storage: `kv_namespace`, `r2_bucket`, `d1`
- Advanced services: `queue`, `vectorize`, `hyperdrive`
- AI/ML: `ai`, `browser` (for Browser Rendering API)
- Networking: `service` (service bindings), `mtls_certificate`
- Analytics: `analytics_engine`
- Assets: `assets` - Unified static asset system replacing Workers Sites

#### Additional Features Not in Public OpenAPI

1. **Versioning API**: `/content/v2?version={versionId}` for deployment versions
2. **Keep Bindings**: Selective preservation of existing bindings during updates
3. **Tail Consumers**: Built-in log streaming configuration
4. **Placement Hints**: Internal optimization directives
5. **Usage Models**: `bundled` vs `unbound` pricing models
6. **Compatibility Flags**: Beyond just dates, specific feature flags

### Assets Binding Implementation

#### Alternative to Workers Sites

The `assets` binding discovered in wrangler source provides a simpler approach:

```fsharp
// Workers Sites approach (KV-based)
type WorkersSitesConfig = {
    bucket: string
    include: string list
    exclude: string list
    // Requires separate @cloudflare/kv-asset-handler package
}

// Assets binding approach (platform-native)
type AssetsBinding = {
    type: "assets"
    name: string  // e.g., "ASSETS"
    // Assets handled natively by platform
}
```

#### How It Works

1. **In Metadata** (no TOML needed):
```json
{
  "bindings": [
    {"type": "assets", "name": "ASSETS"}
  ],
  "assets": {
    "config": {
      "html_handling": "auto-trailing-slash",
      "not_found_handling": "404-page"
    },
    "jwt": "<from-asset-upload-session>"
  }
}
```

2. **In Worker** (F# via Fable):
```fsharp
type Env = {
    ASSETS: Fetcher  // Regular Fetcher
    DATABASE: D1Database
    CACHE: KVNamespace
}

let fetch (req: Request) (env: Env) = async {
    // Assets are just another binding
    match req.url.pathname with
    | StartsWith "/api" -> handleApi req env
    | _ -> env.ASSETS.fetch(req)  // Serve static assets
}
```

#### Key Advantages

1. **Unified Deployment**: Worker + Assets in single API call
2. **No KV Storage**: Direct CDN integration (faster, cheaper)
3. **Native Platform Feature**: No external packages needed
4. **Full Programmatic Control**: Assets are Fetch API responses
5. **Advanced Routing**: `run_worker_first` option for full control

## Challenges & Solutions

### Challenge 1: Resource ID Management

**Problem**: Need to know KV namespace IDs, R2 bucket names, D1 database IDs
**Solution**: CloudflareFS Management APIs already handle this:

```fsharp
// Provision resources and get IDs
let! kvNamespace = kvClient.CreateNamespace(accountId, "my-cache")
let! r2Bucket = r2Client.CreateBucket(accountId, "my-storage")
let! d1Database = d1Client.CreateDatabase(accountId, "my-db")

// Use IDs in deployment
let bindings = [
    KVNamespace("CACHE", kvNamespace.id)
    R2Bucket("STORAGE", r2Bucket.name)
    D1Database("DB", d1Database.uuid)
]
```

### Challenge 2: JavaScript Bundling

**Problem**: Need to bundle TypeScript/JavaScript before upload
**Solution**: Multiple options:

1. Use Fable output directly (already bundled)
2. Shell out to esbuild when needed
3. Use .NET JavaScript bundling libraries

### Challenge 3: Local Development

**Problem**: Wrangler provides local dev server
**Solution**:
- Keep using `wrangler dev` for local development
- OR implement Miniflare bindings in F#
- OR use Cloudflare's preview deployments

## CLI Commands

### Basic Commands
```bash
# Direct deployment (default)
cfs deploy ./deploy.fsx

# Generate TOML only
cfs deploy ./deploy.fsx --offline

# Provision resources and generate TOML
cfs deploy ./deploy.fsx --hybrid

# Dry run - show what would happen
cfs deploy ./deploy.fsx --dry-run

# Generate TOML with specific output
cfs deploy ./deploy.fsx --offline --output ./ci/wrangler.toml
```

### Advanced Options
```bash
# Use specific account
cfs deploy ./deploy.fsx --account abc123

# Override environment
cfs deploy ./deploy.fsx --env production

# Validate without deploying
cfs validate ./deploy.fsx

# Show resource diff
cfs diff ./deploy.fsx --with-remote

# Generate multiple TOMLs for different environments
cfs deploy ./deploy.fsx --offline --multi-env
# Outputs: wrangler.development.toml, wrangler.staging.toml, wrangler.production.toml
```

## Integration with CI/CD

### GitHub Actions Example
```yaml
name: Deploy to Cloudflare

on:
  push:
    branches: [main]

jobs:
  deploy-hybrid:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0'

      - name: Install CloudflareFS CLI
        run: dotnet tool install -g CloudflareFS.CLI

      - name: Provision Resources and Generate TOML
        env:
          CF_API_TOKEN: ${{ secrets.CF_API_TOKEN }}
          CF_ACCOUNT_ID: ${{ secrets.CF_ACCOUNT_ID }}
        run: |
          cfs deploy ./deploy.fsx --hybrid

      - name: Deploy with Wrangler
        uses: cloudflare/wrangler-action@v3
        with:
          apiToken: ${{ secrets.CF_API_TOKEN }}
          wranglerVersion: '3.0.0'
```

### GitLab CI Example
```yaml
deploy:
  stage: deploy
  script:
    # Generate TOML for offline deployment
    - cfs deploy ./deploy.fsx --offline

    # Use generated TOML with wrangler
    - wrangler deploy --config wrangler.toml

  only:
    - main
```

## Benefits of Each Mode

### Direct API Deployment
✅ No TOML files to maintain
✅ Dynamic resource provisioning
✅ Full programmatic control
✅ Immediate feedback
❌ Requires API access from deployment environment

### Offline TOML Generation
✅ Works with existing CI/CD pipelines
✅ No API access needed during deployment
✅ Version control friendly
✅ Compatible with Wrangler workflows
❌ Manual resource ID management

### Hybrid Mode

✅ Best of both worlds
✅ Automated resource provisioning
✅ TOML for deployment compatibility
✅ Reduces configuration drift
❌ Two-phase deployment process

## Migration Path

### Step 1: Start with TOML Generation

```fsharp
// Easiest migration - generate TOML from F#
let config = parseExistingWrangler "wrangler.toml"
            |> toCloudflareFS
            |> enhance  // Add type safety

generateToml config "wrangler-new.toml"
```

### Step 2: Move to Hybrid

```fsharp
// Provision new resources via API, keep TOML deployment
let config = cloudflare {
    importExisting "wrangler.toml"

    // Add new resources via API
    kv "NEW_CACHE" (ensureNamespace "new-cache")
}

config |> hybrid client  // Provisions then generates TOML
```

### Step 3: Full API Deployment

```fsharp
// Eventually move to pure API deployment
let config = cloudflare {
    // Everything defined in F#
    // No TOML needed
}

config |> deploy client
```

## Implementation Roadmap

### Phase 1: Basic Direct Deployment

```fsharp
// Simple F# script deployment
cfs deploy-direct ./worker.js --bindings ./bindings.fsx
```

### Phase 2: Integrated Resource Management

```fsharp
// Full F# deployment script - this IS the configuration
// No TOML or YAML needed - code is the single source of truth
#r "nuget: CloudflareFS"

let deploy() = cloudflare {
    // Provision resources - all in F# code
    let! kv = ensureKVNamespace "cache"
    let! r2 = ensureR2Bucket "storage"
    let! d1 = ensureD1Database "database"

    // Deploy worker with bindings - pure F# computation expression
    worker "my-api" {
        code "./dist/worker.js"
        bindings [
            bind "CACHE" kv
            bind "STORAGE" r2
            bind "DB" d1
        ]
        routes ["api.example.com/*"]
    }

    // Optional: Export wrangler.toml for secondary tooling compatibility
    // This is a backward compatibility feature, not the primary workflow
    exportWranglerCompat "./wrangler.toml"
}
```

### Phase 3: Complete Wrangler Feature Parity

- Implement all wrangler features in F#
- Local dev server integration
- Tail logging
- Secret management
- Cron triggers

## Proof of Concept

A minimal implementation requires:

1. **Complete Workers Management API binding** (currently empty)
2. **Multipart form upload helper**
3. **Metadata serializer for bindings**
4. **Simple CLI command**

```fsharp
// Minimal POC
type CloudflareDirect(apiToken: string, accountId: string) =
    let client = new HttpClient()
    do client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}")

    member this.QuickDeploy(scriptName, jsCode: string, bindings) = async {
        // This would work with current APIs
        let metadata = {|
            main_module = "worker.js"
            compatibility_date = "2024-01-01"
            bindings = bindings
        |}

        // ... multipart upload implementation
    }
```

## Updated Implementation Strategy

### What CloudflareFS Should Build

1. **Workers Upload Client** (Priority 1):

```fsharp
type WorkerDeployment = {
    Script: byte[]  // Fable output
    Assets: AssetManifest option  // Static files
    Bindings: WorkerBinding list
    Routes: Route list
    CompatibilityDate: string
    CompatibilityFlags: string list
}

let deployDirectly (deployment: WorkerDeployment) = async {
    // 1. Upload assets if present
    let! assetJwt =
        match deployment.Assets with
        | Some assets -> uploadAssets accountId scriptName assets
        | None -> async { return None }

    // 2. Create metadata with ALL binding types
    let metadata = createMetadata deployment assetJwt

    // 3. Single multipart upload
    return! uploadWorker accountId scriptName deployment.Script metadata
}
```

2. **Keep Wrangler For**:
   - Local development (`wrangler dev` with hot reload)
   - Complex JavaScript bundling scenarios
   - Initial project scaffolding

3. **Replace Wrangler For**:
   - CI/CD deployments (direct API faster)
   - F# script-based deployments
   - Multi-environment orchestration
   - Production deployments with full control

## Recommendation

**Core Approach**:
CloudflareFS implements code-first IaC as a primary feature. Configuration files like TOML and YAML are treated as secondary formats. While we may provide export functionality for compatibility, the primary approach is pure F# code configuration.

**Short Term**:

1. Implement direct deployment via F# scripts for all scenarios
2. Support wrangler.toml export for secondary tool compatibility
3. Keep wrangler for local development 

**Long Term**:

1. Feature parity with wrangler functionality
2. Direct API calls for everything via F# code
3. No configuration files needed - pure code-driven infrastructure
4. F# scripts as the single source of truth for all deployments

## Conclusion

CloudflareFS's code-first approach provides a viable alternative for Cloudflare infrastructure management. After analyzing wrangler's source code, we've confirmed that static configuration files like TOML are unnecessary for the purposes of this toolkit and can be eliminated in favor of direct F# code managed deployment to forward environments.

The discoveries reveal:

1. **Complete Binding Inventory**: Every binding can be expressed as F# code, not TOML
2. **Assets Binding**: The new `assets` binding integrates well with code-first deployment
3. **Direct API Access**: F# scripts can call APIs directly without TOML intermediaries
4. **No Configuration Files Required**: Everything is achievable through code

The wrangler source code confirms our code-first approach is viable because:

- All APIs accept programmatic input (suitable for F# scripts)
- No static files are actually required by Cloudflare's APIs
- Everything can be computed and deployed via code

CloudflareFS already has most of what's needed. The missing components are:

1. **Workers Script Upload API binding** (for direct F# script deployment)
2. **Asset manifest generation** (code-driven, not config-driven)
3. **CLI commands** that execute F# scripts directly

**Immediate Next Steps**:

1. Implement F# script-based deployment without config files
2. Create code-driven asset manifest generation
3. Build POC that shows pure F# deployment (with optional wrangler.toml export)
4. Demonstrate code-first IaC implementation

## Summary

CloudflareFS provides flexible deployment modes:

1. **Direct** - Pure API deployment for maximum control
2. **Offline** - TOML generation for compatibility
3. **Hybrid** - Resource provisioning + TOML generation

This allows gradual migration from TOML-based workflows to full API-driven deployment while maintaining compatibility with existing tools and pipelines.