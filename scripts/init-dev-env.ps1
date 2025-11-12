# init-dev-env.ps1
# Initialize development environment for PoConnectFive
# Sets up all required tools, dependencies, and local services

param(
    [switch]$SkipAzurite = $false,
    [switch]$SkipRestore = $false
)

$ErrorActionPreference = "Stop"

Write-Host "🔧 PoConnectFive Development Environment Setup" -ForegroundColor Cyan
Write-Host ""

# Check .NET SDK
Write-Host "Checking .NET SDK..." -ForegroundColor Cyan
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "❌ .NET SDK is not installed." -ForegroundColor Red
    Write-Host "Install from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

$dotnetVersion = dotnet --version
Write-Host "✅ .NET SDK: $dotnetVersion" -ForegroundColor Green
Write-Host ""

# Check Node.js (required for Azurite)
Write-Host "Checking Node.js..." -ForegroundColor Cyan
if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "⚠️ Node.js is not installed (required for Azurite)." -ForegroundColor Yellow
    Write-Host "Install from: https://nodejs.org/" -ForegroundColor Yellow
} else {
    $nodeVersion = node --version
    Write-Host "✅ Node.js: $nodeVersion" -ForegroundColor Green
}
Write-Host ""

# Check global.json SDK version
Write-Host "Checking SDK version requirements..." -ForegroundColor Cyan
if (Test-Path "global.json") {
    $globalJson = Get-Content "global.json" | ConvertFrom-Json
    $requiredSdk = $globalJson.sdk.version
    Write-Host "  Required SDK: $requiredSdk" -ForegroundColor Gray
    
    if ($dotnetVersion -ne $requiredSdk) {
        Write-Host "⚠️ Warning: Installed SDK ($dotnetVersion) differs from required version ($requiredSdk)" -ForegroundColor Yellow
    } else {
        Write-Host "✅ SDK version matches requirements" -ForegroundColor Green
    }
}
Write-Host ""

# Restore NuGet packages
if (-not $SkipRestore) {
    Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Cyan
    dotnet restore PoConnectFive.sln
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Package restore failed" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Packages restored" -ForegroundColor Green
    Write-Host ""
}

# Build solution
Write-Host "🔨 Building solution..." -ForegroundColor Cyan
dotnet build PoConnectFive.sln --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Build successful" -ForegroundColor Green
Write-Host ""

# Run tests
Write-Host "🧪 Running tests..." -ForegroundColor Cyan
dotnet test --no-build --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️ Some tests failed" -ForegroundColor Yellow
} else {
    Write-Host "✅ All tests passed" -ForegroundColor Green
}
Write-Host ""

# Check/Install development tools
Write-Host "🛠️ Checking development tools..." -ForegroundColor Cyan

# Check Azurite
if (-not $SkipAzurite) {
    if (-not (Get-Command azurite -ErrorAction SilentlyContinue)) {
        Write-Host "  Installing Azurite globally..." -ForegroundColor Yellow
        npm install -g azurite
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✅ Azurite installed" -ForegroundColor Green
        }
    } else {
        Write-Host "  ✅ Azurite is installed" -ForegroundColor Green
    }
}

# Check dotnet-coverage
if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) {
    Write-Host "  Installing dotnet-coverage..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-coverage
    Write-Host "  ✅ dotnet-coverage installed" -ForegroundColor Green
} else {
    Write-Host "  ✅ dotnet-coverage is installed" -ForegroundColor Green
}

# Check ReportGenerator
if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
    Write-Host "  Installing ReportGenerator..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-reportgenerator-globaltool
    Write-Host "  ✅ ReportGenerator installed" -ForegroundColor Green
} else {
    Write-Host "  ✅ ReportGenerator is installed" -ForegroundColor Green
}

Write-Host ""
Write-Host "✅ Development environment setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📚 Next steps:" -ForegroundColor Cyan
Write-Host "  1. Start Azurite: .\scripts\start-azurite.ps1" -ForegroundColor Gray
Write-Host "  2. Run API: dotnet run --project src\Po.ConnectFive.Api" -ForegroundColor Gray
Write-Host "  3. Run Client: dotnet run --project src\Po.ConnectFive.Client" -ForegroundColor Gray
Write-Host "  4. Or press F5 in VS Code for debug launch" -ForegroundColor Gray
Write-Host ""
