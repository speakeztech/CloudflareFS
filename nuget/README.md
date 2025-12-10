# CloudflareFS NuGet Packages

This folder contains the build scripts and configuration for creating the CloudflareFS NuGet packages:

- **CloudflareFS.Runtime** - F# and Fable bindings for Cloudflare Workers Runtime APIs
- **CloudflareFS.Management** - F# clients for Cloudflare Management APIs

## Building the Packages

From the `nuget/` directory, run:

```bash
dotnet fsi build.fsx
```

This will:
1. Build all projects in Release mode
2. Generate the unified `CloudflareFS.Runtime.fsproj` for Fable
3. Create both NuGet packages in `out/`

## Changing the Version

Edit the respective `.proj` file and update the `<Version>` element:

- `CloudflareFS.Runtime.proj` for the Runtime package
- `CloudflareFS.Management.proj` for the Management package

```xml
<Version>0.1.0</Version>
```

---

## CloudflareFS.Runtime

The `CloudflareFS.Runtime` package bundles multiple F# projects into a single NuGet package that works with Fable (F# to JavaScript compiler).

### The Problem

Fable packages need a special structure:
- A `fable/` folder containing an `.fsproj` file and all `.fs` source files
- The `.fsproj` must list all files in the correct compilation order

Maintaining this manually across 10+ projects would be error-prone and tedious.

### The Solution

The `build.fsx` script automatically generates a unified `CloudflareFS.Runtime.fsproj` by:

1. Reading each Runtime project's `.fsproj` file
2. Extracting the `<Compile Include="..." />` entries (preserving order)
3. Combining them into a single `.fsproj` with correct relative paths

This means when you add, remove, or rename files in any Runtime project, you just rebuild the package and the unified `.fsproj` is automatically updated.

### Package Structure

```
CloudflareFS.Runtime.{version}.nupkg
├── lib/net8.0/           # Compiled DLLs for IDE support
│   ├── CloudFlare.AI.dll
│   ├── CloudFlare.D1.dll
│   ├── CloudFlare.KV.dll
│   └── ...
├── fable/                # Fable sources
│   ├── CloudflareFS.Runtime.fsproj   # Unified project file
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

---

## CloudflareFS.Management

The `CloudflareFS.Management` package provides F# clients for Cloudflare's Management APIs. These are .NET-only (not compatible with Fable) and are used to manage Cloudflare resources from .NET applications.

### Package Structure

```
CloudflareFS.Management.{version}.nupkg
├── lib/netstandard2.0/   # Compiled DLLs
│   ├── CloudFlare.Management.Analytics.dll
│   ├── CloudFlare.Management.D1.dll
│   ├── CloudFlare.Management.Workers.dll
│   └── ...
└── README.md
```

### Projects Included

- `CloudFlare.Management.Analytics` - Analytics API client
- `CloudFlare.Management.D1` - D1 database management
- `CloudFlare.Management.DurableObjects` - Durable Objects management
- `CloudFlare.Management.Hyperdrive` - Hyperdrive management
- `CloudFlare.Management.Pages` - Pages management
- `CloudFlare.Management.Queues` - Queues management
- `CloudFlare.Management.R2` - R2 storage management
- `CloudFlare.Management.Vectorize` - Vectorize management
- `CloudFlare.Management.Workers` - Workers management

---

## Files

| File | Description |
|------|-------------|
| `build.fsx` | Fun.Build script for building and packing |
| `CloudflareFS.Runtime.proj` | Runtime package definition (version, metadata, contents) |
| `CloudflareFS.Management.proj` | Management package definition (version, metadata, contents) |
| `obj/CloudflareFS.Runtime.fsproj` | Auto-generated unified Fable project (in obj/) |
| `out/` | Output folder for .nupkg files (gitignored) |
