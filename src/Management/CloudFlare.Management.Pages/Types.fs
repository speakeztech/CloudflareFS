namespace rec CloudFlare.Management.Pages.Types

///The domain name.
type pagesdomainname = string
///Identifier.
type pagesidentifier = string

type Source =
    { pointer: Option<string> }
    ///Creates an instance of Source with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): Source = { pointer = None }

type pagesmessagesArrayItem =
    { code: int
      documentation_url: Option<string>
      message: string
      source: Option<Source> }
    ///Creates an instance of pagesmessagesArrayItem with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): pagesmessagesArrayItem =
        { code = code
          documentation_url = None
          message = message
          source = None }

type pagesmessages = list<pagesmessagesArrayItem>
///Name of the project.
type pagesprojectname = string

type ErrorsSource =
    { pointer: Option<string> }
    ///Creates an instance of ErrorsSource with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ErrorsSource = { pointer = None }

type Errors =
    { code: int
      documentation_url: Option<string>
      message: string
      source: Option<ErrorsSource> }
    ///Creates an instance of Errors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): Errors =
        { code = code
          documentation_url = None
          message = message
          source = None }

type MessagesSource =
    { pointer: Option<string> }
    ///Creates an instance of MessagesSource with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): MessagesSource = { pointer = None }

type Messages =
    { code: int
      documentation_url: Option<string>
      message: string
      source: Option<MessagesSource> }
    ///Creates an instance of Messages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): Messages =
        { code = code
          documentation_url = None
          message = message
          source = None }

type Resultinfo =
    { ///Total number of results for the requested service.
      count: Option<float>
      ///Current page within paginated list of results.
      page: Option<float>
      ///Number of results per page of results.
      per_page: Option<float>
      ///Total results available without any search parameters.
      total_count: Option<float> }
    ///Creates an instance of Resultinfo with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): Resultinfo =
        { count = None
          page = None
          per_page = None
          total_count = None }

type ``pagesapi-response-collection`` =
    { errors: Option<list<Errors>>
      messages: Option<list<Messages>>
      ///Whether the API call was successful.
      success: Option<bool>
      result_info: Option<Resultinfo> }
    ///Creates an instance of pagesapi-response-collection with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``pagesapi-response-collection`` =
        { errors = None
          messages = None
          success = None
          result_info = None }

type ``pagesapi-response-commonErrorsSource`` =
    { pointer: Option<string> }
    ///Creates an instance of pagesapi-response-commonErrorsSource with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``pagesapi-response-commonErrorsSource`` = { pointer = None }

type ``pagesapi-response-commonErrors`` =
    { code: int
      documentation_url: Option<string>
      message: string
      source: Option<``pagesapi-response-commonErrorsSource``> }
    ///Creates an instance of pagesapi-response-commonErrors with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``pagesapi-response-commonErrors`` =
        { code = code
          documentation_url = None
          message = message
          source = None }

type ``pagesapi-response-commonMessagesSource`` =
    { pointer: Option<string> }
    ///Creates an instance of pagesapi-response-commonMessagesSource with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): ``pagesapi-response-commonMessagesSource`` = { pointer = None }

type ``pagesapi-response-commonMessages`` =
    { code: int
      documentation_url: Option<string>
      message: string
      source: Option<``pagesapi-response-commonMessagesSource``> }
    ///Creates an instance of pagesapi-response-commonMessages with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (code: int, message: string): ``pagesapi-response-commonMessages`` =
        { code = code
          documentation_url = None
          message = message
          source = None }

type ``pagesapi-response-common`` =
    { errors: list<``pagesapi-response-commonErrors``>
      messages: list<``pagesapi-response-commonMessages``>
      result: Option<System.Text.Json.JsonElement>
      ///Whether the API call was successful.
      success: bool }
    ///Creates an instance of pagesapi-response-common with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: list<``pagesapi-response-commonErrors``>,
                          messages: list<``pagesapi-response-commonMessages``>,
                          success: bool): ``pagesapi-response-common`` =
        { errors = errors
          messages = messages
          result = None
          success = success }

type ``pagesapi-response-common-failure`` =
    { errors: Newtonsoft.Json.Linq.JToken
      messages: Newtonsoft.Json.Linq.JToken
      result: Newtonsoft.Json.Linq.JObject
      ///Whether the API call was successful.
      success: bool }
    ///Creates an instance of pagesapi-response-common-failure with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (errors: Newtonsoft.Json.Linq.JToken,
                          messages: Newtonsoft.Json.Linq.JToken,
                          result: Newtonsoft.Json.Linq.JObject,
                          success: bool): ``pagesapi-response-common-failure`` =
        { errors = errors
          messages = messages
          result = result
          success = success }

///Configs for the project build process.
type pagesbuildconfig =
    { ///Enable build caching for the project.
      build_caching: Option<bool>
      ///Command used to build project.
      build_command: Option<string>
      ///Assets output directory of the build.
      destination_dir: Option<string>
      ///Directory to run the command.
      root_dir: Option<string>
      ///The classifying tag for analytics.
      web_analytics_tag: string
      ///The auth token for analytics.
      web_analytics_token: string }
    ///Creates an instance of pagesbuildconfig with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (web_analytics_tag: string, web_analytics_token: string): pagesbuildconfig =
        { build_caching = None
          build_command = None
          destination_dir = None
          root_dir = None
          web_analytics_tag = web_analytics_tag
          web_analytics_token = web_analytics_token }

