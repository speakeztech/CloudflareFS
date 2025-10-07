# CloudflareFS Generation Strategy

## Overview

CloudflareFS uses a dual-generation approach: **Glutinum** for TypeScript-based Runtime APIs (in-Worker) and **Hawaii** for OpenAPI-based Management APIs (external). This strategy ensures type-safe F# bindings across the entire Cloudflare platform.

## Generation Decision Matrix

| Service | TypeScript SDK | OpenAPI | Generation Tool | Layer | Rationale |
|---------|---------------|---------|-----------------|-------|-----------|
| **Workers Runtime** | `@cloudflare/workers-types` | ❌ | **Glutinum** | Runtime | In-Worker APIs, JS interop |
| **KV Runtime** | `@cloudflare/workers-types` | ❌ | **Glutinum** | Runtime | In-Worker storage access |
| **R2 Runtime** | `@cloudflare/workers-types` | ❌ | **Glutinum** | Runtime | In-Worker object storage |
| **D1 Runtime** | `@cloudflare/workers-types` | ❌ | **Glutinum** | Runtime | In-Worker database |
| **AI Runtime** | `@cloudflare/ai` | ✅ | **Glutinum** | Runtime | Has TypeScript SDK |
| **KV Management** | ❌ | ✅ | **Hawaii** | Management | REST API only |
| **R2 Management** | ❌ | ✅ | **Hawaii** | Management | REST API only |
| **D1 Management** | ❌ | ✅ | **Hawaii** | Management | REST API only |
| **Workers Management** | ❌ | ✅ | **Hawaii** | Management | Deployment APIs |
| **DNS** | ❌ | ✅ | **Hawaii** | Management | Zone management |
| **Analytics** | ❌ | ✅ | **Hawaii** | Management | Metrics and logs |

