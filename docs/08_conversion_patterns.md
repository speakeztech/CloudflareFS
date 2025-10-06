# Conversion Tool Error Patterns

This document captures recurring error patterns from TypeScript-to-F# conversion tools (Glutinum v0.12.0, Hawaii v0.66.0) and documents both current workarounds and recommended long-term solutions.

**Note**: This document reflects the current state of generated code. For comprehensive tool improvement recommendations and the strategic vision for eliminating these patterns, see `09_tool_improvement_analysis.md`.

## Common Error Patterns

### 1. Reserved Keyword Handling
**Issue**: F# reserved keywords used as identifiers without proper mapping

**Current Workaround**:
```fsharp
// Manual fix: Backtick escaping
abstract member ``namespace``: string option with get, set
```

**Long-Term Solution** (see `09_tool_improvement_analysis.md`):
```fsharp
// Semantic property mapping with attribute-based renaming
[<CompiledName("namespace")>]  // For Glutinum/Fable (JavaScript interop)
abstract member Namespace: string option with get, set

// OR

[<JsonPropertyName("namespace")>]  // For Hawaii (JSON serialization)
Namespace: string option
```

**Why This Matters**: Backtick escaping forces users to write awkward code (`storage.``namespace```) throughout their applications. Attribute-based mapping keeps F# code clean while preserving the original property names in JavaScript/JSON output.

**Affected Keywords**: `namespace`, `end`, `type`, `module`, `function`, `match`, `with`

### 2. Type Selection for Data Structures
**Issue**: Glutinum generates F# interfaces for TypeScript interfaces, even when they represent pure data structures, forcing awkward object expression syntax

**Current Workaround**:
```fsharp
// Manual fix: Object expression with explicit implementation
let mutable _start = None
{ new DurableObjectListOptions with
    member _.start with get() = _start and set(v) = _start <- v }
```

**Long-Term Solution** (see `09_tool_improvement_analysis.md`):
```fsharp
// Glutinum should detect data structures and generate F# records
type DurableObjectListOptions = {
    start: string option
}

// Clean, immutable usage
let options = { start = Some "cursor-123" }
```

**Why This Matters**: Object expressions with mutable backing fields introduce unnecessary state mutation. TypeScript `interface` declarations for data should map to F# records (immutable by default), not F# interfaces. The JavaScript output is identical either way, so the choice should favor idiomatic F# code.

### 3. Global Values in Namespaces
**Issue**: Placing global values directly in namespaces
```fsharp
// WRONG - FS0201: Namespaces cannot contain values
namespace CloudFlare.Worker.Context
[<Global>]
let Headers: HeadersConstructor = jsNative

// CORRECT - use a module
namespace CloudFlare.Worker.Context
module Globals =
    [<Global>]
    let Headers: HeadersConstructor = jsNative
```

### 4. Forward Reference Issues
**Issue**: Types referenced before declaration
```fsharp
// WRONG - type used before definition
type DurableObjectStorage =
    abstract member list: ?options: DurableObjectListOptions -> ...
// ... later ...
type DurableObjectListOptions = ...

// CORRECT - declare types in dependency order
type DurableObjectListOptions = ...
type DurableObjectStorage =
    abstract member list: ?options: DurableObjectListOptions -> ...
```

### 5. Pattern Matching with Special Characters
**Issue**: Discriminated union cases with special characters (`@`, `-`, `.`) generate invalid identifiers

**Current Workaround**:
```fsharp
// Manual fix: Backtick escaping
match this with
| ``@cfBaaiBgeSmallEnV1Numeric_5`` -> "@cf/baai/bge-small-en-v1.5"
```

**Long-Term Solution** (see `09_tool_improvement_analysis.md`):
```fsharp
// Hawaii should generate semantic names with CompiledName attribute
type VectorizePreset =
    | [<CompiledName "@cf/baai/bge-small-en-v1.5">] CfBaaiBgeSmallEnV15

// Clean pattern matching
match this with
| CfBaaiBgeSmallEnV15 -> "@cf/baai/bge-small-en-v1.5"
```

**Why This Matters**: Backticks in pattern matching reduce code readability. Semantic naming with `CompiledName` attributes provides clean F# code while preserving the original JSON values for serialization.

### 6. Promise/Async Handling
**Issue**: Incorrect Promise construction in Fable
```fsharp
// WRONG - promise computation expression not always available
promise { return () }

// CORRECT - use Fable's Promise API
JS.Constructors.Promise.Create(fun resolve _ -> resolve())
```

### 7. Static vs Instance Methods
**Issue**: Calling instance methods as static
```fsharp
// WRONG
Response.json({| data = value |})

// CORRECT - Response is a global constructor
Response.json({| data = value |}, null)
```

### 8. Discriminated Union Constructor Confusion
**Issue**: DU case names conflicting with system types
```fsharp
// WRONG - Object resolves to System.Object constructor
Some (Object metadata)

// CORRECT - Fully qualify the DU case
Some (VectorizeVectorMetadata.Object metadata)
```

### 9. Missing Async/Promise Helpers
**Issue**: Async.AwaitPromise not available in all contexts
```fsharp
// Add helper at module level
let inline promiseToAsync (p: JS.Promise<'T>) : Async<'T> =
    Async.AwaitPromise p

// Then use consistently
index.query(vector, options) |> promiseToAsync
```

## Summary: Current Workarounds vs Long-Term Vision

| Issue | Current Manual Fix | Recommended Tool Enhancement |
|-------|-------------------|------------------------------|
| Reserved keywords | Backtick escaping | Attribute-based mapping (`CompiledName`/`JsonPropertyName`) |
| Data structures | Object expressions with mutables | Smart type selection (records vs interfaces) |
| DU special characters | Backtick escaping | Semantic naming with `CompiledName` |
| Global values | Manual module wrapping | Auto-wrap globals in modules |
| Type ordering | Manual reordering | Dependency graph analysis |

**Strategic Direction**: The long-term goal is "fire and forget" generation where tools produce idiomatic, compilable F# code without manual intervention. See `09_tool_improvement_analysis.md` for the complete roadmap toward this goal.

## Recommended Tool Enhancements

### Priority 1: Semantic Mapping (Glutinum & Hawaii)
- Implement attribute-based property renaming for reserved keywords
- Use `CompiledName` for Glutinum (JavaScript interop)
- Use `JsonPropertyName` for Hawaii (JSON serialization)
- Generate clean F# identifiers that map to original names

### Priority 2: Smart Type Selection (Glutinum)
- Analyze TypeScript interfaces to distinguish data vs behavior
- Generate F# records for pure data structures
- Generate F# interfaces only for behavioral contracts
- Default to immutability; add `mutable` only when mutation is detected

### Priority 3: Improved Code Organization (Both)
- Auto-wrap global values in modules
- Implement dependency graph analysis for type ordering
- Handle circular references with `and` keyword

For detailed implementation strategies and rationale, see `09_tool_improvement_analysis.md`.

## Testing Strategy

After conversion, always:
1. Run `dotnet build` to catch compilation errors
2. Check for forward reference issues
3. Verify all interfaces are properly implemented
4. Ensure reserved keywords are escaped
5. Validate Promise/async patterns compile with Fable

## Tool-Specific Notes

### Glutinum
- Tends to generate `member val` in object expressions
- May not escape all F# reserved keywords
- Sometimes places globals in wrong scope

### Hawaii
- Similar issues with reserved keywords
- May generate incorrect pattern matching syntax
- Type ordering issues in complex hierarchies