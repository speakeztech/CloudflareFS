# Codebase Structure

## Root Directory
```
/home/hhh/repos/CloudflareFS/
├── src/                      # Main source code
├── generators/               # Code generation tools
├── samples/                  # Example applications
├── tests/                    # Test projects
├── docs/                     # Architecture documentation
├── build/                    # Build scripts
├── nuget/                    # NuGet package configuration
├── patches/                  # Third-party patches
├── CloudflareFS.sln          # Solution file
├── README.md                 # Project readme
├── CONTRIBUTING.md           # Contribution guidelines
├── PROJECT_STATUS.md         # Implementation status
└── LICENSE-*                 # Dual Apache 2.0 / MIT license
```

## Source Code (/src/)
```
src/
├── Core/                     # Shared utilities and types
│   └── CloudFlare.Core/
│
├── Runtime/                  # In-Worker APIs (Glutinum-generated)
│   ├── CloudFlare.Worker.Context/  # Core Worker types (Request, Response, Headers)
│   ├── CloudFlare.D1/              # D1 database bindings
│   ├── CloudFlare.R2/              # R2 object storage bindings
│   ├── CloudFlare.KV/              # KV storage bindings
│   ├── CloudFlare.AI/              # Workers AI bindings
│   ├── CloudFlare.Queues/          # Message queue bindings
│   ├── CloudFlare.Vectorize/       # Vector database bindings
│   ├── CloudFlare.Hyperdrive/      # Database connection pooling
│   └── CloudFlare.DurableObjects/  # Stateful serverless compute
│
└── Management/               # REST APIs (Hawaii-generated)
    ├── CloudFlare.Management.Workers/     # Worker deployment and config
    ├── CloudFlare.Management.D1/          # D1 database management
    ├── CloudFlare.Management.R2/          # R2 bucket management
    ├── CloudFlare.Management.Analytics/   # Analytics API client
    ├── CloudFlare.Management.Queues/      # Queue management
    ├── CloudFlare.Management.Vectorize/   # Vector index management (V2)
    ├── CloudFlare.Management.Hyperdrive/  # Connection config management
    ├── CloudFlare.Management.DurableObjects/ # Namespace management
    └── CloudFlare.Management.Pages/       # Pages project management
```

## Generators (/generators/)
```
generators/
├── glutinum/                 # TypeScript to F# generation
│   ├── glutinum.json         # Configuration
│   └── generate-runtime.ps1
│
└── hawaii/                   # OpenAPI to F# generation
    ├── extract-services.fsx  # Splits 15.5MB spec into service-specific specs
    ├── *-hawaii.json         # Per-service configurations
    ├── post-process-discriminators.fsx  # DU generation post-processor
    ├── post-process-jobject.fsx         # JObject fixes
    └── generate-*.ps1        # Generation scripts
```

## Samples (/samples/)
```
samples/
├── HelloWorker/              # Basic Worker example
├── SecureChat/               # Production chat API with D1, Secrets
└── SecureChat.UI/            # React frontend with Tailwind CSS
```

## Documentation (/docs/)
Key documents:
- `00_architecture_decisions.md` - Key design choices
- `01_generation_strategy.md` - Glutinum vs Hawaii code generation
- `02_dual_layer_architecture.md` - Runtime vs Management APIs
- `03_openapi_generation.md` - Hawaii setup and OpenAPI handling
- `08_conversion_patterns.md` - TypeScript to F# patterns
- `09_tool_improvement_analysis.md` - Tool enhancement roadmap
