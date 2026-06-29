using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using DuskVillage.CharacterAssets;
using DuskVillage.Characters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuskVillage.Rendering;

public sealed class ManaSeedCharacterTextureProvider : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly ManaSeedCharacterAssetCatalog _catalog;
    private readonly Dictionary<string, Texture2D> _textureCache = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    public ManaSeedCharacterTextureProvider(GraphicsDevice graphicsDevice, ManaSeedCharacterAssetCatalog catalog)
    {
        _graphicsDevice = graphicsDevice;
        _catalog = catalog;
    }

    public Texture2D GetTexture(ManaSeedCharacterAsset asset, string paletteId)
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
}
