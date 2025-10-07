namespace rec CloudFlare.Management.Logs

open System.Net
open System.Net.Http
open System.Text
open System.Threading
open CloudFlare.Management.Logs.Types
open CloudFlare.Management.Logs.Http

///Logs API
type LogsClient(httpClient: HttpClient) =
    ///<summary>
    ///Deletes CMB config.
    ///</summary>
    member this.DeleteAccountsAccountIdLogsControlCmbConfig(accountId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.deleteAsync
                    httpClient
                    "/accounts/{account_id}/logs/control/cmb/config"
                    requestParts
                    cancellationToken

            return DeleteAccountsAccountIdLogsControlCmbConfig.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Gets CMB config.
    ///</summary>
    member this.GetAccountsAccountIdLogsControlCmbConfig(accountId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/accounts/{account_id}/logs/control/cmb/config"
                    requestParts
                    cancellationToken

            return GetAccountsAccountIdLogsControlCmbConfig.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Updates CMB config.
    ///</summary>
    member this.PostAccountsAccountIdLogsControlCmbConfig
        (
            accountId: string,
            body: logcontrolcmbconfig,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("account_id", accountId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/accounts/{account_id}/logs/control/cmb/config"
                    requestParts
                    cancellationToken

            return PostAccountsAccountIdLogsControlCmbConfig.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Gets log retention flag for Logpull API.
    ///</summary>
    member this.GetZonesZoneIdLogsControlRetentionFlag(zoneId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts = [ RequestPart.path ("zone_id", zoneId) ]

            let! (status, content) =
                OpenApiHttp.getAsync
                    httpClient
                    "/zones/{zone_id}/logs/control/retention/flag"
                    requestParts
                    cancellationToken

            return GetZonesZoneIdLogsControlRetentionFlag.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Updates log retention flag for Logpull API.
    ///</summary>
    member this.PostZonesZoneIdLogsControlRetentionFlag
        (
            zoneId: string,
            body: logcontrolretentionflag,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("zone_id", zoneId)
                  RequestPart.jsonContent body ]

            let! (status, content) =
                OpenApiHttp.postAsync
                    httpClient
                    "/zones/{zone_id}/logs/control/retention/flag"
                    requestParts
                    cancellationToken

            return PostZonesZoneIdLogsControlRetentionFlag.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///The `/rayids` api route allows lookups by specific rayid. The rayids route will return zero, one, or more records (ray ids are not unique).
    ///</summary>
    member this.GetZonesZoneIdLogsRayidsRayId
        (
            zoneId: string,
            rayId: string,
            ?fields: string,
            ?timestamps: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("zone_id", zoneId)
                  RequestPart.path ("ray_id", rayId)
                  if fields.IsSome then
                      RequestPart.query ("fields", fields.Value)
                  if timestamps.IsSome then
                      RequestPart.query ("timestamps", timestamps.Value) ]

            let! (status, content) =
                OpenApiHttp.getAsync httpClient "/zones/{zone_id}/logs/rayids/{ray_id}" requestParts cancellationToken

            return GetZonesZoneIdLogsRayidsRayId.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///The `/received` api route allows customers to retrieve their edge HTTP logs. The basic access pattern is "give me all the logs for zone Z for minute M", where the minute M refers to the time records were received at Cloudflare's central data center. `start` is inclusive, and `end` is exclusive. Because of that, to get all data, at minutely cadence, starting at 10AM, the proper values are: `start=2018-05-20T10:00:00Z&amp;end=2018-05-20T10:01:00Z`, then `start=2018-05-20T10:01:00Z&amp;end=2018-05-20T10:02:00Z` and so on; the overlap will be handled properly.
    ///</summary>
    member this.GetZonesZoneIdLogsReceived
        (
            zoneId: string,
            ``end``: logshareend,
            ?start: logsharestart,
            ?fields: string,
            ?sample: float,
            ?count: int,
            ?timestamps: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("zone_id", zoneId)
                  RequestPart.query ("end", ``end``)
                  if start.IsSome then
                      RequestPart.query ("start", start.Value)
                  if fields.IsSome then
                      RequestPart.query ("fields", fields.Value)
                  if sample.IsSome then
                      RequestPart.query ("sample", sample.Value)
                  if count.IsSome then
                      RequestPart.query ("count", count.Value)
                  if timestamps.IsSome then
                      RequestPart.query ("timestamps", timestamps.Value) ]

            let! (status, content) =
                OpenApiHttp.getAsync httpClient "/zones/{zone_id}/logs/received" requestParts cancellationToken

            return GetZonesZoneIdLogsReceived.OK(Serializer.deserialize content)
        }

    ///<summary>
    ///Lists all fields available. The response is json object with key-value pairs, where keys are field names, and values are descriptions.
    ///</summary>
    member this.GetZonesZoneIdLogsReceivedFields(zoneId: string, ?cancellationToken: CancellationToken) =
        async {
            let requestParts = [ RequestPart.path ("zone_id", zoneId) ]

            let! (status, content) =
                OpenApiHttp.getAsync httpClient "/zones/{zone_id}/logs/received/fields" requestParts cancellationToken

            return GetZonesZoneIdLogsReceivedFields.OK(Serializer.deserialize content)
        }
