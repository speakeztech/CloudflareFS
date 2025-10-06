namespace rec CloudFlare.Api.Compute.Workers

open System.Net
open System.Net.Http
open System.Text
open System.Threading
open CloudFlare.Api.Compute.Workers.Types
open CloudFlare.Api.Compute.Workers.Http

///Workers Script Management API
type WorkersClient(httpClient: HttpClient) =
    ///<summary>
    ///Fetch a list of uploaded workers.
    ///</summary>
    ///<param name="accountId"></param>
    ///<param name="tags">Filter scripts by tags. Format: comma-separated list of tag:allowed pairs where allowed is 'yes' or 'no'.</param>
    ///<param name="cancellationToken"></param>
    member this.WorkerScriptListWorkers(accountId: string, ?tags: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  if tags.IsSome then
                      RequestPart.query ("tags", tags.Value) ]

            let! (status, content) =
                OpenApiHttp.getAsync httpClient "/accounts/{account_id}/workers/scripts" requestParts cancellationToken

            return WorkerScriptListWorkers.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Delete your worker. This call has no response body on a successful delete.
    ///</summary>
    ///<param name="accountId"></param>
    ///<param name="scriptName"></param>
    ///<param name="force">If set to true, delete will not be stopped by associated service binding, durable object, or other binding. Any of these associated bindings/durable objects will be deleted along with the script.</param>
    ///<param name="cancellationToken"></param>
    member this.WorkerScriptDeleteWorker
        (
            accountId: string,
            scriptName: string,
            ?force: bool,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("script_name", scriptName)
                  if force.IsSome then
                      RequestPart.query ("force", force.Value) ]

            let! (status, content) =
                OpenApiHttp.deleteAsync
                    httpClient
                    "/accounts/{account_id}/workers/scripts/{script_name}"
                    requestParts
                    cancellationToken

            return WorkerScriptDeleteWorker.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Fetch raw script content for your worker. Note this is the original script content, not JSON encoded.
    ///</summary>
    member this.WorkerScriptDownloadWorker
        (
            accountId: string,
            scriptName: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("script_name", scriptName) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/workers/scripts/{script_name}"
                    requestParts
                    cancellationToken

            return WorkerScriptDownloadWorker.OK
        }

    ///<summary>
    ///Upload a worker module. You can find more about the multipart metadata on our docs: https://developers.cloudflare.com/workers/configuration/multipart-upload-metadata/.
    ///</summary>
    member this.WorkerScriptUploadWorkerModule
        (
            accountId: string,
            scriptName: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("script_name", scriptName) ]

            let! (status, content) =
                OpenApiHttp.putAsync
                    httpClient
                    "/accounts/{account_id}/workers/scripts/{script_name}"
                    requestParts
                    cancellationToken

            return WorkerScriptUploadWorkerModule.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Put script content without touching config or metadata.
    ///</summary>
    ///<param name="accountId"></param>
    ///<param name="scriptName"></param>
    ///<param name="metadata">JSON-encoded metadata about the uploaded parts and Worker configuration.</param>
    ///<param name="cFWORKERBODYPART">The multipart name of a script upload part containing script content in service worker format. Alternative to including in a metadata part.</param>
    ///<param name="cFWORKERMAINMODULEPART">The multipart name of a script upload part containing script content in es module format. Alternative to including in a metadata part.</param>
    ///<param name="cancellationToken"></param>
    ///<param name="files">An array of modules (often JavaScript files) comprising a Worker script. At least one module must be present and referenced in the metadata as `main_module` or `body_part` by filename.&amp;lt;br/&amp;gt;Possible Content-Type(s) are: `application/javascript+module`, `text/javascript+module`, `application/javascript`, `text/javascript`, `text/x-python`, `text/x-python-requirement`, `application/wasm`, `text/plain`, `application/octet-stream`, `application/source-map`.</param>
    member this.WorkerScriptPutContent
        (
            accountId: string,
            scriptName: string,
            metadata: Newtonsoft.Json.Linq.JObject,
            ?cFWORKERBODYPART: string,
            ?cFWORKERMAINMODULEPART: string,
            ?cancellationToken: CancellationToken,
            ?files: list<string>
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("script_name", scriptName)
                  RequestPart.multipartFormData ("metadata", metadata.ToString(Newtonsoft.Json.Formatting.None))
                  if cFWORKERBODYPART.IsSome then
                      RequestPart.header ("CF-WORKER-BODY-PART", cFWORKERBODYPART.Value)
                  if cFWORKERMAINMODULEPART.IsSome then
                      RequestPart.header ("CF-WORKER-MAIN-MODULE-PART", cFWORKERMAINMODULEPART.Value)
                  if files.IsSome then
                      RequestPart.multipartFormData ("files", files.Value) ]

            let! (status, content) =
                OpenApiHttp.putAsync
                    httpClient
                    "/accounts/{account_id}/workers/scripts/{script_name}/content"
                    requestParts
                    cancellationToken

            return WorkerScriptPutContent.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Fetches Cron Triggers for a Worker.
    ///</summary>
    member this.WorkerCronTriggerGetCronTriggers
        (
            accountId: string,
            scriptName: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("script_name", scriptName) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/workers/scripts/{script_name}/schedules"
                    requestParts
                    cancellationToken

            return WorkerCronTriggerGetCronTriggers.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Updates Cron Triggers for a Worker.
    ///</summary>
    member this.WorkerCronTriggerUpdateCronTriggers
        (
            accountId: string,
            scriptName: string,
            body: WorkerCronTriggerUpdateCronTriggersPayload,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("script_name", scriptName)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.putAsync
                    httpClient
                    "/accounts/{account_id}/workers/scripts/{script_name}/schedules"
                    requestParts
                    cancellationToken

            return WorkerCronTriggerUpdateCronTriggers.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///List secrets bound to a script.
    ///</summary>
    member this.WorkerListScriptSecrets(accountId: string, scriptName: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("script_name", scriptName) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/workers/scripts/{script_name}/secrets"
                    requestParts
                    cancellationToken

            return WorkerListScriptSecrets.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Add a secret to a script.
    ///</summary>
    member this.WorkerPutScriptSecret(accountId: string, scriptName: string, body: Types.workersbindingkindsecrettext, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("script_name", scriptName)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.putAsync
                    httpClient
                    "/accounts/{account_id}/workers/scripts/{script_name}/secrets"
                    requestParts
                    cancellationToken

            return WorkerPutScriptSecret.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Get list of tails currently deployed on a Worker.
    ///</summary>
    member this.GetAccountsWorkersScriptsTailsByAccountIdAndScriptName
        (
            accountId: string,
            scriptName: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("script_name", scriptName) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/workers/scripts/{script_name}/tails"
                    requestParts
                    cancellationToken

            return GetAccountsWorkersScriptsTailsByAccountIdAndScriptName.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Starts a tail that receives logs and exception from a Worker.
    ///</summary>
    member this.WorkerTailLogsStartTail(accountId: string, scriptName: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("script_name", scriptName) ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/workers/scripts/{script_name}/tails"
                    requestParts
                    cancellationToken

            return WorkerTailLogsStartTail.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Fetches the Usage Model for a given Worker.
    ///</summary>
    member this.WorkerScriptFetchUsageModel
        (
            accountId: string,
            scriptName: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("script_name", scriptName) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/workers/scripts/{script_name}/usage-model"
                    requestParts
                    cancellationToken

            return WorkerScriptFetchUsageModel.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Updates the Usage Model for a given Worker. Requires a Workers Paid subscription.
    ///</summary>
    member this.WorkerScriptUpdateUsageModel
        (
            accountId: string,
            scriptName: string,
            body: WorkerScriptUpdateUsageModelPayload,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("script_name", scriptName)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.putAsync
                    httpClient
                    "/accounts/{account_id}/workers/scripts/{script_name}/usage-model"
                    requestParts
                    cancellationToken

            return WorkerScriptUpdateUsageModel.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Deletes a Workers subdomain for an account.
    ///</summary>
    member this.WorkerSubdomainDeleteSubdomain(accountId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.deleteAsync
                    httpClient
                    "/accounts/{account_id}/workers/subdomain"
                    requestParts
                    cancellationToken

            return WorkerSubdomainDeleteSubdomain.NoContent
        }

    ///<summary>
    ///Returns a Workers subdomain for an account.
    ///</summary>
    member this.WorkerSubdomainGetSubdomain(accountId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/workers/subdomain"
                    requestParts
                    cancellationToken

            return WorkerSubdomainGetSubdomain.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Creates a Workers subdomain for an account.
    ///</summary>
    member this.WorkerSubdomainCreateSubdomain
        (
            accountId: string,
            body: workersschemassubdomain,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.putAsync
                    httpClient
                    "/accounts/{account_id}/workers/subdomain"
                    requestParts
                    cancellationToken

            return WorkerSubdomainCreateSubdomain.OK(Serializer.deserialize content)
        }
