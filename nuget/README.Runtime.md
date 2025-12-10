# CloudflareFS.Runtime

**F# and Fable bindings for Cloudflare Workers Runtime APIs**

> **Worker-side bindings** — Use this package to write Cloudflare Workers in F#. For managing Cloudflare resources from .NET applications, see [CloudflareFS.Management](https://www.nuget.org/packages/CloudflareFS.Management).

## What is this?

CloudflareFS.Runtime provides F# type bindings for the APIs available *inside* a running Cloudflare Worker. These bindings are designed for use with [Fable](https://fable.io/) to compile F# to JavaScript that runs on Cloudflare's edge network.

## Supported Services

| Service | Description |
|---------|-------------|
| **Worker.Context** | Core Worker types (Request, Response, Headers, ExecutionContext) |
| **KV** | Key-value storage |
| **R2** | Object storage (S3-compatible) |
| **D1** | SQLite database |
| **Durable Objects** | Stateful serverless compute |
| **Queues** | Message queues |
| **Vectorize** | Vector database for AI/ML |
| **Hyperdrive** | Database connection pooling |
| **AI** | Workers AI inference |

## Installation

```bash
dotnet add package CloudflareFS.Runtime
```

## Quick Example

```fsharp
open CloudFlare.Worker.Context
open CloudFlare.KV

let handler (request: Request) (env: obj) (ctx: ExecutionContext) =
    promise {
        // Access KV namespace from environment bindings
        let kv = env?MY_KV_NAMESPACE :?> KVNamespace

        // Read a value
        let! value = kv.get "my-key"

        // Return a response
        return Response.create($"Value: {value}")
    }
```

## Requirements

- [Fable](https://fable.io/) 4.x for F# to JavaScript compilation
- A Cloudflare Workers project (typically set up with Wrangler)

## Related Package

- **[CloudflareFS.Management](https://www.nuget.org/packages/CloudflareFS.Management)** — .NET clients for Cloudflare's REST Management APIs (create databases, deploy workers, manage buckets, etc.)

## Links

- [GitHub Repository](https://github.com/speakeztech/CloudflareFS)
- [Cloudflare Workers Documentation](https://developers.cloudflare.com/workers/)
- [Fable Documentation](https://fable.io/docs/)

## License

MIT OR Apache-2.0