///Additional info about the trigger.
type Metadata =
    { ///Where the trigger happened.
      branch: string
      ///Whether the deployment trigger commit was dirty.
      commit_dirty: bool
      ///Hash of the deployment trigger commit.
      commit_hash: string
      ///Message of the deployment trigger commit.
      commit_message: string }
    ///Creates an instance of Metadata with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (branch: string, commit_dirty: bool, commit_hash: string, commit_message: string): Metadata =
        { branch = branch
          commit_dirty = commit_dirty
          commit_hash = commit_hash
          commit_message = commit_message }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type Type =
    | [<CompiledName "github:push">] ``Github:push``
    | [<CompiledName "ad_hoc">] Ad_hoc
    | [<CompiledName "deploy_hook">] Deploy_hook
    member this.Format() =
        match this with
        | ``Github:push`` -> "github:push"
        | Ad_hoc -> "ad_hoc"
        | Deploy_hook -> "deploy_hook"

///Info about what caused the deployment.
type Deploymenttrigger =
    { ///Additional info about the trigger.
      metadata: Metadata
      ///What caused the deployment.
      ``type``: Type }
    ///Creates an instance of Deploymenttrigger with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (metadata: Metadata, ``type``: Type): Deploymenttrigger =
        { metadata = metadata
          ``type`` = ``type`` }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type Environment =
    | [<CompiledName "preview">] Preview
    | [<CompiledName "production">] Production
    member this.Format() =
        match this with
        | Preview -> "preview"
        | Production -> "production"

type pagesdeployment =
    { ///A list of alias URLs pointing to this deployment.
      aliases: list<string>
      ///Configs for the project build process.
      build_config: pagesbuildconfig
      ///When the deployment was created.
      created_on: System.DateTimeOffset
      ///Info about what caused the deployment.
      deployment_trigger: Deploymenttrigger
      ///Environment variables used for builds and Pages Functions.
      env_vars: Map<string, Option<string>>
      ///Type of deploy.
      environment: Environment
      ///Id of the deployment.
      id: string
      ///If the deployment has been skipped.
      is_skipped: bool
      ///The status of the deployment.
      latest_stage: pagesstage
      ///When the deployment was last modified.
      modified_on: System.DateTimeOffset
      ///Id of the project.
      project_id: string
      ///Name of the project.
      project_name: pagesprojectname
      ///Short Id (8 character) of the deployment.
      short_id: string
      ///Configs for the project source control.
      source: pagessource
      ///List of past stages.
      stages: list<pagesstage>
      ///The live URL to view this deployment.
      url: string
      ///Whether the deployment uses functions.
      uses_functions: Option<bool> }
    ///Creates an instance of pagesdeployment with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (aliases: list<string>,
                          build_config: pagesbuildconfig,
                          created_on: System.DateTimeOffset,
                          deployment_trigger: Deploymenttrigger,
                          env_vars: Map<string, Option<string>>,
                          environment: Environment,
                          id: string,
                          is_skipped: bool,
                          latest_stage: pagesstage,
                          modified_on: System.DateTimeOffset,
                          project_id: string,
                          project_name: pagesprojectname,
                          short_id: string,
                          source: pagessource,
                          stages: list<pagesstage>,
                          url: string): pagesdeployment =
        { aliases = aliases
          build_config = build_config
          created_on = created_on
          deployment_trigger = deployment_trigger
          env_vars = env_vars
          environment = environment
          id = id
          is_skipped = is_skipped
          latest_stage = latest_stage
          modified_on = modified_on
          project_id = project_id
          project_name = project_name
          short_id = short_id
          source = source
          stages = stages
          url = url
          uses_functions = None }

///Limits for Pages Functions.
type Limits =
    { ///CPU time limit in milliseconds.
      cpu_ms: int }
    ///Creates an instance of Limits with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (cpu_ms: int): Limits = { cpu_ms = cpu_ms }

///Placement setting used for Pages Functions.
type Placement =
    { ///Placement mode.
      mode: string }
    ///Creates an instance of Placement with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (mode: string): Placement = { mode = mode }

