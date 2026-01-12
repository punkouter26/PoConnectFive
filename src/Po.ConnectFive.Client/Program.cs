using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using PoConnectFive.Client;
using PoConnectFive.Client.Services;
using PoConnectFive.Shared.Interfaces;
using PoConnectFive.Shared.Models;
using PoConnectFive.Shared.Services;
using PoConnectFive.Shared.Services.AI;
using Radzen;
using Serilog;
using Serilog.Extensions.Logging;

// Entry point for Blazor WebAssembly runtime initialization
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register all client services inline (extension method not visible to top-level statements in same file)
ClientServiceRegistration.ConfigureServices(builder.Services, builder.HostEnvironment.BaseAddress);

await builder.Build().RunAsync();

namespace PoConnectFive.Client
{
    /// <summary>
    /// Provides methods for registering client-side services in the DI container.
    /// Used by both the server (for SSR) and the WebAssembly client (for interactive rendering).
    /// </summary>
    public static class ClientServiceRegistration
    {
        /// <summary>
        /// Registers all client-side services required for Blazor WebAssembly interactive rendering.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="baseAddress">The base address for HTTP client configuration.</param>
        /// <returns>The service collection for method chaining.</returns>
        public static IServiceCollection AddClientServices(this IServiceCollection services, string baseAddress)
        {
            return ConfigureServices(services, baseAddress);
        }

        /// <summary>
        /// Internal method to configure services - callable from top-level statements.
        /// </summary>
        public static IServiceCollection ConfigureServices(IServiceCollection services, string baseAddress)
        {
            // Configure Serilog - Information level to reduce console spam
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            // Add logging
            services.AddLogging(logging =>
            {
                logging.AddSerilog(Log.Logger);
                logging.SetMinimumLevel(LogLevel.Information);
            });

            // Configure HttpClient for backend API communication
            services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });

            // Register services
            services.AddScoped<ILocalStorageService, BrowserStorageService>();
            services.AddScoped<IStorageService>(sp => sp.GetRequiredService<ILocalStorageService>() as IStorageService ?? throw new InvalidOperationException("ILocalStorageService could not be cast to IStorageService"));
            services.AddScoped<IPlayerDataService, ApiPlayerDataService>();
            services.AddScoped<ILeaderboardService, LeaderboardService>();
            services.AddScoped<ErrorHandlingService>();

            // Register game services
            services.AddScoped<IGameService, GameService>();
            services.AddScoped<GameStateService>();
            services.AddScoped<GamePageStateManager>();
            services.AddScoped<WinProbabilityService>();
            services.AddScoped<SoundService>();
            services.AddScoped<GameStatisticsService>();
            services.AddScoped<GameAnalyticsService>();

            // Register theme service
            services.AddScoped<PoConnectFive.Client.Services.ThemeService>();

            // Register AI players
            services.AddScoped<IAIPlayer>(sp =>
            {
                // Default to medium difficulty, but this will be overridden when starting a new game
                return AIPlayerFactory.CreateAIPlayer(AIDifficulty.Medium);
            });

            // Add Radzen component services
            services.AddRadzenComponents();

            // Add FluentUI component services
            services.AddFluentUIComponents();

            return services;
        }
    }
}