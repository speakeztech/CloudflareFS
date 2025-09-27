# add-user.ps1 - Add a new user to the SecureChat system
# Usage: .\scripts\add-user.ps1 -Username <username> -Password <password> [-WithStorage]

param(
    [Parameter(Mandatory=$true)]
    [string]$Username,

    [Parameter(Mandatory=$true)]
    [string]$Password,

    [Parameter(Mandatory=$false)]
    [switch]$WithStorage,  # Optional: Create R2 bucket for file storage

    [Parameter(Mandatory=$false)]
    [switch]$SkipDeploy
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Validate username (alphanumeric and underscore only)
if ($Username -notmatch '^[a-zA-Z0-9_]+$') {
    Write-Error "Error: Username must be alphanumeric (underscores allowed)"
    exit 1
}

# Validate password strength
if ($Password.Length -lt 8) {
    Write-Error "Error: Password must be at least 8 characters"
    exit 1
}

# Convert username to lowercase for consistency
$Username = $Username.ToLower()

# Deterministic naming convention
$PasswordSecret = "USER_$($Username.ToUpper())_PASSWORD"
$BucketName = "$Username-chat-storage"
$BucketBinding = "${Username}_chat_storage"

Write-Host ""
Write-Host "SecureChat User Setup" -ForegroundColor Cyan
Write-Host "=====================" -ForegroundColor Cyan
Write-Host "Username: $Username" -ForegroundColor White
if ($WithStorage) {
    Write-Host "Storage: R2 Bucket ($BucketName)" -ForegroundColor White
}
Write-Host ""

try {
    # Step 1: Hash the password using SHA-256 (matching the F# code)
    Write-Host "[1/3] Hashing password..." -ForegroundColor Yellow
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Password)
    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    $hashBytes = $sha256.ComputeHash($bytes)
    $hashedPassword = ($hashBytes | ForEach-Object { $_.ToString("x2") }) -join ''
    Write-Host "  ✓ Password hashed" -ForegroundColor Green

    # Step 2: Store hashed password as secret
    Write-Host "[2/3] Storing password hash as secret: $PasswordSecret" -ForegroundColor Yellow
    $hashedPassword | & npx wrangler secret put $PasswordSecret --name secure-chat 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Secret may already exist. To update, delete first with: npx wrangler secret delete $PasswordSecret"
    } else {
        Write-Host "  ✓ Password hash stored securely" -ForegroundColor Green
    }

    # Step 3: Optional - Create R2 bucket for file storage
    if ($WithStorage) {
        Write-Host "[3/3] Setting up R2 storage..." -ForegroundColor Yellow

        # Check if bucket exists
        $existingBuckets = & npx wrangler r2 bucket list 2>&1
        if ($existingBuckets -match $BucketName) {
            Write-Host "  ⚠ Bucket $BucketName already exists, skipping creation" -ForegroundColor DarkYellow
        } else {
            & npx wrangler r2 bucket create $BucketName
            if ($LASTEXITCODE -ne 0) { throw "Failed to create R2 bucket" }
            Write-Host "  ✓ R2 bucket created" -ForegroundColor Green
        }

        # Add binding to wrangler.toml if not exists
        $wranglerPath = Join-Path (Split-Path $PSScriptRoot -Parent) "wrangler.toml"
        $wranglerContent = Get-Content -Path $wranglerPath -Raw
        if ($wranglerContent -match "binding\s*=\s*`"$BucketBinding`"") {
            Write-Host "  ⚠ Binding already exists in wrangler.toml" -ForegroundColor DarkYellow
        } else {
            $TomlContent = @"

# User storage bucket for $Username
[[r2_buckets]]
binding = "$BucketBinding"
bucket_name = "$BucketName"
"@
            Add-Content -Path $wranglerPath -Value $TomlContent
            Write-Host "  ✓ Configuration updated" -ForegroundColor Green
        }
    }

    Write-Host ""
    Write-Host "════════════════════════════════════════" -ForegroundColor Green
    Write-Host " ✅ User setup complete!" -ForegroundColor Green
    Write-Host "════════════════════════════════════════" -ForegroundColor Green
    Write-Host ""

    if (-not $SkipDeploy) {
        Write-Host "Deploying worker..." -ForegroundColor Yellow
        & npx wrangler deploy
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✓ Worker deployed successfully" -ForegroundColor Green
        } else {
            Write-Warning "Deployment failed. Run 'npx wrangler deploy' manually"
        }
    } else {
        Write-Host "Deploy the worker to activate changes:" -ForegroundColor Cyan
        Write-Host "  npx wrangler deploy" -ForegroundColor White
    }

    Write-Host ""
    Write-Host "User Credentials:" -ForegroundColor Cyan
    Write-Host "  Username: $Username" -ForegroundColor White
    Write-Host "  Password: [as provided]" -ForegroundColor White
    Write-Host ""
    Write-Host "The user can now login to SecureChat with these credentials." -ForegroundColor Gray
    if ($WithStorage) {
        Write-Host "File upload/download is enabled for this user." -ForegroundColor Gray
    }
}
catch {
    Write-Host ""
    Write-Error "Failed to set up user: $_"
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  1. Make sure you're logged in: npx wrangler login" -ForegroundColor Gray
    Write-Host "  2. Check your Cloudflare account permissions" -ForegroundColor Gray
    Write-Host "  3. Verify wrangler.toml is properly configured" -ForegroundColor Gray
    exit 1
}