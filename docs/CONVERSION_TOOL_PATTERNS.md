# Conversion Tool Error Patterns

This document captures recurring error patterns from TypeScript-to-F# conversion tools (Glutinum, Hawaii) and their fixes.

## Common Error Patterns

### 1. Reserved Keyword Handling
**Issue**: F# reserved keywords used as identifiers without escaping
```fsharp
// WRONG - causes compilation error
abstract member namespace: string option with get, set

// CORRECT - use backticks to escape
abstract member ``namespace``: string option with get, set
```

**Affected Keywords**: `namespace`, `end`, `type`, `module`, `function`, `match`, `with`

### 2. Object Expression Syntax
**Issue**: Using `member val` in object expressions (not allowed)
```fsharp
// WRONG - FS3168 error
{ new DurableObjectListOptions with
    member val start = None with get, set }

// CORRECT - use mutable backing field
let mutable _start = None
{ new DurableObjectListOptions with
    member _.start with get() = _start and set(v) = _start <- v }
```

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

### 5. Pattern Matching Syntax
**Issue**: Incorrect parentheses in discriminated union pattern matching
```fsharp
// WRONG - Management API generated code
match this with
| (@cfBaaiBgeSmallEnV1Numeric_5) -> "@cf/baai/bge-small-en-v1.5"

// CORRECT
match this with
| ``@cfBaaiBgeSmallEnV1Numeric_5`` -> "@cf/baai/bge-small-en-v1.5"
```

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

## Recommended Fixes for Conversion Tools

1. **Reserved Keywords**: Add comprehensive F# keyword checking and auto-escape with backticks
2. **Object Expressions**: Detect interface implementations and generate proper getter/setter syntax
3. **Global Values**: When generating globals, always wrap in a module
4. **Type Ordering**: Implement dependency analysis to order type declarations correctly
5. **Pattern Matching**: Special handling for discriminated union case names with special characters
6. **Promise APIs**: Use Fable's Promise module consistently
7. **Static Analysis**: Better detection of global constructors vs instance methods

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