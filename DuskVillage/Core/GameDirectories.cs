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

    public static string ItemDefinitionsDirectory => Path.Combine(
        AppContext.BaseDirectory,
        "Data",
        "Items");

    public static string InventoryAssetDefinitionsDirectory => Path.Combine(
        AppContext.BaseDirectory,
        "Data",
        "InventoryAssets");

    public static string WorldAssetDefinitionsDirectory => Path.Combine(
        AppContext.BaseDirectory,
        "Data",
        "WorldAssets");

    public static string ManaSeedFarmerSpriteZipPath => ResolveContentFile(ManaSeedFarmerSpriteZipFileName);

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
        return ResolveContentFile(fileName, ContentRootCandidates);
    }

    public static string ResolveContentFile(string fileName, IEnumerable<string> contentRoots)
    {
        var relativeFileName = string.IsNullOrWhiteSpace(fileName) ? string.Empty : fileName.Trim();
        var normalizedFileName = Path.GetFileName(relativeFileName);
        var roots = (contentRoots ?? [])
            .Where(root => !string.IsNullOrWhiteSpace(root))
            .Select(Path.GetFullPath)
            .ToArray();

        foreach (var root in roots)
        {
            var candidate = FullPath(root, relativeFileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        foreach (var root in roots.Where(Directory.Exists))
        {
            var match = Directory.EnumerateFiles(root, normalizedFileName, SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(match))
            {
                return Path.GetFullPath(match);
            }
        }

        return roots.Length > 0
            ? FullPath(roots[0], relativeFileName)
            : Path.GetFullPath(relativeFileName);
    }

    private static string FullPath(params string[] parts)
    {
        return Path.GetFullPath(Path.Combine(parts));
    }
}
