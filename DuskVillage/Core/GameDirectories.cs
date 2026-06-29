using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuskVillage.Core;

public static class GameDirectories
{
    private const string AppFolderName = "DuskVillage";
    private const string ManaSeedFarmerSpriteZipFileName = "22.10a - Mana Seed Farmer Sprite System v1.6.zip";

    public const string GameVersion = "0.1.0";

    public static string UserDataRoot { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppFolderName);

    public static string SettingsFilePath => Path.Combine(UserDataRoot, "settings.json");

    public static string SavesDirectory => Path.Combine(UserDataRoot, "Saves");

    public static string CharacterPresetsDirectory => Path.Combine(UserDataRoot, "CharacterPresets");

    public static string LocalizationDirectory => Path.Combine(
        AppContext.BaseDirectory,
        "Data",
        "Localization");

    public static string ActionDefinitionsDirectory => Path.Combine(
        AppContext.BaseDirectory,
        "Data",
        "Actions");

    public static string ManaSeedFarmerSpriteZipPath =>
        ManaSeedFarmerSpriteZipCandidates.FirstOrDefault(File.Exists) ??
        ManaSeedFarmerSpriteZipCandidates[0];

    public static IReadOnlyList<string> ManaSeedFarmerSpriteZipCandidates { get; } =
    [
        FullPath(AppContext.BaseDirectory, "Content", ManaSeedFarmerSpriteZipFileName),
        FullPath(AppContext.BaseDirectory, "..", "..", "..", "Content", ManaSeedFarmerSpriteZipFileName),
        FullPath(Environment.CurrentDirectory, "DuskVillage", "Content", ManaSeedFarmerSpriteZipFileName),
        FullPath(Environment.CurrentDirectory, "Content", ManaSeedFarmerSpriteZipFileName)
    ];

    private static string FullPath(params string[] parts)
    {
        return Path.GetFullPath(Path.Combine(parts));
    }
}
