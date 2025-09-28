namespace rec CloudFlare.Management.Vectorize.Types

///Identifier
type vectorizeidentifier = string
///Specifies the description of the index.
type ``vectorizeindex-description`` = string
type ``vectorizeindex-dimensions`` = int

type ``vectorizeindex-get-vectors-by-id-responseArrayItem`` =
    { ///Identifier for a Vector
      id: Option<``vectorizevector-identifier``>
      metadata: Option<Newtonsoft.Json.Linq.JObject>
      ``namespace``: Option<string>
      values: Option<list<float>> }
    ///Creates an instance of vectorizeindex-get-vectors-by-id-responseArrayItem with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizeindex-get-vectors-by-id-responseArrayItem`` =
        { id = None
          metadata = None
          ``namespace`` = None
          values = None }

type ``vectorizeindex-get-vectors-by-id-response`` = list<``vectorizeindex-get-vectors-by-id-responseArrayItem``>

///Specifies the type of metric to use calculating distance.
[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type ``vectorizeindex-metric`` =
    | [<CompiledName "cosine">] Cosine
    | [<CompiledName "euclidean">] Euclidean
    | [<CompiledName "dot-product">] DotProduct
    member this.Format() =
        match this with
        | Cosine -> "cosine"
        | Euclidean -> "euclidean"
        | DotProduct -> "dot-product"

type ``vectorizeindex-name`` = string

///Specifies the preset to use for the index.
[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type ``vectorizeindex-preset`` =
    | [<CompiledName "@cf/baai/bge-small-en-v1.5">] ``@cfBaaiBgeSmallEnV1Numeric_5``
    | [<CompiledName "@cf/baai/bge-base-en-v1.5">] ``@cfBaaiBgeBaseEnV1Numeric_5``
    | [<CompiledName "@cf/baai/bge-large-en-v1.5">] ``@cfBaaiBgeLargeEnV1Numeric_5``
    | [<CompiledName "openai/text-embedding-ada-002">] OpenaiTextEmbeddingAdaNumeric_002
    | [<CompiledName "cohere/embed-multilingual-v2.0">] CohereEmbedMultilingualV2Numeric_0
    member this.Format() =
        match this with
        | (@cfBaaiBgeSmallEnV1Numeric_5) -> "@cf/baai/bge-small-en-v1.5"
        | (@cfBaaiBgeBaseEnV1Numeric_5) -> "@cf/baai/bge-base-en-v1.5"
        | (@cfBaaiBgeLargeEnV1Numeric_5) -> "@cf/baai/bge-large-en-v1.5"
        | OpenaiTextEmbeddingAdaNumeric_002 -> "openai/text-embedding-ada-002"
        | CohereEmbedMultilingualV2Numeric_0 -> "cohere/embed-multilingual-v2.0"

type vectorizemessagesArrayItem =
    { code: int
      message: string }
    ///Creates an instance of vectorizemessagesArrayItem with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): vectorizemessagesArrayItem = { code = code; message = message }

type vectorizemessages = list<vectorizemessagesArrayItem>
///Identifier for a Vector
type ``vectorizevector-identifier`` = string

type Errors =
    { code: int
      message: string }
    ///Creates an instance of Errors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): Errors = { code = code; message = message }

type Messages =
    { code: int
      message: string }
    ///Creates an instance of Messages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): Messages = { code = code; message = message }

type ``vectorizeapi-response-collection`` =
    { errors: Option<list<Errors>>
      messages: Option<list<Messages>>
      result: Option<Newtonsoft.Json.Linq.JToken>
      ///Whether the API call was successful
      success: Option<bool>
      result_info: Option<vectorizeresultinfo> }
    ///Creates an instance of vectorizeapi-response-collection with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizeapi-response-collection`` =
        { errors = None
          messages = None
          result = None
          success = None
          result_info = None }

type ``vectorizeapi-response-commonErrors`` =
    { code: int
      message: string }
    ///Creates an instance of vectorizeapi-response-commonErrors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``vectorizeapi-response-commonErrors`` =
        { code = code; message = message }

type ``vectorizeapi-response-commonMessages`` =
    { code: int
      message: string }
    ///Creates an instance of vectorizeapi-response-commonMessages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``vectorizeapi-response-commonMessages`` =
        { code = code; message = message }

type ``vectorizeapi-response-common`` =
    { errors: list<``vectorizeapi-response-commonErrors``>
      messages: list<``vectorizeapi-response-commonMessages``>
      result: Newtonsoft.Json.Linq.JToken
      ///Whether the API call was successful
      success: bool }
    ///Creates an instance of vectorizeapi-response-common with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: list<``vectorizeapi-response-commonErrors``>,
                          messages: list<``vectorizeapi-response-commonMessages``>,
                          result: Newtonsoft.Json.Linq.JToken,
                          success: bool): ``vectorizeapi-response-common`` =
        { errors = errors
          messages = messages
          result = result
          success = success }

type ``vectorizeapi-response-common-failure`` =
    { errors: Newtonsoft.Json.Linq.JToken
      messages: Newtonsoft.Json.Linq.JToken
      result: Newtonsoft.Json.Linq.JObject
      ///Whether the API call was successful
      success: bool }
    ///Creates an instance of vectorizeapi-response-common-failure with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: Newtonsoft.Json.Linq.JToken,
                          messages: Newtonsoft.Json.Linq.JToken,
                          result: Newtonsoft.Json.Linq.JObject,
                          success: bool): ``vectorizeapi-response-common-failure`` =
        { errors = errors
          messages = messages
          result = result
          success = success }

