using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PoMiniApps.Shared.Models;
using PoMiniApps.Web.Services.AI;
using PoMiniApps.Web.Services.AudioSynthesis;
using PoMiniApps.Web.Services.Lyrics;
using PoMiniApps.Web.Services.News;
using PoMiniApps.Web.Services.Translation;

namespace PoMiniApps.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Azure:StorageConnectionString"] = "UseDevelopmentStorage=true",
                ["Azure:KeyVaultName"] = "",
                ["Azure:AzureOpenAIEndpoint"] = "",
                ["Azure:AzureOpenAIDeploymentName"] = "",
                ["Azure:AzureOpenAIApiKey"] = "",
                ["Azure:AzureSpeechSubscriptionKey"] = "",
                ["Azure:AzureSpeechRegion"] = "",
                ["Azure:ApplicationInsightsConnectionString"] = ""
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAzureOpenAIService>();
            services.RemoveAll<ITranslationService>();
            services.RemoveAll<AudioSynthesisService>();
            services.RemoveAll<NewsService>();
            services.RemoveAll<LyricsService>();

            services.AddSingleton<IAzureOpenAIService, FakeAzureOpenAIService>();
            services.AddSingleton<ITranslationService, FakeTranslationService>();
            services.AddSingleton<AudioSynthesisService, FakeAudioSynthesisService>();
            services.AddSingleton<NewsService, FakeNewsService>();
            services.AddSingleton<LyricsService, FakeLyricsService>();
        });
    }

    private sealed class FakeAzureOpenAIService : IAzureOpenAIService
    {
        public bool IsConfigured => true;

        public Task<string> GenerateDebateTurnAsync(string prompt, int maxTokens, CancellationToken cancellationToken)
            => Task.FromResult($"mock-turn: {prompt}");

        public Task<JudgeDebateResponse> JudgeDebateAsync(string debateTranscript, string rapper1Name, string rapper2Name, string topic, CancellationToken cancellationToken)
            => Task.FromResult(new JudgeDebateResponse
            {
                WinnerName = rapper1Name,
                Reasoning = "Deterministic mock judge result for tests.",
                Stats = new DebateStats()
            });
    }

    private sealed class FakeTranslationService : ITranslationService
    {
        public Task<string> TranslateToVictorianEnglishAsync(string modernText, CancellationToken cancellationToken = default)
            => Task.FromResult($"Victorian::{modernText}");
    }

    private sealed class FakeAudioSynthesisService : AudioSynthesisService
    {
        public FakeAudioSynthesisService() : base(null!, null!, null!, null!, TimeProvider.System) { }
        public override Task<byte[]> SynthesizeSpeechAsync(string text, CancellationToken cancellationToken = default)
            => Task.FromResult(new byte[] { 1, 2, 3, 4 });
    }

    private sealed class FakeNewsService : NewsService
    {
        public FakeNewsService() : base(null!, null!, null!) { }
        public override Task<List<NewsHeadline>> GetTopHeadlinesAsync(int count)
            => Task.FromResult(new List<NewsHeadline>
            {
                new()
                {
                    Title = "Mock topic headline",
                    Description = "Mock topic description"
                }
            });
    }

    private sealed class FakeLyricsService : LyricsService
    {
        public FakeLyricsService() : base(null!, null!) { }
        public override Task<List<string>> GetAvailableSongsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new List<string> { "mock-song" });

        public override Task<string?> GetLyricsAsync(string songTitle, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>($"lyrics::{songTitle}");

        public override Task<(string? Title, string? Lyrics)> GetRandomLyricsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<(string? Title, string? Lyrics)>(("mock-song", "lyrics::mock-song"));
    }
}
