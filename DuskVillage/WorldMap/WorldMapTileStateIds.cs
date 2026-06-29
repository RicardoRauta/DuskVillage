using System;

namespace DuskVillage.WorldMap;

public static class WorldMapTileStateIds
{
    public const string Empty = "empty";
    public const string Tilled = "tilled";
    public const string Planted = "planted";
    public const string Watered = "watered";

    public static bool IsKnown(string stateId)
    {
        return stateId.Equals(Empty, StringComparison.OrdinalIgnoreCase) ||
            stateId.Equals(Tilled, StringComparison.OrdinalIgnoreCase) ||
            stateId.Equals(Planted, StringComparison.OrdinalIgnoreCase) ||
            stateId.Equals(Watered, StringComparison.OrdinalIgnoreCase);
    }
}
