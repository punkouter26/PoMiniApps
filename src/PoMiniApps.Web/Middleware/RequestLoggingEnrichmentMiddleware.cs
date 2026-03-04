using Serilog.Context;

namespace PoMiniApps.Web.Middleware;

/// <summary>
/// Enriches Serilog log entries with request-scoped correlation properties.
/// </summary>
public class RequestLoggingEnrichmentMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingEnrichmentMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.TraceIdentifier;
        var sessionId = context.Session?.Id ?? context.Connection.Id ?? "no-session";
        var userId = context.User?.Identity?.IsAuthenticated == true
            ? context.User.Identity?.Name ?? "authenticated-user"
            : "anonymous";

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("SessionId", sessionId))
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("UserAgent", context.Request.Headers.UserAgent.ToString()))
        {
            await _next(context);
        }
    }
}
