# CloudflareFS Master Generation Script
# =====================================
# "Fire and Forget" script to update sources and regenerate all bindings
#
# Usage:
#   ./update-and-generate.ps1              # Full update and regeneration
#   ./update-and-generate.ps1 -RuntimeOnly # Only regenerate runtime bindings
#   ./update-and-generate.ps1 -ManagementOnly # Only regenerate management bindings
#   ./update-and-generate.ps1 -SkipUpdate  # Skip source updates, just regenerate
#   ./update-and-generate.ps1 -Clean       # Clean all temp files before generation

param(
    [switch]$SkipUpdate = $false,
    [switch]$RuntimeOnly = $false,
    [switch]$ManagementOnly = $false,
    [switch]$Clean = $false,
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Stop"
$ProgressPreference = if ($Verbose) { "Continue" } else { "SilentlyContinue" }

# Configuration
$CloudflareApiSchemaPath = "D:/repos/Cloudflare/api-schemas"
$WorkingDirectory = $PSScriptRoot

function Write-Step($message) {
    Write-Host "`n$message" -ForegroundColor Cyan
    Write-Host ("-" * $message.Length) -ForegroundColor Cyan
}

function Write-Success($message) {
    Write-Host "✅ $message" -ForegroundColor Green
}

function Write-Info($message) {
    Write-Host "   $message" -ForegroundColor Gray
}

function Write-Warning($message) {
    Write-Host "⚠️  $message" -ForegroundColor Yellow
}

function Write-Error($message) {
    Write-Host "❌ $message" -ForegroundColor Red
}

# Main execution
try {
    Write-Host @"
╔══════════════════════════════════════════════════════════╗
║     CloudflareFS Binding Generation Pipeline            ║
║     Type-safe F# bindings for Cloudflare's platform     ║
╚══════════════════════════════════════════════════════════╝
"@ -ForegroundColor Cyan

    # Step 0: Validate environment
    Write-Step "[0/5] Validating Environment"

    # Check for required tools
    $requiredTools = @(
        @{Name = "dotnet"; Check = "dotnet --version"; Install = "https://dot.net" },
        @{Name = "node"; Check = "node --version"; Install = "https://nodejs.org" },
        @{Name = "npm"; Check = "npm --version"; Install = "Included with Node.js" }
    )

    foreach ($tool in $requiredTools) {
        try {
            $null = Invoke-Expression $tool.Check 2>$null
            Write-Success "$($tool.Name) is installed"
        }
        catch {
            Write-Error "$($tool.Name) is not installed. Install from: $($tool.Install)"
            exit 1
        }
    }

    # Check for Cloudflare API schemas
    if (-not (Test-Path $CloudflareApiSchemaPath)) {
        Write-Error "Cloudflare API schemas not found at: $CloudflareApiSchemaPath"
        Write-Info "Please clone: https://github.com/cloudflare/api-schemas"
        exit 1
    }

    # Step 1: Clean if requested
    if ($Clean) {
        Write-Step "[1/5] Cleaning Temporary Files"

        @("glutinum/temp", "hawaii/temp") | ForEach-Object {
            $path = Join-Path $WorkingDirectory $_
            if (Test-Path $path) {
                Remove-Item -Path $path -Recurse -Force
                Write-Success "Cleaned $_"
            }
        }
    }

    # Step 2: Update source schemas if not skipped
    if (-not $SkipUpdate) {
        Write-Step "[2/5] Updating Source Schemas"

        # Update Cloudflare API schemas
        Write-Info "Updating Cloudflare API schemas..."
        Push-Location $CloudflareApiSchemaPath
        try {
            $gitOutput = git pull origin main 2>&1
            if ($gitOutput -match "Already up to date") {
                Write-Success "API schemas are up to date"
            }
            else {
                Write-Success "API schemas updated"
            }
        }
        finally {
            Pop-Location
        }

        # Update TypeScript packages
        Write-Info "Updating TypeScript packages..."
        $packages = @("@cloudflare/workers-types", "@cloudflare/ai")

        foreach ($package in $packages) {
            npm update $package 2>&1 | Out-Null
            Write-Success "Updated $package"
        }
    }
    else {
        Write-Step "[2/5] Skipping Source Updates (--SkipUpdate specified)"
    }

    # Step 3: Install/Update generation tools
    Write-Step "[3/5] Checking Generation Tools"

    # Check/Install Glutinum
    $glutinumCheck = npm list -g @glutinum/cli 2>&1
    if ($glutinumCheck -notmatch "@glutinum/cli") {
        Write-Info "Installing Glutinum CLI..."
        npm install -g @glutinum/cli
        Write-Success "Glutinum CLI installed"
    }
    else {
        Write-Success "Glutinum CLI is available"
    }

    # Check/Install Hawaii
    $hawaiiCheck = dotnet tool list -g | Select-String "hawaii"
    if (-not $hawaiiCheck) {
        Write-Info "Installing Hawaii..."
        dotnet tool install -g hawaii
        Write-Success "Hawaii installed"
    }
    else {
        Write-Success "Hawaii is available"
    }

    # Step 4: Generate Runtime bindings (TypeScript → F#)
    if (-not $ManagementOnly) {
        Write-Step "[4/5] Generating Runtime Bindings (Glutinum)"

        $glutinumScript = Join-Path $WorkingDirectory "glutinum/generate-runtime.ps1"
        if (Test-Path $glutinumScript) {
            Push-Location (Join-Path $WorkingDirectory "glutinum")
            try {
                # Ensure temp directory exists
                New-Item -ItemType Directory -Path "temp" -Force | Out-Null

                # Copy TypeScript definitions to temp
                $workerTypes = "../../node_modules/@cloudflare/workers-types/index.d.ts"
                if (Test-Path $workerTypes) {
                    Copy-Item $workerTypes "temp/workers-types.d.ts" -Force
                    Write-Info "Copied workers-types.d.ts to temp"
                }
                else {
                    Write-Warning "workers-types not found, skipping runtime generation"
                }

                # Run generation
                & ./generate-runtime.ps1
                if ($LASTEXITCODE -eq 0) {
                    Write-Success "Runtime bindings generated"
                }
                else {
                    Write-Error "Runtime binding generation failed"
                    exit 1
                }
            }
            finally {
                Pop-Location
            }
        }
        else {
            Write-Warning "Runtime generation script not found: $glutinumScript"
        }
    }
    else {
        Write-Info "Skipping runtime bindings (--ManagementOnly specified)"
    }

    # Step 5: Generate Management bindings (OpenAPI → F#)
    if (-not $RuntimeOnly) {
        Write-Step "[5/5] Generating Management Bindings (Hawaii)"

        $hawaiiScript = Join-Path $WorkingDirectory "hawaii/generate-management.ps1"
        if (Test-Path $hawaiiScript) {
            Push-Location (Join-Path $WorkingDirectory "hawaii")
            try {
                # Ensure temp directory exists
                New-Item -ItemType Directory -Path "temp" -Force | Out-Null

                # Extract service-specific OpenAPI specs
                $services = @("KV", "R2", "D1", "Workers", "DNS", "Access")

                foreach ($service in $services) {
                    Write-Info "Extracting $service OpenAPI..."

                    $extractScript = "./extract-service.ps1"
                    if (Test-Path $extractScript) {
                        & $extractScript `
                            -Service $service `
                            -InputFile "$CloudflareApiSchemaPath/openapi.json" `
                            -OutputFile "temp/$service-openapi.json"

                        if ($LASTEXITCODE -eq 0) {
                            Write-Success "$service OpenAPI extracted"
                        }
                        else {
                            Write-Warning "$service extraction failed"
                        }
                    }
                }

                # Generate F# bindings
                & ./generate-management.ps1 -TempDir "temp"
                if ($LASTEXITCODE -eq 0) {
                    Write-Success "Management bindings generated"
                }
                else {
                    Write-Error "Management binding generation failed"
                    exit 1
                }
            }
            finally {
                Pop-Location
            }
        }
        else {
            Write-Warning "Management generation script not found: $hawaiiScript"
        }
    }
    else {
        Write-Info "Skipping management bindings (--RuntimeOnly specified)"
    }

    # Summary
    Write-Host @"

╔══════════════════════════════════════════════════════════╗
║                  Generation Complete!                    ║
╚══════════════════════════════════════════════════════════╝
"@ -ForegroundColor Green

    Write-Success "Runtime bindings:    src/Runtime/"
    Write-Success "Management bindings: src/Management/"

    Write-Host "`nNext steps:" -ForegroundColor Yellow
    Write-Info "1. Review generated files for correctness"
    Write-Info "2. Run tests: dotnet test"
    Write-Info "3. Build packages: dotnet build"
}
catch {
    Write-Error "Generation failed: $_"
    exit 1
}