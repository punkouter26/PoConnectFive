using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace PoConnectFive.Client.Services;

/// <summary>
/// Provides standardized logging functionality throughout the application.
/// </summary>
public static class LoggingHelper
{
    /// <summary>
    /// Log a game start event.
    /// </summary>
    public static void LogGameStart<T>(ILogger<T> logger, string player1Name, string player2Name, bool isAIOpponent, string? difficulty = null)
    {
        var properties = new Dictionary<string, object>
        {
            ["Player1"] = player1Name,
            ["Player2"] = player2Name,
            ["IsAIOpponent"] = isAIOpponent
        };

        if (!string.IsNullOrEmpty(difficulty))
        {
            properties["AIDifficulty"] = difficulty;
        }

        logger.LogInformation("Game started: {Player1} vs {Player2} ({GameType}{Difficulty})",
            player1Name, player2Name,
            isAIOpponent ? "AI" : "Human",
            isAIOpponent ? $", {difficulty} difficulty" : "");
    }

    /// <summary>
    /// Log a game end event.
    /// </summary>
    public static void LogGameEnd<T>(ILogger<T> logger, string? winnerName, TimeSpan duration, string reason)
    {
        logger.LogInformation("Game ended after {Duration:mm\\:ss}: {Result}. Reason: {Reason}",
            duration,
            winnerName != null ? $"{winnerName} won" : "Draw",
            reason);
    }

    /// <summary>
    /// Log an API operation with consistent format.
    /// </summary>
    public static void LogApiOperation<T>(ILogger<T> logger, string operation, string endpoint, bool isSuccess = true, string? errorDetails = null)
    {
        if (isSuccess)
        {
            logger.LogInformation("API {Operation} to {Endpoint} successful", operation, endpoint);
        }
        else
        {
            logger.LogWarning("API {Operation} to {Endpoint} failed: {ErrorDetails}", operation, endpoint, errorDetails ?? "Unknown error");
        }
    }

    /// <summary>
    /// Log a user action with consistent format.
    /// </summary>
    public static void LogUserAction<T>(ILogger<T> logger, string action, string detail)
    {
        logger.LogDebug("User action: {Action} - {Detail}", action, detail);
    }

    /// <summary>
    /// Log a performance metric with consistent format.
    /// </summary>
    public static void LogPerformanceMetric<T>(ILogger<T> logger, string operation, TimeSpan duration)
    {
        logger.LogDebug("Performance: {Operation} completed in {Duration:N0}ms", operation, duration.TotalMilliseconds);
    }

    /// <summary>
    /// Log application lifecycle events.
    /// </summary>
    public static void LogApplicationEvent<T>(ILogger<T> logger, string eventName, string details)
    {
        logger.LogInformation("Application event: {EventName} - {Details}", eventName, details);
    }
}