type ``vectorizeapi-response-singleErrors`` =
    { code: int
      message: string }
    ///Creates an instance of vectorizeapi-response-singleErrors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``vectorizeapi-response-singleErrors`` =
        { code = code; message = message }

type ``vectorizeapi-response-singleMessages`` =
    { code: int
      message: string }
    ///Creates an instance of vectorizeapi-response-singleMessages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``vectorizeapi-response-singleMessages`` =
        { code = code; message = message }

type Result = Map<string, Newtonsoft.Json.Linq.JToken>

type ``vectorizeapi-response-single`` =
    { errors: Option<list<``vectorizeapi-response-singleErrors``>>
      messages: Option<list<``vectorizeapi-response-singleMessages``>>
      result: Option<Newtonsoft.Json.Linq.JToken>
      ///Whether the API call was successful
      success: Option<bool> }
    ///Creates an instance of vectorizeapi-response-single with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizeapi-response-single`` =
        { errors = None
          messages = None
          result = None
          success = None }

type ``vectorizecreate-index-request`` =
    { config: Newtonsoft.Json.Linq.JToken
      ///Specifies the description of the index.
      description: Option<``vectorizeindex-description``>
      name: ``vectorizeindex-name`` }
    ///Creates an instance of vectorizecreate-index-request with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (config: Newtonsoft.Json.Linq.JToken, name: ``vectorizeindex-name``): ``vectorizecreate-index-request`` =
        { config = config
          description = None
          name = name }

type ``vectorizecreate-index-response`` =
    { config: Option<``vectorizeindex-dimension-configuration``>
      ///Specifies the timestamp the resource was created as an ISO8601 string.
      created_on: Option<System.DateTimeOffset>
      ///Specifies the description of the index.
      description: Option<``vectorizeindex-description``>
      ///Specifies the timestamp the resource was modified as an ISO8601 string.
      modified_on: Option<System.DateTimeOffset>
      name: Option<``vectorizeindex-name``> }
    ///Creates an instance of vectorizecreate-index-response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizecreate-index-response`` =
        { config = None
          created_on = None
          description = None
          modified_on = None
          name = None }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type IndexType =
    | [<CompiledName "string">] String
    | [<CompiledName "number">] Number
    | [<CompiledName "boolean">] Boolean
    member this.Format() =
        match this with
        | String -> "string"
        | Number -> "number"
        | Boolean -> "boolean"

type ``vectorizecreate-metadata-index-request`` =
    { ///Specifies the type of metadata property to index.
      indexType: IndexType
      ///Specifies the metadata property to index.
      propertyName: string }
    ///Creates an instance of vectorizecreate-metadata-index-request with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (indexType: IndexType, propertyName: string): ``vectorizecreate-metadata-index-request`` =
        { indexType = indexType
          propertyName = propertyName }

