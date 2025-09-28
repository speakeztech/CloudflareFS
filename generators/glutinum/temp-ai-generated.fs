module rec Glutinum

open Fable.Core
open Fable.Core.JsInterop
open System

[<AbstractClass>]
[<Erase>]
type Exports =
    [<Import("Ai", "REPLACE_ME_WITH_MODULE_NAME"); EmitConstructor>]
    static member Ai<'AiModelList when 'AiModelList :> AiModelListType> () : Ai<'AiModelList> = nativeOnly
    [<Import("AiGateway", "REPLACE_ME_WITH_MODULE_NAME"); EmitConstructor>]
    static member AiGateway () : AiGateway = nativeOnly
    [<Import("AutoRAG", "REPLACE_ME_WITH_MODULE_NAME"); EmitConstructor>]
    static member AutoRAG () : AutoRAG = nativeOnly

[<AllowNullLiteral>]
[<Interface>]
type Ai<'AiModelList when 'AiModelList :> AiModelListType> =
    abstract member aiGatewayLogId: U2<string, obj> with get, set
    abstract member gateway: gatewayId: string -> AiGateway
    abstract member autorag: autoragId: string -> AutoRAG
    abstract member run: model: 'Name * inputs: 'InputOptions * ?options: 'Options -> JS.Promise<'Options>
    abstract member models: ?``params``: AiModelsSearchParams -> JS.Promise<ResizeArray<AiModelsSearchObject>>
    abstract member toMarkdown: files: ResizeArray<Ai.toMarkdown.files> * ?options: Ai.toMarkdown.options -> JS.Promise<ResizeArray<ConversionResponse>>
    abstract member toMarkdown: files: Ai.toMarkdown.files * ?options: Ai.toMarkdown.options -> JS.Promise<ConversionResponse>

[<AllowNullLiteral>]
[<Interface>]
type GatewayRetries =
    abstract member maxAttempts: GatewayRetries.maxAttempts option with get, set
    abstract member retryDelayMs: float option with get, set
    abstract member backoff: GatewayRetries.backoff option with get, set

[<AllowNullLiteral>]
[<Interface>]
type GatewayOptions =
    abstract member id: string with get, set
    abstract member cacheKey: string option with get, set
    abstract member cacheTtl: float option with get, set
    abstract member skipCache: bool option with get, set
    abstract member metadata: GatewayOptions.metadata option with get, set
    abstract member collectLog: bool option with get, set
    abstract member eventId: string option with get, set
    abstract member requestTimeoutMs: float option with get, set
    abstract member retries: GatewayRetries option with get, set

type UniversalGatewayOptions =
    obj

[<AllowNullLiteral>]
[<Interface>]
type AiGatewayPatchLog =
    abstract member score: U2<float, obj> option with get, set
    abstract member feedback: U3<int, int, obj> option with get, set
    abstract member metadata: U2<AiGatewayPatchLog.metadata.U2.Case1, obj> option with get, set

[<AllowNullLiteral>]
[<Interface>]
type AiGatewayLog =
    abstract member id: string with get, set
    abstract member provider: string with get, set
    abstract member model: string with get, set
    abstract member model_type: string option with get, set
    abstract member path: string with get, set
    abstract member duration: float with get, set
    abstract member request_type: string option with get, set
    abstract member request_content_type: string option with get, set
    abstract member status_code: float with get, set
    abstract member response_content_type: string option with get, set
    abstract member success: bool with get, set
    abstract member cached: bool with get, set
    abstract member tokens_in: float option with get, set
    abstract member tokens_out: float option with get, set
    abstract member metadata: AiGatewayLog.metadata option with get, set
    abstract member step: float option with get, set
    abstract member cost: float option with get, set
    abstract member custom_cost: bool option with get, set
    abstract member request_size: float with get, set
    abstract member request_head: string option with get, set
    abstract member request_head_complete: bool with get, set
    abstract member response_size: float with get, set
    abstract member response_head: string option with get, set
    abstract member response_head_complete: bool with get, set
    abstract member created_at: JS.Date with get, set

