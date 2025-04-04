using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Shared.Interfaces // Changed namespace
{
    /// <summary>
    /// Defines the contract for managing player data and leaderboard information.
    /// </summary>
    public interface IPlayerDataService
    {
        // --- New/Updated Methods for API Interaction ---

        /// <summary>
        /// Gets the top players for a specific AI difficulty level.
        /// </summary>
        Task<List<PlayerStats>> GetTopPlayers(AIDifficulty difficulty, int count = 5);

        /// <summary>
        /// Updates player statistics after a game against a specific AI difficulty.
        /// Player stats are identified by name and difficulty.
        /// </summary>
        Task UpdatePlayerStats(string playerName, AIDifficulty difficulty, GameResult result, TimeSpan gameTime);

        // --- Obsolete/Deprecated Methods from Local Storage Implementation ---

        /// <summary>
        /// Gets player stats by ID. Deprecated as stats are now primarily managed by name/difficulty on the server.
        /// </summary>
        [Obsolete("Use API-based methods. Stats are primarily managed by name/difficulty now.", true)]
        Task<PlayerStats?> GetPlayerStats(string playerId);

        /// <summary>
        /// Gets top players overall. Deprecated in favor of difficulty-specific leaderboards.
        /// </summary>
        [Obsolete("Use GetTopPlayers(AIDifficulty difficulty, int count) instead.", true)]
        Task<List<PlayerStats>> GetTopPlayers(int count = 10);

        /// <summary>
        /// Updates player stats by ID. Deprecated as stats are now primarily managed by name/difficulty on the server.
        /// </summary>
        [Obsolete("Use UpdatePlayerStats(string playerName, AIDifficulty difficulty, GameResult result, TimeSpan gameTime) instead.", true)]
        Task UpdatePlayerStats(string playerId, GameResult result, TimeSpan gameTime);

        /// <summary>
        /// Creates a player profile locally. Deprecated as player stats are created/updated implicitly on the server.
        /// </summary>
        [Obsolete("Player stats are created/updated implicitly on the server via UpdatePlayerStats.", true)]
        Task<PlayerStats> CreatePlayer(string playerName);

        /// <summary>
        /// Event raised when data changes. May have limited use with server-side data.
        /// </summary>
        event Action? OnDataChanged;
    }
}
