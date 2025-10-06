namespace rec CloudFlare.Api.Analytics

open System.Net
open System.Net.Http
open System.Text
open System.Threading
open CloudFlare.Api.Analytics.Types
open CloudFlare.Api.Analytics.Http

///Analytics API
type AnalyticsClient(httpClient: HttpClient) =
    ///<summary>
    ///Argo Analytics for a zone
    ///</summary>
    member this.ArgoAnalyticsForZoneArgoAnalyticsForAZone
        (
            zoneId: string,
            ?bins: string,
            ?cancellationToken: CancellationToken
        ) =
        async {
            let requestParts =
                [ RequestPart.path ("zone_id", zoneId)
                  if bins.IsSome then
                      RequestPart.query ("bins", bins.Value) ]

            let! (status, content) =
                OpenApiHttp.getAsync httpClient "/zones/{zone_id}/analytics/latency" requestParts cancellationToken

            return ArgoAnalyticsForZoneArgoAnalyticsForAZone.OK(Serializer.deserialize content)
        }