[<RequireQualifiedAccess>]
[<StringEnum(CaseRules.None)>]
type AIGatewayProviders =
    | ``workers-ai``
    | anthropic
    | ``aws-bedrock``
    | ``azure-openai``
    | ``google-vertex-ai``
    | huggingface
    | openai
    | ``perplexity-ai``
    | replicate
    | groq
    | cohere
    | ``google-ai-studio``
    | mistral
    | grok
    | openrouter
    | deepseek
    | cerebras
    | cartesia
    | elevenlabs
    | ``adobe-firefly``

[<AllowNullLiteral>]
[<Interface>]
type AIGatewayHeaders =
    abstract member ``cf-aig-metadata``: U2<AIGatewayHeaders.````cf-aig-metadata````.U2.Case1, string> with get, set
    abstract member ``cf-aig-custom-cost``: U3<AIGatewayHeaders.````cf-aig-custom-cost````.U3.Case1, AIGatewayHeaders.````cf-aig-custom-cost````.U3.Case2, string> with get, set
    abstract member ``cf-aig-cache-ttl``: U2<float, string> with get, set
    abstract member ``cf-aig-skip-cache``: U2<bool, string> with get, set
    abstract member ``cf-aig-cache-key``: string with get, set
    abstract member ``cf-aig-event-id``: string with get, set
    abstract member ``cf-aig-request-timeout``: U2<float, string> with get, set
    abstract member ``cf-aig-max-attempts``: U2<float, string> with get, set
    abstract member ``cf-aig-retry-delay``: U2<float, string> with get, set
    abstract member ``cf-aig-backoff``: string with get, set
    abstract member ``cf-aig-collect-log``: U2<bool, string> with get, set
    abstract member Authorization: string with get, set
    abstract member ``Content-Type``: string with get, set
    [<EmitIndexer>]
    abstract member Item: key: string -> U4<string, float, bool, obj> with get, set

[<AllowNullLiteral>]
[<Interface>]
type AIGatewayUniversalRequest =
    abstract member provider: U2<AIGatewayProviders, string> with get, set
    abstract member endpoint: string with get, set
    abstract member headers: AIGatewayUniversalRequest.headers with get, set
    abstract member query: obj with get, set

[<AllowNullLiteral>]
[<Interface>]
type AiGatewayInternalError =
    inherit Error

[<AllowNullLiteral>]
[<Interface>]
type AiGatewayLogNotFound =
    inherit Error

[<AllowNullLiteral>]
[<Interface>]
type AiGateway =
    abstract member patchLog: logId: string * data: AiGatewayPatchLog -> JS.Promise<unit>
    abstract member getLog: logId: string -> JS.Promise<AiGatewayLog>
    abstract member run: data: U2<AIGatewayUniversalRequest, ResizeArray<AIGatewayUniversalRequest>> * ?options: AiGateway.run.options -> JS.Promise<Response>
    abstract member getUrl: ?provider: U2<AIGatewayProviders, string> -> JS.Promise<string>

[<AllowNullLiteral>]
[<Interface>]
type AutoRAGInternalError =
    inherit Error

[<AllowNullLiteral>]
[<Interface>]
type AutoRAGNotFoundError =
    inherit Error

[<AllowNullLiteral>]
[<Interface>]
type AutoRAGUnauthorizedError =
    inherit Error

[<AllowNullLiteral>]
[<Interface>]
type AutoRAGNameNotSetError =
    inherit Error

[<AllowNullLiteral>]
[<Interface>]
type ComparisonFilter =
    abstract member key: string with get, set
    abstract member ``type``: ComparisonFilter.``type`` with get, set
    abstract member value: U3<string, float, bool> with get, set

