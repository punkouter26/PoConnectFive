using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PoConnectFive.Server.Services;
using PoConnectFive.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace PoConnectFive.Tests.Integration;

public class HealthControllerIntegrationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;

    public HealthControllerIntegrationTests()
    {
        _factory = new CustomWebApplicationFactory();
    }

    public void Dispose()
    {
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetHealth_AllServicesHealthy_ReturnsOk()
    {
        // Arrange
        var mockHealthService = new Mock<IHealthCheckService>();

        mockHealthService
            .Setup(s => s.PerformAllHealthChecksAsync())
            .ReturnsAsync(new List<PoConnectFive.Server.Controllers.HealthCheckResult>
            {
                new() { Component = "Azure Table Storage", IsHealthy = true, Error = null, ResponseTime = 0 },
                new() { Component = "DNS Resolution", IsHealthy = true, Error = null, ResponseTime = 0 },
                new() { Component = "HTTP Connectivity", IsHealthy = true, Error = null, ResponseTime = 0 }
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IHealthCheckService));
                if (descriptor != null)
                    services.Remove(descriptor);
                services.AddSingleton<IHealthCheckService>(mockHealthService.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"status\":\"Healthy\"", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetHealth_StorageUnhealthy_ReturnsServiceUnavailable()
    {
        // Arrange
        var mockHealthService = new Mock<IHealthCheckService>();

        mockHealthService
            .Setup(s => s.PerformAllHealthChecksAsync())
            .ReturnsAsync(new List<PoConnectFive.Server.Controllers.HealthCheckResult>
            {
                new() { Component = "Azure Table Storage", IsHealthy = false, Error = "Connection failed", ResponseTime = 0 },
                new() { Component = "DNS Resolution", IsHealthy = true, Error = null, ResponseTime = 0 },
                new() { Component = "HTTP Connectivity", IsHealthy = true, Error = null, ResponseTime = 0 }
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IHealthCheckService));
                if (descriptor != null)
                    services.Remove(descriptor);
                services.AddSingleton<IHealthCheckService>(mockHealthService.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/health");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"status\":\"Unhealthy\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Azure Table Storage", content);
    }

    [Fact]
    public async Task GetHealth_MultipleServicesUnhealthy_ReturnsServiceUnavailable()
    {
        // Arrange
        var mockHealthService = new Mock<IHealthCheckService>();

        mockHealthService
            .Setup(s => s.PerformAllHealthChecksAsync())
            .ReturnsAsync(new List<PoConnectFive.Server.Controllers.HealthCheckResult>
            {
                new() { Component = "Azure Table Storage", IsHealthy = false, Error = "Connection timeout", ResponseTime = 0 },
                new() { Component = "DNS Resolution", IsHealthy = false, Error = "DNS lookup failed", ResponseTime = 0 },
                new() { Component = "HTTP Connectivity", IsHealthy = true, Error = null, ResponseTime = 0 }
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IHealthCheckService));
                if (descriptor != null)
                    services.Remove(descriptor);
                services.AddSingleton<IHealthCheckService>(mockHealthService.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/health");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task CheckStorage_Healthy_ReturnsOk()
    {
        // Arrange
        var mockHealthService = new Mock<IHealthCheckService>();

        mockHealthService
            .Setup(s => s.CheckStorageHealthAsync())
            .ReturnsAsync(new PoConnectFive.Server.Controllers.HealthCheckResult
            {
                Component = "Azure Table Storage",
                IsHealthy = true,
                Error = null,
                ResponseTime = 0
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IHealthCheckService));
                if (descriptor != null)
                    services.Remove(descriptor);
                services.AddSingleton<IHealthCheckService>(mockHealthService.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/health/checks/storage");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"status\":\"Healthy\"", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CheckStorage_Unhealthy_ReturnsServiceUnavailable()
    {
        // Arrange
        var mockHealthService = new Mock<IHealthCheckService>();

        mockHealthService
            .Setup(s => s.CheckStorageHealthAsync())
            .ReturnsAsync(new PoConnectFive.Server.Controllers.HealthCheckResult
            {
                Component = "Azure Table Storage",
                IsHealthy = false,
                Error = "Table service unavailable",
                ResponseTime = 0
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IHealthCheckService));
                if (descriptor != null)
                    services.Remove(descriptor);
                services.AddSingleton<IHealthCheckService>(mockHealthService.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/health/checks/storage");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"status\":\"Unhealthy\"", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Table service unavailable", content);
    }

    [Fact]
    public async Task CheckInternet_Healthy_ReturnsOk()
    {
        // Arrange
        var mockHealthService = new Mock<IHealthCheckService>();

        mockHealthService
            .Setup(s => s.CheckInternetConnectivityAsync())
            .ReturnsAsync(new PoConnectFive.Server.Controllers.HealthCheckResult
            {
                Component = "Internet Connectivity",
                IsHealthy = true,
                Error = null,
                ResponseTime = 0
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IHealthCheckService));
                if (descriptor != null)
                    services.Remove(descriptor);
                services.AddSingleton<IHealthCheckService>(mockHealthService.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/health/checks/internet");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CheckInternet_Unhealthy_ReturnsServiceUnavailable()
    {
        // Arrange
        var mockHealthService = new Mock<IHealthCheckService>();

        mockHealthService
            .Setup(s => s.CheckInternetConnectivityAsync())
            .ReturnsAsync(new PoConnectFive.Server.Controllers.HealthCheckResult
            {
                Component = "Internet Connectivity",
                IsHealthy = false,
                Error = "DNS failed: No addresses returned",
                ResponseTime = 0
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IHealthCheckService));
                if (descriptor != null)
                    services.Remove(descriptor);
                services.AddSingleton<IHealthCheckService>(mockHealthService.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/health/checks/internet");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"status\":\"Unhealthy\"", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PostDiagnostics_ValidLog_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        var diagnosticsLog = new
        {
            Results = new[]
            {
                new { Component = "Client Storage", IsHealthy = true, Error = (string?)null },
                new { Component = "Client API", IsHealthy = false, Error = (string?)"Connection timeout" }
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/diagnostics", diagnosticsLog);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
