using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DuskVillage.Settings;

public sealed class GameSettingsService : IGameSettingsService
{
    private readonly string _settingsFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public GameSettingsService(string settingsFilePath)
    {
        _settingsFilePath = settingsFilePath;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public GameSettings Current { get; private set; } = GameSettings.CreateDefault();

    public void Load()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                Current = GameSettings.CreateDefault();
                return;
            }

            var json = File.ReadAllText(_settingsFilePath);
            Current = JsonSerializer.Deserialize<GameSettings>(json, _jsonOptions) ?? GameSettings.CreateDefault();
            Current.Normalize();
        }
        catch (JsonException)
        {
            Current = GameSettings.CreateDefault();
        }
        catch (IOException)
        {
            Current = GameSettings.CreateDefault();
        }
        catch (UnauthorizedAccessException)
        {
            Current = GameSettings.CreateDefault();
        }
    }

    public void Save(GameSettings settings)
    {
        Current = settings.Clone();
        Current.Normalize();

        var directory = Path.GetDirectoryName(_settingsFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(Current, _jsonOptions);
        File.WriteAllText(_settingsFilePath, json);
    }
}
