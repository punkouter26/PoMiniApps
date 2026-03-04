using PoMiniApps.Shared.Models;

namespace PoMiniApps.Web.Extensions;

/// <summary>
/// Maps topic categories to display-friendly data.
/// </summary>
public static class TopicMapperExtensions
{
    private static readonly Dictionary<string, (string Emoji, string Color)> CategoryMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Technology"] = ("💻", "#4FC3F7"),
        ["Politics"] = ("🏛️", "#EF5350"),
        ["Sports"] = ("⚽", "#66BB6A"),
        ["Entertainment"] = ("🎬", "#AB47BC"),
        ["Science"] = ("🔬", "#26C6DA"),
        ["Food"] = ("🍔", "#FFA726"),
        ["Philosophy"] = ("🤔", "#78909C"),
        ["Music"] = ("🎵", "#EC407A"),
        ["Gaming"] = ("🎮", "#7E57C2"),
        ["Environment"] = ("🌍", "#43A047")
    };

    public static string GetEmoji(this Topic topic) =>
        CategoryMap.TryGetValue(topic.Category ?? "", out var data) ? data.Emoji : "🎤";

    public static string GetColor(this Topic topic) =>
        CategoryMap.TryGetValue(topic.Category ?? "", out var data) ? data.Color : "#9E9E9E";

    public static List<Topic> GetDefaultTopics() =>
    [
        new() { Title = "AI: Friend or Foe?", Category = "Technology", Description = "Will AI help or hinder humanity?" },
        new() { Title = "Pineapple on Pizza", Category = "Food", Description = "The eternal debate." },
        new() { Title = "Tabs vs Spaces", Category = "Technology", Description = "The coder's dilemma." },
        new() { Title = "Cats vs Dogs", Category = "Entertainment", Description = "The ultimate pet showdown." },
        new() { Title = "Remote vs Office", Category = "Philosophy", Description = "Where is work best done?" },
        new() { Title = "PC vs Console", Category = "Gaming", Description = "The gaming platform war." },
        new() { Title = "Summer vs Winter", Category = "Environment", Description = "Which season reigns supreme?" },
        new() { Title = "Books vs Movies", Category = "Entertainment", Description = "Story consumption battle." }
    ];
}
