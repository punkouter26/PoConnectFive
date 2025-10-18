using PoConnectFive.Server.Controllers;

namespace PoConnectFive.Server.Services;

/// <summary>
/// Service for performing health checks on various system components
/// </summary>
public interface IHealthCheckService
{
    /// <summary>
    /// Performs comprehensive health checks on all system components
    /// </summary>
    /// <returns>Collection of health check results</returns>
    Task<IEnumerable<HealthCheckResult>> PerformAllHealthChecksAsync();

    /// <summary>
    /// Checks Azure Table Storage connectivity and health
    /// </summary>
    /// <returns>Storage health check result</returns>
    Task<HealthCheckResult> CheckStorageHealthAsync();

    /// <summary>
    /// Checks internet connectivity via DNS and HTTP
    /// </summary>
    /// <returns>Internet connectivity health check result</returns>
    Task<HealthCheckResult> CheckInternetConnectivityAsync();
}
