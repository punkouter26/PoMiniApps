using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoMiniApps.Web.Services.Speech;

namespace PoMiniApps.UnitTests.Services;

public class TextToSpeechServiceTests
{
    [Fact]
    public void Constructor_WithMissingConfig_IsNotConfigured()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var logger = new Mock<ILogger<TextToSpeechService>>();

        var sut = new TextToSpeechService(config, logger.Object);

        sut.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateSpeechAsync_WhenUnconfigured_ThrowsInvalidOperationException()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var logger = new Mock<ILogger<TextToSpeechService>>();
        var sut = new TextToSpeechService(config, logger.Object);

        var act = async () => await sut.GenerateSpeechAsync("hello", "en-GB-RyanNeural", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not configured*");
    }

    [Fact]
    public void Constructor_WithConfig_IsConfigured()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Azure:Speech:Region"] = "eastus",
                ["Azure:Speech:SubscriptionKey"] = "test-key"
            })
            .Build();
        var logger = new Mock<ILogger<TextToSpeechService>>();

        var sut = new TextToSpeechService(config, logger.Object);

        sut.IsConfigured.Should().BeTrue();
    }
}
