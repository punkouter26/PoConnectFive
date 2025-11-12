# deploy.ps1
# Azure deployment script for PoConnectFive
# Deploys the application to Azure App Service using Azure Developer CLI

param(
    [string]$Environment = "dev",
    [switch]$SkipBuild = $false
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 PoConnectFive Deployment Script" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host ""

# Check if azd is installed
if (-not (Get-Command azd -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Azure Developer CLI (azd) is not installed." -ForegroundColor Red
    Write-Host "Install from: https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd" -ForegroundColor Yellow
    exit 1
}

# Set environment
$env:AZURE_ENV_NAME = $Environment

try {
    # Build the solution (unless skipped)
    if (-not $SkipBuild) {
        Write-Host "📦 Building solution..." -ForegroundColor Cyan
        dotnet build PoConnectFive.sln --configuration Release
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed"
        }
        Write-Host "✅ Build successful" -ForegroundColor Green
        Write-Host ""
    }

    # Run tests
    Write-Host "🧪 Running tests..." -ForegroundColor Cyan
    dotnet test --no-build --configuration Release --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Host "⚠️ Tests failed - continue anyway? (Y/N)" -ForegroundColor Yellow
        $continue = Read-Host
        if ($continue -ne "Y") {
            exit 1
        }
    } else {
        Write-Host "✅ Tests passed" -ForegroundColor Green
    }
    Write-Host ""

    # Deploy to Azure
    Write-Host "☁️ Deploying to Azure..." -ForegroundColor Cyan
    azd up --environment $Environment
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Deployment successful!" -ForegroundColor Green
        Write-Host "🌐 Opening deployed application..." -ForegroundColor Cyan
        azd show --output table
    } else {
        throw "Deployment failed"
    }
}
catch {
    Write-Host ""
    Write-Host "❌ Deployment failed: $_" -ForegroundColor Red
    exit 1
}
