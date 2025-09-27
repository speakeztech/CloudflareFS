# Hawaii Code Generation for Cloudflare Management APIs

## Overview

This directory contains the setup for using Hawaii to generate F# client bindings from Cloudflare's OpenAPI specifications.

## What is Hawaii?

Hawaii is an F# code generator by Zaid Ajaj that creates type-safe, idiomatic F# client libraries from OpenAPI/Swagger specifications. It generates:
- Discriminated unions for API responses
- Type-safe route builders
- Async workflows for all operations
- Proper error handling

## Architecture Context

Hawaii is used ONLY for **Management APIs** (Layer 2):
- Creating/managing KV namespaces
- Creating/managing R2 buckets
- Creating/managing D1 databases
- Deploying Workers
- Managing DNS records

It is NOT used for **Runtime APIs** (Layer 1) which use Fable interop.

## Setup

```powershell
# Install Hawaii as global tool
dotnet tool install -g hawaii

# Download Cloudflare OpenAPI spec
Invoke-WebRequest -Uri "https://api.cloudflare.com/schemas/openapi.json" -OutFile cloudflare-openapi.json

# Generate F# client
hawaii cloudflare-openapi.json --output ../../src/Management/CloudFlare.Api/Generated.fs --namespace CloudFlare.Api
```

## Configuration

Create a `hawaii.json` configuration file for custom settings:

```json
{
  "namespace": "CloudFlare.Api",
  "synchronous": false,
  "target": "fsharp",
  "emptyDefinitions": "ignore",
  "overrideSchema": {
    "CloudflareApiResponse": {
      "generateClient": false
    }
  }
}
```

## Usage Example

```fsharp
open CloudFlare.Api

let createKVNamespace accountId name = async {
    let client = CloudflareClient(apiToken = "your-token")
    let! result =
        client.KV.Namespaces.Create(
            accountId,
            { title = name }
        )

    match result with
    | Ok namespace -> return namespace.id
    | Error err -> return failwith err.message
}
```

## Regenerating Bindings

When Cloudflare updates their API:

```powershell
.\generate-bindings.ps1
```

This will:
1. Download the latest OpenAPI spec
2. Run Hawaii with our configuration
3. Generate updated F# bindings

## Notes

- Hawaii generates functional, idiomatic F# code
- All operations return `Async<Result<'T, Error>>`
- Authentication is handled via API tokens
- Rate limiting is respected automatically