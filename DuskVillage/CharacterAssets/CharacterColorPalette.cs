using Microsoft.Xna.Framework;

namespace DuskVillage.CharacterAssets;

public sealed class CharacterColorPalette
{
    public CharacterColorPalette(string paletteId, string labelKey, Color tint)
    {
        PaletteId = paletteId;
        LabelKey = labelKey;
        Tint = tint;
    }

    public string PaletteId { get; }

    public string LabelKey { get; }

    public Color Tint { get; }
}
