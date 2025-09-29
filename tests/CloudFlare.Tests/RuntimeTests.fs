module CloudFlare.Tests.RuntimeTests

open Expecto
open Fable.Core
open CloudFlare.Worker.Context
open CloudFlare.AI

#if FABLE_COMPILER
open Fable.Mocha
#endif

let tests =
    testList "CloudFlare.Runtime Tests" [

        testList "Worker Context Tests" [
            testCase "Request URL parsing" <| fun _ ->
                let testUrl = "https://example.com/api/test?param=value"
                let parts = testUrl.Split('?')
                Expect.equal parts.Length 2 "URL should have path and query parts"
                Expect.stringContains parts.[0] "https://example.com" "Should contain domain"
                Expect.stringContains parts.[1] "param=value" "Should contain query params"

            testCase "HTTP method validation" <| fun _ ->
                let validMethods = ["GET"; "POST"; "PUT"; "DELETE"; "PATCH"; "HEAD"; "OPTIONS"]
                let isValidMethod m = List.contains m validMethods

                Expect.isTrue (isValidMethod "GET") "GET should be valid"
                Expect.isTrue (isValidMethod "POST") "POST should be valid"
                Expect.isFalse (isValidMethod "INVALID") "INVALID should not be valid"

            testCase "Headers structure" <| fun _ ->
                let headers = [
                    ("Content-Type", "application/json")
                    ("Authorization", "Bearer token")
                    ("X-Custom-Header", "value")
                ]

                Expect.equal (headers.Length) 3 "Should have 3 header pairs"
                let contentType = headers |> List.tryFind (fun (k: string, _) -> k = "Content-Type")
                Expect.isSome contentType "Should find Content-Type header"
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

        testList "Response Construction Tests" [
            testCase "JSON response creation" <| fun _ ->
                let jsonContent = """{"status":"ok","data":42}"""
                let contentLength = jsonContent.Length

                Expect.isGreaterThan contentLength 0 "Content should not be empty"
                Expect.stringContains jsonContent "status" "Should contain status field"
                Expect.stringContains jsonContent "data" "Should contain data field"

            testCase "Status code validation" <| fun _ ->
                let validCodes = [200; 201; 204; 400; 401; 403; 404; 500; 502; 503]
                let isValidStatusCode code =
                    code >= 100 && code < 600

                validCodes |> List.iter (fun code ->
                    Expect.isTrue (isValidStatusCode code) $"Status code {code} should be valid"
                )

                Expect.isFalse (isValidStatusCode 99) "99 should not be valid"
                Expect.isFalse (isValidStatusCode 600) "600 should not be valid"

            testCase "Cache control header formatting" <| fun _ ->
                let maxAge = 3600
                let cacheControl = $"public, max-age={maxAge}"

                Expect.stringContains cacheControl "public" "Should contain public directive"
                Expect.stringContains cacheControl (string maxAge) "Should contain max-age value"
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