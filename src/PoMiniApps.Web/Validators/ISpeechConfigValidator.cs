using PoMiniApps.Web.Configuration;

namespace PoMiniApps.Web.Validators;

public interface ISpeechConfigValidator
{
    bool IsValid(ApiSettings settings);
    string GetValidationError(ApiSettings settings);
}
