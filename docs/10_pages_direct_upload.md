# Cloudflare Pages Direct Upload Implementation

## Document Purpose

This document captures the exploration, design decisions, and implementation plan for adding Cloudflare Pages direct upload support to CloudflareFS. It serves as both documentation and "breadcrumbs" for continuing work on this feature.

## Background & Context

### The Goal

Enable deployment of static sites to Cloudflare Pages entirely through CloudflareFS, without falling back to wrangler CLI. This is part of the broader SpeakEZ project which uses CloudflareFS as its sole interface to Cloudflare services.

### Why This Matters

The SpeakEZ CLI already successfully uses CloudflareFS for:
- R2 bucket management
- D1 database management
- Workers deployment (including ES modules with bindings)

Adding Pages support completes the deployment story, allowing a Hugo static site to be deployed alongside the AI worker infrastructure.

## Exploration Journey

### Phase 1: Initial OpenAPI Generation

**Date**: 2024-11-29

Successfully generated a Pages client from Cloudflare's OpenAPI spec using Hawaii:

```bash
# Extract Pages-specific endpoints from Cloudflare's massive OpenAPI spec
python3 extract_pages_openapi.py > schemas/Pages-openapi.json

# Generate F# client
hawaii --config pages-hawaii.json
```

**Generated endpoints include:**
- `PagesProjectGetProjects` - List all projects
- `PagesProjectCreateProject` - Create a new project
- `PagesProjectGetProject` - Get project details
- `PagesProjectDeleteProject` - Delete a project
- `PagesDeploymentCreateDeployment` - Create a deployment
- `PagesDeploymentGetDeployments` - List deployments
- Plus domain management and rollback endpoints

**Type fixes required** (same pattern as R2, D1, Workers):
- Response types were generated as `string` instead of proper typed responses
- Added `result: Option<System.Text.Json.JsonElement>` to `pagesapi-response-common`
- Changed all response discriminated unions to use `pagesapi-response-common`

### Phase 2: First Deployment Attempt

Created a simple `DeployDirectory` method that:
1. Builds a manifest (JSON mapping paths to SHA256 hashes)
2. Calls `PagesDeploymentCreateDeployment` with the manifest

**Result**: Failed with "Operation is not valid due to the current state of the object"

**Root Cause Discovery**: The deployment creation endpoint expects files to ALREADY be uploaded. The manifest references content that must exist on Cloudflare's servers.

### Phase 3: Understanding the Real Upload Flow

Research into wrangler's source code (`packages/wrangler/src/pages/upload.ts`) revealed the actual process:

```
┌─────────────────────────────────────────────────────────────────┐
│                    Pages Direct Upload Flow                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. Build manifest: { "/path": "hash", ... }                    │
│                          │                                       │
│                          ▼                                       │
│  2. POST /pages/assets/check-missing                            │
│     Body: { hashes: ["hash1", "hash2", ...] }                   │
│     Response: { missing: ["hash3", "hash5", ...] }              │
│                          │                                       │
│                          ▼                                       │
│  3. POST /pages/assets/upload (for each missing file)           │
│     Body: [{ key: "hash", value: base64(content),               │
│              metadata: { contentType: "..." } }]                │
│     - Batched uploads (multiple files per request)              │
│     - Concurrent upload streams (default: 3)                    │
│                          │                                       │
│                          ▼                                       │
│  4. POST /pages/assets/upsert-hashes                            │
│     Body: { hashes: ["hash1", "hash2", ...] }                   │
│     - Registers all hashes for the deployment                   │
│                          │                                       │
│                          ▼                                       │
│  5. POST /pages/projects/{name}/deployments                     │
│     Body: multipart with manifest                               │
│     - Creates the actual deployment                             │
│     - References the uploaded assets via manifest               │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

**Key Insight**: The deployment endpoint is the FINAL step, not the upload mechanism.

## Implementation Plan

### Step 1: Add Assets API Methods to PagesClient

These endpoints are NOT in the OpenAPI spec (undocumented APIs), so we implement them directly:

```fsharp
/// Check which file hashes are missing from Cloudflare's storage
member this.CheckMissingAssets(hashes: string list) : Async<Result<string list, string>>

/// Upload file assets in batches
member this.UploadAssets(files: FileUpload list) : Async<Result<unit, string>>

/// Register all hashes for a deployment
member this.UpsertHashes(hashes: string list) : Async<Result<unit, string>>
```

### Step 2: File Upload Data Structure

```fsharp
type FileUpload = {
    Hash: string           // SHA256 hash (hex encoded, NOT base64)
    Content: byte[]        // Raw file content
    ContentType: string    // MIME type
    FilePath: string       // Original path for logging
}
```

### Step 3: Hash Format Discovery (MAJOR CORRECTION!)

**CRITICAL DISCOVERY**: Cloudflare Pages does NOT use SHA-256! It uses BLAKE3 with a specific input format:

1. Read file contents as bytes
2. Base64 encode the contents
3. Append the file extension (without the leading dot)
4. Hash with BLAKE3
5. Take the first 32 hex characters (16 bytes)

```javascript
// From wrangler source (packages/wrangler/src/pages/hash.ts)
// Uses blake3-wasm library
const hash = blake3hash(base64(content) + extension).toString("hex").slice(0, 32);
```

Implementation in F# (requires Blake3 NuGet package):

```fsharp
// Correct implementation
open Blake3

let computeFileHash (filePath: string) : string =
    let content = File.ReadAllBytes(filePath)
    let base64Content = Convert.ToBase64String(content)
    let extension = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant()
    let dataToHash = base64Content + extension
    let hashBytes = Hasher.Hash(Encoding.UTF8.GetBytes(dataToHash))
    // Take first 32 hex characters (16 bytes)
    BitConverter.ToString(hashBytes.AsSpan().Slice(0, 16).ToArray())
        .Replace("-", "").ToLowerInvariant()
