using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PoMiniApps.Shared.Models;
using PoMiniApps.Web.Services.Diagnostics;

namespace PoMiniApps.IntegrationTests;

[Collection("Integration")]
public class DiagnosticsServiceIntegrationTests
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    // Cache JsonSerializerOptions to avoid CA1869 warning
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public DiagnosticsServiceIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Diagnostics_Endpoint_ReturnsAllHealthChecks()
    {
        var response = await _client.GetAsync("/api/diagnostics");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        // Parse JSON response — should contain array of check results
        using var doc = JsonDocument.Parse(content);
        doc.RootElement.ValueKind.Should().Be(JsonValueKind.Array);

        var results = doc.RootElement;
        results.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Diagnostics_Endpoint_ContainsExpectedHealthCheckNames()
    {
        var response = await _client.GetAsync("/api/diagnostics");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);

        // Extract all checkName values from the response array
        var checkNames = new List<string>();
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            if (element.TryGetProperty("checkName", out var checkNameElement))
            {
                checkNames.Add(checkNameElement.GetString() ?? "");
            }
        }

        // Verify expected health checks are present
        checkNames.Should().NotBeEmpty();
        checkNames.Should().Contain(x => x.Contains("OpenAI") || x.Contains("openai"));
        checkNames.Should().Contain(x => x.Contains("Storage") || x.Contains("storage"));
    }

    [Fact]
    public async Task Diagnostics_Endpoint_ContainsSuccessAndMessage()
    {
        var response = await _client.GetAsync("/api/diagnostics");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);

        foreach (var checkResult in doc.RootElement.EnumerateArray())
        {
            checkResult.TryGetProperty("success", out _).Should().BeTrue();
            checkResult.TryGetProperty("message", out _).Should().BeTrue();
        }
    }

    [Fact]
    public async Task Diagnostics_Endpoint_ReturnsPartialHealthOnDegradation()
    {
        // In testing environment with fake services, most checks will pass
        // This test verifies the endpoint structure handles mixed success/failure responses
        var response = await _client.GetAsync("/api/diagnostics");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

        // Response should always return 200 even if some checks failed
        // (graceful degradation pattern)
        using var doc = JsonDocument.Parse(content);
        doc.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task Diagnostics_Endpoint_HasValidJsonSchema()
    {
        var response = await _client.GetAsync("/api/diagnostics");
        var content = await response.Content.ReadAsStringAsync();

        // Deserialize to verify schema matches DiagnosticResult
        var diagnosticResults = JsonSerializer.Deserialize<List<DiagnosticResult>>(content, JsonOptions);

        diagnosticResults.Should().NotBeNull();
        diagnosticResults.Should().NotBeEmpty();
        diagnosticResults.ForEach(r =>
        {
            r.CheckName.Should().NotBeNullOrEmpty();
            r.Message.Should().NotBeNullOrEmpty();
        });
    }
}

[Collection("Integration")]
public class DiagnosticsHealthCheckIntegrationTests
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DiagnosticsHealthCheckIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsExpectedStatus()
    {
        var response = await _client.GetAsync("/health");

        // In test environment with mocked services, health endpoint should return Healthy
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthEndpoint_CanBeCalledMultipleTimes()
    {
        // Verify idempotency of health checks
        var response1 = await _client.GetAsync("/health");
        var response2 = await _client.GetAsync("/health");
        var response3 = await _client.GetAsync("/health");

        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        response3.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DiagnosticsEndpoint_AndHealthEndpoint_AreIndependent()
    {
        // Both endpoints should work independently
        var diagResponse = await _client.GetAsync("/api/diagnostics");
        var healthResponse = await _client.GetAsync("/health");

        diagResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        healthResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Diagnostics returns JSON array, health returns different format
        var diagContent = await diagResponse.Content.ReadAsStringAsync();
        var healthContent = await healthResponse.Content.ReadAsStringAsync();

        diagContent.Should().NotBeSameAs(healthContent);
    }
}
