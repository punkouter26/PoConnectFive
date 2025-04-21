using Azure;
using Azure.Data.Tables;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Server.Services;

public interface ITableStorageService
{
    Task<Result<bool>> CheckConnection();
    Task<List<PlayerStatEntity>> GetTopPlayersByDifficultyAsync(AIDifficulty difficulty, int count = 5);
    Task UpsertPlayerStatAsync(string playerName, AIDifficulty difficulty, GameResult result, TimeSpan gameTime);
} 