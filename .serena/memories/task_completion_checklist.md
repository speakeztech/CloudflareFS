# Task Completion Checklist

## Before Making Changes

### Understand the Context
- [ ] Read relevant documentation in `/docs/`
- [ ] Understand whether working on Runtime (Glutinum) or Management (Hawaii) layer
- [ ] Check if this is generated code or hand-written code
- [ ] Review existing patterns in similar files

### For Runtime API Changes
- [ ] Understand that code must compile with F# AND Fable
- [ ] Check for JavaScript interop implications
- [ ] Verify Fable-compatible patterns are used

### For Management API Changes
- [ ] Verify `Async<Result<'T, 'Error>>` return types (not Task)
- [ ] Use `FSharp.SystemTextJson` (not Newtonsoft.Json)
- [ ] Ensure code is portable (works with Fable, Fidelity, and .NET)

## During Development

### Code Quality
- [ ] Follow F# naming conventions (PascalCase types, camelCase values)
- [ ] Add XML documentation to public APIs
- [ ] Handle reserved keywords properly (backticks or attributes)
- [ ] Prefer immutability over mutable state

### For Generated Code Modifications
- [ ] Document the fix needed in `/docs/08_conversion_patterns.md`
- [ ] Consider if fix should be added to post-processing scripts
- [ ] Ensure fix survives regeneration or document manual steps

## Before Committing

### Build Verification
- [ ] Run `dotnet build` - entire solution compiles
- [ ] Run `dotnet test` - all tests pass
- [ ] Run `dotnet fantomas . --recurse` - code is formatted

### For Generation Changes
- [ ] Test with full regeneration cycle
- [ ] Verify post-processing scripts still work
- [ ] Check for any new compilation errors

### Documentation
- [ ] Update `/PROJECT_STATUS.md` if implementation status changed
- [ ] Update relevant `/docs/*.md` files if architecture changed
- [ ] Update Serena memories if significant patterns discovered

## Commit Message Format
```
feat: Add new Runtime API binding
fix: Correct reserved keyword handling in D1
docs: Update generation pipeline documentation
chore: Upgrade FSharp.SystemTextJson
test: Add integration tests for R2 uploads
```

## Post-Commit

### For New Services
- [ ] Update `/README.md` implementation status table
- [ ] Add sample usage to `/samples/` if appropriate
- [ ] Update Serena memories with new patterns

### For Bug Fixes
- [ ] Add to known issues if pattern might recur
- [ ] Consider contributing fix upstream to Glutinum/Hawaii
