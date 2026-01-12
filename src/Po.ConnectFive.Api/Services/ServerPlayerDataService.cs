using PoConnectFive.Server.Services;
using PoConnectFive.Shared.Interfaces;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Server.Services;

/// <summary>
/// Server-side implementation of IPlayerDataService for SSR rendering.
/// This directly uses ITableStorageService instead of making HTTP calls.
/// </summary>
public class ServerPlayerDataService : IPlayerDataService
{
    private readonly ITableStorageService _tableStorageService;
    private readonly ILogger<ServerPlayerDataService> _logger;

    public event Action? OnDataChanged;

    public ServerPlayerDataService(
        ITableStorageService tableStorageService,
        ILogger<ServerPlayerDataService> logger)
    {
        _tableStorageService = tableStorageService;
        _logger = logger;
    }

    public async Task<List<PlayerStats>> GetTopPlayers(AIDifficulty difficulty, int count = 5)
    {
        try
        {
            var entities = await _tableStorageService.GetTopPlayersByDifficultyAsync(difficulty, count);
            return entities.Select(e => new PlayerStats
            {
                PlayerId = e.RowKey,
                PlayerName = e.PlayerName,
                Wins = e.Wins,
                Losses = e.Losses,
                Draws = e.Draws,
                GamesPlayed = e.GamesPlayed,
                TotalPlayTime = TimeSpan.FromSeconds(e.AverageGameTimeSeconds * e.GamesPlayed),
                WinStreak = e.CurrentWinStreak,
                BestWinStreak = e.BestWinStreak,
                LastPlayed = e.LastPlayed.DateTime
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top players for difficulty {Difficulty}", difficulty);
            return new List<PlayerStats>();
        }
    }

    public async Task UpdatePlayerStats(string playerName, AIDifficulty difficulty, PlayerGameResult result, TimeSpan gameTime)
    {
        try
        {
            await _tableStorageService.UpsertPlayerStatAsync(playerName, difficulty, result, gameTime);
            OnDataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating player stats for {PlayerName}", playerName);
            throw;
        }
    }

    public Task<PlayerStats?> GetPlayerStats(string playerId)
    {
        // This method is not typically used in SSR scenarios
        _logger.LogWarning("GetPlayerStats by ID is not fully implemented for server-side rendering");
        return Task.FromResult<PlayerStats?>(null);
    }
}
