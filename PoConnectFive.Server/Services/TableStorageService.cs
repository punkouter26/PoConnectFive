using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
// using PoConnectFive.Server.Models; // Removed
using PoConnectFive.Shared.Models; // Now contains PlayerStatEntity, AIDifficulty, GameResult

namespace PoConnectFive.Server.Services
{
    public interface ITableStorageService
    {
        Task<List<PlayerStatEntity>> GetTopPlayersByDifficultyAsync(AIDifficulty difficulty, int count = 5);
        Task UpsertPlayerStatAsync(string playerName, AIDifficulty difficulty, GameResult result, TimeSpan gameTime);
    }

    public class TableStorageService : ITableStorageService
    {
        private const string TableName = "PlayerStats"; // Match the table created in Azure
        private readonly TableServiceClient _tableServiceClient;
        private readonly TableClient _tableClient;
        private readonly ILogger<TableStorageService> _logger;

        public TableStorageService(IConfiguration configuration, ILogger<TableStorageService> logger)
        {
            // Ensure connection string is configured (e.g., in appsettings.json or User Secrets)
            var connectionString = configuration.GetConnectionString("StorageConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Azure Storage Connection String 'StorageConnectionString' not configured.");
            }
            _tableServiceClient = new TableServiceClient(connectionString);
            _tableClient = _tableServiceClient.GetTableClient(TableName);
            _tableClient.CreateIfNotExists(); // Ensure table exists
             _logger = logger;
        }

        public async Task<List<PlayerStatEntity>> GetTopPlayersByDifficultyAsync(AIDifficulty difficulty, int count = 5)
        {
            var partitionKey = difficulty.ToString();
            var topPlayers = new List<PlayerStatEntity>();

            try
            {
                 _logger.LogInformation("Querying top players for difficulty {Difficulty}", partitionKey);
                // Query entities for the specific difficulty, order by WinRate descending, then take top 'count'
                // Note: Table storage querying is limited. Efficient sorting/top N requires careful design or post-query processing.
                // For simplicity here, we query all for the partition and sort in memory.
                // For larger datasets, consider alternative indexing or pre-calculated leaderboards.
                 var query = _tableClient.QueryAsync<PlayerStatEntity>(filter: $"PartitionKey eq '{partitionKey}'");
                
                await foreach (var player in query)
                {
                    topPlayers.Add(player);
                }

                // Sort in memory
                topPlayers = topPlayers
                    .OrderByDescending(p => p.WinRate)
                    .ThenByDescending(p => p.Wins) // Secondary sort
                    .Take(count)
                    .ToList();
                 _logger.LogInformation("Found {Count} top players for difficulty {Difficulty}", topPlayers.Count, partitionKey);
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error retrieving top players for difficulty {Difficulty}", partitionKey);
                // Return empty list or rethrow depending on desired error handling
            }

            return topPlayers;
        }

        public async Task UpsertPlayerStatAsync(string playerName, AIDifficulty difficulty, GameResult result, TimeSpan gameTime)
        {
            var partitionKey = difficulty.ToString();
            var rowKey = playerName.ToLowerInvariant(); // Use lowercase for consistent lookup

            try
            {
                 _logger.LogInformation("Upserting player stat for Player: {PlayerName}, Difficulty: {Difficulty}", playerName, difficulty);
                PlayerStatEntity? entity = null;
                try
                {
                    // Attempt to retrieve the existing entity
                    var response = await _tableClient.GetEntityAsync<PlayerStatEntity>(partitionKey, rowKey);
                    entity = response.Value;
                     _logger.LogInformation("Found existing entity for Player: {PlayerName}", playerName);
                }
                catch (Azure.RequestFailedException ex) when (ex.Status == 404)
                {
                     _logger.LogInformation("No existing entity found for Player: {PlayerName}. Creating new.", playerName);
                    // Entity does not exist, create a new one
                    entity = new PlayerStatEntity(playerName, difficulty);
                }

                if (entity != null)
                {
                    // Update the stats
                    entity.UpdateStats(result, gameTime);

                    // Upsert (Insert or Replace) the entity
                    await _tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);
                     _logger.LogInformation("Successfully upserted player stat for Player: {PlayerName}", playerName);
                }
                 else
                {
                    _logger.LogWarning("Entity was unexpectedly null after get/create attempt for Player: {PlayerName}", playerName);
                }
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error upserting player stat for Player: {PlayerName}, Difficulty: {Difficulty}", playerName, difficulty);
                // Handle exception (e.g., log, retry, etc.)
                throw; // Rethrow for controller to handle
            }
        }
    }
}