[<AllowNullLiteral>]
[<Interface>]
type CompoundFilter =
    abstract member ``type``: CompoundFilter.``type`` with get, set
    abstract member filters: ResizeArray<ComparisonFilter> with get, set

[<AllowNullLiteral>]
[<Interface>]
type AutoRagSearchRequest =
    abstract member query: string with get, set
    abstract member filters: U2<CompoundFilter, ComparisonFilter> option with get, set
    abstract member max_num_results: float option with get, set
    abstract member ranking_options: AutoRagSearchRequest.ranking_options option with get, set
    abstract member rewrite_query: bool option with get, set

[<AllowNullLiteral>]
[<Interface>]
type AutoRagAiSearchRequest =
    abstract member query: string with get, set
    abstract member filters: U2<CompoundFilter, ComparisonFilter> option with get, set
    abstract member max_num_results: float option with get, set
    abstract member ranking_options: AutoRagAiSearchRequest.ranking_options option with get, set
    abstract member rewrite_query: bool option with get, set
    abstract member stream: bool option with get, set
    abstract member system_prompt: string option with get, set

[<AllowNullLiteral>]
[<Interface>]
type AutoRagAiSearchRequestStreaming =
    abstract member query: string with get, set
    abstract member filters: U2<CompoundFilter, ComparisonFilter> option with get, set
    abstract member max_num_results: float option with get, set
    abstract member ranking_options: AutoRagAiSearchRequestStreaming.ranking_options option with get, set
    abstract member rewrite_query: bool option with get, set
    abstract member system_prompt: string option with get, set
    abstract member stream: bool with get, set

[<AllowNullLiteral>]
[<Interface>]
type AutoRagSearchResponse =
    abstract member ``object``: string with get, set
    abstract member search_query: string with get, set
    abstract member data: ResizeArray<AutoRagSearchResponse.data> with get, set
    abstract member has_more: bool with get, set
    abstract member next_page: U2<string, obj> with get, set

type AutoRagListResponse =
    ResizeArray<AutoRagListResponse>

[<AllowNullLiteral>]
[<Interface>]
type AutoRagAiSearchResponse =
    abstract member ``object``: string with get, set
    abstract member search_query: string with get, set
    abstract member data: ResizeArray<AutoRagAiSearchResponse.data> with get, set
    abstract member has_more: bool with get, set
    abstract member next_page: U2<string, obj> with get, set
    abstract member response: string with get, set

[<AllowNullLiteral>]
[<Interface>]
type AutoRAG =
    abstract member list: unit -> JS.Promise<AutoRagListResponse>
    abstract member search: ``params``: AutoRagSearchRequest -> JS.Promise<AutoRagSearchResponse>
    abstract member aiSearch: ``params``: AutoRagAiSearchRequestStreaming -> JS.Promise<Response>
    abstract member aiSearch: ``params``: AutoRagAiSearchRequest -> JS.Promise<AutoRagAiSearchResponse>
    abstract member aiSearch: ``params``: AutoRagAiSearchRequest -> JS.Promise<U2<AutoRagAiSearchResponse, Response>>

