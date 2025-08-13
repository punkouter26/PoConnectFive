using Microsoft.AspNetCore.Mvc;
using PoConnectFive.Server.Services;
using System.Net;
using System.Net.Http;

namespace PoConnectFive.Server.Controllers;

[ApiController]
[Route("/healthz")]
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
    public IActionResult CheckHealth()
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        _logger.LogInformation("Health check requested from IP: {RemoteIp}", remoteIp);

        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
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
