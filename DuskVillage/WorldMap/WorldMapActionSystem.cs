using System;
using System.Collections.Generic;
using System.Linq;
using DuskVillage.Actions;
using DuskVillage.Animations;
using DuskVillage.Players;
using DuskVillage.World;

namespace DuskVillage.WorldMap;

public static class WorldMapActionSystem
{
    public static WorldMapActionResult Execute(
        GameActionRegistry registry,
        GameActionRequest request,
        WorldTime worldTime,
        PlayerRuntimeState playerState,
        WorldMapState mapState)
    {
        var normalizedMap = WorldMapFactory.Normalize(mapState);
        if (registry == null || request == null || !registry.TryGet(request.ActionId, out var definition))
        {
            var fallback = GameActionSystem.Execute(registry, request, worldTime, playerState);
            return new WorldMapActionResult(fallback, normalizedMap, fallback.MessageKey);
        }

        var mapEffects = definition.Effects.Where(IsMapEffect).ToArray();
        if (mapEffects.Length > 0 && !TryValidateMapTarget(definition, request.Target, normalizedMap, out var failureMessageKey))
        {
            return Failure(definition, worldTime, playerState, normalizedMap, request.FacingDirection, failureMessageKey);
        }

        if (mapEffects.Length > 0 && !CanApplyMapEffects(mapEffects, request.Target, normalizedMap, out failureMessageKey))
        {
            return Failure(definition, worldTime, playerState, normalizedMap, request.FacingDirection, failureMessageKey);
        }

        var actionResult = GameActionSystem.Execute(registry, request, worldTime, playerState);
        if (!actionResult.Succeeded)
        {
            return new WorldMapActionResult(actionResult, normalizedMap, actionResult.MessageKey);
        }

        var nextMap = normalizedMap.Clone();
        foreach (var effect in mapEffects)
        {
            ApplyMapEffect(effect, request.Target, nextMap);
        }

        return new WorldMapActionResult(actionResult, WorldMapFactory.Normalize(nextMap), actionResult.MessageKey);
    }

    private static bool TryValidateMapTarget(
        GameActionDefinition definition,
        GameActionTarget target,
        WorldMapState map,
        out string failureMessageKey)
    {
        failureMessageKey = string.Empty;
        if (!definition.TargetKind.Equals(GameActionTargetKinds.Tile, StringComparison.OrdinalIgnoreCase) ||
            target == null ||
            !target.Kind.Equals(GameActionTargetKinds.Tile, StringComparison.OrdinalIgnoreCase))
        {
            failureMessageKey = "world.map.invalid_target";
            return false;
        }

        if (!target.AreaId.Equals(map.AreaId, StringComparison.OrdinalIgnoreCase) ||
            !WorldMapRules.IsInside(map, target.TileX, target.TileY))
        {
            failureMessageKey = "world.map.out_of_bounds";
            return false;
        }

        return true;
    }

    private static bool CanApplyMapEffects(
        IEnumerable<GameActionEffectDefinition> effects,
        GameActionTarget target,
        WorldMapState map,
        out string failureMessageKey)
    {
        failureMessageKey = string.Empty;
        var tile = WorldMapRules.GetTile(map, target.TileX, target.TileY);
        foreach (var effect in effects)
        {
            if (effect.Type.Equals(GameActionEffectTypes.PlantCrop, StringComparison.OrdinalIgnoreCase) &&
                !WorldMapRules.CanPlant(tile))
            {
                failureMessageKey = "world.map.not_plantable";
                return false;
            }

            if (effect.Type.Equals(GameActionEffectTypes.WaterTile, StringComparison.OrdinalIgnoreCase) &&
                !WorldMapRules.CanWater(tile))
            {
                failureMessageKey = tile?.StateId.Equals(WorldMapTileStateIds.Watered, StringComparison.OrdinalIgnoreCase) == true
                    ? "world.map.already_watered"
                    : "world.map.no_crop_to_water";
                return false;
            }
        }

        return true;
    }

    private static void ApplyMapEffect(GameActionEffectDefinition effect, GameActionTarget target, WorldMapState map)
    {
        var tile = WorldMapRules.GetTile(map, target.TileX, target.TileY);
        if (tile == null)
        {
            return;
        }

        if (effect.Type.Equals(GameActionEffectTypes.SetTileState, StringComparison.OrdinalIgnoreCase))
        {
            tile.StateId = string.IsNullOrWhiteSpace(effect.TileStateId) ? WorldMapTileStateIds.Empty : effect.TileStateId.Trim();
            return;
        }

        if (effect.Type.Equals(GameActionEffectTypes.PlantCrop, StringComparison.OrdinalIgnoreCase))
        {
            tile.StateId = WorldMapTileStateIds.Planted;
            tile.CropId = string.IsNullOrWhiteSpace(effect.CropId) ? WorldMapCropIds.StarterSeeds : effect.CropId.Trim();
            return;
        }

        if (effect.Type.Equals(GameActionEffectTypes.WaterTile, StringComparison.OrdinalIgnoreCase))
        {
            tile.StateId = WorldMapTileStateIds.Watered;
        }
    }

    private static bool IsMapEffect(GameActionEffectDefinition effect)
    {
        return effect.Type.Equals(GameActionEffectTypes.SetTileState, StringComparison.OrdinalIgnoreCase) ||
            effect.Type.Equals(GameActionEffectTypes.PlantCrop, StringComparison.OrdinalIgnoreCase) ||
            effect.Type.Equals(GameActionEffectTypes.WaterTile, StringComparison.OrdinalIgnoreCase);
    }

    private static WorldMapActionResult Failure(
        GameActionDefinition definition,
        WorldTime worldTime,
        PlayerRuntimeState playerState,
        WorldMapState mapState,
        CharacterFacingDirection facingDirection,
        string messageKey)
    {
        var actionResult = new GameActionResult(
            succeeded: false,
            definition,
            WorldClock.Normalize(worldTime),
            PlayerRuntimeFactory.Clone(playerState),
            CharacterAnimationIds.Idle,
            facingDirection,
            messageKey,
            startedNewDay: false,
            forcedDayEnd: false,
            Array.Empty<GameActionAppliedEffect>());

        return new WorldMapActionResult(actionResult, mapState.Clone(), messageKey);
    }
}
