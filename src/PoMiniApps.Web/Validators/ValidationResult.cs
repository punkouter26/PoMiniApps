namespace PoMiniApps.Web.Validators;

/// <summary>
/// Simple validation result pattern for input validation.
/// </summary>
public sealed record ValidationResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    private ValidationResult(bool isSuccess, string? errorMessage = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Success() => new(true);
    public static ValidationResult Failure(string errorMessage) => new(false, errorMessage);
}
