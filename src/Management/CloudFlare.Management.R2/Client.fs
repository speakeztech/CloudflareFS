namespace rec CloudFlare.Management.R2

open System.Net
open System.Net.Http
open System.Text
open System.Threading
open CloudFlare.Management.R2.Types
open CloudFlare.Management.R2.Http

///R2 Object Storage Management API
type R2Client(httpClient: HttpClient) =
    ///<summary>
    ///Lists all R2 buckets on your account.
    ///</summary>
    member this.R2ListBuckets
        (
            accountId: string,
            ?nameContains: string,
            ?startAfter: string,
            ?perPage: float,
            ?order: string,
            ?direction: string,
            ?cursor: string,
            ?cfR2Jurisdiction: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  if nameContains.IsSome then
                      RequestPart.query ("name_contains", nameContains.Value)
                  if startAfter.IsSome then
                      RequestPart.query ("start_after", startAfter.Value)
                  if perPage.IsSome then
                      RequestPart.query ("per_page", perPage.Value)
                  if order.IsSome then
                      RequestPart.query ("order", order.Value)
                  if direction.IsSome then
                      RequestPart.query ("direction", direction.Value)
                  if cursor.IsSome then
                      RequestPart.query ("cursor", cursor.Value)
                  if cfR2Jurisdiction.IsSome then
                      RequestPart.header ("cf-r2-jurisdiction", cfR2Jurisdiction.Value) ]

            let! (status, content) =
                OpenApiHttp.getAsync httpClient "/accounts/{account_id}/r2/buckets" requestParts cancellationToken

            return R2ListBuckets.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Creates a new R2 bucket.
    ///</summary>
    member this.R2CreateBucket
        (
            accountId: string,
            body: R2CreateBucketPayload,
            ?cfR2Jurisdiction: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body
                  if cfR2Jurisdiction.IsSome then
                      RequestPart.header ("cf-r2-jurisdiction", cfR2Jurisdiction.Value) ]

            let! (status, content) =
                OpenApiHttp.postAsync httpClient "/accounts/{account_id}/r2/buckets" requestParts cancellationToken

            return R2CreateBucket.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Deletes an existing R2 bucket.
    ///</summary>
    member this.R2DeleteBucket
        (
            bucketName: string,
            accountId: string,
            ?cfR2Jurisdiction: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("bucket_name", bucketName)
                  RequestPart.path ("account_id", accountId)
                  if cfR2Jurisdiction.IsSome then
                      RequestPart.header ("cf-r2-jurisdiction", cfR2Jurisdiction.Value) ]

            let! (status, content) =
                OpenApiHttp.deleteAsync
                    httpClient
                    "/accounts/{account_id}/r2/buckets/{bucket_name}"
                    requestParts
                    cancellationToken

            return R2DeleteBucket.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Gets properties of an existing R2 bucket.
    ///</summary>
    member this.R2GetBucket
        (
            accountId: string,
            bucketName: string,
            ?cfR2Jurisdiction: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("bucket_name", bucketName)
                  if cfR2Jurisdiction.IsSome then
                      RequestPart.header ("cf-r2-jurisdiction", cfR2Jurisdiction.Value) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/r2/buckets/{bucket_name}"
                    requestParts
                    cancellationToken

            return R2GetBucket.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Updates properties of an existing R2 bucket.
    ///</summary>
    member this.R2PatchBucket
        (
            accountId: string,
            bucketName: string,
            cfR2StorageClass: string,
            ?cfR2Jurisdiction: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("bucket_name", bucketName)
                  RequestPart.header ("cf-r2-storage-class", cfR2StorageClass)
                  if cfR2Jurisdiction.IsSome then
                      RequestPart.header ("cf-r2-jurisdiction", cfR2Jurisdiction.Value) ]

            let! (status, content) =
                OpenApiHttp.patchAsync
                    httpClient
                    "/accounts/{account_id}/r2/buckets/{bucket_name}"
                    requestParts
                    cancellationToken

            return R2PatchBucket.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Disables Sippy on this bucket.
    ///</summary>
    member this.R2DeleteBucketSippyConfig
        (
            bucketName: string,
            accountId: string,
            ?cfR2Jurisdiction: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("bucket_name", bucketName)
                  RequestPart.path ("account_id", accountId)
                  if cfR2Jurisdiction.IsSome then
                      RequestPart.header ("cf-r2-jurisdiction", cfR2Jurisdiction.Value) ]

            let! (status, content) =
                OpenApiHttp.deleteAsync
                    httpClient
                    "/accounts/{account_id}/r2/buckets/{bucket_name}/sippy"
                    requestParts
                    cancellationToken

            return R2DeleteBucketSippyConfig.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Gets configuration for Sippy for an existing R2 bucket.
    ///</summary>
    member this.R2GetBucketSippyConfig
        (
            accountId: string,
            bucketName: string,
            ?cfR2Jurisdiction: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("bucket_name", bucketName)
                  if cfR2Jurisdiction.IsSome then
                      RequestPart.header ("cf-r2-jurisdiction", cfR2Jurisdiction.Value) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/r2/buckets/{bucket_name}/sippy"
                    requestParts
                    cancellationToken

            return R2GetBucketSippyConfig.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Sets configuration for Sippy for an existing R2 bucket.
    ///</summary>
    member this.R2PutBucketSippyConfig
        (
            accountId: string,
            bucketName: string,
            ?cfR2Jurisdiction: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("bucket_name", bucketName)
                  if cfR2Jurisdiction.IsSome then
                      RequestPart.header ("cf-r2-jurisdiction", cfR2Jurisdiction.Value) ]

            let! (status, content) =
                OpenApiHttp.putAsync
                    httpClient
                    "/accounts/{account_id}/r2/buckets/{bucket_name}/sippy"
                    requestParts
                    cancellationToken

            return R2PutBucketSippyConfig.OK(Serializer.deserialize content)
        }
