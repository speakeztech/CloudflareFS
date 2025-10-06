# R2WebDAV CLI

A command-line tool for managing R2-backed WebDAV workers on Cloudflare. This tool uses the CloudFlareFS Management API to provision and deploy workers **without requiring `wrangler` or `wrangler.toml` configuration files**.

## Philosophy

This CLI demonstrates the **code-first deployment** approach using CloudFlareFS:

- ✅ **Pure F# and Management API** - No TOML configuration files
- ✅ **Type-safe operations** - Full IntelliSense and compile-time checking
- ✅ **Idempotent commands** - Safe to run repeatedly
- ✅ **Fable-based deployment** - Compiles F# to JavaScript and deploys via API
- ✅ **Complete automation** - From user creation to worker deployment

## Prerequisites

### Global Tools Required

Install these tools globally:

```bash
# Fable - F# to JavaScript compiler
dotnet tool install -g fable

# Verify installation
fable --version
```

### Build and Install R2WebDAV CLI

```bash
cd samples/R2WebDAV-CLI

# Build and pack
dotnet pack -c Release

# Install globally
dotnet tool install -g --add-source ./bin/Release R2WebDAV.CLI

# Verify installation
r2webdav --help
```

### Other Requirements

- **.NET 10 SDK** (or latest .NET SDK)
- **Node.js and npm** - For Fable's JavaScript dependencies
- **Cloudflare Account** with:
  - API Token with Workers and R2 permissions
  - Account ID

## Configuration

Set these environment variables:

### Linux/macOS

```bash
# Required
export CLOUDFLARE_API_TOKEN="your-api-token"
export CLOUDFLARE_ACCOUNT_ID="your-account-id"

# Optional (defaults to 'r2-webdav-fsharp')
export CLOUDFLARE_WORKER_NAME="your-worker-name"
```

### Windows (PowerShell)

```powershell
$env:CLOUDFLARE_API_TOKEN="your-api-token"
$env:CLOUDFLARE_ACCOUNT_ID="your-account-id"
$env:CLOUDFLARE_WORKER_NAME="your-worker-name"  # optional
```

**Getting your Cloudflare credentials**:
1. **API Token**: https://dash.cloudflare.com/profile/api-tokens
   - Create token with `Workers R2 Storage:Edit` and `Workers Scripts:Edit` permissions
2. **Account ID**: Found in URL when logged into Cloudflare dashboard
   - Example: `https://dash.cloudflare.com/YOUR_ACCOUNT_ID`

## Commands

### `r2webdav add-user`

Add a new WebDAV user with a dedicated R2 bucket.

```bash
r2webdav add-user --username alice --password secret123
```

**What it does**:
1. ✅ Creates an R2 bucket: `alice-webdav-bucket`
2. ✅ Sets a worker secret: `USER_ALICE_PASSWORD`
3. ✅ Adds an R2 binding: `alice_webdav_sync` → `alice-webdav-bucket`
4. ✅ Preserves existing user bindings

**Idempotent**: Safe to run multiple times (updates password if user exists)

### `r2webdav remove-user`

Remove a user and all associated resources.

```bash
r2webdav remove-user --username alice
```

**What it does**:
1. ✅ Removes the R2 bucket binding from worker
2. ✅ Deletes the worker secret
3. ✅ Deletes the R2 bucket (must be empty)

### `r2webdav list-users`

List all users configured for this worker.

```bash
r2webdav list-users
```

**Example output**:
```
┌──────────┬───────────────────────┬─────────────────────┐
│ Username │ Bucket Name           │ Created             │
├──────────┼───────────────────────┼─────────────────────┤
│ alice    │ alice-webdav-bucket   │ 2025-10-06 16:05:18 │
│ bob      │ bob-webdav-bucket     │ 2025-10-06 16:08:14 │
└──────────┴───────────────────────┴─────────────────────┘
```

**Scoped**: Only shows users bound to the configured worker (not other workers in your account)

### `r2webdav status`

Show comprehensive deployment status.

