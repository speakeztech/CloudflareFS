# extract-service.ps1
# Extracts service-specific endpoints from Cloudflare's massive OpenAPI spec
# Creates focused, manageable OpenAPI files for Hawaii processing

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("KV", "R2", "D1", "Workers", "DNS", "Zones", "Analytics", "Logs", "Access")]
    [string]$Service,

    [Parameter(Mandatory=$false)]
    [string]$InputFile = "D:\repos\Cloudflare\api-schemas\openapi.json",

    [Parameter(Mandatory=$false)]
    [string]$OutputDir = ".\temp",

    [switch]$ShowDetails
)

$ErrorActionPreference = "Stop"

# Service endpoint patterns - these define what paths belong to each service
$servicePatterns = @{
    "KV" = @{
        paths = @(
            "*/storage/kv/namespaces*"
        )
        operationPrefix = "kv"
        description = "KV Storage Management API"
    }
    "R2" = @{
        paths = @(
            "*/r2/buckets*"
        )
        operationPrefix = "r2"
        description = "R2 Object Storage Management API"
    }
    "D1" = @{
        paths = @(
            "*/d1/database*"
        )
        operationPrefix = "d1"
        description = "D1 Database Management API"
    }
    "Workers" = @{
        paths = @(
            "*/workers/scripts*",
            "*/workers/services*",
            "*/workers/routes*"
        )
        operationPrefix = "workers"
        description = "Workers Script Management API"
    }
    "DNS" = @{
        paths = @(
            "*/dns_records*"
        )
        operationPrefix = "dns"
        description = "DNS Records Management API"
    }
    "Zones" = @{
        paths = @(
            "/zones",
            "/zones/{zone_id}"
        )
        operationPrefix = "zones"
        description = "Zone Management API"
    }
    "Analytics" = @{
        paths = @(
            "*/analytics*"
        )
        operationPrefix = "analytics"
        description = "Analytics API"
    }
    "Logs" = @{
        paths = @(
            "*/logs*",
            "*/logpull*",
            "*/logpush*"
        )
        operationPrefix = "logs"
        description = "Logs API"
    }
    "Access" = @{
        paths = @(
            "*/access/apps*",
            "*/access/groups*",
            "*/access/policies*",
            "*/access/service_tokens*"
        )
        operationPrefix = "access"
        description = "Zero Trust Access API"
    }
}

Write-Host "Extracting $Service API from Cloudflare OpenAPI spec..." -ForegroundColor Cyan

# Ensure output directory exists
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

# Check input file size
$fileInfo = Get-Item $InputFile
$fileSizeMB = [math]::Round($fileInfo.Length / 1MB, 2)
Write-Host "Source OpenAPI file size: $fileSizeMB MB" -ForegroundColor Gray

# Load the OpenAPI spec
Write-Host "Loading OpenAPI specification..." -ForegroundColor Yellow
$json = Get-Content $InputFile -Raw

# Parse JSON
Write-Host "Parsing JSON..." -ForegroundColor Yellow
$spec = $json | ConvertFrom-Json

if (-not $spec) {
    throw "Failed to parse OpenAPI specification"
}

# Get service configuration
$serviceConfig = $servicePatterns[$Service]
if (-not $serviceConfig) {
    throw "Unknown service: $Service"
}

# Create filtered specification structure
Write-Host "Creating filtered OpenAPI for $Service..." -ForegroundColor Yellow

$filtered = [PSCustomObject]@{
    openapi = $spec.openapi
    info = @{
        title = "Cloudflare $Service API"
        description = $serviceConfig.description
        version = $spec.info.version
    }
    servers = $spec.servers
    paths = @{}
    components = @{
        schemas = @{}
        parameters = @{}
        responses = @{}
        examples = @{}
        requestBodies = @{}
        headers = @{}
        securitySchemes = $spec.components.securitySchemes
    }
}

# Extract matching paths
$pathCount = 0
$referencedSchemas = New-Object System.Collections.Generic.HashSet[string]

foreach ($pathProp in $spec.paths.PSObject.Properties) {
    $path = $pathProp.Name
    $pathData = $pathProp.Value

    # Check if this path matches any of our patterns
    $matches = $false
    foreach ($pattern in $serviceConfig.paths) {
        if ($path -like $pattern) {
            $matches = $true
            break
        }
    }

    if ($matches) {
        $filtered.paths | Add-Member -NotePropertyName $path -NotePropertyValue $pathData
        $pathCount++

        if ($ShowDetails) {
            Write-Host "  Added path: $path" -ForegroundColor DarkGray
        }

        # Collect schema references from this path
        $pathJson = $pathData | ConvertTo-Json -Depth 100
        $schemaRefs = [regex]::Matches($pathJson, '"?\$ref"?\s*:\s*"#/components/schemas/([^"]+)"')
        foreach ($match in $schemaRefs) {
            $schemaName = $match.Groups[1].Value
            [void]$referencedSchemas.Add($schemaName)
        }
    }
}

