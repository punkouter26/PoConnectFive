using System;
using Azure; // Keep Azure dependencies here for now
using Azure.Data.Tables; // Keep Azure dependencies here for now
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Shared.Models // Changed namespace
{
    /// <summary>
    /// Represents a player's statistics entity for Azure Table Storage.
    /// PartitionKey = AIDifficulty (Easy, Medium, Hard)
    /// RowKey = PlayerName (case-insensitive for consistency)
    /// NOTE: This entity now resides in Shared, but still contains Azure-specific attributes.
    /// A cleaner approach might be a separate DTO in Shared and keep the Entity in Server,
    /// but moving the entity is simpler for this refactor.
    /// </summary>
    public class PlayerStatEntity : ITableEntity
    {
        // PartitionKey: Group stats by difficulty
        public string PartitionKey { get; set; } = default!;

        // RowKey: Unique identifier within the partition (player name)
        public string RowKey { get; set; } = default!;

        // Player's display name (might differ in casing from RowKey)
        public string PlayerName { get; set; } = default!;
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int GamesPlayed { get; set; }
        public double WinRate { get; set; }
        public int CurrentWinStreak { get; set; }
        public int BestWinStreak { get; set; }
        public double AverageGameTimeSeconds { get; set; }
        public DateTimeOffset LastPlayed { get; set; }

        // Required by ITableEntity
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Parameterless constructor needed for table storage deserialization
        public PlayerStatEntity() { }

        // Constructor to map from game results
        public PlayerStatEntity(string playerName, AIDifficulty difficulty)
        {
            PartitionKey = difficulty.ToString();
            // Store RowKey as lowercase for consistent lookups, but keep original casing for display
            RowKey = playerName.ToLowerInvariant();
            PlayerName = playerName;
            LastPlayed = DateTimeOffset.UtcNow;
        }

        public void UpdateStats(PlayerGameResult result, TimeSpan gameTime)
        {
            GamesPlayed++;
            LastPlayed = DateTimeOffset.UtcNow;

            // Calculate cumulative moving average for game time
            AverageGameTimeSeconds = ((AverageGameTimeSeconds * (GamesPlayed - 1)) + gameTime.TotalSeconds) / GamesPlayed;

            switch (result)
            {
                case PlayerGameResult.Win:
                    Wins++;
                    CurrentWinStreak++;
                    if (CurrentWinStreak > BestWinStreak)
                    {
                        BestWinStreak = CurrentWinStreak;
                    }
                    break;
                case PlayerGameResult.Loss:
                    Losses++;
                    CurrentWinStreak = 0;
                    break;
                case PlayerGameResult.Draw:
                    Draws++;
                    CurrentWinStreak = 0; // Typically streaks end on draws too
                    break;
            }

            // Recalculate Win Rate (avoid division by zero)
            if (GamesPlayed > 0)
            {
                WinRate = (double)Wins / GamesPlayed * 100.0;
            }
            else
            {
                WinRate = 0;
            }
        }
    }
}
