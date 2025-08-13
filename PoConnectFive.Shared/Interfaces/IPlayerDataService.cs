using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Shared.Interfaces
{
    /// <summary>
    /// Defines the contract for managing player data and leaderboard information.
    /// </summary>
    public interface IPlayerDataService
    {
        /// <summary>
        /// Gets the top players for a specific AI difficulty level.
        /// </summary>
        Task<List<PlayerStats>> GetTopPlayers(AIDifficulty difficulty, int count = 5);

        /// <summary>
        /// Updates player statistics after a game against a specific AI difficulty.
        /// Player stats are identified by name and difficulty.
        /// </summary>
        Task UpdatePlayerStats(string playerName, AIDifficulty difficulty, PlayerGameResult result, TimeSpan gameTime);

        /// <summary>
        /// Gets player stats by ID. Maintained for backward compatibility.
        /// </summary>
        Task<PlayerStats?> GetPlayerStats(string playerId);

        /// <summary>
        /// Event raised when data changes. May have limited use with server-side data.
        /// </summary>
        event Action? OnDataChanged;
    }
}
