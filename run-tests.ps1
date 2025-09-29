# CloudFlareFS Test Runner Script (PowerShell)

param(
    [Parameter(Position=0)]
    [string]$Filter = "",

    [Parameter()]
    [switch]$Watch,

    [Parameter()]
    [switch]$FableTests,

    [Parameter()]
    [switch]$Coverage,

    [Parameter()]
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "CloudFlareFS Test Runner" -ForegroundColor Cyan
Write-Host "========================" -ForegroundColor Cyan
Write-Host ""

# Change to tests directory
Push-Location tests/CloudFlare.Tests

try {
    if ($FableTests) {
        Write-Host "Running Fable Tests (JavaScript)..." -ForegroundColor Green

        # Install npm dependencies if needed
        if (-not (Test-Path "node_modules")) {
            Write-Host "Installing npm dependencies..." -ForegroundColor Yellow
            npm install
        }

        if ($Watch) {
            npm run test:fable:watch
        } else {
            npm run test:fable
        }
    }
    elseif ($Coverage) {
        Write-Host "Running Tests with Code Coverage..." -ForegroundColor Green
        dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

        Write-Host ""
        Write-Host "Coverage report generated in coverage.opencover.xml" -ForegroundColor Yellow
    }
    else {
        Write-Host "Running .NET Tests (Expecto)..." -ForegroundColor Green

        $args = @()

        if ($Filter) {
            $args += "--filter=$Filter"
            Write-Host "Filter: $Filter" -ForegroundColor Yellow
        }

        if ($Watch) {
            Write-Host "Watch mode enabled" -ForegroundColor Yellow
            dotnet watch run $args
        }
        elseif ($Verbose) {
            dotnet run -- --debug $args
        }
        else {
            dotnet run $args
        }
    }

    Write-Host ""
    Write-Host "Tests completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "Test run failed: $_" -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}