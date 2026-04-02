namespace PoMiniApps.Shared.Models;

public class JudgeDebateResponse
{
    public required string WinnerName { get; set; }
    public required string Reasoning { get; set; }
    public required DebateStats Stats { get; set; }
}
