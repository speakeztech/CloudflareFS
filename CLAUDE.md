# Claude Assistant Instructions for CloudflareFS

## Project Overview

CloudflareFS is a comprehensive F# binding library for the Cloudflare platform with a **dual-layer architecture**:
- **Runtime Layer**: In-Worker JavaScript interop via Fable/Glutinum (for code running inside Workers)
- **Management Layer**: REST API clients via Hawaii (for external infrastructure management)

## Critical Project Philosophy

**This is essentially a compiler for Cloudflare. Placeholders are no good.**

- NEVER create placeholder or stub implementations
- NEVER delete existing generated code without explicit permission
- ALL code must be functional and complete
- If something can't be implemented properly, investigate and fix the root cause

## Architecture Understanding

### Two Distinct Layers

1. **Runtime APIs** (src/Runtime/*)
   - Run INSIDE Cloudflare Workers
   - Generated from TypeScript definitions using Glutinum
   - Use Fable for F# to JavaScript compilation
   - Direct platform bindings with microsecond latency
   - Examples: CloudFlare.Worker.Context, CloudFlare.D1, CloudFlare.R2

2. **Management APIs** (src/Management/*)
   - Run OUTSIDE Workers (on your machine, CI/CD, etc.)
   - Generated from OpenAPI specs using Hawaii
   - Standard HTTP REST clients
   - Used for provisioning and configuration
   - Examples: CloudFlare.Management.D1, CloudFlare.Management.R2

### Key Tools and Their Purposes

- **Fable**: F# to JavaScript compiler (for Runtime layer only)
- **Glutinum**: TypeScript to F# binding generator (for Runtime layer)
- **Hawaii**: OpenAPI to F# client generator (for Management layer)
- **Cloudflare Workers**: The edge computing platform we're targeting

## Code Generation Guidelines

### When Working with Glutinum (Runtime Layer)

1. **Source**: TypeScript definitions from @cloudflare/workers-types
2. **Output**: F# bindings in src/Runtime/*/Generated.fs files
3. **NEVER manually edit Generated.fs files** - fix issues in the source or generation process
4. **Common issues**:
   - Object expression syntax (cannot use `member val` - use mutable backing fields)
   - Duplicate type definitions (remove duplicates, keep the most complete one)

### When Working with Hawaii (Management Layer)

1. **Source**: Cloudflare OpenAPI specifications
2. **Critical Issue**: The main OpenAPI spec is 15.5MB - too large for direct processing
3. **Solution**: Extract service-specific specs first, then generate
4. **Location of specs**: D:/repos/Cloudflare/api-schemas/openapi.json

#### Vectorize V2 Migration Lesson

**ALWAYS check for deprecated APIs:**
- Vectorize V1 was deprecated in August 2024
- OpenAPI spec contained only deprecated V1 endpoints
- Hawaii correctly skips deprecated operations (this is good!)
- Solution: Update extraction to use V2 paths (`/vectorize/v2/indexes`)

### Service Extraction Pattern

```fsharp
// In extract-services.fsx
{ Name = "ServiceName"
  PathPatterns = [
      "/accounts/{account_id}/service/v2/endpoints"  // Use current API version!
      // ... other endpoints
  ]
  OperationPrefix = "service"
  Description = "Service Description" }
```

## Common Pitfalls and Solutions

### 1. Pattern Matching with @ Symbols

**Problem**: F# doesn't allow @ in pattern matching
```fsharp
// ERROR:
| (@cfBaaiBgeSmallEnV1Numeric_5) -> "@cf/baai/bge-small-en-v1.5"
```

**Solution**: Use backtick escaping
```fsharp
// CORRECT:
| ``@cfBaaiBgeSmallEnV1Numeric_5`` -> "@cf/baai/bge-small-en-v1.5"
```

### 2. Empty Generated Clients

**Symptom**: Hawaii runs successfully but generates empty client (no methods)

**Check**:
1. Are the API endpoints deprecated in the OpenAPI spec?
2. Do the extraction patterns match current API paths?
3. Check Cloudflare's changelog for API version updates

### 3. Object Expression Errors

**Problem**: Cannot use `member val` in object expressions
```fsharp
// ERROR:
{ new IFoo with
    member val Bar = "baz" with get, set }
```

**Solution**: Use explicit implementation
```fsharp
// CORRECT:
{ new IFoo with
    member this.Bar
        with get() = bar
        and set(value) = bar <- value }
```

## Development Workflow

### When Fixing Compilation Errors

1. **Read the error carefully** - F# errors are usually precise
2. **Check if it's generated code** - fix the generator, not the output
3. **For Runtime APIs**: Check if Glutinum generated it correctly
4. **For Management APIs**: Check if Hawaii extraction is using current API version
5. **Run from solution level**: `dotnet build` at the root to catch all errors

### When Adding New Services

1. **Determine the layer**:
   - Does it run inside a Worker? → Runtime layer (Glutinum)
   - Is it for management/configuration? → Management layer (Hawaii)

2. **For Runtime services**:
   - Find TypeScript definitions
   - Configure Glutinum
   - Generate and test

3. **For Management services**:
   - Extract service-specific OpenAPI
   - Verify no deprecated endpoints
   - Configure Hawaii
   - Generate and validate output has expected methods

## Testing and Validation

### Always Validate Generated Code

1. **Check for empty outputs** - A sign of deprecated APIs or wrong patterns
2. **Compile immediately** - Don't wait to find syntax errors
3. **Look for complete implementations** - No placeholders, no TODOs
4. **Test basic operations** - Ensure the generated client actually works

### Compilation Commands

```bash
# Build everything
dotnet build

# Build specific layer
dotnet build src/Runtime
dotnet build src/Management

# Run specific sample
cd samples/HelloWorker
dotnet fable . --outDir dist
npx wrangler dev
```

## Project File Locations

### Key Configuration Files
- `generators/hawaii/extract-services.fsx` - Service extraction configuration
- `generators/hawaii/*-hawaii.json` - Hawaii generation configs
- `generators/glutinum/` - Glutinum configuration

### Source Repositories
- OpenAPI Specs: `D:/repos/Cloudflare/api-schemas/`
- Hawaii Source: `D:/repos/Hawaii/`
- CloudflareFS: `D:/repos/CloudflareFS/`

## Communication Style

When working on CloudflareFS:

1. **Be precise about the layer** you're working on (Runtime vs Management)
2. **Never create placeholders** - investigate and fix root causes
3. **Preserve existing code** unless explicitly told to modify it
4. **Check for API deprecations** when things don't generate as expected
5. **Compile frequently** to catch issues early
6. **Document lessons learned** in the appropriate markdown files

## Red Flags to Watch For

- 🚨 Empty generated clients (check for deprecated APIs)
- 🚨 Deleting large amounts of code (probably a mistake)
- 🚨 Creating "placeholder" or "TODO" implementations
- 🚨 Compilation errors in generated code (fix the generator)
- 🚨 Using V1 APIs when V2 exists (always use latest stable version)

## Success Metrics

- ✅ Zero compilation errors at solution level
- ✅ All generated clients have expected methods
- ✅ No placeholder implementations
- ✅ Clear separation between Runtime and Management layers
- ✅ Generated code follows F# idioms and conventions

## Remember

> "This is essentially a compiler for Cloudflare. Placeholders are no good."

Every piece of code should be production-ready. If something can't be implemented properly, investigate why and fix the root cause rather than working around it with stubs or placeholders.