Write-Host "Extracted $pathCount paths" -ForegroundColor Green

# Function to recursively collect schema dependencies
function Get-SchemaDependencies {
    param([string]$schemaName, [hashtable]$allSchemas, [hashtable]$visited)

    if ($visited.ContainsKey($schemaName)) {
        return
    }
    $visited[$schemaName] = $true

    if (-not $allSchemas.ContainsKey($schemaName)) {
        return
    }

    $schema = $allSchemas[$schemaName]
    $schemaJson = $schema | ConvertTo-Json -Depth 100

    # Find all $ref references
    $refs = [regex]::Matches($schemaJson, '"?\$ref"?\s*:\s*"#/components/schemas/([^"]+)"')
    foreach ($match in $refs) {
        $refName = $match.Groups[1].Value
        Get-SchemaDependencies -schemaName $refName -allSchemas $allSchemas -visited $visited
    }
}

# Extract referenced schemas and their dependencies
Write-Host "Extracting referenced schemas..." -ForegroundColor Yellow

$allSchemas = @{}
foreach ($schemaProp in $spec.components.schemas.PSObject.Properties) {
    $allSchemas[$schemaProp.Name] = $schemaProp.Value
}

$schemaVisited = @{}
foreach ($schemaName in $referencedSchemas) {
    Get-SchemaDependencies -schemaName $schemaName -allSchemas $allSchemas -visited $schemaVisited
}

# Also check for schemas matching service prefix
foreach ($schemaProp in $spec.components.schemas.PSObject.Properties) {
    if ($schemaProp.Name -like "$($serviceConfig.operationPrefix)_*" -or
        $schemaProp.Name -like "$($serviceConfig.operationPrefix)-*") {
        $schemaVisited[$schemaProp.Name] = $true
    }
}

# Add common schemas that are always needed
$commonSchemas = @(
    "api-response-common",
    "api-response-single",
    "api-response-collection",
    "messages",
    "result_info",
    "schemas-errors"
)

foreach ($commonSchema in $commonSchemas) {
    if ($allSchemas.ContainsKey($commonSchema)) {
        $schemaVisited[$commonSchema] = $true
    }
}

# Add collected schemas to filtered spec
$schemaCount = 0
foreach ($schemaName in $schemaVisited.Keys) {
    if ($allSchemas.ContainsKey($schemaName)) {
        $filtered.components.schemas | Add-Member -NotePropertyName $schemaName -NotePropertyValue $allSchemas[$schemaName]
        $schemaCount++
    }
}

Write-Host "Extracted $schemaCount schemas" -ForegroundColor Green

# Extract referenced parameters
if ($spec.components.parameters) {
    foreach ($paramProp in $spec.components.parameters.PSObject.Properties) {
        # Check if this parameter is referenced
        $paramRef = "#/components/parameters/$($paramProp.Name)"
        $specJson = $filtered | ConvertTo-Json -Depth 100
        if ($specJson -match [regex]::Escape($paramRef)) {
            $filtered.components.parameters | Add-Member -NotePropertyName $paramProp.Name -NotePropertyValue $paramProp.Value
        }
    }
}

# Save filtered spec
$outputFile = Join-Path $OutputDir "$Service-openapi.json"
Write-Host "Saving filtered OpenAPI to $outputFile..." -ForegroundColor Yellow

$filtered | ConvertTo-Json -Depth 100 | Out-File $outputFile -Encoding UTF8

# Check output file size
$outputInfo = Get-Item $outputFile
$outputSizeMB = [math]::Round($outputInfo.Length / 1MB, 2)
$outputSizeKB = [math]::Round($outputInfo.Length / 1KB, 2)

if ($outputSizeMB -ge 1) {
    Write-Host "Output file size: $outputSizeMB MB" -ForegroundColor Green
} else {
    Write-Host "Output file size: $outputSizeKB KB" -ForegroundColor Green
}

# Size reduction
$reduction = [math]::Round((1 - $outputInfo.Length / $fileInfo.Length) * 100, 2)
Write-Host "Size reduction: $reduction%" -ForegroundColor Cyan

Write-Host "`n✅ Successfully extracted $Service API" -ForegroundColor Green
Write-Host "   Paths: $pathCount" -ForegroundColor Gray
Write-Host "   Schemas: $schemaCount" -ForegroundColor Gray
Write-Host "   Output: $outputFile" -ForegroundColor Gray