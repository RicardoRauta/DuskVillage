using System;
using System.Collections.Generic;
using DuskVillage.Animations;
using DuskVillage.Characters;
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
