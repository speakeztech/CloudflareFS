namespace rec CloudFlare.Management.Vectorize

open System.Net
open System.Net.Http
open System.Text
open System.Threading
open CloudFlare.Management.Vectorize.Types
open CloudFlare.Management.Vectorize.Http

///Vector Database Management API
type VectorizeClient(httpClient: HttpClient) =
