# Sample CLI

A command-line tool for managing and deploying Cloudflare Workers using F# and the CloudflareFS Management API. This tool demonstrates **code-first deployment** - deploy workers **without requiring `wrangler` or `wrangler.toml` configuration files**.

## Philosophy

This CLI demonstrates the **code-first deployment** approach using CloudflareFS:

- ✅ **Pure F# and Management API** - No TOML configuration files
- ✅ **Type-safe operations** - Full IntelliSense and compile-time checking
- ✅ **Idempotent commands** - Safe to run repeatedly
- ✅ **Fable-based deployment** - Compiles F# to JavaScript and deploys via API
- ✅ **Multi-worker support** - Deploy any F# worker project
- ✅ **Complete automation** - From user management to worker deployment

## Prerequisites

### Global Tools Required

Install these tools globally:

```bash
# Fable - F# to JavaScript compiler
dotnet tool install -g fable

# Verify installation
fable --version
```

### Build and Install Sample CLI

```bash
cd samples/Sample-CLI

# Build and pack
dotnet pack -c Release

# Install globally
dotnet tool install -g --add-source ./bin/Release Sample.CLI

# Verify installation
sample-cli --help
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

# Optional (used as default for R2WebDAV commands)
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

### `sample-cli deploy`

Build and deploy any F# worker project using Fable and the Management API.

```bash
# Deploy HelloWorker
sample-cli deploy -p ../HelloWorker

# Deploy R2WebDAV
sample-cli deploy -p ../R2WebDAV

# Force redeploy (ignores source change detection)
sample-cli deploy -p ../HelloWorker --force
```

**What it does**:
1. ✅ Detects the project name and entry point from `.fsproj`
2. ✅ Runs `npm install` (if `node_modules` doesn't exist)
3. ✅ Compiles F# to JavaScript using Fable incrementally
4. ✅ Uploads worker to Cloudflare via Management API
5. ✅ Preserves existing bindings (for R2WebDAV users)
6. ✅ Activates worker on `workers.dev` subdomain
7. ✅ Returns the active worker URL

**Features**:
- **Idempotent**: Tracks source file hashes - skips deployment if nothing changed
- **Force flag**: Use `--force` or `-f` to deploy anyway
- **Auto-detection**: Reads `.fsproj` to find the correct entry point file
- **No wrangler required**: Pure Management API - no `wrangler.toml` needed!

**Example Output**:
```
Deploying: helloworker
Source: D:\repos\CloudflareFS\samples\HelloWorker
Entry point: HelloWorker.js
✓ Deployed successfully!

Worker URL: https://helloworker.engineering-0c5.workers.dev
Click the URL above to open in browser
```

### R2WebDAV User Management Commands

These commands are specific to managing the R2WebDAV worker:

#### `sample-cli add-user`

Add a new WebDAV user with a dedicated R2 bucket.

```bash
sample-cli add-user --username alice --password secret123
```

**What it does**:
1. ✅ Creates an R2 bucket: `alice-webdav-bucket`
2. ✅ Sets a worker secret: `USER_ALICE_PASSWORD`
3. ✅ Adds an R2 binding: `alice_webdav_sync` → `alice-webdav-bucket`
4. ✅ Preserves existing user bindings

**Idempotent**: Safe to run multiple times (updates password if user exists)

#### `sample-cli remove-user`

Remove a user and all associated resources.

```bash
sample-cli remove-user --username alice
```

**What it does**:
1. ✅ Removes the R2 bucket binding from worker
2. ✅ Deletes the worker secret
3. ✅ Deletes the R2 bucket (must be empty)

#### `sample-cli list-users`

List all users configured for the R2WebDAV worker.

```bash
sample-cli list-users
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

#### `sample-cli status`

Show deployment status for the R2WebDAV worker.

```bash
sample-cli status
```

**Example output**:
```
── R2WebDAV Status: r2-webdav-fsharp ───────────────────────

Worker Deployment
  Name: r2-webdav-fsharp
  Status: ✓ Deployed

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
```

## Complete Workflow Examples

### Deploying HelloWorker

```bash
# 1. Set up environment
export CLOUDFLARE_API_TOKEN="your-token"
export CLOUDFLARE_ACCOUNT_ID="your-account"

# 2. Deploy HelloWorker
sample-cli deploy -p ../HelloWorker

# 3. Visit the URL shown in output
# https://helloworker.<subdomain>.workers.dev
```

### R2WebDAV Setup

