using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PoConnectFive.Shared.Models; // Now contains PlayerStatEntity, PlayerStatUpdateDto, etc.
using PoConnectFive.Shared.Interfaces; // Added reference to the interface in Shared project
// using PoConnectFive.Server.Controllers; // Removed
// using PoConnectFive.Server.Models; // Removed

namespace PoConnectFive.Client.Services
{
    /// <summary>
    /// Implements IPlayerDataService by calling the backend API.
    /// </summary>
    public class ApiPlayerDataService : IPlayerDataService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiPlayerDataService> _logger;

        // Note: Caching could be added here for performance, but keep it simple for now.

        public event Action? OnDataChanged; // This might be less relevant now data is server-side

        public ApiPlayerDataService(HttpClient httpClient, ILogger<ApiPlayerDataService> logger)
        {
            _httpClient = httpClient;
             _logger = logger;
        }

        // GetPlayerStats might not be needed if we only display leaderboards
        public Task<PlayerStats?> GetPlayerStats(string playerId)
        {
             _logger.LogWarning("GetPlayerStats is not implemented in ApiPlayerDataService as stats are primarily managed server-side for leaderboards.");
            // If needed later, this would require a specific API endpoint.
            return Task.FromResult<PlayerStats?>(null); 
        }

        // This method now needs to fetch data for a specific difficulty from the API
        // The Leaderboard page will call this multiple times.
        public async Task<List<PlayerStats>> GetTopPlayers(AIDifficulty difficulty, int count = 5)
        {
             _logger.LogInformation("Fetching top {Count} players for difficulty {Difficulty} from API.", count, difficulty);
            try
            {
                // The API returns PlayerStatEntity, we need to map it back to PlayerStats for the existing interface contract.
                // Alternatively, update the interface/Leaderboard page to use PlayerStatEntity or a shared DTO.
                // For now, let's map it.
                var entities = await _httpClient.GetFromJsonAsync<List<PlayerStatEntity>>($"api/leaderboard/{difficulty}");
                
                if (entities == null) return new List<PlayerStats>();

                // Manual mapping (Consider AutoMapper for more complex scenarios)
                return entities.Select(e => 
                {
                    // Use the factory method and then set properties
                    var stats = PlayerStats.CreateNew(e.RowKey, e.PlayerName); 
                    stats.Wins = e.Wins;
                    stats.Losses = e.Losses;
                    stats.Draws = e.Draws;
                    stats.GamesPlayed = e.GamesPlayed;
                    // WinRate is calculated, no need to set
                    stats.WinStreak = e.CurrentWinStreak; // Map from Entity's CurrentWinStreak
                    stats.BestWinStreak = e.BestWinStreak;
                    // Map AverageGameTimeSeconds back to TotalPlayTime
                    stats.TotalPlayTime = TimeSpan.FromSeconds(e.AverageGameTimeSeconds * e.GamesPlayed); // Approximate total time
                    stats.LastPlayed = e.LastPlayed.DateTime; // Convert DateTimeOffset
                    return stats;
                }).ToList();
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error fetching leaderboard for difficulty {Difficulty} from API.", difficulty);
                return new List<PlayerStats>(); // Return empty list on error
            }
        }
        
        // Overload required by interface, but the difficulty-specific one is preferred now.
         public Task<List<PlayerStats>> GetTopPlayers(int count = 10)
        {
             _logger.LogWarning("GetTopPlayers without difficulty is deprecated. Call GetTopPlayers(difficulty, count) instead.");
             // Return empty or perhaps fetch for a default difficulty? Returning empty for now.
             return Task.FromResult(new List<PlayerStats>());
        }


        public async Task UpdatePlayerStats(string playerName, AIDifficulty difficulty, GameResult result, TimeSpan gameTime)
        {
             _logger.LogInformation("Sending player stat update to API for Player: {PlayerName}, Difficulty: {Difficulty}", playerName, difficulty);
            try
            {
                var updateDto = new PlayerStatUpdateDto
                {
                    PlayerName = playerName,
                    Difficulty = difficulty,
                    Result = result,
                    GameTimeMilliseconds = gameTime.TotalMilliseconds
                };

                var content = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/leaderboard/playerstats", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                     _logger.LogError("API error updating player stats. Status: {StatusCode}, Content: {ErrorContent}", response.StatusCode, errorContent);
                    // Optionally throw an exception or handle the error appropriately
                }
                 else 
                {
                     _logger.LogInformation("Successfully sent player stat update for Player: {PlayerName}", playerName);
                     OnDataChanged?.Invoke(); // Notify leaderboard page if it's listening
                }
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error sending player stat update to API for Player: {PlayerName}", playerName);
                 // Handle exception
            }
        }
        
        // This method is now less relevant as stats are tied to name+difficulty in the table.
        // The server handles entity creation on first update.
        public Task<PlayerStats> CreatePlayer(string playerName)
        {
             _logger.LogWarning("CreatePlayer is deprecated in ApiPlayerDataService. Player stats are created/updated on the server via UpdatePlayerStats.");
            // Return a dummy/placeholder if needed by calling code, but it won't be persisted here.
             // Use the correct factory method
             return Task.FromResult(PlayerStats.CreateNew(Guid.NewGuid().ToString(), playerName));
        }

        // Overload needed to satisfy interface - delegate to the correct method
         public Task UpdatePlayerStats(string playerId, GameResult result, TimeSpan gameTime)
        {
             _logger.LogWarning("UpdatePlayerStats using PlayerId is deprecated. Use UpdatePlayerStats(playerName, difficulty, result, gameTime).");
             // This overload is problematic as we don't know the difficulty or original player name easily.
             // For now, make it a no-op or throw an exception.
             return Task.CompletedTask; 
        }
    }
}
