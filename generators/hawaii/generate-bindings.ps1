# generate-bindings.ps1 - Generate F# client from Cloudflare OpenAPI spec

param(
    [Parameter(Mandatory=$false)]
    [string]$SpecUrl = "https://api.cloudflare.com/schemas/openapi.json",

    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "..\..\src\Management\CloudFlare.Api\Generated.fs"
)

Write-Host "Hawaii Binding Generator for Cloudflare Management APIs" -ForegroundColor Cyan
Write-Host "=======================================================" -ForegroundColor Cyan
Write-Host ""

# Check if Hawaii is installed
$hawaiiCheck = dotnet tool list -g | Select-String "hawaii"
if (-not $hawaiiCheck) {
    Write-Host "Installing Hawaii tool..." -ForegroundColor Yellow
    dotnet tool install -g hawaii
}

# Download latest OpenAPI spec
Write-Host "Downloading Cloudflare OpenAPI specification..." -ForegroundColor Yellow
$specFile = "cloudflare-openapi.json"

try {
    Invoke-WebRequest -Uri $SpecUrl -OutFile $specFile
    Write-Host "✓ OpenAPI spec downloaded" -ForegroundColor Green
}
catch {
    Write-Error "Failed to download OpenAPI spec: $_"
    exit 1
}

# Ensure output directory exists
$outputDir = Split-Path -Parent $OutputPath
if (-not (Test-Path $outputDir)) {
    Write-Host "Creating output directory: $outputDir" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

# Generate F# client
Write-Host "Generating F# client bindings..." -ForegroundColor Yellow

try {
    hawaii $specFile `
        --output $OutputPath `
        --namespace CloudFlare.Api `
        --synchronous false `
        --target fsharp

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ F# client bindings generated successfully" -ForegroundColor Green
        Write-Host "  Output: $OutputPath" -ForegroundColor Gray
    } else {
        Write-Error "Hawaii generation failed"
        exit 1
    }
}
catch {
    Write-Error "Error during client generation: $_"
    exit 1
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review generated bindings in $OutputPath" -ForegroundColor Gray
Write-Host "  2. Add CloudFlare.Api project reference to your solution" -ForegroundColor Gray
Write-Host "  3. Configure API authentication in your code" -ForegroundColor Gray