using PoMiniApps.Shared.Models;
using System.Text;

namespace PoMiniApps.Web.Factories;

/// <summary>
/// Factory for creating initial DebateState instances.
/// </summary>
public static class DebateStateFactory
{
    public static DebateState CreateEmpty() => new()
    {
        Rapper1 = new Rapper(),
        Rapper2 = new Rapper(),
        Topic = new Topic(),
        DebateTranscript = new StringBuilder(),
        CurrentTurnText = string.Empty,
        ErrorMessage = string.Empty
    };

    public static DebateState CreateForNewDebate(Rapper rapper1, Rapper rapper2, Topic topic, int maxTurns)
    {
        int totalRounds = maxTurns / 2;
        // Intro is shown separately in the UI, not as a turn
        return new DebateState
        {
            Rapper1 = rapper1,
            Rapper2 = rapper2,
            Topic = topic,
            IsDebateInProgress = true,
            CurrentTurn = 0,
            IsRapper1Turn = true,
            DebateTranscript = new StringBuilder(),
            CurrentTurnText = string.Empty,
            ErrorMessage = string.Empty
        };
    }
}
