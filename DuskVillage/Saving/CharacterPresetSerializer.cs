using System;
using System.IO;
using System.Text.Json;
using DuskVillage.Characters;

namespace DuskVillage.Saving;

public static class CharacterPresetSerializer
{
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static string Serialize(CharacterPreset preset)
    {
        CharacterPresetFactory.EnsureDefaultSkills(preset);
        return JsonSerializer.Serialize(preset, JsonOptions);
    }

    public static CharacterPreset Deserialize(string json)
    {
        var preset = JsonSerializer.Deserialize<CharacterPreset>(json, JsonOptions)
            ?? throw new InvalidDataException("Character preset JSON is empty.");

        Normalize(preset);
        return preset;
    }

    public static CharacterPreset Load(string filePath)
    {
        return Deserialize(File.ReadAllText(filePath));
    }

    public static void Save(CharacterPreset preset, string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(filePath, Serialize(preset));
    }

    public static void Normalize(CharacterPreset preset)
    {
        preset.SchemaVersion = preset.SchemaVersion <= 0 ? CharacterPreset.CurrentSchemaVersion : preset.SchemaVersion;
        preset.PresetId = string.IsNullOrWhiteSpace(preset.PresetId) ? $"character_{Guid.NewGuid():N}" : preset.PresetId.Trim();
        preset.Name = string.IsNullOrWhiteSpace(preset.Name) ? "Alden" : preset.Name.Trim();
        preset.FamilyName = string.IsNullOrWhiteSpace(preset.FamilyName) ? "Vale" : preset.FamilyName.Trim();
        preset.AgeCategoryId = string.IsNullOrWhiteSpace(preset.AgeCategoryId) ? CharacterOptionCatalog.YoungAdult : preset.AgeCategoryId;
        preset.OriginId = string.IsNullOrWhiteSpace(preset.OriginId) ? CharacterOptionCatalog.Newcomer : preset.OriginId;
        preset.BirthdaySeasonId = string.IsNullOrWhiteSpace(preset.BirthdaySeasonId) ? CharacterOptionCatalog.Spring : preset.BirthdaySeasonId;
        preset.BirthdayDay = Math.Clamp(preset.BirthdayDay <= 0 ? 1 : preset.BirthdayDay, CharacterPresetValidator.BirthdayDayMinimum, CharacterPresetValidator.BirthdayDayMaximum);
        preset.MotivationId = string.IsNullOrWhiteSpace(preset.MotivationId) ? CharacterOptionCatalog.MotivationFreshStart : preset.MotivationId;
        preset.Appearance ??= CharacterAppearanceData.CreateDefault();
        preset.Attributes ??= new CharacterAttributeBlock();
        preset.Needs ??= new CharacterNeedsBlock();
        preset.Skills ??= new System.Collections.Generic.List<CharacterSkillState>();
        CharacterPresetFactory.EnsureDefaultSkills(preset);
    }
}