```bash
# 1. Set up environment
export CLOUDFLARE_API_TOKEN="your-token"
export CLOUDFLARE_ACCOUNT_ID="your-account"
export CLOUDFLARE_WORKER_NAME="r2-webdav-fsharp"

# 2. Deploy the R2WebDAV worker
sample-cli deploy -p ../R2WebDAV

# 3. Add WebDAV users
sample-cli add-user --username alice --password secret123
sample-cli add-user --username bob --password password456

# 4. Check status
sample-cli status

# 5. List all users
sample-cli list-users

# 6. Connect from Super Productivity
# Server: https://r2-webdav-fsharp.<subdomain>.workers.dev/webdav
# Username: alice
# Password: secret123

# 7. Remove a user when done
sample-cli remove-user --username alice
```

## Architecture

This CLI demonstrates CloudflareFS's dual-layer architecture:

### Management Layer (This CLI)
- **CloudFlare.Management.R2** - R2 bucket management
- **CloudFlare.Management.Workers** - Worker deployment and configuration
- Generated from OpenAPI specs using **Hawaii**
- Uses **FSharp.SystemTextJson** for modern JSON handling
- Runs on your local machine or CI/CD
- Built on **.NET 10**

### Runtime Layer (The Workers)
- **CloudFlare.Worker.Context** - Worker runtime bindings
- **CloudFlare.R2** - R2 runtime operations
- Generated from TypeScript definitions using **Glutinum**
- Compiled to JavaScript via **Fable**
- Runs inside Cloudflare Workers

## Project Structure

```
samples/Sample-CLI/
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
├── Sample-CLI.fsproj      # Project file
└── README.md              # This file
```

## Advantages Over Wrangler

1. **Type Safety** - Full F# type checking and IntelliSense for all operations
2. **Code-First** - Infrastructure as code, not TOML configuration
3. **Single Language** - F# for both infrastructure and runtime logic
4. **API-Driven** - Direct Management API access, no CLI wrapper subprocess
5. **Composable** - Build custom workflows using CloudflareFS libraries
6. **Modern Stack** - .NET 10, FSharp.SystemTextJson, pure F# idioms
7. **Multi-Worker** - Deploy different workers from same tool
8. **Idempotent** - Safe to re-run commands without side effects
9. **Auto-Discovery** - Reads `.fsproj` to find entry points automatically

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

### Smart Entry Point Detection

The deploy command automatically detects the entry point by:
1. Finding the `.fsproj` file in the worker directory
2. Parsing `<Compile Include="..." />` entries
3. Using the **last** compiled file as the entry point (F# convention)

**Examples**:
- `HelloWorker.fsproj` with `HelloWorker.fs` → compiles to `HelloWorker.js`
- `R2WebDAV.fsproj` with `Main.fs` → compiles to `Main.js`

### Subdomain Activation

After deployment, the tool:
1. Gets your account's `workers.dev` subdomain
2. Activates the worker on that subdomain
3. Returns the working URL (e.g., `https://helloworker.engineering-0c5.workers.dev`)

### Deterministic Naming (R2WebDAV)

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
dotnet run -- deploy -p ../HelloWorker
dotnet run -- add-user --username testuser --password testpass123
```

### Updating the Tool

After making changes:

```bash
dotnet pack -c Release
dotnet tool uninstall -g Sample.CLI
dotnet tool install -g --add-source ./bin/Release Sample.CLI
```

### Uninstalling

```bash
dotnet tool uninstall -g Sample.CLI
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

### "No .fsproj file found"

The deploy command looks for a `.fsproj` file in the worker directory. Make sure you're pointing to the correct path with `-p`.

### "Entry point file not found"

The tool parses the `.fsproj` to find the entry point. Ensure your project has at least one `<Compile Include="..." />` entry.

### Deploy fails with idempotency check

If source hasn't changed, deployment is skipped. Use `--force` or `-f` to deploy anyway:
```bash
sample-cli deploy -p ../HelloWorker --force
```

## Related Samples

- **R2WebDAV** (`../R2WebDAV/`) - WebDAV server backed by R2
- **HelloWorker** (`../HelloWorker/`) - Minimal worker example

Both can be deployed with this CLI!

## Related Documentation

See the CloudflareFS documentation in `docs/`:

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

Same as CloudflareFS project

---

**Note**: This is a sample project demonstrating CloudflareFS Management API usage. It's designed as a learning resource and starting point for building your own infrastructure-as-code tools with F#.
