using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PoConnectFive.Server;
using PoConnectFive.Server.Services;
using PoConnectFive.Shared.Models;
using Xunit;

namespace PoConnectFive.Tests
{
    public class LeaderboardIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public LeaderboardIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task PostPlayerStats_ThenGetLeaderboard_ReturnsPersistedEntry()
        {
            // Arrange
            var mockStorage = new Mock<ITableStorageService>();

            var sampleDto = new PlayerStatUpdateDto
            {
                PlayerName = "TestPlayer",
                Difficulty = AIDifficulty.Easy,
                Result = PlayerGameResult.Win,
                GameTimeMilliseconds = 1500
            };

            // Track that UpsertPlayerStatAsync is called with expected parameters
            mockStorage
                .Setup(s => s.UpsertPlayerStatAsync(It.IsAny<string>(), It.IsAny<AIDifficulty>(), It.IsAny<PlayerGameResult>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Prepare a leaderboard result to be returned by GetTopPlayersByDifficultyAsync
            var returnedList = new List<PlayerStatEntity>
            {
                new PlayerStatEntity("TestPlayer", AIDifficulty.Easy)
                {
                    Wins = 1,
                    Losses = 0,
                    WinRate = 1.0
                }
            };

            mockStorage
                .Setup(s => s.GetTopPlayersByDifficultyAsync(AIDifficulty.Easy, 5))
                .ReturnsAsync(returnedList);

            // Create a test server that replaces ITableStorageService with our mock
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing ITableStorageService registrations if any
                    var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITableStorageService));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddSingleton<ITableStorageService>(mockStorage.Object);
                });
            }).CreateClient();

            // Act - POST the player stat update
            var postResponse = await client.PostAsJsonAsync("/api/leaderboard/playerstats", sampleDto);

            // Assert POST succeeded (NoContent)
            Assert.Equal(HttpStatusCode.NoContent, postResponse.StatusCode);
            mockStorage.Verify(s => s.UpsertPlayerStatAsync(
                It.Is<string>(n => n == sampleDto.PlayerName),
                It.Is<AIDifficulty>(d => d == sampleDto.Difficulty),
                It.Is<PlayerGameResult>(r => r == sampleDto.Result),
                It.IsAny<TimeSpan>()), Times.Once);

            // Act - GET the leaderboard for Easy difficulty
            var getResponse = await client.GetAsync($"/api/leaderboard/{AIDifficulty.Easy}");

            // Assert GET succeeded and contains our mocked data
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var leaderboard = await getResponse.Content.ReadFromJsonAsync<List<PlayerStatEntity>>();
            Assert.NotNull(leaderboard);
            Assert.Contains(leaderboard!, e => e.RowKey == "testplayer" || e.PartitionKey == AIDifficulty.Easy.ToString());
        }
    }
}
