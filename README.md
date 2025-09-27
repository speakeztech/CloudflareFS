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

CloudflareFS is organized into three layers:

### 1. Runtime Layer (Fable -> JavaScript)
Bindings for code executing in Workers/Pages environments, generated from `@cloudflare/*` TypeScript packages:
- Worker Context, Fetch, Cache, Streams
- Storage: KV, R2, D1, Durable Objects
- Queues, Vectorize, Hyperdrive
- AI, WebSockets, WebCrypto

### 2. Management Layer (REST APIs)
F# clients for Cloudflare's control plane, generated from OpenAPI schemas:
- Zero Trust (Access, Gateway, Tunnels, CASB, DLP)
- DNS, Load Balancing, WAF
- Analytics, Billing, User Management

### 3. Tool Layer
Development and deployment utilities:
- Wrangler CLI wrapper
- Secret management with encryption
- Miniflare integration for testing
- Project templates and scaffolding

## Package Structure

| Package | Description | Source |
|---------|-------------|--------|
| **CloudflareFS.Core** | Shared types and utilities | Hand-written |
| **CloudflareFS.Worker** | Workers runtime bindings | Generated from `@cloudflare/workers-types` |
| **CloudflareFS.KV** | Key-Value store | Generated from `@cloudflare/workers-types` |
| **CloudflareFS.R2** | Object storage (S3-compatible) | Generated from `@cloudflare/workers-types` |
| **CloudflareFS.D1** | SQLite database | Generated from `@cloudflare/workers-types` |
| **CloudflareFS.AI** | Workers AI bindings | Generated from `@cloudflare/ai` |
| **CloudflareFS.ZeroTrust** | Zero Trust services | Generated from OpenAPI |
| **CloudflareFS.DNS** | DNS management | Generated from OpenAPI |
| **CloudflareFS.Wrangler** | CLI tooling wrapper | Hand-written |

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
npm install

# Generate bindings
dotnet fsi build/Generate.fsx

# Build all packages
dotnet build

# Run tests
dotnet test
```

### Regenerating Bindings
```bash
# Update TypeScript bindings via Glutinum
npm update @cloudflare/workers-types
dotnet fsi build/GenerateBindings.fsx

# Update REST API clients from OpenAPI
dotnet fsi build/GenerateOpenApi.fsx
```

## Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Prerequisites
- .NET 8.0 or later
- Node.js 18+ and npm
- Glutinum CLI (`npm install -g @glutinum/cli`)
- F# 8.0 or later

## Roadmap

- [ ] Core Workers bindings
- [ ] Storage services (KV, R2, D1)
- [ ] Zero Trust management APIs
- [ ] Complete AI service bindings
- [ ] MoQ relay support (pending Cloudflare release)
- [ ] Type provider for wrangler.toml
- [ ] Visual Studio Code extension
- [ ] Complete test coverage

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