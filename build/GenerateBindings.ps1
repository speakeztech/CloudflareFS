# CloudflareFS Binding Generation Script (PowerShell)
# This script generates F# bindings from Cloudflare TypeScript definitions

$ErrorActionPreference = "Stop"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "CloudflareFS Binding Generation" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Glutinum is installed
$glutinumCheck = npm list -g @glutinum/cli --depth=0 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Installing Glutinum CLI globally..." -ForegroundColor Yellow
    npm install -g @glutinum/cli
}

# Ensure npm packages are installed
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing npm packages..." -ForegroundColor Yellow
    npm install
}

# Configuration for bindings
$bindings = @(
    @{
        Name = "Workers Types"
        Source = "node_modules/@cloudflare/workers-types/index.d.ts"
        Output = "src/Runtime/CloudFlare.Worker.Context/Generated.fs"
    },
    @{
        Name = "AI"
        Source = "node_modules/@cloudflare/ai/dist/index.d.ts"
        Output = "src/Runtime/CloudFlare.AI/Generated.fs"
    }
)

$successful = 0
$failed = 0

foreach ($binding in $bindings) {
    Write-Host "Generating bindings for $($binding.Name)..." -ForegroundColor Blue

    if (-not (Test-Path $binding.Source)) {
        Write-Host "  Source file not found: $($binding.Source)" -ForegroundColor Red
        $failed++
        continue
    }

    # Ensure output directory exists
    $outputDir = Split-Path $binding.Output -Parent
    if (-not (Test-Path $outputDir)) {
        New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
    }

    # Run Glutinum
    $cmd = "npx @glutinum/cli `"$($binding.Source)`" --out-file `"$($binding.Output)`""
    Write-Host "  Running: $cmd" -ForegroundColor DarkGray

    try {
        Invoke-Expression $cmd
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  Success!" -ForegroundColor Green
            $successful++
        } else {
            Write-Host "  Failed with exit code: $LASTEXITCODE" -ForegroundColor Red
            $failed++
        }
    }
    catch {
        Write-Host "  Error: $_" -ForegroundColor Red
        $failed++
    }
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Generation Complete" -ForegroundColor Cyan
Write-Host "Successful: $successful" -ForegroundColor Green
if ($failed -gt 0) {
    Write-Host "Failed: $failed" -ForegroundColor Red
}
Write-Host "=========================================" -ForegroundColor Cyan