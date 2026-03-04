namespace PoMiniApps.Web.Services.Translation;

public interface ITranslationCache
{
    bool TryGetTranslation(string modernText, out string? victorianText);
    void CacheTranslation(string modernText, string victorianText);
}
