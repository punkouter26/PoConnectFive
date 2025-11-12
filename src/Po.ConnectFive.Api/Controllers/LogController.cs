using System.ComponentModel.DataAnnotations;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;
using PoConnectFive.Server.Extensions;

namespace PoConnectFive.Server.Controllers;

/// <summary>
/// Controller for receiving client-side logs and telemetry
/// Implements client-to-server log ingestion for centralized monitoring.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LogController : ControllerBase
{
    private readonly ILogger<LogController> _logger;
    private readonly TelemetryClient _telemetryClient;

    public LogController(ILogger<LogController> logger, TelemetryClient telemetryClient)
    {
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    /// <summary>
    /// POST /api/log/client - Receives client-side logs.
    /// </summary>
    [HttpPost("client")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult LogFromClient([FromBody] ClientLogEntry logEntry)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Get client information
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers["User-Agent"].ToString();

        // Log to Serilog/Application Insights based on log level
        switch (logEntry.Level.ToLowerInvariant())
        {
            case "error":
            case "critical":
                _logger.LogError("[Client {ClientIp}] {Message} | Category: {Category} | Data: {Data}",
                    clientIp, logEntry.Message, logEntry.Category, logEntry.AdditionalData);

                // Track using extension method
                _telemetryClient.TrackClientLog(
                    logEntry.Level,
                    logEntry.Message,
                    clientIp,
                    userAgent,
                    logEntry.Category,
                    logEntry.Exception);
                break;

            case "warning":
                _logger.LogWarning("[Client {ClientIp}] {Message} | Category: {Category} | Data: {Data}",
                    clientIp, logEntry.Message, logEntry.Category, logEntry.AdditionalData);
                break;

            case "information":
            case "info":
                _logger.LogInformation("[Client {ClientIp}] {Message} | Category: {Category} | Data: {Data}",
                    clientIp, logEntry.Message, logEntry.Category, logEntry.AdditionalData);
                break;

            default:
                _logger.LogDebug("[Client {ClientIp}] {Message} | Category: {Category} | Data: {Data}",
                    clientIp, logEntry.Message, logEntry.Category, logEntry.AdditionalData);
                break;
        }

        return Ok(new { status = "logged", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// POST /api/log/event - Receives custom telemetry events from client.
    /// </summary>
    [HttpPost("event")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult LogEvent([FromBody] TelemetryEvent telemetryEvent)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Track custom event using extension method
        _telemetryClient.TrackClientEvent(
            telemetryEvent.EventName,
            clientIp,
            Request.Headers["User-Agent"].ToString(),
            telemetryEvent.Properties,
            telemetryEvent.Metrics);

        _logger.LogInformation("[Client Event] {EventName} from {ClientIp} | Properties: {Properties}",
            telemetryEvent.EventName, clientIp, telemetryEvent.Properties);

        return Ok(new { status = "tracked", eventName = telemetryEvent.EventName, timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// POST /api/log/performance - Receives performance metrics from client.
    /// </summary>
    [HttpPost("performance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult LogPerformance([FromBody] PerformanceMetric performanceMetric)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Track metric using extension method
        _telemetryClient.TrackClientPerformance(
            performanceMetric.MetricName,
            performanceMetric.Value,
            clientIp,
            performanceMetric.Component);

        _logger.LogInformation("[Client Performance] {MetricName}: {Value}ms from {ClientIp}",
            performanceMetric.MetricName, performanceMetric.Value, clientIp);

        return Ok(new { status = "tracked", timestamp = DateTime.UtcNow });
    }
}

/// <summary>
/// Model for client log entries.
/// </summary>
public class ClientLogEntry
{
    [Required(ErrorMessage = "Log level is required")]
    public string Level { get; set; } = "Information";

    [Required(ErrorMessage = "Log message is required")]
    [StringLength(5000, ErrorMessage = "Message cannot exceed 5000 characters")]
    public string Message { get; set; } = "";

    [StringLength(200, ErrorMessage = "Category cannot exceed 200 characters")]
    public string? Category { get; set; }

    [StringLength(10000, ErrorMessage = "Exception cannot exceed 10000 characters")]
    public string? Exception { get; set; }

    public Dictionary<string, object>? AdditionalData { get; set; }
}

/// <summary>
/// Model for custom telemetry events.
/// </summary>
public class TelemetryEvent
{
    [Required(ErrorMessage = "Event name is required")]
    [StringLength(200, ErrorMessage = "Event name cannot exceed 200 characters")]
    public string EventName { get; set; } = "";

    public Dictionary<string, object>? Properties { get; set; }
    public Dictionary<string, object>? Metrics { get; set; }
}

/// <summary>
/// Model for performance metrics.
/// </summary>
public class PerformanceMetric
{
    [Required(ErrorMessage = "Metric name is required")]
    [StringLength(200, ErrorMessage = "Metric name cannot exceed 200 characters")]
    public string MetricName { get; set; } = "";

    [Range(0, double.MaxValue, ErrorMessage = "Value must be non-negative")]
    public double Value { get; set; }

    [StringLength(200, ErrorMessage = "Component name cannot exceed 200 characters")]
    public string? Component { get; set; }
}
