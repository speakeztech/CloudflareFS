# Key Files

## Configuration & Build
- `/CloudflareFS.sln` - Main solution file
- `/global.json` - .NET SDK version configuration
- `/package.json` - Node.js dependencies for workers-types

## Source Code

### Runtime APIs (In-Worker)
- `/src/Runtime/CloudFlare.Worker.Context/` - Core Worker types (Request, Response, Headers)
- `/src/Runtime/CloudFlare.D1/` - D1 database bindings
- `/src/Runtime/CloudFlare.R2/` - R2 object storage bindings
- `/src/Runtime/CloudFlare.KV/` - KV storage bindings
- `/src/Runtime/CloudFlare.AI/` - Workers AI bindings

### Management APIs (External)
- `/src/Management/CloudFlare.Management.Workers/` - Worker deployment and configuration
- `/src/Management/CloudFlare.Management.D1/` - D1 database management
- `/src/Management/CloudFlare.Management.R2/` - R2 bucket management
- `/src/Management/CloudFlare.Management.Analytics/` - Analytics API client

### Management API Structure (per service)
- `Types.fs` - Data types and records
- `Client.fs` - API client implementation
- `OpenApiHttp.fs` - HTTP handling utilities
- `StringEnum.fs` - String enumeration helpers

## Generation Scripts
- `/generators/hawaii/extract-services.fsx` - Segments 15.5MB OpenAPI into services
- `/generators/hawaii/*-hawaii.json` - Per-service Hawaii configurations
- `/generators/hawaii/post-process-discriminators.fsx` - DU generation post-processor
- `/generators/hawaii/generate-workers.ps1` - Full Workers generation script
- `/generators/glutinum/glutinum.json` - Glutinum configuration

## Documentation
- `/docs/00_architecture_decisions.md` - Key design choices and roadmap
- `/docs/01_generation_strategy.md` - Glutinum vs Hawaii strategy
- `/docs/02_dual_layer_architecture.md` - Runtime vs Management APIs
- `/docs/03_openapi_generation.md` - Hawaii setup and post-processing
- `/docs/08_conversion_patterns.md` - TypeScript to F# patterns and fixes
- `/docs/09_tool_improvement_analysis.md` - Comprehensive tool improvement roadmap
- `/PROJECT_STATUS.md` - Current implementation status
- `/CONTRIBUTING.md` - Contribution guidelines

## Samples
- `/samples/HelloWorker/` - Basic Worker example
  - `Main.fs` - Entry point
  - `wrangler.toml` - Wrangler configuration
- `/samples/SecureChat/` - Production chat API
  - `Main.fs` - API implementation
  - `/scripts/add-user.ps1` - User management
- `/samples/SecureChat.UI/` - React frontend

## External Dependencies (Not in Repo)
- `D:/repos/Cloudflare/api-schemas/openapi.json` - Full Cloudflare OpenAPI spec (15.5MB)
- `node_modules/@cloudflare/workers-types/` - TypeScript definitions
