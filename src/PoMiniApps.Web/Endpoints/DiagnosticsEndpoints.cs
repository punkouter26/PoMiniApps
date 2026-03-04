using Microsoft.Extensions.Diagnostics.HealthChecks;
using PoMiniApps.Shared.Models;
using System.Globalization;

namespace PoMiniApps.Web.Endpoints;

/// <summary>
/// Minimal API endpoints for diagnostics.
/// </summary>
public static class DiagnosticsEndpoints
{
    public static IEndpointRouteBuilder MapDiagnosticsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/diagnostics").WithTags("Diagnostics").WithOpenApi();

        group.MapGet("/", async (HealthCheckService healthCheckService, ILogger<Program> logger) =>
        {
            var report = await healthCheckService.CheckHealthAsync();
            var results = report.Entries.Select(entry =>
            {
                var isHealthy = entry.Value.Status == HealthStatus.Healthy;
                if (!isHealthy) logger.LogWarning("{Check} failed: {Status}", entry.Key, entry.Value.Status);
                return new DiagnosticResult
                {
                    CheckName = entry.Key,
                    Success = isHealthy,
                    Message = isHealthy ? $"{entry.Key} is healthy"
                        : $"{entry.Key} failed: {entry.Value.Description ?? entry.Value.Exception?.Message ?? "Unknown error"}"
                };
            }).ToList();

            return Results.Ok(results);
        })
        .WithName("RunDiagnostics")
        .WithSummary("Runs all diagnostics checks and returns results.");

        group.MapGet("/config", (IConfiguration configuration) =>
        {
            var values = configuration
                .AsEnumerable()
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Value))
                .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    entry => entry.Key,
                    entry => ShouldMask(entry.Key)
                        ? MaskMiddle(entry.Value!)
                        : entry.Value!,
                    StringComparer.OrdinalIgnoreCase);

            return Results.Ok(values);
        })
        .WithName("GetDiagnosticsConfiguration")
        .WithSummary("Returns application configuration key/value pairs with sensitive values masked.");

        return endpoints;
    }

    private static bool ShouldMask(string key)
    {
        string[] sensitiveTokens = ["password", "secret", "token", "key", "connectionstring", "clientid", "clientsecret"];

        return sensitiveTokens.Any(token => key.Contains(token, StringComparison.OrdinalIgnoreCase));
    }

    private static string MaskMiddle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (value.Length <= 6)
        {
            return new string('*', value.Length);
        }

        var keep = Math.Max(2, value.Length / 5);
        var maskedLength = Math.Max(0, value.Length - (keep * 2));

        return string.Create(CultureInfo.InvariantCulture, $"{value[..keep]}{new string('*', maskedLength)}{value[^keep..]}");
    }
}
