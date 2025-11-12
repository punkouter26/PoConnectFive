using System;

namespace PoConnectFive.Shared.Models;

/// <summary>
/// Represents player statistics and achievements.
/// </summary>
public class PlayerStats
{
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public int GamesPlayed { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public DateTime LastPlayed { get; set; }
    public TimeSpan TotalPlayTime { get; set; }
    public int WinStreak { get; set; }
    public int BestWinStreak { get; set; }
    public double WinRate => GamesPlayed > 0 ? (double)Wins / GamesPlayed * 100 : 0;

    // Factory method for creating new player stats
    public static PlayerStats CreateNew(string playerId, string playerName)
    {
        return new PlayerStats
        {
            PlayerId = playerId,
            PlayerName = playerName,
            LastPlayed = DateTime.UtcNow
        };
    }

    // Update stats after a game
    public void UpdateStats(PlayerGameResult result, TimeSpan gameTime)
    {
        GamesPlayed++;
        TotalPlayTime += gameTime;
        LastPlayed = DateTime.UtcNow;

        switch (result)
        {
            case PlayerGameResult.Win:
                Wins++;
                WinStreak++;
                BestWinStreak = Math.Max(BestWinStreak, WinStreak);
                break;
            case PlayerGameResult.Loss:
                Losses++;
                WinStreak = 0;
                break;
            case PlayerGameResult.Draw:
                Draws++;
                WinStreak = 0;
                break;
        }
    }
}

public enum PlayerGameResult
{
    Win,
    Loss,
    Draw
}
