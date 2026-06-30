using System;
using System.Collections.Generic;
using DuskVillage.Animations;
using DuskVillage.Characters;
using DuskVillage.Inventory;
using DuskVillage.Needs;
using DuskVillage.Players;
using DuskVillage.World;

namespace DuskVillage.Actions;

public static class GameActionSystem
{
    public static GameActionResult Execute(
        GameActionRegistry registry,
        GameActionRequest request,
        WorldTime worldTime,
        PlayerRuntimeState playerState)
    {
        registry ??= GameActionRegistry.Empty;
        request ??= new GameActionRequest();
        var originalWorldTime = WorldClock.Normalize(worldTime);
        var originalPlayerState = PlayerRuntimeFactory.Clone(playerState);

        if (!registry.TryGet(request.ActionId, out var definition))
        {
            return Failure(
                originalWorldTime,
                originalPlayerState,
                request.FacingDirection,
                "action.result.missing");
        }

        if (!request.ActorEntityId.Equals(originalPlayerState.EntityId, StringComparison.OrdinalIgnoreCase))
        {
            return Failure(
                originalWorldTime,
                originalPlayerState,
                request.FacingDirection,
                "action.result.invalid_actor",
                definition);
        }

        if (!TargetMatches(definition.TargetKind, request.Target))
        {
            return Failure(
                originalWorldTime,
                originalPlayerState,
                request.FacingDirection,
                "action.result.invalid_target",
                definition);
        }

        var nextWorldTime = originalWorldTime;
        var nextPlayerState = PlayerRuntimeFactory.Clone(originalPlayerState);
        var startedNewDay = false;
        var forcedDayEnd = false;
        var appliedEffects = new List<GameActionAppliedEffect>();

        if (!CanApplyInventoryEffects(definition.Effects, originalPlayerState))
        {
            return Failure(
                originalWorldTime,
                originalPlayerState,
                request.FacingDirection,
                "action.result.missing_item",
                definition);
        }

        if (definition.TimeCostMinutes > 0)
        {
            var clockResult = WorldClock.Advance(nextWorldTime, definition.TimeCostMinutes);
            if (clockResult.ForcedDayEnd)
            {
                var needsResult = NeedsSystem.ApplySleep(nextPlayerState.Needs);
                nextPlayerState.Needs = needsResult.Needs;
            }

            nextWorldTime = clockResult.Time;
            startedNewDay = clockResult.StartedNewDay;
            forcedDayEnd = clockResult.ForcedDayEnd;
        }

        foreach (var effect in definition.Effects)
        {
            ApplyEffect(effect, ref nextWorldTime, nextPlayerState, appliedEffects, ref startedNewDay, ref forcedDayEnd);
        }

        nextPlayerState.Needs = NeedsSystem.Normalize(nextPlayerState.Needs);

        return new GameActionResult(
            succeeded: true,
            definition,
            nextWorldTime,
            nextPlayerState,
            definition.AnimationId,
            request.FacingDirection,
            string.IsNullOrWhiteSpace(definition.SuccessMessageKey) ? "action.result.completed" : definition.SuccessMessageKey,
            startedNewDay,
            forcedDayEnd,
            appliedEffects);
    }

    private static void ApplyEffect(
        GameActionEffectDefinition effect,
        ref WorldTime worldTime,
        PlayerRuntimeState playerState,
        List<GameActionAppliedEffect> appliedEffects,
        ref bool startedNewDay,
        ref bool forcedDayEnd)
    {
        if (effect.Type.Equals(GameActionEffectTypes.ChangeNeed, StringComparison.OrdinalIgnoreCase))
        {
            ApplyNeedChange(playerState.Needs, effect.NeedId, effect.Amount);
            playerState.Needs = NeedsSystem.Normalize(playerState.Needs);
            appliedEffects.Add(new GameActionAppliedEffect(effect.Type, effect.NeedId, effect.Amount));
            return;
        }

        if (effect.Type.Equals(GameActionEffectTypes.AddMoney, StringComparison.OrdinalIgnoreCase))
        {
            playerState.Money = Math.Max(0, playerState.Money + effect.Amount);
            appliedEffects.Add(new GameActionAppliedEffect(effect.Type, string.Empty, effect.Amount));
            return;
        }

        if (effect.Type.Equals(GameActionEffectTypes.RequireItem, StringComparison.OrdinalIgnoreCase))
        {
            var quantity = RequiredItemQuantity(effect);
            appliedEffects.Add(new GameActionAppliedEffect(effect.Type, effect.ItemId, quantity));
            return;
        }

        if (effect.Type.Equals(GameActionEffectTypes.ConsumeItem, StringComparison.OrdinalIgnoreCase))
        {
            var quantity = RequiredItemQuantity(effect);
            var result = InventorySystem.RemoveItem(playerState.Inventory, effect.ItemId, quantity);
            playerState.Inventory = result.Inventory;
            appliedEffects.Add(new GameActionAppliedEffect(effect.Type, effect.ItemId, -quantity));
            return;
        }

        if (effect.Type.Equals(GameActionEffectTypes.SleepToNextDay, StringComparison.OrdinalIgnoreCase))
        {
            var clockResult = WorldClock.SleepToNextDay(worldTime);
            var needsResult = NeedsSystem.ApplySleep(playerState.Needs);
            worldTime = clockResult.Time;
            playerState.Needs = needsResult.Needs;
            startedNewDay = true;
            forcedDayEnd = forcedDayEnd || clockResult.ForcedDayEnd;
            appliedEffects.Add(new GameActionAppliedEffect(effect.Type, string.Empty, 0));
        }
    }

