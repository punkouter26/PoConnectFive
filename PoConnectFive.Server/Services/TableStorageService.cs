using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Server.Services
{
    public class TableStorageService : ITableStorageService
    {
        private const string TableName = "PlayerStats";
        private readonly TableServiceClient _tableServiceClient;
        private readonly TableClient _tableClient;
        private readonly ILogger<TableStorageService> _logger;
        private const string TestTableName = "connectivediagnostics";

        public TableStorageService(IConfiguration configuration, ILogger<TableStorageService> logger)
        {
            _logger = logger;
            var connectionString = configuration["AzureTableStorage:ConnectionString"];

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Azure Table Storage connection string is not configured");
                throw new InvalidOperationException("Azure Table Storage connection string is not configured");
            }

            try
            {
                _tableServiceClient = new TableServiceClient(connectionString);
                _tableClient = _tableServiceClient.GetTableClient(TableName);
                _tableClient.CreateIfNotExists();
                _logger.LogInformation("Table Storage Service initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Table Storage Service: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Result<bool>> CheckConnection()
        {
            try
            {
                _logger.LogInformation("Starting Table Storage connection check");

                // First check if we can list tables
                _logger.LogInformation("Attempting to list tables...");
                await foreach (var table in _tableServiceClient.QueryAsync())
                {
                    _logger.LogInformation("Found table: {TableName}", table.Name);
                }

                // Try to create a test table
                _logger.LogInformation("Attempting to create test table {TableName}...", TestTableName);
                var tableClient = _tableServiceClient.GetTableClient(TestTableName);
                await tableClient.CreateIfNotExistsAsync();

                // Try to insert a test entity
                var testEntity = new TableEntity("test", Guid.NewGuid().ToString())
                {
                    { "TestProperty", "TestValue" }
                };

                _logger.LogInformation("Attempting to insert test entity...");
                await tableClient.AddEntityAsync(testEntity);

                // Try to retrieve the test entity
                _logger.LogInformation("Attempting to retrieve test entity...");
                var retrievedEntity = await tableClient.GetEntityAsync<TableEntity>(
                    testEntity.PartitionKey,
                    testEntity.RowKey
                );

                // Clean up
                _logger.LogInformation("Cleaning up test entity...");
                await tableClient.DeleteEntityAsync(
                    testEntity.PartitionKey,
                    testEntity.RowKey
                );

                _logger.LogInformation("Table Storage connection check completed successfully");
                return Result<bool>.Success(true);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex,
                    "Table Storage connection check failed with status code {StatusCode}: {Message}",
                    ex.Status, ex.Message);
                return Result<bool>.Failure($"Azure Table Storage request failed: {ex.Message} (Status: {ex.Status})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Table Storage connection check failed: {Message}", ex.Message);
                return Result<bool>.Failure($"Table Storage connection check failed: {ex.Message}");
            }
        }

        public async Task<List<PlayerStatEntity>> GetTopPlayersByDifficultyAsync(AIDifficulty difficulty, int count = 5)
        {
            var partitionKey = difficulty.ToString();
            var topPlayers = new List<PlayerStatEntity>();

            try
            {
                _logger.LogInformation("Querying top players for difficulty {Difficulty}", partitionKey);
                var query = _tableClient.QueryAsync<PlayerStatEntity>(filter: $"PartitionKey eq '{partitionKey}'");

                await foreach (var player in query)
                {
                    topPlayers.Add(player);
                }

                topPlayers = topPlayers
                    .OrderByDescending(p => p.WinRate)
                    .ThenByDescending(p => p.Wins)
                    .Take(count)
                    .ToList();
                _logger.LogInformation("Found {Count} top players for difficulty {Difficulty}", topPlayers.Count, partitionKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top players for difficulty {Difficulty}", partitionKey);
            }

            return topPlayers;
        }

        public async Task UpsertPlayerStatAsync(string playerName, AIDifficulty difficulty, PlayerGameResult result, TimeSpan gameTime)
        {
            var partitionKey = difficulty.ToString();
            var rowKey = playerName.ToLowerInvariant();

            try
            {
                _logger.LogInformation("Upserting player stat for Player: {PlayerName}, Difficulty: {Difficulty}", playerName, difficulty);
                PlayerStatEntity? entity = null;
                try
                {
                    var response = await _tableClient.GetEntityAsync<PlayerStatEntity>(partitionKey, rowKey);
                    entity = response.Value;
                    _logger.LogInformation("Found existing entity for Player: {PlayerName}", playerName);
                }
                catch (Azure.RequestFailedException ex) when (ex.Status == 404)
                {
                    _logger.LogInformation("No existing entity found for Player: {PlayerName}. Creating new.", playerName);
                    entity = new PlayerStatEntity(playerName, difficulty);
                }

                if (entity != null)
                {
                    entity.UpdateStats(result, gameTime);
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
                throw;
            }
        }
    }

    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public string Error { get; }

        private Result(bool isSuccess, T value, string error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value) => new(true, value, string.Empty);
        public static Result<T> Failure(string error) => new(false, default!, error);
    }
}

