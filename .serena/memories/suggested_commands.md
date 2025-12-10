# Suggested Commands

## Building the Solution
```bash
# Build entire solution
cd /home/hhh/repos/CloudflareFS
dotnet build

# Build specific project
dotnet build src/Runtime/CloudFlare.D1

# Build release
dotnet build -c Release
```

## Running Tests
```bash
# Run all tests
dotnet test

# Or use the test script
./run-tests.sh       # Linux/macOS
.\run-tests.ps1      # Windows
```

## Code Formatting
```bash
# Format all code with Fantomas
dotnet fantomas . --recurse
```

## Generation Pipeline

### Runtime Bindings (TypeScript to F#)
```bash
cd generators/glutinum
npx @glutinum/cli generate \
    ./node_modules/@cloudflare/workers-types/index.d.ts \
    --output ../../src/Runtime/CloudFlare.Worker.Context/Generated.fs
```

### Management APIs (OpenAPI to F#)
```bash
cd generators/hawaii

# 1. Extract service-specific specs from 15.5MB OpenAPI
dotnet fsi extract-services.fsx

# 2. Generate F# clients
hawaii --config d1-hawaii.json

# 3. Apply post-processing (for Workers)
dotnet fsi post-process-discriminators.fsx ../../src/Management/CloudFlare.Management.Workers/Types.fs
```

### Full Workers Generation
```bash
cd generators/hawaii
./generate-workers.ps1
```

## Sample Applications

### HelloWorker
```bash
cd samples/HelloWorker
dotnet fable . --outDir dist
npx wrangler dev
```

### SecureChat API
```bash
cd samples/SecureChat
# Add users via PowerShell
./scripts/add-user.ps1 -Username alice -Password "Pass123!"
# Build and run
dotnet fable . --outDir dist
npx wrangler dev
```

### SecureChat UI
```bash
cd samples/SecureChat.UI
npm install
npm run fable  # Starts dev server
```

## Deployment
```bash
# Deploy Worker
npx wrangler deploy

# Deploy Pages
npm run build
npx wrangler pages deploy dist
```

## Prerequisites
```bash
# .NET SDK 8.0+
dotnet --version

# Node.js 18+
node --version

# Fable compiler
dotnet tool install fable --global

# Wrangler CLI
npm install -g wrangler

# Glutinum CLI
npm install -g @glutinum/cli

# Hawaii
dotnet tool install -g hawaii
```
