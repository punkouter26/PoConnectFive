# Local Development Configuration Guide

## Required Configuration Keys

### 1. Azure Table Storage (appsettings.Development.json)

For local development, use Azure Storage Emulator:

```json
{
  "ConnectionStrings": {
    "StorageConnectionString": "UseDevelopmentStorage=true"
  },
  "AzureTableStorage": {
    "ConnectionString": "UseDevelopmentStorage=true"
  }
}
```

**Setup Instructions:**
- Install [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) for local Azure Storage emulation
- Run: `npm install -g azurite` (or use VS Code Azurite extension)
- Start: `azurite --silent --location c:\azurite --debug c:\azurite\debug.log`

### 2. Application Insights (appsettings.Development.json)

For local development, Application Insights telemetry is optional:

```json
{
  "APPLICATIONINSIGHTS_CONNECTION_STRING": "InstrumentationKey=your-key-here;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus.livediagnostics.monitor.azure.com/;ApplicationId=your-app-id"
}
```

**Note:** If you don't have Application Insights, telemetry will simply be disabled locally.

### 3. OpenAI Configuration (appsettings.json - Production Only)

OpenAI is configured in production but not required for local development:

```json
{
  "OpenAI": {
    "Endpoint": "",
    "ApiKey": ""
  }
}
```

## Port Configuration

The application is configured to run on:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001

This is configured in `PoConnectFive.Server/Properties/launchSettings.json`

## CORS Configuration

CORS is **NOT CONFIGURED** because:
- The Blazor WebAssembly client is hosted inside the ASP.NET Core Server project
- Both the API and client are served from the same origin (localhost:5000/5001)
- No cross-origin requests occur in this architecture

## Debugging

### F5 Debugging in VS Code
- Press F5 to start debugging
- The `.vscode/launch.json` is configured to:
  - Build the project
  - Launch the Server project
  - Attach the debugger
  - Open browser to https://localhost:5001

### Available Diagnostic Endpoints

- `/diag` - Client-side diagnostics page (checks all API connections)
- `/api/health` - Comprehensive health check endpoint (checks all external dependencies)
- `/swagger` - Swagger UI for API documentation (Development only)

## User Secrets Configuration

The project uses User Secrets (ID: `772a1227-9c5e-47fe-acd1-7c24839dd8b5`) for sensitive data.

To set user secrets:
```bash
cd PoConnectFive.Server
dotnet user-secrets set "ConnectionStrings:StorageConnectionString" "UseDevelopmentStorage=true"
dotnet user-secrets set "APPLICATIONINSIGHTS_CONNECTION_STRING" "your-connection-string"
```

## Build and Run

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run the server (which hosts the Blazor WASM client)
dotnet run --project PoConnectFive.Server
```

Then navigate to https://localhost:5001

## Health Check Testing

Test the health endpoint:
```bash
curl http://localhost:5000/api/health
```

Expected response:
```json
{
  "status": "Healthy",
  "timestamp": "2025-01-12T...",
  "checks": [
    {
      "component": "Azure Table Storage",
      "isHealthy": true,
      "error": null,
      "responseTime": 0
    },
    {
      "component": "DNS Resolution",
      "isHealthy": true,
      "error": null,
      "responseTime": 0
    },
    {
      "component": "HTTP Connectivity",
      "isHealthy": true,
      "error": null,
      "responseTime": 0
    }
  ]
}
```
