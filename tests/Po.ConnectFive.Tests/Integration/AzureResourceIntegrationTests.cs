using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;
using PoConnectFive.Server.Services;
using System.Threading.Tasks;
using System;

namespace PoConnectFive.Tests.Integration
{
    /// <summary>
    /// Integration tests to verify Azure resource connectivity
    /// These tests require actual Azure resources or Azurite emulator to be running
    /// </summary>
    public class AzureResourceIntegrationTests
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TableStorageService> _logger;
        private readonly string? _aiConnectionString;

        public AzureResourceIntegrationTests()
        {
            // Build configuration from appsettings
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.Development.json", optional: false)
                .Build();

            _aiConnectionString = _configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
                ?? _configuration.GetConnectionString("APPLICATIONINSIGHTS_CONNECTION_STRING");

            // Create logger
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });
            _logger = loggerFactory.CreateLogger<TableStorageService>();
        }

        [Fact]
        public void ApplicationInsights_ConnectionString_ShouldBeConfigured()
        {
            Assert.False(string.IsNullOrWhiteSpace(_aiConnectionString), "APPLICATIONINSIGHTS_CONNECTION_STRING is not configured in tests configuration.");
        }

        [Fact]
        public async Task TableStorage_Connection_ShouldSucceed()
        {
            // Arrange
            var service = new TableStorageService(_configuration, _logger);

            // Act
            var result = await service.CheckConnection();

            // Assert
            Assert.True(result.IsSuccess, $"Table Storage connection failed: {result.Error}");
        }

        [Fact]
        public async Task TableStorage_UpsertPlayerStat_ShouldSucceed()
        {
            // Arrange
            var service = new TableStorageService(_configuration, _logger);
            var playerName = $"TestPlayer_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var difficulty = PoConnectFive.Shared.Models.AIDifficulty.Medium;
            var gameResult = PoConnectFive.Shared.Models.PlayerGameResult.Win;
            var gameTime = TimeSpan.FromMinutes(5);

            // Act & Assert - Should not throw
            await service.UpsertPlayerStatAsync(playerName, difficulty, gameResult, gameTime);

            // Verify we can retrieve the player
            var topPlayers = await service.GetTopPlayersByDifficultyAsync(difficulty, 10);
            Assert.Contains(topPlayers, p => p.PlayerName == playerName);
        }

        [Fact]
        public async Task TableStorage_GetTopPlayers_ShouldReturnResults()
        {
            // Arrange
            var service = new TableStorageService(_configuration, _logger);
            var difficulty = PoConnectFive.Shared.Models.AIDifficulty.Easy;

            // Act
            var topPlayers = await service.GetTopPlayersByDifficultyAsync(difficulty, 5);

            // Assert
            Assert.NotNull(topPlayers);
            // Note: May be empty if no players exist yet, but should not throw
        }
    }
}
