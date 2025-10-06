namespace rec CloudFlare.Management.Hyperdrive

open System.Net
open System.Net.Http
open System.Text
open System.Threading
open CloudFlare.Management.Hyperdrive.Types
open CloudFlare.Management.Hyperdrive.Http

///Database Connection Pooling API
type HyperdriveClient(httpClient: HttpClient) =
    ///<summary>
    ///Returns a list of Hyperdrives.
    ///</summary>
    member this.ListHyperdrive(accountId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/hyperdrive/configs"
                    requestParts
                    cancellationToken

            return ListHyperdrive.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Creates and returns a new Hyperdrive configuration.
    ///</summary>
    member this.CreateHyperdrive
        (
            accountId: string,
            body: hyperdrivehyperdriveconfig,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/hyperdrive/configs"
                    requestParts
                    cancellationToken

            return CreateHyperdrive.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Deletes the specified Hyperdrive.
    ///</summary>
    member this.DeleteHyperdrive(accountId: string, hyperdriveId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("hyperdrive_id", hyperdriveId) ]

            let! (status, content) =
                OpenApiHttp.deleteAsync
                    httpClient
                    "/accounts/{account_id}/hyperdrive/configs/{hyperdrive_id}"
                    requestParts
                    cancellationToken

            return DeleteHyperdrive.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Returns the specified Hyperdrive configuration.
    ///</summary>
    member this.GetHyperdrive(accountId: string, hyperdriveId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("hyperdrive_id", hyperdriveId) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/hyperdrive/configs/{hyperdrive_id}"
                    requestParts
                    cancellationToken

            return GetHyperdrive.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Patches and returns the specified Hyperdrive configuration. Custom caching settings are not kept if caching is disabled.
    ///</summary>
    member this.PatchHyperdrive
        (
            accountId: string,
            hyperdriveId: string,
            body: hyperdrivehyperdriveconfigpatch,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("hyperdrive_id", hyperdriveId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.patchAsync
                    httpClient
                    "/accounts/{account_id}/hyperdrive/configs/{hyperdrive_id}"
                    requestParts
                    cancellationToken

            return PatchHyperdrive.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Updates and returns the specified Hyperdrive configuration.
    ///</summary>
    member this.UpdateHyperdrive
        (
            accountId: string,
            hyperdriveId: string,
            body: hyperdrivehyperdriveconfig,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("hyperdrive_id", hyperdriveId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.putAsync
                    httpClient
                    "/accounts/{account_id}/hyperdrive/configs/{hyperdrive_id}"
                    requestParts
                    cancellationToken

            return UpdateHyperdrive.OK(Serializer.deserialize content)
        }
