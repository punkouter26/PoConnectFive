namespace PoConnectFive.Shared.Models;

/// <summary>
/// Move quality ratings used by GameAnalyticsService.
/// </summary>
public enum MoveQuality
{
    Blunder = 1,
    Poor = 2,
    Average = 3,
    Good = 4,
    Excellent = 5
}

/// <summary>
/// Playing style categories used by GameAnalyticsService.
/// </summary>
public enum PlayingStyle
{
    Aggressive,
    Defensive,
    Balanced,
    Tactical,
    Positional,
    Unpredictable
}

/// <summary>
/// Trend direction enumeration used by GameAnalyticsService.
/// </summary>
public enum TrendDirection
{
    Improving,
    Declining,
    Stable
}
