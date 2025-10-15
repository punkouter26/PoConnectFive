# AI Coding Assistant Rules for PoConnectFive

This file provides guidance for AI coding assistants (like GitHub Copilot, Claude Code, etc.) when working with the PoConnectFive codebase.

## Project Overview

PoConnectFive is a Connect Five game built with:
- **Frontend**: Blazor WebAssembly (.NET 9.0)
- **Backend**: ASP.NET Core Web API (.NET 9.0)
- **Shared Logic**: .NET Standard library for game logic
- **Storage**: Azure Table Storage
- **Telemetry**: Application Insights
- **Testing**: xUnit with integration tests

## Architecture Principles

### SOLID Principles
All code should adhere to SOLID principles:
- **Single Responsibility**: Each class should have one reason to change
- **Open/Closed**: Open for extension, closed for modification
- **Liskov Substitution**: Subtypes must be substitutable for their base types
- **Interface Segregation**: Many specific interfaces over one general interface
- **Dependency Inversion**: Depend on abstractions, not concretions

### Design Patterns Used
- **Repository Pattern**: For data access (Table Storage)
- **Factory Pattern**: For creating game states
- **Strategy Pattern**: For AI player implementations
- **State Pattern**: For game flow management
- **Memento Pattern**: For board state serialization

## Code Style and Standards

### Naming Conventions
- **PascalCase**: Classes, methods, properties, public fields
- **camelCase**: Private fields, local variables, parameters
- **_camelCase**: Private instance fields (with underscore prefix)
- **UPPER_CASE**: Constants

### File Organization
```
PoConnectFive.Client/       # Blazor WebAssembly client
├── Pages/                  # Razor pages/components
├── wwwroot/               # Static assets
│   ├── css/              # Stylesheets
│   └── js/               # JavaScript interop
PoConnectFive.Server/      # ASP.NET Core API
├── Controllers/           # API controllers
├── Services/             # Business logic services
└── Program.cs            # App configuration
PoConnectFive.Shared/      # Shared library
├── Models/               # Domain models
└── Services/            # Shared services
PoConnectFive.Tests/       # Unit and integration tests
```

### Logging Standards
- Use **Serilog** for structured logging
- Log levels:
  - **Information**: Normal flow events (game started, move made)
  - **Warning**: Unexpected but recoverable events (validation failures)
  - **Error**: Error events that still allow the app to continue
  - **Critical**: Events requiring immediate attention
- Include context: player IDs, game IDs, IP addresses where relevant
- Never log sensitive data (passwords, tokens, etc.)

### Error Handling
- Use try-catch blocks for external dependencies (Azure, HTTP calls)
- Return meaningful error messages to users
- Log exceptions with full context
- Implement graceful degradation for non-critical failures

## Azure Resources

### Required Resources
All resources should be created in the same resource group with the same name as the solution:
- **Resource Group**: `PoConnectFive`
- **App Service**: `PoConnectFive` (using existing F1 plan in `PoShared`)
- **Application Insights**: `PoConnectFive`
- **Storage Account**: `PoConnectFive` (only if needed beyond Table Storage)

### Configuration
- **Local Development**: Use `appsettings.Development.json` with `UseDevelopmentStorage=true` for Azurite
- **Production**: Use App Service configuration settings (never commit secrets)
- **Location**: `eastus2` (when possible)

## Testing Guidelines

### Unit Tests
- Test all business logic thoroughly
- Use AAA pattern: Arrange, Act, Assert
- Name tests descriptively: `MethodName_Scenario_ExpectedResult`
- Mock external dependencies (storage, HTTP clients)

### Integration Tests
- Verify Azure resource connectivity
- Test complete user workflows
- Use `WebApplicationFactory` for API tests
- Clean up test data after execution

## API Guidelines

### RESTful Conventions
- **GET**: Retrieve resources (idempotent)
- **POST**: Create resources or trigger actions
- **PUT**: Update/replace resources (idempotent)
- **DELETE**: Remove resources (idempotent)

### Response Codes
- **200 OK**: Successful GET/PUT/DELETE
- **201 Created**: Successful POST creating a resource
- **400 Bad Request**: Invalid input
- **404 Not Found**: Resource doesn't exist
- **500 Internal Server Error**: Server-side error
- **503 Service Unavailable**: External dependency failure

### Health Endpoints
- **GET /api/health**: Overall system health
- **GET /api/health/storage**: Azure Storage health
- **GET /api/health/internet**: Network connectivity
- **POST /api/health/log**: Client-side diagnostics logging

## Blazor Component Guidelines

### Component Structure
```razor
@page "/route"
@using Statements
@inject Services

<PageTitle>PoConnectFive - Page Name</PageTitle>

<!-- HTML markup -->

@code {
    // Component code
}
```

### State Management
- Use component parameters for parent-child communication
- Keep state local when possible
- Use services for shared state across components
- Invoke `StateHasChanged()` when updating UI from async operations

## Security Best Practices

### CORS
- Not needed - Blazor WASM is hosted within the API project
- Client runs on same origin as API

### Authentication/Authorization
- Currently not implemented (future enhancement)
- When adding: use ASP.NET Core Identity or Azure AD B2C

### Input Validation
- Validate all user input on both client and server
- Sanitize data before storage
- Use Data Annotations for model validation

## Performance Guidelines

### Client-Side
- Minimize JavaScript interop calls
- Use virtualization for large lists
- Lazy load components when appropriate
- Compress static assets

### Server-Side
- Use async/await for I/O operations
- Cache frequently accessed data
- Optimize database queries
- Use response compression

## Deployment

### CI/CD via GitHub Actions
- Trigger on push to `master` branch
- Steps: Build → Test → Deploy using AZD
- Use federated credentials (no secrets in repo)
- Verify `/api/health` returns 200 after deployment

### App Service Configuration
- Use 32-bit worker process (F1 tier constraint)
- AlwaysOn disabled (F1 tier)
- Enable Swagger in all environments
- Set environment variables for connection strings

## Common Tasks

### Adding a New API Endpoint
1. Create controller method in `PoConnectFive.Server/Controllers`
2. Add service logic if needed
3. Update health checks if endpoint uses external dependencies
4. Add integration test
5. Update Swagger documentation

### Adding a New Page
1. Create `.razor` file in `PoConnectFive.Client/Pages`
2. Add `@page` directive with route
3. Inject required services
4. Add navigation link if needed
5. Add page title

### Adding a New Game Feature
1. Update domain models in `PoConnectFive.Shared/Models`
2. Add business logic in `PoConnectFive.Shared/Services`
3. Update UI components
4. Add unit tests for logic
5. Add integration tests for full workflow

## When Making Changes

### Before Committing
- ✅ Run `dotnet build` - ensure 0 errors and 0 warnings
- ✅ Run `dotnet test` - ensure all tests pass
- ✅ Run `dotnet format` - ensure consistent code style
- ✅ Update documentation if adding new features
- ✅ Review code for SOLID principles adherence

### Pull Request Checklist
- Descriptive title and description
- Link related issues
- Include test coverage for new code
- Update README/documentation if needed
- Ensure CI/CD pipeline passes

## Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Blazor Documentation](https://docs.microsoft.com/aspnet/core/blazor)
- [Azure App Service Documentation](https://docs.microsoft.com/azure/app-service)
- [Serilog Documentation](https://serilog.net)
- [Application Insights Documentation](https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview)

## Questions or Issues?

If you encounter issues or have questions about the codebase:
1. Check this AGENTS.md file
2. Review existing code for patterns
3. Check documentation in README.md or PRD.MD
4. Review commit history for context
5. Open a GitHub issue if needed
