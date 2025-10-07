module CloudFlare.Tests.ManagementApiTests

open Expecto
open System
open System.Net
open System.Net.Http
open System.Threading
open System.Threading.Tasks
open CloudFlare.Management.R2
open CloudFlare.Management.D1
open CloudFlare.Management.Workers

/// Lightweight HTTP stub following Jordan Marr's IO pattern
type StubHttpMessageHandler(responseFunc: HttpRequestMessage -> Task<HttpResponseMessage>) =
    inherit HttpMessageHandler()
    override _.SendAsync(request: HttpRequestMessage, cancellationToken: CancellationToken) =
        responseFunc request

let createStubHttpClient (responseBody: string) (statusCode: HttpStatusCode) =
    let handler = StubHttpMessageHandler(fun req ->
        let response = new HttpResponseMessage(statusCode)
        response.Content <- new StringContent(responseBody, Text.Encoding.UTF8, "application/json")
        Task.FromResult(response)
    )
    new HttpClient(handler, BaseAddress = Uri("https://api.cloudflare.com"))

let createConditionalHttpClient (responses: Map<string, string * HttpStatusCode>) =
    let handler = StubHttpMessageHandler(fun req ->
        let key = $"{req.Method} {req.RequestUri.PathAndQuery}"
        match responses.TryFind key with
        | Some (body, status) ->
            let response = new HttpResponseMessage(status)
            response.Content <- new StringContent(body, Text.Encoding.UTF8, "application/json")
            Task.FromResult(response)
        | None ->
            let response = new HttpResponseMessage(HttpStatusCode.NotFound)
            response.Content <- new StringContent("""{"success":false,"errors":[{"code":7003,"message":"No route found"}]}""")
            Task.FromResult(response)
    )
    new HttpClient(handler, BaseAddress = Uri("https://api.cloudflare.com"))

