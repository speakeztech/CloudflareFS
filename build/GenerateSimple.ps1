# CloudflareFS Advanced Binding Generation Script
param(
    [switch]$SkipPreprocessing
)

$ErrorActionPreference = "Stop"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "CloudflareFS Advanced Binding Generation" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Create temp directory
$tempDir = "temp"
if (-not (Test-Path $tempDir)) {
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
}

# Step 1: Preprocess the workers-types to fix Glutinum issues
if (-not $SkipPreprocessing) {
    Write-Host "Step 1: Preprocessing TypeScript definitions..." -ForegroundColor Blue

    $workersTypesInput = "node_modules/@cloudflare/workers-types/index.d.ts"
    $workersTypesOutput = "temp/workers-types.preprocessed.d.ts"

    if (Test-Path $workersTypesInput) {
        node build/preprocessor.js $workersTypesInput $workersTypesOutput

        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✅ Preprocessing successful" -ForegroundColor Green
        } else {
            Write-Host "  ❌ Preprocessing failed" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "  ⚠️ Workers types not found, skipping preprocessing" -ForegroundColor Yellow
    }
}

# Step 2: Generate bindings with Glutinum
Write-Host ""
Write-Host "Step 2: Generating F# bindings..." -ForegroundColor Blue

$bindings = @(
    @{
        Name = "Workers Types (preprocessed)"
        Source = "temp/workers-types.preprocessed.d.ts"
        Output = "src/Runtime/CloudFlare.Worker.Context/Generated.fs"
        Namespace = "CloudFlare.Worker"
    },
    @{
        Name = "AI"
        Source = "node_modules/@cloudflare/ai/dist/index.d.ts"
        Output = "src/Runtime/CloudFlare.AI/Generated.fs"
        Namespace = "CloudFlare.AI"
    }
)

foreach ($binding in $bindings) {
    Write-Host "  Processing $($binding.Name)..." -ForegroundColor Cyan

    if (Test-Path $binding.Source) {
        # Ensure output directory exists
        $outputDir = Split-Path $binding.Output -Parent
        if (-not (Test-Path $outputDir)) {
            New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
        }

        # Run Glutinum
        $cmd = "npx @glutinum/cli `"$($binding.Source)`" --out-file `"$($binding.Output)`""

        Invoke-Expression $cmd 2>&1 | Out-Null

        if (Test-Path $binding.Output) {
            Write-Host "    ✅ Generated successfully" -ForegroundColor Green

            # Post-process the file
            Write-Host "    🔧 Post-processing..." -ForegroundColor Gray

            $content = Get-Content $binding.Output -Raw

            # Fix module declaration
            $content = $content -replace "module rec Glutinum", "module rec $($binding.Namespace)"

            # Add namespace and imports
            $header = @"
namespace $($binding.Namespace)

open Fable.Core
open Fable.Core.JsInterop
open System

"@
            $content = $header + "`n" + $content

            # Fix Promise types
            $content = $content -replace "Promise<", "JS.Promise<"

            # Add attributes to types
            $content = $content -replace "(?m)^type (\w+) =", "[<AllowNullLiteral>]`ntype `$1 ="

            Set-Content -Path $binding.Output -Value $content -Encoding UTF8

            Write-Host "    ✅ Post-processing complete" -ForegroundColor Green
        } else {
            Write-Host "    ❌ Generation failed" -ForegroundColor Red
        }
    } else {
        Write-Host "    ⚠️ Source not found: $($binding.Source)" -ForegroundColor Yellow
    }
}

# Step 3: Create extension modules
Write-Host ""
Write-Host "Step 3: Creating extension modules..." -ForegroundColor Blue

foreach ($binding in $bindings) {
    $outputDir = Split-Path $binding.Output -Parent
    $extensionPath = Join-Path $outputDir "Extensions.fs"

    $extensionContent = @"
namespace $($binding.Namespace)

[<AutoOpen>]
module Extensions =
    open Fable.Core
    open Fable.Core.JsInterop
    open System

    // Computation expression for working with promises
    type PromiseBuilder() =
        member _.Bind(m: JS.Promise<'a>, f: 'a -> JS.Promise<'b>) =
            m?``then``(f) :?> JS.Promise<'b>
        member _.Return(x: 'a) =
            JS.Promise.resolve(x)
        member _.ReturnFrom(x: JS.Promise<'a>) = x
        member _.Zero() = JS.Promise.resolve()

    let promise = PromiseBuilder()

    // Async extensions
    type JS.Promise<'T> with
        member this.ToAsync() =
            Async.AwaitPromise this
"@

    Set-Content -Path $extensionPath -Value $extensionContent -Encoding UTF8
    Write-Host "  ✅ Created extension module for $($binding.Namespace)" -ForegroundColor Green
}

# Clean up
if (Test-Path $tempDir) {
    Remove-Item -Path $tempDir -Recurse -Force
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "✅ Generation Complete!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan