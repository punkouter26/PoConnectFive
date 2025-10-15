# PoConnectFive Azure Deployment Script
# This script deploys the PoConnectFive application to Azure using AZD CLI

param(
    [Parameter(Mandatory=$false)]
    [string]$EnvironmentName = "prod",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "eastus2"
)

Write-Host "?? PoConnectFive Azure Deployment Script" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Check if azd is installed
Write-Host "Checking for Azure Developer CLI (azd)..." -ForegroundColor Yellow
$azdInstalled = Get-Command azd -ErrorAction SilentlyContinue
if (-not $azdInstalled) {
    Write-Host "? Azure Developer CLI (azd) is not installed." -ForegroundColor Red
    Write-Host "Please install it from: https://aka.ms/install-azd" -ForegroundColor Yellow
    exit 1
}
Write-Host "? Azure Developer CLI found" -ForegroundColor Green
Write-Host ""

# Check if user is logged in to Azure
Write-Host "Checking Azure login status..." -ForegroundColor Yellow
$azdAuth = azd auth login --check-status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "??  Not logged in to Azure. Logging in..." -ForegroundColor Yellow
    azd auth login
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Failed to login to Azure" -ForegroundColor Red
        exit 1
    }
}
Write-Host "? Logged in to Azure" -ForegroundColor Green
Write-Host ""

# Initialize environment if not already done
Write-Host "Initializing Azure Developer environment..." -ForegroundColor Yellow
$envExists = azd env list 2>&1 | Select-String -Pattern $EnvironmentName
if (-not $envExists) {
    Write-Host "Creating new environment: $EnvironmentName" -ForegroundColor Yellow
    azd env new $EnvironmentName --location $Location
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Failed to create environment" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "Environment '$EnvironmentName' already exists" -ForegroundColor Green
    azd env select $EnvironmentName
}
Write-Host ""

# Deploy infrastructure and application
Write-Host "???  Deploying infrastructure and application..." -ForegroundColor Cyan
Write-Host "This may take several minutes..." -ForegroundColor Yellow
Write-Host ""

azd up --environment $EnvironmentName

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host "? Deployment completed successfully!" -ForegroundColor Green
    Write-Host "=========================================" -ForegroundColor Green
    Write-Host ""
    
    # Get the deployed URL
    $webAppUri = azd env get-values | Select-String -Pattern "WEB_APP_URI" | ForEach-Object { $_.ToString().Split('=')[1].Trim('"') }
    if ($webAppUri) {
        Write-Host "?? Application URL: $webAppUri" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "You can now access your application at the URL above." -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "To view all environment values, run:" -ForegroundColor Yellow
    Write-Host "  azd env get-values" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "? Deployment failed" -ForegroundColor Red
    Write-Host "Please check the error messages above." -ForegroundColor Yellow
    exit 1
}
