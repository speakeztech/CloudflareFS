namespace CloudFlare.Vectorize

open Fable.Core
open Fable.Core.JsInterop
open System

/// Metadata value types for vectors
type VectorizeVectorMetadataValue =
    | String of string
    | Number of float
    | Boolean of bool
    | StringArray of string[]

/// Metadata for vectors
type VectorizeVectorMetadata =
    | Value of VectorizeVectorMetadataValue
    | Object of Map<string, VectorizeVectorMetadataValue>

/// Vector representation
[<AllowNullLiteral>]
type VectorizeVector =
    abstract member id: string with get, set
    abstract member values: float[] with get, set
    abstract member metadata: VectorizeVectorMetadata option with get, set
    abstract member ``namespace``: string option with get, set

/// Query options for vector search
[<AllowNullLiteral>]
type VectorizeQueryOptions =
    abstract member topK: int option with get, set
    abstract member ``namespace``: string option with get, set
    abstract member includeValues: bool option with get, set
    abstract member includeMetadata: bool option with get, set
    abstract member filter: obj option with get, set

/// Vector match result
[<AllowNullLiteral>]
type VectorMatch =
    abstract member id: string with get
    abstract member score: float with get
    abstract member values: float[] option with get
    abstract member metadata: VectorizeVectorMetadata option with get

/// Query matches result
[<AllowNullLiteral>]
type VectorizeMatches =
    abstract member matches: ResizeArray<VectorMatch> with get
    abstract member count: int with get

/// Insert result
[<AllowNullLiteral>]
type VectorizeInsertResult =
    abstract member ids: ResizeArray<string> with get
    abstract member count: int with get

/// Upsert result
[<AllowNullLiteral>]
type VectorizeUpsertResult =
    abstract member ids: ResizeArray<string> with get
    abstract member count: int with get

/// Mutation result for delete operations
[<AllowNullLiteral>]
type VectorizeMutationResult =
    abstract member mutationCount: int with get

/// Vectorize index interface
[<AllowNullLiteral>]
[<Interface>]
type VectorizeIndex =
    /// Query vectors by similarity
    abstract member query: vector: float[] * ?options: VectorizeQueryOptions -> JS.Promise<VectorizeMatches>

    /// Insert new vectors
    abstract member insert: vectors: ResizeArray<VectorizeVector> -> JS.Promise<VectorizeInsertResult>

    /// Upsert vectors (insert or update)
    abstract member upsert: vectors: ResizeArray<VectorizeVector> -> JS.Promise<VectorizeUpsertResult>

    /// Get vectors by IDs
    abstract member getByIds: ids: ResizeArray<string> -> JS.Promise<ResizeArray<VectorizeVector>>

    /// Delete vectors by IDs
    abstract member deleteByIds: ids: ResizeArray<string> -> JS.Promise<VectorizeMutationResult>

/// Environment with Vectorize bindings
[<AllowNullLiteral>]
type IVectorizeEnvironment =
    /// Access a Vectorize index binding by name
    [<Emit("$0[$1]")>]
    abstract member getVectorizeIndex: name: string -> VectorizeIndex

/// Distance metrics for vector similarity
type DistanceMetric =
    | Euclidean
    | Cosine
    | DotProduct
    member this.ToString() =
        match this with
        | Euclidean -> "euclidean"
        | Cosine -> "cosine"
        | DotProduct -> "dot-product"

/// Index configuration
[<AllowNullLiteral>]
type VectorizeIndexConfig =
    abstract member dimensions: int with get, set
    abstract member metric: string with get, set
    abstract member namespaces: ResizeArray<string> option with get, set