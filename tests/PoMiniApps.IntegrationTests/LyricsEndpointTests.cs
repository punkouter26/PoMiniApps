using System.Net;
using System.Text.Json;

namespace PoMiniApps.IntegrationTests;

[Collection("Integration")]
public class LyricsEndpointTests
{
    private readonly HttpClient _client;

    public LyricsEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task LyricsSongs_Endpoint_ReturnsSongNames()
    {
        var response = await _client.GetAsync("/api/lyrics/songs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        doc.RootElement.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task LyricsByTitle_Endpoint_ReturnsLyricsPayload()
    {
        var response = await _client.GetAsync("/api/lyrics/mock-song");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("lyrics::mock-song");
    }

    [Fact]
    public async Task RandomLyrics_Endpoint_ReturnsLyricsPayload()
    {
        var response = await _client.GetAsync("/api/lyrics/random");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("mock-song");
    }
}
