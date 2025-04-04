using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PoConnectFive.Client;
using PoConnectFive.Client.Services;
using PoConnectFive.Shared.Services;
using PoConnectFive.Shared.Interfaces;
using PoConnectFive.Shared.Services.AI;
using PoConnectFive.Shared.Models;
using Radzen; // Add Radzen namespace

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient for backend API communication
// Use the HTTPS address from launchSettings.json
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7033") }); // Use HTTPS

// Register services
// builder.Services.AddScoped<ILocalStorageService, BrowserStorageService>(); // Removed
// builder.Services.AddScoped<IPlayerDataService, PlayerDataService>(); // Removed
builder.Services.AddScoped<IPlayerDataService, ApiPlayerDataService>(); // Use new API service
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>(); // Keep this if still used elsewhere, otherwise remove

// Register game service
builder.Services.AddScoped<IGameService>(sp => new GameService()); 

// Register AI players
builder.Services.AddScoped<IAIPlayer>(sp =>
{
    // Default to medium difficulty, but this will be overridden when starting a new game
    return AIPlayerFactory.CreateAIPlayer(AIDifficulty.Medium);
});

// Add Radzen component services
builder.Services.AddRadzenComponents();

await builder.Build().RunAsync();
