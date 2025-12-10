# CloudflareFS.Management

**F# clients for Cloudflare Management REST APIs**

> **Management-side clients** — Use this package to manage Cloudflare resources from .NET applications. For writing Cloudflare Workers in F#, see [CloudflareFS.Runtime](https://www.nuget.org/packages/CloudflareFS.Runtime).

## What is this?

CloudflareFS.Management provides F# client libraries for Cloudflare's REST Management APIs. Use these to programmatically create, configure, and manage Cloudflare resources from any .NET application — deployment scripts, admin tools, CI/CD pipelines, or backend services.

## Supported Services

| Service | Description |
|---------|-------------|
| **Workers** | Deploy and configure Workers scripts |
| **KV** | Create and manage KV namespaces |
| **R2** | Create and manage R2 buckets |
| **D1** | Create and manage D1 databases |
| **Durable Objects** | Manage Durable Object namespaces |
| **Queues** | Create and manage message queues |
| **Vectorize** | Create and manage vector indexes |
| **Hyperdrive** | Configure database connection pooling |
| **Pages** | Manage Cloudflare Pages projects |
| **Analytics** | Query analytics data |

## Installation

```bash
dotnet add package CloudflareFS.Management
```

## Quick Example

```fsharp
open System.Net.Http
open CloudFlare.Management.D1

let httpClient = new HttpClient()
httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}")

let d1Client = D1Client(httpClient)

// List all D1 databases in an account
let! databases = d1Client.ListDatabases(accountId)

for db in databases do
    printfn $"Database: {db.Name} (ID: {db.Id})"
```

## Authentication

All Management API calls require authentication. Use either:

- **API Token** (recommended): Create a token at [dash.cloudflare.com/profile/api-tokens](https://dash.cloudflare.com/profile/api-tokens)
- **API Key**: Your Global API Key + email (legacy method)

```fsharp
// API Token (recommended)
httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}")

// Or API Key (legacy)
httpClient.DefaultRequestHeaders.Add("X-Auth-Key", apiKey)
httpClient.DefaultRequestHeaders.Add("X-Auth-Email", email)
```

## Requirements

- .NET Standard 2.0 compatible runtime (.NET 6+, .NET Framework 4.6.1+)
- `System.Net.Http` for HTTP requests
- `FSharp.SystemTextJson` (included as dependency)

## Related Package

- **[CloudflareFS.Runtime](https://www.nuget.org/packages/CloudflareFS.Runtime)** — F# and Fable bindings for writing Cloudflare Workers (in-worker APIs for KV, R2, D1, etc.)

## Links

- [GitHub Repository](https://github.com/speakeztech/CloudflareFS)
- [Cloudflare API Documentation](https://developers.cloudflare.com/api/)

## License

MIT OR Apache-2.0
