using Azure.Data.Tables;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PoConnectFive.Server.Features.Health;

/// <summary>
/// Health check for Azure Table Storage connectivity.
/// Validates connection to the table storage service.
/// </summary>
public class TableStorageHealthCheck : IHealthCheck
{
    private readonly TableServiceClient _tableServiceClient;
    private readonly ILogger<TableStorageHealthCheck> _logger;

    public TableStorageHealthCheck(
        TableServiceClient tableServiceClient,
        ILogger<TableStorageHealthCheck> logger)
    {
        _tableServiceClient = tableServiceClient ?? throw new ArgumentNullException(nameof(tableServiceClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Attempt to get account info to verify connectivity
            var accountInfo = await _tableServiceClient.GetPropertiesAsync(cancellationToken);

            return HealthCheckResult.Healthy(
                "Azure Table Storage is accessible",
                new Dictionary<string, object>
                {
                    { "accountName", _tableServiceClient.AccountName }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure Table Storage health check failed");

            return HealthCheckResult.Unhealthy(
                "Azure Table Storage is not accessible",
                ex,
                new Dictionary<string, object>
                {
                    { "accountName", _tableServiceClient.AccountName }
                });
        }
    }
}
