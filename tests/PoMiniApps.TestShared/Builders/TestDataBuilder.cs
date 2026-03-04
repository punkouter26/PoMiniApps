namespace PoMiniApps.TestShared.Builders;

/// <summary>
/// Provides builder patterns for constructing common test objects.
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// Creates a default mock HTTP client for testing API calls.
    /// </summary>
    /// <returns>A configured HttpClient instance.</returns>
    public static HttpClient BuildHttpClient()
    {
        return new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
    }

    /// <summary>
    /// Creates a mock service collection for dependency injection testing.
    /// </summary>
    /// <returns>An IServiceCollection instance.</returns>
    public static IServiceCollection BuildServiceCollection()
    {
        var services = new ServiceCollection();
        return services;
    }
}
