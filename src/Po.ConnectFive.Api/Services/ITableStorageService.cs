using Azure;
using Azure.Data.Tables;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Server.Services;

public interface ITableStorageService
{
    public Task<Result<bool>> CheckConnection();
    public Task<List<PlayerStatEntity>> GetTopPlayersByDifficultyAsync(AIDifficulty difficulty, int count = 5);
    public Task UpsertPlayerStatAsync(string playerName, AIDifficulty difficulty, PlayerGameResult result, TimeSpan gameTime);
}
