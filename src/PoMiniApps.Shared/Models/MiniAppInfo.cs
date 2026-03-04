namespace PoMiniApps.Shared.Models;

public record MiniAppInfo(
    string Id,
    string Name,
    string Description,
    string Icon,
    string Route,
    string[] Tags,
    string Color = "#1976d2"
);

public static class MiniApps
{
    public static readonly MiniAppInfo[] All = [
        new MiniAppInfo(
            Id: "rap-battle",
            Name: "Rap Battle Arena",
            Description: "Watch legendary rappers face off in AI-generated rap battles. Pick your rappers, choose a topic from the latest news, and let the AI deliver fire bars with real-time text-to-speech delivery.",
            Icon: "🎙️",
            Route: "/apps/lingual/rap-battle",
            Tags: ["AZURE OPENAI", "SPEECH", "SIGNALR", "AI JUDGING"],
            Color: "#e53935"
        ),
        new MiniAppInfo(
            Id: "victorian-translator",
            Name: "Victorian Translator",
            Description: "Transform modern English into elegant Victorian-era prose. Load Cockney rhyming slang lyrics, type your own text, or generate random content — then hear it spoken aloud in a proper British accent.",
            Icon: "🎩",
            Route: "/apps/lingual/victorian-translator",
            Tags: ["AI TRANSLATION", "LYRICS LIBRARY", "BRITISH TTS"],
            Color: "#764ba2"
        ),
        // Future mini apps can be added here
        // new MiniAppInfo(...),
    ];

    public static MiniAppInfo? GetById(string id) => All.FirstOrDefault(a => a.Id == id);
}
