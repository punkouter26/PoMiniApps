namespace PoMiniApps.TestShared.Assertions;

/// <summary>
/// Custom assertion helpers for common test scenarios.
/// </summary>
public static class AssertionExtensions
{
    /// <summary>
    /// Asserts that a collection has the expected count of items.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <param name="expectedCount">The expected number of items.</param>
    public static void ShouldHaveCount<T>(this IEnumerable<T> collection, int expectedCount)
    {
        collection.Count().Should().Be(expectedCount);
    }

    /// <summary>
    /// Asserts that a string is not null or empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    public static void ShouldNotBeNullOrEmpty(this string? value)
    {
        value.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Asserts that an object is a specific type.
    /// </summary>
    /// <typeparam name="T">The expected type.</typeparam>
    /// <param name="obj">The object to check.</param>
    public static T ShouldBeOfType<T>(this object? obj) where T : notnull
    {
        return obj.Should().BeOfType<T>().Subject;
    }
}
