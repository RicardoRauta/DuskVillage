using System;
using DuskVillage.UI;
using DuskVillage.WorldAssets;
using DuskVillage.WorldMap;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuskVillage.Rendering;

public sealed class WorldMapRenderer
{
    private const int ManaSeedTerrainColumns = 64;
    private const int TileSourceSize = 16;

    private readonly SeasonalWorldAssetCatalog _assets;
    private readonly SeasonalWorldTextureProvider _textures;

    public WorldMapRenderer(SeasonalWorldAssetCatalog assets, SeasonalWorldTextureProvider textures)
    {
        _assets = assets ?? SeasonalWorldAssetCatalog.Empty;
        _textures = textures;
    }

    public WorldMapViewport Draw(
        UiDrawContext draw,
        WorldMapState map,
        string seasonId,
        Rectangle bounds,
        Point? highlightedTile,
        Vector2? focusTile = null,
        bool fillViewport = false)
    {
        var normalized = WorldMapFactory.Normalize(map);
        var viewport = fillViewport
            ? CreateCameraViewport(normalized, bounds, focusTile)
            : CreateFitViewport(normalized, bounds);
        var mapBounds = viewport.Bounds;

        var terrain = GetTerrainTexture(seasonId);
        foreach (var tile in normalized.Tiles)
        {
            var destination = viewport.TileBounds(tile.X, tile.Y);
            if (destination.Intersects(bounds))
            {
                DrawTile(draw, terrain, tile, destination);
            }
        }

        if (highlightedTile.HasValue &&
            highlightedTile.Value.X >= 0 &&
            highlightedTile.Value.Y >= 0 &&
            highlightedTile.Value.X < normalized.Width &&
            highlightedTile.Value.Y < normalized.Height)
        {
            draw.Border(viewport.TileBounds(highlightedTile.Value.X, highlightedTile.Value.Y), draw.Theme.Accent, 3);
        }

        draw.Border(mapBounds, draw.Theme.Border);
        return viewport;
    }

    private static WorldMapViewport CreateFitViewport(WorldMapState map, Rectangle bounds)
    {
        var tileSize = Math.Max(8, Math.Min(bounds.Width / map.Width, bounds.Height / map.Height));
        var mapBounds = new Rectangle(
            bounds.X + (bounds.Width - map.Width * tileSize) / 2,
            bounds.Y + (bounds.Height - map.Height * tileSize) / 2,
            map.Width * tileSize,
            map.Height * tileSize);

        return new WorldMapViewport(mapBounds, tileSize);
    }

    private static WorldMapViewport CreateCameraViewport(WorldMapState map, Rectangle bounds, Vector2? focusTile)
    {
        var tileSize = Math.Max(8, (int)Math.Ceiling(Math.Max(
            bounds.Width / (double)map.Width,
            bounds.Height / (double)map.Height)));
        var mapWidth = map.Width * tileSize;
        var mapHeight = map.Height * tileSize;
        var mapX = bounds.X + (bounds.Width - mapWidth) / 2;
        var mapY = bounds.Y + (bounds.Height - mapHeight) / 2;

        if (focusTile.HasValue)
        {
            mapX = (int)Math.Round(bounds.X + bounds.Width / 2f - focusTile.Value.X * tileSize);
            mapY = (int)Math.Round(bounds.Y + bounds.Height / 2f - focusTile.Value.Y * tileSize);
        }

        mapX = ClampMapOffset(mapX, bounds.X, bounds.Width, mapWidth);
        mapY = ClampMapOffset(mapY, bounds.Y, bounds.Height, mapHeight);

        return new WorldMapViewport(new Rectangle(mapX, mapY, mapWidth, mapHeight), tileSize);
    }

    private static int ClampMapOffset(int offset, int viewportStart, int viewportLength, int mapLength)
    {
        if (mapLength <= viewportLength)
        {
            return viewportStart + (viewportLength - mapLength) / 2;
        }

        return Math.Clamp(offset, viewportStart + viewportLength - mapLength, viewportStart);
    }

