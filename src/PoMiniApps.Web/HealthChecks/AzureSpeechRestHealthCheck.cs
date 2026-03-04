using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using PoMiniApps.Web.Configuration;
using PoMiniApps.Web.Validators;

namespace PoMiniApps.Web.HealthChecks;

public class AzureSpeechRestHealthCheck : IHealthCheck
{
    private readonly ApiSettings _settings;
    private readonly SpeechConfigValidator _validator;

    public AzureSpeechRestHealthCheck(IOptions<ApiSettings> settings, SpeechConfigValidator validator)
    {
        _settings = settings.Value;
        _validator = validator;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_validator.IsValid(_settings)
            ? HealthCheckResult.Healthy("Azure Speech REST API is configured.")
            : HealthCheckResult.Degraded($"Azure Speech REST API not configured: {_validator.GetValidationError(_settings)}"));
    }
}
