using DuskVillage.Animations;
using DuskVillage.Players;

namespace DuskVillage.WorldMap;

public static class WorldMapTargetResolver
{
    public static (int X, int Y) ResolveAdjacentTile(PlayerLocationState location, CharacterFacingDirection facingDirection)
    {
        var x = location?.TileX ?? 0;
        var y = location?.TileY ?? 0;

        return facingDirection switch
        {
            CharacterFacingDirection.Up => (x, y - 1),
            CharacterFacingDirection.Right => (x + 1, y),
            CharacterFacingDirection.Left => (x - 1, y),
            _ => (x, y + 1)
        };
    }

    public static WorldMapMoveResult TryMove(WorldMapState map, PlayerLocationState location, CharacterFacingDirection direction)
    {
        var normalizedMap = WorldMapFactory.Normalize(map);
        var current = location?.Clone() ?? new PlayerLocationState
        {
            AreaId = normalizedMap.AreaId,
            TileX = WorldMapFactory.DefaultPlayerTileX,
            TileY = WorldMapFactory.DefaultPlayerTileY
        };

        current.AreaId = string.IsNullOrWhiteSpace(current.AreaId) ? normalizedMap.AreaId : current.AreaId;
        current.EnsurePosition();
        var (targetX, targetY) = ResolveAdjacentTile(current, direction);
        if (!current.AreaId.Equals(normalizedMap.AreaId, System.StringComparison.OrdinalIgnoreCase) ||
            !WorldMapRules.IsInside(normalizedMap, targetX, targetY))
        {
            return new WorldMapMoveResult(false, current, "world.map.move_blocked");
        }

        var targetTile = WorldMapRules.GetTile(normalizedMap, targetX, targetY);
        if (!WorldMapRules.IsPassable(targetTile))
        {
            return new WorldMapMoveResult(false, current, "world.map.move_blocked");
        }

        current.AreaId = normalizedMap.AreaId;
        current.SetTile(targetX, targetY);
        return new WorldMapMoveResult(true, current, "world.map.moved");
    }
}
