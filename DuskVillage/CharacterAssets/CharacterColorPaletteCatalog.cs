using System.Collections.Generic;
using DuskVillage.Characters;
using DuskVillage.UI;
using Microsoft.Xna.Framework;

namespace DuskVillage.CharacterAssets;

public static class CharacterColorPaletteCatalog
{
    private static readonly IReadOnlyList<CharacterColorPalette> SkinPalettes =
    [
        new(CharacterAppearanceData.DefaultPaletteId, "character.color.default", Color.White),
        new("skin_pale", "character.color.skin_pale", new Color(255, 232, 207)),
        new("skin_warm", "character.color.skin_warm", new Color(238, 181, 132)),
        new("skin_olive", "character.color.skin_olive", new Color(196, 153, 103)),
        new("skin_brown", "character.color.skin_brown", new Color(142, 91, 61)),
        new("skin_deep", "character.color.skin_deep", new Color(88, 55, 44))
    ];

    private static readonly IReadOnlyList<CharacterColorPalette> HairPalettes =
    [
        new(CharacterAppearanceData.DefaultPaletteId, "character.color.default", Color.White),
        new("hair_black", "character.color.hair_black", new Color(58, 52, 49)),
        new("hair_brown", "character.color.hair_brown", new Color(122, 76, 44)),
        new("hair_blonde", "character.color.hair_blonde", new Color(218, 177, 83)),
        new("hair_red", "character.color.hair_red", new Color(170, 72, 44)),
        new("hair_silver", "character.color.hair_silver", new Color(197, 196, 181)),
        new("hair_ashen", "character.color.hair_ashen", new Color(117, 124, 122))
    ];

    private static readonly IReadOnlyList<CharacterColorPalette> ClothingPalettes =
    [
        new(CharacterAppearanceData.DefaultPaletteId, "character.color.default", Color.White),
        new("cloth_linen", "character.color.cloth_linen", new Color(221, 203, 160)),
        new("cloth_rust", "character.color.cloth_rust", new Color(177, 85, 55)),
        new("cloth_forest", "character.color.cloth_forest", new Color(89, 132, 88)),
        new("cloth_blue", "character.color.cloth_blue", new Color(77, 112, 166)),
        new("cloth_violet", "character.color.cloth_violet", new Color(132, 91, 154)),
        new("cloth_charcoal", "character.color.cloth_charcoal", new Color(78, 82, 86))
    ];

    public static IReadOnlyList<SelectorOption> GetSelectorOptions(string slotId)
    {
        var palettes = GetPalettes(slotId);
        var options = new List<SelectorOption>(palettes.Count);
        foreach (var palette in palettes)
        {
            options.Add(new SelectorOption(palette.PaletteId, palette.LabelKey));
        }

        return options;
    }

    public static Color GetTint(string slotId, string paletteId)
    {
        foreach (var palette in GetPalettes(slotId))
        {
            if (palette.PaletteId == paletteId)
            {
                return palette.Tint;
            }
        }

        return Color.White;
    }

    private static IReadOnlyList<CharacterColorPalette> GetPalettes(string slotId)
    {
        return slotId switch
        {
            CharacterAppearanceSlotIds.Body => SkinPalettes,
            CharacterAppearanceSlotIds.Hair => HairPalettes,
            _ => ClothingPalettes
        };
    }
}
