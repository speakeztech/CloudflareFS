# Known Issues & Resolutions

## Resolved Issues

### 1. Vectorize API Version Migration
- **Issue**: Generated Vectorize client was empty
- **Root Cause**: Cloudflare deprecated V1 API in August 2024; Hawaii correctly skips deprecated operations
- **Resolution**: Updated extraction patterns to use V2 endpoints (`/vectorize/v2/indexes`)
- **Date Resolved**: September 2025

### 2. Pattern Matching with @ Symbols
- **Issue**: F# compilation errors with `@` in pattern matching
- **Resolution**: Use backtick escaping: `` `@cfBaaiBgeSmallEnV1Numeric_5` ``
- **Files Affected**: CloudFlare.Vectorize/Types.fs

### 3. Workers Discriminated Unions
- **Issue**: Hawaii doesn't natively support OpenAPI discriminator schemas
- **Resolution**: Post-processing script generates DUs from binding types
- **Script**: `generators/hawaii/post-process-discriminators.fsx`
- **Impact**: 29 binding types successfully consolidated into single DU

### 4. Hawaii Null Reference Exceptions
- **Issue**: Complex nested schemas caused NullReferenceException
- **Resolution**: Added null checks in local Hawaii fork
- **Files Modified**: `src/Program.fs`, `src/OperationParameters.fs`
- **Status**: Fixed locally, pending upstream contribution

### 5. Type Name Sanitization
- **Issue**: Names with hyphens AND underscores (`workers-kv_key_name`) inconsistently sanitized
- **Resolution**: Changed to cumulative transformations in sanitizeTypeName
- **Status**: Fixed locally in Hawaii fork

## Current Known Issues

### 1. KV Management API Generation
- **Issue**: Hawaii complex schema issues prevent generation
- **Status**: Under investigation
- **Workaround**: Manual implementation may be required

### 2. Reserved Keyword Properties
- **Issue**: Properties like `namespace`, `type` require manual backtick escaping
- **Current Workaround**: `` abstract member ``namespace``: string ``
- **Better Solution**: Use `[<JsonPropertyName>]` or `[<CompiledName>]` attributes
- **Status**: Documented in `09_tool_improvement_analysis.md`

### 3. Object Expression Syntax (Glutinum)
- **Issue**: Generates `member val` in object expressions (FS3168 error)
- **Workaround**: Convert to explicit getter/setter with backing fields
- **Better Solution**: Glutinum should generate records for pure data structures

### 4. Global Values in Namespaces (Glutinum)
- **Issue**: Global values placed directly in namespaces (FS0201 error)
- **Workaround**: Manually wrap in `Globals` module
- **Better Solution**: Glutinum should auto-wrap globals in modules

## Tool Improvement Roadmap

See `/docs/09_tool_improvement_analysis.md` for comprehensive tool enhancement recommendations:

### Priority 1: Semantic Property Mapping
- **Glutinum**: Use `[<CompiledName>]` for reserved keywords
- **Hawaii**: Use `[<JsonPropertyName>]` for reserved keywords

### Priority 2: Smart Type Selection (Glutinum)
- Generate F# records for pure data structures
- Generate F# interfaces only for behavioral contracts
- Default to immutability

### Priority 3: Type Dependency Analysis (Both)
- Topological sort for type declarations
- Handle circular references with `and` keyword

## Debugging Tips

### Empty Generated Client
If Hawaii produces an empty client:
1. Check if all operations are deprecated
2. Verify API version in extraction patterns
3. Look for Hawaii warnings about skipped operations

### Compilation Errors After Generation
1. Check for reserved keywords needing backticks
2. Look for forward reference issues (type ordering)
3. Verify object expression syntax
4. Check for global values outside modules

### JSON Serialization Issues
- Ensure `FSharp.SystemTextJson` is used (not `Newtonsoft.Json`)
- Check `[<JsonPropertyName>]` attributes match OpenAPI schema
