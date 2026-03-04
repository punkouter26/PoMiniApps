using System.Net;
using System.Net.Http.Json;

namespace PoMiniApps.IntegrationTests;

[Collection("Integration")]
public class SpeechEndpointTests
{
    private readonly HttpClient _client;

    public SpeechEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Synthesize_Endpoint_ReturnsAudioFile()
    {
        var response = await _client.PostAsJsonAsync("/api/speech/synthesize", new { Text = "test speech" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("audio/mpeg");
        (await response.Content.ReadAsByteArrayAsync()).Should().NotBeEmpty();
    }

    [Fact]
    public async Task Synthesize_Endpoint_WithEmptyText_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/speech/synthesize", new { Text = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
