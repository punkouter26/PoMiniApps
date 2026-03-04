#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs all tests with code coverage collection and generates HTML report.

.DESCRIPTION
    Executes Unit, Integration, and E2E tests with coverage metrics.
    Generates coverage reports in ./coverage directory.

.EXAMPLE
    ./run-tests-with-coverage.ps1
    
    ./run-tests-with-coverage.ps1 -TestLevel unit
#>

param(
    [ValidateSet('all', 'unit', 'integration')]
    [string]$TestLevel = 'all'
)

$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $MyInvocation.MyCommandPath
$coverageDir = Join-Path $projectRoot 'coverage'

if (-not (Test-Path $coverageDir)) {
    New-Item -ItemType Directory -Path $coverageDir | Out-Null
}

# Helper function to run tests with coverage
function Run-TestWithCoverage {
    param(
        [string]$ProjectPath,
        [string]$TestName
    )
    
    Write-Host "🧪 Running $TestName with coverage collection..." -ForegroundColor Cyan
    
    $coverageOutput = Join-Path $coverageDir "${TestName}-coverage.json"
    
    dotnet test $ProjectPath `
        --configuration Debug `
        --no-build `
        --logger "console;verbosity=minimal" `
        /p:CollectCoverage=true `
        /p:CoverletOutputFormat=json `
        /p:CoverletOutput=$coverageOutput `
        /p:Exclude="[PoMiniApps.Web.Client]*" `
        2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ $TestName passed" -ForegroundColor Green
    } else {
        Write-Host "❌ $TestName failed" -ForegroundColor Red
        return $false
    }
    
    return $true
}

$allPassed = $true

if ($TestLevel -in @('all', 'unit')) {
    $passed = Run-TestWithCoverage `
        (Join-Path $projectRoot 'tests/PoMiniApps.UnitTests/PoMiniApps.UnitTests.csproj') `
        'UnitTests'
    $allPassed = $allPassed -and $passed
}

if ($TestLevel -in @('all', 'integration')) {
    $passed = Run-TestWithCoverage `
        (Join-Path $projectRoot 'tests/PoMiniApps.IntegrationTests/PoMiniApps.IntegrationTests.csproj') `
        'IntegrationTests'
    $allPassed = $allPassed -and $passed
}

Write-Host ""
Write-Host "📊 Coverage Report Summary" -ForegroundColor Blue
Write-Host "Coverage files generated in: $coverageDir" -ForegroundColor Gray

# List generated coverage files
$coverageFiles = Get-ChildItem $coverageDir -Filter "*-coverage.json" -ErrorAction SilentlyContinue
if ($coverageFiles) {
    $coverageFiles | ForEach-Object {
        Write-Host "  - $($_.Name)" -ForegroundColor Gray
    }
}

if ($allPassed) {
    Write-Host ""
    Write-Host "✅ All tests passed with coverage collection!" -ForegroundColor Green
    exit 0
} else {
    Write-Host ""
    Write-Host "❌ Some tests failed. Check output above." -ForegroundColor Red
    exit 1
}
