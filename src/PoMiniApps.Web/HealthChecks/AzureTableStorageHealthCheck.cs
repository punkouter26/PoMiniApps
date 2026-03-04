using Azure.Data.Tables;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PoMiniApps.Web.HealthChecks;

public class AzureTableStorageHealthCheck : IHealthCheck
{
    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(10);

    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureTableStorageHealthCheck> _logger;

    public AzureTableStorageHealthCheck(IConfiguration configuration, ILogger<AzureTableStorageHealthCheck> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Match the configuration keys used in TableStorageService
            var connectionString = _configuration["PoMiniApps:AzureStorageConnectionString"]
                ?? _configuration["PoMiniApps:Azure:StorageConnectionString"]
                ?? _configuration["AzureStorageConnectionString"]
                ?? _configuration["Azure:StorageConnectionString"];

            if (string.IsNullOrWhiteSpace(connectionString))
                return HealthCheckResult.Degraded("Table Storage connection string not configured.");

            var serviceClient = new TableServiceClient(connectionString);

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linkedCts.CancelAfter(ProbeTimeout);

            await foreach (var _ in serviceClient.QueryAsync(cancellationToken: linkedCts.Token))
            {
                break; // Just verify connectivity
            }

            return HealthCheckResult.Healthy("Azure Table Storage is reachable.");
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Table Storage health check timed out after {TimeoutSeconds}s.", ProbeTimeout.TotalSeconds);
            return HealthCheckResult.Unhealthy($"Azure Table Storage probe timed out after {ProbeTimeout.TotalSeconds}s.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Table Storage health check failed.");
            return HealthCheckResult.Unhealthy("Azure Table Storage is not reachable.", ex);
        }
    }
}
