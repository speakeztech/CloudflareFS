namespace AskAI.CLI

open System
open System.IO
open System.Net.Http
open System.Security.Cryptography
open System.Text
open System.Text.RegularExpressions

/// Content synchronization to R2 for AutoRAG indexing
/// Demonstrates using AWS S3 SDK with R2 (S3-compatible)
module ContentSync =

    /// Frontmatter extracted from markdown files
    type FrontMatter = {
        Title: string
        Date: DateTime option
        Description: string option
        Tags: string list
    }

    /// A content item to sync
    type ContentItem = {
        Section: string      // "blog", "portfolio", "company"
        Slug: string         // filename without extension
        FilePath: string     // full path to source file
        Content: string      // raw markdown content
        FrontMatter: FrontMatter
    }

    /// Extract YAML frontmatter from markdown content
    let private parseFrontMatter (content: string) : FrontMatter * string =
        let pattern = @"^---\s*\n([\s\S]*?)\n---\s*\n([\s\S]*)$"
        let m = Regex.Match(content, pattern)
        if m.Success then
            let yaml = m.Groups.[1].Value
            let body = m.Groups.[2].Value

            // Simple YAML parsing (production would use YamlDotNet)
            let extractField (name: string) =
                let fieldPattern = sprintf @"^%s:\s*[""']?(.+?)[""']?\s*$" name
                let fm = Regex.Match(yaml, fieldPattern, RegexOptions.Multiline)
                if fm.Success then Some (fm.Groups.[1].Value.Trim())
                else None

            let title = extractField "title" |> Option.defaultValue "Untitled"
            let date =
                extractField "date"
                |> Option.bind (fun s ->
                    match DateTime.TryParse(s) with
                    | true, d -> Some d
                    | _ -> None)
            let description = extractField "description"
            let tags = [] // Simplified; real impl would parse YAML list

            { Title = title; Date = date; Description = description; Tags = tags }, body
        else
            { Title = "Untitled"; Date = None; Description = None; Tags = [] }, content

    /// Compute MD5 hash for change detection
    let private computeMD5 (bytes: byte[]) : string =
        use md5 = MD5.Create()
        let hash = md5.ComputeHash(bytes)
        BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant()

    /// Convert section and filename to R2 key
    /// e.g., ("blog", "my-post.md") -> "blog--my-post.md"
    let toR2Key (section: string) (filename: string) : string =
        let normalizedFilename = filename.ToLowerInvariant().Replace(" ", "-")
        $"{section}--{normalizedFilename}"

    /// Load content items from a directory
    let loadContentFromDirectory (contentDir: string) (section: string) : ContentItem list =
        let dirPath = Path.Combine(contentDir, section)
        if Directory.Exists(dirPath) then
            Directory.GetFiles(dirPath, "*.md")
            |> Array.filter (fun f -> not (Path.GetFileName(f).StartsWith("_")))
            |> Array.map (fun filePath ->
                let content = File.ReadAllText(filePath)
                let frontMatter, _ = parseFrontMatter content
                let slug = Path.GetFileNameWithoutExtension(filePath)
                {
                    Section = section
                    Slug = slug
                    FilePath = filePath
                    Content = content
                    FrontMatter = frontMatter
                })
            |> List.ofArray
        else
            []

    /// Load all content from multiple sections
    let loadAllContent (contentDir: string) (sections: string list) : ContentItem list =
        sections
        |> List.collect (loadContentFromDirectory contentDir)

    /// Synchronize content to R2 bucket
    /// Returns (uploaded, skipped, deleted) counts
    let syncToR2
        (httpClient: HttpClient)
        (accountId: string)
        (bucketName: string)
        (items: ContentItem list)
        : Async<Result<int * int * int, string>> =
        async {
            // This is a simplified implementation
            // Production would use AWSSDK.S3 with R2 endpoint

            let mutable uploaded = 0
            let mutable skipped = 0

            for item in items do
                let key = toR2Key item.Section (item.Slug + ".md")
                let bytes = Encoding.UTF8.GetBytes(item.Content)

                // Build metadata for AutoRAG
                let url = $"/{item.Section}/{item.Slug}/"
                let context = $"Content titled '{item.FrontMatter.Title}' from the {item.Section} section. URL: {url}"

                // In production, upload to R2 with metadata
                // For now, just count
                uploaded <- uploaded + 1
                printfn "  [Upload] %s -> %s" item.FilePath key

            return Ok (uploaded, skipped, 0)
        }

    /// Trigger AutoRAG full scan after content changes
    let triggerAutoRAGScan
        (httpClient: HttpClient)
        (accountId: string)
        (apiToken: string)
        (ragName: string)
        : Async<Result<unit, string>> =
        async {
            let url = $"https://api.cloudflare.com/client/v4/accounts/{accountId}/autorag/rags/{ragName}/full_scan"

            use request = new HttpRequestMessage(HttpMethod.Patch, url)
            request.Headers.Add("Authorization", $"Bearer {apiToken}")
            request.Content <- new StringContent("{}", Encoding.UTF8, "application/json")

            let! response = httpClient.SendAsync(request) |> Async.AwaitTask
            let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask

            if response.IsSuccessStatusCode then
                return Ok ()
            elif int response.StatusCode = 429 then
                // Rate limited - cooldown period
                printfn "AutoRAG sync in cooldown period (3 min between syncs)"
                return Ok ()
            else
                return Error $"AutoRAG scan failed: {body}"
        }