type ``vectorizecreate-metadata-index-response`` =
    { ///The unique identifier for the async mutation operation containing the changeset.
      mutationId: Option<Newtonsoft.Json.Linq.JToken> }
    ///Creates an instance of vectorizecreate-metadata-index-response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizecreate-metadata-index-response`` = { mutationId = None }

type ``vectorizedelete-metadata-index-request`` =
    { ///Specifies the metadata property for which the index must be deleted.
      propertyName: string }
    ///Creates an instance of vectorizedelete-metadata-index-request with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (propertyName: string): ``vectorizedelete-metadata-index-request`` =
        { propertyName = propertyName }

type ``vectorizedelete-metadata-index-response`` =
    { ///The unique identifier for the async mutation operation containing the changeset.
      mutationId: Option<Newtonsoft.Json.Linq.JToken> }
    ///Creates an instance of vectorizedelete-metadata-index-response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizedelete-metadata-index-response`` = { mutationId = None }

type ``vectorizeindex-delete-vectors-by-id-request`` =
    { ///A list of vector identifiers to delete from the index indicated by the path.
      ids: Option<list<``vectorizevector-identifier``>> }
    ///Creates an instance of vectorizeindex-delete-vectors-by-id-request with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizeindex-delete-vectors-by-id-request`` = { ids = None }

type ``vectorizeindex-delete-vectors-by-id-response`` =
    { ///The count of the vectors successfully deleted.
      count: Option<int>
      ///Array of vector identifiers of the vectors that were successfully processed for deletion.
      ids: Option<list<``vectorizevector-identifier``>> }
    ///Creates an instance of vectorizeindex-delete-vectors-by-id-response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizeindex-delete-vectors-by-id-response`` = { count = None; ids = None }

type ``vectorizeindex-delete-vectors-by-id-v2-response`` =
    { ///The unique identifier for the async mutation operation containing the changeset.
      mutationId: Option<Newtonsoft.Json.Linq.JToken> }
    ///Creates an instance of vectorizeindex-delete-vectors-by-id-v2-response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizeindex-delete-vectors-by-id-v2-response`` = { mutationId = None }

type ``vectorizeindex-dimension-configuration`` =
    { ///Specifies the number of dimensions for the index
      dimensions: ``vectorizeindex-dimensions``
      ///Specifies the type of metric to use calculating distance.
      metric: ``vectorizeindex-metric`` }
    ///Creates an instance of vectorizeindex-dimension-configuration with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (dimensions: ``vectorizeindex-dimensions``, metric: ``vectorizeindex-metric``): ``vectorizeindex-dimension-configuration`` =
        { dimensions = dimensions
          metric = metric }

type ``vectorizeindex-get-vectors-by-id-request`` =
    { ///A list of vector identifiers to retrieve from the index indicated by the path.
      ids: Option<list<``vectorizevector-identifier``>> }
    ///Creates an instance of vectorizeindex-get-vectors-by-id-request with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizeindex-get-vectors-by-id-request`` = { ids = None }

type ``vectorizeindex-info-response`` =
    { ///Specifies the number of dimensions for the index
      dimensions: Option<``vectorizeindex-dimensions``>
      ///Specifies the timestamp the last mutation batch was processed as an ISO8601 string.
      processedUpToDatetime: Option<System.DateTimeOffset>
      ///The unique identifier for the async mutation operation containing the changeset.
      processedUpToMutation: Option<Newtonsoft.Json.Linq.JToken>
      ///Specifies the number of vectors present in the index
      vectorCount: Option<int> }
    ///Creates an instance of vectorizeindex-info-response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizeindex-info-response`` =
        { dimensions = None
          processedUpToDatetime = None
          processedUpToMutation = None
          vectorCount = None }

type ``vectorizeindex-insert-response`` =
    { ///Specifies the count of the vectors successfully inserted.
      count: Option<int>
      ///Array of vector identifiers of the vectors successfully inserted.
      ids: Option<list<``vectorizevector-identifier``>> }
    ///Creates an instance of vectorizeindex-insert-response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizeindex-insert-response`` = { count = None; ids = None }

type ``vectorizeindex-insert-v2-response`` =
    { ///The unique identifier for the async mutation operation containing the changeset.
      mutationId: Option<Newtonsoft.Json.Linq.JToken> }
    ///Creates an instance of vectorizeindex-insert-v2-response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizeindex-insert-v2-response`` = { mutationId = None }

