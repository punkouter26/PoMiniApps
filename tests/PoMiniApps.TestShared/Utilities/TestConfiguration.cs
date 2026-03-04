namespace PoMiniApps.TestShared.Utilities;

/// <summary>
/// Utilities for test configuration and setup.
/// </summary>
public static class TestConfiguration
{
    /// <summary>
    /// Gets the standard test timeout in milliseconds.
    /// </summary>
    public static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets the default retry interval for timing-sensitive tests.
    /// </summary>
    public static readonly TimeSpan RetryInterval = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Gets the base URL for integration tests.
    /// </summary>
    public static readonly string BaseUrl = "http://localhost:5000";

    /// <summary>
    /// Retries a function until it succeeds or timeout is reached.
    /// </summary>
    /// <param name="action">The action to retry.</param>
    /// <param name="timeout">The maximum time to retry.</param>
    /// <param name="interval">The interval between retries.</param>
    public static async Task RetryAsync(
        Func<Task> action,
        TimeSpan? timeout = null,
        TimeSpan? interval = null)
    {
        timeout ??= TestTimeout;
        interval ??= RetryInterval;

        var endTime = DateTime.UtcNow.Add(timeout.Value);

        while (DateTime.UtcNow < endTime)
        {
            try
            {
                await action();
                return;
            }
            catch
            {
                await Task.Delay(interval.Value);
            }
        }

        throw new TimeoutException($"Retry operation timed out after {timeout.Value.TotalSeconds}s");
    }
}
