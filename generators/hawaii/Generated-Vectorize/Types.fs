namespace rec CloudFlare.Management.Vectorize.Types

///Identifier
type vectorizeidentifier = string
///Specifies the description of the index.
type vectorizeindexdescription = string
type vectorizeindexdimensions = int

type vectorizeindexgetvectorsbyidresponseArrayItem =
    { ///Identifier for a Vector
      id: Option<vectorizevectoridentifier>
      metadata: Option<System.Text.Json.JsonElement>
      ``namespace``: Option<string>
      values: Option<list<float>> }
    ///Creates an instance of vectorizeindexgetvectorsbyidresponseArrayItem with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizeindexgetvectorsbyidresponseArrayItem =
        { id = None
          metadata = None
          ``namespace`` = None
          values = None }

type vectorizeindexgetvectorsbyidresponse = list<vectorizeindexgetvectorsbyidresponseArrayItem>

///Specifies the type of metric to use calculating distance.
[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type vectorizeindexmetric =
    | [<CompiledName "cosine">] Cosine
    | [<CompiledName "euclidean">] Euclidean
    | [<CompiledName "dot-product">] DotProduct
    member this.Format() =
        match this with
        | Cosine -> "cosine"
        | Euclidean -> "euclidean"
        | DotProduct -> "dot-product"

type vectorizeindexname = string

///Specifies the preset to use for the index.
[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type vectorizeindexpreset =
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
type vectorizevectoridentifier = string

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

type vectorizeapiresponsecollection =
    { errors: Option<list<Errors>>
      messages: Option<list<Messages>>
      result: Option<System.Text.Json.JsonElement>
      ///Whether the API call was successful
      success: Option<bool>
      result_info: Option<vectorizeresultinfo> }
    ///Creates an instance of vectorizeapiresponsecollection with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizeapiresponsecollection =
        { errors = None
          messages = None
          result = None
          success = None
          result_info = None }

type vectorizeapiresponsecommonErrors =
    { code: int
      message: string }
    ///Creates an instance of vectorizeapiresponsecommonErrors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): vectorizeapiresponsecommonErrors =
        { code = code; message = message }

type vectorizeapiresponsecommonMessages =
    { code: int
      message: string }
    ///Creates an instance of vectorizeapiresponsecommonMessages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): vectorizeapiresponsecommonMessages =
        { code = code; message = message }

type vectorizeapiresponsecommon =
    { errors: list<vectorizeapiresponsecommonErrors>
      messages: list<vectorizeapiresponsecommonMessages>
      result: System.Text.Json.JsonElement
      ///Whether the API call was successful
      success: bool }
    ///Creates an instance of vectorizeapiresponsecommon with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: list<vectorizeapiresponsecommonErrors>,
                          messages: list<vectorizeapiresponsecommonMessages>,
                          result: System.Text.Json.JsonElement,
                          success: bool): vectorizeapiresponsecommon =
        { errors = errors
          messages = messages
          result = result
          success = success }

type vectorizeapiresponsecommonfailure =
    { errors: System.Text.Json.JsonElement
      messages: System.Text.Json.JsonElement
      result: System.Text.Json.JsonElement
      ///Whether the API call was successful
      success: bool }
    ///Creates an instance of vectorizeapiresponsecommonfailure with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: System.Text.Json.JsonElement,
                          messages: System.Text.Json.JsonElement,
                          result: System.Text.Json.JsonElement,
                          success: bool): vectorizeapiresponsecommonfailure =
        { errors = errors
          messages = messages
          result = result
          success = success }

type vectorizeapiresponsesingleErrors =
    { code: int
      message: string }
    ///Creates an instance of vectorizeapiresponsesingleErrors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): vectorizeapiresponsesingleErrors =
        { code = code; message = message }

type vectorizeapiresponsesingleMessages =
    { code: int
      message: string }
    ///Creates an instance of vectorizeapiresponsesingleMessages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): vectorizeapiresponsesingleMessages =
        { code = code; message = message }

type Result = Map<string, System.Text.Json.JsonElement>

