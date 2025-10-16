using Microsoft.AspNetCore.Mvc;
using PoConnectFive.Server.Services;
using System.Net;
using System.Net.Http;

namespace PoConnectFive.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly ITableStorageService _storageService;
    private readonly IHttpClientFactory _httpClientFactory;

    public HealthController(
        ILogger<HealthController> logger,
        ITableStorageService storageService,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _storageService = storageService;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> CheckHealth()
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        _logger.LogInformation("Health check requested from IP: {RemoteIp}", remoteIp);

        var healthChecks = new List<HealthCheckResult>();

        // Check Azure Table Storage
        try
        {
            var storageResult = await _storageService.CheckConnection();
            healthChecks.Add(new HealthCheckResult
            {
                Component = "Azure Table Storage",
                IsHealthy = storageResult.IsSuccess,
                Error = storageResult.Error,
                ResponseTime = 0 // Can add timing if needed
            });
        }
        catch (Exception ex)
        {
            healthChecks.Add(new HealthCheckResult
            {
                Component = "Azure Table Storage",
                IsHealthy = false,
                Error = ex.Message,
                ResponseTime = 0
            });
        }

        // Check Internet Connectivity (DNS)
        try
        {
            var dnsResult = await Dns.GetHostAddressesAsync("google.com");
            healthChecks.Add(new HealthCheckResult
            {
                Component = "DNS Resolution",
                IsHealthy = dnsResult.Any(),
                Error = dnsResult.Any() ? null : "No addresses returned",
                ResponseTime = 0
            });
        }
        catch (Exception ex)
        {
            healthChecks.Add(new HealthCheckResult
            {
                Component = "DNS Resolution",
                IsHealthy = false,
                Error = ex.Message,
                ResponseTime = 0
            });
        }

        // Check HTTP Connectivity
        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync("https://www.google.com");
            healthChecks.Add(new HealthCheckResult
            {
                Component = "HTTP Connectivity",
                IsHealthy = response.IsSuccessStatusCode,
                Error = response.IsSuccessStatusCode ? null : $"Status: {response.StatusCode}",
                ResponseTime = 0
            });
        }
        catch (Exception ex)
        {
            healthChecks.Add(new HealthCheckResult
            {
                Component = "HTTP Connectivity",
                IsHealthy = false,
                Error = ex.Message,
                ResponseTime = 0
            });
        }

        var allHealthy = healthChecks.All(h => h.IsHealthy);

        var response = new
        {
            Status = allHealthy ? "Healthy" : "Unhealthy",
            Timestamp = DateTime.UtcNow,
            Checks = healthChecks
        };

        if (!allHealthy)
        {
            _logger.LogWarning("Health check failed. Unhealthy components: {UnhealthyComponents}",
                string.Join(", ", healthChecks.Where(h => !h.IsHealthy).Select(h => h.Component)));
            return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }

        return Ok(response);
    }

    [HttpGet("storage")]
    public async Task<IActionResult> CheckStorage()
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        _logger.LogInformation("Storage check requested from IP: {RemoteIp}", remoteIp);

        try
        {
            var result = await _storageService.CheckConnection();

            if (!result.IsSuccess)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Storage check failed with exception: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                Status = "Unhealthy",
                Error = $"Storage check failed: {ex.Message}",
                Timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("internet")]
    public async Task<IActionResult> CheckInternet()
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        _logger.LogInformation("Internet connectivity check requested from IP: {RemoteIp}", remoteIp);

        try
        {
            // DNS Check
            _logger.LogInformation("Attempting DNS resolution for google.com...");
            var dnsResult = await Dns.GetHostAddressesAsync("google.com");
            if (!dnsResult.Any())
            {
                _logger.LogWarning("DNS resolution failed: No addresses returned");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    Status = "Unhealthy",
                    Error = "DNS resolution failed",
                    Timestamp = DateTime.UtcNow
                });
            }

            // HTTP Check
            _logger.LogInformation("Attempting HTTP request to google.com...");
            using var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://www.google.com");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("HTTP check failed with status code: {StatusCode}", response.StatusCode);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    Status = "Unhealthy",
                    Error = $"HTTP check failed with status code: {response.StatusCode}",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internet check failed with exception: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                Status = "Unhealthy",
                Error = $"Internet check failed: {ex.Message}",
                Timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpPost("log")]
    public IActionResult LogDiagnostics([FromBody] DiagnosticsLog log)
    {
        var clientUrl = Request.Headers["Referer"].ToString();
        _logger.LogInformation("Received diagnostics log from {ClientUrl} at {Timestamp}",
            clientUrl, DateTime.UtcNow);

        foreach (var result in log.Results.Where(r => !r.IsHealthy))
        {
            _logger.LogWarning("Unhealthy diagnostic result: {Component} - {Error}",
                result.Component, result.Error);
        }

        return Ok();
    }
}

public class DiagnosticsLog
{
    public List<DiagnosticResult> Results { get; set; } = new();
}

public class DiagnosticResult
{
    public string Component { get; set; } = "";
    public bool IsHealthy { get; set; }
    public string? Error { get; set; }
}

public class HealthCheckResult
{
    public string Component { get; set; } = "";
    public bool IsHealthy { get; set; }
    public string? Error { get; set; }
    public long ResponseTime { get; set; }
}
