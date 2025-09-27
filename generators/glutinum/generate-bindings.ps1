# generate-bindings.ps1 - Generate F# bindings from TypeScript definitions

param(
    [Parameter(Mandatory=$false)]
    [string]$InputFile = "workers-types.d.ts",

    [Parameter(Mandatory=$false)]
    [string]$OutputFile = "..\..\src\Runtime\CloudFlare.Worker.Context\Generated.fs"
)

Write-Host "CloudflareFS Binding Generator" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan
Write-Host ""

# Check if Glutinum is installed
$glutinumCheck = npm list -g @glutinum/cli 2>&1
if ($glutinumCheck -notmatch "@glutinum/cli") {
    Write-Host "Installing Glutinum CLI..." -ForegroundColor Yellow
    npm install -g @glutinum/cli
}

# Generate bindings
Write-Host "Generating F# bindings from $InputFile..." -ForegroundColor Yellow

try {
    npx @glutinum/cli $InputFile --out-file $OutputFile

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Bindings generated successfully" -ForegroundColor Green
        Write-Host "  Output: $OutputFile" -ForegroundColor Gray
    } else {
        Write-Error "Failed to generate bindings"
        exit 1
    }
}
catch {
    Write-Error "Error during binding generation: $_"
    Write-Host ""
    Write-Host "If you encounter stack overflow errors:" -ForegroundColor Yellow
    Write-Host "  1. Simplify the TypeScript definitions" -ForegroundColor Gray
    Write-Host "  2. Use the preprocessor: node ..\preprocessors\simplify.js" -ForegroundColor Gray
    Write-Host "  3. Consider manual binding creation for complex types" -ForegroundColor Gray
    exit 1
}