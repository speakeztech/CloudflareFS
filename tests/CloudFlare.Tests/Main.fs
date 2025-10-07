module CloudFlare.Tests.Main

open System
open Expecto

[<EntryPoint>]
let main args =
    let allTests =
        testList "All CloudFlareFS Tests" [
            CoreTests.tests
            ManagementTests.tests
            RuntimeTests.tests
            DeploymentTests.tests
        ]

    runTestsWithCLIArgs [] args allTests