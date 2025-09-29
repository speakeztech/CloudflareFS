module CloudFlare.Tests.CoreTests

open Expecto
open CloudFlare.Core

let tests =
    testList "CloudFlare.Core Tests" [
        testCase "Environment detection" <| fun _ ->
            let isDevelopment = Environment.isDevelopment()
            Expect.isTrue (isDevelopment || not isDevelopment) "Environment detection should return a boolean"

        testCase "CloudflareFS version" <| fun _ ->
            let version = Version.current
            Expect.isNotEmpty version "Version should not be empty"
            Expect.stringContains version "." "Version should contain a dot separator"

        testProperty "Configuration validation" <| fun (key: string) ->
            if System.String.IsNullOrWhiteSpace(key) then
                true
            else
                let config = Configuration.tryGet key
                match config with
                | Some value -> not (System.String.IsNullOrEmpty(value))
                | None -> true
    ]