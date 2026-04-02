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

        // Main diagnostics endpoint that returns health check results
        group.MapGet("", async (IServiceProvider services, IConfiguration configuration, ILogger<Program> logger) =>
        {
            var results = new List<DiagnosticResult>();

            // Check system health
            try
            {
                results.Add(new DiagnosticResult
                {
                    CheckName = "API Health",
                    Category = "System",
                    Success = true,
                    Message = "API server responding normally"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "API health check failed");
                results.Add(new DiagnosticResult
                {
                    CheckName = "API Health",
                    Category = "System",
                    Success = false,
                    Message = $"API health check failed: {ex.Message}"
                });
            }

            return Results.Ok(results);
        })
        .WithName("GetDiagnostics")
        .WithSummary("Returns all diagnostic health check results.");

        group.MapGet("/config", (IConfiguration configuration, IHostEnvironment env) =>
        {
            // Only allow in Development environment to prevent exposing secrets
            if (!env.IsDevelopment())
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

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
        .WithSummary("Returns application configuration key/value pairs with sensitive values masked (Development only).");

        return endpoints;
    }

    /// <summary>
    /// Diagnostic result model for health checks.
    /// </summary>
    private class DiagnosticResult
    {
        public string CheckName { get; set; } = "";
        public string Category { get; set; } = "Other";
        public bool Success { get; set; }
        public bool IsWarning { get; set; }
        public string Message { get; set; } = "";
    }

    private static bool ShouldMask(string key)
    {
        string[] sensitiveTokens = ["password", "secret", "token", "key", "connectionstring", "clientid", "clientsecret", "pat", "credential", "privatekey", "apikey"];

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
