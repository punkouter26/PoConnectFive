using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Server.Extensions;

/// <summary>
/// Extension methods for Application Insights TelemetryClient
/// Consolidates common telemetry patterns to reduce code duplication.
/// </summary>
public static class TelemetryExtensions
{
    /// <summary>
    /// Track a game completion event with standardized properties and metrics.
    /// </summary>
    public static void TrackGameCompleted(
        this TelemetryClient telemetryClient,
        string playerName,
        AIDifficulty difficulty,
        PlayerGameResult result,
        double gameTimeMilliseconds)
    {
        telemetryClient.TrackEvent("GameCompleted", new Dictionary<string, string>
        {
            { "PlayerName", playerName },
            { "Difficulty", difficulty.ToString() },
            { "Result", result.ToString() },
            { "GameTimeMs", gameTimeMilliseconds.ToString() }
        }, new Dictionary<string, double>
        {
            { "GameDurationSeconds", gameTimeMilliseconds / 1000.0 }
        });
    }

    /// <summary>
    /// Track game duration metric with difficulty and result context.
    /// </summary>
    public static void TrackGameDuration(
        this TelemetryClient telemetryClient,
        double durationMilliseconds,
        AIDifficulty difficulty,
        PlayerGameResult result)
    {
        telemetryClient.TrackMetric("GameDuration", durationMilliseconds, new Dictionary<string, string>
        {
            { "Difficulty", difficulty.ToString() },
            { "Result", result.ToString() }
        });
    }

    /// <summary>
    /// Track leaderboard load performance with difficulty context.
    /// </summary>
    public static void TrackLeaderboardLoad(
        this TelemetryClient telemetryClient,
        double loadTimeMilliseconds,
        AIDifficulty difficulty,
        int playerCount)
    {
        telemetryClient.TrackMetric("LeaderboardLoadTime", loadTimeMilliseconds, new Dictionary<string, string>
        {
            { "Difficulty", difficulty.ToString() },
            { "PlayerCount", playerCount.ToString() }
        });

        telemetryClient.TrackEvent("LeaderboardViewed", new Dictionary<string, string>
        {
            { "Difficulty", difficulty.ToString() },
            { "PlayerCount", playerCount.ToString() },
            { "LoadTimeMs", loadTimeMilliseconds.ToString() }
        });
    }

    /// <summary>
    /// Track an operation exception with standardized properties.
    /// </summary>
    public static void TrackOperationException(
        this TelemetryClient telemetryClient,
        Exception exception,
        string operationName,
        IDictionary<string, string>? additionalProperties = null)
    {
        var properties = new Dictionary<string, string>
        {
            { "Operation", operationName }
        };

        if (additionalProperties != null)
        {
            foreach (var kvp in additionalProperties)
            {
                properties[kvp.Key] = kvp.Value;
            }
        }

        telemetryClient.TrackException(exception, properties);
    }

    /// <summary>
    /// Track a client log event from browser.
    /// </summary>
    public static void TrackClientLog(
        this TelemetryClient telemetryClient,
        string level,
        string message,
        string clientIp,
        string userAgent,
        string? category = null,
        string? exceptionMessage = null)
    {
        var properties = new Dictionary<string, string>
        {
            { "Source", "Client" },
            { "ClientIp", clientIp },
            { "UserAgent", userAgent },
            { "Level", level },
            { "Message", message }
        };

        if (!string.IsNullOrEmpty(category))
        {
            properties["Category"] = category;
        }

        if (!string.IsNullOrEmpty(exceptionMessage))
        {
            var exception = new Exception(exceptionMessage);
            telemetryClient.TrackException(exception, properties);
        }
        else
        {
            telemetryClient.TrackEvent("ClientLog", properties);
        }
    }

    /// <summary>
    /// Track a custom client event with properties and metrics.
    /// </summary>
    public static void TrackClientEvent(
        this TelemetryClient telemetryClient,
        string eventName,
        string clientIp,
        string userAgent,
        IDictionary<string, object>? customProperties = null,
        IDictionary<string, object>? customMetrics = null)
    {
        var properties = new Dictionary<string, string>
        {
            { "Source", "Client" },
            { "ClientIp", clientIp },
            { "UserAgent", userAgent }
        };

        if (customProperties != null)
        {
            foreach (var prop in customProperties)
            {
                properties[prop.Key] = prop.Value?.ToString() ?? "";
            }
        }

        var metrics = new Dictionary<string, double>();
        if (customMetrics != null)
        {
            foreach (var metric in customMetrics)
            {
                if (double.TryParse(metric.Value?.ToString(), out double value))
                {
                    metrics[metric.Key] = value;
                }
            }
        }

        telemetryClient.TrackEvent(eventName, properties, metrics);
    }

    /// <summary>
    /// Track a client performance metric.
    /// </summary>
    public static void TrackClientPerformance(
        this TelemetryClient telemetryClient,
        string metricName,
        double value,
        string clientIp,
        string? component = null)
    {
        var properties = new Dictionary<string, string>
        {
            { "Source", "Client" },
            { "ClientIp", clientIp }
        };

        if (!string.IsNullOrEmpty(component))
        {
            properties["Component"] = component;
        }

        telemetryClient.TrackMetric(metricName, value, properties);
    }

    /// <summary>
    /// Handle operation errors consistently with structured logging and telemetry tracking.
    /// Consolidates error handling pattern to reduce duplication across controllers.
    /// </summary>
    /// <typeparam name="T">The controller type for typed logging.</typeparam>
    /// <param name="controller">The controller instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="telemetryClient">The telemetry client instance.</param>
    /// <param name="operationName">The name of the operation that failed.</param>
    /// <param name="context">Optional additional context properties for telemetry.</param>
    /// <returns>An IActionResult with status 500 and error message.</returns>
    public static IActionResult HandleOperationError<T>(
        this ControllerBase controller,
        Exception ex,
        ILogger<T> logger,
        TelemetryClient telemetryClient,
        string operationName,
        Dictionary<string, string>? context = null)
    {
        // Log the error with structured logging
        logger.LogError(ex, "Error in {Operation}", operationName);

        // Track the exception in Application Insights
        telemetryClient.TrackOperationException(ex, operationName, context ?? new Dictionary<string, string>());

        // Return a consistent error response
        return controller.StatusCode(500, $"Error in {operationName}");
    }
}
