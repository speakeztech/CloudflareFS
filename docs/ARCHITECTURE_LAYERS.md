# CloudflareFS Architecture Layers

## Overview

CloudflareFS is designed as a layered architecture that can power multiple frontends - from CLI tools to desktop monitoring applications. Think of it as building the "Erlang Observer" for Cloudflare's platform.

## Architecture Layers

```
┌──────────────────────────────────────────────────────────┐
│                    Frontend Layer                        │
├──────────────────────────────────────────────────────────┤
│  cfs CLI Tool          │  Desktop Observer  │  VS Code   │
│  (Phase 1)             │  (Phase 2)         │  Extension │
└──────────────────────────────────────────────────────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────┐
│                 Orchestration Layer                      │
├──────────────────────────────────────────────────────────┤
│  Deployment       │  Monitoring      │  Configuration    │
│  Workflows        │  Aggregation     │  Management       │
│  • Resource       │  • Analytics     │  • TOML Gen       │
│  • Provisioning   │  • Logs          │  • Validation     │
│  • Rollbacks      │  • Metrics       │  • Migrations     │
└──────────────────────────────────────────────────────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────┐
│                    Core API Layer                        │
├──────────────────────────────────────────────────────────┤
│  Management APIs           │  Monitoring APIs            │
│  • Workers                 │  • Analytics                │
│  • KV/R2/D1               │  • Logs (tail)              │
│  • DNS                    │  • Metrics                  │
│  • Zero Trust             │  • Health                   │
│  (Hawaii-generated)       │  (Hawaii-generated)         │
└──────────────────────────────────────────────────────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────┐
│                   Transport Layer                        │
├──────────────────────────────────────────────────────────┤
│  HTTP Client     │  WebSocket      │  Event Streams     │
│  • Auth          │  • Log tailing  │  • Real-time       │
│  • Retry         │  • Live metrics │  • Analytics       │
│  • Rate limits   │                 │                    │
└──────────────────────────────────────────────────────────┘
```

## Layer Responsibilities

### 1. Core API Layer (Foundation)
```fsharp
// CloudFlare.Api.Core - Pure API bindings
namespace CloudFlare.Api.Core

type IKVClient =
    abstract ListNamespaces: accountId: string -> Async<KVNamespace[]>
    abstract CreateNamespace: accountId: string * title: string -> Async<KVNamespace>
    abstract DeleteNamespace: accountId: string * namespaceId: string -> Async<unit>

type IAnalyticsClient =
    abstract GetZoneAnalytics: zoneId: string * since: DateTime * until: DateTime -> Async<Analytics>
    abstract GetWorkerAnalytics: accountId: string * scriptName: string -> Async<WorkerAnalytics>

type ILogClient =
    abstract TailLogs: zoneId: string * ?filter: LogFilter -> IAsyncEnumerable<LogEntry>
    abstract GetLogpull: zoneId: string * fields: string[] -> AsyncSeq<LogRecord>
```

