// PoConnectFive.AppHost - Aspire Orchestration Entry Point
// This project orchestrates all services in the PoConnectFive solution

var builder = DistributedApplication.CreateBuilder(args);

// Reference existing shared Application Insights from PoShared resource group
// The connection string should come from Azure Key Vault or environment variables
var appInsightsConnectionString = builder.AddParameter("AppInsightsConnectionString", secret: true);

// Add Azure Storage using Azurite emulator in Docker for local development
// In production, this will use Azure Table Storage
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(emulator => emulator
        .WithDataVolume("azurite-data")
        .WithTablePort(10002)
        .WithBlobPort(10000)
        .WithQueuePort(10001));

var tableStorage = storage.AddTables("tableStorage");

// Add the Blazor Web App (server project hosts the client)
// The web project is the unified Blazor Web App with Interactive WebAssembly rendering
builder.AddProject<Projects.Po_ConnectFive_Api>("web")
    .WithExternalHttpEndpoints()
    .WithReference(tableStorage)
    .WithEnvironment("APPLICATIONINSIGHTS_CONNECTION_STRING", appInsightsConnectionString)
    .WithHttpHealthCheck("/api/health");

builder.Build().Run();
