using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using DuskVillage.InventoryAssets;
using Microsoft.Xna.Framework.Graphics;

namespace DuskVillage.Rendering;

public sealed class InventoryUiTextureProvider : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Dictionary<string, Texture2D> _textureCache = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    public InventoryUiTextureProvider(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
    }

    public Texture2D GetTexture(InventoryUiAsset asset)
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
}