[<AllowNullLiteral>]
[<Interface>]
type BasicImageTransformations =
    /// <summary>
    /// Maximum width in image pixels. The value must be an integer.
    /// </summary>
    abstract member width: float option with get, set
    /// <summary>
    /// Maximum height in image pixels. The value must be an integer.
    /// </summary>
    abstract member height: float option with get, set
    /// <summary>
    /// Resizing mode as a string. It affects interpretation of width and height
    /// options:
    ///  - scale-down: Similar to contain, but the image is never enlarged. If
    ///    the image is larger than given width or height, it will be resized.
    ///    Otherwise its original size will be kept.
    ///  - contain: Resizes to maximum size that fits within the given width and
    ///    height. If only a single dimension is given (e.g. only width), the
    ///    image will be shrunk or enlarged to exactly match that dimension.
    ///    Aspect ratio is always preserved.
    ///  - cover: Resizes (shrinks or enlarges) to fill the entire area of width
    ///    and height. If the image has an aspect ratio different from the ratio
    ///    of width and height, it will be cropped to fit.
    ///  - crop: The image will be shrunk and cropped to fit within the area
    ///    specified by width and height. The image will not be enlarged. For images
    ///    smaller than the given dimensions it's the same as scale-down. For
    ///    images larger than the given dimensions, it's the same as cover.
    ///    See also trim.
    ///  - pad: Resizes to the maximum size that fits within the given width and
    ///    height, and then fills the remaining area with a background color
    ///    (white by default). Use of this mode is not recommended, as the same
    ///    effect can be more efficiently achieved with the contain mode and the
    ///    CSS object-fit: contain property.
    ///  - squeeze: Stretches and deforms to the width and height given, even if it
    ///    breaks aspect ratio
    /// </summary>
    abstract member fit: BasicImageTransformations.fit option with get, set
    /// <summary>
    /// Image segmentation using artificial intelligence models. Sets pixels not
    /// within selected segment area to transparent e.g "foreground" sets every
    /// background pixel as transparent.
    /// </summary>
    abstract member segment: string option with get, set
    /// <summary>
    /// When cropping with fit: "cover", this defines the side or point that should
    /// be left uncropped. The value is either a string
    /// "left", "right", "top", "bottom", "auto", or "center" (the default),
    /// or an object {x, y} containing focal point coordinates in the original
    /// image expressed as fractions ranging from 0.0 (top or left) to 1.0
    /// (bottom or right), 0.5 being the center. {fit: "cover", gravity: "top"} will
    /// crop bottom or left and right sides as necessary, but won’t crop anything
    /// from the top. {fit: "cover", gravity: {x:0.5, y:0.2}} will crop each side to
    /// preserve as much as possible around a point at 20% of the height of the
    /// source image.
    /// </summary>
    abstract member gravity: BasicImageTransformations.gravity option with get, set
    /// <summary>
    /// Background color to add underneath the image. Applies only to images with
    /// transparency (such as PNG). Accepts any CSS color (#RRGGBB, rgba(…),
    /// hsl(…), etc.)
    /// </summary>
    abstract member background: string option with get, set
    /// <summary>
    /// Number of degrees (90, 180, 270) to rotate the image by. width and height
    /// options refer to axes after rotation.
    /// </summary>
    abstract member rotate: BasicImageTransformations.rotate option with get, set

[<AllowNullLiteral>]
[<Interface>]
type BasicImageTransformationsGravityCoordinates =
    abstract member x: float option with get, set
    abstract member y: float option with get, set
    abstract member mode: BasicImageTransformationsGravityCoordinates.mode option with get, set

[<AllowNullLiteral>]
[<Interface>]
type RequestInitCfProperties =
    inherit Record<string, obj>
    abstract member cacheEverything: bool option with get, set
    /// <summary>
    /// A request's cache key is what determines if two requests are
    /// "the same" for caching purposes. If a request has the same cache key
    /// as some previous request, then we can serve the same cached response for
    /// both. (e.g. 'some-key')
    ///
    /// Only available for Enterprise customers.
    /// </summary>
    abstract member cacheKey: string option with get, set
    /// <summary>
    /// This allows you to append additional Cache-Tag response headers
    /// to the origin response without modifications to the origin server.
    /// This will allow for greater control over the Purge by Cache Tag feature
    /// utilizing changes only in the Workers process.
    ///
    /// Only available for Enterprise customers.
    /// </summary>
    abstract member cacheTags: ResizeArray<string> option with get, set
    /// <summary>
    /// Force response to be cached for a given number of seconds. (e.g. 300)
    /// </summary>
    abstract member cacheTtl: float option with get, set
    /// <summary>
    /// Force response to be cached for a given number of seconds based on the Origin status code.
    /// (e.g. { '200-299': 86400, '404': 1, '500-599': 0 })
    /// </summary>
    abstract member cacheTtlByStatus: RequestInitCfProperties.cacheTtlByStatus option with get, set

