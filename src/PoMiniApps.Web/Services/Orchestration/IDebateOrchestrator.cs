using PoMiniApps.Shared.Models;

namespace PoMiniApps.Web.Services.Orchestration;

public interface IDebateOrchestrator
{
    DebateState CurrentState { get; }
    Task StartNewDebateAsync(Rapper rapper1, Rapper rapper2, Topic topic);
    Task SignalAudioPlaybackCompleteAsync();
    void ResetDebate();
}
