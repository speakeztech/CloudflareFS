# R2WebDAV-CLI v1.0 - Practical Scope

## Current Limitation

The Workers Management API from CloudflareFS is not yet generated due to Hawaii's complexity with the Workers OpenAPI schema. Therefore, v1.0 of the CLI will use a **hybrid approach**:

1. **R2 Management** - Use CloudflareFS Management API (Hawaii-generated)
2. **Worker Secrets & Deployment** - Use `wrangler` CLI as a subprocess (temporary)
3. **TOML Manipulation** - Use Tomlet library

## V1.0 Commands

### 1. `add-user` - Add new WebDAV user
**What it does**:
- ✅ Creates R2 bucket via Management API
- ⚠️ Stores secret via `wrangler secret put` subprocess
- ✅ Updates wrangler.toml via Tomlet
- ⚠️ Deploys via `wrangler deploy` subprocess

**Advantage over PowerShell**:
- Cross-platform F# instead of Windows PowerShell
- Better error handling and rollback
- Rich TUI with FsSpectre
- Type-safe R2 bucket operations

### 2. `list-users` - List configured users
**What it does**:
- ✅ Lists R2 buckets via Management API
- ✅ Parses wrangler.toml to correlate bindings
- ⚠️ Lists secrets via `wrangler secret list` subprocess

**Advantage**:
- Formatted table with bucket sizes and dates
- Correlates buckets with bindings automatically

### 3. `remove-user` - Remove user and resources
**What it does**:
- ✅ Deletes R2 bucket via Management API
- ⚠️ Deletes secret via `wrangler secret delete` subprocess
- ✅ Updates wrangler.toml via Tomlet

**Advantage**:
- Atomic rollback on failure
- Interactive confirmation
- Better error messages

### 4. `status` - Show deployment status
**What it does**:
- ✅ Lists all R2 buckets and sizes via Management API
- ✅ Parses wrangler.toml configuration
- ⚠️ Gets worker info via `wrangler deployments list` subprocess

**Advantage**:
- Comprehensive dashboard view
- Detects configuration drift

## V2.0 Future Scope

Once CloudflareFS.Management.Workers is generated, we can eliminate all `wrangler` subprocess calls:

1. **Direct Worker Deployment**:
   ```fsharp
   let! result = CloudFlare.Management.Workers.Scripts.upload accountId scriptName content bindings
   ```

2. **Secret Management**:
   ```fsharp
   let! result = CloudFlare.Management.Workers.Secrets.put accountId secretName secretValue
   ```

3. **Worker Configuration**:
   ```fsharp
   let! result = CloudFlare.Management.Workers.Bindings.update accountId bindings
   ```

## Implementation Strategy

### Core Modules

**Config.fs** - Environment and credentials
```fsharp
type CloudflareConfig = {
    ApiToken: string
    AccountId: string
    WorkerName: string
    WranglerPath: string option
}

let loadConfig() : Result<CloudflareConfig, string>
```

**Naming.fs** - Deterministic naming conventions
```fsharp
let getBucketName (username: string) = $"{username.ToLower()}-webdav-bucket"
let getBindingName (username: string) = $"{username.ToLower()}_webdav_sync"
let getSecretName (username: string) = $"USER_{username.ToUpper()}_PASSWORD"
```

**TomlHelper.fs** - TOML manipulation
```fsharp
let addR2Binding (tomlPath: string) (binding: string) (bucketName: string) : Result<unit, string>
let removeR2Binding (tomlPath: string) (binding: string) : Result<unit, string>
let listR2Bindings (tomlPath: string) : Result<(string * string) list, string>
```

**WranglerInterop.fs** - Subprocess calls to wrangler (temporary)
```fsharp
let putSecret (secretName: string) (value: string) : Async<Result<unit, string>>
let deleteSecret (secretName: string) : Async<Result<unit, string>>
let listSecrets () : Async<Result<string list, string>>
let deploy () : Async<Result<string, string>>
```

**R2Client.fs** - CloudflareFS R2 Management API wrapper
```fsharp
let createBucket (config: CloudflareConfig) (bucketName: string) : Async<Result<unit, string>>
let deleteBucket (config: CloudflareConfig) (bucketName: string) : Async<Result<unit, string>>
let listBuckets (config: CloudflareConfig) : Async<Result<BucketInfo list, string>>
```

### Command Implementation

Each command in `Commands/` folder, all using FsSpectre for TUI.

## Why This Hybrid Approach is Still Better

1. **Type Safety** - F# type system prevents errors in R2 operations
2. **Cross-Platform** - Works on Linux/Mac/Windows (unlike PowerShell scripts)
3. **Better UX** - FsSpectre provides rich terminal UI
4. **Atomic Operations** - Rollback on failure
5. **Testable** - Can unit test all R2 operations
6. **Future-Proof** - Easy to swap wrangler calls for Management API when available
7. **Single Binary** - Can distribute as self-contained executable

## Migration Path

When Workers Management API becomes available:

1. Keep all command interfaces the same
2. Replace `WranglerInterop.fs` with `WorkersClient.fs`
3. Update implementation to use Management API
4. Remove `wrangler` as a dependency

Users won't see any difference - just faster, more reliable operations!
