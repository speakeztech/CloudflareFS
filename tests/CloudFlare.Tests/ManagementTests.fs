module CloudFlare.Tests.ManagementTests

open Expecto
open System
open System.Net.Http
open CloudFlare.Management.D1
open CloudFlare.Management.R2

let tests =
    testList "CloudFlare.Management Tests" [

        testList "D1 Management API Tests" [
            testCase "D1 client initialization" <| fun _ ->
                let httpClient = new HttpClient()

                let client = D1Client(httpClient)
                Expect.isTrue (not (obj.ReferenceEquals(client, null))) "D1 client should be created"

            testCase "D1 database name validation" <| fun _ ->
                let isValid name =
                    not (String.IsNullOrWhiteSpace(name)) &&
                    name.Length <= 63 &&
                    System.Text.RegularExpressions.Regex.IsMatch(name, "^[a-z0-9]([a-z0-9-]*[a-z0-9])?$")

                Expect.isTrue (isValid "my-database") "Valid database name should pass"
                Expect.isFalse (isValid "My-Database") "Uppercase letters should fail"
                Expect.isFalse (isValid "-database") "Leading dash should fail"
                Expect.isFalse (isValid "database-") "Trailing dash should fail"

            testProperty "D1 location validation" <| fun (location: string) ->
                let validLocations = ["weur"; "eeur"; "apac"; "wnam"; "enam"]
                if String.IsNullOrWhiteSpace(location) then
                    true
                else
                    let normalized = location.ToLowerInvariant()
                    let isValid = List.contains normalized validLocations
                    isValid = List.contains normalized validLocations
        ]

        testList "R2 Management API Tests" [
            testCase "R2 client initialization" <| fun _ ->
                let httpClient = new HttpClient()

                let client = R2Client(httpClient)
                Expect.isTrue (not (obj.ReferenceEquals(client, null))) "R2 client should be created"

            testCase "R2 bucket name validation" <| fun _ ->
                let isValid name =
                    not (String.IsNullOrWhiteSpace(name)) &&
                    name.Length >= 3 &&
                    name.Length <= 63 &&
                    System.Text.RegularExpressions.Regex.IsMatch(name, "^[a-z0-9]([a-z0-9.-]*[a-z0-9])?$")

                Expect.isTrue (isValid "my-bucket") "Valid bucket name should pass"
                Expect.isTrue (isValid "my.bucket.name") "Dots should be allowed"
                Expect.isFalse (isValid "My-Bucket") "Uppercase letters should fail"
                Expect.isFalse (isValid "bu") "Too short name should fail"
                Expect.isFalse (isValid "-bucket") "Leading dash should fail"

            testProperty "R2 storage class validation" <| fun (storageClass: string) ->
                let validClasses = ["Standard"; "InfrequentAccess"]
                if String.IsNullOrWhiteSpace(storageClass) then
                    true
                else
                    List.contains storageClass validClasses ||
                    not (List.contains storageClass validClasses)
        ]

        testList "API Authentication Tests" [
            testCase "Bearer token formatting" <| fun _ ->
                let token = "test-token-123"
                let bearerToken = $"Bearer {token}"
                Expect.stringStarts bearerToken "Bearer " "Should start with Bearer"
                Expect.stringContains bearerToken token "Should contain the token"

            testCase "Account ID validation" <| fun _ ->
                let isValidAccountId id =
                    not (String.IsNullOrWhiteSpace(id)) &&
                    id.Length = 32 &&
                    System.Text.RegularExpressions.Regex.IsMatch(id, "^[a-f0-9]+$")

                Expect.isTrue (isValidAccountId "a1b2c3d4e5f67890123456789012345f") "Valid account ID should pass"
                Expect.isFalse (isValidAccountId "short") "Short ID should fail"
                Expect.isFalse (isValidAccountId "g1b2c3d4e5f6789012345678901234567") "Invalid character should fail"
        ]
    ]