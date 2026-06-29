using DuskVillage.Characters;
using DuskVillage.CharacterAssets;
using DuskVillage.UI;
using Microsoft.Xna.Framework;

namespace DuskVillage.Rendering;

internal static class CharacterFallbackRenderer
{
    public static void Draw(UiDrawContext draw, CharacterAppearanceData appearance, Rectangle destination)
    {
        var head = new Rectangle(destination.X + destination.Width / 3, destination.Y + destination.Height / 12, destination.Width / 3, destination.Height / 4);
        var body = new Rectangle(destination.X + destination.Width / 4, head.Bottom, destination.Width / 2, destination.Height / 2);
        var legs = new Rectangle(destination.X + destination.Width / 3, body.Bottom, destination.Width / 3, destination.Height / 4);

        draw.Fill(body, CharacterColorPaletteCatalog.GetTint(CharacterAppearanceSlotIds.Shirt, appearance.GetPalette(CharacterAppearanceSlotIds.Shirt)));
        draw.Fill(legs, CharacterColorPaletteCatalog.GetTint(CharacterAppearanceSlotIds.LowerOne, appearance.GetPalette(CharacterAppearanceSlotIds.LowerOne)));
        draw.Fill(head, CharacterColorPaletteCatalog.GetTint(CharacterAppearanceSlotIds.Body, appearance.GetPalette(CharacterAppearanceSlotIds.Body)));
        draw.Border(destination, draw.Theme.Border, 1);
    }
}
