using PoConnectFive.Server.Controllers;
using System.Net;

namespace PoConnectFive.Server.Services;

/// <summary>
/// Service implementation for performing health checks on various system components
/// </summary>
public class HealthCheckService : IHealthCheckService
{
    private readonly ILogger<HealthCheckService> _logger;
    private readonly ITableStorageService _storageService;
    private readonly IHttpClientFactory _httpClientFactory;

    public HealthCheckService(
        ILogger<HealthCheckService> logger,
        ITableStorageService storageService,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _storageService = storageService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<HealthCheckResult>> PerformAllHealthChecksAsync()
    {
        var healthChecks = new List<HealthCheckResult>();

        // Check Azure Table Storage
        var storageHealth = await CheckStorageHealthAsync();
        healthChecks.Add(storageHealth);

        // Check DNS Resolution
        var dnsHealth = await CheckDnsResolutionAsync();
        healthChecks.Add(dnsHealth);

        // Check HTTP Connectivity
        var httpHealth = await CheckHttpConnectivityAsync();
        healthChecks.Add(httpHealth);

        return healthChecks;
    }

    public async Task<HealthCheckResult> CheckStorageHealthAsync()
    {
        try
        {
            var storageResult = await _storageService.CheckConnection();
            return new HealthCheckResult
            {
                Component = "Azure Table Storage",
                IsHealthy = storageResult.IsSuccess,
                Error = storageResult.Error,
                ResponseTime = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Storage health check failed");
            return new HealthCheckResult
            {
                Component = "Azure Table Storage",
                IsHealthy = false,
                Error = ex.Message,
                ResponseTime = 0
            };
        }
    }

    public async Task<HealthCheckResult> CheckInternetConnectivityAsync()
    {
        // Check DNS first
        var dnsCheck = await CheckDnsResolutionAsync();
        if (!dnsCheck.IsHealthy)
        {
            return new HealthCheckResult
            {
                Component = "Internet Connectivity",
                IsHealthy = false,
                Error = $"DNS failed: {dnsCheck.Error}",
                ResponseTime = 0
            };
        }

        // Check HTTP
        var httpCheck = await CheckHttpConnectivityAsync();
        if (!httpCheck.IsHealthy)
        {
            return new HealthCheckResult
            {
                Component = "Internet Connectivity",
                IsHealthy = false,
                Error = $"HTTP failed: {httpCheck.Error}",
                ResponseTime = 0
            };
        }

        return new HealthCheckResult
        {
            Component = "Internet Connectivity",
            IsHealthy = true,
            Error = null,
            ResponseTime = 0
        };
    }

    private async Task<HealthCheckResult> CheckDnsResolutionAsync()
    {
        try
        {
            _logger.LogInformation("Attempting DNS resolution for google.com...");
            var dnsResult = await Dns.GetHostAddressesAsync("google.com");

            var isHealthy = dnsResult.Any();
            return new HealthCheckResult
            {
                Component = "DNS Resolution",
                IsHealthy = isHealthy,
                Error = isHealthy ? null : "No addresses returned",
                ResponseTime = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DNS resolution failed");
            return new HealthCheckResult
            {
                Component = "DNS Resolution",
                IsHealthy = false,
                Error = ex.Message,
                ResponseTime = 0
            };
        }
    }

    private async Task<HealthCheckResult> CheckHttpConnectivityAsync()
    {
        try
        {
            _logger.LogInformation("Attempting HTTP request to google.com...");
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync("https://www.google.com");

            var isHealthy = response.IsSuccessStatusCode;
            return new HealthCheckResult
            {
                Component = "HTTP Connectivity",
                IsHealthy = isHealthy,
                Error = isHealthy ? null : $"Status: {response.StatusCode}",
                ResponseTime = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP connectivity check failed");
            return new HealthCheckResult
            {
                Component = "HTTP Connectivity",
                IsHealthy = false,
                Error = ex.Message,
                ResponseTime = 0
            };
        }
    }
}
