using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PoMiniApps.Shared.Models;

namespace PoMiniApps.IntegrationTests;

[Collection("Integration")]
public class TranslationEndpointTests
{
    private readonly HttpClient _client;

    public TranslationEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Translate_Endpoint_ReturnsMockedTranslation()
    {
        var response = await _client.PostAsJsonAsync("/api/translation", new TranslationRequest("hello there"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<TranslationResponse>();
        payload.Should().NotBeNull();
        payload!.TranslatedText.Should().Be("Victorian::hello there");
    }

    [Fact]
    public async Task Translate_Endpoint_WithInvalidRequest_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/translation", new TranslationRequest(""));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("errors");
    }
}
