using System.Collections.Generic;
using DuskVillage.Animations;
using DuskVillage.Players;
using DuskVillage.World;

namespace DuskVillage.Actions;

public sealed class GameActionResult
{
    public GameActionResult(
        bool succeeded,
        GameActionDefinition definition,
        WorldTime worldTime,
        PlayerRuntimeState playerState,
        string animationId,
        CharacterFacingDirection facingDirection,
        string messageKey,
        bool startedNewDay,
        bool forcedDayEnd,
        IReadOnlyList<GameActionAppliedEffect> appliedEffects)
    {
        Succeeded = succeeded;
        Definition = definition;
        WorldTime = worldTime;
        PlayerState = playerState;
        AnimationId = animationId;
        FacingDirection = facingDirection;
        MessageKey = messageKey;
        StartedNewDay = startedNewDay;
        ForcedDayEnd = forcedDayEnd;
        AppliedEffects = appliedEffects;
    }

    public bool Succeeded { get; }

    public GameActionDefinition Definition { get; }

    public WorldTime WorldTime { get; }

    public PlayerRuntimeState PlayerState { get; }

    public string AnimationId { get; }

    public CharacterFacingDirection FacingDirection { get; }

    public string MessageKey { get; }

    public bool StartedNewDay { get; }

    public bool ForcedDayEnd { get; }

    public IReadOnlyList<GameActionAppliedEffect> AppliedEffects { get; }
}
