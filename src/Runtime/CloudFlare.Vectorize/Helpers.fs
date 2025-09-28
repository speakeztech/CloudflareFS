module CloudFlare.Vectorize.Helpers

open CloudFlare.Vectorize
open Fable.Core.JsInterop
open System

/// Create a vector with just ID and values
let createVector (id: string) (values: float[]) =
    { new VectorizeVector with
        member val id = id with get, set
        member val values = values with get, set
        member val metadata = None with get, set
        member val namespace = None with get, set }

/// Create a vector with metadata
let createVectorWithMetadata (id: string) (values: float[]) (metadata: Map<string, VectorizeVectorMetadataValue>) =
    { new VectorizeVector with
        member val id = id with get, set
        member val values = values with get, set
        member val metadata = Some (Object metadata) with get, set
        member val namespace = None with get, set }

/// Create query options for top K results
let queryOptions (topK: int) =
    { new VectorizeQueryOptions with
        member val topK = Some topK with get, set
        member val namespace = None with get, set
        member val includeValues = None with get, set
        member val includeMetadata = None with get, set
        member val filter = None with get, set }

/// Create full query options
let fullQueryOptions (topK: int) (includeValues: bool) (includeMetadata: bool) =
    { new VectorizeQueryOptions with
        member val topK = Some topK with get, set
        member val namespace = None with get, set
        member val includeValues = Some includeValues with get, set
        member val includeMetadata = Some includeMetadata with get, set
        member val filter = None with get, set }

/// Vectorize computation expression
type VectorizeBuilder(index: VectorizeIndex) =
    member _.Index = index

    member _.Query(vector: float[], ?topK: int) =
        let options =
            match topK with
            | Some k -> queryOptions k
            | None -> null
        index.query(vector, options) |> Async.AwaitPromise

    member _.QueryWithOptions(vector: float[], options: VectorizeQueryOptions) =
        index.query(vector, options) |> Async.AwaitPromise

    member _.Insert(vectors: VectorizeVector list) =
        index.insert(ResizeArray vectors) |> Async.AwaitPromise

    member _.Upsert(vectors: VectorizeVector list) =
        index.upsert(ResizeArray vectors) |> Async.AwaitPromise

    member _.GetByIds(ids: string list) =
        index.getByIds(ResizeArray ids) |> Async.AwaitPromise

    member _.DeleteByIds(ids: string list) =
        index.deleteByIds(ResizeArray ids) |> Async.AwaitPromise

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

/// Convert text embedding to vector (placeholder - would use actual embedding model)
let textToVector (text: string) (dimensions: int) =
    // This is a placeholder - in real usage, you would use an embedding model
    // like OpenAI embeddings or a local model
    let hash = text.GetHashCode()
    let random = System.Random(hash)
    Array.init dimensions (fun _ -> random.NextDouble() * 2.0 - 1.0)
    |> normalize

/// Semantic search helper
let semanticSearch (index: VectorizeIndex) (query: string) (topK: int) =
    async {
        // In real implementation, would use actual embedding model
        let queryVector = textToVector query 1536 // Common embedding dimension

        let options = fullQueryOptions topK false true
        let! results = index.query(queryVector, options) |> Async.AwaitPromise

        return results.matches |> Seq.toList
    }

/// Batch insert helper with chunking
let batchInsert (index: VectorizeIndex) (vectors: VectorizeVector list) (chunkSize: int) =
    async {
        let chunks = vectors |> List.chunkBySize chunkSize
        let mutable totalInserted = 0

        for chunk in chunks do
            let! result = index.insert(ResizeArray chunk) |> Async.AwaitPromise
            totalInserted <- totalInserted + result.count

        return totalInserted
    }

/// Active pattern for metadata value extraction
let (|StringMeta|NumberMeta|BoolMeta|ArrayMeta|) (value: VectorizeVectorMetadataValue) =
    match value with
    | String s -> StringMeta s
    | Number n -> NumberMeta n
    | Boolean b -> BoolMeta b
    | StringArray arr -> ArrayMeta arr