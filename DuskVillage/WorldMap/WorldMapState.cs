using System.Collections.Generic;

namespace DuskVillage.WorldMap;

public sealed class WorldMapState
{
    public string AreaId { get; set; } = WorldMapFactory.DefaultAreaId;

    public int Width { get; set; } = WorldMapFactory.DefaultWidth;

    public int Height { get; set; } = WorldMapFactory.DefaultHeight;

    public List<WorldMapTile> Tiles { get; set; } = [];

    public WorldMapState Clone()
    {
        var clone = new WorldMapState
        {
            AreaId = AreaId,
            Width = Width,
            Height = Height,
            Tiles = new List<WorldMapTile>(Tiles.Count)
        };

        foreach (var tile in Tiles)
        {
            clone.Tiles.Add(tile.Clone());
        }

        return clone;
    }
}
