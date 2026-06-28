using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using DuskVillage.CharacterAssets;
using DuskVillage.Characters;
using DuskVillage.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuskVillage.Rendering;

public sealed class CharacterPortraitRenderer : IDisposable
{
    private static readonly Rectangle PreviewSource = new(0, 0, 64, 64);
    private readonly GraphicsDevice _graphicsDevice;
    private readonly ManaSeedCharacterAssetCatalog _catalog;
    private readonly Dictionary<string, Texture2D> _textureCache = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    public CharacterPortraitRenderer(GraphicsDevice graphicsDevice, ManaSeedCharacterAssetCatalog catalog)
    {
        _graphicsDevice = graphicsDevice;
        _catalog = catalog;
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
            DrawFallback(draw, appearance, destination);
            return;
        }

        foreach (var asset in ResolveDrawableAssets(appearance))
        {
            var texture = GetTexture(asset, appearance.GetPalette(asset.SlotId));
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

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var texture in _textureCache.Values)
        {
            texture.Dispose();
        }

        _textureCache.Clear();
        _disposed = true;
    }

    private IReadOnlyList<ManaSeedCharacterAsset> ResolveDrawableAssets(CharacterAppearanceData appearance)
    {
        var selectedHeadwear = _catalog.FindAsset(appearance.GetLayer(CharacterAppearanceSlotIds.Head));
        var headwearHidesHair = selectedHeadwear?.HidesHair == true;
        var anyHeadwear = selectedHeadwear != null;
        var resolved = new List<ManaSeedCharacterAsset>();

        foreach (var slot in _catalog.Slots.OrderBy(slot => slot.LayerOrder))
        {
            var assetId = appearance.GetLayer(slot.SlotId);
            if (assetId == CharacterAppearanceData.NoneAssetId)
            {
                continue;
            }

            var asset = _catalog.FindAsset(assetId);
            if (asset == null)
            {
                continue;
            }

            if (slot.SlotId == CharacterAppearanceSlotIds.Hair &&
                (headwearHidesHair || (asset.HidesWhenHeadwearEquipped && anyHeadwear)))
            {
                continue;
            }

            resolved.Add(asset);
        }

        return resolved;
    }

    private Texture2D GetTexture(ManaSeedCharacterAsset asset, string paletteId)
    {
        var key = $"{asset.AssetId}|{paletteId}";
        if (_textureCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var rawTexture = GetRawTexture(asset);
        if (rawTexture == null)
        {
            return null;
        }

        if (paletteId == CharacterAppearanceData.DefaultPaletteId)
        {
            _textureCache[key] = rawTexture;
            return rawTexture;
        }

        var tint = CharacterColorPaletteCatalog.GetTint(asset.SlotId, paletteId);
        var colored = Recolor(rawTexture, tint);
        _textureCache[key] = colored;
        return colored;
    }

    private Texture2D GetRawTexture(ManaSeedCharacterAsset asset)
    {
        var rawKey = $"{asset.AssetId}|{CharacterAppearanceData.DefaultPaletteId}";
        if (_textureCache.TryGetValue(rawKey, out var cached))
        {
            return cached;
        }

        if (string.IsNullOrWhiteSpace(asset.ZipEntryPath) || !File.Exists(_catalog.ZipPath))
        {
            return null;
        }

        try
        {
            using var archive = ZipFile.OpenRead(_catalog.ZipPath);
            var entry = archive.GetEntry(asset.ZipEntryPath);
            if (entry == null)
            {
                return null;
            }

            using var stream = entry.Open();
            var texture = Texture2D.FromStream(_graphicsDevice, stream);
            _textureCache[rawKey] = texture;
            return texture;
        }
        catch (IOException)
        {
            return null;
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    private Texture2D Recolor(Texture2D source, Color tint)
    {
        var pixels = new Color[source.Width * source.Height];
        source.GetData(pixels);

        for (var i = 0; i < pixels.Length; i++)
        {
            var pixel = pixels[i];
            if (pixel.A == 0)
            {
                continue;
            }

            var luminance = (pixel.R * 0.299f + pixel.G * 0.587f + pixel.B * 0.114f) / 255f;
            var shade = Math.Clamp(0.45f + luminance * 0.85f, 0f, 1.25f);
            pixels[i] = new Color(
                Math.Clamp((int)(tint.R * shade), 0, 255),
                Math.Clamp((int)(tint.G * shade), 0, 255),
                Math.Clamp((int)(tint.B * shade), 0, 255),
                pixel.A);
        }

        var texture = new Texture2D(_graphicsDevice, source.Width, source.Height);
        texture.SetData(pixels);
        return texture;
    }

    private static void DrawFallback(UiDrawContext draw, CharacterAppearanceData appearance, Rectangle destination)
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
