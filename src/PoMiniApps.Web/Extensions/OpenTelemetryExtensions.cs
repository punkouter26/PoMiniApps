using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace PoMiniApps.Web.Extensions;

/// <summary>
/// Configures OpenTelemetry tracing and metrics.
/// Azure Monitor integration is handled via Application Insights SDK.
/// </summary>
public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddPoMiniGamesOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var poSharedConnectionString =
            configuration["PoMiniApps:Azure:PoShared:ApplicationInsights:ConnectionString"]
            ?? configuration["PoMiniApps:APPLICATIONINSIGHTS_CONNECTION_STRING"]
            ?? configuration["Azure:PoShared:ApplicationInsights:ConnectionString"]
            ?? configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("PoMiniApps"))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                if (!string.IsNullOrWhiteSpace(poSharedConnectionString))
                {
                    tracing.AddAzureMonitorTraceExporter(options =>
                    {
                        options.ConnectionString = poSharedConnectionString;
                    });
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (!string.IsNullOrWhiteSpace(poSharedConnectionString))
                {
                    metrics.AddAzureMonitorMetricExporter(options =>
                    {
                        options.ConnectionString = poSharedConnectionString;
                    });
                }
            });

        return services;
    }
}
