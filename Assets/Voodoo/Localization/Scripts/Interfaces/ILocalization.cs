using System;

namespace Voodoo.Distribution
{
    public interface ILocalization
    {
        void SetLanguage(string langCode);

        // takes a string id and returns the localized version of the string.
        string GetTranslation(string key, string languageCode = null, bool logError = true);

        event Action OnLanguageChange;
    }
}