# R2WebDAV-CLI Sample

Command-line tool demonstrating CloudflareFS Management API usage for R2 bucket management.

## Commands

### `status`
Show deployment overview: worker info, R2 buckets, configured users

### `add-user --username <name> --password <pass>`
Create R2 bucket for new WebDAV user (manual secret/deployment steps displayed)

### `list-users`
List all WebDAV users from R2 bucket naming convention

## Usage

```bash
# Set environment variables
export CLOUDFLARE_API_TOKEN=your-token
export CLOUDFLARE_ACCOUNT_ID=your-account-id
export CLOUDFLARE_WORKER_NAME=r2-webdav-fsharp  # optional

# Run commands
dotnet run -- status
dotnet run -- add-user --username alice --password secret123
dotnet run -- list-users
```

## Limitations

- Worker secret management requires manual `wrangler secret put` (Workers Management API has minor compilation issues)
- Deployment requires manual `wrangler deploy`
- R2Client uses direct HTTP calls (to be updated with generated Management.R2 client)

See `../../docs/10_r2webdav_cli_design.md` for full design documentation.
