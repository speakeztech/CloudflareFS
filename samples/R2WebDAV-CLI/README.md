# R2WebDAV-CLI

Command-line tool demonstrating CloudflareFS Management API usage for R2 WebDAV user management.

## Overview

This sample project shows how to build a .NET CLI tool using CloudflareFS Management APIs to:
- Create R2 buckets for WebDAV users
- List configured WebDAV users
- Show deployment status

It demonstrates the **API-first** approach: using CloudflareFS Management APIs instead of shelling out to wrangler commands.

## Quick Start

### Prerequisites

- .NET 10.0 SDK (or .NET 8+)
- Cloudflare account with API token
- R2WebDAV worker deployed (see `../R2WebDAV/`)

### Environment Variables

```bash
export CLOUDFLARE_API_TOKEN=your-api-token
export CLOUDFLARE_ACCOUNT_ID=your-account-id
export CLOUDFLARE_WORKER_NAME=r2-webdav-fsharp  # optional, defaults to this
```

**Getting your Cloudflare credentials**:
1. **API Token**: https://dash.cloudflare.com/profile/api-tokens
   - Create token with `Workers R2 Storage:Edit` permission
2. **Account ID**: Found in URL when logged into Cloudflare dashboard
   - Example: `https://dash.cloudflare.com/YOUR_ACCOUNT_ID`

### Option 1: Run with `dotnet run`

```bash
cd samples/R2WebDAV-CLI

# Show help
dotnet run -- --help

# Check status
dotnet run -- status

# Add a user (creates R2 bucket, shows manual steps for secrets/deployment)
dotnet run -- add-user --username alice --password secret123

# List users
dotnet run -- list-users
```

### Option 2: Install as Local .NET Tool

For the full CLI experience, install it as a local dotnet tool:

```bash
cd samples/R2WebDAV-CLI

# Build and pack the tool
dotnet pack

# Install locally
dotnet tool install --global --add-source ./bin/Release R2WebDAV.CLI

# Now use the `r2webdav` command directly
r2webdav status
r2webdav add-user --username alice --password secret123
r2webdav list-users
```

**To update after making changes**:
```bash
dotnet tool uninstall -g R2WebDAV.CLI
dotnet pack
dotnet tool install --global --add-source ./bin/Release R2WebDAV.CLI
```

**To uninstall**:
```bash
dotnet tool uninstall -g R2WebDAV.CLI
```

## Commands

### `status`

Shows deployment status: worker info, R2 buckets, configured users.

```bash
r2webdav status
```

**Output**:
```
R2WebDAV Deployment Status
━━━━━━━━━━━━━━━━━━━━━━━━━━

Worker Deployment
  Name: r2-webdav-fsharp
  Status: ⚠ Check manually with wrangler

R2 Buckets
  Total: 3 bucket(s)
  • alice-webdav-bucket
  • bob-webdav-bucket
  • charlie-webdav-bucket

Configured Users
  3 user(s): alice, bob, charlie
```

### `add-user`

Creates R2 bucket for a new WebDAV user using Management API.

```bash
r2webdav add-user --username alice --password secret123
```

**What it does**:
1. ✅ **Creates R2 bucket** via CloudFlare.Management.R2 API
2. ⚠️ **Displays manual steps** for:
   - Storing password secret (`wrangler secret put`)
   - Updating wrangler.toml with R2 binding
   - Deploying worker (`wrangler deploy`)

**Why manual steps?**
The Workers Management API is generated but secret/deployment operations require additional testing. The CLI demonstrates the R2 Management API working perfectly while documenting the remaining manual steps.

### `list-users`

Lists all WebDAV users by scanning R2 buckets with naming convention.

```bash
r2webdav list-users
```

**Output**:
```
┌──────────┬──────────────────────────┬────────────────────────┐
│ Username │ Bucket Name              │ Created                │
├──────────┼──────────────────────────┼────────────────────────┤
│ alice    │ alice-webdav-bucket      │ 2025-10-06 10:30:00   │
│ bob      │ bob-webdav-bucket        │ 2025-10-05 14:15:00   │
└──────────┴──────────────────────────┴────────────────────────┘
```

