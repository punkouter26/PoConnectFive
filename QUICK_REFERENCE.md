# PoConnectFive - Quick Reference Card

## ?? Quick Start

```bash
# Start the application
dotnet run --project PoConnectFive.Server

# Or press F5 in VS Code
```

Navigate to: https://localhost:5001

## ?? Key URLs

| URL | Description |
|-----|-------------|
| https://localhost:5001 | Main application |
| https://localhost:5001/diag | System diagnostics page |
| https://localhost:5001/api/health | Health check API endpoint |
| https://localhost:5001/swagger | Swagger API documentation (Dev only) |

## ?? Development Ports

- **HTTP:** http://localhost:5000
- **HTTPS:** https://localhost:5001

## ?? NuGet Packages Status

All packages are up-to-date as of Phase 1 completion:
- ? Microsoft.AspNetCore.Components.WebAssembly: 9.0.9
- ? Radzen.Blazor: 8.0.4
- ? Swashbuckle.AspNetCore: 9.0.6
- ? All .NET 9 packages: Latest stable

## ??? Project Structure

```
PoConnectFive/
??? PoConnectFive.Server/     # ASP.NET Core API + hosts WASM
?   ??? Controllers/           # API endpoints
?   ??? Services/              # Business logic
?   ??? Properties/            # Launch settings
??? PoConnectFive.Client/      # Blazor WebAssembly
?   ??? Pages/                 # Razor pages
?   ??? Services/              # Client services
?   ??? wwwroot/               # Static files
??? PoConnectFive.Shared/      # Shared models & services

```

## ?? Health Checks

The `/api/health` endpoint checks:
1. ? Azure Table Storage connectivity
2. ? DNS resolution (Internet connectivity)
3. ? HTTP connectivity (External API access)

## ?? Required Configuration

### Minimal Setup (Local Development)
```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "StorageConnectionString": "UseDevelopmentStorage=true"
  }
}
```

### Required Tools
- [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) - Azure Storage Emulator

## ?? Debugging

### VS Code (F5)
- Automatically builds and launches with debugger attached
- Opens browser to https://localhost:5001
- Breakpoints work in both Server and Client code

### Manual Debugging
```bash
# Build
dotnet build

# Run with hot reload
dotnet watch --project PoConnectFive.Server
```

## ?? Commands Cheat Sheet

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run server
dotnet run --project PoConnectFive.Server

# Watch mode (hot reload)
dotnet watch --project PoConnectFive.Server

# Format code
dotnet format

# Check for outdated packages
dotnet list package --outdated

# Update a package
dotnet add package <PackageName> --version <Version>

# Clean build artifacts
dotnet clean
```

## ?? Testing Endpoints

```bash
# Health check
curl http://localhost:5000/api/health

# Swagger UI
# Navigate to: https://localhost:5001/swagger
```

## ?? Documentation Files

- `LOCAL_DEVELOPMENT_CONFIG.md` - Complete setup guide
- `PHASE1_COMPLETION_SUMMARY.md` - Phase 1 task completion report
- `README.md` - Project overview (if exists)

## ?? Target Framework

All projects: **.NET 9.0** (`net9.0`)

## ?? Architecture

- **Hosting Model:** Blazor WebAssembly hosted in ASP.NET Core
- **CORS:** Not needed (same-origin)
- **Authentication:** Not configured (future enhancement)

## ?? Notes

- Client and Server are served from the same origin
- No CORS configuration needed
- Azure Storage Emulator (Azurite) required for local development
- Application Insights is optional for local development

---

**Last Updated:** Phase 1 Completion - January 2025
**Project Status:** ? Ready for Development