[<Global>]
[<AllowNullLiteral>]
type AutoRagListResponse
    [<ParamObject; Emit("$0")>]
    (
        id: string,
        enable: bool,
        ``type``: string,
        source: string,
        vectorize_name: string,
        paused: bool,
        status: string
    ) =

    member val id : string = nativeOnly with get, set
    member val enable : bool = nativeOnly with get, set
    member val ``type`` : string = nativeOnly with get, set
    member val source : string = nativeOnly with get, set
    member val vectorize_name : string = nativeOnly with get, set
    member val paused : bool = nativeOnly with get, set
    member val status : string = nativeOnly with get, set

module Ai =

    module toMarkdown =

        [<Global>]
        [<AllowNullLiteral>]
        type files
            [<ParamObject; Emit("$0")>]
            (
                name: string,
                blob: Blob
            ) =

            member val name : string = nativeOnly with get, set
            member val blob : Blob = nativeOnly with get, set

        [<Global>]
        [<AllowNullLiteral>]
        type options
            [<ParamObject; Emit("$0")>]
            (
                ?gateway: GatewayOptions,
                ?extraHeaders: obj
            ) =

            member val gateway : GatewayOptions option = nativeOnly with get, set
            member val extraHeaders : obj option = nativeOnly with get, set

        [<Global>]
        [<AllowNullLiteral>]
        type files
            [<ParamObject; Emit("$0")>]
            (
                name: string,
                blob: Blob
            ) =

            member val name : string = nativeOnly with get, set
            member val blob : Blob = nativeOnly with get, set

        [<Global>]
        [<AllowNullLiteral>]
        type options
            [<ParamObject; Emit("$0")>]
            (
                ?gateway: GatewayOptions,
                ?extraHeaders: obj
            ) =

            member val gateway : GatewayOptions option = nativeOnly with get, set
            member val extraHeaders : obj option = nativeOnly with get, set

module GatewayRetries =

    [<RequireQualifiedAccess>]
    type maxAttempts =
        | ``1`` = 1
        | ``2`` = 2
        | ``3`` = 3
        | ``4`` = 4
        | ``5`` = 5

    [<RequireQualifiedAccess>]
    [<StringEnum(CaseRules.None)>]
    type backoff =
        | constant
        | linear
        | exponential

module GatewayOptions =

    [<AllowNullLiteral>]
    [<Interface>]
    type metadata =
        [<EmitIndexer>]
        abstract member Item: key: string -> U5<float, string, bool, obj, obj> with get, set

module AiGatewayPatchLog =

    module metadata =

        module U2 =

            [<AllowNullLiteral>]
            [<Interface>]
            type Case1 =
                [<EmitIndexer>]
                abstract member Item: key: string -> U5<float, string, bool, obj, obj> with get, set

module AiGatewayLog =

    [<AllowNullLiteral>]
    [<Interface>]
    type metadata =
        [<EmitIndexer>]
        abstract member Item: key: string -> U5<float, string, bool, obj, obj> with get, set

module AIGatewayHeaders =

    module ````cf-aig-metadata```` =

        module U2 =

            [<AllowNullLiteral>]
            [<Interface>]
            type Case1 =
                [<EmitIndexer>]
                abstract member Item: key: string -> U5<float, string, bool, obj, obj> with get, set

    module ````cf-aig-custom-cost```` =

        module U3 =

            [<Global>]
            [<AllowNullLiteral>]
            type Case1
                [<ParamObject; Emit("$0")>]
                (
                    ?per_token_in: float,
                    ?per_token_out: float
                ) =

                member val per_token_in : float option = nativeOnly with get, set
                member val per_token_out : float option = nativeOnly with get, set

            [<Global>]
            [<AllowNullLiteral>]
            type Case2
                [<ParamObject; Emit("$0")>]
                (
                    ?total_cost: float
                ) =

                member val total_cost : float option = nativeOnly with get, set

