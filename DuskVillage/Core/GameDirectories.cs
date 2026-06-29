using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuskVillage.Core;

public static class GameDirectories
{
    private const string AppFolderName = "DuskVillage";
    private const string ContentFolderName = "Content";
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

    public static string WorldAssetDefinitionsDirectory => Path.Combine(
        AppContext.BaseDirectory,
        "Data",
        "WorldAssets");

    public static string ManaSeedFarmerSpriteZipPath =>
        ManaSeedFarmerSpriteZipCandidates.FirstOrDefault(File.Exists) ??
        ManaSeedFarmerSpriteZipCandidates[0];

    public static IReadOnlyList<string> ContentRootCandidates { get; } =
    [
        FullPath(AppContext.BaseDirectory, ContentFolderName),
        FullPath(AppContext.BaseDirectory, "..", "..", "..", ContentFolderName),
        FullPath(Environment.CurrentDirectory, "DuskVillage", ContentFolderName),
        FullPath(Environment.CurrentDirectory, ContentFolderName)
    ];

    public static IReadOnlyList<string> ManaSeedFarmerSpriteZipCandidates { get; } =
    [
        FullPath(AppContext.BaseDirectory, ContentFolderName, ManaSeedFarmerSpriteZipFileName),
        FullPath(AppContext.BaseDirectory, "..", "..", "..", ContentFolderName, ManaSeedFarmerSpriteZipFileName),
        FullPath(Environment.CurrentDirectory, "DuskVillage", ContentFolderName, ManaSeedFarmerSpriteZipFileName),
        FullPath(Environment.CurrentDirectory, ContentFolderName, ManaSeedFarmerSpriteZipFileName)
    ];

    public static string ResolveContentFile(string fileName)
    {
        return ContentRootCandidates
            .Select(root => FullPath(root, fileName))
            .FirstOrDefault(File.Exists) ??
            FullPath(ContentRootCandidates[0], fileName);
    }

    private static string FullPath(params string[] parts)
    {
        return Path.GetFullPath(Path.Combine(parts));
    }
}
