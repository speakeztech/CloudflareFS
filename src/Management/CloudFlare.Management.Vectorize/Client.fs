namespace rec CloudFlare.Management.Vectorize

open System.Net
open System.Net.Http
open System.Text
open System.Threading
open CloudFlare.Management.Vectorize.Types
open CloudFlare.Management.Vectorize.Http

///Vector Database Management API (V2)
type VectorizeClient(httpClient: HttpClient) =
    ///<summary>
    ///Returns a list of Vectorize Indexes
    ///</summary>
    member this.VectorizeListVectorizeIndexes(accountId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/vectorize/v2/indexes"
                    requestParts
                    cancellationToken

            return VectorizeListVectorizeIndexes.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Creates and returns a new Vectorize Index.
    ///</summary>
    member this.VectorizeCreateVectorizeIndex
        (
            accountId: string,
            body: ``vectorizecreate-index-request``,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/vectorize/v2/indexes"
                    requestParts
                    cancellationToken

            return VectorizeCreateVectorizeIndex.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Deletes the specified Vectorize Index.
    ///</summary>
    member this.VectorizeDeleteVectorizeIndex
        (
            accountId: string,
            indexName: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("index_name", indexName) ]

            let! (status, content) =
                OpenApiHttp.deleteAsync
                    httpClient
                    "/accounts/{account_id}/vectorize/v2/indexes/{index_name}"
                    requestParts
                    cancellationToken

            return VectorizeDeleteVectorizeIndex.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Returns the specified Vectorize Index.
    ///</summary>
    member this.VectorizeGetVectorizeIndex
        (
            accountId: string,
            indexName: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("index_name", indexName) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/vectorize/v2/indexes/{index_name}"
                    requestParts
                    cancellationToken

            return VectorizeGetVectorizeIndex.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Delete a set of vectors from an index by their vector identifiers.
    ///</summary>
    member this.VectorizeDeleteVectorsById
        (
            accountId: string,
            indexName: string,
            body: ``vectorizeindex-delete-vectors-by-id-request``,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("index_name", indexName)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/delete_by_ids"
                    requestParts
                    cancellationToken

            return VectorizeDeleteVectorsById.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Get a set of vectors from an index by their vector identifiers.
    ///</summary>
    member this.VectorizeGetVectorsById
        (
            accountId: string,
            indexName: string,
            body: ``vectorizeindex-get-vectors-by-id-request``,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("index_name", indexName)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/get_by_ids"
                    requestParts
                    cancellationToken

            return VectorizeGetVectorsById.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Get information about a vectorize index.
    ///</summary>
    member this.VectorizeIndexInfo(accountId: string, indexName: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("index_name", indexName) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/info"
                    requestParts
                    cancellationToken

            return VectorizeIndexInfo.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Inserts vectors into the specified index and returns a mutation id corresponding to the vectors enqueued for insertion.
    ///</summary>
    member this.VectorizeInsertVector
        (
            accountId: string,
            indexName: string,
            ?unparsableBehavior: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("index_name", indexName)
                  if unparsableBehavior.IsSome then
                      RequestPart.query ("unparsable-behavior", unparsableBehavior.Value) ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/insert"
                    requestParts
                    cancellationToken

            return VectorizeInsertVector.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Returns a paginated list of vector identifiers from the specified index.
    ///</summary>
    member this.VectorizeListVectors
        (
            accountId: string,
            indexName: string,
            ?count: int,
            ?cursor: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("index_name", indexName)
                  if count.IsSome then
                      RequestPart.query ("count", count.Value)
                  if cursor.IsSome then
                      RequestPart.query ("cursor", cursor.Value) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/list"
                    requestParts
                    cancellationToken

            return VectorizeListVectors.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Enable metadata filtering based on metadata property. Limited to 10 properties.
    ///</summary>
    member this.VectorizeCreateMetadataIndex
        (
            accountId: string,
            indexName: string,
            body: ``vectorizecreate-metadata-index-request``,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("index_name", indexName)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/metadata_index/create"
                    requestParts
                    cancellationToken

            return VectorizeCreateMetadataIndex.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Allow Vectorize to delete the specified metadata index.
    ///</summary>
    member this.VectorizeDeleteMetadataIndex
        (
            accountId: string,
            indexName: string,
            body: ``vectorizedelete-metadata-index-request``,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("index_name", indexName)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/metadata_index/delete"
                    requestParts
                    cancellationToken

            return VectorizeDeleteMetadataIndex.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///List Metadata Indexes for the specified Vectorize Index.
    ///</summary>
    member this.VectorizeListMetadataIndexes
        (
            accountId: string,
            indexName: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("index_name", indexName) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/metadata_index/list"
                    requestParts
                    cancellationToken

            return VectorizeListMetadataIndexes.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Finds vectors closest to a given vector in an index.
    ///</summary>
    member this.VectorizeQueryVector
        (
            accountId: string,
            indexName: string,
            body: ``vectorizeindex-query-v2-request``,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("index_name", indexName)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/query"
                    requestParts
                    cancellationToken

            return VectorizeQueryVector.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Upserts vectors into the specified index, creating them if they do not exist and returns a mutation id corresponding to the vectors enqueued for upsertion.
    ///</summary>
    member this.VectorizeUpsertVector
        (
            accountId: string,
            indexName: string,
            ?unparsableBehavior: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("index_name", indexName)
                  if unparsableBehavior.IsSome then
                      RequestPart.query ("unparsable-behavior", unparsableBehavior.Value) ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/vectorize/v2/indexes/{index_name}/upsert"
                    requestParts
                    cancellationToken

            return VectorizeUpsertVector.OK(Serializer.deserialize content)
        }
