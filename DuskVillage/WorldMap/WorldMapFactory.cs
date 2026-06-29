using System;
using System.Collections.Generic;
using System.Linq;

namespace DuskVillage.WorldMap;

public static class WorldMapFactory
{
    public const string DefaultAreaId = "starter_farm";
    public const int DefaultWidth = 16;
    public const int DefaultHeight = 12;
    public const int DefaultPlayerTileX = 7;
    public const int DefaultPlayerTileY = 7;

    public static WorldMapState CreateDefault()
    {
        var map = new WorldMapState
        {
            AreaId = DefaultAreaId,
            Width = DefaultWidth,
            Height = DefaultHeight,
            Tiles = []
        };

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                map.Tiles.Add(CreateDefaultTile(x, y));
            }
        }

        return map;
    }

    public static WorldMapState Normalize(WorldMapState map)
    {
        if (map == null)
        {
            return CreateDefault();
        }

        var width = map.Width <= 0 ? DefaultWidth : map.Width;
        var height = map.Height <= 0 ? DefaultHeight : map.Height;
        var sourceTiles = (map.Tiles ?? [])
            .Where(tile => tile != null && tile.X >= 0 && tile.Y >= 0 && tile.X < width && tile.Y < height)
            .GroupBy(tile => (tile.X, tile.Y))
            .ToDictionary(group => group.Key, group => NormalizeTile(group.Last()));

        var normalized = new WorldMapState
        {
            AreaId = string.IsNullOrWhiteSpace(map.AreaId) ? DefaultAreaId : map.AreaId.Trim(),
            Width = width,
            Height = height,
            Tiles = new List<WorldMapTile>(width * height)
        };

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                normalized.Tiles.Add(sourceTiles.TryGetValue((x, y), out var tile) ? tile : CreateDefaultTile(x, y));
            }
        }

        return normalized;
    }

    public static WorldMapTile CreateDefaultTile(int x, int y)
    {
        return new WorldMapTile
        {
            X = x,
            Y = y,
            TypeId = DefaultTypeAt(x, y),
            StateId = WorldMapTileStateIds.Empty,
            CropId = string.Empty
        };
    }

    private static WorldMapTile NormalizeTile(WorldMapTile tile)
    {
        var normalized = tile.Clone();
        normalized.TypeId = string.IsNullOrWhiteSpace(normalized.TypeId) || !WorldMapTileTypeIds.IsKnown(normalized.TypeId)
            ? WorldMapTileTypeIds.Grass
            : normalized.TypeId.Trim();
        normalized.StateId = string.IsNullOrWhiteSpace(normalized.StateId) || !WorldMapTileStateIds.IsKnown(normalized.StateId)
            ? WorldMapTileStateIds.Empty
            : normalized.StateId.Trim();
        normalized.CropId = normalized.StateId.Equals(WorldMapTileStateIds.Planted, StringComparison.OrdinalIgnoreCase) ||
            normalized.StateId.Equals(WorldMapTileStateIds.Watered, StringComparison.OrdinalIgnoreCase)
                ? normalized.CropId?.Trim() ?? string.Empty
                : string.Empty;
        return normalized;
    }

    private static string DefaultTypeAt(int x, int y)
    {
        if (x == 0 || y == 0 || x == DefaultWidth - 1 || y == DefaultHeight - 1)
        {
            return WorldMapTileTypeIds.Water;
        }

        if (x >= 4 && x <= 9 && y >= 3 && y <= 5)
        {
            return WorldMapTileTypeIds.Soil;
        }

        if (x == 2 || y == 8)
        {
            return WorldMapTileTypeIds.Path;
        }

        return WorldMapTileTypeIds.Grass;
    }
}
