graph TB
    subgraph "Azure Cloud"
        subgraph "Resource Group: PoConnectFive-RG"
            AppServicePlan[PoConnectFive-AppServicePlan<br/>SKU: Standard B1]
            AppService[PoConnectFive-AppService<br/>Web App for Containers]
            StorageAccount[poconnectfivestorage<br/>Azure Storage Account]
            TableStorage[(PoConnectFivePlayerStats<br/>Azure Table Storage)]
            AppInsights[PoConnectFive-AppInsights<br/>Application Insights]
        end
    end

    subgraph "CI/CD Pipeline (e.g., GitHub Actions)"
        Build[Build: dotnet build / dotnet publish]
        Test[Unit & Integration Tests]
        Package[Package Artifacts]
        Deploy[Deploy to Azure App Service]
    end

    subgraph "Local Development"
        DevMachine[Developer Machine<br/>VS Code / .NET CLI]
        AzuriteLocal[Azurite Emulator<br/>Local Storage]
    end

    DevMachine -- "Git Push" --> Build
    Build -- "Artifacts" --> Test
    Test -- "Pass" --> Package
    Package -- "Web Deploy / Zip Deploy" --> Deploy
    Deploy -- "Deploys to" --> AppService

    AppService -- "Hosts" --> API[PoConnectFive.Server.dll]
    API -- "Connection String" --> StorageAccount
    StorageAccount -- "Contains" --> TableStorage
    API -- "Telemetry" --> AppInsights
    AppService -- "Runs on" --> AppServicePlan

    DevMachine -- "Local Debugging" --> API_Local[Local API Instance]
    API_Local -- "UseDevelopmentStorage=true" --> AzuriteLocal

    style AppService fill:#0078d4,stroke:#333,stroke-width:2px,color:#fff
    style StorageAccount fill:#00bcf2,stroke:#333,stroke-width:2px,color:#fff
    style TableStorage fill:#00bcf2,stroke:#333,stroke-width:2px,color:#fff
    style AppInsights fill:#0078d4,stroke:#333,stroke-width:2px,color:#fff
    style DevMachine fill:#f25022,stroke:#333,stroke-width:2px,color:#fff
    style AzuriteLocal fill:#7fba00,stroke:#333,stroke-width:2px,color:#fff