type vectorizeapiresponsesingle =
    { errors: Option<list<vectorizeapiresponsesingleErrors>>
      messages: Option<list<vectorizeapiresponsesingleMessages>>
      result: Option<System.Text.Json.JsonElement>
      ///Whether the API call was successful
      success: Option<bool> }
    ///Creates an instance of vectorizeapiresponsesingle with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizeapiresponsesingle =
        { errors = None
          messages = None
          result = None
          success = None }

type vectorizecreateindexrequest =
    { config: System.Text.Json.JsonElement
      ///Specifies the description of the index.
      description: Option<vectorizeindexdescription>
      name: vectorizeindexname }
    ///Creates an instance of vectorizecreateindexrequest with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (config: System.Text.Json.JsonElement, name: vectorizeindexname): vectorizecreateindexrequest =
        { config = config
          description = None
          name = name }

type vectorizecreateindexresponse =
    { config: Option<vectorizeindexdimensionconfiguration>
      ///Specifies the timestamp the resource was created as an ISO8601 string.
      created_on: Option<System.DateTimeOffset>
      ///Specifies the description of the index.
      description: Option<vectorizeindexdescription>
      ///Specifies the timestamp the resource was modified as an ISO8601 string.
      modified_on: Option<System.DateTimeOffset>
      name: Option<vectorizeindexname> }
    ///Creates an instance of vectorizecreateindexresponse with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizecreateindexresponse =
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

type vectorizecreatemetadataindexrequest =
    { ///Specifies the type of metadata property to index.
      indexType: IndexType
      ///Specifies the metadata property to index.
      propertyName: string }
    ///Creates an instance of vectorizecreatemetadataindexrequest with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (indexType: IndexType, propertyName: string): vectorizecreatemetadataindexrequest =
        { indexType = indexType
          propertyName = propertyName }

type vectorizecreatemetadataindexresponse =
    { ///The unique identifier for the async mutation operation containing the changeset.
      mutationId: Option<System.Text.Json.JsonElement> }
    ///Creates an instance of vectorizecreatemetadataindexresponse with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizecreatemetadataindexresponse = { mutationId = None }

type vectorizedeletemetadataindexrequest =
    { ///Specifies the metadata property for which the index must be deleted.
      propertyName: string }
    ///Creates an instance of vectorizedeletemetadataindexrequest with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (propertyName: string): vectorizedeletemetadataindexrequest = { propertyName = propertyName }

type vectorizedeletemetadataindexresponse =
    { ///The unique identifier for the async mutation operation containing the changeset.
      mutationId: Option<System.Text.Json.JsonElement> }
    ///Creates an instance of vectorizedeletemetadataindexresponse with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizedeletemetadataindexresponse = { mutationId = None }

type vectorizeindexdeletevectorsbyidrequest =
    { ///A list of vector identifiers to delete from the index indicated by the path.
      ids: Option<list<vectorizevectoridentifier>> }
    ///Creates an instance of vectorizeindexdeletevectorsbyidrequest with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizeindexdeletevectorsbyidrequest = { ids = None }

type vectorizeindexdeletevectorsbyidresponse =
    { ///The count of the vectors successfully deleted.
      count: Option<int>
      ///Array of vector identifiers of the vectors that were successfully processed for deletion.
      ids: Option<list<vectorizevectoridentifier>> }
    ///Creates an instance of vectorizeindexdeletevectorsbyidresponse with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizeindexdeletevectorsbyidresponse = { count = None; ids = None }

type vectorizeindexdeletevectorsbyidv2response =
    { ///The unique identifier for the async mutation operation containing the changeset.
      mutationId: Option<System.Text.Json.JsonElement> }
    ///Creates an instance of vectorizeindexdeletevectorsbyidv2response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizeindexdeletevectorsbyidv2response = { mutationId = None }

type vectorizeindexdimensionconfiguration =
    { ///Specifies the number of dimensions for the index
      dimensions: vectorizeindexdimensions
      ///Specifies the type of metric to use calculating distance.
      metric: vectorizeindexmetric }
    ///Creates an instance of vectorizeindexdimensionconfiguration with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (dimensions: vectorizeindexdimensions, metric: vectorizeindexmetric): vectorizeindexdimensionconfiguration =
        { dimensions = dimensions
          metric = metric }

