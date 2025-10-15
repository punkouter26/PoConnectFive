# PoConnectFive - Azure Deployment Guide

## Overview

This guide provides instructions for deploying the PoConnectFive application to Azure using Azure Developer CLI (azd).

## Prerequisites

- [Azure Developer CLI (azd)](https://aka.ms/install-azd)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Azure subscription with appropriate permissions

## Azure Resources Created

The deployment creates the following resources in the **PoConnectFive** resource group:

1. **App Service** (PoConnectFive)
   - Hosts the Blazor WebAssembly application and API
   - Uses the shared App Service Plan from PoShared resource group
   
2. **Managed Identity** (id-PoConnectFive)
   - Provides secure, password-less authentication to Azure resources
   - Has Storage Table Data Contributor role on shared storage account

### Shared Resources (from PoShared resource group)

The application uses these existing shared resources:

1. **App Service Plan** (PoSharedAppServicePlan) - Free/Shared tier
2. **Application Insights** (PoSharedApplicationInsights) - For telemetry and logging
3. **Storage Account** (posharedtablestorage) - For Azure Table Storage

## Local Development Setup

### 1. Install Azurite (Azure Storage Emulator)

```bash
# Install Azurite globally
npm install -g azurite

# Or use VS Code extension: "Azurite" by Microsoft
```

### 2. Start Azurite

```bash
# Start Azurite with default settings
azurite --silent --location c:\azurite --debug c:\azurite\debug.log
```

### 3. Configure Application Settings

The application is pre-configured for local development in `appsettings.Development.json`:

```json
{
  "AzureTableStorage": {
    "ConnectionString": "UseDevelopmentStorage=true"
  }
}
```

### 4. Run the Application Locally

```bash
# From the solution root
dotnet run --project PoConnectFive.Server

# Or press F5 in VS Code
```

Navigate to: https://localhost:5001

## Azure Deployment

### Option 1: Quick Deployment (Recommended)

Use the provided PowerShell script for automated deployment:

```powershell
# Deploy to production environment in eastus2
.\deploy-azure.ps1

# Or specify environment and location
.\deploy-azure.ps1 -EnvironmentName "prod" -Location "eastus2"
```

### Option 2: Manual Deployment

#### Step 1: Login to Azure

```bash
azd auth login
```

#### Step 2: Initialize Environment

```bash
# Create a new environment
azd env new prod --location eastus2

# Or select existing environment
azd env select prod
```

#### Step 3: Deploy Infrastructure and Application

```bash
# Deploy everything (infrastructure + code)
azd up

# Or deploy separately
azd provision  # Deploy infrastructure only
azd deploy     # Deploy application code only
```

### Step 4: Verify Deployment

```bash
# View all environment variables
azd env get-values

# Get the deployed URL
azd env get-value WEB_APP_URI
```

## Testing Azure Connectivity

### Run Integration Tests

The solution includes integration tests to verify Azure resource connectivity:

```bash
# Ensure Azurite is running first
azurite --silent --location c:\azurite --debug c:\azurite\debug.log

# Run integration tests
dotnet test PoConnectFive.Tests --filter "Category=Integration"

# Or run all tests
dotnet test
```

### Test Endpoints

After deployment, test these endpoints:

1. **Health Check**: `https://[your-app].azurewebsites.net/api/health`
2. **Diagnostics**: `https://[your-app].azurewebsites.net/diag`
3. **Swagger UI**: `https://[your-app].azurewebsites.net/swagger` (if Development environment)

## Resource Configuration

### Hard-coded Values (No User Input Required)

All infrastructure values are hard-coded in `infra/main.bicep`:

- **Resource Group**: PoConnectFive
- **Location**: Specified during `azd env new` (default: eastus2)
- **Shared Resource Group**: PoShared
- **Shared App Service Plan**: PoSharedAppServicePlan
- **Shared Application Insights**: PoSharedApplicationInsights
- **Shared Storage Account**: posharedtablestorage
- **App Service Name**: PoConnectFive
- **Managed Identity**: id-PoConnectFive

### Application Settings (Automatically Configured)

The following settings are automatically configured in Azure App Service:

```bash
APPLICATIONINSIGHTS_CONNECTION_STRING=<from-shared-resource>
ApplicationInsightsAgent_EXTENSION_VERSION=~3
AzureTableStorage__ConnectionString=<from-shared-storage-account>
```

## Monitoring and Diagnostics

### Application Insights

View telemetry, logs, and performance metrics in Azure Portal:

1. Navigate to PoShared resource group
2. Open PoSharedApplicationInsights
3. View Application Map, Performance, Failures, etc.

### Application Logs

View logs in Azure Portal:

1. Navigate to PoConnectFive resource group
2. Open PoConnectFive App Service
3. Go to Monitoring > Log stream

### Diagnostics Page

Access the built-in diagnostics page:

```
https://[your-app].azurewebsites.net/diag
```

This page checks:
- Azure Table Storage connectivity
- DNS resolution
- HTTP connectivity

## Updating the Application

### Update Code Only

```bash
# Deploy latest code without infrastructure changes
azd deploy
```

### Update Infrastructure

```bash
# Re-provision infrastructure with latest bicep changes
azd provision
```

### Full Update

```bash
# Update both infrastructure and code
azd up
```

## Cleanup

### Remove All Resources

```bash
# Delete the environment and all deployed resources
azd down

# Or manually delete the resource group
az group delete --name PoConnectFive --yes --no-wait
```

**Note**: This will only delete resources in the PoConnectFive resource group. Shared resources in PoShared will remain.

## Troubleshooting

### Deployment Fails

1. **Check Azure CLI login**: `azd auth login --check-status`
2. **Verify permissions**: Ensure you have Contributor role on subscription
3. **Check shared resources**: Verify PoShared resource group exists with required resources
4. **View detailed logs**: `azd up --debug`

### Application Not Starting

1. **Check Application Insights**: Look for startup errors
2. **View Log Stream**: In Azure Portal, App Service > Log stream
3. **Verify Configuration**: Ensure AzureTableStorage__ConnectionString is set
4. **Check Health Endpoint**: `curl https://[your-app].azurewebsites.net/api/health`

### Integration Tests Fail

1. **Ensure Azurite is running**: Check if localhost:10002 (Table Storage) is accessible
2. **Check connection string**: Verify `UseDevelopmentStorage=true` in appsettings.Development.json
3. **Run Azurite with verbose logging**: `azurite --debug c:\azurite\debug.log`

## Cost Optimization

All resources use the cheapest/free tiers:

- **App Service Plan**: Free tier (shared with other apps in PoShared)
- **Application Insights**: Pay-as-you-go (minimal cost for low traffic)
- **Storage Account**: Standard LRS (lowest cost tier)
- **Managed Identity**: Free

Estimated monthly cost: **$0-5** for low traffic scenarios.

## Security Best Practices

1. **Managed Identity**: Application uses managed identity instead of connection strings
2. **HTTPS Only**: All traffic is forced to HTTPS
3. **No Secrets in Code**: All sensitive values stored in Azure App Service Configuration
4. **RBAC**: Minimal permissions granted (Storage Table Data Contributor only)

## Additional Resources

- [Azure Developer CLI Documentation](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/)
- [Azure App Service Documentation](https://learn.microsoft.com/en-us/azure/app-service/)
- [Azure Table Storage Documentation](https://learn.microsoft.com/en-us/azure/storage/tables/)
- [Application Insights Documentation](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)

## Support

For issues or questions:
1. Check this documentation
2. Review Application Insights logs
3. Check the `/diag` endpoint for connectivity issues
4. Review Azure Portal resource health

---

**Last Updated**: Phase 2 Completion - January 2025