type pagesdeploymentconfigvalues =
    { ///Constellation bindings used for Pages Functions.
      ai_bindings: Option<Map<string, string>>
      ///Whether to always use the latest compatibility date for Pages Functions.
      always_use_latest_compatibility_date: bool
      ///Analytics Engine bindings used for Pages Functions.
      analytics_engine_datasets: Option<Map<string, string>>
      ///Browser bindings used for Pages Functions.
      browsers: Option<Map<string, Option<string>>>
      ///The major version of the build image to use for Pages Functions.
      build_image_major_version: int
      ///Compatibility date used for Pages Functions.
      compatibility_date: string
      ///Compatibility flags used for Pages Functions.
      compatibility_flags: list<string>
      ///D1 databases used for Pages Functions.
      d1_databases: Option<Map<string, string>>
      ///Durable Object namespaces used for Pages Functions.
      durable_object_namespaces: Option<Map<string, string>>
      ///Environment variables used for builds and Pages Functions.
      env_vars: Map<string, Option<string>>
      ///Whether to fail open when the deployment config cannot be applied.
      fail_open: bool
      ///Hyperdrive bindings used for Pages Functions.
      hyperdrive_bindings: Option<Map<string, string>>
      ///KV namespaces used for Pages Functions.
      kv_namespaces: Option<Map<string, string>>
      ///Limits for Pages Functions.
      limits: Option<Limits>
      ///mTLS bindings used for Pages Functions.
      mtls_certificates: Option<Map<string, string>>
      ///Placement setting used for Pages Functions.
      placement: Option<Placement>
      ///Queue Producer bindings used for Pages Functions.
      queue_producers: Option<Map<string, string>>
      ///R2 buckets used for Pages Functions.
      r2_buckets: Option<Map<string, string>>
      ///Services used for Pages Functions.
      services: Option<Map<string, string>>
      ///Vectorize bindings used for Pages Functions.
      vectorize_bindings: Option<Map<string, string>>
      ///Hash of the Wrangler configuration used for the deployment.
      wrangler_config_hash: Option<string> }
    ///Creates an instance of pagesdeploymentconfigvalues with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (always_use_latest_compatibility_date: bool,
                          build_image_major_version: int,
                          compatibility_date: string,
                          compatibility_flags: list<string>,
                          env_vars: Map<string, Option<string>>,
                          fail_open: bool): pagesdeploymentconfigvalues =
        { ai_bindings = None
          always_use_latest_compatibility_date = always_use_latest_compatibility_date
          analytics_engine_datasets = None
          browsers = None
          build_image_major_version = build_image_major_version
          compatibility_date = compatibility_date
          compatibility_flags = compatibility_flags
          d1_databases = None
          durable_object_namespaces = None
          env_vars = env_vars
          fail_open = fail_open
          hyperdrive_bindings = None
          kv_namespaces = None
          limits = None
          mtls_certificates = None
          placement = None
          queue_producers = None
          r2_buckets = None
          services = None
          vectorize_bindings = None
          wrangler_config_hash = None }

///Limits for Pages Functions.
type pagesdeploymentconfigvaluesrequestLimits =
    { ///CPU time limit in milliseconds.
      cpu_ms: int }
    ///Creates an instance of pagesdeploymentconfigvaluesrequestLimits with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (cpu_ms: int): pagesdeploymentconfigvaluesrequestLimits = { cpu_ms = cpu_ms }

///Placement setting used for Pages Functions.
type pagesdeploymentconfigvaluesrequestPlacement =
    { ///Placement mode.
      mode: string }
    ///Creates an instance of pagesdeploymentconfigvaluesrequestPlacement with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (mode: string): pagesdeploymentconfigvaluesrequestPlacement = { mode = mode }

type pagesdeploymentconfigvaluesrequest =
    { ///Constellation bindings used for Pages Functions.
      ai_bindings: Option<Map<string, Option<string>>>
      ///Whether to always use the latest compatibility date for Pages Functions.
      always_use_latest_compatibility_date: Option<bool>
      ///Analytics Engine bindings used for Pages Functions.
      analytics_engine_datasets: Option<Map<string, Option<string>>>
      ///Browser bindings used for Pages Functions.
      browsers: Option<Map<string, Option<string>>>
      ///The major version of the build image to use for Pages Functions.
      build_image_major_version: Option<int>
      ///Compatibility date used for Pages Functions.
      compatibility_date: Option<string>
      ///Compatibility flags used for Pages Functions.
      compatibility_flags: Option<list<string>>
      ///D1 databases used for Pages Functions.
      d1_databases: Option<Map<string, Option<string>>>
      ///Durable Object namespaces used for Pages Functions.
      durable_object_namespaces: Option<Map<string, Option<string>>>
      ///Environment variables used for builds and Pages Functions.
      env_vars: Option<Map<string, Option<string>>>
      ///Whether to fail open when the deployment config cannot be applied.
      fail_open: Option<bool>
      ///Hyperdrive bindings used for Pages Functions.
      hyperdrive_bindings: Option<Map<string, Option<string>>>
      ///KV namespaces used for Pages Functions.
      kv_namespaces: Option<Map<string, Option<string>>>
      ///Limits for Pages Functions.
      limits: Option<pagesdeploymentconfigvaluesrequestLimits>
      ///mTLS bindings used for Pages Functions.
      mtls_certificates: Option<Map<string, Option<string>>>
      ///Placement setting used for Pages Functions.
      placement: Option<pagesdeploymentconfigvaluesrequestPlacement>
      ///Queue Producer bindings used for Pages Functions.
      queue_producers: Option<Map<string, Option<string>>>
      ///R2 buckets used for Pages Functions.
      r2_buckets: Option<Map<string, Option<string>>>
      ///Services used for Pages Functions.
      services: Option<Map<string, Option<string>>>
      ///Vectorize bindings used for Pages Functions.
      vectorize_bindings: Option<Map<string, Option<string>>>
      ///Hash of the Wrangler configuration used for the deployment.
      wrangler_config_hash: Option<string> }
    ///Creates an instance of pagesdeploymentconfigvaluesrequest with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): pagesdeploymentconfigvaluesrequest =
        { ai_bindings = None
          always_use_latest_compatibility_date = None
          analytics_engine_datasets = None
          browsers = None
          build_image_major_version = None
          compatibility_date = None
          compatibility_flags = None
          d1_databases = None
          durable_object_namespaces = None
          env_vars = None
          fail_open = None
          hyperdrive_bindings = None
          kv_namespaces = None
          limits = None
          mtls_certificates = None
          placement = None
          queue_producers = None
          r2_buckets = None
          services = None
          vectorize_bindings = None
          wrangler_config_hash = None }

