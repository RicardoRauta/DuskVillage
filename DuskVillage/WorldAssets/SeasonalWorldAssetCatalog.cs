using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using DuskVillage.Core;
using DuskVillage.World;

namespace DuskVillage.WorldAssets;

public sealed class SeasonalWorldAssetCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private readonly Dictionary<string, SeasonalWorldAssetPack> _packsBySeason;

    private SeasonalWorldAssetCatalog(IReadOnlyList<SeasonalWorldAssetPack> packs)
    {
        Packs = packs;
        _packsBySeason = packs.ToDictionary(pack => pack.SeasonId, StringComparer.OrdinalIgnoreCase);
    }

    public static SeasonalWorldAssetCatalog Empty { get; } = new(Array.Empty<SeasonalWorldAssetPack>());

    public IReadOnlyList<SeasonalWorldAssetPack> Packs { get; }

    public static SeasonalWorldAssetCatalog LoadFromDirectory(string definitionsDirectory)
    {
        return LoadFromDirectories([definitionsDirectory], GameDirectories.ContentRootCandidates);
    }

    public static SeasonalWorldAssetCatalog LoadFromDirectories(
        IEnumerable<string> definitionDirectories,
        IEnumerable<string> contentRoots)
    {
        var definitions = new List<SeasonalWorldAssetPackDefinition>();
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

    public static SeasonalWorldAssetCatalog FromDefinitions(
        IEnumerable<SeasonalWorldAssetPackDefinition> definitions,
        IEnumerable<string> contentRoots)
    {
        var packs = new List<SeasonalWorldAssetPack>();
        var seenSeasons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var roots = (contentRoots ?? [])
            .Where(root => !string.IsNullOrWhiteSpace(root))
            .Select(Path.GetFullPath)
            .ToArray();

        foreach (var definition in definitions ?? [])
        {
            ValidatePack(definition);
            if (!seenSeasons.Add(definition.SeasonId))
            {
                throw new InvalidDataException($"Duplicate seasonal asset pack for season '{definition.SeasonId}'.");
            }

            var zipPath = ResolveZipPath(definition.ZipFileName, roots);
            var availableEntries = ReadEntryNames(zipPath);
            var zipExists = File.Exists(zipPath);
            var assets = new List<SeasonalWorldAsset>();
            var seenAssets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var asset in definition.Assets)
            {
                ValidateAsset(definition.SeasonId, asset);
                if (!seenAssets.Add(asset.Id))
                {
                    throw new InvalidDataException($"Duplicate seasonal asset id '{asset.Id}' in season '{definition.SeasonId}'.");
                }

                assets.Add(new SeasonalWorldAsset(
                    definition.SeasonId,
                    string.IsNullOrWhiteSpace(definition.VariantId) ? "default" : definition.VariantId.Trim(),
                    asset.Id.Trim(),
                    definition.ZipFileName.Trim(),
                    zipPath,
                    NormalizeEntryPath(asset.EntryPath),
                    asset.TileWidth,
                    asset.TileHeight,
                    Math.Max(0, asset.FrameDurationMilliseconds),
                    zipExists,
                    availableEntries.Contains(NormalizeEntryPath(asset.EntryPath))));
            }

            packs.Add(new SeasonalWorldAssetPack(
                definition.SeasonId,
                string.IsNullOrWhiteSpace(definition.VariantId) ? "default" : definition.VariantId.Trim(),
                definition.ZipFileName.Trim(),
                zipPath,
                assets));
        }

        packs.Sort((left, right) => SeasonIndex(left.SeasonId).CompareTo(SeasonIndex(right.SeasonId)));
        return new SeasonalWorldAssetCatalog(packs);
    }

    public bool TryGetPack(string seasonId, out SeasonalWorldAssetPack pack)
    {
        return _packsBySeason.TryGetValue(seasonId ?? string.Empty, out pack);
    }

    public SeasonalWorldAssetPack GetPackOrFallback(string seasonId)
    {
        if (TryGetPack(seasonId, out var pack))
        {
            return pack;
        }

        return TryGetPack(WorldCalendarRules.Spring, out var spring) ? spring : Packs.FirstOrDefault();
    }

    public SeasonalWorldAsset FindAsset(string seasonId, string assetId)
    {
        return TryGetPack(seasonId, out var pack) ? pack.FindAsset(assetId) : null;
    }

    private static IReadOnlyList<SeasonalWorldAssetPackDefinition> ReadDefinitions(string filePath)
    {
        try
        {
            return JsonSerializer.Deserialize<List<SeasonalWorldAssetPackDefinition>>(
                    File.ReadAllText(filePath),
                    JsonOptions) ??
                [];
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"Seasonal world asset definition file '{filePath}' is not valid JSON.", exception);
        }
    }

    private static void ValidatePack(SeasonalWorldAssetPackDefinition definition)
    {
        if (definition == null)
        {
            throw new InvalidDataException("Seasonal world asset definition is null.");
        }

        definition.SeasonId = string.IsNullOrWhiteSpace(definition.SeasonId) ? string.Empty : definition.SeasonId.Trim();
        definition.ZipFileName = string.IsNullOrWhiteSpace(definition.ZipFileName) ? string.Empty : definition.ZipFileName.Trim();
        definition.Assets ??= [];

        if (!WorldCalendarRules.Seasons.Contains(definition.SeasonId, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidDataException($"Seasonal world asset pack has unknown season '{definition.SeasonId}'.");
        }

        if (string.IsNullOrWhiteSpace(definition.ZipFileName))
        {
            throw new InvalidDataException($"Seasonal world asset pack '{definition.SeasonId}' is missing a zip file name.");
        }

        if (definition.Assets.Count == 0)
        {
            throw new InvalidDataException($"Seasonal world asset pack '{definition.SeasonId}' must define at least one asset.");
        }
    }

    private static void ValidateAsset(string seasonId, SeasonalWorldAssetDefinition asset)
    {
        if (asset == null)
        {
            throw new InvalidDataException($"Seasonal world asset pack '{seasonId}' has a null asset.");
        }

        asset.Id = string.IsNullOrWhiteSpace(asset.Id) ? string.Empty : asset.Id.Trim();
        asset.EntryPath = NormalizeEntryPath(asset.EntryPath);

        if (string.IsNullOrWhiteSpace(asset.Id))
        {
            throw new InvalidDataException($"Seasonal world asset pack '{seasonId}' has an asset with no id.");
        }

        if (string.IsNullOrWhiteSpace(asset.EntryPath))
        {
            throw new InvalidDataException($"Seasonal world asset '{seasonId}/{asset.Id}' is missing a zip entry path.");
        }

        if (asset.TileWidth <= 0 || asset.TileHeight <= 0)
        {
            throw new InvalidDataException($"Seasonal world asset '{seasonId}/{asset.Id}' has invalid tile dimensions.");
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

        return contentRoots.Count > 0
            ? Path.GetFullPath(Path.Combine(contentRoots[0], zipFileName))
            : GameDirectories.ResolveContentFile(zipFileName);
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

    private static int SeasonIndex(string seasonId)
    {
        for (var i = 0; i < WorldCalendarRules.Seasons.Count; i++)
        {
            if (WorldCalendarRules.Seasons[i].Equals(seasonId, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return int.MaxValue;
    }
}