    private static bool CanApplyInventoryEffects(IEnumerable<GameActionEffectDefinition> effects, PlayerRuntimeState playerState)
    {
        var requiredByItem = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var effect in effects)
        {
            if (!effect.Type.Equals(GameActionEffectTypes.RequireItem, StringComparison.OrdinalIgnoreCase) &&
                !effect.Type.Equals(GameActionEffectTypes.ConsumeItem, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var itemId = effect.ItemId?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            requiredByItem.TryGetValue(itemId, out var current);
            requiredByItem[itemId] = current + RequiredItemQuantity(effect);
        }

        foreach (var required in requiredByItem)
        {
            if (!InventorySystem.HasItem(playerState.Inventory, required.Key, required.Value))
            {
                return false;
            }
        }

        return true;
    }

    private static int RequiredItemQuantity(GameActionEffectDefinition effect)
    {
        return Math.Max(1, effect.Amount);
    }

    private static void ApplyNeedChange(CharacterNeedsBlock needs, string needId, int amount)
    {
        if (needId.Equals(GameActionNeedIds.Energy, StringComparison.OrdinalIgnoreCase))
        {
            needs.Energy += amount;
            return;
        }

        if (needId.Equals(GameActionNeedIds.Hunger, StringComparison.OrdinalIgnoreCase))
        {
            needs.Hunger += amount;
            return;
        }

        if (needId.Equals(GameActionNeedIds.Health, StringComparison.OrdinalIgnoreCase))
        {
            needs.Health += amount;
            return;
        }

        if (needId.Equals(GameActionNeedIds.Mood, StringComparison.OrdinalIgnoreCase))
        {
            needs.Mood += amount;
        }
    }

    private static bool TargetMatches(string requiredTargetKind, GameActionTarget target)
    {
        target ??= GameActionTarget.None();
        if (requiredTargetKind.Equals(GameActionTargetKinds.None, StringComparison.OrdinalIgnoreCase))
        {
            return target.Kind.Equals(GameActionTargetKinds.None, StringComparison.OrdinalIgnoreCase) ||
                target.Kind.Equals(GameActionTargetKinds.Self, StringComparison.OrdinalIgnoreCase);
        }

        if (requiredTargetKind.Equals(GameActionTargetKinds.Self, StringComparison.OrdinalIgnoreCase))
        {
            return target.Kind.Equals(GameActionTargetKinds.None, StringComparison.OrdinalIgnoreCase) ||
                target.Kind.Equals(GameActionTargetKinds.Self, StringComparison.OrdinalIgnoreCase);
        }

        return requiredTargetKind.Equals(target.Kind, StringComparison.OrdinalIgnoreCase);
    }

    private static GameActionResult Failure(
        WorldTime worldTime,
        PlayerRuntimeState playerState,
        CharacterFacingDirection facingDirection,
        string messageKey,
        GameActionDefinition definition = null)
    {
        definition ??= new GameActionDefinition
        {
            Id = string.Empty,
            LabelKey = string.Empty,
            AnimationId = CharacterAnimationIds.Idle
        };

        return new GameActionResult(
            succeeded: false,
            definition,
            worldTime,
            playerState,
            CharacterAnimationIds.Idle,
            facingDirection,
            messageKey,
            startedNewDay: false,
            forcedDayEnd: false,
            Array.Empty<GameActionAppliedEffect>());
    }
}
