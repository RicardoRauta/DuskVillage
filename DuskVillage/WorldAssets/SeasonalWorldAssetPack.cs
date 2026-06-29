using System;
using System.Collections.Generic;
using System.Linq;

namespace DuskVillage.WorldAssets;

public sealed class SeasonalWorldAssetPack
{
    private readonly Dictionary<string, SeasonalWorldAsset> _assetsById;

    public SeasonalWorldAssetPack(
        string seasonId,
        string variantId,
        string zipFileName,
        string zipPath,
        IReadOnlyList<SeasonalWorldAsset> assets)
    {
        SeasonId = seasonId;
        VariantId = variantId;
        ZipFileName = zipFileName;
        ZipPath = zipPath;
        Assets = assets;
        _assetsById = assets.ToDictionary(asset => asset.Id, StringComparer.OrdinalIgnoreCase);
    }

    public string SeasonId { get; }

    public string VariantId { get; }

    public string ZipFileName { get; }

    public string ZipPath { get; }

    public IReadOnlyList<SeasonalWorldAsset> Assets { get; }

    public bool IsAvailable => Assets.Any(asset => asset.IsAvailable);

    public SeasonalWorldAsset FindAsset(string assetId)
    {
        return !string.IsNullOrWhiteSpace(assetId) && _assetsById.TryGetValue(assetId, out var asset)
            ? asset
            : null;
    }
}