type Data =
    { line: string
      ts: string }
    ///Creates an instance of Data with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (line: string, ts: string): Data = { line = line; ts = ts }

type pagesdeploymentlog =
    { data: list<Data>
      includes_container_logs: bool
      total: int }
    ///Creates an instance of pagesdeploymentlog with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (data: list<Data>, includes_container_logs: bool, total: int): pagesdeploymentlog =
        { data = data
          includes_container_logs = includes_container_logs
          total = total }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type Certificateauthority =
    | [<CompiledName "google">] Google
    | [<CompiledName "lets_encrypt">] Lets_encrypt
    member this.Format() =
        match this with
        | Google -> "google"
        | Lets_encrypt -> "lets_encrypt"

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type Status =
    | [<CompiledName "initializing">] Initializing
    | [<CompiledName "pending">] Pending
    | [<CompiledName "active">] Active
    | [<CompiledName "deactivated">] Deactivated
    | [<CompiledName "blocked">] Blocked
    | [<CompiledName "error">] Error
    member this.Format() =
        match this with
        | Initializing -> "initializing"
        | Pending -> "pending"
        | Active -> "active"
        | Deactivated -> "deactivated"
        | Blocked -> "blocked"
        | Error -> "error"

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type Method =
    | [<CompiledName "http">] Http
    | [<CompiledName "txt">] Txt
    member this.Format() =
        match this with
        | Http -> "http"
        | Txt -> "txt"

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type ValidationdataStatus =
    | [<CompiledName "initializing">] Initializing
    | [<CompiledName "pending">] Pending
    | [<CompiledName "active">] Active
    | [<CompiledName "deactivated">] Deactivated
    | [<CompiledName "error">] Error
    member this.Format() =
        match this with
        | Initializing -> "initializing"
        | Pending -> "pending"
        | Active -> "active"
        | Deactivated -> "deactivated"
        | Error -> "error"

type Validationdata =
    { error_message: Option<string>
      method: Method
      status: ValidationdataStatus
      txt_name: Option<string>
      txt_value: Option<string> }
    ///Creates an instance of Validationdata with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (method: Method, status: ValidationdataStatus): Validationdata =
        { error_message = None
          method = method
          status = status
          txt_name = None
          txt_value = None }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type VerificationdataStatus =
    | [<CompiledName "pending">] Pending
    | [<CompiledName "active">] Active
    | [<CompiledName "deactivated">] Deactivated
    | [<CompiledName "blocked">] Blocked
    | [<CompiledName "error">] Error
    member this.Format() =
        match this with
        | Pending -> "pending"
        | Active -> "active"
        | Deactivated -> "deactivated"
        | Blocked -> "blocked"
        | Error -> "error"

type Verificationdata =
    { error_message: Option<string>
      status: VerificationdataStatus }
    ///Creates an instance of Verificationdata with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (status: VerificationdataStatus): Verificationdata =
        { error_message = None
          status = status }

type pagesdomain =
    { certificate_authority: Certificateauthority
      created_on: string
      domain_id: string
      id: string
      ///The domain name.
      name: pagesdomainname
      status: Status
      validation_data: Validationdata
      verification_data: Verificationdata
      zone_tag: string }
    ///Creates an instance of pagesdomain with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (certificate_authority: Certificateauthority,
                          created_on: string,
                          domain_id: string,
                          id: string,
                          name: pagesdomainname,
                          status: Status,
                          validation_data: Validationdata,
                          verification_data: Verificationdata,
                          zone_tag: string): pagesdomain =
        { certificate_authority = certificate_authority
          created_on = created_on
          domain_id = domain_id
          id = id
          name = name
          status = status
          validation_data = validation_data
          verification_data = verification_data
          zone_tag = zone_tag }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type pagesplaintextenvvarType =
    | [<CompiledName "plain_text">] Plain_text
    member this.Format() =
        match this with
        | Plain_text -> "plain_text"

///A plaintext environment variable.
type pagesplaintextenvvar =
    { ``type``: pagesplaintextenvvarType
      ///Environment variable value.
      value: string }
    ///Creates an instance of pagesplaintextenvvar with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (``type``: pagesplaintextenvvarType, value: string): pagesplaintextenvvar =
        { ``type`` = ``type``; value = value }

