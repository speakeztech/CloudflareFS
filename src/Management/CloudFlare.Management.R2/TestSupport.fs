namespace CloudFlare.Management.R2

open System.Net.Http

type R2Client(httpClient: HttpClient, accountId: string, apiToken: string) =
    member val HttpClient = httpClient
    member val AccountId = accountId
    member val ApiToken = apiToken