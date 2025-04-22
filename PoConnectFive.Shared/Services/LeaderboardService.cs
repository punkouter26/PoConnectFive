using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoConnectFive.Shared.Models;
using Microsoft.Extensions.Logging;

namespace PoConnectFive.Shared.Services
{
    /// <summary>
    /// Manages player statistics and leaderboard functionality
    /// 
    /// SOLID Principles:
    /// - Single Responsibility: Handles only player statistics and rankings
    /// - Open/Closed: New ranking criteria can be added without modifying existing code
    /// - Liskov Substitution: Different storage implementations can be used interchangeably
    /// - Interface Segregation: Separate interfaces for storage and leaderboard operations
    /// - Dependency Inversion: Depends on IStorageService abstraction
    /// 
    /// Design Patterns:
    /// - Repository Pattern: Abstracts data access operations
    /// - Strategy Pattern: Storage mechanism can be swapped (local storage, cloud storage, etc.)
    /// - Observer Pattern: Stats updates trigger leaderboard recalculation
    /// </summary>
    public interface ILeaderboardService
    {
        Task<List<PlayerStats>> GetTopPlayers(int count = 10);
        Task<PlayerStats> GetPlayerStats(string playerId);
        Task UpdatePlayerStats(string playerId, GameResult result, TimeSpan gameTime);
        Task<PlayerStats> CreatePlayer(string playerName);
    }

    public class LeaderboardService : ILeaderboardService
    {
        private readonly IStorageService _storageService;
        private readonly ILogger<LeaderboardService> _logger;
        private const string STORAGE_KEY = "playerStats";

        public LeaderboardService(IStorageService storageService, ILogger<LeaderboardService> logger)
        {
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<List<PlayerStats>> GetTopPlayers(int count = 10)
        {
            var sessions = await _storageService.GetAllAsync<GameSession>("GameSessions");
            var playerStats = new Dictionary<string, PlayerStats>();

            foreach (var session in sessions)
            {
                UpdatePlayerStats(playerStats, session.Player1, session.Winner);
                UpdatePlayerStats(playerStats, session.Player2, session.Winner);
            }

            return playerStats.Values
                .OrderByDescending(p => p.WinRate)
                .ThenByDescending(p => p.GamesPlayed)
                .Take(count)
                .ToList();
        }

        public async Task<PlayerStats> GetPlayerStats(string playerId)
        {
            var allStats = await GetAllPlayerStats();
            return allStats.FirstOrDefault(p => p.PlayerId == playerId) 
                ?? throw new KeyNotFoundException($"Player {playerId} not found");
        }

        public async Task UpdatePlayerStats(string playerId, GameResult result, TimeSpan gameTime)
        {
            // Repository pattern: Data access abstraction
            var allStats = await GetAllPlayerStats();
            var playerStats = allStats.FirstOrDefault(p => p.PlayerId == playerId);
            
            if (playerStats == null)
                throw new KeyNotFoundException($"Player {playerId} not found");

            // Observer pattern: Stats update triggers rankings recalculation
            playerStats.UpdateStats(result, gameTime);
            await SavePlayerStats(allStats);
        }

        public async Task<PlayerStats> CreatePlayer(string playerName)
        {
            var allStats = await GetAllPlayerStats();
            
            // Factory pattern: Creating new player stats
            string playerId = Guid.NewGuid().ToString();
            var newStats = PlayerStats.CreateNew(playerId, playerName);
            allStats.Add(newStats);
            
            await SavePlayerStats(allStats);
            return newStats;
        }

        // Repository pattern: Data access methods
        private async Task<List<PlayerStats>> GetAllPlayerStats()
        {
            try
            {
                var stats = await _storageService.GetItem<List<PlayerStats>>(STORAGE_KEY);
                return stats ?? new List<PlayerStats>();
            }
            catch
            {
                return new List<PlayerStats>();
            }
        }

        private Task SavePlayerStats(List<PlayerStats> stats)
        {
            return _storageService.SetItem(STORAGE_KEY, stats);
        }

        private void UpdatePlayerStats(Dictionary<string, PlayerStats> playerStats, Player player, Player? winner)
        {
            if (!playerStats.ContainsKey(player.Name))
            {
                playerStats[player.Name] = PlayerStats.CreateNew(player.Id.ToString(), player.Name);
            }

            var stats = playerStats[player.Name];
            stats.GamesPlayed++;
            if (winner?.Id == player.Id)
            {
                stats.Wins++;
            }
        }
    }

    /// <summary>
    /// Abstract storage operations to support different storage implementations
    /// Interface Segregation Principle: Focused interface for storage operations
    /// Strategy Pattern: Different storage mechanisms can be implemented
    /// </summary>
    public interface IStorageService
    {
        Task<T?> GetItem<T>(string key);
        Task SetItem<T>(string key, T value);
        Task RemoveItem(string key);
        Task<List<T>> GetAllAsync<T>(string collectionName);
    }
}
