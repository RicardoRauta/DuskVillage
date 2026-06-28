using System;
using System.Collections.Generic;

namespace DuskVillage.Characters;

public sealed class CharacterAppearanceData
{
    public const string NoneAssetId = "none";
    public const string DefaultPaletteId = "default";

    public Dictionary<string, string> Layers { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> Palettes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public string GetLayer(string slotId)
    {
        return Layers.TryGetValue(slotId, out var assetId) && !string.IsNullOrWhiteSpace(assetId)
            ? assetId
            : NoneAssetId;
    }

    public void SetLayer(string slotId, string assetId)
    {
        if (!CharacterAppearanceSlotIds.IsKnown(slotId))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(assetId) || assetId.Equals(NoneAssetId, StringComparison.OrdinalIgnoreCase))
        {
            Layers.Remove(slotId);
            return;
        }

        Layers[slotId] = assetId;
    }

    public string GetPalette(string slotId)
    {
        return Palettes.TryGetValue(slotId, out var paletteId) && !string.IsNullOrWhiteSpace(paletteId)
            ? paletteId
            : DefaultPaletteId;
    }

    public void SetPalette(string slotId, string paletteId)
    {
        if (!CharacterAppearanceSlotIds.IsKnown(slotId))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(paletteId) || paletteId.Equals(DefaultPaletteId, StringComparison.OrdinalIgnoreCase))
        {
            Palettes.Remove(slotId);
            return;
        }

        Palettes[slotId] = paletteId;
    }

    public CharacterAppearanceData Clone()
    {
        return new CharacterAppearanceData
        {
            Layers = new Dictionary<string, string>(Layers, StringComparer.OrdinalIgnoreCase),
            Palettes = new Dictionary<string, string>(Palettes, StringComparer.OrdinalIgnoreCase)
        };
    }

    public static CharacterAppearanceData CreateDefault()
    {
        var appearance = new CharacterAppearanceData();
        appearance.SetLayer(CharacterAppearanceSlotIds.Body, "fbas_01body_human_00");
        appearance.SetLayer(CharacterAppearanceSlotIds.FootwearLow, "fbas_03fot1_boots_00a");
        appearance.SetLayer(CharacterAppearanceSlotIds.LowerOne, "fbas_04lwr1_longpants_00a");
        appearance.SetLayer(CharacterAppearanceSlotIds.Shirt, "fbas_05shrt_shortshirt_00a");
        appearance.SetLayer(CharacterAppearanceSlotIds.Hair, "fbas_13hair_dapper_00");
        appearance.SetPalette(CharacterAppearanceSlotIds.Body, "skin_warm");
        appearance.SetPalette(CharacterAppearanceSlotIds.FootwearLow, "cloth_rust");
        appearance.SetPalette(CharacterAppearanceSlotIds.LowerOne, "cloth_charcoal");
        appearance.SetPalette(CharacterAppearanceSlotIds.Shirt, "cloth_linen");
        appearance.SetPalette(CharacterAppearanceSlotIds.Hair, "hair_brown");
        return appearance;
    }
}
