# R2WebDAV - F# WebDAV Server over Cloudflare R2

A production-ready WebDAV server implementation running on Cloudflare Workers, written entirely in F# and compiled to JavaScript via Fable.

## Features

- Full WebDAV protocol support (PROPFIND, GET, PUT, DELETE, MKCOL, COPY, MOVE)
- Multi-user authentication with timing-safe password comparison
- Deterministic naming for user buckets and secrets
- CORS support for web clients
- Directory listing and navigation
- Cloudflare R2 storage backend

## Architecture

This sample demonstrates CloudflareFS's dual-layer architecture:

- **Types.fs** - WebDAV property types and R2 object conversion
- **Auth.fs** - Basic authentication with timing-safe comparison
- **R2Helpers.fs** - R2 bucket operations (list, head, delete)
- **WebDav.fs** - WebDAV protocol handlers
- **Main.fs** - Request routing and CORS

## Setup

### 1. Install dependencies

```bash
npm install
```

### 2. Configure user buckets

Edit `wrangler.toml` to add R2 bucket bindings for each user:

```toml
[[r2_buckets]]
binding = "alice_webdav_sync"
bucket_name = "alice-webdav-bucket"

[[r2_buckets]]
binding = "bob_webdav_sync"
bucket_name = "bob-webdav-bucket"
```

### 3. Set user passwords

```bash
wrangler secret put USER_ALICE_PASSWORD
wrangler secret put USER_BOB_PASSWORD
```

### 4. Build and run

```bash
# Development
npm run dev

# Deploy to production
npm run deploy
```

## Usage

Connect any WebDAV client to your Worker URL:

```
URL: https://your-worker.workers.dev/webdav
Username: alice
Password: (the password you set)
```

## How It Works

### Authentication

The server uses deterministic naming:
- Username `alice` → Bucket binding `alice_webdav_sync`
- Username `alice` → Password secret `USER_ALICE_PASSWORD`

This eliminates configuration files - just follow the naming convention.

### R2 Storage

Files are stored directly in R2 with:
- `httpMetadata` for Content-Type, Content-Language, etc.
- `customMetadata` for WebDAV properties (e.g., resourcetype)

Directories are represented as zero-byte objects with `resourcetype: <collection />`.

### F# to JavaScript

The F# code compiles to JavaScript via Fable:

```fsharp
// F# with CloudflareFS bindings
let fetch (request: Request) (env: Env) (ctx: ExecutionContext) =
    promise {
        let! authResult = parseAndVerifyAuth authHeader env
        match authResult with
        | Some (username, bucket) ->
            let! response = dispatchHandler request bucket
            return addCorsHeaders response
        | None ->
            return unauthorizedResponse ()
    }
```

Becomes clean JavaScript that runs on Cloudflare Workers.

## Comparison with TypeScript Version

The original TypeScript implementation is a single 881-line file. This F# version:

- Uses modular architecture (5 files)
- Leverages F# pattern matching for request routing
- Uses computation expressions (`promise { }`) for async
- Provides type safety via CloudflareFS bindings
- Compiles to equivalent JavaScript output

## CloudflareFS Benefits

1. **Type-safe bindings** - R2Bucket, Request, Response types from CloudflareFS.Worker.Context and CloudflareFS.R2
2. **Wrapper functions** - Clean F# API hiding JavaScript interop details
3. **Fable compilation** - F# → JavaScript for Workers runtime
4. **Idiomatic F#** - Pattern matching, computation expressions, options instead of nulls

## License

MIT OR Apache-2.0
