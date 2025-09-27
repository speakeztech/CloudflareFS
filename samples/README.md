# CloudflareFS Samples

This directory contains sample applications demonstrating the CloudflareFS F# bindings for Cloudflare Workers.

## Samples

### 1. HelloWorker
A simple "Hello World" style Worker demonstrating:
- Basic routing
- JSON responses
- Custom headers
- KV namespace operations
- Redirects

**To run:**
```bash
cd HelloWorker
npm install
dotnet fable . --outDir dist
npx wrangler dev
```

### 2. SecureChat
A secure chat application with web interface demonstrating:
- Web-based chat UI (embedded HTML/JS)
- Password storage using Cloudflare Secrets
- Timing-safe authentication
- User management via PowerShell scripts
- D1 database for messages and sessions
- KV namespace for session lookups
- Optional R2 storage per user
- Optional message encryption
- CORS handling
- RESTful API

**To run:**
```bash
cd SecureChat
npm install

# Initialize D1 database
npx wrangler d1 create secure-chat-db
npx wrangler d1 execute secure-chat-db --local --file=./schema.sql

# Create users (REQUIRED - no self-registration)
.\scripts\add-user.ps1 -Username alice -Password "SecurePass123!"
.\scripts\add-user.ps1 -Username bob -Password "AnotherPass456!" -WithStorage

# Build and run
dotnet fable . --outDir dist
npx wrangler dev
```

See [SECURITY.md](SecureChat/SECURITY.md) for security details.

## Prerequisites

1. **Fable**: Install the Fable compiler
   ```bash
   dotnet tool install fable --global
   ```

2. **Wrangler**: Cloudflare's CLI tool
   ```bash
   npm install -g wrangler
   ```

3. **Cloudflare Account**: Sign up at [cloudflare.com](https://cloudflare.com)

## Building Samples

Each sample follows the same build pattern:

1. **Compile F# to JavaScript**:
   ```bash
   dotnet fable . --outDir dist
   ```

2. **Run locally**:
   ```bash
   npx wrangler dev
   ```

3. **Deploy to Cloudflare**:
   ```bash
   npx wrangler deploy
   ```

## Configuration

### KV Namespaces
Create a KV namespace:
```bash
npx wrangler kv:namespace create "MY_KV"
```

Update `wrangler.toml` with the returned ID:
```toml
[[kv_namespaces]]
binding = "MY_KV"
id = "your-kv-namespace-id"
```

### D1 Databases
Create a D1 database:
```bash
npx wrangler d1 create my-database
```

Update `wrangler.toml` with the returned ID:
```toml
[[d1_databases]]
binding = "DB"
database_name = "my-database"
database_id = "your-database-id"
```

## API Endpoints (SecureChat)

### Authentication
- `POST /api/register` - Create new user account
- `POST /api/login` - Login and receive session token

### Chat Operations
- `POST /api/rooms` - Create a new chat room
- `POST /api/messages` - Send a message to a room
- `GET /api/messages/{roomId}` - Get messages from a room

### Request Examples

**Register:**
```bash
curl -X POST http://localhost:8787/api/register \
  -H "Content-Type: application/json" \
  -d '{"username":"user1","password":"pass123"}'
```

**Send Message:**
```bash
curl -X POST http://localhost:8787/api/messages \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{"roomId":"room123","content":"Hello!"}'
```

## Development Tips

1. **Hot Reload**: Use `dotnet fable watch` for automatic recompilation
2. **Debugging**: Use `wrangler tail` to view live logs
3. **Local Storage**: Wrangler provides local KV and D1 emulation
4. **TypeScript Definitions**: Generate with Glutinum for additional APIs

## Troubleshooting

### Common Issues

1. **Module not found errors**: Ensure you run `dotnet fable` before `wrangler dev`
2. **KV/D1 binding errors**: Check that your namespace/database IDs are correct
3. **CORS errors**: The samples include CORS headers, but adjust for your domain

### Getting Help

- CloudflareFS Issues: [GitHub Issues](https://github.com/cloudflarefs/cloudflarefs/issues)
- Cloudflare Workers Docs: [developers.cloudflare.com](https://developers.cloudflare.com/workers)
- F# Community: [fsharp.org](https://fsharp.org)

## License

These samples are part of CloudflareFS and are dual-licensed under MIT OR Apache-2.0.