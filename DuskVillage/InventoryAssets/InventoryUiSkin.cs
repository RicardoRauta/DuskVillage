using System;
using System.Collections.Generic;
using System.Linq;

namespace DuskVillage.InventoryAssets;

public sealed class InventoryUiSkin
{
    private readonly Dictionary<string, InventoryUiAsset> _assetsById;

    public InventoryUiSkin(
        string id,
        string labelKey,
        string variantId,
        string zipFileName,
        string zipPath,
        IReadOnlyList<InventoryUiAsset> assets)
    {
        Id = id;
        LabelKey = labelKey;
        VariantId = variantId;
        ZipFileName = zipFileName;
        ZipPath = zipPath;
        Assets = assets;
        _assetsById = assets.ToDictionary(asset => asset.Id, StringComparer.OrdinalIgnoreCase);
    }

    public string Id { get; }

    public string LabelKey { get; }

    public string VariantId { get; }

    public string ZipFileName { get; }

    public string ZipPath { get; }

    public IReadOnlyList<InventoryUiAsset> Assets { get; }

    public bool IsAvailable => Assets.Any(asset => asset.IsAvailable);

    public InventoryUiAsset FindAsset(string assetId)
    {
        return _assetsById.TryGetValue(assetId ?? string.Empty, out var asset) ? asset : null;
    }
}