///Configs for deployments in a project.
type Deploymentconfigs =
    { ///Configs for preview deploys.
      preview: Newtonsoft.Json.Linq.JToken
      ///Configs for production deploys.
      production: Newtonsoft.Json.Linq.JToken }
    ///Creates an instance of Deploymentconfigs with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (preview: Newtonsoft.Json.Linq.JToken, production: Newtonsoft.Json.Linq.JToken): Deploymentconfigs =
        { preview = preview
          production = production }

type pagesproject =
    { ///Configs for the project build process.
      build_config: Option<pagesbuildconfig>
      canonical_deployment: Newtonsoft.Json.Linq.JToken
      ///When the project was created.
      created_on: System.DateTimeOffset
      ///Configs for deployments in a project.
      deployment_configs: Deploymentconfigs
      ///A list of associated custom domains for the project.
      domains: Option<list<string>>
      ///Framework the project is using.
      framework: string
      ///Version of the framework the project is using.
      framework_version: string
      ///ID of the project.
      id: string
      latest_deployment: Newtonsoft.Json.Linq.JToken
      ///Name of the project.
      name: pagesprojectname
      ///Name of the preview script.
      preview_script_name: string
      ///Production branch of the project. Used to identify production deployments.
      production_branch: string
      ///Name of the production script.
      production_script_name: string
      ///Configs for the project source control.
      source: Option<pagessource>
      ///The Cloudflare subdomain associated with the project.
      subdomain: Option<string>
      ///Whether the project uses functions.
      uses_functions: bool }
    ///Creates an instance of pagesproject with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (canonical_deployment: Newtonsoft.Json.Linq.JToken,
                          created_on: System.DateTimeOffset,
                          deployment_configs: Deploymentconfigs,
                          framework: string,
                          framework_version: string,
                          id: string,
                          latest_deployment: Newtonsoft.Json.Linq.JToken,
                          name: pagesprojectname,
                          preview_script_name: string,
                          production_branch: string,
                          production_script_name: string,
                          uses_functions: bool): pagesproject =
        { build_config = None
          canonical_deployment = canonical_deployment
          created_on = created_on
          deployment_configs = deployment_configs
          domains = None
          framework = framework
          framework_version = framework_version
          id = id
          latest_deployment = latest_deployment
          name = name
          preview_script_name = preview_script_name
          production_branch = production_branch
          production_script_name = production_script_name
          source = None
          subdomain = None
          uses_functions = uses_functions }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type pagessecrettextenvvarType =
    | [<CompiledName "secret_text">] Secret_text
    member this.Format() =
        match this with
        | Secret_text -> "secret_text"

///An encrypted environment variable.
type pagessecrettextenvvar =
    { ``type``: pagessecrettextenvvarType
      ///Secret value.
      value: string }
    ///Creates an instance of pagessecrettextenvvar with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (``type``: pagessecrettextenvvarType, value: string): pagessecrettextenvvar =
        { ``type`` = ``type``; value = value }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type Previewdeploymentsetting =
    | [<CompiledName "all">] All
    | [<CompiledName "none">] None
    | [<CompiledName "custom">] Custom
    member this.Format() =
        match this with
        | All -> "all"
        | None -> "none"
        | Custom -> "custom"

type Config =
    { ///The owner of the repository.
      owner: string
      ///The owner ID of the repository.
      owner_id: Option<string>
      ///A list of paths that should be excluded from triggering a preview deployment. Wildcard syntax (`*`) is supported.
      path_excludes: Option<list<string>>
      ///A list of paths that should be watched to trigger a preview deployment. Wildcard syntax (`*`) is supported.
      path_includes: Option<list<string>>
      ///Whether to enable PR comments.
      pr_comments_enabled: bool
      ///A list of branches that should not trigger a preview deployment. Wildcard syntax (`*`) is supported. Must be used with `preview_deployment_setting` set to `custom`.
      preview_branch_excludes: Option<list<string>>
      ///A list of branches that should trigger a preview deployment. Wildcard syntax (`*`) is supported. Must be used with `preview_deployment_setting` set to `custom`.
      preview_branch_includes: Option<list<string>>
      ///Controls whether commits to preview branches trigger a preview deployment.
      preview_deployment_setting: Option<Previewdeploymentsetting>
      ///The production branch of the repository.
      production_branch: string
      ///Whether to trigger a production deployment on commits to the production branch.
      production_deployments_enabled: Option<bool>
      ///The ID of the repository.
      repo_id: Option<string>
      ///The name of the repository.
      repo_name: string }
    ///Creates an instance of Config with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (owner: string, pr_comments_enabled: bool, production_branch: string, repo_name: string): Config =
        { owner = owner
          owner_id = None
          path_excludes = None
          path_includes = None
          pr_comments_enabled = pr_comments_enabled
          preview_branch_excludes = None
          preview_branch_includes = None
          preview_deployment_setting = None
          production_branch = production_branch
          production_deployments_enabled = None
          repo_id = None
          repo_name = repo_name }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type pagessourceType =
    | [<CompiledName "github">] Github
    | [<CompiledName "gitlab">] Gitlab
    member this.Format() =
        match this with
        | Github -> "github"
        | Gitlab -> "gitlab"

