using System;
using DuskVillage.CharacterAssets;
using DuskVillage.Characters;
using DuskVillage.UI;
using Microsoft.Xna.Framework;

namespace DuskVillage.Rendering;

public sealed class CharacterPortraitRenderer
{
    private static readonly Rectangle PreviewSource = new(0, 0, 64, 64);
    private readonly ManaSeedCharacterAssetCatalog _catalog;
    private readonly ManaSeedCharacterTextureProvider _textures;

    public CharacterPortraitRenderer(ManaSeedCharacterAssetCatalog catalog, ManaSeedCharacterTextureProvider textures)
    {
        _catalog = catalog;
        _textures = textures;
    }

    public void Draw(UiDrawContext draw, CharacterAppearanceData appearance, Rectangle bounds)
    {
        var destinationSize = Math.Min(bounds.Width - 20, bounds.Height - 20);
        destinationSize = Math.Max(64, destinationSize);
        var destination = new Rectangle(
            bounds.X + (bounds.Width - destinationSize) / 2,
            bounds.Y + (bounds.Height - destinationSize) / 2,
            destinationSize,
            destinationSize);

        if (!_catalog.IsAvailable)
        {
            CharacterFallbackRenderer.Draw(draw, appearance, destination);
            return;
        }

        foreach (var asset in CharacterLayerResolver.ResolveDrawableAssets(_catalog, appearance))
        {
            var texture = _textures.GetTexture(asset, appearance.GetPalette(asset.SlotId));
            if (texture == null)
            {
                continue;
            }

            var source = texture.Width >= PreviewSource.Width && texture.Height >= PreviewSource.Height
                ? PreviewSource
                : new Rectangle(0, 0, texture.Width, texture.Height);

            draw.SpriteBatch.Draw(texture, destination, source, Color.White);
        }
    }
}
