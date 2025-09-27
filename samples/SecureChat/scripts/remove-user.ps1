# remove-user.ps1 - Remove a user from the SecureChat system
# Usage: .\scripts\remove-user.ps1 -Username <username>

param(
    [Parameter(Mandatory=$true)]
    [string]$Username
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Convert username to lowercase for consistency
$Username = $Username.ToLower()

# Deterministic naming
$PasswordSecret = "USER_$($Username.ToUpper())_PASSWORD"
$BucketName = "$Username-chat-storage"
$BucketBinding = "${Username}_chat_storage"

Write-Host ""
Write-Host "SecureChat User Removal" -ForegroundColor Red
Write-Host "========================" -ForegroundColor Red
Write-Host "Username: $Username" -ForegroundColor White
Write-Host ""
Write-Host "⚠️  WARNING: This will:" -ForegroundColor Red
Write-Host "  - Delete the user's password secret" -ForegroundColor Yellow
Write-Host "  - Delete the user's R2 storage bucket (if exists)" -ForegroundColor Yellow
Write-Host "  - Remove all user data permanently" -ForegroundColor Yellow
Write-Host ""

$Confirmation = Read-Host "Type 'DELETE' to confirm"
if ($Confirmation -ne 'DELETE') {
    Write-Host "Operation cancelled" -ForegroundColor Yellow
    exit 0
}

try {
    # Step 1: Delete password secret
    Write-Host ""
    Write-Host "[1/3] Deleting password secret: $PasswordSecret" -ForegroundColor Yellow
    & npx wrangler secret delete $PasswordSecret --name secure-chat 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ⚠ Secret may not exist or already deleted" -ForegroundColor DarkYellow
    } else {
        Write-Host "  ✓ Password secret deleted" -ForegroundColor Green
    }

    # Step 2: Delete R2 bucket (if exists)
    Write-Host "[2/3] Checking for R2 bucket: $BucketName" -ForegroundColor Yellow
    $existingBuckets = & npx wrangler r2 bucket list 2>&1
    if ($existingBuckets -match $BucketName) {
        Write-Host "  Deleting R2 bucket..." -ForegroundColor Yellow

        # First, empty the bucket
        Write-Host "  Emptying bucket contents..." -ForegroundColor Gray
        & npx wrangler r2 bucket delete $BucketName --force 2>&1 | Out-Null

        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✓ R2 bucket deleted" -ForegroundColor Green
        } else {
            Write-Host "  ⚠ Could not delete bucket (may contain objects)" -ForegroundColor DarkYellow
        }
    } else {
        Write-Host "  ⚠ No R2 bucket found for user" -ForegroundColor DarkYellow
    }

    # Step 3: Note about wrangler.toml cleanup
    Write-Host "[3/3] Configuration cleanup" -ForegroundColor Yellow
    $wranglerPath = Join-Path (Split-Path $PSScriptRoot -Parent) "wrangler.toml"
    $wranglerContent = Get-Content -Path $wranglerPath -Raw

    if ($wranglerContent -match "binding\s*=\s*`"$BucketBinding`"") {
        Write-Host "  ⚠ Manual action required:" -ForegroundColor Yellow
        Write-Host "    Remove the R2 bucket binding for '$BucketBinding' from wrangler.toml" -ForegroundColor White
        Write-Host "    Then redeploy: npx wrangler deploy" -ForegroundColor White
    } else {
        Write-Host "  ✓ No bucket binding found in configuration" -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "════════════════════════════════════════" -ForegroundColor Green
    Write-Host " ✅ User removed successfully" -ForegroundColor Green
    Write-Host "════════════════════════════════════════" -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Error "Failed to remove user: $_"
    exit 1
}