### 2. Orchestration Layer (Business Logic)
```fsharp
// CloudFlare.Orchestration - Higher-level operations
namespace CloudFlare.Orchestration

module Deployment =
    /// Deploy with automatic resource provisioning
    let deployWorker (client: ICloudflareClient) config = async {
        // 1. Provision resources
        let! resources = provisionResources client config.resources

        // 2. Compile/bundle script
        let! script = bundleScript config.source

        // 3. Deploy worker
        let! worker = client.Workers.Upload(config.accountId, config.name, {
            script = script
            bindings = resources.bindings
        })

        // 4. Configure routes
        do! configureRoutes client worker config.routes

        // 5. Set secrets
        do! setSecrets client worker config.secrets

        return worker
    }

    /// Generate TOML for offline deployment
    let generateToml config =
        let toml = TomlDocument()
        toml.["name"] <- config.name
        toml.["compatibility_date"] <- config.compatibilityDate

        config.bindings |> List.iter (function
            | KVBinding(name, id) ->
                toml.["kv_namespaces"].Add({| binding = name; id = id |})
            | R2Binding(name, bucket) ->
                toml.["r2_buckets"].Add({| binding = name; bucket_name = bucket |})
            | D1Binding(name, id) ->
                toml.["d1_databases"].Add({| binding = name; database_id = id |})
        )

        toml.ToString()

module Monitoring =
    /// Aggregate metrics from multiple sources
    let getSystemHealth (client: ICloudflareClient) accountId = async {
        let! (workers, analytics, errors) =
            Async.Parallel3(
                client.Workers.ListScripts(accountId),
                client.Analytics.GetAccountAnalytics(accountId, DateTime.Now.AddHours(-1), DateTime.Now),
                client.Logs.GetErrors(accountId, DateTime.Now.AddMinutes(-5))
            )

        return {
            workers = workers |> Array.map (fun w -> {
                name = w.name
                requests = analytics.GetRequestsForWorker(w.name)
                errors = errors |> Array.filter (fun e -> e.scriptName = w.name)
                cpu = analytics.GetCPUForWorker(w.name)
            })
            overall = {
                totalRequests = analytics.totalRequests
                errorRate = float errors.Length / float analytics.totalRequests
            }
        }
    }
```

### 3. Frontend Layer (User Interfaces)

#### CLI Tool (Phase 1)
```fsharp
// cfs CLI tool
module CloudFlareFS.CLI

open CloudFlare.Orchestration

[<EntryPoint>]
let main argv =
    match argv with
    | [| "deploy"; scriptPath |] ->
        // Execute F# script
        let config = FSI.execute scriptPath
        Deployment.deployWorker client config
        |> Async.RunSynchronously

    | [| "deploy"; "--offline"; scriptPath |] ->
        // Generate TOML instead of deploying
        let config = FSI.execute scriptPath
        let toml = Deployment.generateToml config
        File.WriteAllText("wrangler.toml", toml)
        printfn "Generated wrangler.toml"

    | [| "monitor"; accountId |] ->
        // Real-time monitoring
        Monitoring.getSystemHealth client accountId
        |> Async.RunSynchronously
        |> Display.table

    | _ -> showHelp()
```

#### Desktop Observer (Phase 2)
```fsharp
// CloudFlare.Observer - Desktop monitoring application
module CloudFlareFS.Observer

open Avalonia.FuncUI
open CloudFlare.Orchestration

type Model = {
    account: AccountInfo
    workers: WorkerStatus[]
    analytics: RealtimeAnalytics
    logs: LogEntry[]
}

let view model dispatch =
    DockPanel.create [
        // Top: Account selector and controls
        DockPanel.dock Dock.Top

        Grid.create [
            Grid.columnDefinitions "*, *, *"
            Grid.children [
                // Workers panel - like Erlang processes
                WorkersPanel.view model.workers dispatch

                // Analytics graphs - CPU, memory, requests
                AnalyticsPanel.view model.analytics dispatch

                // Log stream - real-time tail
                LogPanel.view model.logs dispatch
            ]
        ]
    ]

let update msg model =
    match msg with
    | WorkerSelected w ->
        { model with selectedWorker = Some w },
        Cmd.ofAsync Monitoring.getWorkerDetails client w

    | RefreshRequested ->
        model,
        Cmd.batch [
            Cmd.ofAsync Monitoring.getSystemHealth client model.account.id
            Cmd.ofAsync Monitoring.tailLogs client model.account.id
        ]
```

## Key Design Principles

### 1. Clean Separation
- **Core API**: Pure bindings, no business logic
- **Orchestration**: Business logic, workflows, aggregation
- **Frontend**: UI concerns only