## Automated Pipeline Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    SOURCE CONTROL (Pristine)                │
├─────────────────────────────────────────────────────────────┤
│ D:/repos/Cloudflare/api-schemas/  (NEVER MODIFIED)          │
│   ├── openapi.json (15.5MB - full spec)                     │
│   ├── openapi.yaml                                          │
│   └── common.yaml                                           │
│                                                             │
│ node_modules/ (via npm)                                     │
│   ├── @cloudflare/workers-types/                            │
│   └── @cloudflare/ai/                                       │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    GENERATION PIPELINE                      │
├─────────────────────────────────────────────────────────────┤
│ generators/                                                 │
│   ├── update-and-generate.ps1  (Master Script)              │
│   │                                                         │
│   ├── glutinum/                (TypeScript → F#)            │
│   │   ├── glutinum.json        (Configuration)              │
│   │   └── generate-runtime.ps1                              │
│   │                                                         │
│   └── hawaii/                  (OpenAPI → F#)               │
│       ├── extract-services.fsx (Splits 15.5MB spec)         │
│       ├── *-hawaii.json        (Service configs)            │
│       └── generate-management.ps1                           │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    GENERATED OUTPUT                         │
├─────────────────────────────────────────────────────────────┤
│ src/                                                        │
│   ├── Runtime/     (Glutinum-generated, Fable-compatible)   │
│   │   ├── CloudFlare.Worker.Context/                        │
│   │   ├── CloudFlare.KV/                                    │
│   │   ├── CloudFlare.R2/                                    │
│   │   ├── CloudFlare.D1/                                    │
│   │   └── CloudFlare.AI/                                    │
│   │                                                         │
│   └── Management/  (Hawaii-generated, multi-target)         │
│       ├── CloudFlare.Management.KV/                         │
│       ├── CloudFlare.Management.R2/                         │
│       ├── CloudFlare.Management.D1/                         │
│       └── CloudFlare.Management.Analytics/                  │
└─────────────────────────────────────────────────────────────┘
```

## Generation Tools

### Glutinum (Runtime Layer)

**Purpose**: Generate F# bindings from TypeScript definitions for in-Worker APIs.

**Configuration** (`generators/glutinum/glutinum.json`):
```json
{
  "input": "./node_modules/@cloudflare/workers-types/index.d.ts",
  "output": "./src/Runtime/CloudFlare.Worker.Context/Generated.fs",
  "namespace": "CloudFlare.Worker.Context",
  "generateStaticMembers": true
}
```

**Execution**:
```bash
npx @glutinum/cli generate --config generators/glutinum/glutinum.json
```

### Hawaii (Management Layer)

**Purpose**: Generate F# clients from OpenAPI specifications for management APIs.

**Two-Step Process**:

1. **Extract Service-Specific Specs** (handles 15.5MB problem):
```fsharp
// extract-services.fsx
let services = [
    { Name = "D1"
      PathPatterns = ["/accounts/{account_id}/d1/database"]
      OperationPrefix = "d1" }
    // ... other services
]
// Creates focused specs: d1-openapi.json (217KB), etc.
```

2. **Generate F# Clients**:
```bash
hawaii --config generators/hawaii/d1-hawaii.json
# Output: src/Management/CloudFlare.Management.D1/
```

## Key Implementation Decisions

### 1. Source Schema Preservation
- **Never modify** source OpenAPI or TypeScript definitions
- Keep pristine copies in separate repos
- All transformations happen in generation pipeline

### 2. OpenAPI Segmentation Strategy
- Full spec (15.5MB) too large for tools
- Extract service-specific subsets (45-250KB each)
- Preserve all schema dependencies and references
- Each service gets independent generation

### 3. Namespace Organization
- Runtime: `CloudFlare.{Service}` (e.g., CloudFlare.D1)
- Management: `CloudFlare.Management.{Service}`
- Clear separation prevents confusion

### 4. F# Portability (Management APIs)
- Use `async { }` not `Task<T>` for Fable/Fidelity compatibility
- Return `Async<Result<'T, 'Error>>` for functional error handling
- No System.Net.Http or .NET-specific dependencies
- Use `FSharp.SystemTextJson` for serialization (not `Newtonsoft.Json`)

### 5. Post-Processing Pipeline
- **Discriminated Union Generation**: `post-process-discriminators.fsx` automatically creates DU types for binding variants
- **System.Text.Json Migration**: All generated clients use `FSharp.SystemTextJson` for Fable compatibility
- **Namespace Correction**: Automated fixes for any namespace mismatches during generation

**Example Post-Processing**:
```bash
# After Hawaii generation
dotnet fsi post-process-discriminators.fsx ../../src/Management/CloudFlare.Management.Workers/Types.fs
# Output: Creates workersbindingitem DU with 29 cases
```

**Generated Output**:
```fsharp
// Auto-generated discriminated union
type workersbindingitem =
    | Assets of workersbindingkindassets
    | D1 of workersbindingkindd1
    | R2bucket of workersbindingkindr2bucket
    // ... 26 more cases
```

## Common Issues and Solutions

### Issue: Type Selection for Data Structures (Glutinum)
**Problem**: Glutinum generates F# interfaces for TypeScript data structures, forcing object expression syntax
**Current Workaround**: Manual conversion to proper type definitions or object expressions
**Long-Term Solution**: Glutinum should analyze TypeScript patterns and generate F# records for pure data structures
**Reference**: See `09_tool_improvement_analysis.md` for comprehensive strategy

### Issue: Reserved Keyword Handling (Both Tools)
**Problem**: F# reserved keywords used without proper mapping, requiring backtick escaping
**Current Workaround**: Manual backtick addition post-generation
**Long-Term Solution**: Attribute-based semantic mapping (`CompiledName` for Glutinum, `JsonPropertyName` for Hawaii)
**Reference**: See `08_conversion_patterns.md` and `09_tool_improvement_analysis.md`

### Issue: Hawaii Discriminated Unions
**Problem**: Hawaii doesn't natively support OpenAPI discriminator schemas
**Solution**: Post-processing script generates F# discriminated unions from binding type patterns
**Status**: ✅ Solved with automated post-processing
**Example**: Workers binding types (29 variants) now fully typed via `post-process-discriminators.fsx`

### Issue: Hawaii Null Reference Exceptions
**Problem**: Complex nested schemas cause Hawaii crashes (currently only KV Management API)
**Current Status**: Under investigation for KV schemas
**Workaround**: Manual implementation or schema simplification may be required
**Completed**: Workers Management API now fully functional with post-processing pipeline

### Issue: Duplicate Type Definitions
**Problem**: Same types defined in multiple services
**Solution**: Extract common types to shared project, reference from services

### Issue: API Deprecation
**Problem**: OpenAPI contains deprecated endpoints (e.g., Vectorize V1)
**Solution**: Always verify API version status; Hawaii correctly skips deprecated operations
**Example**: Vectorize V2 migration (August 2024) - updated extraction patterns from `/vectorize/indexes` to `/vectorize/v2/indexes`

## Validation Checklist

After generation, verify:

- [ ] Generated files compile without errors
- [ ] Namespaces match project structure
- [ ] No duplicate type definitions
- [ ] Management APIs use `Async<Result<'T, 'Error>>`
- [ ] Runtime APIs use Fable-compatible types
- [ ] All expected endpoints are present
- [ ] No deprecated APIs included

## Future Enhancements

1. **Incremental Generation**: Detect changes and regenerate only affected services
2. **Type Deduplication**: Automatic extraction of common types
3. **Custom Transformations**: Hook system for post-generation fixes
4. **CI Integration**: GitHub Actions for automatic regeneration on schema updates