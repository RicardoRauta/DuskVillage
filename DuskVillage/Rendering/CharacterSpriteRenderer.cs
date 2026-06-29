using System;
using DuskVillage.Animations;
using DuskVillage.CharacterAssets;
using DuskVillage.Characters;
using DuskVillage.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuskVillage.Rendering;

public sealed class CharacterSpriteRenderer
{
    private readonly ManaSeedCharacterAssetCatalog _catalog;
    private readonly ManaSeedCharacterTextureProvider _textures;

    public CharacterSpriteRenderer(ManaSeedCharacterAssetCatalog catalog, ManaSeedCharacterTextureProvider textures)
    {
        _catalog = catalog;
        _textures = textures;
    }

    public void Draw(UiDrawContext draw, CharacterAppearanceData appearance, CharacterAnimationState animation, Rectangle bounds)
    {
        var destinationSize = Math.Min(bounds.Width - 20, bounds.Height - 20);
        destinationSize = Math.Max(CharacterAnimationCatalog.CellSize, destinationSize);
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

        var frame = CharacterAnimationSystem.GetCurrentFrame(animation);
        var source = SourceFromCell(frame.CellIndex);
        var effects = frame.FlipX ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        foreach (var asset in CharacterLayerResolver.ResolveDrawableAssets(_catalog, appearance))
        {
            var texture = _textures.GetTexture(asset, appearance.GetPalette(asset.SlotId));
            if (texture == null || texture.Width < source.Right || texture.Height < source.Bottom)
            {
                continue;
            }

            draw.SpriteBatch.Draw(texture, destination, source, Color.White, 0f, Vector2.Zero, effects, 0f);
        }
    }

    private static Rectangle SourceFromCell(int cellIndex)
    {
        return new Rectangle(
            CharacterAnimationCatalog.CellColumn(cellIndex) * CharacterAnimationCatalog.CellSize,
            CharacterAnimationCatalog.CellRow(cellIndex) * CharacterAnimationCatalog.CellSize,
            CharacterAnimationCatalog.CellSize,
            CharacterAnimationCatalog.CellSize);
    }
}