type vectorizeindexgetvectorsbyidrequest =
    { ///A list of vector identifiers to retrieve from the index indicated by the path.
      ids: Option<list<vectorizevectoridentifier>> }
    ///Creates an instance of vectorizeindexgetvectorsbyidrequest with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizeindexgetvectorsbyidrequest = { ids = None }

type vectorizeindexinforesponse =
    { ///Specifies the number of dimensions for the index
      dimensions: Option<vectorizeindexdimensions>
      ///Specifies the timestamp the last mutation batch was processed as an ISO8601 string.
      processedUpToDatetime: Option<System.DateTimeOffset>
      ///The unique identifier for the async mutation operation containing the changeset.
      processedUpToMutation: Option<System.Text.Json.JsonElement>
      ///Specifies the number of vectors present in the index
      vectorCount: Option<int> }
    ///Creates an instance of vectorizeindexinforesponse with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizeindexinforesponse =
        { dimensions = None
          processedUpToDatetime = None
          processedUpToMutation = None
          vectorCount = None }

type vectorizeindexinsertresponse =
    { ///Specifies the count of the vectors successfully inserted.
      count: Option<int>
      ///Array of vector identifiers of the vectors successfully inserted.
      ids: Option<list<vectorizevectoridentifier>> }
    ///Creates an instance of vectorizeindexinsertresponse with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizeindexinsertresponse = { count = None; ids = None }

type vectorizeindexinsertv2response =
    { ///The unique identifier for the async mutation operation containing the changeset.
      mutationId: Option<System.Text.Json.JsonElement> }
    ///Creates an instance of vectorizeindexinsertv2response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizeindexinsertv2response = { mutationId = None }

type vectorizeindexlistvectorsresponse =
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
      vectors: list<vectorizevectorlistitem> }
    ///Creates an instance of vectorizeindexlistvectorsresponse with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (count: int, isTruncated: bool, totalCount: int, vectors: list<vectorizevectorlistitem>): vectorizeindexlistvectorsresponse =
        { count = count
          cursorExpirationTimestamp = None
          isTruncated = isTruncated
          nextCursor = None
          totalCount = totalCount
          vectors = vectors }

type vectorizeindexpresetconfiguration =
    { ///Specifies the preset to use for the index.
      preset: vectorizeindexpreset }
    ///Creates an instance of vectorizeindexpresetconfiguration with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (preset: vectorizeindexpreset): vectorizeindexpresetconfiguration = { preset = preset }

type vectorizeindexqueryrequest =
    { ///A metadata filter expression used to limit nearest neighbor results.
      filter: Option<System.Text.Json.JsonElement>
      ///Whether to return the metadata associated with the closest vectors.
      returnMetadata: Option<bool>
      ///Whether to return the values associated with the closest vectors.
      returnValues: Option<bool>
      ///The number of nearest neighbors to find.
      topK: Option<float>
      ///The search vector that will be used to find the nearest neighbors.
      vector: list<float> }
    ///Creates an instance of vectorizeindexqueryrequest with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (vector: list<float>): vectorizeindexqueryrequest =
        { filter = None
          returnMetadata = None
          returnValues = None
          topK = None
          vector = vector }

type Matches =
    { ///Identifier for a Vector
      id: Option<vectorizevectoridentifier>
      metadata: Option<System.Text.Json.JsonElement>
      ///The score of the vector according to the index's distance metric
      score: Option<float>
      values: Option<list<float>> }
    ///Creates an instance of Matches with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): Matches =
        { id = None
          metadata = None
          score = None
          values = None }

type vectorizeindexqueryresponse =
    { ///Specifies the count of vectors returned by the search
      count: Option<int>
      ///Array of vectors matched by the search
      matches: Option<list<Matches>> }
    ///Creates an instance of vectorizeindexqueryresponse with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizeindexqueryresponse = { count = None; matches = None }

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

