using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace DuskVillage.Localization;

public sealed class LocalizationService : ILocalizationService
{
    private readonly Dictionary<string, Dictionary<string, string>> _tables = new(StringComparer.OrdinalIgnoreCase);

    public LocalizationService(string localizationDirectory, string initialLanguageCode)
    {
        DefaultLanguageCode = "en";
        Load(localizationDirectory);
        SetLanguage(initialLanguageCode);
    }

    public string DefaultLanguageCode { get; }

    public string LanguageCode { get; private set; } = "en";

    public IReadOnlyList<string> AvailableLanguages => new List<string>(_tables.Keys);

    public void SetLanguage(string languageCode)
    {
        if (!string.IsNullOrWhiteSpace(languageCode) && _tables.ContainsKey(languageCode))
        {
            LanguageCode = languageCode;
            return;
        }

        LanguageCode = DefaultLanguageCode;
    }

    public string Text(string key, params object[] args)
    {
        var template = Resolve(key);
        if (args.Length == 0)
        {
            return template;
        }

        return string.Format(CultureInfo.InvariantCulture, template, args);
    }

    private void Load(string localizationDirectory)
    {
        if (!Directory.Exists(localizationDirectory))
        {
            _tables[DefaultLanguageCode] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            return;
        }

        foreach (var filePath in Directory.GetFiles(localizationDirectory, "*.json", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var table = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (table == null)
                {
                    continue;
                }

                var languageCode = Path.GetFileNameWithoutExtension(filePath);
                _tables[languageCode] = new Dictionary<string, string>(table, StringComparer.OrdinalIgnoreCase);
            }
            catch (JsonException)
            {
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        if (!_tables.ContainsKey(DefaultLanguageCode))
        {
            _tables[DefaultLanguageCode] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private string Resolve(string key)
    {
        if (_tables.TryGetValue(LanguageCode, out var current) && current.TryGetValue(key, out var text))
        {
            return text;
        }

        if (_tables.TryGetValue(DefaultLanguageCode, out var fallback) && fallback.TryGetValue(key, out text))
        {
            return text;
        }

        return key;
    }
}
