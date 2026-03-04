using PoMiniApps.Web.Services.AI;
using PoMiniApps.Web.Services.Speech;
using PoMiniApps.Web.Services.Data;

namespace PoMiniApps.Web.Services.Factories;

/// <summary>
/// Factory interface for creating debate-related services with proper DI scoping.
/// </summary>
public interface IDebateServiceFactory
{
    IDebateServiceScope CreateScope();
}

/// <summary>
/// Scoped set of services needed for a debate turn.
/// </summary>
public interface IDebateServiceScope : IDisposable
{
    IAzureOpenAIService AIService { get; }
    ITextToSpeechService TTSService { get; }
    IRapperRepository RapperRepository { get; }
}
