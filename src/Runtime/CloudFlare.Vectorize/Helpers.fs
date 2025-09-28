module CloudFlare.Vectorize.Helpers

open CloudFlare.Vectorize
open Fable.Core
open Fable.Core.JsInterop
open System

// Helper to convert JS Promise to F# Async
let inline promiseToAsync (p: JS.Promise<'T>) : Async<'T> =
    Async.AwaitPromise p

/// Create a vector with just ID and values
let createVector (id: string) (values: float[]) =
    let mutable _id = id
    let mutable _values = values
    let mutable _metadata = None
    let mutable _namespace = None
    { new VectorizeVector with
        member _.id with get() = _id and set(v) = _id <- v
        member _.values with get() = _values and set(v) = _values <- v
        member _.metadata with get() = _metadata and set(v) = _metadata <- v
        member _.``namespace`` with get() = _namespace and set(v) = _namespace <- v }

/// Create a vector with metadata
let createVectorWithMetadata (id: string) (values: float[]) (metadata: Map<string, VectorizeVectorMetadataValue>) =
    let mutable _id = id
    let mutable _values = values
    let mutable _metadata = Some (VectorizeVectorMetadata.Object metadata)
    let mutable _namespace = None
    { new VectorizeVector with
        member _.id with get() = _id and set(v) = _id <- v
        member _.values with get() = _values and set(v) = _values <- v
        member _.metadata with get() = _metadata and set(v) = _metadata <- v
        member _.``namespace`` with get() = _namespace and set(v) = _namespace <- v }

/// Create query options for top K results
let queryOptions (topK: int) =
    let mutable _topK = Some topK
    let mutable _namespace = None
    let mutable _includeValues = None
    let mutable _includeMetadata = None
    let mutable _filter = None
    { new VectorizeQueryOptions with
        member _.topK with get() = _topK and set(v) = _topK <- v
        member _.``namespace`` with get() = _namespace and set(v) = _namespace <- v
        member _.includeValues with get() = _includeValues and set(v) = _includeValues <- v
        member _.includeMetadata with get() = _includeMetadata and set(v) = _includeMetadata <- v
        member _.filter with get() = _filter and set(v) = _filter <- v }

/// Create full query options
let fullQueryOptions (topK: int) (includeValues: bool) (includeMetadata: bool) =
    let mutable _topK = Some topK
    let mutable _namespace = None
    let mutable _includeValues = Some includeValues
    let mutable _includeMetadata = Some includeMetadata
    let mutable _filter = None
    { new VectorizeQueryOptions with
        member _.topK with get() = _topK and set(v) = _topK <- v
        member _.``namespace`` with get() = _namespace and set(v) = _namespace <- v
        member _.includeValues with get() = _includeValues and set(v) = _includeValues <- v
        member _.includeMetadata with get() = _includeMetadata and set(v) = _includeMetadata <- v
        member _.filter with get() = _filter and set(v) = _filter <- v }

/// Vectorize computation expression
type VectorizeBuilder(index: VectorizeIndex) =
    member _.Index = index

    member _.Query(vector: float[], ?topK: int) =
        let options =
            match topK with
            | Some k -> queryOptions k
            | None -> null
        index.query(vector, options) |> promiseToAsync

    member _.QueryWithOptions(vector: float[], options: VectorizeQueryOptions) =
        index.query(vector, options) |> promiseToAsync

    member _.Insert(vectors: VectorizeVector list) =
        index.insert(ResizeArray vectors) |> promiseToAsync

    member _.Upsert(vectors: VectorizeVector list) =
        index.upsert(ResizeArray vectors) |> promiseToAsync

    member _.GetByIds(ids: string list) =
        index.getByIds(ResizeArray ids) |> promiseToAsync

    member _.DeleteByIds(ids: string list) =
        index.deleteByIds(ResizeArray ids) |> promiseToAsync

/// Create a vectorize builder
let vectorize (index: VectorizeIndex) = VectorizeBuilder(index)

/// Compute cosine similarity between two vectors
let cosineSimilarity (a: float[]) (b: float[]) =
    if a.Length <> b.Length then
        failwith "Vectors must have same dimensions"

    let dotProduct = Array.fold2 (fun acc x y -> acc + x * y) 0.0 a b
    let magnitudeA = sqrt(Array.fold (fun acc x -> acc + x * x) 0.0 a)
    let magnitudeB = sqrt(Array.fold (fun acc x -> acc + x * x) 0.0 b)

    dotProduct / (magnitudeA * magnitudeB)

/// Compute euclidean distance between two vectors
let euclideanDistance (a: float[]) (b: float[]) =
    if a.Length <> b.Length then
        failwith "Vectors must have same dimensions"

    Array.fold2 (fun acc x y ->
        let diff = x - y
        acc + diff * diff
    ) 0.0 a b
    |> sqrt

/// Normalize a vector to unit length
let normalize (vector: float[]) =
    let magnitude = sqrt(Array.fold (fun acc x -> acc + x * x) 0.0 vector)
    if magnitude = 0.0 then
        vector
    else
        Array.map (fun x -> x / magnitude) vector

/// Generate a deterministic pseudo-random vector from text for testing purposes
/// Note: For production, use actual embedding models like OpenAI or Cohere through Cloudflare AI
let textToVector (text: string) (dimensions: int) =
    // Generates a deterministic vector based on text hash - useful for testing
    // For production, use Cloudflare AI Workers or external embedding services
    let hash = text.GetHashCode()
    let random = System.Random(hash)
    Array.init dimensions (fun _ -> random.NextDouble() * 2.0 - 1.0)
    |> normalize

/// Semantic search helper
let semanticSearch (index: VectorizeIndex) (query: string) (topK: int) =
    async {
        // Use Cloudflare AI Workers for production embeddings
        let queryVector = textToVector query 1536 // Common embedding dimension

        let options = fullQueryOptions topK false true
        let! results = index.query(queryVector, options) |> Async.AwaitPromise

        return results.matches |> unbox<seq<_>> |> Seq.toList
    }

/// Batch insert helper with chunking
let batchInsert (index: VectorizeIndex) (vectors: VectorizeVector list) (chunkSize: int) =
    async {
        let chunks = vectors |> List.chunkBySize chunkSize
        let mutable totalInserted = 0

        for chunk in chunks do
            let! result = index.insert(ResizeArray chunk) |> Async.AwaitPromise
            totalInserted <- totalInserted + (result.count |> unbox<int>)

        return totalInserted
    }

/// Active pattern for metadata value extraction
let (|StringMeta|NumberMeta|BoolMeta|ArrayMeta|) (value: VectorizeVectorMetadataValue) =
    match value with
    | String s -> StringMeta s
    | Number n -> NumberMeta n
    | Boolean b -> BoolMeta b
    | StringArray arr -> ArrayMeta arr