namespace PoMiniApps.TestShared.Factories;

/// <summary>
/// Provides common mock factory methods for testing.
/// </summary>
public static class MockFactory
{
    /// <summary>
    /// Creates a mock HTTP client factory for testing.
    /// </summary>
    /// <returns>A configured mock HTTP client factory.</returns>
    public static Mock<IHttpClientFactory> CreateMockHttpClientFactory()
    {
        var mockFactory = new Mock<IHttpClientFactory>();
        var mockClient = new HttpClient();

        mockFactory
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(mockClient);

        return mockFactory;
    }

    /// <summary>
    /// Creates a mock service provider for dependency injection testing.
    /// </summary>
    /// <returns>A configured mock service provider.</returns>
    public static Mock<IServiceProvider> CreateMockServiceProvider()
    {
        return new Mock<IServiceProvider>();
    }

    /// <summary>
    /// Creates a mock logger factory.
    /// </summary>
    /// <returns>A configured mock logger factory.</returns>
    public static Mock<ILoggerFactory> CreateMockLoggerFactory()
    {
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();

        mockLoggerFactory
            .Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(mockLogger.Object);

        return mockLoggerFactory;
    }
}