///Configs for the project source control.
type pagessource =
    { config: Config
      ///The source control management provider.
      ``type``: pagessourceType }
    ///Creates an instance of pagessource with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (config: Config, ``type``: pagessourceType): pagessource =
        { config = config; ``type`` = ``type`` }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type Name =
    | [<CompiledName "queued">] Queued
    | [<CompiledName "initialize">] Initialize
    | [<CompiledName "clone_repo">] Clone_repo
    | [<CompiledName "build">] Build
    | [<CompiledName "deploy">] Deploy
    member this.Format() =
        match this with
        | Queued -> "queued"
        | Initialize -> "initialize"
        | Clone_repo -> "clone_repo"
        | Build -> "build"
        | Deploy -> "deploy"

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type pagesstageStatus =
    | [<CompiledName "success">] Success
    | [<CompiledName "idle">] Idle
    | [<CompiledName "active">] Active
    | [<CompiledName "failure">] Failure
    | [<CompiledName "canceled">] Canceled
    member this.Format() =
        match this with
        | Success -> "success"
        | Idle -> "idle"
        | Active -> "active"
        | Failure -> "failure"
        | Canceled -> "canceled"

///The status of the deployment.
type pagesstage =
    { ///When the stage ended.
      ended_on: System.DateTimeOffset
      ///The current build stage.
      name: Name
      ///When the stage started.
      started_on: System.DateTimeOffset
      ///State of the current stage.
      status: pagesstageStatus }
    ///Creates an instance of pagesstage with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (ended_on: System.DateTimeOffset,
                          name: Name,
                          started_on: System.DateTimeOffset,
                          status: pagesstageStatus): pagesstage =
        { ended_on = ended_on
          name = name
          started_on = started_on
          status = status }

[<RequireQualifiedAccess>]
type PagesProjectGetProjects =
    ///Get projects response.
    | OK of payload: ``pagesapi-response-common``

///Configs for the project build process.
type Buildconfig =
    { ///Enable build caching for the project.
      build_caching: Option<bool>
      ///Command used to build project.
      build_command: Option<string>
      ///Output directory of the build.
      destination_dir: Option<string>
      ///Directory to run the command.
      root_dir: Option<string>
      ///The classifying tag for analytics.
      web_analytics_tag: Option<string>
      ///The auth token for analytics.
      web_analytics_token: Option<string> }
    ///Creates an instance of Buildconfig with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): Buildconfig =
        { build_caching = None
          build_command = None
          destination_dir = None
          root_dir = None
          web_analytics_tag = None
          web_analytics_token = None }

///Configs for deployments in a project.
type PagesProjectCreateProjectPayloadDeploymentconfigs =
    { ///Configs for preview deploys.
      preview: Option<Newtonsoft.Json.Linq.JToken>
      ///Configs for production deploys.
      production: Option<Newtonsoft.Json.Linq.JToken> }
    ///Creates an instance of PagesProjectCreateProjectPayloadDeploymentconfigs with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): PagesProjectCreateProjectPayloadDeploymentconfigs = { preview = None; production = None }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type PagesProjectCreateProjectPayloadSourceConfigPreviewdeploymentsetting =
    | [<CompiledName "all">] All
    | [<CompiledName "none">] None
    | [<CompiledName "custom">] Custom
    member this.Format() =
        match this with
        | All -> "all"
        | None -> "none"
        | Custom -> "custom"

type PagesProjectCreateProjectPayloadSourceConfig =
    { ///The owner of the repository.
      owner: Option<string>
      ///The owner ID of the repository.
      owner_id: Option<string>
      ///A list of paths that should be excluded from triggering a preview deployment. Wildcard syntax (`*`) is supported.
      path_excludes: Option<list<string>>
      ///A list of paths that should be watched to trigger a preview deployment. Wildcard syntax (`*`) is supported.
      path_includes: Option<list<string>>
      ///Whether to enable PR comments.
      pr_comments_enabled: Option<bool>
      ///A list of branches that should not trigger a preview deployment. Wildcard syntax (`*`) is supported. Must be used with `preview_deployment_setting` set to `custom`.
      preview_branch_excludes: Option<list<string>>
      ///A list of branches that should trigger a preview deployment. Wildcard syntax (`*`) is supported. Must be used with `preview_deployment_setting` set to `custom`.
      preview_branch_includes: Option<list<string>>
      ///Controls whether commits to preview branches trigger a preview deployment.
      preview_deployment_setting: Option<PagesProjectCreateProjectPayloadSourceConfigPreviewdeploymentsetting>
      ///The production branch of the repository.
      production_branch: Option<string>
      ///Whether to trigger a production deployment on commits to the production branch.
      production_deployments_enabled: Option<bool>
      ///The ID of the repository.
      repo_id: Option<string>
      ///The name of the repository.
      repo_name: Option<string> }
    ///Creates an instance of PagesProjectCreateProjectPayloadSourceConfig with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): PagesProjectCreateProjectPayloadSourceConfig =
        { owner = None
          owner_id = None
          path_excludes = None
          path_includes = None
          pr_comments_enabled = None
          preview_branch_excludes = None
          preview_branch_includes = None
          preview_deployment_setting = None
          production_branch = None
          production_deployments_enabled = None
          repo_id = None
          repo_name = None }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type PagesProjectCreateProjectPayloadSourceType =
    | [<CompiledName "github">] Github
    | [<CompiledName "gitlab">] Gitlab
    member this.Format() =
        match this with
        | Github -> "github"
        | Gitlab -> "gitlab"

