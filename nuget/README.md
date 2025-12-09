# CloudflareFS.Runtime NuGet Package

This folder contains the build scripts and configuration for creating the `CloudflareFS.Runtime` NuGet package.

## Building the Package

From the `nuget/` directory, run:

```bash
dotnet fsi build.fsx
```

This will:
1. Build all projects in Release mode
2. Generate the unified `CloudflareFS.fsproj` for Fable
3. Create the NuGet package in `out/CloudflareFS.Runtime.{version}.nupkg`

## Changing the Version

Edit `CloudflareFS.Runtime.proj` and update the `<Version>` element:

```xml
<Version>0.1.0</Version>
```

## How It Works

The `CloudflareFS.Runtime` package bundles multiple F# projects into a single NuGet package that works with Fable (F# to JavaScript compiler).

### The Problem

Fable packages need a special structure:
- A `fable/` folder containing an `.fsproj` file and all `.fs` source files
- The `.fsproj` must list all files in the correct compilation order

Maintaining this manually across 10+ projects would be error-prone and tedious.

### The Solution

The `build.fsx` script automatically generates a unified `CloudflareFS.fsproj` by:

1. Reading each Runtime project's `.fsproj` file
2. Extracting the `<Compile Include="..." />` entries (preserving order)
3. Combining them into a single `.fsproj` with correct relative paths

This means when you add, remove, or rename files in any Runtime project, you just rebuild the package and the unified `.fsproj` is automatically updated.

### Package Structure

The generated package contains:

```
CloudflareFS.Runtime.{version}.nupkg
├── lib/net8.0/           # Compiled DLLs for IDE support
│   ├── CloudFlare.AI.dll
│   ├── CloudFlare.D1.dll
│   ├── CloudFlare.KV.dll
│   └── ...
├── fable/                # Fable sources
│   ├── CloudflareFS.fsproj   # Unified project file
│   ├── Core/
│   │   └── CloudFlare.Core/*.fs
│   └── Runtime/
│       ├── CloudFlare.AI/*.fs
│       ├── CloudFlare.D1/*.fs
│       └── ...
└── README.md
```

### Projects Included

The package includes Core and all Runtime projects:

- `CloudFlare.Core` - Core utilities
- `CloudFlare.Worker.Context` - Worker context types (Request, Response, etc.)
- `CloudFlare.AI` - Workers AI bindings
- `CloudFlare.D1` - D1 database bindings
- `CloudFlare.DurableObjects` - Durable Objects bindings
- `CloudFlare.Hyperdrive` - Hyperdrive bindings
- `CloudFlare.KV` - KV storage bindings
- `CloudFlare.Queues` - Queues bindings
- `CloudFlare.R2` - R2 storage bindings
- `CloudFlare.Vectorize` - Vectorize bindings

Management projects are **not** included as they use .NET-specific libraries incompatible with Fable. They will be distributed as separate packages.

## Files

| File | Description |
|------|-------------|
| `build.fsx` | Fun.Build script for building and packing |
| `CloudflareFS.Runtime.proj` | NuGet package definition (version, metadata, contents) |
| `CloudflareFS.fsproj` | Auto-generated unified Fable project (gitignored) |
| `out/` | Output folder for .nupkg files (gitignored) |
