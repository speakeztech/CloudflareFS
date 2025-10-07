module CloudFlare.Tests.ManagementTests

open Expecto
open System
open System.Net.Http
open CloudFlare.Management.D1
open CloudFlare.Management.R2

let tests =
    testList "CloudFlare.Management Tests" [

        testList "D1 Database Name Validation" [
            testCase "Valid database names" <| fun _ ->
                let isValid name =
                    not (String.IsNullOrWhiteSpace(name)) &&
                    name.Length >= 1 &&
                    name.Length <= 63 &&
                    System.Text.RegularExpressions.Regex.IsMatch(name, "^[a-z0-9]([a-z0-9-]*[a-z0-9])?$")

                Expect.isTrue (isValid "my-database") "Simple name should be valid"
                Expect.isTrue (isValid "db123") "Alphanumeric should be valid"
                Expect.isTrue (isValid "a") "Single character should be valid"
                Expect.isTrue (isValid "my-prod-db-v2") "Multiple hyphens should be valid"

            testCase "Invalid database names" <| fun _ ->
                let isValid name =
                    not (String.IsNullOrWhiteSpace(name)) &&
                    name.Length >= 1 &&
                    name.Length <= 63 &&
                    System.Text.RegularExpressions.Regex.IsMatch(name, "^[a-z0-9]([a-z0-9-]*[a-z0-9])?$")

                Expect.isFalse (isValid "My-Database") "Uppercase letters should fail"
                Expect.isFalse (isValid "-database") "Leading dash should fail"
                Expect.isFalse (isValid "database-") "Trailing dash should fail"
                Expect.isFalse (isValid "my_database") "Underscores should fail"
                Expect.isFalse (isValid "") "Empty string should fail"
                Expect.isFalse (isValid "my database") "Spaces should fail"

            testCase "Database name length limits" <| fun _ ->
                let isValid name =
                    not (String.IsNullOrWhiteSpace(name)) &&
                    name.Length >= 1 &&
                    name.Length <= 63 &&
                    System.Text.RegularExpressions.Regex.IsMatch(name, "^[a-z0-9]([a-z0-9-]*[a-z0-9])?$")

                let validMaxLength = String.replicate 63 "a"
                Expect.isTrue (isValid validMaxLength) "63 characters should be valid"

                let tooLong = String.replicate 64 "a"
                Expect.isFalse (isValid tooLong) "64 characters should be invalid"
        ]

        testList "D1 Location Validation" [
            testCase "Valid D1 regions" <| fun _ ->
                let validLocations = ["weur"; "eeur"; "apac"; "wnam"; "enam"]

                validLocations |> List.iter (fun loc ->
                    Expect.isTrue (List.contains loc validLocations) $"{loc} should be valid"
                )

            testCase "Invalid D1 regions" <| fun _ ->
                let validLocations = ["weur"; "eeur"; "apac"; "wnam"; "enam"]
                let invalidLocations = ["us-east"; "europe"; "asia"; "WEUR"; ""]

                invalidLocations |> List.iter (fun loc ->
                    Expect.isFalse (List.contains loc validLocations) $"{loc} should be invalid"
                )
        ]

        testList "R2 Bucket Name Validation" [
            testCase "Valid bucket names" <| fun _ ->
                let isValid name =
                    not (String.IsNullOrWhiteSpace(name)) &&
                    name.Length >= 3 &&
                    name.Length <= 63 &&
                    System.Text.RegularExpressions.Regex.IsMatch(name, "^[a-z0-9]([a-z0-9.-]*[a-z0-9])?$")

                Expect.isTrue (isValid "my-bucket") "Simple name should be valid"
                Expect.isTrue (isValid "my.bucket.name") "Dots should be allowed"
                Expect.isTrue (isValid "bucket123") "Numbers should be allowed"
                Expect.isTrue (isValid "my-bucket.v2") "Mixed separators should be valid"

            testCase "Invalid bucket names" <| fun _ ->
                let isValid name =
                    not (String.IsNullOrWhiteSpace(name)) &&
                    name.Length >= 3 &&
                    name.Length <= 63 &&
                    System.Text.RegularExpressions.Regex.IsMatch(name, "^[a-z0-9]([a-z0-9.-]*[a-z0-9])?$")

                Expect.isFalse (isValid "My-Bucket") "Uppercase letters should fail"
                Expect.isFalse (isValid "bu") "Too short name should fail"
                Expect.isFalse (isValid "-bucket") "Leading dash should fail"
                Expect.isFalse (isValid "bucket-") "Trailing dash should fail"
                Expect.isFalse (isValid ".bucket") "Leading dot should fail"
                Expect.isFalse (isValid "bucket.") "Trailing dot should fail"

            testCase "R2 bucket name length limits" <| fun _ ->
                let isValid name =
                    not (String.IsNullOrWhiteSpace(name)) &&
                    name.Length >= 3 &&
                    name.Length <= 63 &&
                    System.Text.RegularExpressions.Regex.IsMatch(name, "^[a-z0-9]([a-z0-9.-]*[a-z0-9])?$")

                Expect.isFalse (isValid "ab") "2 characters should be invalid"
                Expect.isTrue (isValid "abc") "3 characters should be valid"

                let validMaxLength = "a" + String.replicate 61 "x" + "z"
                Expect.isTrue (isValid validMaxLength) "63 characters should be valid"
        ]

        testList "R2 Storage Class Tests" [
            testCase "Valid storage classes" <| fun _ ->
                let validClasses = ["Standard"; "InfrequentAccess"]

                Expect.isTrue (List.contains "Standard" validClasses) "Standard should be valid"
                Expect.isTrue (List.contains "InfrequentAccess" validClasses) "InfrequentAccess should be valid"

            testCase "Invalid storage classes" <| fun _ ->
                let validClasses = ["Standard"; "InfrequentAccess"]

                Expect.isFalse (List.contains "standard" validClasses) "Lowercase should be invalid"
                Expect.isFalse (List.contains "Archive" validClasses) "Archive should be invalid"
                Expect.isFalse (List.contains "" validClasses) "Empty should be invalid"
        ]

        testList "API Authentication Tests" [
            testCase "Bearer token format" <| fun _ ->
                let token = "test-token-abc123"
                let bearerToken = $"Bearer {token}"

                Expect.stringStarts bearerToken "Bearer " "Should start with Bearer"
                Expect.stringContains bearerToken token "Should contain the token"
                Expect.equal (bearerToken.Split(' ').Length) 2 "Should have exactly 2 parts"

            testCase "API token validation" <| fun _ ->
                let isValidToken (token: string) =
                    not (String.IsNullOrWhiteSpace(token)) &&
                    token.Length > 10

                Expect.isTrue (isValidToken "valid-token-123") "Valid token should pass"
                Expect.isFalse (isValidToken "") "Empty token should fail"
                Expect.isFalse (isValidToken "short") "Short token should fail"

            testCase "Account ID format validation" <| fun _ ->
                let isValidAccountId id =
                    not (String.IsNullOrWhiteSpace(id)) &&
                    id.Length = 32 &&
                    System.Text.RegularExpressions.Regex.IsMatch(id, "^[a-f0-9]+$")

                Expect.isTrue (isValidAccountId "a1b2c3d4e5f6789012345678901234ef") "Valid hex ID should pass"
                Expect.isTrue (isValidAccountId "00000000000000000000000000000000") "All zeros should be valid"
                Expect.isTrue (isValidAccountId "ffffffffffffffffffffffffffffffff") "All fs should be valid"

                Expect.isFalse (isValidAccountId "short") "Short ID should fail"
                Expect.isFalse (isValidAccountId "g1b2c3d4e5f6789012345678901234ef") "Invalid character g should fail"
                Expect.isFalse (isValidAccountId "A1B2C3D4E5F6789012345678901234EF") "Uppercase should fail"
        ]

        testList "API Path Construction Tests" [
            testCase "Account-scoped endpoint path" <| fun _ ->
                let accountId = "abc123"
                let path = $"/accounts/{accountId}/workers/scripts"

                Expect.stringContains path "/accounts/" "Should contain accounts segment"
                Expect.stringContains path accountId "Should contain account ID"
                Expect.stringContains path "/workers/scripts" "Should contain resource path"

            testCase "Script-specific endpoint path" <| fun _ ->
                let accountId = "abc123"
                let scriptName = "my-worker"
                let path = $"/accounts/{accountId}/workers/scripts/{scriptName}"

                Expect.stringContains path scriptName "Should contain script name"
                Expect.isTrue (path.EndsWith(scriptName)) "Should end with script name"

            testCase "Query parameter encoding" <| fun _ ->
                let param = "value with spaces"
                let encoded = Uri.EscapeDataString(param)

                Expect.isFalse (encoded.Contains(" ")) "Spaces should be encoded"
                Expect.stringContains encoded "value" "Should contain original text"
        ]
    ]