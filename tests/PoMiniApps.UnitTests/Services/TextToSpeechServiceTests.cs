using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PoMiniApps.Web.Configuration;
using PoMiniApps.Web.Services.Speech;

namespace PoMiniApps.UnitTests.Services;

public class TextToSpeechServiceTests
{
    private static IOptions<ApiSettings> Settings(string region = "", string key = "") =>
        Options.Create(new ApiSettings { AzureSpeechRegion = region, AzureSpeechSubscriptionKey = key });

    [Fact]
    public void Constructor_WithMissingConfig_IsNotConfigured()
    {
        var logger = new Mock<ILogger<TextToSpeechService>>();

        var sut = new TextToSpeechService(Settings(), logger.Object);

        sut.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateSpeechAsync_WhenUnconfigured_ThrowsInvalidOperationException()
    {
        var logger = new Mock<ILogger<TextToSpeechService>>();
        var sut = new TextToSpeechService(Settings(), logger.Object);

        var act = async () => await sut.GenerateSpeechAsync("hello", "en-GB-RyanNeural", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not configured*");
    }

    [Fact]
    public void Constructor_WithConfig_IsConfigured()
    {
        var logger = new Mock<ILogger<TextToSpeechService>>();

        var sut = new TextToSpeechService(Settings("eastus", "test-key"), logger.Object);

        sut.IsConfigured.Should().BeTrue();
    }
}