```

This was a critical bug that caused 500 errors when serving the deployed site - the manifest referenced hashes that didn't match what was actually uploaded.

### Step 4: Upload Batching Strategy

Wrangler batches files to avoid request size limits:

```
MAX_BUCKET_SIZE = 50 MB
MAX_BUCKET_FILE_COUNT = 5000
MAX_UPLOAD_ATTEMPTS = 5
BULK_UPLOAD_CONCURRENCY = 3
```

Implementation approach:
1. Sort files into buckets by size
2. Upload buckets concurrently (but respect rate limits)
3. Retry failed uploads with exponential backoff

### Step 5: Content Type Detection

Need to map file extensions to MIME types:

```fsharp
let getContentType (filePath: string) =
    match Path.GetExtension(filePath).ToLowerInvariant() with
    | ".html" | ".htm" -> "text/html"
    | ".css" -> "text/css"
    | ".js" -> "application/javascript"
    | ".json" -> "application/json"
    | ".png" -> "image/png"
    | ".jpg" | ".jpeg" -> "image/jpeg"
    | ".gif" -> "image/gif"
    | ".svg" -> "image/svg+xml"
    | ".woff" -> "font/woff"
    | ".woff2" -> "font/woff2"
    | ".xml" -> "application/xml"
    | ".txt" -> "text/plain"
    | _ -> "application/octet-stream"
```

## API Endpoint Details

### POST /accounts/{account_id}/pages/assets/check-missing

```
Request:
{
  "hashes": ["abc123...", "def456...", ...]
}

Response:
{
  "success": true,
  "result": ["def456..."],  // Only hashes that need uploading
  "errors": [],
  "messages": []
}
```

### POST /accounts/{account_id}/pages/assets/upload

```
Request:
[
  {
    "key": "abc123...",           // Hash as key
    "value": "base64content...",  // File content base64 encoded
    "metadata": {
      "contentType": "text/html"
    },
    "base64": true
  },
  ...
]

Response:
{
  "success": true,
  "result": null,
  "errors": [],
  "messages": []
}
```

### POST /accounts/{account_id}/pages/assets/upsert-hashes

```
Request:
{
  "hashes": ["abc123...", "def456...", ...]
}

Response:
{
  "success": true,
  "result": null,
  "errors": [],
  "messages": []
}
```

## Current State

### Completed ✅
- [x] Pages OpenAPI spec extraction
- [x] Hawaii code generation for Pages client
- [x] Type fixes for response types
- [x] Basic PagesClient wrapper in SpeakEZ CLI
- [x] Project creation working
- [x] DeployPages CLI command structure
- [x] Implement `GetUploadToken` method (JWT acquisition)
- [x] Implement `CheckMissingAssets` method with JWT auth
- [x] Implement `UploadAssetBatch` method with batching (50MB/100 files per batch)
- [x] Implement `UpsertHashes` method with JWT auth
- [x] Fix hash encoding - **BLAKE3** (not SHA-256!) with base64+extension input
- [x] Update `DeployDirectory` to orchestrate full upload flow
- [x] Progress reporting during upload
- [x] **WORKING DEPLOYMENT** - Hugo site successfully deployed to Pages!

### Not Yet Implemented (Future Enhancements)
- [ ] Concurrent upload streams (currently sequential batches)
- [ ] Retry logic with exponential backoff
- [ ] JWT token refresh handling for long uploads

## Code Location Reference

### CloudflareFS
- `src/Management/CloudFlare.Management.Pages/` - Generated Pages client
- `generators/hawaii/schemas/Pages-openapi.json` - Extracted OpenAPI spec
- `generators/hawaii/pages-hawaii.json` - Hawaii configuration

### SpeakEZ CLI
- `cli/src/Core/PagesClient.fs` - High-level Pages operations wrapper
- `cli/src/Commands/DeployPages.fs` - CLI command implementation

## Testing Strategy

### Manual Testing
1. Build Hugo site: `cd hugo && hugo --minify`
2. Run deployment: `dotnet run --project cli -- deploy-pages --verbose`
3. Verify site at: `https://speakez-blog.pages.dev`

### Verification Points
- [ ] Project creation succeeds
- [ ] File hashes computed correctly (hex format)
- [ ] Missing assets check returns expected results
- [ ] File uploads succeed
- [ ] Hash registration succeeds
- [ ] Deployment creation succeeds
- [ ] Site accessible and renders correctly

## Error Handling Considerations

### Rate Limiting
Cloudflare may rate limit uploads. Strategy:
- Start with concurrency of 1
- Increase if no 429 errors
- Decrease on rate limit errors

### Large Sites
For sites with many files:
- Batch uploads to avoid timeout
- Report progress to user
- Allow resumption on failure (track uploaded hashes)

### Network Failures
- Retry individual file uploads
- Don't re-upload already successful files
- Provide clear error messages

## Open Questions

1. **JWT Token Handling**: Does the Pages API use JWT tokens that need refresh during long uploads? Wrangler handles this - need to investigate.

2. **Exact Hash Format**: Confirmed hex-encoded SHA256, but need to verify casing (lowercase vs uppercase).

3. **Bucket Metadata**: What exactly goes in the upload metadata? Just content type or more?

4. **Project Configuration**: Can we configure custom domains, build settings via API?

## Next Steps

1. Implement `CheckMissingAssets` in PagesClient
2. Test with a simple single-file upload
3. Implement full batched upload flow
4. Update DeployDirectory to orchestrate the complete flow
5. Test with actual Hugo site (444 files)
6. Add progress reporting

---

*Last Updated: 2024-11-29*
*Status: In Progress - Implementing direct upload flow*
