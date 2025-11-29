namespace rec CloudFlare.Management.Pages

open System.Net
open System.Net.Http
open System.Text
open System.Threading
open CloudFlare.Management.Pages.Types
open CloudFlare.Management.Pages.Http

///Cloudflare Pages Management API - Create and deploy static sites
type PagesClient(httpClient: HttpClient) =
    ///<summary>
    ///Fetch a list of all user projects.
    ///</summary>
    member this.PagesProjectGetProjects
        (
            accountId: string,
            ?page: int,
            ?perPage: int,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  if page.IsSome then
                      RequestPart.query ("page", page.Value)
                  if perPage.IsSome then
                      RequestPart.query ("per_page", perPage.Value) ]

            let! (status, content) =
                OpenApiHttp.getAsync httpClient "/accounts/{account_id}/pages/projects" requestParts cancellationToken

            return PagesProjectGetProjects.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Create a new project.
    ///</summary>
    member this.PagesProjectCreateProject
        (
            accountId: string,
            body: PagesProjectCreateProjectPayload,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync httpClient "/accounts/{account_id}/pages/projects" requestParts cancellationToken

            return PagesProjectCreateProject.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Delete a project by name.
    ///</summary>
    member this.PagesProjectDeleteProject
        (
            projectName: string,
            accountId: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("project_name", projectName)
                  RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.deleteAsync
                    httpClient
                    "/accounts/{account_id}/pages/projects/{project_name}"
                    requestParts
                    cancellationToken

            return PagesProjectDeleteProject.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Fetch a project by name.
    ///</summary>
    member this.PagesProjectGetProject(projectName: string, accountId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("project_name", projectName)
                  RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/pages/projects/{project_name}"
                    requestParts
                    cancellationToken

            return PagesProjectGetProject.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Set new attributes for an existing project. Modify environment variables. To delete an environment variable, set the key to null.
    ///</summary>
    member this.PagesProjectUpdateProject
        (
            projectName: string,
            accountId: string,
            body: PagesProjectUpdateProjectPayload,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("project_name", projectName)
                  RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.patchAsync
                    httpClient
                    "/accounts/{account_id}/pages/projects/{project_name}"
                    requestParts
                    cancellationToken

            return PagesProjectUpdateProject.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Fetch a list of project deployments.
    ///</summary>
    member this.PagesDeploymentGetDeployments
        (
            projectName: string,
            accountId: string,
            ?env: string,
            ?page: int,
            ?perPage: int,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("project_name", projectName)
                  RequestPart.path ("account_id", accountId)
                  if env.IsSome then
                      RequestPart.query ("env", env.Value)
                  if page.IsSome then
                      RequestPart.query ("page", page.Value)
                  if perPage.IsSome then
                      RequestPart.query ("per_page", perPage.Value) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/pages/projects/{project_name}/deployments"
                    requestParts
                    cancellationToken

            return PagesDeploymentGetDeployments.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Start a new deployment from production. The repository and account must have already been authorized on the Cloudflare Pages dashboard.
    ///</summary>
    ///<param name="projectName"></param>
    ///<param name="accountId"></param>
    ///<param name="cancellationToken"></param>
    ///<param name="wranglerConfigHash">Hash of the Wrangler configuration file used for this deployment.</param>
    ///<param name="pagesBuildOutputDir">The build output directory path.</param>
    ///<param name="manifest">
    ///JSON string containing a manifest of files to deploy. Maps file paths to their content hashes.
    ///Required for direct upload deployments. Maximum 20,000 entries.
    ///</param>
    ///<param name="functionsFilepathRoutingConfigJson">Functions routing configuration file.</param>
    ///<param name="commitMessage">Git commit message associated with this deployment.</param>
    ///<param name="commitHash">Git commit SHA associated with this deployment.</param>
    ///<param name="commitDirty">Boolean string indicating if the working directory has uncommitted changes.</param>
    ///<param name="branch">The branch to build the new deployment from. The `HEAD` of the branch will be used. If omitted, the production branch will be used by default.</param>
    ///<param name="workerJs">
    ///Worker JavaScript file. Mutually exclusive with `_worker.bundle`.
    ///Cannot specify both `_worker.js` and `_worker.bundle` in the same request.
    ///</param>
    ///<param name="workerBundle">
    ///Worker bundle file in multipart/form-data format. Mutually exclusive with `_worker.js`.
    ///Cannot specify both `_worker.js` and `_worker.bundle` in the same request.
    ///Maximum size: 25 MiB.
    ///</param>
    ///<param name="routesJson">Routes configuration file defining routing rules.</param>
    ///<param name="redirects">Redirects configuration file for the deployment.</param>
    ///<param name="headers">Headers configuration file for the deployment.</param>
    member this.PagesDeploymentCreateDeployment
        (
            projectName: string,
            accountId: string,
            ?cancellationToken: CancellationToken,
            ?wranglerConfigHash: string,
            ?pagesBuildOutputDir: string,
            ?manifest: string,
            ?functionsFilepathRoutingConfigJson: string,
            ?commitMessage: string,
            ?commitHash: string,
            ?commitDirty: string,
            ?branch: string,
            ?workerJs: string,
            ?workerBundle: string,
            ?routesJson: string,
            ?redirects: string,
            ?headers: string
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("project_name", projectName)
                  RequestPart.path ("account_id", accountId)
                  if wranglerConfigHash.IsSome then
                      RequestPart.multipartFormData ("wrangler_config_hash", wranglerConfigHash.Value)
                  if pagesBuildOutputDir.IsSome then
                      RequestPart.multipartFormData ("pages_build_output_dir", pagesBuildOutputDir.Value)
                  if manifest.IsSome then
                      RequestPart.multipartFormData ("manifest", manifest.Value)
                  if functionsFilepathRoutingConfigJson.IsSome then
                      RequestPart.multipartFormData (
                          "functions-filepath-routing-config.json",
                          functionsFilepathRoutingConfigJson.Value
                      )
                  if commitMessage.IsSome then
                      RequestPart.multipartFormData ("commit_message", commitMessage.Value)
                  if commitHash.IsSome then
                      RequestPart.multipartFormData ("commit_hash", commitHash.Value)
                  if commitDirty.IsSome then
                      RequestPart.multipartFormData ("commit_dirty", commitDirty.Value)
                  if branch.IsSome then
                      RequestPart.multipartFormData ("branch", branch.Value)
                  if workerJs.IsSome then
                      RequestPart.multipartFormData ("_worker.js", workerJs.Value)
                  if workerBundle.IsSome then
                      RequestPart.multipartFormData ("_worker.bundle", workerBundle.Value)
                  if routesJson.IsSome then
                      RequestPart.multipartFormData ("_routes.json", routesJson.Value)
                  if redirects.IsSome then
                      RequestPart.multipartFormData ("_redirects", redirects.Value)
                  if headers.IsSome then
                      RequestPart.multipartFormData ("_headers", headers.Value) ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/pages/projects/{project_name}/deployments"
                    requestParts
                    cancellationToken

            return PagesDeploymentCreateDeployment.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Delete a deployment.
    ///</summary>
    member this.PagesDeploymentDeleteDeployment
        (
            deploymentId: string,
            projectName: string,
            accountId: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("deployment_id", deploymentId)
                  RequestPart.path ("project_name", projectName)
                  RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.deleteAsync
                    httpClient
                    "/accounts/{account_id}/pages/projects/{project_name}/deployments/{deployment_id}"
                    requestParts
                    cancellationToken

            return PagesDeploymentDeleteDeployment.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Fetch information about a deployment.
    ///</summary>
    member this.PagesDeploymentGetDeploymentInfo
        (
            deploymentId: string,
            projectName: string,
            accountId: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("deployment_id", deploymentId)
                  RequestPart.path ("project_name", projectName)
                  RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/pages/projects/{project_name}/deployments/{deployment_id}"
                    requestParts
                    cancellationToken

            return PagesDeploymentGetDeploymentInfo.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Fetch deployment logs for a project.
    ///</summary>
    member this.PagesDeploymentGetDeploymentLogs
        (
            deploymentId: string,
            projectName: string,
            accountId: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("deployment_id", deploymentId)
                  RequestPart.path ("project_name", projectName)
                  RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/pages/projects/{project_name}/deployments/{deployment_id}/history/logs"
                    requestParts
                    cancellationToken

            return PagesDeploymentGetDeploymentLogs.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Retry a previous deployment.
    ///</summary>
    member this.PagesDeploymentRetryDeployment
        (
            deploymentId: string,
            projectName: string,
            accountId: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("deployment_id", deploymentId)
                  RequestPart.path ("project_name", projectName)
                  RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/pages/projects/{project_name}/deployments/{deployment_id}/retry"
                    requestParts
                    cancellationToken

            return PagesDeploymentRetryDeployment.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Rollback the production deployment to a previous deployment. You can only rollback to succesful builds on production.
    ///</summary>
    member this.PagesDeploymentRollbackDeployment
        (
            deploymentId: string,
            projectName: string,
            accountId: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("deployment_id", deploymentId)
                  RequestPart.path ("project_name", projectName)
                  RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/pages/projects/{project_name}/deployments/{deployment_id}/rollback"
                    requestParts
                    cancellationToken

            return PagesDeploymentRollbackDeployment.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Fetch a list of all domains associated with a Pages project.
    ///</summary>
    member this.PagesDomainsGetDomains(projectName: string, accountId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("project_name", projectName)
                  RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/pages/projects/{project_name}/domains"
                    requestParts
                    cancellationToken

            return PagesDomainsGetDomains.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Add a new domain for the Pages project.
    ///</summary>
    member this.PagesDomainsAddDomain
        (
            projectName: string,
            accountId: string,
            body: PagesDomainsAddDomainPayload,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("project_name", projectName)
                  RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/pages/projects/{project_name}/domains"
                    requestParts
                    cancellationToken

            return PagesDomainsAddDomain.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Delete a Pages project's domain.
    ///</summary>
    member this.PagesDomainsDeleteDomain
        (
            domainName: string,
            projectName: string,
            accountId: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("domain_name", domainName)
                  RequestPart.path ("project_name", projectName)
                  RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.deleteAsync
                    httpClient
                    "/accounts/{account_id}/pages/projects/{project_name}/domains/{domain_name}"
                    requestParts
                    cancellationToken

            return PagesDomainsDeleteDomain.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Fetch a single domain.
    ///</summary>
    member this.PagesDomainsGetDomain
        (
            domainName: string,
            projectName: string,
            accountId: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("domain_name", domainName)
                  RequestPart.path ("project_name", projectName)
                  RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/pages/projects/{project_name}/domains/{domain_name}"
                    requestParts
                    cancellationToken

            return PagesDomainsGetDomain.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Retry the validation status of a single domain.
    ///</summary>
    member this.PagesDomainsPatchDomain
        (
            domainName: string,
            projectName: string,
            accountId: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("domain_name", domainName)
                  RequestPart.path ("project_name", projectName)
                  RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.patchAsync
                    httpClient
                    "/accounts/{account_id}/pages/projects/{project_name}/domains/{domain_name}"
                    requestParts
                    cancellationToken

            return PagesDomainsPatchDomain.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Purge all cached build artifacts for a Pages project
    ///</summary>
    member this.PagesPurgeBuildCache(projectName: string, accountId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("project_name", projectName)
                  RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/pages/projects/{project_name}/purge_build_cache"
                    requestParts
                    cancellationToken

            return PagesPurgeBuildCache.OK(Serializer.deserialize content)
        }
