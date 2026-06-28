using System;
using System.IO;
using System.Linq;
using DuskVillage.Characters;

namespace DuskVillage.Saving;

public sealed class FileCharacterPresetStorage : ICharacterPresetStorage
{
    public FileCharacterPresetStorage(string defaultDirectory)
    {
        DefaultDirectory = defaultDirectory;
    }

    public string DefaultDirectory { get; }

    public string CreateDefaultExportPath(CharacterPreset preset)
    {
        Directory.CreateDirectory(DefaultDirectory);
        var fileName = SanitizeFileName(string.IsNullOrWhiteSpace(preset.Name) ? "character" : preset.Name);
        return Path.Combine(DefaultDirectory, $"{fileName}.dvchar.json");
    }

    public CharacterPreset Load(string filePath)
    {
        return CharacterPresetSerializer.Load(filePath);
    }

    public void Save(CharacterPreset preset, string filePath)
    {
        CharacterPresetSerializer.Save(preset, filePath);
    }

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value
            .Trim()
            .Select(character => invalid.Contains(character) ? '_' : character)
            .ToArray();

        var sanitized = new string(chars);
        return string.IsNullOrWhiteSpace(sanitized) ? "character" : sanitized;
    }
}
