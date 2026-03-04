using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PoMiniApps.Web.Configuration;
using PoMiniApps.Web.Services.AudioSynthesis;
using PoMiniApps.Web.Validators;

namespace PoMiniApps.UnitTests.Services;

public class AudioSynthesisServiceTests
{
    [Fact]
    public async Task SynthesizeSpeechAsync_WithInvalidConfig_ThrowsInvalidOperationException()
    {
        var options = Options.Create(new ApiSettings());
        var validator = new Mock<ISpeechConfigValidator>();
        validator.Setup(v => v.IsValid(It.IsAny<ApiSettings>())).Returns(false);
        validator.Setup(v => v.GetValidationError(It.IsAny<ApiSettings>())).Returns("missing config");
        var httpClientFactory = new Mock<IHttpClientFactory>();
        var logger = new Mock<ILogger<AudioSynthesisService>>();

        var sut = new AudioSynthesisService(options, validator.Object, httpClientFactory.Object, logger.Object, TimeProvider.System);

        var act = async () => await sut.SynthesizeSpeechAsync("hello");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*missing config*");
    }

    [Fact]
    public async Task SynthesizeSpeechAsync_WithValidConfig_ReturnsAudioBytes_AndCachesToken()
    {
        var settings = new ApiSettings
        {
            AzureSpeechRegion = "eastus",
            AzureSpeechSubscriptionKey = "test-key"
        };
        var options = Options.Create(settings);

        var validator = new Mock<ISpeechConfigValidator>();
        validator.Setup(v => v.IsValid(It.IsAny<ApiSettings>())).Returns(true);

        var handler = new SequenceHttpMessageHandler();
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(f => f.CreateClient("SpeechToken"))
            .Returns(() => new HttpClient(handler, disposeHandler: false));
        httpClientFactory
            .Setup(f => f.CreateClient("SpeechSynthesis"))
            .Returns(() => new HttpClient(handler, disposeHandler: false));

        var logger = new Mock<ILogger<AudioSynthesisService>>();
        var sut = new AudioSynthesisService(options, validator.Object, httpClientFactory.Object, logger.Object, TimeProvider.System);

        var first = await sut.SynthesizeSpeechAsync("first");
        var second = await sut.SynthesizeSpeechAsync("second");

        first.Should().Equal(new byte[] { 9, 8, 7 });
        second.Should().Equal(new byte[] { 9, 8, 7 });
        handler.TokenRequests.Should().Be(1);
        handler.SynthesisRequests.Should().Be(2);
    }

    private sealed class SequenceHttpMessageHandler : HttpMessageHandler
    {
        public int TokenRequests { get; private set; }
        public int SynthesisRequests { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri?.AbsoluteUri.Contains("issueToken", StringComparison.OrdinalIgnoreCase) == true)
            {
                TokenRequests++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("mock-token")
                });
            }

            if (request.RequestUri?.AbsoluteUri.Contains("cognitiveservices/v1", StringComparison.OrdinalIgnoreCase) == true)
            {
                SynthesisRequests++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(new byte[] { 9, 8, 7 })
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}