///Configs for the project source control.
type PagesProjectCreateProjectPayloadSource =
    { config: PagesProjectCreateProjectPayloadSourceConfig
      ///The source control management provider.
      ``type``: PagesProjectCreateProjectPayloadSourceType }
    ///Creates an instance of PagesProjectCreateProjectPayloadSource with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (config: PagesProjectCreateProjectPayloadSourceConfig,
                          ``type``: PagesProjectCreateProjectPayloadSourceType): PagesProjectCreateProjectPayloadSource =
        { config = config; ``type`` = ``type`` }

type PagesProjectCreateProjectPayload =
    { ///Configs for the project build process.
      build_config: Option<Buildconfig>
      ///Configs for deployments in a project.
      deployment_configs: Option<PagesProjectCreateProjectPayloadDeploymentconfigs>
      ///Name of the project.
      name: string
      ///Production branch of the project. Used to identify production deployments.
      production_branch: string
      ///Configs for the project source control.
      source: Option<PagesProjectCreateProjectPayloadSource> }
    ///Creates an instance of PagesProjectCreateProjectPayload with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (name: string, production_branch: string): PagesProjectCreateProjectPayload =
        { build_config = None
          deployment_configs = None
          name = name
          production_branch = production_branch
          source = None }

[<RequireQualifiedAccess>]
type PagesProjectCreateProject =
    ///Create project response.
    | OK of payload: ``pagesapi-response-common``

[<RequireQualifiedAccess>]
type PagesProjectDeleteProject =
    ///Delete project response.
    | OK of payload: ``pagesapi-response-common``

[<RequireQualifiedAccess>]
type PagesProjectGetProject =
    ///Get project response.
    | OK of payload: ``pagesapi-response-common``

///Configs for the project build process.
type PagesProjectUpdateProjectPayloadBuildconfig =
    { ///Enable build caching for the project.
      build_caching: Option<bool>
      ///Command used to build project.
      build_command: Option<string>
      ///Output directory of the build.
      destination_dir: Option<string>
      ///Directory to run the command.
      root_dir: Option<string>
      ///The classifying tag for analytics.
      web_analytics_tag: Option<string>
      ///The auth token for analytics.
      web_analytics_token: Option<string> }
    ///Creates an instance of PagesProjectUpdateProjectPayloadBuildconfig with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): PagesProjectUpdateProjectPayloadBuildconfig =
        { build_caching = None
          build_command = None
          destination_dir = None
          root_dir = None
          web_analytics_tag = None
          web_analytics_token = None }

///Configs for deployments in a project.
type PagesProjectUpdateProjectPayloadDeploymentconfigs =
    { ///Configs for preview deploys.
      preview: Option<Newtonsoft.Json.Linq.JToken>
      ///Configs for production deploys.
      production: Option<Newtonsoft.Json.Linq.JToken> }
    ///Creates an instance of PagesProjectUpdateProjectPayloadDeploymentconfigs with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): PagesProjectUpdateProjectPayloadDeploymentconfigs = { preview = None; production = None }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type PagesProjectUpdateProjectPayloadSourceConfigPreviewdeploymentsetting =
    | [<CompiledName "all">] All
    | [<CompiledName "none">] None
    | [<CompiledName "custom">] Custom
    member this.Format() =
        match this with
        | All -> "all"
        | None -> "none"
        | Custom -> "custom"

type PagesProjectUpdateProjectPayloadSourceConfig =
    { ///The owner of the repository.
      owner: Option<string>
      ///The owner ID of the repository.
      owner_id: Option<string>
      ///A list of paths that should be excluded from triggering a preview deployment. Wildcard syntax (`*`) is supported.
      path_excludes: Option<list<string>>
      ///A list of paths that should be watched to trigger a preview deployment. Wildcard syntax (`*`) is supported.
      path_includes: Option<list<string>>
      ///Whether to enable PR comments.
      pr_comments_enabled: Option<bool>
      ///A list of branches that should not trigger a preview deployment. Wildcard syntax (`*`) is supported. Must be used with `preview_deployment_setting` set to `custom`.
      preview_branch_excludes: Option<list<string>>
      ///A list of branches that should trigger a preview deployment. Wildcard syntax (`*`) is supported. Must be used with `preview_deployment_setting` set to `custom`.
      preview_branch_includes: Option<list<string>>
      ///Controls whether commits to preview branches trigger a preview deployment.
      preview_deployment_setting: Option<PagesProjectUpdateProjectPayloadSourceConfigPreviewdeploymentsetting>
      ///The production branch of the repository.
      production_branch: Option<string>
      ///Whether to trigger a production deployment on commits to the production branch.
      production_deployments_enabled: Option<bool>
      ///The ID of the repository.
      repo_id: Option<string>
      ///The name of the repository.
      repo_name: Option<string> }
    ///Creates an instance of PagesProjectUpdateProjectPayloadSourceConfig with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): PagesProjectUpdateProjectPayloadSourceConfig =
        { owner = None
          owner_id = None
          path_excludes = None
          path_includes = None
          pr_comments_enabled = None
          preview_branch_excludes = None
          preview_branch_includes = None
          preview_deployment_setting = None
          production_branch = None
          production_deployments_enabled = None
          repo_id = None
          repo_name = None }

