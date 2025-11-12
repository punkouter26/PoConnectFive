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

            // Act - PUT the player stat update (RESTful)
            var postResponse = await client.PutAsJsonAsync($"/api/leaderboard/players/{sampleDto.PlayerName}/stats", sampleDto);

            // Assert PUT succeeded (NoContent)
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

        [Fact]
        public async Task UpdatePlayerStats_InvalidDifficulty_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            var invalidDto = new
            {
                PlayerName = "TestPlayer",
                Difficulty = 999, // Invalid difficulty
                Result = 0,
                GameTimeMilliseconds = 1000
            };

            var response = await client.PostAsJsonAsync("/api/leaderboard/playerstats", invalidDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdatePlayerStats_NullPlayerName_ReturnsBadRequest()
        {
            var mockStorage = new Mock<ITableStorageService>();

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITableStorageService));
                    if (descriptor != null) services.Remove(descriptor);
                    services.AddSingleton<ITableStorageService>(mockStorage.Object);
                });
            }).CreateClient();

            var invalidDto = new PlayerStatUpdateDto
            {
                PlayerName = null!, // Null player name
                Difficulty = AIDifficulty.Easy,
                Result = PlayerGameResult.Win,
                GameTimeMilliseconds = 1000
            };

            var response = await client.PostAsJsonAsync("/api/leaderboard/playerstats", invalidDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdatePlayerStats_EmptyPlayerName_ReturnsBadRequest()
        {
            var mockStorage = new Mock<ITableStorageService>();

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITableStorageService));
                    if (descriptor != null) services.Remove(descriptor);
                    services.AddSingleton<ITableStorageService>(mockStorage.Object);
                });
            }).CreateClient();

            var invalidDto = new PlayerStatUpdateDto
            {
                PlayerName = "", // Empty player name
                Difficulty = AIDifficulty.Medium,
                Result = PlayerGameResult.Win,
                GameTimeMilliseconds = 1000
            };

            var response = await client.PutAsJsonAsync("/api/leaderboard/players/ /stats", invalidDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetTopPlayers_ValidDifficulty_ReturnsOk()
        {
            var mockStorage = new Mock<ITableStorageService>();

            mockStorage
                .Setup(s => s.GetTopPlayersByDifficultyAsync(AIDifficulty.Medium, 5))
                .ReturnsAsync(new List<PlayerStatEntity>());

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITableStorageService));
                    if (descriptor != null) services.Remove(descriptor);
                    services.AddSingleton<ITableStorageService>(mockStorage.Object);
                });
            }).CreateClient();

            var response = await client.GetAsync($"/api/leaderboard/{AIDifficulty.Medium}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UpdatePlayerStats_NegativeGameTime_ReturnsBadRequest()
        {
            var mockStorage = new Mock<ITableStorageService>();

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITableStorageService));
                    if (descriptor != null) services.Remove(descriptor);
                    services.AddSingleton<ITableStorageService>(mockStorage.Object);
                });
            }).CreateClient();

            var invalidDto = new PlayerStatUpdateDto
            {
                PlayerName = "TestNegativeTime",
                Difficulty = AIDifficulty.Easy,
                Result = PlayerGameResult.Win,
                GameTimeMilliseconds = -1000 // Negative time
            };

            var response = await client.PutAsJsonAsync("/api/leaderboard/players/TestNegativeTime/stats", invalidDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Real integration tests (without mocks) for GET endpoint
        [Fact]
        public async Task GetTopPlayers_RealStorage_ReturnsTop5OrderedByWinRate()
        {
            // Arrange - Use real storage (Azurite)
            var client = _factory.CreateClient();

            // Use unique player names with timestamp to avoid conflicts with previous test data
            var timestamp = DateTime.UtcNow.Ticks.ToString();
            await SeedTestPlayer(client, $"TopPlayer_{timestamp}", AIDifficulty.Easy, wins: 10, losses: 0);
            await SeedTestPlayer(client, $"GoodPlayer_{timestamp}", AIDifficulty.Easy, wins: 7, losses: 3);
            await SeedTestPlayer(client, $"AveragePlayer_{timestamp}", AIDifficulty.Easy, wins: 5, losses: 5);
            await SeedTestPlayer(client, $"PoorPlayer_{timestamp}", AIDifficulty.Easy, wins: 2, losses: 8);
            await SeedTestPlayer(client, $"WorstPlayer_{timestamp}", AIDifficulty.Easy, wins: 0, losses: 10);
            await SeedTestPlayer(client, $"ExtraPlayer1_{timestamp}", AIDifficulty.Easy, wins: 6, losses: 4);
            await SeedTestPlayer(client, $"ExtraPlayer2_{timestamp}", AIDifficulty.Easy, wins: 4, losses: 6);

            // Act
            var response = await client.GetAsync("/api/leaderboard/Easy");

            // Assert
            response.EnsureSuccessStatusCode();
            var players = await response.Content.ReadFromJsonAsync<List<PlayerStatEntity>>();

            Assert.NotNull(players);
            Assert.True(players.Count <= 5); // Should return at most 5
            // Verify ordering: each player should have win rate >= next player
            for (int i = 0; i < players.Count - 1; i++)
            {
                Assert.True(players[i].WinRate >= players[i + 1].WinRate,
                    $"Players not ordered by win rate: {players[i].PlayerName} ({players[i].WinRate}) should be >= {players[i + 1].PlayerName} ({players[i + 1].WinRate})");
            }
            Assert.All(players, p => Assert.Equal(AIDifficulty.Easy.ToString(), p.PartitionKey));
        }

        [Theory]
        [InlineData(AIDifficulty.Easy)]
        [InlineData(AIDifficulty.Medium)]
        [InlineData(AIDifficulty.Hard)]
        public async Task GetTopPlayers_RealStorage_AllDifficulties_ReturnsCorrectData(AIDifficulty difficulty)
        {
            // Arrange
            var client = _factory.CreateClient();
            await SeedTestPlayer(client, $"Player1_{difficulty}", difficulty, wins: 5, losses: 5);
            await SeedTestPlayer(client, $"Player2_{difficulty}", difficulty, wins: 3, losses: 7);

            // Act
            var response = await client.GetAsync($"/api/leaderboard/{difficulty}");

            // Assert
            response.EnsureSuccessStatusCode();
            var players = await response.Content.ReadFromJsonAsync<List<PlayerStatEntity>>();

            Assert.NotNull(players);
            Assert.NotEmpty(players);
            Assert.All(players, p => Assert.Equal(difficulty.ToString(), p.PartitionKey));
        }

        [Fact]
        public async Task GetTopPlayers_RealStorage_DifferentDifficulties_DoesNotMix()
        {
            // Arrange
            var client = _factory.CreateClient();
            await SeedTestPlayer(client, "UniqueEasyPlayer", AIDifficulty.Easy, wins: 10, losses: 0);
            await SeedTestPlayer(client, "UniqueMediumPlayer", AIDifficulty.Medium, wins: 10, losses: 0);
            await SeedTestPlayer(client, "UniqueHardPlayer", AIDifficulty.Hard, wins: 10, losses: 0);

            // Act
            var easyResponse = await client.GetAsync("/api/leaderboard/Easy");
            var mediumResponse = await client.GetAsync("/api/leaderboard/Medium");
            var hardResponse = await client.GetAsync("/api/leaderboard/Hard");

            // Assert
            var easyPlayers = await easyResponse.Content.ReadFromJsonAsync<List<PlayerStatEntity>>();
            var mediumPlayers = await mediumResponse.Content.ReadFromJsonAsync<List<PlayerStatEntity>>();
            var hardPlayers = await hardResponse.Content.ReadFromJsonAsync<List<PlayerStatEntity>>();

            Assert.NotNull(easyPlayers);
            Assert.NotNull(mediumPlayers);
            Assert.NotNull(hardPlayers);

            // Verify each difficulty has its own player
            Assert.Contains(easyPlayers, p => p.PlayerName == "UniqueEasyPlayer");
            Assert.Contains(mediumPlayers, p => p.PlayerName == "UniqueMediumPlayer");
            Assert.Contains(hardPlayers, p => p.PlayerName == "UniqueHardPlayer");

            // Verify no cross-contamination
            Assert.DoesNotContain(easyPlayers, p => p.PlayerName == "UniqueMediumPlayer");
            Assert.DoesNotContain(mediumPlayers, p => p.PlayerName == "UniqueHardPlayer");
        }

        // Helper method to seed test players using real API
        private async Task SeedTestPlayer(HttpClient client, string playerName, AIDifficulty difficulty, int wins, int losses)
        {
            for (int i = 0; i < wins; i++)
            {
                var winUpdate = new PlayerStatUpdateDto
                {
                    PlayerName = playerName,
                    Difficulty = difficulty,
                    Result = PlayerGameResult.Win,
                    GameTimeMilliseconds = 60000
                };
                await client.PutAsJsonAsync($"/api/leaderboard/players/{playerName}/stats", winUpdate);
            }

            for (int i = 0; i < losses; i++)
            {
                var lossUpdate = new PlayerStatUpdateDto
                {
                    PlayerName = playerName,
                    Difficulty = difficulty,
                    Result = PlayerGameResult.Loss,
                    GameTimeMilliseconds = 45000
                };
                await client.PutAsJsonAsync($"/api/leaderboard/players/{playerName}/stats", lossUpdate);
            }
        }
    }
}
