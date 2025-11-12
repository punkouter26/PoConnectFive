# start-azurite.ps1
# Start Azurite storage emulator for local development
# Azurite emulates Azure Table Storage, Blob Storage, and Queue Storage

param(
    [switch]$Blob = $true,
    [switch]$Queue = $true,
    [switch]$Table = $true,
    [string]$Location = ".",
    [switch]$Silent = $false
)

$ErrorActionPreference = "Stop"

if (-not $Silent) {
    Write-Host "🗄️ Starting Azurite Storage Emulator" -ForegroundColor Cyan
    Write-Host ""
}

# Check if Azurite is installed
if (-not (Get-Command azurite -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Azurite is not installed." -ForegroundColor Red
    Write-Host "Install with: npm install -g azurite" -ForegroundColor Yellow
    Write-Host "Or use: npm install --save-dev azurite (locally)" -ForegroundColor Yellow
    exit 1
}

# Check if Node.js is installed
if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Node.js is not installed." -ForegroundColor Red
    Write-Host "Install from: https://nodejs.org/" -ForegroundColor Yellow
    exit 1
}

# Build command
$azuriteCmd = "azurite"
$services = @()

if ($Blob) { $services += "blob" }
if ($Queue) { $services += "queue" }
if ($Table) { $services += "table" }

if ($services.Count -eq 0) {
    Write-Host "⚠️ No services selected. Enabling all services." -ForegroundColor Yellow
    $services = @("blob", "queue", "table")
}

if (-not $Silent) {
    Write-Host "📋 Services: $($services -join ', ')" -ForegroundColor Cyan
    Write-Host "📁 Data location: $Location" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "🚀 Starting Azurite..." -ForegroundColor Green
    Write-Host "   Blob Service:  http://127.0.0.1:10000" -ForegroundColor Gray
    Write-Host "   Queue Service: http://127.0.0.1:10001" -ForegroundColor Gray
    Write-Host "   Table Service: http://127.0.0.1:10002" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Press Ctrl+C to stop" -ForegroundColor Yellow
    Write-Host ""
}

try {
    # Start Azurite
    if ($Silent) {
        & azurite --location $Location --silent
    }
    else {
        & azurite --location $Location
    }
}
catch {
    Write-Host ""
    Write-Host "❌ Azurite failed to start: $_" -ForegroundColor Red
    exit 1
}
