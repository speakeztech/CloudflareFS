# R2WebDAV-CLI Design Document

## Overview

R2WebDAV-CLI is an F# command-line tool that replaces PowerShell scripts with a type-safe, cross-platform solution for managing WebDAV users. It uses CloudflareFS Management APIs exclusively, eliminating dependency on `wrangler` CLI commands.

## Architecture

### Replacement Strategy

**Original PowerShell Workflow**:
```
add-user.ps1 → wrangler r2 bucket create → wrangler secret put → edit wrangler.toml → wrangler deploy
```

**New F# CLI Workflow**:
```
R2WebDAV-CLI add-user → Management.R2 API → Management.Workers API → TOML library → Management.Workers API
```

### Technology Stack

1. **CloudflareFS.Management.R2** - R2 bucket creation/deletion via Hawaii-generated client
2. **CloudflareFS.Management.Workers** - Worker deployment and secrets management
3. **SpectreCoff** - Terminal UI library (F# wrapper for Spectre.Console)
4. **Tomlet** or **Tommy** - TOML parsing and manipulation for wrangler.toml
5. **Argu** - Command-line argument parsing

## Commands

### 1. `add-user`
Add a new WebDAV user with deterministic naming

**Usage**:
```bash
dotnet run --project samples/R2WebDAV-CLI add-user --username alice --password secret123
```

**Workflow**:
1. Validate username (alphanumeric + underscore)
2. Generate deterministic names:
   - Bucket: `{username}-webdav-bucket`
   - Binding: `{username}_webdav_sync`
   - Secret: `USER_{USERNAME}_PASSWORD`
3. Check if R2 bucket exists (Management.R2.Buckets.List)
4. Create R2 bucket if needed (Management.R2.Buckets.Create)
5. Store password as Worker secret (Management.Workers.Secrets.Put)
6. Parse wrangler.toml
7. Add R2 bucket binding to wrangler.toml
8. Save wrangler.toml
9. Deploy worker with updated configuration (Management.Workers.Scripts.Upload)
10. Display connection details with formatted output

**TUI Elements**:
- Progress spinner during API calls
- Status panels showing each step
- Success/error messages with color coding
- Connection details in a formatted panel

### 2. `list-users`
Display all configured WebDAV users

**Usage**:
```bash
dotnet run --project samples/R2WebDAV-CLI list-users
```

**Workflow**:
1. List all Worker secrets (Management.Workers.Secrets.List)
2. Filter secrets matching `USER_*_PASSWORD` pattern
3. Extract usernames from secret names
4. List R2 buckets (Management.R2.Buckets.List)
5. Match buckets to users
6. Display formatted table of users with bucket info

**TUI Elements**:
- Table with columns: Username, Bucket Name, Bucket Size, Created Date
- Color-coded status indicators
- Summary footer with total count

### 3. `remove-user`
Remove a WebDAV user and all associated resources

**Usage**:
```bash
dotnet run --project samples/R2WebDAV-CLI remove-user --username alice
```

**Workflow**:
1. Confirm deletion with interactive prompt
2. Delete R2 bucket (Management.R2.Buckets.Delete)
3. Delete Worker secret (Management.Workers.Secrets.Delete)
4. Remove binding from wrangler.toml
5. Save wrangler.toml
6. Optionally redeploy worker
7. Display confirmation

**TUI Elements**:
- Warning panel with red border
- Confirmation prompt requiring "DELETE" input
- Progress indicators
- Success confirmation

### 4. `deploy`
Deploy the WebDAV worker with current configuration

**Usage**:
```bash
dotnet run --project samples/R2WebDAV-CLI deploy
```

**Workflow**:
1. Parse wrangler.toml
2. Read compiled JavaScript from dist/Main.js
3. Upload worker script (Management.Workers.Scripts.Upload)
4. Update worker bindings
5. Display deployment URL

**TUI Elements**:
- File upload progress
- Deployment status
- Worker URL in highlighted panel

### 5. `status`
Show current WebDAV deployment status

**Usage**:
```bash
dotnet run --project samples/R2WebDAV-CLI status
```

**Workflow**:
1. Get worker info (Management.Workers.Scripts.Get)
2. List all secrets
3. List all R2 buckets
4. Parse wrangler.toml
5. Display comprehensive status

**TUI Elements**:
- Multiple panels showing:
  - Worker info (name, version, routes)
  - User count
  - R2 bucket statistics
  - Configuration status

## Project Structure

```
samples/R2WebDAV-CLI/
├── R2WebDAV-CLI.fsproj
├── Program.fs              # Entry point with Argu command parsing
├── Commands/
│   ├── AddUser.fs         # Add user command
│   ├── ListUsers.fs       # List users command
│   ├── RemoveUser.fs      # Remove user command
│   ├── Deploy.fs          # Deploy command
│   └── Status.fs          # Status command
├── Core/
│   ├── Config.fs          # Configuration and environment setup
│   ├── Naming.fs          # Deterministic naming conventions
│   ├── TomlHelper.fs      # TOML parsing/manipulation
│   └── CloudflareClient.fs # Management API client wrapper
└── UI/
    ├── Tables.fs          # Table rendering
    ├── Panels.fs          # Status panels
    └── Progress.fs        # Progress indicators
```

## Dependencies

```xml
<PackageReference Include="Argu" Version="6.2.4" />
<PackageReference Include="Spectre.Console" Version="0.49.1" />
<PackageReference Include="SpectreCoff" Version="0.8.0" />
<PackageReference Include="Tomlet" Version="5.3.1" />

<ProjectReference Include="..\..\src\Management\CloudFlare.Management.R2\CloudFlare.Management.R2.fsproj" />
<ProjectReference Include="..\..\src\Management\CloudFlare.Management.Workers\CloudFlare.Management.Workers.fsproj" />
```

## Configuration

The CLI reads Cloudflare credentials from environment variables or config file:

```bash
# Environment variables
export CLOUDFLARE_API_TOKEN=your-token
export CLOUDFLARE_ACCOUNT_ID=your-account-id

# Or use .env file
CLOUDFLARE_API_TOKEN=your-token
CLOUDFLARE_ACCOUNT_ID=your-account-id
```

## Key Advantages Over PowerShell Scripts

1. **No wrangler dependency** - Uses Management APIs directly
2. **Type safety** - F# type system prevents errors
3. **Cross-platform** - Works on Windows, Linux, macOS
4. **Better UX** - SpectreCoff provides rich terminal UI
5. **Atomic operations** - Transaction-like behavior with rollback on failure
6. **Testable** - Can unit test all operations
7. **Extensible** - Easy to add new commands
8. **Single binary** - Self-contained executable via `dotnet publish`

## Implementation Notes

### Authentication

CloudflareFS Management APIs use HTTP clients with bearer token authentication:

```fsharp
let createClient (token: string) (accountId: string) =
    let httpClient = new HttpClient()
    httpClient.DefaultRequestHeaders.Authorization <-
        AuthenticationHeaderValue("Bearer", token)
    // Return configured Hawaii-generated clients
```

### Error Handling

All Management API calls return `Result<'T, 'Error>` for functional error handling:

```fsharp
match! createBucket accountId bucketName with
| Ok bucket ->
    AnsiConsole.MarkupLine("[green]✓ Bucket created[/]")
    return Ok bucket
| Error err ->
    AnsiConsole.MarkupLine($"[red]✗ Failed: {err}[/]")
    return Error err
```

### TOML Manipulation

Use Tomlet for safe TOML editing:

```fsharp
let addR2Binding (tomlPath: string) (binding: string) (bucketName: string) =
    let doc = Toml.ReadFile(tomlPath)
    let r2Buckets = doc.GetArray("r2_buckets")
    r2Buckets.Add(
        TomlTable [
            "binding", TomlString binding
            "bucket_name", TomlString bucketName
        ]
    )
    Toml.WriteFile(tomlPath, doc)
```

## Future Enhancements

1. **Bulk user import** from CSV
2. **User quota management** via R2 bucket policies
3. **Activity monitoring** via Analytics API
4. **Backup/restore** operations
5. **Interactive mode** with menu-based navigation
6. **Configuration wizard** for first-time setup
7. **Health checks** for worker and buckets
