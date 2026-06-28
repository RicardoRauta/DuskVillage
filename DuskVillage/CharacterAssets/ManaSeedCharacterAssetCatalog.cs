using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using DuskVillage.Characters;
using DuskVillage.UI;

namespace DuskVillage.CharacterAssets;

public sealed class ManaSeedCharacterAssetCatalog
{
    private readonly Dictionary<string, List<ManaSeedCharacterAsset>> _assetsBySlot = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ManaSeedCharacterAsset> _assetsById = new(StringComparer.OrdinalIgnoreCase);

    private ManaSeedCharacterAssetCatalog(string zipPath, bool isAvailable)
    {
        ZipPath = zipPath;
        IsAvailable = isAvailable;
    }

    public string ZipPath { get; }

    public bool IsAvailable { get; }

    public IReadOnlyList<ManaSeedCharacterSlot> Slots { get; } = ManaSeedCharacterSlot.CreateDefaultSlots();

    public static ManaSeedCharacterAssetCatalog Load(string zipPath)
    {
        var catalog = new ManaSeedCharacterAssetCatalog(zipPath, File.Exists(zipPath));
        if (!catalog.IsAvailable)
        {
            catalog.AddFallbackBody();
            return catalog;
        }

        using var archive = ZipFile.OpenRead(zipPath);
        foreach (var entry in archive.Entries)
        {
            if (!entry.FullName.StartsWith("farmer_base_sheets/", StringComparison.OrdinalIgnoreCase) ||
                !entry.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                entry.FullName.EndsWith("fbas_XXfldr_blank_sheet.png", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!ManaSeedAssetNameParser.TryParse(entry.FullName, out var parsed) ||
                !CharacterAppearanceSlotIds.IsKnown(parsed.SlotId))
            {
                continue;
            }

            var asset = new ManaSeedCharacterAsset(
                parsed.SlotId,
                parsed.AssetId,
                parsed.DisplayName,
                entry.FullName,
                parsed.SlotId == CharacterAppearanceSlotIds.Head && parsed.SpecialId == "e",
                parsed.SlotId == CharacterAppearanceSlotIds.Hair && parsed.SpecialId == "e");

            catalog.Add(asset);
        }

        catalog.EnsureRequiredFallbacks();
        catalog.Sort();
        return catalog;
    }

    public IReadOnlyList<ManaSeedCharacterAsset> GetAssets(string slotId)
    {
        return _assetsBySlot.TryGetValue(slotId, out var assets) ? assets : Array.Empty<ManaSeedCharacterAsset>();
    }

    public IReadOnlyList<SelectorOption> GetSelectorOptions(ManaSeedCharacterSlot slot)
    {
        var options = new List<SelectorOption>();
        if (!slot.IsRequired)
        {
            options.Add(new SelectorOption(CharacterAppearanceData.NoneAssetId, "character.appearance.none"));
        }

        foreach (var asset in GetAssets(slot.SlotId))
        {
            options.Add(new SelectorOption(asset.AssetId, asset.DisplayName));
        }

        return options;
    }

    public ManaSeedCharacterAsset FindAsset(string assetId)
    {
        return !string.IsNullOrWhiteSpace(assetId) && _assetsById.TryGetValue(assetId, out var asset)
            ? asset
            : null;
    }

    private void Add(ManaSeedCharacterAsset asset)
    {
        if (!_assetsBySlot.TryGetValue(asset.SlotId, out var assets))
        {
            assets = new List<ManaSeedCharacterAsset>();
            _assetsBySlot[asset.SlotId] = assets;
        }

        assets.Add(asset);
        _assetsById[asset.AssetId] = asset;
    }

    private void AddFallbackBody()
    {
        Add(new ManaSeedCharacterAsset(
            CharacterAppearanceSlotIds.Body,
            "fbas_01body_human_00",
            "Human",
            string.Empty,
            hidesHair: false,
            hidesWhenHeadwearEquipped: false));
    }

    private void EnsureRequiredFallbacks()
    {
        if (GetAssets(CharacterAppearanceSlotIds.Body).Count == 0)
        {
            AddFallbackBody();
        }
    }

    private void Sort()
    {
        foreach (var assets in _assetsBySlot.Values)
        {
            assets.Sort((left, right) => string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