### 2. Multiple Output Modes
```fsharp
type DeploymentOutput =
    | Direct of CloudflareClient        // Deploy directly via API
    | TomlFile of path: string          // Generate TOML
    | DryRun of DeploymentPlan         // Show what would happen
    | Terraform of TerraformConfig     // Generate Terraform (future)

let deploy output config =
    match output with
    | Direct client ->
        Deployment.deployWorker client config
    | TomlFile path ->
        let toml = Deployment.generateToml config
        File.WriteAllText(path, toml)
    | DryRun ->
        Deployment.planDeployment config
    | Terraform ->
        Deployment.generateTerraform config
```

### 3. Monitoring-First Design
```fsharp
// All APIs should support streaming/polling for monitoring
type MonitoringCapabilities = {
    // Pull-based (polling)
    getMetrics: unit -> Async<Metrics>
    getStatus: unit -> Async<Status>

    // Push-based (streaming)
    tailLogs: unit -> IAsyncEnumerable<LogEntry>
    streamMetrics: unit -> IObservable<Metric>

    // Aggregation
    getAnalytics: TimeRange -> Async<Analytics>
    getHealth: unit -> Async<HealthStatus>
}
```

## Implementation Phases

### Phase 1: CLI with Offline Support
```bash
# Direct deployment
cfs deploy ./infrastructure.fsx

# Generate TOML for CI/CD
cfs deploy ./infrastructure.fsx --offline --output wrangler.toml

# Monitor
cfs monitor my-worker --tail
cfs analytics my-worker --last 1h
```

### Phase 2: Desktop Observer
- Real-time worker monitoring
- Resource visualization (KV, R2, D1)
- Log streaming with filters
- Analytics dashboards
- Interactive management

### Phase 3: Advanced Features
- Multi-account support
- Cost tracking
- Performance profiling
- Deployment history
- Rollback capabilities

## Example: Deployment Script with Offline Option

```fsharp
// deploy.fsx
#r "nuget: CloudflareFS"

open CloudflareFS
open CloudflareFS.Orchestration

let config = {
    name = "my-api"
    accountId = getEnv "CF_ACCOUNT_ID"

    resources = {
        kv = ["cache"; "sessions"]
        r2 = ["uploads"]
        d1 = ["database"]
    }

    bindings = [
        KV "CACHE" "cache"
        KV "SESSIONS" "sessions"
        R2 "UPLOADS" "uploads"
        D1 "DB" "database"
    ]

    routes = [
        "api.example.com/*"
    ]

    secrets = [
        "API_KEY", getEnv "API_KEY"
        "DB_URL", getEnv "DATABASE_URL"
    ]
}

// This can either deploy directly OR generate TOML
match Environment.GetEnvironmentVariable("DEPLOY_MODE") with
| "offline" ->
    generateToml config |> writeFile "wrangler.toml"
    printfn "Generated wrangler.toml for offline deployment"
| _ ->
    deployWorker (CloudflareClient(apiToken)) config
    |> Async.RunSynchronously
    printfn "Deployed successfully"
```

## Monitoring Endpoints to Prioritize

For the Observer tool, focus on these "low hanging fruit" endpoints:

1. **Analytics** - `/zones/{id}/analytics/dashboard`
2. **Logs** - `/zones/{id}/logs/received`
3. **Health Checks** - `/zones/{id}/healthchecks`
4. **Rate Limiting** - `/zones/{id}/rate_limits/analytics`
5. **Workers Analytics** - `/accounts/{id}/workers/scripts/{name}/analytics`
6. **KV Operations** - `/accounts/{id}/storage/analytics`
7. **R2 Metrics** - `/accounts/{id}/r2/buckets/{name}/metrics`

These provide rich data for visualization without complex aggregation.

## Summary

This architecture ensures:
1. **Clean separation** for multiple frontends
2. **Offline deployment** via TOML generation
3. **Monitoring-first** API design
4. **Progressive enhancement** from CLI to GUI
5. **Type-safe** configuration and deployment

The same APIs power everything from simple CLI deployments to rich desktop monitoring, following the Erlang Observer pattern for Cloudflare's platform.