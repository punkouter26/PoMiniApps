using Microsoft.Extensions.Diagnostics.HealthChecks;
using PoMiniApps.Web.Services.Speech;

namespace PoMiniApps.Web.HealthChecks;

public class TextToSpeechHealthCheck : IHealthCheck
{
    private readonly ITextToSpeechService _service;
    public TextToSpeechHealthCheck(ITextToSpeechService service) => _service = service;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_service.IsConfigured
            ? HealthCheckResult.Healthy("TTS (Speech SDK) is configured.")
            : HealthCheckResult.Degraded("TTS (Speech SDK) is not configured."));
    }
}
