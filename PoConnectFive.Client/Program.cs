using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PoConnectFive.Client;
using PoConnectFive.Client.Services;
using PoConnectFive.Shared.Services;
using PoConnectFive.Shared.Interfaces;
using PoConnectFive.Shared.Services.AI;
using PoConnectFive.Shared.Models;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register services
builder.Services.AddScoped<ILocalStorageService, BrowserStorageService>();
builder.Services.AddScoped<IPlayerDataService, PlayerDataService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();

// Register game service
builder.Services.AddScoped<IGameService>(sp => new GameService());

// Register AI players
builder.Services.AddScoped<IAIPlayer>(sp =>
{
    // Default to medium difficulty, but this will be overridden when starting a new game
    return AIPlayerFactory.CreateAIPlayer(AIDifficulty.Medium);
});

await builder.Build().RunAsync();
