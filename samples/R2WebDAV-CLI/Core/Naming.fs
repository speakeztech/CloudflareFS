module R2WebDAV.CLI.Naming

/// Deterministic naming conventions for R2WebDAV resources
/// Matches the naming scheme used in the original PowerShell scripts

let getBucketName (username: string) =
    $"{username.ToLower()}-webdav-bucket"

let getBindingName (username: string) =
    $"{username.ToLower()}_webdav_sync"

let getSecretName (username: string) =
    $"USER_{username.ToUpper()}_PASSWORD"

let isValidUsername (username: string) =
    if String.length username = 0 || String.length username > 32 then
        false
    else
        username |> Seq.forall (fun c ->
            System.Char.IsLetterOrDigit c || c = '-' || c = '_'
        )

let validateUsername (username: string) : Result<string, string> =
    if not (isValidUsername username) then
        Error "Username must be 1-32 characters and contain only letters, numbers, hyphens, or underscores"
    else
        Ok (username.ToLower())