type ``vectorizeindex-list-vectors-response`` =
    { ///Number of vectors returned in this response
      count: int
      ///When the cursor expires as an ISO8601 string
      cursorExpirationTimestamp: Option<System.DateTimeOffset>
      ///Whether there are more vectors available beyond this response
      isTruncated: bool
      ///Cursor for the next page of results
      nextCursor: Option<string>
      ///Total number of vectors in the index
      totalCount: int
      ///Array of vector items
      vectors: list<``vectorizevector-list-item``> }
    ///Creates an instance of vectorizeindex-list-vectors-response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (count: int, isTruncated: bool, totalCount: int, vectors: list<``vectorizevector-list-item``>): ``vectorizeindex-list-vectors-response`` =
        { count = count
          cursorExpirationTimestamp = None
          isTruncated = isTruncated
          nextCursor = None
          totalCount = totalCount
          vectors = vectors }

type ``vectorizeindex-preset-configuration`` =
    { ///Specifies the preset to use for the index.
      preset: ``vectorizeindex-preset`` }
    ///Creates an instance of vectorizeindex-preset-configuration with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (preset: ``vectorizeindex-preset``): ``vectorizeindex-preset-configuration`` =
        { preset = preset }

type ``vectorizeindex-query-request`` =
    { ///A metadata filter expression used to limit nearest neighbor results.
      filter: Option<Newtonsoft.Json.Linq.JObject>
      ///Whether to return the metadata associated with the closest vectors.
      returnMetadata: Option<bool>
      ///Whether to return the values associated with the closest vectors.
      returnValues: Option<bool>
      ///The number of nearest neighbors to find.
      topK: Option<float>
      ///The search vector that will be used to find the nearest neighbors.
      vector: list<float> }
    ///Creates an instance of vectorizeindex-query-request with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (vector: list<float>): ``vectorizeindex-query-request`` =
        { filter = None
          returnMetadata = None
          returnValues = None
          topK = None
          vector = vector }

type Matches =
    { ///Identifier for a Vector
      id: Option<``vectorizevector-identifier``>
      metadata: Option<Newtonsoft.Json.Linq.JObject>
      ///The score of the vector according to the index's distance metric
      score: Option<float>
      values: Option<list<float>> }
    ///Creates an instance of Matches with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): Matches =
        { id = None
          metadata = None
          score = None
          values = None }

type ``vectorizeindex-query-response`` =
    { ///Specifies the count of vectors returned by the search
      count: Option<int>
      ///Array of vectors matched by the search
      matches: Option<list<Matches>> }
    ///Creates an instance of vectorizeindex-query-response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizeindex-query-response`` = { count = None; matches = None }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type ReturnMetadata =
    | [<CompiledName "none">] None
    | [<CompiledName "indexed">] Indexed
    | [<CompiledName "all">] All
    member this.Format() =
        match this with
        | None -> "none"
        | Indexed -> "indexed"
        | All -> "all"

type ``vectorizeindex-query-v2-request`` =
    { ///A metadata filter expression used to limit nearest neighbor results.
      filter: Option<Newtonsoft.Json.Linq.JObject>
      ///Whether to return no metadata, indexed metadata or all metadata associated with the closest vectors.
      returnMetadata: Option<ReturnMetadata>
      ///Whether to return the values associated with the closest vectors.
      returnValues: Option<bool>
      ///The number of nearest neighbors to find.
      topK: Option<float>
      ///The search vector that will be used to find the nearest neighbors.
      vector: list<float> }
    ///Creates an instance of vectorizeindex-query-v2-request with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (vector: list<float>): ``vectorizeindex-query-v2-request`` =
        { filter = None
          returnMetadata = None
          returnValues = None
          topK = None
          vector = vector }

type ``vectorizeindex-query-v2-responseMatches`` =
    { ///Identifier for a Vector
      id: Option<``vectorizevector-identifier``>
      metadata: Option<Newtonsoft.Json.Linq.JObject>
      ``namespace``: Option<string>
      ///The score of the vector according to the index's distance metric
      score: Option<float>
      values: Option<list<float>> }
    ///Creates an instance of vectorizeindex-query-v2-responseMatches with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizeindex-query-v2-responseMatches`` =
        { id = None
          metadata = None
          ``namespace`` = None
          score = None
          values = None }

