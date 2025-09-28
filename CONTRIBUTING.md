# Contributing to CloudflareFS

First off, thank you for considering contributing to CloudflareFS! It's people like you that help make CloudflareFS a great tool for the F# and Cloudflare communities.

## Code of Conduct

By participating in this project, you agree to abide by our Code of Conduct:
- Be respectful and inclusive
- Welcome newcomers and help them get started
- Focus on what is best for the community
- Show empathy towards other community members

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check existing issues to avoid duplicates. When creating a bug report, please include:

- **Clear and descriptive title**
- **Steps to reproduce** the behavior
- **Expected behavior** description
- **Actual behavior** description
- **Environment details** (OS, .NET version, Node.js version)
- **Code samples** if applicable
- **Error messages** and stack traces

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, provide:

- **Clear and descriptive title**
- **Detailed description** of the proposed feature
- **Use case** explaining why this enhancement would be useful
- **Possible implementation** approach if you have ideas
- **Examples** from other projects if applicable

### Pull Requests

1. **Fork the repository** and create your branch from `main`
2. **Follow the coding standards** (see below)
3. **Write or update tests** for your changes
4. **Update documentation** if you're changing APIs
5. **Ensure all tests pass** before submitting
6. **Create a clear PR description** explaining your changes

## Development Setup

### Prerequisites

```bash
# Required tools
.NET 8.0 or later
Node.js 18+
npm or yarn

# F# specific tools
dotnet tool install -g fable
dotnet tool install -g fantomas

# Binding generation tools
npm install -g @glutinum/cli
dotnet tool install -g hawaii
```

### Getting Started

```bash
# Clone your fork
git clone https://github.com/your-username/CloudflareFS.git
cd CloudflareFS

# Install dependencies
dotnet restore
npm install

# Build the project
dotnet build

# Run tests
dotnet test

# Format code
dotnet fantomas . --recurse
```

## Project Structure

Understanding the project structure will help you contribute effectively:

```
CloudflareFS/
├── src/
│   ├── Runtime/          # In-Worker APIs (JavaScript interop)
│   │   └── CloudFlare.*/ # Individual service bindings
│   │
│   ├── Management/       # REST APIs (HTTP clients)
│   │   └── CloudFlare.Management.*/ # Management clients
│   │
│   └── Core/            # Shared utilities and types
│
├── generators/
│   ├── glutinum/        # TypeScript → F# generation
│   └── hawaii/          # OpenAPI → F# generation
│
├── samples/             # Example applications
├── docs/               # Documentation
└── tests/              # Test projects
```

## Coding Standards

### F# Style Guide

We follow the [F# Style Guide](https://docs.microsoft.com/en-us/dotnet/fsharp/style-guide/) with these specifics:

```fsharp
// Use meaningful names
let calculateUserScore user =  // ✅ Good
let calc u =                   // ❌ Avoid

// Prefer immutability
let mutable x = 1  // ⚠️ Use only when necessary

// Use type annotations for public APIs
let processRequest (request: Request) : Async<Response> =  // ✅

// Organize code logically
module UserManagement =
    type User = { Id: string; Name: string }

    let private validateUser user = ...

    let createUser name = ...
```

### Formatting

- Use **Fantomas** for consistent formatting
- Run `dotnet fantomas . --recurse` before committing
- Configure your editor to format on save if possible

### Documentation

- Add XML documentation to all public APIs:
```fsharp
/// <summary>
/// Creates a new KV namespace with the specified name.
/// </summary>
/// <param name="accountId">The Cloudflare account ID</param>
/// <param name="name">The namespace name</param>
/// <returns>The created namespace</returns>
let createNamespace (accountId: string) (name: string) = ...
```

## Contributing to Different Areas

### Runtime Bindings

Working on Runtime APIs (in-Worker JavaScript interop):

1. **Check TypeScript definitions** in `@cloudflare/workers-types`
2. **Use Glutinum** for initial generation when possible
3. **Hand-tune** for F# idioms (Options instead of nulls, etc.)
4. **Test** with actual Worker deployment

Example contribution:
```fsharp
// src/Runtime/CloudFlare.Queues/Types.fs
namespace CloudFlare.Queues

[<AllowNullLiteral>]
type Queue =
    abstract member send: message: obj -> JS.Promise<unit>
    abstract member sendBatch: messages: obj[] -> JS.Promise<BatchResult>
```

### Management APIs

Working on Management APIs (REST clients):

1. **Extract service** from Cloudflare's OpenAPI spec
2. **Configure Hawaii** generation
3. **Update namespaces** to match project structure
4. **Add helper methods** for common operations

Example:
```fsharp
// src/Management/CloudFlare.Management.DNS/Helpers.fs
module CloudFlare.Management.DNS.Helpers

let createARecord zone name ipAddress =
    { Type = "A"
      Name = name
      Content = ipAddress
      TTL = Some 3600
      Proxied = Some true }
```

### Adding New Services

To add support for a new Cloudflare service:

1. **Create Runtime binding** if it has in-Worker APIs
2. **Create Management client** if it has REST APIs
3. **Add sample** demonstrating usage
4. **Update documentation** in README and docs/

### Documentation Contributions

- **API Documentation**: Add XML docs to code
- **Guides**: Create markdown files in `docs/`
- **Examples**: Add to `samples/` directory
- **README**: Keep current with implementation status

## Testing

### Unit Tests

```fsharp
// tests/CloudFlare.KV.Tests/Tests.fs
[<Test>]
let ``KV.get returns None for missing key`` () =
    let kv = mockKVNamespace()
    let result = kv.get("missing") |> Async.RunSynchronously
    Assert.AreEqual(None, result)
```

### Integration Tests

For integration tests that require Cloudflare resources:
- Use test accounts when possible
- Mock external dependencies
- Document any required setup

## Commit Messages

Follow conventional commits format:

```
feat: Add Durable Objects runtime bindings
fix: Correct KV expiration handling
docs: Update Management API examples
chore: Upgrade Fable to 4.x
test: Add R2 multipart upload tests
```

## Release Process

1. **Version Bump**: Update version in `.fsproj` files
2. **Changelog**: Update CHANGELOG.md
3. **Tag**: Create git tag `v1.2.3`
4. **NuGet**: Packages are automatically published on tag

## Getting Help

- **Discord**: Join our community chat (link in README)
- **GitHub Discussions**: Ask questions and share ideas
- **Issues**: Report bugs or request features
- **Twitter**: Follow @CloudflareFS for updates

## Recognition

Contributors will be recognized in:
- The README acknowledgments section
- Release notes for significant contributions
- The CONTRIBUTORS.md file

## License

By contributing, you agree that your contributions will be licensed under the same dual MIT/Apache 2.0 license that covers the project.

## Thank You!

Your contributions make CloudflareFS better for everyone. Whether it's fixing a typo, adding a new binding, or implementing a major feature, every contribution is valued and appreciated.

Happy coding! 🚀