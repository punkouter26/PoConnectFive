# PoConnectFive

A web-based implementation of the classic Connect Five game, built with .NET 9 and Blazor WebAssembly.

## Table of Contents

- [Description](#description)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-start)
  - [Local Development](#local-development)
  - [Running the Application](#running-the-application)
- [Application Structure](#application-structure)
- [API and Database Connections](#api-and-database-connections)
- [Diagnostics](#diagnostics)
- [Contributing](#contributing)

## Description

PoConnectFive is a modern, responsive web application that allows users to play the Connect Five game. It supports both player-versus-player and player-versus-AI game modes, with multiple difficulty levels for the AI. The application tracks player statistics, maintains a leaderboard, and provides a comprehensive diagnostics page to monitor the health of its dependencies.

## Features

- **Interactive Gameplay**: A dynamic and visually appealing game board.
- **Multiple Game Modes**: Challenge a friend or test your skills against an AI opponent (Easy, Medium, Hard).
- **Player Statistics**: Track your wins, losses, and draws.
- **Leaderboard**: See how you rank against other players.
- **Sound Effects**: Enjoy audio feedback for in-game actions.
- **Settings**: Customize your experience with AI difficulty, sound toggles, and player names.
- **Diagnostics**: Monitor the status of API, storage, and internet connectivity in real-time.

## Technology Stack

- **Frontend**: Blazor WebAssembly, .NET 9
- **Backend**: ASP.NET Core Web API, .NET 9
- **Data Storage**: Azure Table Storage (using Azurite for local development)
- **UI Components**: Radzen.Blazor
- **Logging**: Serilog
- **API Documentation**: Swagger/OpenAPI (Swashbuckle.AspNetCore)

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio Code](https://code.visualstudio.com/) (or another preferred IDE)
- [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio-code) for local Azure Storage emulation (optional, as the project is configured to use it by default)

## Getting Started

### Local Development

1.  **Clone the repository**
    ```bash
    git clone https://github.com/punkouter25/PoConnectFive.git
    cd PoConnectFive
    ```

2.  **Restore NuGet packages**
    The `dotnet run` command will automatically restore packages, but you can do it explicitly:
    ```bash
    dotnet restore
    ```

3.  **Ensure Azurite is running (for local storage)**
    The application is configured to use `UseDevelopmentStorage=true` in `PoConnectFive.Server/appsettings.Development.json`, which connects to the Azurite emulator.
    - If you have Azurite installed globally, it might start automatically.
    - You can also start it manually via VS Code (Azure Storage extension) or by running `azurite` in your terminal if it's in your PATH.

### Running the Application

You can run the application from the root directory of the solution.

-   **Using VS Code:**
    1.  Open the `PoConnectFive` folder in VS Code.
    2.  Press `F5` to start debugging. This will build the solution, launch the `PoConnectFive.Server` project, and attach the debugger.
    3.  Your default browser should open to `https://localhost:5001`.

-   **Using the Command Line:**
    Navigate to the root directory of the solution (`PoConnectFive`) and run:
    ```bash
    dotnet run --project PoConnectFive.Server/PoConnectFive.Server.csproj
    ```
    This command will:
    - Build the `PoConnectFive.Server` and its dependencies.
    - Host the Blazor WebAssembly client.
    - Start the API server, typically listening on `http://localhost:5000` and `https://localhost:5001`.

    Open your browser and navigate to `https://localhost:5001`.

## Application Structure

The solution is structured into three main projects:

-   **`PoConnectFive.Client`**: The Blazor WebAssembly frontend project. Contains all the UI components (`.razor` files), client-side services, and static web assets.
-   **`PoConnectFive.Server`**: The ASP.NET Core Web API backend project. Contains API controllers, data access services, and server-side configuration.
-   **`PoConnectFive.Shared`**: A class library for shared models, interfaces, and services used by both the client and server projects.

## API and Database Connections

### API

The backend API is hosted by the `PoConnectFive.Server` project. Key API endpoints include:

-   **Player Data**: `/api/player` (for statistics and leaderboard updates).
-   **Leaderboard**: `/api/leaderboard` (for fetching top players).
-   **Health Checks**: `/healthz` (for application, storage, and internet connectivity status).

The API is documented using Swagger/OpenAPI. When the server is running, you can access the Swagger UI at `https://localhost:5001/swagger`.

### Database (Azure Table Storage)

-   **Local Development**: The application uses the **Azurite** emulator for local development. The connection string is configured in `PoConnectFive.Server/appsettings.Development.json` as `UseDevelopmentStorage=true`. This allows you to develop and test without needing a live Azure Storage account.
-   **Production/Azure Deployment**: For deployment to Azure, the `StorageConnectionString` in `appsettings.json` (or more securely, in Azure App Service Configuration) should be updated to point to your Azure Table Storage account. The connection string should be kept secret and not committed to version control.

**Data Access**:
-   Interaction with Azure Table Storage is abstracted through the `ITableStorageService` interface, implemented by `TableStorageService` in the `PoConnectFive.Server/Services` directory.
-   Data entities, such as `PlayerStatEntity`, are stored in tables named following the convention `PoConnectFive[TableName]` (e.g., `PoConnectFivePlayerStats`).

## Diagnostics

The application includes a diagnostics page accessible at `/diag`. This page checks the health of critical dependencies:

1.  **API Health**: Checks if the main API endpoint is responding.
2.  **Storage Health**: Verifies the connection to Azure Table Storage (or Azurite locally).
3.  **Internet Connectivity**: Confirms that the server can resolve DNS and make HTTP requests to an external site (e.g., google.com).

The results are displayed in a user-friendly table, and any errors are logged to the server for further analysis. This page is crucial for troubleshooting issues related to external services.

## Code Quality

The project enforces code quality standards through static code analysis tools configured at the solution level.

### Static Analysis Tools

- **StyleCop.Analyzers**: Enforces C# coding style conventions
  - Configuration: `stylecop.json` and `.globalconfig` at repository root
  - Provides consistent formatting, naming conventions, and code structure
  
- **SonarAnalyzer.CSharp**: Detects code smells, bugs, and security vulnerabilities
  - Identifies unused variables, complexity issues, and potential bugs
  - Suggests improvements for maintainability and reliability
  
- **EditorConfig**: Defines core coding style rules
  - Configuration: `.editorconfig` at repository root
  - Ensures consistent formatting across different editors and IDEs

### Configuration Files

- **`.editorconfig`**: Core editor settings (indentation, line endings, naming conventions)
- **`.globalconfig`**: Global analyzer rule configuration with custom severity levels
- **`stylecop.json`**: StyleCop-specific settings and documentation rules
- **`Directory.Build.props`**: MSBuild properties applied to all projects
  - Enables nullable reference types
  - Enforces code style in build process
  - Links analyzer configuration files

### Build Warnings

The solution is configured to show analyzer warnings during build. To treat warnings as errors in Release builds, set the `TreatWarningsAsErrors` property to `true` in `Directory.Build.props`.

To see all analyzer warnings:
```bash
dotnet build
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

Please ensure to update tests as appropriate and adhere to the existing coding style.
