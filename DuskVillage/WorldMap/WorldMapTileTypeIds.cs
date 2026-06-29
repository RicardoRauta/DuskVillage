using System;

namespace DuskVillage.WorldMap;

public static class WorldMapTileTypeIds
{
    public const string Grass = "grass";
    public const string Soil = "soil";
    public const string Path = "path";
    public const string Water = "water";

    public static bool IsKnown(string tileTypeId)
    {
        return tileTypeId.Equals(Grass, StringComparison.OrdinalIgnoreCase) ||
            tileTypeId.Equals(Soil, StringComparison.OrdinalIgnoreCase) ||
            tileTypeId.Equals(Path, StringComparison.OrdinalIgnoreCase) ||
            tileTypeId.Equals(Water, StringComparison.OrdinalIgnoreCase);
    }
}