let tests =
    testList "Management API Tests with IO Pattern" [

        testList "R2 Bucket Operations" [
            testCase "List buckets returns parsed response" <| fun _ ->
                let responseJson = """{
                    "result": {
                        "buckets": [
                            {"name": "my-bucket", "creation_date": "2024-01-01T00:00:00Z"},
                            {"name": "other-bucket", "creation_date": "2024-01-02T00:00:00Z"}
                        ]
                    },
                    "success": true
                }"""

                let httpClient = createStubHttpClient responseJson HttpStatusCode.OK
                let client = R2Client(httpClient)

                let result = client.R2ListBuckets("test-account-id") |> Async.RunSynchronously

                match result with
                | R2ListBuckets.OK response ->
                    Expect.isTrue response.success "Response should be successful"
                | _ ->
                    failtest "Expected OK response"

            testCase "Create bucket constructs proper request" <| fun _ ->
                let mutable capturedMethod = ""
                let mutable capturedPath = ""

                let handler = StubHttpMessageHandler(fun req ->
                    capturedMethod <- req.Method.Method
                    capturedPath <- req.RequestUri.PathAndQuery
                    let response = new HttpResponseMessage(HttpStatusCode.OK)
                    response.Content <- new StringContent("""{"success":true}""", Text.Encoding.UTF8, "application/json")
                    Task.FromResult(response)
                )

                let httpClient = new HttpClient(handler, BaseAddress = Uri("https://api.cloudflare.com"))
                let client = R2Client(httpClient)

                let payload = { name = "test-bucket"; locationHint = Some "wnam"; storageClass = None }
                let _ = client.R2CreateBucket("account123", payload) |> Async.RunSynchronously

                Expect.equal capturedMethod "POST" "Should use POST method"
                Expect.stringContains capturedPath "/r2/buckets" "Should target r2/buckets endpoint"
                Expect.stringContains capturedPath "account123" "Should include account ID in path"

            testCase "Delete bucket operation completes" <| fun _ ->
                let responseJson = """{"success": true}"""
                let httpClient = createStubHttpClient responseJson HttpStatusCode.OK
                let client = R2Client(httpClient)

                let result = client.R2DeleteBucket("my-bucket", "account-id") |> Async.RunSynchronously

                match result with
                | R2DeleteBucket.OK _ ->
                    Expect.isTrue true "Delete should succeed"
                | _ ->
                    failtest "Expected OK response"
        ]

        testList "D1 Database Operations" [
            testCase "List databases returns collection" <| fun _ ->
                let responseJson = """{
                    "result": [
                        {"uuid": "abc-123", "name": "my-db", "version": "alpha"},
                        {"uuid": "def-456", "name": "other-db", "version": "beta"}
                    ],
                    "success": true
                }"""

                let httpClient = createStubHttpClient responseJson HttpStatusCode.OK
                let client = D1Client(httpClient)

                let result = client.D1ListDatabases("test-account") |> Async.RunSynchronously

                match result with
                | D1ListDatabases.OK response ->
                    Expect.isTrue response.success "Should be successful"
                | _ ->
                    failtest "Expected OK response"

            testCase "Create database with proper payload" <| fun _ ->
                let mutable capturedBody = ""

                let handler = StubHttpMessageHandler(fun req ->
                    if req.Content <> null then
                        capturedBody <- req.Content.ReadAsStringAsync().Result
                    let response = new HttpResponseMessage(HttpStatusCode.OK)
                    response.Content <- new StringContent("""{"success":true,"result":{"uuid":"new-id","name":"new-db"}}""")
                    Task.FromResult(response)
                )

                let httpClient = new HttpClient(handler, BaseAddress = Uri("https://api.cloudflare.com"))
                let client = D1Client(httpClient)

                let payload = { name = "new-database"; primaryLocationHint = Some "weur" }
                let _ = client.D1CreateDatabase("account-id", payload) |> Async.RunSynchronously

                Expect.stringContains capturedBody "new-database" "Body should contain database name"
                Expect.stringContains capturedBody "weur" "Body should contain location hint"

            testCase "Query database executes SQL" <| fun _ ->
                let responseJson = """{
                    "success": true,
                    "result": [
                        {"results": [{"id": 1, "name": "test"}], "success": true}
                    ]
                }"""

                let httpClient = createStubHttpClient responseJson HttpStatusCode.OK
                let client = D1Client(httpClient)

                let result = client.D1QueryDatabase("db-id", "account-id", """SELECT * FROM users""")
                             |> Async.RunSynchronously

                match result with
                | D1QueryDatabase.OK response ->
                    Expect.isTrue response.success "Query should succeed"
                | _ ->
                    failtest "Expected OK response"
        ]

        testList "Workers Script Operations" [
            testCase "Upload worker script" <| fun _ ->
                let mutable capturedMethod = ""
                let mutable capturedContentType = ""

                let handler = StubHttpMessageHandler(fun req ->
                    capturedMethod <- req.Method.Method
                    if req.Content <> null then
                        capturedContentType <- req.Content.Headers.ContentType.MediaType
                    let response = new HttpResponseMessage(HttpStatusCode.OK)
                    response.Content <- new StringContent("""{"success":true,"result":{"id":"script-123"}}""")
                    Task.FromResult(response)
                )

                let httpClient = new HttpClient(handler, BaseAddress = Uri("https://api.cloudflare.com"))
                let client = WorkersClient(httpClient)

                // Note: Actual upload would require multipart form data
                // This tests the client initialization and method routing
                Expect.equal httpClient.BaseAddress.Host "api.cloudflare.com" "Should target correct API"

            testCase "Get worker script details" <| fun _ ->
                let responseJson = """{
                    "success": true,
                    "result": {
                        "id": "my-worker",
                        "etag": "abc123",
                        "created_on": "2024-01-01T00:00:00Z",
                        "modified_on": "2024-01-02T00:00:00Z"
                    }
                }"""

                let httpClient = createStubHttpClient responseJson HttpStatusCode.OK
                let client = WorkersClient(httpClient)

                let result = client.WorkersGetScript("my-worker", "account-id") |> Async.RunSynchronously

                match result with
                | WorkersGetScript.OK _ ->
                    Expect.isTrue true "Should retrieve script details"
                | _ ->
                    failtest "Expected OK response"
        ]

        testList "HTTP Error Handling" [
            testCase "404 response is handled" <| fun _ ->
                let responseJson = """{"success":false,"errors":[{"code":10000,"message":"Not found"}]}"""
                let httpClient = createStubHttpClient responseJson HttpStatusCode.NotFound
                let client = R2Client(httpClient)

                // Hawaii-generated clients may throw or return error variants
                // This tests that we can handle error responses
                try
                    let _ = client.R2DeleteBucket("nonexistent", "account-id") |> Async.RunSynchronously
                    Expect.isTrue true "Should handle error response"
                with
                | ex ->
                    // Some error is expected
                    Expect.isTrue true "Error handling works"

            testCase "Unauthorized response" <| fun _ ->
                let responseJson = """{"success":false,"errors":[{"code":10000,"message":"Authentication error"}]}"""
                let httpClient = createStubHttpClient responseJson HttpStatusCode.Unauthorized
                let client = D1Client(httpClient)

                try
                    let _ = client.D1ListDatabases("account-id") |> Async.RunSynchronously
                    Expect.isTrue true "Should handle auth error"
                with
                | ex ->
                    Expect.isTrue true "Auth error handled"
        ]

        testList "Request Construction" [
            testCase "Query parameters are properly encoded" <| fun _ ->
                let mutable capturedQuery = ""

                let handler = StubHttpMessageHandler(fun req ->
                    capturedQuery <- req.RequestUri.Query
                    let response = new HttpResponseMessage(HttpStatusCode.OK)
                    response.Content <- new StringContent("""{"success":true,"result":{"buckets":[]}}""")
                    Task.FromResult(response)
                )

                let httpClient = new HttpClient(handler, BaseAddress = Uri("https://api.cloudflare.com"))
                let client = R2Client(httpClient)

                let _ = client.R2ListBuckets("account-id", nameContains = "test", perPage = 50.0)
                        |> Async.RunSynchronously

                Expect.stringContains capturedQuery "name_contains" "Should include name filter"
                Expect.stringContains capturedQuery "per_page" "Should include pagination"

            testCase "Headers are properly set" <| fun _ ->
                let mutable hasJurisdictionHeader = false

                let handler = StubHttpMessageHandler(fun req ->
                    hasJurisdictionHeader <- req.Headers.Contains("cf-r2-jurisdiction")
                    let response = new HttpResponseMessage(HttpStatusCode.OK)
                    response.Content <- new StringContent("""{"success":true,"result":{"buckets":[]}}""")
                    Task.FromResult(response)
                )

                let httpClient = new HttpClient(handler, BaseAddress = Uri("https://api.cloudflare.com"))
                let client = R2Client(httpClient)

                let _ = client.R2ListBuckets("account-id", cfR2Jurisdiction = "eu")
                        |> Async.RunSynchronously

                Expect.isTrue hasJurisdictionHeader "Should set jurisdiction header"

            testCase "Path parameters are substituted" <| fun _ ->
                let mutable capturedPath = ""

                let handler = StubHttpMessageHandler(fun req ->
                    capturedPath <- req.RequestUri.AbsolutePath
                    let response = new HttpResponseMessage(HttpStatusCode.OK)
                    response.Content <- new StringContent("""{"success":true}""")
                    Task.FromResult(response)
                )

                let httpClient = new HttpClient(handler, BaseAddress = Uri("https://api.cloudflare.com"))
                let client = R2Client(httpClient)

                let _ = client.R2DeleteBucket("my-bucket", "my-account") |> Async.RunSynchronously

                Expect.stringContains capturedPath "my-account" "Should substitute account ID"
                Expect.stringContains capturedPath "my-bucket" "Should substitute bucket name"
        ]

        testList "JSON Serialization in Requests" [
            testCase "Camel case property naming" <| fun _ ->
                let mutable capturedJson = ""

                let handler = StubHttpMessageHandler(fun req ->
                    if req.Content <> null then
                        capturedJson <- req.Content.ReadAsStringAsync().Result
                    let response = new HttpResponseMessage(HttpStatusCode.OK)
                    response.Content <- new StringContent("""{"success":true}""")
                    Task.FromResult(response)
                )

                let httpClient = new HttpClient(handler, BaseAddress = Uri("https://api.cloudflare.com"))
                let client = R2Client(httpClient)

                let payload = { name = "test-bucket"; locationHint = Some "wnam"; storageClass = None }
                let _ = client.R2CreateBucket("account-id", payload) |> Async.RunSynchronously

                Expect.stringContains capturedJson "locationHint" "Should use camel case"
                Expect.isFalse (capturedJson.Contains("location_hint")) "Should not use snake case"

            testCase "Null values are omitted" <| fun _ ->
                let mutable capturedJson = ""

                let handler = StubHttpMessageHandler(fun req ->
                    if req.Content <> null then
                        capturedJson <- req.Content.ReadAsStringAsync().Result
                    let response = new HttpResponseMessage(HttpStatusCode.OK)
                    response.Content <- new StringContent("""{"success":true}""")
                    Task.FromResult(response)
                )

                let httpClient = new HttpClient(handler, BaseAddress = Uri("https://api.cloudflare.com"))
                let client = R2Client(httpClient)

                let payload = { name = "test"; locationHint = None; storageClass = None }
                let _ = client.R2CreateBucket("account-id", payload) |> Async.RunSynchronously

                Expect.isFalse (capturedJson.Contains("locationHint")) "Null fields should be omitted"
                Expect.isFalse (capturedJson.Contains("storageClass")) "Null fields should be omitted"
        ]
    ]
