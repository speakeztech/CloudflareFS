# CloudflareFS Deployment Modes

## Overview

CloudflareFS supports multiple deployment modes to fit different workflows - from direct API deployment to offline TOML generation for traditional CI/CD pipelines.

## Deployment Modes

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

## Configuration Strategies

### Strategy 1: Environment-Aware Deployment
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

### Strategy 2: Resource Resolution
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

## Summary

CloudflareFS provides flexible deployment modes:
1. **Direct** - Pure API deployment for maximum control
2. **Offline** - TOML generation for compatibility
3. **Hybrid** - Resource provisioning + TOML generation

This allows gradual migration from TOML-based workflows to full API-driven deployment while maintaining compatibility with existing tools and pipelines.