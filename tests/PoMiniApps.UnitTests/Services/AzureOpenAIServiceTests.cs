using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PoMiniApps.Shared.Models;
using PoMiniApps.Web.Configuration;
using PoMiniApps.Web.Services.AI;

namespace PoMiniApps.UnitTests.Services;

public class AzureOpenAIServiceTests
{
    private readonly Mock<ILogger<AzureOpenAIService>> _loggerMock;

    public AzureOpenAIServiceTests()
    {
        _loggerMock = new Mock<ILogger<AzureOpenAIService>>();
    }

    private static IOptions<ApiSettings> BuildOptions(string endpoint = "", string apiKey = "", string deploymentName = "gpt-4o")
        => Options.Create(new ApiSettings
        {
            AzureOpenAIEndpoint = endpoint,
            AzureOpenAIApiKey = apiKey,
            AzureOpenAIDeploymentName = deploymentName
        });

    [Fact]
    public void Constructor_WithMissingEndpoint_MarksServiceAsUnconfigured()
    {
        // Act
        var service = new AzureOpenAIService(BuildOptions(endpoint: "", apiKey: "test-key"), _loggerMock.Object);

        // Assert
        service.IsConfigured.Should().BeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithMissingApiKey_MarksServiceAsUnconfigured()
    {
        // Act
        var service = new AzureOpenAIService(
            BuildOptions(endpoint: "https://test.openai.azure.com/", apiKey: ""), _loggerMock.Object);

        // Assert
        service.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithValidConfiguration_MarksServiceAsConfigured()
    {
        // Act
        var service = new AzureOpenAIService(
            BuildOptions("https://test.openai.azure.com/", "test-api-key"), _loggerMock.Object);

        // Assert
        service.IsConfigured.Should().BeTrue();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("initialized with endpoint")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithMissingDeploymentName_UsesDefaultGpt4o()
    {
        // Act
        var service = new AzureOpenAIService(
            BuildOptions("https://test.openai.azure.com/", "test-api-key", deploymentName: ""), _loggerMock.Object);

        // Assert
        service.IsConfigured.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateDebateTurnAsync_WithUnconfiguredService_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = new AzureOpenAIService(BuildOptions(), _loggerMock.Object);

        // Act
        var act = async () => await service.GenerateDebateTurnAsync("Test prompt", 100, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Azure OpenAI service is not configured.");
    }

    [Fact]
    public async Task JudgeDebateAsync_WithUnconfiguredService_ThrowsInvalidOperationException()
    {
        // Arrange
        var service = new AzureOpenAIService(BuildOptions(), _loggerMock.Object);

        // Act
        var act = async () => await service.JudgeDebateAsync(
            "transcript", "Rapper1", "Rapper2", "topic", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Azure OpenAI service is not configured.");
    }

    [Fact]
    public void IsConfigured_WhenEndpointAndKeyAreSet_ReturnsTrue()
    {
        // Act
        var service = new AzureOpenAIService(
            BuildOptions("https://test.openai.azure.com/", "valid-key"), _loggerMock.Object);

        // Assert
        service.IsConfigured.Should().BeTrue();
    }

    [Fact]
    public void IsConfigured_WhenEndpointIsMissing_ReturnsFalse()
    {
        // Act
        var service = new AzureOpenAIService(BuildOptions(endpoint: "", apiKey: "valid-key"), _loggerMock.Object);

        // Assert
        service.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public void IsConfigured_WhenApiKeyIsMissing_ReturnsFalse()
    {
        // Act
        var service = new AzureOpenAIService(
            BuildOptions("https://test.openai.azure.com/", apiKey: ""), _loggerMock.Object);

        // Assert
        service.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public void Constructor_LogsWarningWhenNotConfigured()
    {
        // Act
        var service = new AzureOpenAIService(BuildOptions(), _loggerMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("AI features will be unavailable")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithNullConfiguration_MarksAsUnconfigured()
    {
        // Act
        var service = new AzureOpenAIService(BuildOptions(), _loggerMock.Object);

        // Assert
        service.IsConfigured.Should().BeFalse();
    }
}