type ``vectorizeindex-query-v2-response`` =
    { ///Specifies the count of vectors returned by the search
      count: Option<int>
      ///Array of vectors matched by the search
      matches: Option<list<``vectorizeindex-query-v2-responseMatches``>> }
    ///Creates an instance of vectorizeindex-query-v2-response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizeindex-query-v2-response`` = { count = None; matches = None }

type ``vectorizeindex-upsert-response`` =
    { ///Specifies the count of the vectors successfully inserted.
      count: Option<int>
      ///Array of vector identifiers of the vectors successfully inserted.
      ids: Option<list<``vectorizevector-identifier``>> }
    ///Creates an instance of vectorizeindex-upsert-response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizeindex-upsert-response`` = { count = None; ids = None }

type ``vectorizeindex-upsert-v2-response`` =
    { ///The unique identifier for the async mutation operation containing the changeset.
      mutationId: Option<Newtonsoft.Json.Linq.JToken> }
    ///Creates an instance of vectorizeindex-upsert-v2-response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizeindex-upsert-v2-response`` = { mutationId = None }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type MetadataIndexesIndexType =
    | [<CompiledName "string">] String
    | [<CompiledName "number">] Number
    | [<CompiledName "boolean">] Boolean
    member this.Format() =
        match this with
        | String -> "string"
        | Number -> "number"
        | Boolean -> "boolean"

type MetadataIndexes =
    { ///Specifies the type of indexed metadata property.
      indexType: Option<MetadataIndexesIndexType>
      ///Specifies the indexed metadata property.
      propertyName: Option<string> }
    ///Creates an instance of MetadataIndexes with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): MetadataIndexes =
        { indexType = None
          propertyName = None }

type ``vectorizelist-metadata-index-response`` =
    { ///Array of indexed metadata properties.
      metadataIndexes: Option<list<MetadataIndexes>> }
    ///Creates an instance of vectorizelist-metadata-index-response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``vectorizelist-metadata-index-response`` = { metadataIndexes = None }

type vectorizeresultinfo =
    { ///Total number of results for the requested service
      count: Option<float>
      ///Current page within paginated list of results
      page: Option<float>
      ///Number of results per page of results
      per_page: Option<float>
      ///Total results available without any search parameters
      total_count: Option<float> }
    ///Creates an instance of vectorizeresultinfo with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizeresultinfo =
        { count = None
          page = None
          per_page = None
          total_count = None }

type ``vectorizeupdate-index-request`` =
    { ///Specifies the description of the index.
      description: ``vectorizeindex-description`` }
    ///Creates an instance of vectorizeupdate-index-request with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (description: ``vectorizeindex-description``): ``vectorizeupdate-index-request`` =
        { description = description }

type ``vectorizevector-list-item`` =
    { ///Identifier for a Vector
      id: ``vectorizevector-identifier`` }
    ///Creates an instance of vectorizevector-list-item with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (id: ``vectorizevector-identifier``): ``vectorizevector-list-item`` = { id = id }

[<RequireQualifiedAccess>]
type ``Vectorize(Deprecated)ListVectorizeIndexes`` =
    ///List Vectorize Index Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type ``Vectorize(Deprecated)CreateVectorizeIndex`` =
    ///Create Vectorize Index Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type ``Vectorize(Deprecated)DeleteVectorizeIndex`` =
    ///Delete Vectorize Index Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type ``Vectorize(Deprecated)GetVectorizeIndex`` =
    ///Get Vectorize Index Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type ``Vectorize(Deprecated)UpdateVectorizeIndex`` =
    ///Update Vectorize Index Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type ``Vectorize(Deprecated)DeleteVectorsById`` =
    ///Delete Vector Identifiers Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type ``Vectorize(Deprecated)GetVectorsById`` =
    ///Get Vectors By Identifier Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type ``Vectorize(Deprecated)InsertVector`` =
    ///Insert Vectors Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type ``Vectorize(Deprecated)QueryVector`` =
    ///Query Vectors Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type ``Vectorize(Deprecated)UpsertVector`` =
    ///Insert Vectors Response
    | OK of payload: string
