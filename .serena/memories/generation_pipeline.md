# Generation Pipeline

## Overview

CloudflareFS uses a dual-generation approach:
- **Glutinum**: TypeScript to F# for Runtime APIs
- **Hawaii**: OpenAPI to F# for Management APIs

## The TypeScript to JavaScript Round-Trip (Runtime)

```
TypeScript (.d.ts) → Glutinum → F# Bindings → Fable → JavaScript (Worker)
```

Glutinum-generated code must satisfy THREE compilers:
1. **F# compiler** - Syntactically valid F#
2. **Fable compiler** - Fable-compatible patterns
3. **JavaScript runtime** - Correct JS property names

**Key Insight**: Properties like `namespace` must become `Namespace` in F# but still produce `namespace` in JavaScript output.

## The OpenAPI to HTTP Round-Trip (Management)

```
OpenAPI Schema → Hawaii → F# Client → Any Compiler → HTTP Requests (JSON)
```

Hawaii-generated code has a simpler contract:
1. **F# compiler** - Valid F#
2. **JSON serialization** - Property names match OpenAPI schema
3. **Multiple targets** - Works with Fable, Fidelity, and .NET

## Glutinum Pipeline (Runtime Layer)

### Configuration
```json
// generators/glutinum/glutinum.json
{
  "input": "./node_modules/@cloudflare/workers-types/index.d.ts",
  "output": "./src/Runtime/CloudFlare.Worker.Context/Generated.fs",
  "namespace": "CloudFlare.Worker.Context",
  "generateStaticMembers": true
}
```

### Common Issues & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| Reserved keywords | `namespace`, `type`, etc. | Use `[<CompiledName>]` or backticks |
| Object expression errors (FS3168) | `member val` in object expressions | Convert to getter/setter with backing fields |
| Global values in namespaces (FS0201) | Global values not in module | Wrap in `Globals` module |
| Forward references | Types used before declaration | Reorder type declarations |

## Hawaii Pipeline (Management Layer)

### Two-Step Process

**Step 1: Extract Service-Specific Specs**

The full Cloudflare OpenAPI spec is 15.5MB (300,000+ lines). We segment it:

```fsharp
// generators/hawaii/extract-services.fsx
let services = [
    { Name = "D1"
      PathPatterns = ["/accounts/{account_id}/d1/database"]
      OperationPrefix = "d1" }
    { Name = "Vectorize"
      PathPatterns = ["/accounts/{account_id}/vectorize/v2/indexes"]  // Note: V2!
      OperationPrefix = "vectorize" }
]
```

**Step 2: Generate F# Clients**

```json
// generators/hawaii/d1-hawaii.json
{
  "namespace": "CloudFlare.Management.D1",
  "synchronous": false,
  "target": "fsharp",
  "generateClient": true,
  "clientName": "D1ManagementClient"
}
```

### Post-Processing Pipeline

Hawaii needs post-processing for:
1. **Discriminated Unions**: OpenAPI discriminator schemas
2. **System.Text.Json**: Migration from Newtonsoft.Json

**Post-Processor Scripts**:
- `post-process-discriminators.fsx` - Generates F# DUs from binding types
- `post-process-jobject.fsx` - Fixes JObject multipart form data

**Example Output**:
```fsharp
// Auto-generated discriminated union
type workersbindingitem =
    | Assets of workersbindingkindassets
    | D1 of workersbindingkindd1
    | R2bucket of workersbindingkindr2bucket
    // ... 29 cases total
```

## Critical: API Version Handling

**Case Study: Vectorize API**

Vectorize V1 was deprecated in August 2024. Hawaii correctly skips deprecated operations, resulting in empty clients if pointing to V1 paths.

**Solution**: Always use current API version in extraction patterns:
```fsharp
// WRONG (deprecated)
PathPatterns = ["/accounts/{account_id}/vectorize/indexes"]

// CORRECT (current V2)
PathPatterns = ["/accounts/{account_id}/vectorize/v2/indexes"]
```

## Validation Checklist

After generation, verify:
- [ ] Generated files compile without errors
- [ ] Namespaces match project structure
- [ ] No duplicate type definitions
- [ ] Management APIs use `Async<Result<'T, 'Error>>`
- [ ] Runtime APIs use Fable-compatible types
- [ ] All expected endpoints are present
- [ ] No deprecated APIs included

## Generation Status

### Completed
- Workers Management API (with post-processing)
- D1, R2, Analytics, Queues, Vectorize (V2), Hyperdrive, DurableObjects Management APIs
- All Runtime APIs (KV, R2, D1, AI, Queues, Vectorize, Hyperdrive, DurableObjects)

### In Progress
- KV Management API (Hawaii complex schema issues)
- Logs Management API (extraction patterns pending)
