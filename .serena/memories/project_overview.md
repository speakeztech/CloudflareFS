# CloudflareFS Project Overview

## Purpose
CloudflareFS brings F# to the Cloudflare platform through comprehensive, type-safe bindings auto-generated from TypeScript definitions and OpenAPI specifications. The project provides a complete F# toolkit for Cloudflare's ecosystem.

**Primary Goal**: Deliver production-ready F# libraries for Cloudflare's entire platform through a dual-layer architecture covering both in-Worker runtime operations and external management APIs.

## The "Code-First" Mission
CloudflareFS aims to make F# `.fsx` configuration of Cloudflare solutions a first-class consideration:
- **Infrastructure as Code**: All infrastructure defined in F#, not TOML/YAML
- **Type Safety**: Full F# typing across all Cloudflare services
- **Portability**: Management APIs compilable via Fable, Fidelity, or .NET

## Tech Stack
- **Language**: F# (.NET 8.0+)
- **Build System**: dotnet CLI / MSBuild
- **Core Dependencies**:
  - Fable (F# to JavaScript compilation)
  - FSharp.SystemTextJson (JSON serialization)
- **Generation Tools**:
  - Glutinum CLI v0.12.0 (TypeScript to F# bindings)
  - Hawaii v0.66.0 (OpenAPI to F# clients)
- **Target Platform**: Cloudflare Workers, Pages, and Management APIs

## Dual-Layer Architecture
```
CloudflareFS/
└── src/
   ├── Runtime/           # In-Worker APIs (JavaScript interop via Glutinum)
   │   ├── CloudFlare.Worker.Context/
   │   ├── CloudFlare.D1/
   │   ├── CloudFlare.R2/
   │   ├── CloudFlare.KV/
   │   ├── CloudFlare.AI/
   │   └── ...
   │
   └── Management/        # REST APIs (HTTP clients via Hawaii)
       ├── CloudFlare.Management.Workers/
       ├── CloudFlare.Management.D1/
       ├── CloudFlare.Management.R2/
       ├── CloudFlare.Management.Analytics/
       └── ...
```

### Runtime Layer (In-Worker)
- **Purpose**: Operations inside Cloudflare Workers (V8 isolate)
- **Source**: TypeScript definitions from `@cloudflare/workers-types`
- **Generation**: Glutinum (TypeScript to F#)
- **Usage**: Direct platform access with microsecond latency

### Management Layer (External)
- **Purpose**: Infrastructure provisioning, monitoring, management
- **Source**: Cloudflare OpenAPI specifications (15.5MB, segmented)
- **Generation**: Hawaii (OpenAPI to F#)
- **Usage**: REST API clients for external tools and deployment scripts

## External Dependencies
- **Cloudflare API Schemas**: OpenAPI specification at `D:/repos/Cloudflare/api-schemas/`
- **Cloudflare Workers Types**: `@cloudflare/workers-types` via npm
