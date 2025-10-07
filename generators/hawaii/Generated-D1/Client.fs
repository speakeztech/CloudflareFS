namespace rec CloudFlare.Management.D1

open System.Net
open System.Net.Http
open System.Text
open System.Threading
open CloudFlare.Management.D1.Types
open CloudFlare.Management.D1.Http

///D1 Database Management API
type D1Client(httpClient: HttpClient) =
    ///<summary>
    ///Returns a list of D1 databases.
    ///</summary>
    member this.CloudflareD1ListDatabases
        (
            accountId: string,
            ?name: string,
            ?page: float,
            ?perPage: float,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  if name.IsSome then
                      RequestPart.query ("name", name.Value)
                  if page.IsSome then
                      RequestPart.query ("page", page.Value)
                  if perPage.IsSome then
                      RequestPart.query ("per_page", perPage.Value) ]

            let! (status, content) =
                OpenApiHttp.getAsync httpClient "/accounts/{account_id}/d1/database" requestParts cancellationToken

            return CloudflareD1ListDatabases.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Returns the created D1 database.
    ///</summary>
    member this.CloudflareD1CreateDatabase
        (
            accountId: string,
            body: CloudflareD1CreateDatabasePayload,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync httpClient "/accounts/{account_id}/d1/database" requestParts cancellationToken

            return CloudflareD1CreateDatabase.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Deletes the specified D1 database.
    ///</summary>
    member this.CloudflareD1DeleteDatabase
        (
            accountId: string,
            databaseId: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("database_id", databaseId) ]

            let! (status, content) =
                OpenApiHttp.deleteAsync
                    httpClient
                    "/accounts/{account_id}/d1/database/{database_id}"
                    requestParts
                    cancellationToken

            return CloudflareD1DeleteDatabase.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Returns the specified D1 database.
    ///</summary>
    member this.CloudflareD1GetDatabase(accountId: string, databaseId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("database_id", databaseId) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/d1/database/{database_id}"
                    requestParts
                    cancellationToken

            return CloudflareD1GetDatabase.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Updates partially the specified D1 database.
    ///</summary>
    member this.CloudflareD1UpdatePartialDatabase
        (
            accountId: string,
            databaseId: string,
            body: d1databaseupdatepartialrequestbody,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("database_id", databaseId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.patchAsync
                    httpClient
                    "/accounts/{account_id}/d1/database/{database_id}"
                    requestParts
                    cancellationToken

            return CloudflareD1UpdatePartialDatabase.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Updates the specified D1 database.
    ///</summary>
    member this.CloudflareD1UpdateDatabase
        (
            accountId: string,
            databaseId: string,
            body: d1databaseupdaterequestbody,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("database_id", databaseId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.putAsync
                    httpClient
                    "/accounts/{account_id}/d1/database/{database_id}"
                    requestParts
                    cancellationToken

            return CloudflareD1UpdateDatabase.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Returns the query result as an object.
    ///</summary>
    member this.CloudflareD1QueryDatabase
        (
            accountId: string,
            databaseId: string,
            body: CloudflareD1QueryDatabasePayload,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("database_id", databaseId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/d1/database/{database_id}/query"
                    requestParts
                    cancellationToken

            return CloudflareD1QueryDatabase.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Returns the query result rows as arrays rather than objects. This is a performance-optimized version of the /query endpoint.
    ///</summary>
    member this.CloudflareD1RawDatabaseQuery
        (
            accountId: string,
            databaseId: string,
            body: CloudflareD1RawDatabaseQueryPayload,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.path ("database_id", databaseId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/d1/database/{database_id}/raw"
                    requestParts
                    cancellationToken

            return CloudflareD1RawDatabaseQuery.OK(Serializer.deserialize content)
        }
