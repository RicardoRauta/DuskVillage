using System.Collections.Generic;

namespace DuskVillage.Localization;

public interface ILocalizationService
{
    string DefaultLanguageCode { get; }

    string LanguageCode { get; }

    IReadOnlyList<string> AvailableLanguages { get; }

    void SetLanguage(string languageCode);

    string Text(string key, params object[] args);
}
