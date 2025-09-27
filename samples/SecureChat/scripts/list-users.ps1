# list-users.ps1 - List all configured users in the SecureChat system
# Usage: .\scripts\list-users.ps1

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "SecureChat User List" -ForegroundColor Cyan
Write-Host "====================" -ForegroundColor Cyan
Write-Host ""

try {
    # Get all secrets from the worker
    Write-Host "Fetching configured secrets..." -ForegroundColor Yellow
    $secrets = & npx wrangler secret list --name secure-chat 2>&1

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to list secrets. Make sure you're logged in: npx wrangler login"
        exit 1
    }

    # Parse user secrets (format: USER_<USERNAME>_PASSWORD)
    $userPattern = 'USER_(.+)_PASSWORD'
    $users = @()

    foreach ($line in $secrets -split "`n") {
        if ($line -match $userPattern) {
            $username = $Matches[1].ToLower()
            $users += $username
        }
    }

    if ($users.Count -eq 0) {
        Write-Host "No users configured" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "To add a user, run:" -ForegroundColor Gray
        Write-Host "  .\scripts\add-user.ps1 -Username <username> -Password <password>" -ForegroundColor White
    } else {
        Write-Host "Configured Users:" -ForegroundColor Green
        Write-Host ""

        # Check for R2 buckets
        $buckets = & npx wrangler r2 bucket list 2>&1

        foreach ($user in $users | Sort-Object) {
            Write-Host "  • $user" -ForegroundColor White -NoNewline

            # Check if user has R2 storage
            $bucketName = "$user-chat-storage"
            if ($buckets -match $bucketName) {
                Write-Host " [R2 Storage Enabled]" -ForegroundColor Green
            } else {
                Write-Host ""
            }
        }

        Write-Host ""
        Write-Host "Total users: $($users.Count)" -ForegroundColor Cyan
    }

    # Check wrangler.toml for orphaned bindings
    Write-Host ""
    Write-Host "Checking for configuration issues..." -ForegroundColor Yellow

    $wranglerPath = Join-Path (Split-Path $PSScriptRoot -Parent) "wrangler.toml"
    if (Test-Path $wranglerPath) {
        $wranglerContent = Get-Content -Path $wranglerPath -Raw
        $bindingPattern = 'binding\s*=\s*"([^"]+_chat_storage)"'
        $matches = [regex]::Matches($wranglerContent, $bindingPattern)

        $orphaned = @()
        foreach ($match in $matches) {
            $binding = $match.Groups[1].Value
            $expectedUser = $binding -replace '_chat_storage$', ''
            if ($expectedUser -notin $users) {
                $orphaned += $binding
            }
        }

        if ($orphaned.Count -gt 0) {
            Write-Host "  ⚠ Found orphaned R2 bindings in wrangler.toml:" -ForegroundColor Yellow
            foreach ($binding in $orphaned) {
                Write-Host "    - $binding" -ForegroundColor Gray
            }
            Write-Host "  Consider removing these bindings and redeploying" -ForegroundColor Gray
        } else {
            Write-Host "  ✓ Configuration is clean" -ForegroundColor Green
        }
    }

    Write-Host ""
}
catch {
    Write-Host ""
    Write-Error "Failed to list users: $_"
    exit 1
}