using System;

namespace DuskVillage.WorldMap;

public static class WorldMapRules
{
    public static bool IsInside(WorldMapState map, int tileX, int tileY)
    {
        if (map == null)
        {
            return false;
        }

        var width = map.Width <= 0 ? WorldMapFactory.DefaultWidth : map.Width;
        var height = map.Height <= 0 ? WorldMapFactory.DefaultHeight : map.Height;
        return tileX >= 0 && tileY >= 0 && tileX < width && tileY < height;
    }

    public static WorldMapTile GetTile(WorldMapState map, int tileX, int tileY)
    {
        if (!IsInside(map, tileX, tileY) || map.Tiles == null)
        {
            return null;
        }

        foreach (var tile in map.Tiles)
        {
            if (tile != null && tile.X == tileX && tile.Y == tileY)
            {
                return tile;
            }
        }

        return null;
    }

    public static bool IsPassable(WorldMapTile tile)
    {
        return tile != null && !tile.TypeId.Equals(WorldMapTileTypeIds.Water, StringComparison.OrdinalIgnoreCase);
    }

    public static bool CanPlant(WorldMapTile tile)
    {
        if (tile == null || !tile.TypeId.Equals(WorldMapTileTypeIds.Soil, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return tile.StateId.Equals(WorldMapTileStateIds.Empty, StringComparison.OrdinalIgnoreCase) ||
            tile.StateId.Equals(WorldMapTileStateIds.Tilled, StringComparison.OrdinalIgnoreCase);
    }

    public static bool CanWater(WorldMapTile tile)
    {
        return tile != null && tile.TypeId.Equals(WorldMapTileTypeIds.Soil, StringComparison.OrdinalIgnoreCase) &&
            tile.StateId.Equals(WorldMapTileStateIds.Planted, StringComparison.OrdinalIgnoreCase);
    }
}
