using System.Net;
using System.Text.Json;

namespace PoMiniApps.IntegrationTests;

[Collection("Integration")]
public class TopicsEndpointTests
{
    private readonly HttpClient _client;

    public TopicsEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Topics_Endpoint_ReturnsTopicsList()
    {
        var response = await _client.GetAsync("/api/topics");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        doc.RootElement.GetArrayLength().Should().BeGreaterThan(0);
        doc.RootElement[0].TryGetProperty("title", out _).Should().BeTrue();
    }
}
