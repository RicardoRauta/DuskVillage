using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using DuskVillage.Core;

namespace DuskVillage.InventoryAssets;

public sealed class InventoryUiAssetCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private readonly Dictionary<string, InventoryUiSkin> _skinsById;

    private InventoryUiAssetCatalog(IReadOnlyList<InventoryUiSkin> skins)
    {
        Skins = skins;
        _skinsById = skins.ToDictionary(skin => skin.Id, StringComparer.OrdinalIgnoreCase);
    }

    public static InventoryUiAssetCatalog Empty { get; } = new(Array.Empty<InventoryUiSkin>());

    public IReadOnlyList<InventoryUiSkin> Skins { get; }

    public static InventoryUiAssetCatalog LoadFromDirectory(string definitionsDirectory)
    {
        return LoadFromDirectories([definitionsDirectory], GameDirectories.ContentRootCandidates);
    }

    public static InventoryUiAssetCatalog LoadFromDirectories(
        IEnumerable<string> definitionDirectories,
        IEnumerable<string> contentRoots)
    {
        var definitions = new List<InventoryUiSkinDefinition>();
        foreach (var directory in (definitionDirectories ?? []).Where(directory => !string.IsNullOrWhiteSpace(directory)))
        {
            if (!Directory.Exists(directory))
            {
                continue;
            }

            foreach (var filePath in Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                definitions.AddRange(ReadDefinitions(filePath));
            }
        }

        return FromDefinitions(definitions, contentRoots);
    }

    public static InventoryUiAssetCatalog FromDefinitions(
        IEnumerable<InventoryUiSkinDefinition> definitions,
        IEnumerable<string> contentRoots)
    {
        var skins = new List<InventoryUiSkin>();
        var seenSkins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var roots = (contentRoots ?? [])
            .Where(root => !string.IsNullOrWhiteSpace(root))
            .Select(Path.GetFullPath)
            .ToArray();

        foreach (var definition in definitions ?? [])
        {
            ValidateSkin(definition);
            if (!seenSkins.Add(definition.Id))
            {
                throw new InvalidDataException($"Duplicate inventory UI skin id '{definition.Id}'.");
            }

            var zipPath = ResolveZipPath(definition.ZipFileName, roots);
            var availableEntries = ReadEntryNames(zipPath);
            var zipExists = File.Exists(zipPath);
            var assets = new List<InventoryUiAsset>();
            var seenAssets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var asset in definition.Assets)
            {
                ValidateAsset(definition.Id, asset);
                if (!seenAssets.Add(asset.Id))
                {
                    throw new InvalidDataException($"Duplicate inventory UI asset id '{asset.Id}' in skin '{definition.Id}'.");
                }

                assets.Add(new InventoryUiAsset(
                    definition.Id.Trim(),
                    string.IsNullOrWhiteSpace(definition.VariantId) ? "default" : definition.VariantId.Trim(),
                    asset.Id.Trim(),
                    definition.ZipFileName.Trim(),
                    zipPath,
                    NormalizeEntryPath(asset.EntryPath),
                    asset.Width,
                    asset.Height,
                    Math.Max(0, asset.FrameDurationMilliseconds),
                    zipExists,
                    availableEntries.Contains(NormalizeEntryPath(asset.EntryPath))));
            }

            skins.Add(new InventoryUiSkin(
                definition.Id.Trim(),
                definition.LabelKey.Trim(),
                string.IsNullOrWhiteSpace(definition.VariantId) ? "default" : definition.VariantId.Trim(),
                definition.ZipFileName.Trim(),
                zipPath,
                assets));
        }

        skins.Sort((left, right) => string.Compare(left.Id, right.Id, StringComparison.OrdinalIgnoreCase));
        return new InventoryUiAssetCatalog(skins);
    }

    public bool TryGetSkin(string skinId, out InventoryUiSkin skin)
    {
        return _skinsById.TryGetValue(skinId ?? string.Empty, out skin);
    }

    public InventoryUiSkin GetSkinOrFallback(string skinId)
    {
        if (TryGetSkin(skinId, out var skin))
        {
            return skin;
        }

        return TryGetSkin(InventoryUiAssetIds.TravelerBackpackSkin, out var backpack) ? backpack : Skins.FirstOrDefault();
    }

    public InventoryUiAsset FindAsset(string skinId, string assetId)
    {
        return GetSkinOrFallback(skinId)?.FindAsset(assetId);
    }

    private static IReadOnlyList<InventoryUiSkinDefinition> ReadDefinitions(string filePath)
    {
        try
        {
            return JsonSerializer.Deserialize<List<InventoryUiSkinDefinition>>(
                    File.ReadAllText(filePath),
                    JsonOptions) ??
                [];
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"Inventory UI asset definition file '{filePath}' is not valid JSON.", exception);
        }
    }

    private static void ValidateSkin(InventoryUiSkinDefinition definition)
    {
        if (definition == null)
        {
            throw new InvalidDataException("Inventory UI skin definition is null.");
        }

        definition.Id = string.IsNullOrWhiteSpace(definition.Id) ? string.Empty : definition.Id.Trim();
        definition.LabelKey = string.IsNullOrWhiteSpace(definition.LabelKey) ? definition.Id : definition.LabelKey.Trim();
        definition.ZipFileName = string.IsNullOrWhiteSpace(definition.ZipFileName) ? string.Empty : definition.ZipFileName.Trim();
        definition.Assets ??= [];

        if (string.IsNullOrWhiteSpace(definition.Id))
        {
            throw new InvalidDataException("Inventory UI skin is missing an id.");
        }

        if (string.IsNullOrWhiteSpace(definition.ZipFileName))
        {
            throw new InvalidDataException($"Inventory UI skin '{definition.Id}' is missing a zip file name.");
        }

        if (definition.Assets.Count == 0)
        {
            throw new InvalidDataException($"Inventory UI skin '{definition.Id}' must define at least one asset.");
        }
    }

    private static void ValidateAsset(string skinId, InventoryUiAssetDefinition asset)
    {
        if (asset == null)
        {
            throw new InvalidDataException($"Inventory UI skin '{skinId}' has a null asset.");
        }

        asset.Id = string.IsNullOrWhiteSpace(asset.Id) ? string.Empty : asset.Id.Trim();
        asset.EntryPath = NormalizeEntryPath(asset.EntryPath);

        if (string.IsNullOrWhiteSpace(asset.Id))
        {
            throw new InvalidDataException($"Inventory UI skin '{skinId}' has an asset with no id.");
        }

        if (string.IsNullOrWhiteSpace(asset.EntryPath))
        {
            throw new InvalidDataException($"Inventory UI asset '{skinId}/{asset.Id}' is missing a zip entry path.");
        }

        if (asset.Width <= 0 || asset.Height <= 0)
        {
            throw new InvalidDataException($"Inventory UI asset '{skinId}/{asset.Id}' has invalid dimensions.");
        }
    }

    private static string ResolveZipPath(string zipFileName, IReadOnlyList<string> contentRoots)
    {
        foreach (var root in contentRoots)
        {
            var candidate = Path.GetFullPath(Path.Combine(root, zipFileName));
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return GameDirectories.ResolveContentFile(zipFileName, contentRoots);
    }

    private static HashSet<string> ReadEntryNames(string zipPath)
    {
        var entries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(zipPath))
        {
            return entries;
        }

        try
        {
            using var archive = ZipFile.OpenRead(zipPath);
            foreach (var entry in archive.Entries)
            {
                if (entry.Length > 0)
                {
                    entries.Add(NormalizeEntryPath(entry.FullName));
                }
            }
        }
        catch (IOException)
        {
        }
        catch (InvalidDataException)
        {
        }

        return entries;
    }

    private static string NormalizeEntryPath(string entryPath)
    {
        return string.IsNullOrWhiteSpace(entryPath)
            ? string.Empty
            : entryPath.Trim().Replace('\\', '/');
    }
}
