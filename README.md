# CloudflareFS

[![NuGet](https://img.shields.io/nuget/v/CloudflareFS.Core.svg)](https://www.nuget.org/packages/CloudflareFS.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE-MIT)
[![License: Apache 2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE-APACHE)
[![Powered by Cloudflare](https://img.shields.io/badge/Powered%20by-Cloudflare-orange?logo=cloudflare&logoColor=white)](https://www.cloudflare.com)

<div align="center">

  ### 🚧 Preliminary Work - Not Production Ready 🚧

</div>

CloudflareFS brings F# to the Cloudflare platform through comprehensive, type-safe bindings auto-generated from TypeScript definitions and OpenAPI specifications. This project aims to advance both the binding generation tools (Hawaii and Glutinum) and eventually deliver production-ready F# libraries for Cloudflare's entire ecosystem.

## Overview

CloudflareFS is more than a set of libraries, it is designed to provide a complete set of F# tools that targets Cloudflare's platform through a **dual-layer architecture**:

- **Runtime APIs**: In-Worker JavaScript interop for edge computing
- **Management APIs**: REST clients for infrastructure provisioning

Built on [Fable](https://fable.io/) for JavaScript compilation, [Glutinum](https://github.com/glutinum-org/cli) for TypeScript binding generation, and [Hawaii](https://github.com/Zaid-Ajaj/Hawaii) for OpenAPI client generation.

## Architecture

### Two-Layer Design

```bash
CloudflareFS/
└─ src/
   ├── Runtime/           # In-Worker APIs (JavaScript interop)
   │   ├── CloudFlare.Worker.Context/
   │   ├── CloudFlare.D1/
   │   ├── CloudFlare.R2/
   │   ├── CloudFlare.KV/
   │   └── CloudFlare.AI/
   │
   └── Management/        # REST APIs (HTTP clients)
       ├── CloudFlare.Management.D1/
       ├── CloudFlare.Management.R2/
       ├── CloudFlare.Management.Analytics/
       └── CloudFlare.Management.KV/
```

### Runtime Layer (In-Worker)
- **Purpose**: Operations inside Cloudflare Workers
- **Source**: TypeScript definitions via Glutinum
- **Usage**: Direct platform access with microsecond latency

### Management Layer (External)
- **Purpose**: Infrastructure provisioning, monitoring and management
- **Source**: OpenAPI specs via Hawaii
- **Usage**: REST API clients for external tools

## Current Implementation Status

> **⚠️ Important Note**: While the generated code is coherent, extensive testing and validation is required before production use.

### ✅ Generated

| Layer | Package | Description |
|-------|---------|-------------|
| **Runtime** | CloudFlare.Worker.Context | Core Worker types (Request, Response) |
| **Runtime** | CloudFlare.KV | Key-Value storage bindings |
| **Runtime** | CloudFlare.R2 | Object storage bindings |
| **Runtime** | CloudFlare.D1 | Database bindings |
| **Runtime** | CloudFlare.AI | Workers AI bindings |
| **Runtime** | CloudFlare.Queues | Message queue bindings |
| **Runtime** | CloudFlare.Vectorize | Vector database bindings |
| **Runtime** | CloudFlare.Hyperdrive | Database connection pooling |
| **Runtime** | CloudFlare.DurableObjects | Stateful serverless compute |
| **Management** | CloudFlare.Management.Workers | Worker deployment and configuration |
| **Management** | CloudFlare.Management.R2 | R2 bucket management |
| **Management** | CloudFlare.Management.D1 | D1 database management |
| **Management** | CloudFlare.Management.Analytics | Analytics API client |
| **Management** | CloudFlare.Management.Queues | Queue management |
| **Management** | CloudFlare.Management.Vectorize | Vector index management (V2 API) |
| **Management** | CloudFlare.Management.Hyperdrive | Connection config management |
| **Management** | CloudFlare.Management.DurableObjects | Namespace management |

### 🔄 In Progress

- CloudFlare.Management.KV (Hawaii generation issues)
- CloudFlare.Management.Logs (spec extraction pending)
- Browser APIs (WebSockets, Streams, Cache, WebCrypto)

### 📝 Recent Updates

- **Namespace Standardization** (January 2025): Unified all Management APIs under consistent `CloudFlare.Management.*` naming, removing legacy `Api.Compute.*` and `Api.Storage.*` patterns.
- **System.Text.Json Migration**: All generated clients now use `FSharp.SystemTextJson` instead of `Newtonsoft.Json` for better Fable compatibility.
- **Hawaii Post-Processing**: Automated post-processing pipeline for discriminated unions and type generation improvements.
- **Vectorize V2 Migration**: Successfully migrated from deprecated V1 API to V2 (August 2024). Hawaii correctly skips deprecated operations.
- **Full Compilation**: All Runtime and Management packages now compile cleanly with zero errors.

## Installation

### Runtime Packages (For Workers)
```bash
dotnet add package CloudFlare.Worker.Context
dotnet add package CloudFlare.D1
dotnet add package CloudFlare.R2
dotnet add package CloudFlare.KV
```

### Management Packages (For Tools/Scripts)
```bash
dotnet add package CloudFlare.Management.Workers
dotnet add package CloudFlare.Management.D1
dotnet add package CloudFlare.Management.R2
dotnet add package CloudFlare.Management.Analytics
```

## Usage Examples

### Complete Workflow: Infrastructure + Runtime

```fsharp
// 1. Infrastructure Setup (Management API - runs on your machine)
open CloudFlare.Management.D1
open System.Net.Http

let setupInfrastructure (accountId: string) (apiToken: string) = async {
    let httpClient = new HttpClient()
    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}")

    let d1Client = D1ManagementClient(httpClient)
    let! database = d1Client.CreateDatabase(
        accountId = accountId,
        name = "production-db",
        primaryLocationHint = Some "wnam"
    )

    printfn $"Created database: {database.uuid}"
    return database.uuid
}

// 2. Runtime Operations (Runtime API - runs in Worker)
open CloudFlare.D1
open CloudFlare.Worker.Context

[<Export>]
let fetch (request: Request) (env: Env) (ctx: ExecutionContext) =
    async {
        // env.DATABASE is bound via wrangler.toml
        let db = env.DATABASE

        match request.method with
        | "GET" ->
            let! users = db.prepare("SELECT * FROM users").all<User>()
            return Response.json(users)

        | "POST" ->
            let! body = request.json<User>()
            let! result =
                db.prepare("INSERT INTO users (name, email) VALUES (?, ?)")
                  .bind(body.name, body.email)
                  .run()
            return Response.json({| success = result.success |})

        | _ -> return Response.methodNotAllowed()
    }
```

### KV Storage Example

```fsharp
// Runtime API - Inside Worker
open CloudFlare.KV

let handleKVRequest (env: Env) = async {
    // Get value
    let! value = env.CACHE.get("user:123")

    // Set with expiration
    do! env.CACHE.put("session:abc", userJson,
        KVPutOptions(expirationTtl = 3600))

    // List keys with prefix
    let! keys = env.CACHE.list(KVListOptions(prefix = "user:"))

    return Response.json(keys)
}
```

### R2 Object Storage Example

```fsharp
// Management API - Create bucket
let createBucket (accountId: string) = async {
    let r2Client = R2ManagementClient(httpClient)
    let! bucket = r2Client.CreateBucket(
        accountId = accountId,
        name = "my-assets",
        location = Some "wnam"
    )
    return bucket
}

// Runtime API - Use bucket
let handleR2Request (env: Env) = async {
    let bucket = env.ASSETS

    // Get object
    let! obj = bucket.get("image.png")
    match obj with
    | Some r2Object ->
        return Response.create(r2Object.body,
            ResponseInit(headers = r2Object.httpMetadata))
    | None ->
        return Response.notFound()
}
```

## Building from Source

### Prerequisites
- .NET 8.0 or later
- Node.js 18+
- Glutinum CLI: `npm install -g @glutinum/cli`
- Hawaii: `dotnet tool install -g hawaii`

### Build Commands

```bash
# Clone repository
git clone https://github.com/yourusername/CloudflareFS.git
cd CloudflareFS

# Build Runtime bindings
dotnet build src/Runtime/CloudFlare.Worker.Context

# Build Management APIs
dotnet build src/Management/CloudFlare.Management.D1

# Run sample Worker
cd samples/HelloWorker
dotnet fable . --outDir dist
npx wrangler dev
```

## Generation Pipeline

### Runtime Bindings (TypeScript → F#)
```bash
cd generators/glutinum
npx @glutinum/cli generate
    ../../node_modules/@cloudflare/workers-types/index.d.ts \
    --output ../../src/Runtime/CloudFlare.Worker.Context/Generated.fs
```

### Management APIs (OpenAPI → F#)
```bash
cd generators/hawaii

# 1. Segment the massive OpenAPI spec
dotnet fsi extract-services.fsx

# 2. Generate F# clients
hawaii --config d1-hawaii.json
```

## Sample Projects

### HelloWorker
Basic Worker with KV storage:
```bash
cd samples/HelloWorker
dotnet fable . --outDir dist
npx wrangler dev
```

### SecureChat
Production-ready chat API featuring:
- User authentication via Cloudflare Secrets
- D1 database for message persistence
- PowerShell user management scripts
- Separate React UI with Tailwind CSS

```bash
cd samples/SecureChat
.\scripts\add-user.ps1 -Username alice -Password "Pass123!"
dotnet fable . --outDir dist
npx wrangler dev
```

## Vision & Roadmap

> **Note**: The following features represent future roadmap elements for CloudflareFS as a complete toolkit.

### The CloudflareFS CLI (`cfs`)

The `cfs` command-line tool will be a key command-and-control element of CloudflareFS, providing a type-safe, F#-first wrapper to Wrangler that unifies both runtime deployment and management plane orchestration. Unlike traditional TOML-based configuration, `cfs` will use F# scripts for infrastructure-as-code with full IntelliSense support.

#### Deployment Flexibility

```fsharp
// deploy.fsx - Type-safe deployment configuration
#r "nuget: CloudflareFS"
open CloudflareFS.Deployment

let deploy env = cloudflare {
    account (getAccountId env)

    worker $"api-service-{env}" {
        // Intelligent resource management
        kv "CACHE" (ensureOrCreate "cache-namespace")
        r2 "STORAGE" (ensureOrCreate "assets-bucket")
        d1 "DATABASE" (ensureOrCreate "app-database" {
            migrations = "./migrations"
            location = "wnam"
        })

        route $"api-{env}.example.com/*"
        compatibilityDate "2024-01-01"
    }
}

// Execute with different modes:
// cfs deploy ./deploy.fsx              # Direct API deployment
// cfs deploy ./deploy.fsx --offline    # Generate wrangler.toml
// cfs deploy ./deploy.fsx --hybrid     # Provision + TOML generation
```

#### Orchestration Capabilities

The CLI will handle the complete lifecycle of Cloudflare resources:

- **Provisioning**: Create and configure KV namespaces, R2 buckets, D1 databases
- **Migration**: Database schema migrations, data transfers, blue-green deployments
- **Monitoring**: Real-time logs, metrics, and alerts directly in the terminal
- **Testing**: Integration test runners for Workers
- **Rollback**: Intelligent deployment history with one-command rollbacks

### Firetower - Visual Platform Observatory

Firetower will provide Erlang Observer-style monitoring for Cloudflare, deployable both as a desktop application and on Cloudflare Pages itself:

#### Real-Time Observability

- **Worker Health**: CPU usage, memory consumption, request rates, error tracking
- **Resource Monitoring**: KV operations, R2 bandwidth, D1 query performance
- **Distributed Tracing**: Request flow across Workers and services
- **Cost Analytics**: Real-time billing estimates and optimization suggestions

### Unified Development Experience

The complete CloudflareFS toolkit will provide:

1. **Local Development**: Full Worker emulation with F# hot-reload with full multi-Worker local development support that Cloudflare supports
2. **CI/CD Integration**: GitHub Actions first, with GitLab CI templates for later in the roadmap
3. **Multi-Environment Management**: Development, staging, and production configurations
4. **Team Collaboration**: Shared deployment scripts with role-based permissions
5. **Compliance & Auditing**: Deployment history, change tracking, and approval workflows

## Documentation

### Core Architecture
- [Architecture Decisions](docs/00_architecture_decisions.md) - Key design choices and roadmap
- [Generation Strategy](docs/01_generation_strategy.md) - Glutinum vs Hawaii code generation
- [Dual Layer Architecture](docs/02_dual_layer_architecture.md) - Runtime vs Management APIs
- [OpenAPI Generation](docs/03_openapi_generation.md) - Hawaii setup and OpenAPI handling

### Implementation Details
- [Code-First Deployment](docs/04_code_first_deployment.md) - Code-driven deployment strategies
- [Gap Analysis](docs/05_gap_analysis.md) - Coverage comparison with workers-sdk
- [Conversion Patterns](docs/08_conversion_patterns.md) - TypeScript to F# patterns

### Tools & Future
- [Firetower Concept](docs/06_firetower_concept.md) - Monitoring tool design
- [Pulumi Insights](docs/07_pulumi_insights.md) - Lessons from Pulumi's approach

### Examples
- [Samples](samples/) - Working examples demonstrating the framework capabilities

## Contributing

We welcome contributions! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Development Areas

CloudflareFS is advancing toward production-ready bindings through systematic tool improvement and comprehensive coverage:

**Completed**:
- ✅ Management API namespace standardization (all services now use `CloudFlare.Management.*`)
- ✅ Workers Management API with automated post-processing for discriminated unions
- ✅ System.Text.Json migration for Fable compatibility
- ✅ All Runtime bindings (KV, R2, D1, AI, Queues, Vectorize, Hyperdrive, DurableObjects)
- ✅ 8 Management APIs fully generated and compiling

**In Progress**:
- 🔄 KV Management API (Hawaii generation challenges)
- 🔄 Logs Management API (extraction patterns pending)
- 🔄 Tool improvement analysis for Glutinum and Hawaii

**Future Work**:
- Build the `cfs` CLI tool for type-safe deployment
- Create Firetower monitoring application
- Expand sample applications and documentation
- Contribute improvements back to Glutinum and Hawaii

## Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/CloudflareFS/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/CloudflareFS/discussions)

## License

Licensed under either of

* Apache License, Version 2.0 ([LICENSE-APACHE](LICENSE-APACHE) or http://www.apache.org/licenses/LICENSE-2.0)
* MIT license ([LICENSE-MIT](LICENSE-MIT) or http://opensource.org/licenses/MIT)

at your option.

### Contribution

Unless you explicitly state otherwise, any contribution intentionally submitted for inclusion in the work by you shall be dual licensed as above, without any additional terms or conditions.

---

## Acknowledgments

CloudflareFS stands on the shoulders of giants:

- **[Fable](https://fable.io/)** - The magnificent F# to JavaScript compiler. Special thanks to Alfonso García-Caro, Maxime Mangel and all maintainers/contributors.

- **[Glutinum](https://github.com/glutinum-org)** - TypeScript to F# binding generator. Thanks to Maxime Mangel for this invaluable tool.

- **[Hawaii](https://github.com/Zaid-Ajaj/Hawaii)** - OpenAPI to F# client generator. Thanks to Zaid Ajaj for creating this and pioneering F# on Cloudflare Workers.

- **[F# Software Foundation](https://fsharp.org/)** - For fostering a language that makes functional programming practical and enjoyable.

- **[Cloudflare](https://cloudflare.com)** - For building an incredible edge platform and commitment to open source.

This project is SpeakEZ's contribution back to these amazing communities.
