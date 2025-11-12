using Microsoft.AspNetCore.Mvc;
using PoConnectFive.Server.Services;

namespace PoConnectFive.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly IHealthCheckService _healthCheckService;

    public HealthController(
        ILogger<HealthController> logger,
        IHealthCheckService healthCheckService)
    {
        _logger = logger;
        _healthCheckService = healthCheckService;
    }

    [HttpGet]
    public async Task<IActionResult> CheckHealth()
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        _logger.LogInformation("Health check requested from IP: {RemoteIp}", remoteIp);

        var healthChecks = await _healthCheckService.PerformAllHealthChecksAsync();
        var allHealthy = healthChecks.All(h => h.IsHealthy);

        var healthResponse = new
        {
            Status = allHealthy ? "Healthy" : "Unhealthy",
            Timestamp = DateTime.UtcNow,
            Checks = healthChecks
        };

        if (!allHealthy)
        {
            _logger.LogWarning("Health check failed. Unhealthy components: {UnhealthyComponents}",
                string.Join(", ", healthChecks.Where(h => !h.IsHealthy).Select(h => h.Component)));
            return StatusCode(StatusCodes.Status503ServiceUnavailable, healthResponse);
        }

        return Ok(healthResponse);
    }

    [HttpGet("checks/storage")]
    public async Task<IActionResult> CheckStorage()
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        _logger.LogInformation("Storage check requested from IP: {RemoteIp}", remoteIp);

        var result = await _healthCheckService.CheckStorageHealthAsync();

        if (!result.IsHealthy)
        {
            _logger.LogWarning("Storage check failed: {Error}", result.Error);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                Status = "Unhealthy",
                Error = result.Error,
                Timestamp = DateTime.UtcNow
            });
        }

        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("checks/internet")]
    public async Task<IActionResult> CheckInternet()
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        _logger.LogInformation("Internet connectivity check requested from IP: {RemoteIp}", remoteIp);

        var result = await _healthCheckService.CheckInternetConnectivityAsync();

        if (!result.IsHealthy)
        {
            _logger.LogWarning("Internet connectivity check failed: {Error}", result.Error);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                Status = "Unhealthy",
                Error = result.Error,
                Timestamp = DateTime.UtcNow
            });
        }

        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
    }
}

/// <summary>
/// Model for health check results.
/// </summary>
public class HealthCheckResult
{
    public string Component { get; set; } = "";
    public bool IsHealthy { get; set; }
    public string? Error { get; set; }
    public long ResponseTime { get; set; }
}
