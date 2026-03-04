using PoMiniApps.Web.Services.AI;
using PoMiniApps.Web.Services.Speech;
using PoMiniApps.Web.Services.Data;

namespace PoMiniApps.Web.Services.Factories;

/// <summary>
/// Factory implementation using IServiceProvider for scoped service creation.
/// </summary>
public class DebateServiceFactory : IDebateServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    public DebateServiceFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;
    public IDebateServiceScope CreateScope() => new DebateServiceScope(_serviceProvider.CreateScope());
}

internal class DebateServiceScope : IDebateServiceScope
{
    private readonly IServiceScope _scope;
    public DebateServiceScope(IServiceScope scope)
    {
        _scope = scope;
        AIService = _scope.ServiceProvider.GetRequiredService<IAzureOpenAIService>();
        TTSService = _scope.ServiceProvider.GetRequiredService<ITextToSpeechService>();
        RapperRepository = _scope.ServiceProvider.GetRequiredService<IRapperRepository>();
    }
    public IAzureOpenAIService AIService { get; }
    public ITextToSpeechService TTSService { get; }
    public IRapperRepository RapperRepository { get; }
    public void Dispose() => _scope?.Dispose();
}