type vectorizeindexqueryv2request =
    { ///A metadata filter expression used to limit nearest neighbor results.
      filter: Option<System.Text.Json.JsonElement>
      ///Whether to return no metadata, indexed metadata or all metadata associated with the closest vectors.
      returnMetadata: Option<ReturnMetadata>
      ///Whether to return the values associated with the closest vectors.
      returnValues: Option<bool>
      ///The number of nearest neighbors to find.
      topK: Option<float>
      ///The search vector that will be used to find the nearest neighbors.
      vector: list<float> }
    ///Creates an instance of vectorizeindexqueryv2request with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (vector: list<float>): vectorizeindexqueryv2request =
        { filter = None
          returnMetadata = None
          returnValues = None
          topK = None
          vector = vector }

type vectorizeindexqueryv2responseMatches =
    { ///Identifier for a Vector
      id: Option<vectorizevectoridentifier>
      metadata: Option<System.Text.Json.JsonElement>
      ``namespace``: Option<string>
      ///The score of the vector according to the index's distance metric
      score: Option<float>
      values: Option<list<float>> }
    ///Creates an instance of vectorizeindexqueryv2responseMatches with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizeindexqueryv2responseMatches =
        { id = None
          metadata = None
          ``namespace`` = None
          score = None
          values = None }

type vectorizeindexqueryv2response =
    { ///Specifies the count of vectors returned by the search
      count: Option<int>
      ///Array of vectors matched by the search
      matches: Option<list<vectorizeindexqueryv2responseMatches>> }
    ///Creates an instance of vectorizeindexqueryv2response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizeindexqueryv2response = { count = None; matches = None }

type vectorizeindexupsertresponse =
    { ///Specifies the count of the vectors successfully inserted.
      count: Option<int>
      ///Array of vector identifiers of the vectors successfully inserted.
      ids: Option<list<vectorizevectoridentifier>> }
    ///Creates an instance of vectorizeindexupsertresponse with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizeindexupsertresponse = { count = None; ids = None }

type vectorizeindexupsertv2response =
    { ///The unique identifier for the async mutation operation containing the changeset.
      mutationId: Option<System.Text.Json.JsonElement> }
    ///Creates an instance of vectorizeindexupsertv2response with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizeindexupsertv2response = { mutationId = None }

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

type vectorizelistmetadataindexresponse =
    { ///Array of indexed metadata properties.
      metadataIndexes: Option<list<MetadataIndexes>> }
    ///Creates an instance of vectorizelistmetadataindexresponse with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): vectorizelistmetadataindexresponse = { metadataIndexes = None }

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

type vectorizeupdateindexrequest =
    { ///Specifies the description of the index.
      description: vectorizeindexdescription }
    ///Creates an instance of vectorizeupdateindexrequest with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (description: vectorizeindexdescription): vectorizeupdateindexrequest =
        { description = description }

type vectorizevectorlistitem =
    { ///Identifier for a Vector
      id: vectorizevectoridentifier }
    ///Creates an instance of vectorizevectorlistitem with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (id: vectorizevectoridentifier): vectorizevectorlistitem = { id = id }

[<RequireQualifiedAccess>]
type VectorizeListVectorizeIndexes =
    ///List Vectorize Index Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type VectorizeCreateVectorizeIndex =
    ///Create Vectorize Index Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type VectorizeDeleteVectorizeIndex =
    ///Delete Vectorize Index Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type VectorizeGetVectorizeIndex =
    ///Get Vectorize Index Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type VectorizeDeleteVectorsById =
    ///Delete Vector Identifiers Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type VectorizeGetVectorsById =
    ///Get Vectors By Identifier Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type VectorizeIndexInfo =
    ///Get Vectorize Index Info Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type VectorizeInsertVector =
    ///Insert Vectors Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type VectorizeListVectors =
    ///List Vectors Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type VectorizeCreateMetadataIndex =
    ///Create Metadata Index Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type VectorizeDeleteMetadataIndex =
    ///Delete Metadata Index Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type VectorizeListMetadataIndexes =
    ///List Metadata Index Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type VectorizeQueryVector =
    ///Query Vectors Response
    | OK of payload: string

[<RequireQualifiedAccess>]
type VectorizeUpsertVector =
    ///Upsert Vectors Response
    | OK of payload: string
