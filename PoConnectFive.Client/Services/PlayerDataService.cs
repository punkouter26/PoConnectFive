using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoConnectFive.Shared.Models;
using PoConnectFive.Shared.Interfaces; // Add reference to new interface location

namespace PoConnectFive.Client.Services
{
    // Interface moved to PoConnectFive.Shared.Interfaces

    /// <summary>
    /// OBSOLETE: Manages player data using local storage. Replaced by ApiPlayerDataService.
    /// </summary>
    public class PlayerDataService // Removed ": IPlayerDataService"
    {
        private const string PLAYERS_KEY = "players";
        private readonly ILocalStorageService _storage;
        private readonly Dictionary<string, PlayerStats> _cache;

        public event Action? OnDataChanged;

        public PlayerDataService(ILocalStorageService storage)
        {
            _storage = storage;
            _cache = new Dictionary<string, PlayerStats>();
        }

        public async Task<PlayerStats?> GetPlayerStats(string playerId)
        {
            await EnsureCacheLoaded();
            return _cache.GetValueOrDefault(playerId);
        }

        public async Task<List<PlayerStats>> GetTopPlayers(int count = 10)
        {
            await EnsureCacheLoaded();
            return _cache.Values
                .OrderByDescending(p => p.WinRate)
                .ThenByDescending(p => p.Wins)
                .ThenBy(p => p.GamesPlayed)
                .Take(count)
                .ToList();
        }

        public async Task UpdatePlayerStats(string playerId, GameResult result, TimeSpan gameTime)
        {
            await EnsureCacheLoaded();
            
            if (!_cache.TryGetValue(playerId, out var stats))
                throw new KeyNotFoundException($"Player {playerId} not found");

            stats.UpdateStats(result, gameTime);
            await SaveCache();
            OnDataChanged?.Invoke();
        }

        public async Task<PlayerStats> CreatePlayer(string playerName)
        {
            await EnsureCacheLoaded();

            // Generate unique ID
            string playerId;
            do
            {
                playerId = Guid.NewGuid().ToString();
            } while (_cache.ContainsKey(playerId));

            var stats = PlayerStats.CreateNew(playerId, playerName);
            _cache[playerId] = stats;
            await SaveCache();
            OnDataChanged?.Invoke();

            return stats;
        }

        private async Task EnsureCacheLoaded()
        {
            if (_cache.Count == 0)
            {
                var players = await _storage.GetItem<Dictionary<string, PlayerStats>>(PLAYERS_KEY);
                if (players != null)
                {
                    foreach (var kvp in players)
                    {
                        _cache[kvp.Key] = kvp.Value;
                    }
                }
            }
        }

        private Task SaveCache()
        {
            return _storage.SetItem(PLAYERS_KEY, _cache);
        }
    }
}
