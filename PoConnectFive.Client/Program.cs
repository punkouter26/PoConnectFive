using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PoConnectFive.Client;
using PoConnectFive.Client.Services;
using PoConnectFive.Shared.Services;
using PoConnectFive.Shared.Interfaces;
using PoConnectFive.Shared.Services.AI;
using PoConnectFive.Shared.Models;
using Radzen; // Add Radzen namespace
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddSerilog(Log.Logger);
    logging.SetMinimumLevel(LogLevel.Debug);
});

// Read configuration from wwwroot/appsettings.json
var apiUrl = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrEmpty(apiUrl))
{
    // Fallback or throw error if not configured
    apiUrl = builder.HostEnvironment.BaseAddress; // Default to base address if not found
    Console.WriteLine("Warning: ApiBaseUrl not found in configuration. Falling back to host base address.");
}

// Configure HttpClient for backend API communication
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) }); 

// Register services
builder.Services.AddScoped<ILocalStorageService, BrowserStorageService>();
builder.Services.AddScoped<IStorageService>(sp => sp.GetRequiredService<ILocalStorageService>() as IStorageService);
builder.Services.AddScoped<IPlayerDataService, ApiPlayerDataService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();

// Register game services
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<GameStateService>();
builder.Services.AddScoped<WinProbabilityService>();
builder.Services.AddScoped<SoundService>();
builder.Services.AddScoped<GameStatisticsService>();

// Register AI players
builder.Services.AddScoped<IAIPlayer>(sp =>
{
    // Default to medium difficulty, but this will be overridden when starting a new game
    return AIPlayerFactory.CreateAIPlayer(AIDifficulty.Medium);
});

// Add Radzen component services
builder.Services.AddRadzenComponents();

await builder.Build().RunAsync();
