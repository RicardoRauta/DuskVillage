using System;
using DuskVillage.Animations;
using DuskVillage.Players;

namespace DuskVillage.WorldMap;

public static class WorldMapContinuousMovementSystem
{
    public static WorldMapContinuousMovementResult Move(
        WorldMapState map,
        PlayerLocationState location,
        double inputX,
        double inputY,
        double elapsedSeconds,
        double speedTilesPerSecond)
    {
        var normalizedMap = WorldMapFactory.Normalize(map);
        var current = NormalizeLocation(normalizedMap, location);
        var facingDirection = ResolveFacingDirection(inputX, inputY);

        if (!facingDirection.HasValue || elapsedSeconds <= 0 || speedTilesPerSecond <= 0)
        {
            return new WorldMapContinuousMovementResult(false, false, current, facingDirection, "world.map.moved");
        }

        var length = Math.Sqrt(inputX * inputX + inputY * inputY);
        var normalizedX = inputX / length;
        var normalizedY = inputY / length;
        var distance = speedTilesPerSecond * elapsedSeconds;
        var moved = false;
        var blocked = false;

        if (Math.Abs(normalizedX) > 0)
        {
            var targetX = current.GetPositionX() + normalizedX * distance;
            if (CanStandAt(normalizedMap, current.AreaId, targetX, current.GetPositionY()))
            {
                current.SetPosition(targetX, current.GetPositionY());
                moved = true;
            }
            else
            {
                blocked = true;
            }
        }

        if (Math.Abs(normalizedY) > 0)
        {
            var targetY = current.GetPositionY() + normalizedY * distance;
            if (CanStandAt(normalizedMap, current.AreaId, current.GetPositionX(), targetY))
            {
                current.SetPosition(current.GetPositionX(), targetY);
                moved = true;
            }
            else
            {
                blocked = true;
            }
        }

        return new WorldMapContinuousMovementResult(
            moved,
            blocked && !moved,
            current,
            facingDirection,
            moved ? "world.map.moved" : "world.map.move_blocked");
    }

    public static bool CanStandAt(WorldMapState map, string areaId, double positionX, double positionY)
    {
        if (map == null || !string.Equals(areaId, map.AreaId, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var tileX = PlayerLocationState.PositionToTile(positionX);
        var tileY = PlayerLocationState.PositionToTile(positionY);
        return WorldMapRules.IsInside(map, tileX, tileY) &&
            WorldMapRules.IsPassable(WorldMapRules.GetTile(map, tileX, tileY));
    }

    private static PlayerLocationState NormalizeLocation(WorldMapState map, PlayerLocationState location)
    {
        var normalized = location?.Clone() ?? new PlayerLocationState
        {
            AreaId = map.AreaId,
            TileX = WorldMapFactory.DefaultPlayerTileX,
            TileY = WorldMapFactory.DefaultPlayerTileY
        };

        normalized.AreaId = string.IsNullOrWhiteSpace(normalized.AreaId) ? map.AreaId : normalized.AreaId;
        normalized.EnsurePosition();

        if (!CanStandAt(map, normalized.AreaId, normalized.GetPositionX(), normalized.GetPositionY()))
        {
            normalized.AreaId = map.AreaId;
            normalized.SetTile(WorldMapFactory.DefaultPlayerTileX, WorldMapFactory.DefaultPlayerTileY);
        }

        return normalized;
    }

    private static CharacterFacingDirection? ResolveFacingDirection(double inputX, double inputY)
    {
        if (Math.Abs(inputX) < 0.001 && Math.Abs(inputY) < 0.001)
        {
            return null;
        }

        if (Math.Abs(inputX) > Math.Abs(inputY))
        {
            return inputX > 0 ? CharacterFacingDirection.Right : CharacterFacingDirection.Left;
        }

        return inputY > 0 ? CharacterFacingDirection.Down : CharacterFacingDirection.Up;
    }
}
