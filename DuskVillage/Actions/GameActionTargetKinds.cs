using System;

namespace DuskVillage.Actions;

public static class GameActionTargetKinds
{
    public const string None = "none";
    public const string Self = "self";
    public const string Tile = "tile";
    public const string Entity = "entity";

    public static bool IsKnown(string targetKind)
    {
        return targetKind.Equals(None, StringComparison.OrdinalIgnoreCase) ||
            targetKind.Equals(Self, StringComparison.OrdinalIgnoreCase) ||
            targetKind.Equals(Tile, StringComparison.OrdinalIgnoreCase) ||
            targetKind.Equals(Entity, StringComparison.OrdinalIgnoreCase);
    }
}