module AIGatewayUniversalRequest =

    [<AllowNullLiteral>]
    [<Interface>]
    type headers =
        abstract member ``cf-aig-metadata``: U2<AIGatewayUniversalRequest.headers.Partial.````cf-aig-metadata````.U2.Case1, string> option with get, set
        abstract member ``cf-aig-custom-cost``: U3<AIGatewayUniversalRequest.headers.Partial.````cf-aig-custom-cost````.U3.Case1, AIGatewayUniversalRequest.headers.Partial.````cf-aig-custom-cost````.U3.Case2, string> option with get, set
        abstract member ``cf-aig-cache-ttl``: U2<float, string> option with get, set
        abstract member ``cf-aig-skip-cache``: U2<bool, string> option with get, set
        abstract member ``cf-aig-cache-key``: string option with get, set
        abstract member ``cf-aig-event-id``: string option with get, set
        abstract member ``cf-aig-request-timeout``: U2<float, string> option with get, set
        abstract member ``cf-aig-max-attempts``: U2<float, string> option with get, set
        abstract member ``cf-aig-retry-delay``: U2<float, string> option with get, set
        abstract member ``cf-aig-backoff``: string option with get, set
        abstract member ``cf-aig-collect-log``: U2<bool, string> option with get, set
        abstract member Authorization: string option with get, set
        abstract member ``Content-Type``: string option with get, set

    module headers =

        module Partial =

            module ````cf-aig-metadata```` =

                module U2 =

                    [<AllowNullLiteral>]
                    [<Interface>]
                    type Case1 =
                        [<EmitIndexer>]
                        abstract member Item: key: string -> U5<float, string, bool, obj, obj> with get, set

            module ````cf-aig-custom-cost```` =

                module U3 =

                    [<Global>]
                    [<AllowNullLiteral>]
                    type Case1
                        [<ParamObject; Emit("$0")>]
                        (
                            ?per_token_in: float,
                            ?per_token_out: float
                        ) =

                        member val per_token_in : float option = nativeOnly with get, set
                        member val per_token_out : float option = nativeOnly with get, set

                    [<Global>]
                    [<AllowNullLiteral>]
                    type Case2
                        [<ParamObject; Emit("$0")>]
                        (
                            ?total_cost: float
                        ) =

                        member val total_cost : float option = nativeOnly with get, set

module AiGateway =

    module run =

        [<Global>]
        [<AllowNullLiteral>]
        type options
            [<ParamObject; Emit("$0")>]
            (
                ?gateway: UniversalGatewayOptions,
                ?extraHeaders: obj
            ) =

            member val gateway : UniversalGatewayOptions option = nativeOnly with get, set
            member val extraHeaders : obj option = nativeOnly with get, set

module ComparisonFilter =

    [<RequireQualifiedAccess>]
    [<StringEnum(CaseRules.None)>]
    type ``type`` =
        | eq
        | ne
        | gt
        | gte
        | lt
        | lte

module CompoundFilter =

    [<RequireQualifiedAccess>]
    [<StringEnum(CaseRules.None)>]
    type ``type`` =
        | ``and``
        | ``or``

module AutoRagSearchRequest =

    [<Global>]
    [<AllowNullLiteral>]
    type ranking_options
        [<ParamObject; Emit("$0")>]
        (
            ?ranker: string,
            ?score_threshold: float
        ) =

        member val ranker : string option = nativeOnly with get, set
        member val score_threshold : float option = nativeOnly with get, set

module AutoRagAiSearchRequest =

    [<Global>]
    [<AllowNullLiteral>]
    type ranking_options
        [<ParamObject; Emit("$0")>]
        (
            ?ranker: string,
            ?score_threshold: float
        ) =

        member val ranker : string option = nativeOnly with get, set
        member val score_threshold : float option = nativeOnly with get, set