[<Fable.Core.StringEnum; RequireQualifiedAccess>]
type PagesProjectUpdateProjectPayloadSourceType =
    | [<CompiledName "github">] Github
    | [<CompiledName "gitlab">] Gitlab
    member this.Format() =
        match this with
        | Github -> "github"
        | Gitlab -> "gitlab"

///Configs for the project source control.
type PagesProjectUpdateProjectPayloadSource =
    { config: PagesProjectUpdateProjectPayloadSourceConfig
      ///The source control management provider.
      ``type``: PagesProjectUpdateProjectPayloadSourceType }
    ///Creates an instance of PagesProjectUpdateProjectPayloadSource with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (config: PagesProjectUpdateProjectPayloadSourceConfig,
                          ``type``: PagesProjectUpdateProjectPayloadSourceType): PagesProjectUpdateProjectPayloadSource =
        { config = config; ``type`` = ``type`` }

type PagesProjectUpdateProjectPayload =
    { ///Configs for the project build process.
      build_config: Option<PagesProjectUpdateProjectPayloadBuildconfig>
      ///Configs for deployments in a project.
      deployment_configs: Option<PagesProjectUpdateProjectPayloadDeploymentconfigs>
      ///Name of the project.
      name: Option<string>
      ///Production branch of the project. Used to identify production deployments.
      production_branch: Option<string>
      ///Configs for the project source control.
      source: Option<PagesProjectUpdateProjectPayloadSource> }
    ///Creates an instance of PagesProjectUpdateProjectPayload with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (): PagesProjectUpdateProjectPayload =
        { build_config = None
          deployment_configs = None
          name = None
          production_branch = None
          source = None }

[<RequireQualifiedAccess>]
type PagesProjectUpdateProject =
    ///Update project response.
    | OK of payload: ``pagesapi-response-common``

[<RequireQualifiedAccess>]
type PagesDeploymentGetDeployments =
    ///Get deployments response.
    | OK of payload: ``pagesapi-response-common``

[<RequireQualifiedAccess>]
type PagesDeploymentCreateDeployment =
    ///Create deployment response.
    | OK of payload: ``pagesapi-response-common``

[<RequireQualifiedAccess>]
type PagesDeploymentDeleteDeployment =
    ///Delete deployment response.
    | OK of payload: ``pagesapi-response-common``

[<RequireQualifiedAccess>]
type PagesDeploymentGetDeploymentInfo =
    ///Get deployment info response.
    | OK of payload: ``pagesapi-response-common``

[<RequireQualifiedAccess>]
type PagesDeploymentGetDeploymentLogs =
    ///Get deployment logs response.
    | OK of payload: ``pagesapi-response-common``

[<RequireQualifiedAccess>]
type PagesDeploymentRetryDeployment =
    ///Retry deployment response.
    | OK of payload: ``pagesapi-response-common``

[<RequireQualifiedAccess>]
type PagesDeploymentRollbackDeployment =
    ///Rollback deployment response.
    | OK of payload: ``pagesapi-response-common``

[<RequireQualifiedAccess>]
type PagesDomainsGetDomains =
    ///Get domains response.
    | OK of payload: ``pagesapi-response-common``

type PagesDomainsAddDomainPayload =
    { ///The domain name.
      name: pagesdomainname }
    ///Creates an instance of PagesDomainsAddDomainPayload with all optional fields initialized to None. The required fields are parameters of this function
    static member Create (name: pagesdomainname): PagesDomainsAddDomainPayload = { name = name }

[<RequireQualifiedAccess>]
type PagesDomainsAddDomain =
    ///Add domain response.
    | OK of payload: ``pagesapi-response-common``

[<RequireQualifiedAccess>]
type PagesDomainsDeleteDomain =
    ///Delete domain response.
    | OK of payload: ``pagesapi-response-common``

[<RequireQualifiedAccess>]
type PagesDomainsGetDomain =
    ///Get domain response.
    | OK of payload: ``pagesapi-response-common``

[<RequireQualifiedAccess>]
type PagesDomainsPatchDomain =
    ///Patch domain response.
    | OK of payload: ``pagesapi-response-common``

[<RequireQualifiedAccess>]
type PagesPurgeBuildCache =
    ///Purge build cache response.
    | OK of payload: ``pagesapi-response-common``
