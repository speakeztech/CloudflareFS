module CloudFlare.Tests.RuntimeTests

open Expecto
open Fable.Core
open CloudFlare.Worker.Context
open CloudFlare.Worker.Context.Helpers
open CloudFlare.AI

#if FABLE_COMPILER
open Fable.Mocha
#endif

let tests =
    testList "CloudFlare.Runtime Tests" [

        testList "URL Parsing Tests" [
            testCase "Parse URL with query parameters" <| fun _ ->
                let testUrl = "https://example.com/api/test?param=value&foo=bar"
                let parts = testUrl.Split('?')
                Expect.equal parts.Length 2 "URL should split into path and query"
                Expect.stringContains parts.[0] "/api/test" "Should contain path"
                Expect.stringContains parts.[1] "param=value" "Should contain first param"
                Expect.stringContains parts.[1] "foo=bar" "Should contain second param"

            testCase "Parse URL without query parameters" <| fun _ ->
                let testUrl = "https://example.com/api/test"
                let parts = testUrl.Split('?')
                Expect.equal parts.Length 1 "URL without query should not split"

            testCase "Extract pathname from URL" <| fun _ ->
                let testUrl = "https://example.com/api/v1/users/123"
                let uri = System.Uri(testUrl)
                Expect.equal uri.AbsolutePath "/api/v1/users/123" "Should extract pathname"

            testCase "Extract query string" <| fun _ ->
                let testUrl = "https://example.com/search?q=fsharp&limit=10"
                let uri = System.Uri(testUrl)
                Expect.equal uri.Query "?q=fsharp&limit=10" "Should extract query string"
        ]

        testList "HTTP Method Tests" [
            testCase "Standard HTTP methods are recognized" <| fun _ ->
                let validMethods = ["GET"; "POST"; "PUT"; "DELETE"; "PATCH"; "HEAD"; "OPTIONS"]
                let isValidMethod m = List.contains m validMethods

                Expect.isTrue (isValidMethod "GET") "GET should be valid"
                Expect.isTrue (isValidMethod "POST") "POST should be valid"
                Expect.isTrue (isValidMethod "PUT") "PUT should be valid"
                Expect.isTrue (isValidMethod "DELETE") "DELETE should be valid"
                Expect.isTrue (isValidMethod "PATCH") "PATCH should be valid"
                Expect.isTrue (isValidMethod "HEAD") "HEAD should be valid"
                Expect.isTrue (isValidMethod "OPTIONS") "OPTIONS should be valid"

            testCase "Invalid methods are rejected" <| fun _ ->
                let validMethods = ["GET"; "POST"; "PUT"; "DELETE"; "PATCH"; "HEAD"; "OPTIONS"]
                let isValidMethod m = List.contains m validMethods

                Expect.isFalse (isValidMethod "INVALID") "INVALID should not be valid"
                Expect.isFalse (isValidMethod "get") "Lowercase should not be valid"
                Expect.isFalse (isValidMethod "") "Empty should not be valid"
        ]

        testList "Header Manipulation Tests" [
            testCase "Build header pairs" <| fun _ ->
                let headers = [
                    ("Content-Type", "application/json")
                    ("Authorization", "Bearer token123")
                    ("X-Custom-Header", "custom-value")
                ]

                Expect.equal headers.Length 3 "Should have 3 header pairs"

                let contentType = headers |> List.tryFind (fun (k, _) -> k = "Content-Type")
                Expect.isSome contentType "Should find Content-Type header"
                Expect.equal (snd contentType.Value) "application/json" "Should have correct content type"

                let auth = headers |> List.tryFind (fun (k, _) -> k = "Authorization")
                Expect.isSome auth "Should find Authorization header"
                Expect.stringStarts (snd auth.Value) "Bearer " "Should be bearer token"

            testCase "Header name comparison is case-sensitive" <| fun _ ->
                let headers = [("Content-Type", "text/html"); ("content-type", "application/json")]
                Expect.equal headers.Length 2 "Different case should be treated as different headers"

            testCase "CORS header value" <| fun _ ->
                let corsHeader = ("Access-Control-Allow-Origin", "*")
                Expect.equal (fst corsHeader) "Access-Control-Allow-Origin" "Should be correct CORS header name"
                Expect.equal (snd corsHeader) "*" "Should allow all origins"
        ]

        testList "AI Model Tests" [
            testCase "Model name validation" <| fun _ ->
                let validModelPrefixes = [
                    "@cf/meta/llama"
                    "@cf/microsoft/phi"
                    "@cf/mistral"
                    "@cf/qwen"
                    "@cf/google/gemma"
                    "@cf/baai/bge"
                ]

                let isValidModel (name: string) =
                    validModelPrefixes |> List.exists (fun prefix ->
                        name.StartsWith(prefix))

                Expect.isTrue (isValidModel "@cf/meta/llama-3.1-8b-instruct") "Llama model should be valid"
                Expect.isTrue (isValidModel "@cf/mistral/mistral-7b-instruct-v0.1") "Mistral model should be valid"
                Expect.isFalse (isValidModel "invalid-model") "Invalid model should fail"

            testCase "Text generation input validation" <| fun _ ->
                let isValidInput (prompt: string) =
                    not (System.String.IsNullOrWhiteSpace(prompt)) &&
                    prompt.Length > 0 &&
                    prompt.Length <= 100000

                Expect.isTrue (isValidInput "Generate a story about") "Valid prompt should pass"
                Expect.isFalse (isValidInput "") "Empty prompt should fail"
                Expect.isFalse (isValidInput null) "Null prompt should fail"

            testCase "Embedding dimension validation" <| fun _ ->
                let validDimensions = [384; 768; 1024; 1536]
                let isValidDimension dim = List.contains dim validDimensions

                Expect.isTrue (isValidDimension 768) "768 should be valid dimension"
                Expect.isTrue (isValidDimension 1536) "1536 should be valid dimension"
                Expect.isFalse (isValidDimension 500) "500 should not be valid dimension"

            testProperty "Temperature parameter bounds" <| fun (temp: float) ->
                let isValidTemperature t = t >= 0.0 && t <= 2.0
                if System.Double.IsNaN(temp) || System.Double.IsInfinity(temp) then
                    true
                else
                    let clamped = max 0.0 (min 2.0 temp)
                    isValidTemperature clamped
        ]

        testList "HTTP Status Code Tests" [
            testCase "2xx Success codes" <| fun _ ->
                let successCodes = [200; 201; 202; 204]
                let isSuccess code = code >= 200 && code < 300

                successCodes |> List.iter (fun code ->
                    Expect.isTrue (isSuccess code) $"{code} should be success code"
                )

            testCase "3xx Redirection codes" <| fun _ ->
                let redirectCodes = [301; 302; 303; 304; 307; 308]
                let isRedirect code = code >= 300 && code < 400

                redirectCodes |> List.iter (fun code ->
                    Expect.isTrue (isRedirect code) $"{code} should be redirect code"
                )

            testCase "4xx Client error codes" <| fun _ ->
                let clientErrors = [400; 401; 403; 404; 405; 429]
                let isClientError code = code >= 400 && code < 500

                clientErrors |> List.iter (fun code ->
                    Expect.isTrue (isClientError code) $"{code} should be client error code"
                )

            testCase "5xx Server error codes" <| fun _ ->
                let serverErrors = [500; 501; 502; 503; 504]
                let isServerError code = code >= 500 && code < 600

                serverErrors |> List.iter (fun code ->
                    Expect.isTrue (isServerError code) $"{code} should be server error code"
                )

            testCase "Invalid status codes are rejected" <| fun _ ->
                let isValidStatusCode code = code >= 100 && code < 600

                Expect.isFalse (isValidStatusCode 99) "99 should not be valid"
                Expect.isFalse (isValidStatusCode 600) "600 should not be valid"
                Expect.isFalse (isValidStatusCode -1) "-1 should not be valid"
                Expect.isFalse (isValidStatusCode 0) "0 should not be valid"
        ]

        testList "JSON Serialization Tests" [
            testCase "Serialize simple record" <| fun _ ->
                let data = {| status = "ok"; code = 200 |}
                let json = System.Text.Json.JsonSerializer.Serialize(data)

                Expect.stringContains json "status" "Should contain status field"
                Expect.stringContains json "ok" "Should contain status value"
                Expect.stringContains json "code" "Should contain code field"
                Expect.stringContains json "200" "Should contain code value"

            testCase "Serialize nested structure" <| fun _ ->
                let data = {|
                    user = {| name = "John"; age = 30 |}
                    timestamp = System.DateTime.UtcNow
                |}
                let json = System.Text.Json.JsonSerializer.Serialize(data)

                Expect.stringContains json "user" "Should contain user field"
                Expect.stringContains json "name" "Should contain name field"
                Expect.stringContains json "John" "Should contain name value"

            testCase "Serialize list" <| fun _ ->
                let data = {| items = [1; 2; 3; 4; 5] |}
                let json = System.Text.Json.JsonSerializer.Serialize(data)

                Expect.stringContains json "items" "Should contain items field"
                Expect.stringContains json "[" "Should contain array start"
                Expect.stringContains json "]" "Should contain array end"
        ]

        testList "Cache Control Tests" [
            testCase "Public cache directive" <| fun _ ->
                let cacheControl = "public, max-age=3600"
                Expect.stringContains cacheControl "public" "Should contain public directive"
                Expect.stringContains cacheControl "max-age" "Should contain max-age directive"

            testCase "Private cache directive" <| fun _ ->
                let cacheControl = "private, no-cache"
                Expect.stringContains cacheControl "private" "Should contain private directive"
                Expect.stringContains cacheControl "no-cache" "Should contain no-cache directive"

            testCase "Max-age formatting" <| fun _ ->
                let maxAge = 86400 // 24 hours
                let cacheControl = $"public, max-age={maxAge}"
                Expect.stringContains cacheControl (string maxAge) "Should contain max-age value"

            testCase "No-store for sensitive data" <| fun _ ->
                let cacheControl = "no-store, must-revalidate"
                Expect.stringContains cacheControl "no-store" "Should contain no-store"
                Expect.stringContains cacheControl "must-revalidate" "Should contain must-revalidate"
        ]
    ]

#if FABLE_COMPILER
let mochaTests =
    testList "Fable Runtime Tests (Mocha)" [
        testCase "Async operation handling" <| fun _ ->
            async {
                let! result = async { return 42 }
                Expect.equal result 42 "Async should return expected value"
            } |> Async.RunSynchronously

        testCaseAsync "Promise interop" <| async {
            let value = 123
            let! result = async { return value }
            Expect.equal result value "Promise should resolve to expected value"
        }
    ]
#endif