# CloudflareFS

[![NuGet](https://img.shields.io/nuget/v/CloudflareFS.Core.svg)](https://www.nuget.org/packages/CloudflareFS.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE-MIT)
[![License: Apache 2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE-APACHE)

Comprehensive F# bindings and tooling for the entire Cloudflare platform, bringing type-safe, functional-first development to edge computing and cloud infrastructure.

## Overview

CloudflareFS provides complete F# bindings for Cloudflare's platform through a **dual-layer architecture**:
- **Runtime APIs**: In-Worker JavaScript interop for edge computing
- **Management APIs**: REST clients for infrastructure provisioning

Built on [Fable](https://fable.io/) for JavaScript compilation, [Glutinum](https://github.com/glutinum-org/cli) for TypeScript binding generation, and [Hawaii](https://github.com/Zaid-Ajaj/Hawaii) for OpenAPI client generation.

## Architecture

### Two-Layer Design

```
CloudflareFS/
├── src/
│   ├── Runtime/           # In-Worker APIs (JavaScript interop)
│   │   ├── CloudFlare.Worker.Context/
│   │   ├── CloudFlare.D1/
│   │   ├── CloudFlare.R2/
│   │   ├── CloudFlare.KV/
│   │   └── CloudFlare.AI/
│   │
│   └── Management/        # REST APIs (HTTP clients)
│       ├── CloudFlare.Management.D1/
│       ├── CloudFlare.Management.R2/
│       ├── CloudFlare.Management.Analytics/
│       └── CloudFlare.Management.KV/
```

### Runtime Layer (In-Worker)
- **Purpose**: Operations inside Cloudflare Workers
- **Source**: TypeScript definitions via Glutinum
- **Usage**: Direct platform access with microsecond latency

### Management Layer (External)
- **Purpose**: Infrastructure provisioning and management
- **Source**: OpenAPI specs via Hawaii
- **Usage**: REST API clients for external tools

## Current Implementation Status

### ✅ Completed

| Layer | Package | Description |
|-------|---------|-------------|
| **Runtime** | CloudFlare.Worker.Context | Core Worker types (Request, Response) |
| **Runtime** | CloudFlare.KV | Key-Value storage bindings |
| **Runtime** | CloudFlare.R2 | Object storage bindings |
| **Runtime** | CloudFlare.D1 | Database bindings |
| **Runtime** | CloudFlare.AI | Workers AI bindings |
| **Management** | CloudFlare.Management.R2 | R2 bucket management |
| **Management** | CloudFlare.Management.D1 | D1 database management |
| **Management** | CloudFlare.Management.Analytics | Analytics API client |

### 🔄 In Progress

- CloudFlare.Management.KV (Hawaii generation issues)
- CloudFlare.Management.Workers (Hawaii generation issues)
- Runtime bindings for Durable Objects, Queues, Vectorize

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

## Future Roadmap

### CloudflareFS CLI (`cfs`)
F#-based alternative to Wrangler:
```fsharp
// Deploy with F# script instead of wrangler.toml
cfs deploy ./deploy.fsx

// deploy.fsx
let config = {
    name = "my-worker"
    compatibilityDate = "2024-01-01"
    bindings = [
        D1Database("DATABASE", databaseId)
        KVNamespace("CACHE", namespaceId)
    ]
}
```

### Firetower Monitoring Tool
Real-time Cloudflare platform monitoring:
- Desktop app via Avalonia.FuncUI
- Web deployment on Cloudflare Pages
- Live metrics, logs, and alerts
- Inspired by Erlang's Observer

### Type Provider for wrangler.toml
```fsharp
type MyWorker = WranglerProvider<"./wrangler.toml">

// Fully typed bindings from config
let db = MyWorker.Bindings.DATABASE
let kv = MyWorker.Bindings.CACHE
```

## Documentation

- [Architecture Decisions](docs/ARCHITECTURE_DECISIONS.md) - Detailed architecture documentation
- [Runtime vs Management](docs/RUNTIME_VS_MANAGEMENT.md) - Understanding the dual-layer design
- [Firetower Concept](docs/FIRETOWER_CONCEPT.md) - Monitoring tool design
- [Samples](samples/) - Working examples

## Contributing

We welcome contributions! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Development Areas
- Complete remaining Runtime bindings (Durable Objects, Queues)
- Fix Hawaii generation for KV/Workers Management APIs
- Build the `cfs` CLI tool
- Create more sample applications
- Improve documentation

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

- **[Fable](https://fable.io/)** - The magnificent F# to JavaScript compiler. Special thanks to Alfonso García-Caro and all contributors.

- **[Glutinum](https://github.com/glutinum-org)** - TypeScript to F# binding generator. Thanks to Maxime Mangel for this invaluable tool.

- **[Hawaii](https://github.com/Zaid-Ajaj/Hawaii)** - OpenAPI to F# client generator. Thanks to Zaid Ajaj for creating this and pioneering F# on Cloudflare Workers.

- **[F# Software Foundation](https://fsharp.org/)** - For fostering a language that makes functional programming practical and enjoyable.

- **[Cloudflare](https://cloudflare.com)** - For building an incredible edge platform and commitment to open source.

This project is our contribution back to these amazing communities.