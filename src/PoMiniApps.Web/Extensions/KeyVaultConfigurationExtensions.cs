using Azure.Identity;

namespace PoMiniApps.Web.Extensions;

/// <summary>
/// Adds Azure Key Vault as a configuration source using DefaultAzureCredential.
/// Fails gracefully if Key Vault is not accessible.
/// </summary>
public static class KeyVaultConfigurationExtensions
{
    public static IConfigurationBuilder AddPoMiniGamesKeyVault(this IConfigurationBuilder builder, IConfiguration currentConfig)
    {
        // Temporarily skip Key Vault in non-Development environments due to managed identity timeouts
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment != "Development")
        {
            Console.WriteLine("[INFO] Key Vault disabled in production - using app settings/environment variables");
            return builder;
        }

        var keyVaultName =
            currentConfig["PoMiniApps:Azure:KeyVault:Name"]
            ?? currentConfig["Azure:KeyVault:Name"]
            ?? currentConfig["Azure:KeyVaultName"];
        if (!string.IsNullOrWhiteSpace(keyVaultName))
        {
            try
            {
                var kvUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
                builder.AddAzureKeyVault(kvUri, new DefaultAzureCredential());
                Console.WriteLine($"[INFO] Connected to Key Vault: {keyVaultName}");
            }
            catch (Exception ex)
            {
                // Log but don't crash — app continues with local/env config
                Console.WriteLine($"[WARN] Key Vault '{keyVaultName}' not accessible: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("[INFO] No Key Vault configured (Azure:KeyVault:Name not set)");
        }
        return builder;
    }
}
