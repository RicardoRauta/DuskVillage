using DuskVillage.Animations;
using DuskVillage.Players;

namespace DuskVillage.WorldMap;

public static class WorldMapTargetResolver
{
    private const double HorizontalToeOffsetTiles = 0.56;
    private const double DownToeOffsetTiles = 0.56;
    private const double UpHeadOffsetTiles = -1.94;

    public static (int X, int Y) ResolveAdjacentTile(PlayerLocationState location, CharacterFacingDirection facingDirection)
    {
        var positionX = location?.GetPositionX() ?? 0.5;
        var positionY = location?.GetPositionY() ?? 0.5;

        return facingDirection switch
        {
            CharacterFacingDirection.Up => TargetTile(positionX, positionY + UpHeadOffsetTiles),
            CharacterFacingDirection.Right => TargetTile(positionX + HorizontalToeOffsetTiles, positionY),
            CharacterFacingDirection.Left => TargetTile(positionX - HorizontalToeOffsetTiles, positionY),
            _ => TargetTile(positionX, positionY + DownToeOffsetTiles)
        };
    }

    private static (int X, int Y) TargetTile(double positionX, double positionY)
    {
        return (
            PlayerLocationState.PositionToTile(positionX),
            PlayerLocationState.PositionToTile(positionY));
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
        var (targetX, targetY) = ResolveMovementTile(current, direction);
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

    private static (int X, int Y) ResolveMovementTile(PlayerLocationState location, CharacterFacingDirection facingDirection)
    {
        var tileX = PlayerLocationState.PositionToTile(location.GetPositionX());
        var tileY = PlayerLocationState.PositionToTile(location.GetPositionY());

        return facingDirection switch
        {
            CharacterFacingDirection.Up => (tileX, tileY - 1),
            CharacterFacingDirection.Right => (tileX + 1, tileY),
            CharacterFacingDirection.Left => (tileX - 1, tileY),
            _ => (tileX, tileY + 1)
        };
    }
}
