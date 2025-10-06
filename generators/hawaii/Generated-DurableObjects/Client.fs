namespace rec CloudFlare.Management.DurableObjects

open System.Net
open System.Net.Http
open System.Text
open System.Threading
open CloudFlare.Management.DurableObjects.Types
open CloudFlare.Management.DurableObjects.Http

///Durable Objects Management API
type DurableObjectsClient(httpClient: HttpClient) =
    ///<summary>
    ///Returns the Durable Object namespaces owned by an account.
    ///</summary>
    ///<param name="accountId"></param>
    ///<param name="page">Current page.</param>
    ///<param name="perPage">Items per-page.</param>
    ///<param name="cancellationToken"></param>
    member this.DurableObjectsNamespaceListNamespaces
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
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/workers/durable_objects/namespaces"
                    requestParts
                    cancellationToken

            return DurableObjectsNamespaceListNamespaces.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Returns the Durable Objects in a given namespace.
    ///</summary>
    member this.DurableObjectsNamespaceListObjects
        (
            accountId: string,
            id: string,
            ?limit: float,
            ?cursor: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("id", id)
                  if limit.IsSome then
                      RequestPart.query ("limit", limit.Value)
                  if cursor.IsSome then
                      RequestPart.query ("cursor", cursor.Value) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/workers/durable_objects/namespaces/{id}/objects"
                    requestParts
                    cancellationToken

            return DurableObjectsNamespaceListObjects.OK(Serializer.deserialize content)
        }
