using System.Net;
using System.Text.Json;

namespace PoMiniApps.IntegrationTests;

[Collection("Integration")]
public class DebateEndpointTests
{
    private readonly HttpClient _client;

    public DebateEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DebateState_Endpoint_ReturnsExpectedShape()
    {
        var response = await _client.GetAsync("/api/debate/state");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("currentTurn", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("isDebateInProgress", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("rapper1", out _).Should().BeTrue();
    }

    [Fact]
    public async Task DebateReset_Endpoint_ReturnsOk()
    {
        var response = await _client.PostAsync("/api/debate/reset", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Debate reset");
    }
}