    private Texture2D GetTerrainTexture(string seasonId)
    {
        var asset = _assets
            .GetPackOrFallback(seasonId)
            ?.FindAsset(SeasonalWorldAssetIds.TerrainWang);

        return _textures?.GetTextureOrFallback(asset, seasonId, SeasonalWorldAssetIds.TerrainWang);
    }

    private void DrawTile(UiDrawContext draw, Texture2D terrain, WorldMapTile tile, Rectangle destination)
    {
        if (terrain != null)
        {
            draw.SpriteBatch.Draw(terrain, destination, SourceRectangle(terrain, TileId(tile.TypeId)), Color.White);
        }
        else
        {
            draw.Fill(destination, FallbackColor(tile.TypeId));
        }

        if (tile.StateId.Equals(WorldMapTileStateIds.Planted, StringComparison.OrdinalIgnoreCase) ||
            tile.StateId.Equals(WorldMapTileStateIds.Watered, StringComparison.OrdinalIgnoreCase))
        {
            DrawSprout(draw, destination, tile.StateId.Equals(WorldMapTileStateIds.Watered, StringComparison.OrdinalIgnoreCase));
        }

        draw.Border(destination, new Color(0, 0, 0, 36), 1);
    }

    private static Rectangle SourceRectangle(Texture2D texture, int tileId)
    {
        if (texture.Width <= TileSourceSize || texture.Height <= TileSourceSize)
        {
            return new Rectangle(0, 0, Math.Min(TileSourceSize, texture.Width), Math.Min(TileSourceSize, texture.Height));
        }

        var column = tileId % ManaSeedTerrainColumns;
        var row = tileId / ManaSeedTerrainColumns;
        var x = Math.Clamp(column * TileSourceSize, 0, Math.Max(0, texture.Width - TileSourceSize));
        var y = Math.Clamp(row * TileSourceSize, 0, Math.Max(0, texture.Height - TileSourceSize));
        return new Rectangle(x, y, TileSourceSize, TileSourceSize);
    }

    private static int TileId(string tileTypeId)
    {
        if (tileTypeId.Equals(WorldMapTileTypeIds.Soil, StringComparison.OrdinalIgnoreCase))
        {
            return 128;
        }

        if (tileTypeId.Equals(WorldMapTileTypeIds.Path, StringComparison.OrdinalIgnoreCase))
        {
            return 131;
        }

        if (tileTypeId.Equals(WorldMapTileTypeIds.Water, StringComparison.OrdinalIgnoreCase))
        {
            return 132;
        }

        return 130;
    }

    private static Color FallbackColor(string tileTypeId)
    {
        if (tileTypeId.Equals(WorldMapTileTypeIds.Soil, StringComparison.OrdinalIgnoreCase))
        {
            return new Color(112, 78, 48);
        }

        if (tileTypeId.Equals(WorldMapTileTypeIds.Path, StringComparison.OrdinalIgnoreCase))
        {
            return new Color(118, 112, 92);
        }

        if (tileTypeId.Equals(WorldMapTileTypeIds.Water, StringComparison.OrdinalIgnoreCase))
        {
            return new Color(46, 98, 142);
        }

        return new Color(58, 118, 64);
    }

    private static void DrawSprout(UiDrawContext draw, Rectangle tile, bool watered)
    {
        if (watered)
        {
            draw.Fill(new Rectangle(tile.X + 3, tile.Y + 3, tile.Width - 6, tile.Height - 6), new Color(74, 131, 168, 86));
        }

        var stemWidth = Math.Max(3, tile.Width / 10);
        var stemHeight = Math.Max(8, tile.Height / 3);
        var stem = new Rectangle(
            tile.X + tile.Width / 2 - stemWidth / 2,
            tile.Y + tile.Height / 2,
            stemWidth,
            stemHeight);
        draw.Fill(stem, new Color(42, 132, 58));
        draw.Fill(new Rectangle(stem.X - stemWidth * 2, stem.Y - stemWidth, stemWidth * 2, stemWidth), new Color(62, 158, 72));
        draw.Fill(new Rectangle(stem.Right, stem.Y - stemWidth, stemWidth * 2, stemWidth), new Color(62, 158, 72));
    }
}
