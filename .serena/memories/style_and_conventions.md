# Style and Conventions

## F# Code Style
- Follow the official [F# Style Guide](https://docs.microsoft.com/en-us/dotnet/fsharp/style-guide/)
- Use **Fantomas** for consistent formatting: `dotnet fantomas . --recurse`
- Standard F# naming: PascalCase for types/modules, camelCase for values/functions
- Functional-first approach with immutable data structures
- Heavy use of discriminated unions and pattern matching
- Type annotations for public APIs

## Namespace Conventions
- Runtime APIs: `CloudFlare.{Service}` (e.g., `CloudFlare.D1`)
- Management APIs: `CloudFlare.Management.{Service}` (e.g., `CloudFlare.Management.D1`)
- Core/shared: `CloudFlare.Core`

## Generated Code Conventions

### For Runtime APIs (Glutinum-generated)
- Use `[<CompiledName>]` for reserved keyword properties (preferred)
- Fallback to backtick escaping when necessary: `abstract member ``namespace``: string`
- Place global values in `Globals` module, not at namespace level

### For Management APIs (Hawaii-generated)
- Use `[<JsonPropertyName>]` for reserved keyword properties
- Use `FSharp.SystemTextJson` for serialization
- Use `Async<Result<'T, 'Error>>` return types

## Pattern Matching with Special Characters
When discriminated union cases have special characters:

```fsharp
// Backtick escaping for special characters
match this with
| ``@cfBaaiBgeSmallEnV1Numeric_5`` -> "@cf/baai/bge-small-en-v1.5"

// Preferred (with CompiledName attribute)
type VectorizePreset =
    | [<CompiledName "@cf/baai/bge-small-en-v1.5">] CfBaaiBgeSmallEnV15
```

## Documentation Standards
- Add XML documentation to all public APIs
- Use triple-slash comments for IntelliSense

```fsharp
/// <summary>
/// Creates a new KV namespace with the specified name.
/// </summary>
/// <param name="accountId">The Cloudflare account ID</param>
/// <param name="name">The namespace name</param>
/// <returns>The created namespace</returns>
let createNamespace (accountId: string) (name: string) = ...
```

## Commit Messages
Follow conventional commits format:
```
feat: Add Durable Objects runtime bindings
fix: Correct KV expiration handling
docs: Update Management API examples
chore: Upgrade Fable to 4.x
test: Add R2 multipart upload tests
```

## Critical Anti-Patterns to AVOID

### 1. .NET-Specific Types in Management APIs
```fsharp
// WRONG - won't compile with Fable/Fidelity
member this.CreateDatabase(...) -> Task<T>

// CORRECT - portable
member this.CreateDatabase(...) -> Async<Result<T, Error>>
```

### 2. Reflexive Mutability
```fsharp
// WRONG - unnecessary mutation for data structures
let mutable _value = None
{ new Options with member _.value with get() = _value and set(v) = _value <- v }

// CORRECT - immutable record
type Options = { value: string option }
let opts = { value = Some "value" }
```

### 3. Object Expressions for Pure Data
Glutinum sometimes generates interfaces for data structures. Prefer records:
```fsharp
// Generated (awkward)
type WorkerOptions =
    abstract member timeout: int with get, set

// Better (manual fix or tool improvement)
type WorkerOptions = { timeout: int }
```

### 4. Newtonsoft.Json in Management APIs
All Management APIs use `FSharp.SystemTextJson` for Fable compatibility.
