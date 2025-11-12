using Microsoft.AspNetCore.Mvc;

namespace PoConnectFive.Server.Controllers;

/// <summary>
/// Controller for diagnostic operations and client-side diagnostic reporting.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(ILogger<DiagnosticsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// POST /api/diagnostics - Receives diagnostic reports from clients.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult LogDiagnostics([FromBody] DiagnosticsLog log)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var clientUrl = Request.Headers["Referer"].ToString();
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        _logger.LogInformation("Received diagnostics log from {ClientUrl} (IP: {ClientIp}) at {Timestamp}",
            clientUrl, clientIp, DateTime.UtcNow);

        foreach (var result in log.Results.Where(r => !r.IsHealthy))
        {
            _logger.LogWarning("Unhealthy diagnostic result: {Component} - {Error}",
                result.Component, result.Error);
        }

        return Ok(new { status = "logged", timestamp = DateTime.UtcNow });
    }
}

/// <summary>
/// Model for diagnostic log submissions.
/// </summary>
public class DiagnosticsLog
{
    public List<DiagnosticResult> Results { get; set; } = new();
}

/// <summary>
/// Model for individual diagnostic results.
/// </summary>
public class DiagnosticResult
{
    public string Component { get; set; } = "";
    public bool IsHealthy { get; set; }
    public string? Error { get; set; }
}
