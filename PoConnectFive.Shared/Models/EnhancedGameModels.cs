using System;
using System.Collections.Generic;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Shared.Models
{
    /// <summary>
    /// Represents a game result with comprehensive data for analytics
    /// </summary>
    public class GameResult
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime? CompletedAt { get; set; }
        public string? OpponentName { get; set; }
        public string? OpponentDifficulty { get; set; } // "Easy", "Medium", "Hard", or null for human
        public bool IsWin { get; set; }
        public bool IsDraw { get; set; }
        public TimeSpan? Duration { get; set; }
        public int TotalMoves { get; set; }
        public Player? Winner { get; set; }
        public List<string> MoveHistory { get; set; } = new();
        public Dictionary<string, object> GameMetadata { get; set; } = new();
    }

    /// <summary>
    /// Enhanced player statistics with detailed analytics
    /// </summary>
    public class PlayerStatistics
    {
        public string Id { get; set; } = string.Empty;
        public string PlayerId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public int TotalGames { get; set; }
        public int TotalWins { get; set; }
        public int Wins { get; set; }
        public int TotalLosses { get; set; }
        public int Losses { get; set; }
        public int TotalDraws { get; set; }
        public int Draws { get; set; }
        public double WinRate { get; set; }
        public TimeSpan TotalPlayTime { get; set; }
        public TimeSpan AverageGameDuration { get; set; }
        public int LongestWinStreak { get; set; }
        public int BestWinStreak { get; set; }
        public int CurrentWinStreak { get; set; }
        public Dictionary<string, int> OpponentStats { get; set; } = new();
        public List<GameResult> RecentGames { get; set; } = new();
        public DateTime LastPlayed { get; set; }
        public Dictionary<string, double> AdvancedMetrics { get; set; } = new();
    }

    /// <summary>
    /// Move quality ratings
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
    /// Playing style categories
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
    /// Trend direction enumeration
    /// </summary>
    public enum TrendDirection
    {
        Improving,
        Declining,
        Stable
    }

    // Additional types for enhanced UI features

    public enum PreviewEffectType
    {
        Glow,
        Pulse,
        Shadow,
        Highlight
    }

    public enum HapticPatternType
    {
        Light,
        Medium,
        Heavy,
        Custom
    }

    public enum IndicatorType
    {
        Success,
        Warning,
        Error,
        Info,
        Neutral,
        WinningLine,
        Threat,
        BlockingMove,
        PreviewPiece,
        LastMove
    }

    public enum ChartType
    {
        Line,
        Bar,
        Pie,
        Scatter,
        Area,
        Performance
    }

    public enum InsightType
    {
        Performance,
        Strategy,
        Improvement,
        Achievement,
        Trend,
        Positive,
        Strategic,
        Warning
    }

    public enum InsightPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum ExportFormat
    {
        PDF,
        CSV,
        JSON,
        PNG,
        Csv  // Alternative casing that some code might expect
    }

    public enum ChartPeriod
    {
        LastWeek,
        LastMonth,
        LastQuarter,
        LastYear,
        AllTime,
        Daily,
        Weekly,
        Monthly
    }

    // Complex data models for enhanced features
    public class ThemeConfiguration
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, string> Colors { get; set; } = new();
        public bool IsDarkMode { get; set; }
    }

    public class AccessibilitySettings
    {
        public bool HighContrast { get; set; }
        public bool ScreenReaderEnabled { get; set; }
        public bool KeyboardNavigation { get; set; }
        public double FontSize { get; set; } = 1.0;

        // Additional properties used in enhanced pages
        public bool HighContrastMode { get; set; }
        public bool ColorBlindFriendly { get; set; }
        public bool ReducedMotion { get; set; }
        public bool HapticFeedback { get; set; }
        public double FontSizeMultiplier { get; set; } = 1.0;
        public bool ScreenReaderSupport { get; set; }
    }

    public class ScreenReaderDescription
    {
        public string Text { get; set; } = string.Empty;
        public string AriaLabel { get; set; } = string.Empty;
    }

    public class MovePreview
    {
        public int Column { get; set; }
        public int PlayerId { get; set; }
        public PreviewEffectType Effect { get; set; }
        public string Color { get; set; } = string.Empty;
        public List<PreviewEffect> PreviewEffects { get; set; } = new();
    }

    public class VisualIndicator
    {
        public IndicatorType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public List<(int Row, int Column)> Positions { get; set; } = new();
    }

    public class AnimationConfig
    {
        public TimeSpan Duration { get; set; }
        public string EasingFunction { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
    }

    public class DashboardData
    {
        public Dictionary<string, object> Metrics { get; set; } = new();
        public List<ChartData> Charts { get; set; } = new();
        public DateTime LastUpdated { get; set; }
        public TrendAnalysis TrendAnalysis { get; set; } = new();
        public ChartData PerformanceChart { get; set; } = new();
        public ChartData WinRateChart { get; set; } = new();
        public ChartData DifficultyProgressChart { get; set; } = new();
        public ChartData PlayingTimeChart { get; set; } = new();
        public List<Achievement> AchievementProgress { get; set; } = new();
        public List<PerformanceInsight> PerformanceInsights { get; set; } = new();
    }

    public class ChartData
    {
        public string Label { get; set; } = string.Empty;
        public List<DataPoint> Points { get; set; } = new();
        public ChartType Type { get; set; }
    }

    public class DataPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class ImprovementSuggestion
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public InsightPriority Priority { get; set; }
    }

    public class Achievement
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsUnlocked { get; set; }
        public DateTime? UnlockedDate { get; set; }
    }

    public class TrendAnalysis
    {
        public double TrendDirection { get; set; }
        public string TrendDescription { get; set; } = string.Empty;
        public Dictionary<string, double> Metrics { get; set; } = new();
    }

    public class PerformanceInsight
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public InsightType Type { get; set; }
        public InsightPriority Priority { get; set; }
    }

    public class DateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ChartConfiguration
    {
        public ChartType ChartType { get; set; }
        public ChartPeriod Period { get; set; }
        public Dictionary<string, object> Options { get; set; } = new();
    }

    public class ExportConfiguration
    {
        public ExportFormat Format { get; set; }
        public DateRange DateRange { get; set; } = new();
        public List<string> IncludedMetrics { get; set; } = new();
    }

    // Class versions for enhanced functionality
    public class PreviewEffect
    {
        public EffectType Type { get; set; }
        public (int Row, int Column) Position { get; set; }
        public string Color { get; set; } = string.Empty;
        public double Opacity { get; set; } = 1.0;
    }

    public enum EffectType
    {
        Highlight,
        Shadow,
        Glow,
        Border,
        Pulse
    }

    public class HapticPattern
    {
        public TimeSpan Duration { get; set; }
        public double Intensity { get; set; }
        public string Pattern { get; set; } = string.Empty;
    }
}
