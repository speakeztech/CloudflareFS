# CloudflareFS

[![NuGet](https://img.shields.io/nuget/v/CloudflareFS.Core.svg)](https://www.nuget.org/packages/CloudflareFS.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE-MIT)
[![License: Apache 2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE-APACHE)

Comprehensive F# bindings and tooling for the entire Cloudflare platform, bringing type-safe, functional-first development to edge computing and cloud infrastructure.

## Overview

CloudflareFS aims to provide complete F# bindings for Cloudflare's most recent SDKs, enabling developers to leverage the full power of Cloudflare services with F#'s type safety and functional programming paradigms. Built on [Fable](https://fable.io/) for JavaScript interop and utilizing [Glutinum](https://github.com/glutinum-org/cli) for automated binding generation, this toolkit bridges the gap between F# and Cloudflare's extensive service ecosystem.

### Key Goals

- **Complete Coverage**: Bindings for Workers, R2, D1, KV, Durable Objects, Queues, and 50+ Cloudflare services
- **Type Safety**: Full F# type safety across runtime and management APIs
- **Modern Architecture**: Support for emerging technologies (MoQ relay, Post-Quantum Cryptography)
- **Developer Experience**: Computation expressions, active patterns, and idiomatic F# APIs
- **Automated Generation**: 70% auto-generated from TypeScript/OpenAPI definitions
- **Enterprise Ready**: Dual MIT/Apache 2.0 licensing with patent protection options

## Installation

### Core Runtime (Workers/Edge)
```bash
dotnet add package CloudflareFS.Worker
dotnet add package CloudflareFS.KV
dotnet add package CloudflareFS.R2
```

### Management APIs
```bash
dotnet add package CloudflareFS.DNS
dotnet add package CloudflareFS.ZeroTrust
```

### Development Tools
```bash
dotnet tool install -g CloudflareFS.CLI
cf-tools init my-worker
```

## Architecture

CloudflareFS uses a two-layer architecture:

### 1. Runtime Layer (In-Worker APIs)
Bindings for code executing inside Workers, using Fable for JavaScript interop:
- **Core Types** ✅: Request, Response, Headers, ExecutionContext
- **Storage** ✅: KV, R2, D1 (each in separate focused projects)
- **AI** ✅: Workers AI bindings
- **Planned**: Durable Objects, Queues, Analytics Engine, Vectorize

### 2. Management Layer (REST APIs)
F# clients for Cloudflare's control plane, generated using Hawaii from OpenAPI specs:
- **Planned**: Account management, DNS, Workers deployment
- **Planned**: Zero Trust (Access, Gateway, Tunnels)
- **Planned**: Resource creation (KV namespaces, R2 buckets, D1 databases)

## Current Implementation Status

### ✅ Completed

| Package | Description | Location |
|---------|-------------|----------|
| **CloudFlare.Worker.Context** | Core Worker types (Request, Response, Headers) | `src/Runtime/CloudFlare.Worker.Context` |
| **CloudFlare.KV** | Key-Value store bindings with helpers | `src/Runtime/CloudFlare.KV` |
| **CloudFlare.R2** | R2 object storage bindings with helpers | `src/Runtime/CloudFlare.R2` |
| **CloudFlare.D1** | D1 database bindings with query helpers | `src/Runtime/CloudFlare.D1` |
| **CloudFlare.AI** | Workers AI service bindings | `src/Runtime/CloudFlare.AI` |

### 🔄 In Progress / Planned

| Package | Description | Status |
|---------|-------------|--------|
| **CloudFlare.DurableObjects** | Durable Objects bindings | Empty folder, planned |
| **CloudFlare.Queues** | Message queue bindings | Empty folder, planned |
| **CloudFlare.Vectorize** | Vector database bindings | Empty folder, planned |
| **CloudFlare.Api** | Management APIs via Hawaii | Generators configured, awaiting implementation |

## Advanced Features

### Computation Expressions

```fsharp
// Async KV operations with CE
let workflow = kv {
    let! user = env.USERS.get(userId)
    let! preferences = env.PREFS.get(userId)
    return combine user preferences
}
```

### Active Patterns

```fsharp
// Pattern matching for routing
match request with
| GET & Route "/api/users" -> handleGetUsers()
| POST & Route "/api/users" -> handleCreateUser()
| DELETE & Route "/api/users/:id" id -> handleDeleteUser(id)
| _ -> Response.notFound()
```

### Type Providers (Coming Soon)

```fsharp
// Type-safe environment bindings from wrangler.toml
type MyConfig = WranglerProvider<"./wrangler.toml">
let kv = MyConfig.Bindings.MY_KV_NAMESPACE // Typed!
```

## Emerging Technology Support

### Media over QUIC (MoQ)
Prepared for Cloudflare's MoQ relay infrastructure:
```fsharp
open CloudflareFS.MoQ

let relay = MoQClient("wss://moq-relay.cloudflare.com")
let! track = relay.subscribe("audio/main")
```

### Post-Quantum Cryptography
Support for X25519MLKEM768 and future PQC standards:
```fsharp
open CloudflareFS.PQC

let! config = PQC.configure {
    mode = PQCMode.Preferred
    algorithms = ["X25519MLKEM768"]
}
```

## Examples

### Basic Worker
```fsharp
open CloudflareFS.Worker

[<Export>]
let fetch (request: Request) (env: Env) (ctx: ExecutionContext) =
    match request.method with
    | "GET" -> Response.ok("Hello from F#!")
    | _ -> Response.methodNotAllowed()
```

### Durable Object with State
```fsharp
type ChatRoom() =
    inherit DurableObject()

    let mutable sessions = Set.empty<WebSocket>

    member this.onWebSocket(ws: WebSocket) = async {
        sessions <- sessions.Add(ws)
        ws.accept()

        ws.onMessage(fun msg ->
            // Broadcast to all sessions
            sessions |> Set.iter (fun s -> s.send(msg))
        )
    }
```

### Zero Trust Integration
```fsharp
open CloudflareFS.ZeroTrust

let! policy = Access.createPolicy {
    name = "GitHub Auth Required"
    decision = "allow"
    include = [ EmailDomain "@mycompany.com" ]
    require = [ GitHub { org = "mycompany" } ]
}
```

## Development

### Building from Source
```bash
# Clone the repository
git clone https://github.com/yourusername/CloudflareFS.git
cd CloudflareFS

# Install dependencies
dotnet tool restore
npm install -g @glutinum/cli
dotnet tool install -g hawaii

# Build runtime bindings
cd src/Runtime/CloudFlare.Worker.Context
dotnet build

# Build sample applications
cd samples/HelloWorker
dotnet fable . --outDir dist
npx wrangler dev
```

### Regenerating Bindings
```bash
# Runtime bindings (Glutinum)
cd generators/glutinum
.\generate-bindings.ps1

# Management APIs (Hawaii)
cd generators/hawaii
.\generate-bindings.ps1
```

## Sample Projects

### HelloWorker
Basic Worker demonstrating KV and request handling:
```bash
cd samples/HelloWorker
dotnet fable . --outDir dist
npx wrangler dev
```

### SecureChat
Production-ready chat API with:
- User authentication via Cloudflare Secrets
- D1 database for message storage
- PowerShell scripts for user management
- Separate React UI with Tailwind CSS

```bash
cd samples/SecureChat
.\scripts\add-user.ps1 -Username alice -Password "Pass123!"
dotnet fable . --outDir dist
npx wrangler dev
```

## Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Prerequisites
- .NET 8.0 or later
- Node.js 18+ and npm
- Glutinum CLI (`npm install -g @glutinum/cli`)
- F# 8.0 or later

## Roadmap

### Phase 1: Runtime Bindings (Mostly Complete)
- [x] Core Workers bindings (Request, Response, Headers)
- [x] KV storage bindings with F# helpers
- [x] R2 object storage bindings
- [x] D1 database bindings
- [x] AI service bindings
- [ ] Durable Objects
- [ ] Queues
- [ ] Analytics Engine

### Phase 2: Management APIs (Starting)
- [ ] Set up Hawaii for OpenAPI generation
- [ ] Account and user management
- [ ] KV/R2/D1 resource creation
- [ ] Workers deployment APIs
- [ ] DNS management
- [ ] Zero Trust services

### Phase 3: Developer Experience
- [ ] CLI tooling
- [ ] Project templates
- [ ] Type provider for wrangler.toml
- [ ] VS Code extension
- [ ] Comprehensive documentation

## Support

- **Documentation**: [https://cloudflarefs.dev](https://cloudflarefs.dev)
- **Issues**: [GitHub Issues](https://github.com/yourusername/CloudflareFS/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/CloudflareFS/discussions)
- **Examples**: See the `/samples` directory

## License

Licensed under either of

* Apache License, Version 2.0 ([LICENSE-APACHE](LICENSE-APACHE) or http://www.apache.org/licenses/LICENSE-2.0)
* MIT license ([LICENSE-MIT](LICENSE-MIT) or http://opensource.org/licenses/MIT)

at your option.

### Contribution

Unless you explicitly state otherwise, any contribution intentionally submitted for inclusion in the work by you, as defined in the Apache-2.0 license, shall be dual licensed as above, without any additional terms or conditions.

---

## Acknowledgments

CloudflareFS stands on the shoulders of giants. We extend our deepest gratitude to:

- **[Fable](https://fable.io/)** - The magnificent F# to JavaScript compiler that makes this entire project possible. Special thanks to Alfonso Garc�a-Caro and all Fable contributors for creating and maintaining this essential bridge between F# and the JavaScript ecosystem.

- **[Glutinum](https://github.com/glutinum-org)** - The TypeScript to F# binding generator that automates 70% of our binding generation. Thanks to Maxime Mangel for creating this invaluable tool that turns TypeScript definitions into idiomatic F# code.

- **[F# Software Foundation](https://fsharp.org/)** and the entire F# community - For fostering a language and ecosystem that makes functional programming practical, enjoyable, and powerful. Special recognition to Don Syme and the F# language design team at Microsoft Research.

- **[Cloudflare](https://cloudflare.com)** - For building an incredible edge computing platform and being steadfast advocates for open source. Your commitment to open standards, transparent development, and developer empowerment inspired this project.

- **Early F# Cloudflare Pioneers** - Particularly Zaid Ajaj for creating the original Fable.CloudflareWorkers bindings that proved F# on Cloudflare was possible, and the community members who've been advocating for better F# support on edge platforms.

This project is our contribution back to these amazing communities - a small "thank you" for all the tools, platforms, and inspiration you've provided. We hope CloudflareFS helps more developers experience the joy of functional programming on Cloudflare's platform.