## Project Structure

```
samples/R2WebDAV-CLI/
├── Core/
│   ├── Config.fs        # Environment variable loading
│   ├── Naming.fs        # Deterministic naming conventions
│   └── R2Client.fs      # R2 Management API wrapper
├── Commands/
│   ├── AddUser.fs       # Create user + bucket (API-first)
│   ├── ListUsers.fs     # List all WebDAV users
│   └── Status.fs        # Deployment status display
├── Program.fs           # Argu command-line parsing
├── R2WebDAV-CLI.fsproj  # Project configuration
└── README.md            # This file
```

## Key Features

### API-First Architecture

This CLI demonstrates the CloudflareFS philosophy:

```fsharp
// ✅ API-first: Use CloudflareFS Management APIs
let r2 = R2Operations(config)
let! result = r2.CreateBucket(bucketName)

// ❌ NOT: Shelling out to wrangler
// Process.Start("wrangler", "r2 bucket create ...")
```

**Benefits**:
- Type-safe API calls
- Async/await support
- Detailed error messages
- Cross-platform (no shell dependencies)
- Testable (no subprocess mocking)

### Deterministic Naming

Resources are named consistently:

```fsharp
// From username "alice":
let bucketName = "alice-webdav-bucket"         // R2 bucket
let bindingName = "alice_webdav_sync"          // Worker binding
let secretName = "USER_ALICE_PASSWORD"         // Secret name
```

This makes automation reliable and resources discoverable.

### Spectre.Console UI

Rich terminal output using FsSpectre:

- Colored status indicators (✓ ✗ ⚠)
- Loading spinners during API calls
- Tables for list commands
- Formatted error messages

## Limitations & Future Work

### Current Limitations

1. **Secret Management** - Manual wrangler command required
2. **Worker Deployment** - Manual wrangler command required
3. **TOML Updates** - Manual editing of wrangler.toml required

### Why?

The Workers Management API is fully generated and compiles, but these specific operations need integration testing before automation. The CLI shows the R2 Management API working perfectly (API-first!) while documenting the manual steps for Worker operations.

### Future Enhancements

When Workers Management API operations are tested:

1. **Automated Secrets** - Use `WorkersManagementClient.Secrets.Put()`
2. **Automated Deployment** - Use `WorkersManagementClient.Scripts.Upload()`
3. **Remove User Command** - Delete bucket + secret + unbind

See `AddUser.fs:44-61` for TODO markers.

## Development

### Building

```bash
dotnet build
```

### Testing Locally

```bash
# Set credentials
export CLOUDFLARE_API_TOKEN=your-token
export CLOUDFLARE_ACCOUNT_ID=your-account-id

# Run commands
dotnet run -- status
dotnet run -- add-user --username testuser --password testpass123
```

### Packaging

The project is configured as a dotnet tool (`PackAsTool=true`):

```xml
<PropertyGroup>
  <PackAsTool>true</PackAsTool>
  <ToolCommandName>r2webdav</ToolCommandName>
  <PackageId>R2WebDAV.CLI</PackageId>
  <Version>1.0.0</Version>
</PropertyGroup>
```

## Related Documentation

- **R2WebDAV Worker**: `../R2WebDAV/README.md` - The F#/Fable worker this CLI manages
- **Management APIs**: `../../docs/02_dual_layer_architecture.md` - Understanding Runtime vs Management layers
- **Tool Improvements**: `../../docs/09_tool_improvement_analysis.md` - Hawaii post-processors and generation

## Design Decisions

See `../../docs/10_r2webdav_cli_design.md` for the full design rationale including:
- Why API-first over wrangler CLI
- Deterministic naming strategy
- Command structure and user experience
- Future automation roadmap

---

**Note**: This is a sample project demonstrating CloudflareFS Management API usage. It's not published to NuGet - it's designed as a learning resource and starting point for your own CLI tools.
