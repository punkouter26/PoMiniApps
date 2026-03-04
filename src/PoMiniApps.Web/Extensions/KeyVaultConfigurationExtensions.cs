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
        var keyVaultName =
            currentConfig["PoMiniApps:Azure:KeyVault:Name"]
            ?? currentConfig["Azure:KeyVault:Name"]
            ?? currentConfig["Azure:KeyVaultName"];
        
        // Skip Key Vault in production/Azure - use environment variables and app settings instead
        // This avoids managed identity timeout issues
        if (string.IsNullOrWhiteSpace(keyVaultName) || !IsLocalDevelopment())
        {
            if (!IsLocalDevelopment())
            {
                Console.WriteLine("[INFO] Key Vault skipped in Azure - using app settings and environment variables");
            }
            return builder;
        }

        // Only load Key Vault in local development
        try
        {
            var kvUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
            builder.AddAzureKeyVault(kvUri, new DefaultAzureCredential());
            Console.WriteLine($"[INFO] Connected to Key Vault: {keyVaultName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARN] Key Vault '{keyVaultName}' not accessible: {ex.Message}");
        }

        return builder;
    }

    private static bool IsLocalDevelopment()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return env == "Development" || string.IsNullOrEmpty(env) && !IsRunningInAzure();
    }

    private static bool IsRunningInAzure()
    {
        return Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") != null;
    }
}
