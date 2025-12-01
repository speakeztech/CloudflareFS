namespace AskAI.CLI

open System
open System.IO
open System.Text.Json

/// Configuration and state management for the Ask AI CLI
module Config =

    /// Cloudflare API configuration
    type CloudflareConfig = {
        ApiToken: string
        AccountId: string
    }

    /// Resource names for provisioning
    type ResourceNames = {
        WorkerName: string
        R2BucketName: string
        D1DatabaseName: string
        AutoRAGName: string
    }

    let defaultResourceNames = {
        WorkerName = "ask-ai"
        R2BucketName = "ask-ai-content"
        D1DatabaseName = "ask-ai-analytics"
        AutoRAGName = "ask-ai-rag"
    }

    /// Deployment scope determined by git diff analysis
    type DeploymentScope =
        | ContentOnly     // Only content changes - sync R2 (triggers AutoRAG re-index)
        | WorkerOnly      // Only worker changes - redeploy worker
        | FullDeploy      // Both content and worker changes
        | NoDeploy        // No relevant changes detected

    /// Deployment state persisted between runs
    type DeploymentState = {
        R2BucketCreated: bool
        D1DatabaseId: string option
        WorkerDeployed: bool
        WorkerUrl: string option
        LastDeployHash: string option
        LastSyncTimestamp: DateTime option
        LastDeployedCommit: string option
    }

    let emptyState = {
        R2BucketCreated = false
        D1DatabaseId = None
        WorkerDeployed = false
        WorkerUrl = None
        LastDeployHash = None
        LastSyncTimestamp = None
        LastDeployedCommit = None
    }

    /// Load configuration from environment variables
    let loadConfig () : Result<CloudflareConfig, string> =
        let apiToken = Environment.GetEnvironmentVariable("CLOUDFLARE_API_TOKEN")
        let accountId = Environment.GetEnvironmentVariable("CLOUDFLARE_ACCOUNT_ID")

        match apiToken, accountId with
        | null, _ -> Error "CLOUDFLARE_API_TOKEN environment variable not set"
        | _, null -> Error "CLOUDFLARE_ACCOUNT_ID environment variable not set"
        | token, account -> Ok { ApiToken = token; AccountId = account }

    let stateFilePath = ".ask-ai-deploy-state.json"

    /// Load deployment state from file
    let loadState () : DeploymentState =
        if File.Exists(stateFilePath) then
            try
                let json = File.ReadAllText(stateFilePath)
                JsonSerializer.Deserialize<DeploymentState>(json)
            with _ ->
                emptyState
        else
            emptyState

    /// Save deployment state to file
    let saveState (state: DeploymentState) : unit =
        let options = JsonSerializerOptions(WriteIndented = true)
        let json = JsonSerializer.Serialize(state, options)
        File.WriteAllText(stateFilePath, json)
