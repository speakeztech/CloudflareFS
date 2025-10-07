module CloudFlare.Tests.CoreTests

open Expecto
open CloudFlare.Core

let tests =
    testList "CloudFlare.Core Tests" [

        testList "Version Tests" [
            testCase "Version has semantic format" <| fun _ ->
                let version = Version.current
                let parts = version.Split('.')
                Expect.isGreaterThanOrEqual parts.Length 2 "Version should have at least major.minor"
                Expect.all parts (fun p -> System.Int32.TryParse(p) |> fst) "All parts should be numeric"
        ]

        testList "Configuration Tests" [
            testCase "Can set and get configuration values" <| fun _ ->
                Configuration.set "test-key" "test-value"
                let result = Configuration.tryGet "test-key"
                Expect.equal result (Some "test-value") "Should retrieve set value"

            testCase "tryGet returns None for missing keys" <| fun _ ->
                let result = Configuration.tryGet "non-existent-key-12345"
                Expect.isNone result "Should return None for missing key"

            testCase "get throws for missing keys" <| fun _ ->
                Expect.throws
                    (fun () -> Configuration.get "missing-key-67890" |> ignore)
                    "Should throw for missing key"

            testCase "getOrDefault returns default for missing keys" <| fun _ ->
                let result = Configuration.getOrDefault "missing-key" "default-value"
                Expect.equal result "default-value" "Should return default for missing key"

            testCase "getOrDefault returns value for existing keys" <| fun _ ->
                Configuration.set "existing-key" "actual-value"
                let result = Configuration.getOrDefault "existing-key" "default-value"
                Expect.equal result "actual-value" "Should return actual value, not default"

            testCase "Configuration updates existing keys" <| fun _ ->
                Configuration.set "update-key" "initial"
                Configuration.set "update-key" "updated"
                let result = Configuration.get "update-key"
                Expect.equal result "updated" "Should update existing key"
        ]
    ]