module AutoRagAiSearchRequestStreaming =

    [<Global>]
    [<AllowNullLiteral>]
    type ranking_options
        [<ParamObject; Emit("$0")>]
        (
            ?ranker: string,
            ?score_threshold: float
        ) =

        member val ranker : string option = nativeOnly with get, set
        member val score_threshold : float option = nativeOnly with get, set

module AutoRagSearchResponse =

    [<Global>]
    [<AllowNullLiteral>]
    type data
        [<ParamObject; Emit("$0")>]
        (
            file_id: string,
            filename: string,
            score: float,
            attributes: AutoRagSearchResponse.data.attributes,
            content: ResizeArray<AutoRagSearchResponse.data.content>
        ) =

        member val file_id : string = nativeOnly with get, set
        member val filename : string = nativeOnly with get, set
        member val score : float = nativeOnly with get, set
        member val attributes : AutoRagSearchResponse.data.attributes = nativeOnly with get, set
        member val content : ResizeArray<AutoRagSearchResponse.data.content> = nativeOnly with get, set

    module data =

        [<AllowNullLiteral>]
        [<Interface>]
        type attributes =
            [<EmitIndexer>]
            abstract member Item: key: string -> U4<string, float, bool, obj> with get, set

        [<Global>]
        [<AllowNullLiteral>]
        type content
            [<ParamObject; Emit("$0")>]
            (
                ``type``: string,
                text: string
            ) =

            member val ``type`` : string = nativeOnly with get, set
            member val text : string = nativeOnly with get, set

module AutoRagAiSearchResponse =

    [<Global>]
    [<AllowNullLiteral>]
    type data
        [<ParamObject; Emit("$0")>]
        (
            file_id: string,
            filename: string,
            score: float,
            attributes: AutoRagAiSearchResponse.data.attributes,
            content: ResizeArray<AutoRagAiSearchResponse.data.content>
        ) =

        member val file_id : string = nativeOnly with get, set
        member val filename : string = nativeOnly with get, set
        member val score : float = nativeOnly with get, set
        member val attributes : AutoRagAiSearchResponse.data.attributes = nativeOnly with get, set
        member val content : ResizeArray<AutoRagAiSearchResponse.data.content> = nativeOnly with get, set

    module data =

        [<AllowNullLiteral>]
        [<Interface>]
        type attributes =
            [<EmitIndexer>]
            abstract member Item: key: string -> U4<string, float, bool, obj> with get, set

        [<Global>]
        [<AllowNullLiteral>]
        type content
            [<ParamObject; Emit("$0")>]
            (
                ``type``: string,
                text: string
            ) =

            member val ``type`` : string = nativeOnly with get, set
            member val text : string = nativeOnly with get, set

module BasicImageTransformations =

    [<RequireQualifiedAccess>]
    [<StringEnum(CaseRules.None)>]
    type fit =
        | ``scale-down``
        | contain
        | cover
        | crop
        | pad
        | squeeze

    [<RequireQualifiedAccess>]
    [<StringEnum(CaseRules.None)>]
    type gravity =
        | face
        | left
        | right
        | top
        | bottom
        | center
        | auto
        | entropy

    [<RequireQualifiedAccess>]
    type rotate =
        | ``0`` = 0
        | ``90`` = 90
        | ``180`` = 180
        | ``270`` = 270
        | ``360`` = 360

module BasicImageTransformationsGravityCoordinates =

    [<RequireQualifiedAccess>]
    [<StringEnum(CaseRules.None)>]
    type mode =
        | remainder
        | ``box-center``

module RequestInitCfProperties =

    [<AllowNullLiteral>]
    [<Interface>]
    type cacheTtlByStatus =
        [<EmitIndexer>]
        abstract member Item: key: string -> float with get, set
