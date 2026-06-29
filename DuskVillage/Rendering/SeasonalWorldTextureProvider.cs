using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using DuskVillage.World;
using DuskVillage.WorldAssets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuskVillage.Rendering;

public sealed class SeasonalWorldTextureProvider : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Dictionary<string, Texture2D> _textureCache = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    public SeasonalWorldTextureProvider(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
    }

    public Texture2D GetTexture(SeasonalWorldAsset asset)
    {
        if (asset == null)
        {
            return null;
        }

        if (_textureCache.TryGetValue(asset.StableId, out var cached))
        {
            return cached;
        }

        if (!asset.IsAvailable || !File.Exists(asset.ZipPath))
        {
            return null;
        }

        try
        {
            using var archive = ZipFile.OpenRead(asset.ZipPath);
            var entry = archive.GetEntry(asset.EntryPath);
            if (entry == null)
            {
                return null;
            }

            using var stream = entry.Open();
            var texture = Texture2D.FromStream(_graphicsDevice, stream);
            _textureCache[asset.StableId] = texture;
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
        catch (ArgumentException)
        {
            return null;
        }
    }

    public Texture2D GetTextureOrFallback(SeasonalWorldAsset asset, string seasonId, string assetId)
    {
        return GetTexture(asset) ?? GetFallbackTexture(seasonId, assetId);
    }

    public Texture2D GetFallbackTexture(string seasonId, string assetId)
    {
        var key = $"fallback/{seasonId}/{assetId}";
        if (_textureCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var primary = FallbackColor(seasonId);
        var secondary = Color.Lerp(primary, Color.Black, 0.35f);
        var pixels = new Color[16 * 16];
        for (var y = 0; y < 16; y++)
        {
            for (var x = 0; x < 16; x++)
            {
                var border = x == 0 || y == 0 || x == 15 || y == 15;
                var checker = ((x / 4) + (y / 4)) % 2 == 0;
                pixels[y * 16 + x] = border ? Color.Black : checker ? primary : secondary;
            }
        }

        var texture = new Texture2D(_graphicsDevice, 16, 16);
        texture.SetData(pixels);
        _textureCache[key] = texture;
        return texture;
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

    private static Color FallbackColor(string seasonId)
    {
        if (seasonId != null && seasonId.Equals(WorldCalendarRules.Summer, StringComparison.OrdinalIgnoreCase))
        {
            return new Color(82, 148, 72);
        }

        if (seasonId != null && seasonId.Equals(WorldCalendarRules.Autumn, StringComparison.OrdinalIgnoreCase))
        {
            return new Color(170, 104, 48);
        }

        if (seasonId != null && seasonId.Equals(WorldCalendarRules.Winter, StringComparison.OrdinalIgnoreCase))
        {
            return new Color(156, 190, 210);
        }

        return new Color(92, 157, 94);
    }
}
