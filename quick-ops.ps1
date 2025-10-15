# PoConnectFive - Quick Operations Script
# Common commands for managing the PoConnectFive application

Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "   PoConnectFive - Quick Operations" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

function Show-Menu {
    Write-Host "Select an operation:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  LOCAL DEVELOPMENT" -ForegroundColor Green
    Write-Host "  1. Start Azurite (Azure Storage Emulator)"
    Write-Host "  2. Run Application Locally"
    Write-Host "  3. Run Integration Tests"
    Write-Host "  4. Check Application Health"
    Write-Host ""
    Write-Host "  AZURE DEPLOYMENT" -ForegroundColor Green
    Write-Host "  5. Deploy to Azure (Full)"
    Write-Host "  6. Deploy Code Only"
    Write-Host "  7. View Deployment Status"
    Write-Host "  8. Open Azure App Service in Browser"
    Write-Host ""
    Write-Host "  UTILITIES" -ForegroundColor Green
    Write-Host "  9. Build Solution"
    Write-Host " 10. Run All Tests"
    Write-Host " 11. Clean Solution"
    Write-Host ""
    Write-Host "  0. Exit" -ForegroundColor Red
    Write-Host ""
}

function Start-Azurite {
    Write-Host "Starting Azurite..." -ForegroundColor Yellow
    $azuriteDir = "C:\azurite"
    if (-not (Test-Path $azuriteDir)) {
        New-Item -ItemType Directory -Path $azuriteDir | Out-Null
    }
    Start-Process azurite -ArgumentList "--silent --location $azuriteDir --debug $azuriteDir\debug.log" -NoNewWindow
    Write-Host "? Azurite started" -ForegroundColor Green
}

function Run-LocalApp {
    Write-Host "Running application locally..." -ForegroundColor Yellow
    Set-Location -Path (Split-Path -Parent $PSScriptRoot)
    dotnet run --project PoConnectFive.Server
}

function Run-IntegrationTests {
    Write-Host "Running integration tests..." -ForegroundColor Yellow
    Set-Location -Path (Split-Path -Parent $PSScriptRoot)
    dotnet test PoConnectFive.Tests --filter "FullyQualifiedName~Integration"
}

function Check-AppHealth {
    Write-Host "Checking application health..." -ForegroundColor Yellow
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/api/health" -UseBasicParsing
        $health = $response.Content | ConvertFrom-Json
        Write-Host "? Status: $($health.Status)" -ForegroundColor Green
        Write-Host "Timestamp: $($health.Timestamp)" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Component Health:" -ForegroundColor Cyan
        foreach ($check in $health.Checks) {
            $icon = if ($check.IsHealthy) { "?" } else { "?" }
            $color = if ($check.IsHealthy) { "Green" } else { "Red" }
            Write-Host "  $icon $($check.Component)" -ForegroundColor $color
            if (-not $check.IsHealthy) {
                Write-Host "     Error: $($check.Error)" -ForegroundColor Red
            }
        }
    }
    catch {
        Write-Host "? Failed to connect to application" -ForegroundColor Red
        Write-Host "Make sure the application is running on http://localhost:5000" -ForegroundColor Yellow
    }
}

function Deploy-ToAzure {
    Write-Host "Deploying to Azure..." -ForegroundColor Yellow
    .\deploy-azure.ps1
}

function Deploy-CodeOnly {
    Write-Host "Deploying code only..." -ForegroundColor Yellow
    azd deploy
}

function Show-DeploymentStatus {
    Write-Host "Deployment Status:" -ForegroundColor Yellow
    azd env get-values
}

function Open-AzureApp {
    Write-Host "Opening Azure app in browser..." -ForegroundColor Yellow
    $appUri = azd env get-value WEB_APP_URI 2>$null
    if ($appUri) {
        Start-Process $appUri
    } else {
        Write-Host "? Application not deployed or environment not set" -ForegroundColor Red
        Write-Host "Run deployment first (option 5)" -ForegroundColor Yellow
    }
}

function Build-Solution {
    Write-Host "Building solution..." -ForegroundColor Yellow
    Set-Location -Path (Split-Path -Parent $PSScriptRoot)
    dotnet build
}

function Run-AllTests {
    Write-Host "Running all tests..." -ForegroundColor Yellow
    Set-Location -Path (Split-Path -Parent $PSScriptRoot)
    dotnet test
}

function Clean-Solution {
    Write-Host "Cleaning solution..." -ForegroundColor Yellow
    Set-Location -Path (Split-Path -Parent $PSScriptRoot)
    dotnet clean
}

# Main loop
do {
    Show-Menu
    $choice = Read-Host "Enter your choice"
    Write-Host ""
    
    switch ($choice) {
        '1' { Start-Azurite; Pause }
        '2' { Run-LocalApp }
        '3' { Run-IntegrationTests; Pause }
        '4' { Check-AppHealth; Pause }
        '5' { Deploy-ToAzure; Pause }
        '6' { Deploy-CodeOnly; Pause }
        '7' { Show-DeploymentStatus; Pause }
        '8' { Open-AzureApp; Pause }
        '9' { Build-Solution; Pause }
        '10' { Run-AllTests; Pause }
        '11' { Clean-Solution; Pause }
        '0' { Write-Host "Goodbye!" -ForegroundColor Cyan; break }
        default { Write-Host "Invalid choice. Please try again." -ForegroundColor Red; Pause }
    }
    
    Write-Host ""
} while ($choice -ne '0')
