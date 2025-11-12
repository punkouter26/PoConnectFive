# test-coverage.ps1
# Generate code coverage reports for PoConnectFive
# Requires dotnet-coverage tool: dotnet tool install --global dotnet-coverage

param(
    [string]$OutputDir = "docs/coverage",
    [int]$MinimumCoverage = 80,
    [switch]$OpenReport = $true
)

$ErrorActionPreference = "Stop"

Write-Host "📊 PoConnectFive Code Coverage Report Generator" -ForegroundColor Cyan
Write-Host ""

# Check if dotnet-coverage is installed
if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) {
    Write-Host "⚙️ Installing dotnet-coverage tool..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-coverage
}

# Create output directory
$fullOutputDir = Join-Path $PSScriptRoot "..\$OutputDir"
if (-not (Test-Path $fullOutputDir)) {
    New-Item -ItemType Directory -Path $fullOutputDir -Force | Out-Null
}

try {
    Write-Host "🧹 Cleaning previous coverage data..." -ForegroundColor Cyan
    Get-ChildItem -Path . -Recurse -Filter "*.coverage" | Remove-Item -Force
    Get-ChildItem -Path . -Recurse -Filter "coverage.*.xml" | Remove-Item -Force
    Write-Host ""

    # Run tests with coverage
    Write-Host "🧪 Running tests with coverage collection..." -ForegroundColor Cyan
    dotnet test --collect:"XPlat Code Coverage" --results-directory:"$fullOutputDir/raw"
    
    if ($LASTEXITCODE -ne 0) {
        throw "Tests failed"
    }
    Write-Host "✅ Tests completed successfully" -ForegroundColor Green
    Write-Host ""

    # Find the coverage file
    $coverageFile = Get-ChildItem -Path "$fullOutputDir/raw" -Recurse -Filter "coverage.cobertura.xml" | Select-Object -First 1
    
    if (-not $coverageFile) {
        throw "Coverage file not found"
    }

    Write-Host "📈 Generating HTML report..." -ForegroundColor Cyan
    
    # Install ReportGenerator if not already installed
    if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
        Write-Host "⚙️ Installing ReportGenerator..." -ForegroundColor Yellow
        dotnet tool install --global dotnet-reportgenerator-globaltool
    }

    # Generate report
    reportgenerator `
        -reports:"$($coverageFile.FullName)" `
        -targetdir:"$fullOutputDir/html" `
        -reporttypes:"Html;HtmlSummary;Badges;Cobertura"

    Write-Host "✅ Report generated" -ForegroundColor Green
    Write-Host ""

    # Parse coverage percentage
    [xml]$coverageXml = Get-Content $coverageFile.FullName
    $lineCoverage = [math]::Round([double]$coverageXml.coverage.'line-rate' * 100, 2)
    $branchCoverage = [math]::Round([double]$coverageXml.coverage.'branch-rate' * 100, 2)

    Write-Host "📊 Coverage Summary:" -ForegroundColor Cyan
    Write-Host "  Line Coverage:   $lineCoverage%" -ForegroundColor $(if ($lineCoverage -ge $MinimumCoverage) { "Green" } else { "Yellow" })
    Write-Host "  Branch Coverage: $branchCoverage%" -ForegroundColor $(if ($branchCoverage -ge $MinimumCoverage) { "Green" } else { "Yellow" })
    Write-Host ""

    if ($lineCoverage -lt $MinimumCoverage) {
        Write-Host "⚠️ Warning: Line coverage ($lineCoverage%) is below minimum threshold ($MinimumCoverage%)" -ForegroundColor Yellow
    } else {
        Write-Host "✅ Coverage meets minimum threshold of $MinimumCoverage%" -ForegroundColor Green
    }
    Write-Host ""

    # Copy summary to docs
    $summaryFile = Join-Path $fullOutputDir "html\index.html"
    if (Test-Path $summaryFile) {
        Write-Host "📁 Report location: $summaryFile" -ForegroundColor Cyan
        
        if ($OpenReport) {
            Write-Host "🌐 Opening report in browser..." -ForegroundColor Cyan
            Start-Process $summaryFile
        }
    }
}
catch {
    Write-Host ""
    Write-Host "❌ Coverage generation failed: $_" -ForegroundColor Red
    exit 1
}
