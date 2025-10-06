# Generate Workers Management API with post-processing
# This script runs Hawaii generation and applies post-processors for known gaps

$ErrorActionPreference = "Stop"

Write-Host "Generating Workers Management API..." -ForegroundColor Cyan

# Step 1: Run Hawaii
Write-Host "`n[1/3] Running Hawaii code generation..." -ForegroundColor Yellow
Push-Location $PSScriptRoot
dotnet run --project D:\repos\Hawaii\src\Hawaii.fsproj -- --config workers-hawaii.json
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Hawaii generation failed" -ForegroundColor Red
    Pop-Location
    exit 1
}
Pop-Location

# Step 2: Post-process discriminated unions
Write-Host "`n[2/3] Post-processing: Adding discriminated unions..." -ForegroundColor Yellow
dotnet fsi "$PSScriptRoot\post-process-discriminators.fsx" "$PSScriptRoot\Generated-Workers\Types.fs"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Discriminator post-processing failed" -ForegroundColor Red
    exit 1
}

# Step 3: Post-process JObject multipart
Write-Host "`n[3/4] Post-processing: Fixing JObject multipart..." -ForegroundColor Yellow
dotnet fsi "$PSScriptRoot\post-process-jobject.fsx" "$PSScriptRoot\Generated-Workers\Client.fs"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: JObject post-processing failed" -ForegroundColor Red
    exit 1
}

# Step 4: Post-process secrets API
Write-Host "`n[4/4] Post-processing: Adding secrets API body parameter..." -ForegroundColor Yellow
dotnet fsi "$PSScriptRoot\post-process-secrets.fsx"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Secrets post-processing failed" -ForegroundColor Red
    exit 1
}

Write-Host "`n✓ Generation complete!" -ForegroundColor Green
Write-Host "  Output: $PSScriptRoot\Generated-Workers\" -ForegroundColor Gray
Write-Host "`nTo use the generated code, copy to src/Management/CloudFlare.Management.Workers/:" -ForegroundColor Gray
Write-Host "  cp Generated-Workers/Types.fs ../../src/Management/CloudFlare.Management.Workers/" -ForegroundColor Gray
Write-Host "  cp Generated-Workers/Client.fs ../../src/Management/CloudFlare.Management.Workers/" -ForegroundColor Gray
