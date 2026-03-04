using PoMiniApps.Shared.Models;

namespace PoMiniApps.Web.Services.Diagnostics;

public interface IDiagnosticsService
{
    Task<List<DiagnosticResult>> RunAllChecksAsync();
}