```bash
r2webdav status
```

**Example output**:
```
── R2WebDAV Status: r2-webdav-fsharp ───────────────────────

Worker Deployment
  Name: r2-webdav-fsharp
  Status: ✓ Deployed
  Last Modified: 10/6/2025 4:11:31 PM +00:00

R2 Buckets (WebDAV)
  Total: 2 bucket(s)
  • alice-webdav-bucket (user: alice)
  • bob-webdav-bucket (user: bob)

Configured Users
  2 user(s) configured for r2-webdav-fsharp:
  • alice
    Bucket: alice-webdav-bucket
    Binding: alice_webdav_sync
    Secret: USER_ALICE_PASSWORD
  • bob
    Bucket: bob-webdav-bucket
    Binding: bob_webdav_sync
    Secret: USER_BOB_PASSWORD
```

**Scoped**: Shows only resources bound to the configured worker

### `r2webdav deploy`

Build and deploy the R2WebDAV worker using Fable and the Management API.

```bash
r2webdav deploy
```

**What it does**:
1. ✅ Runs `npm install` (if `node_modules` doesn't exist)
2. ✅ Compiles F# to JavaScript using Fable: `fable . --outDir dist`
3. ✅ Uploads worker to Cloudflare via Management API
4. ✅ Uses existing bindings configured via `add-user`
5. ✅ Sets compatibility date and flags

**No wrangler required**: Uses pure Management API - no `wrangler.toml` needed!

#### Custom Worker Path

By default, deploy looks for the worker in `../R2WebDAV` relative to the current directory.

```bash
r2webdav deploy --worker-path /path/to/your/worker
```

## Complete Workflow Example

```bash
# 1. Set up environment
export CLOUDFLARE_API_TOKEN="your-token"
export CLOUDFLARE_ACCOUNT_ID="your-account"

# 2. Add users
r2webdav add-user --username alice --password secret123
r2webdav add-user --username bob --password password456

# 3. Build and deploy worker
r2webdav deploy

# 4. Check status
r2webdav status

# 5. List all users
r2webdav list-users

# 6. Access your WebDAV server
# https://r2-webdav-fsharp.<your-account>.workers.dev/
# Username: alice, Password: secret123

# 7. Remove a user when done
r2webdav remove-user --username alice
```

## Architecture

This CLI demonstrates CloudFlareFS's dual-layer architecture:

### Management Layer (This CLI)
- **CloudFlare.Management.R2** - R2 bucket management
- **CloudFlare.Management.Workers** - Worker deployment and configuration
- Generated from OpenAPI specs using **Hawaii**
- Uses **FSharp.SystemTextJson** for modern JSON handling
- Runs on your local machine or CI/CD
- Built on **.NET 10**

### Runtime Layer (The Worker)
- **CloudFlare.Worker.Context** - Worker runtime bindings
- **CloudFlare.R2** - R2 runtime operations
- Generated from TypeScript definitions using **Glutinum**
- Compiled to JavaScript via **Fable**
- Runs inside Cloudflare Workers

## Project Structure

```
samples/R2WebDAV-CLI/
├── Core/
│   ├── Config.fs          # Environment variable configuration
│   ├── Naming.fs          # Deterministic resource naming
│   ├── R2Client.fs        # R2 Management API operations
│   └── WorkersClient.fs   # Workers Management API operations
├── Commands/
│   ├── AddUser.fs         # Create user + bucket + binding + secret
│   ├── RemoveUser.fs      # Remove all user resources
│   ├── ListUsers.fs       # List users (scoped to worker)
│   ├── Status.fs          # Show deployment status
│   └── Deploy.fs          # Build with Fable and deploy via API
├── Program.fs             # CLI argument parsing (Argu)
├── R2WebDAV-CLI.fsproj    # Project file
└── README.md              # This file
```

## Advantages Over Wrangler

1. **Type Safety** - Full F# type checking and IntelliSense for all operations
2. **Code-First** - Infrastructure as code, not TOML configuration
3. **Single Language** - F# for both infrastructure and runtime logic
4. **API-Driven** - Direct Management API access, no CLI wrapper subprocess
5. **Composable** - Build custom workflows using CloudFlareFS libraries
6. **Modern Stack** - .NET 10, FSharp.SystemTextJson, pure F# idioms
7. **Scoped Operations** - Commands only affect the configured worker
8. **Idempotent** - Safe to re-run commands without side effects

## Key Features

### API-First Architecture

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

Resources follow consistent naming patterns from username:

```fsharp
// From username "alice":
let bucketName = "alice-webdav-bucket"         // R2 bucket
let bindingName = "alice_webdav_sync"          // Worker binding
let secretName = "USER_ALICE_PASSWORD"         // Secret name
```

This makes automation reliable and resources discoverable.

### Spectre.Console UI

Rich terminal output using FsSpectre:

- ✓ ✗ ⚠ Colored status indicators
- Spinners during async API calls
- Tables for list commands
- Formatted error messages with proper escaping

## Development

### Building

```bash
dotnet build
```

### Testing Locally (without installing)

```bash
# Set credentials
export CLOUDFLARE_API_TOKEN=your-token
export CLOUDFLARE_ACCOUNT_ID=your-account-id

# Run commands via dotnet run
dotnet run -- status
dotnet run -- add-user --username testuser --password testpass123
dotnet run -- deploy
```

### Updating the Tool

After making changes:

```bash
dotnet pack -c Release
dotnet tool uninstall -g R2WebDAV.CLI
dotnet tool install -g --add-source ./bin/Release R2WebDAV.CLI
```

### Uninstalling

```bash
dotnet tool uninstall -g R2WebDAV.CLI
```

## Troubleshooting

### "Fable not found"

Install Fable globally:
```bash
dotnet tool install -g fable
fable --version  # Verify
```

### "Configuration Error: CLOUDFLARE_API_TOKEN environment variable not set"

Ensure environment variables are set in your current shell session. They are not persisted across sessions unless added to your shell profile (`.bashrc`, `.zshrc`, PowerShell profile, etc.).

### "No WebDAV buckets bound to this worker"

This means no users have been added yet. Run `r2webdav add-user` first. The `status` and `list-users` commands only show resources bound to the configured worker (not other workers in your account).

### "Bucket must be empty before deletion"

When removing a user, the R2 bucket must be empty. Delete objects via:
- Cloudflare dashboard
- R2 API
- WebDAV client (mount and delete files)

### Deploy fails with "Could not execute because the specified command or file was not found"

This means `fable` is not in your PATH. Install it globally:
```bash
dotnet tool install -g fable
```

Then verify `fable` can be found:
```bash
which fable  # Linux/macOS
where fable  # Windows
```

## Related Samples

- **R2WebDAV** (`../R2WebDAV/`) - The F# worker that this CLI deploys
- **HelloWorker** (`../HelloWorker/`) - Minimal worker example

## Related Documentation

See the CloudFlareFS documentation in `docs/`:

- `docs/00_architecture_decisions.md` - Why dual-layer architecture
- `docs/02_dual_layer_architecture.md` - Understanding Runtime vs Management
- `docs/03_openapi_generation.md` - How Management APIs are generated
- `docs/04_code_first_deployment.md` - Philosophy behind this approach
- `docs/08_conversion_patterns.md` - Converting between layers

## Technology Stack

- **Language**: F# (via .NET 10)
- **JSON**: FSharp.SystemTextJson (not Newtonsoft.Json)
- **CLI Framework**: Argu (functional argument parsing)
- **UI**: Spectre.Console via FsSpectre
- **Code Generation**: Hawaii (OpenAPI → F#)
- **Worker Compilation**: Fable (F# → JavaScript)
- **APIs**: Cloudflare Management API (REST)

## License

Same as CloudFlareFS project

---

**Note**: This is a sample project demonstrating CloudflareFS Management API usage. It's designed as a learning resource and starting point for building your own infrastructure-as-code tools with F#.
