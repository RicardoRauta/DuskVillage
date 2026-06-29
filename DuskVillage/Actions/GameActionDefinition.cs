using System.Collections.Generic;
using DuskVillage.Animations;

namespace DuskVillage.Actions;

public sealed class GameActionDefinition
{
    public string Id { get; set; } = string.Empty;

    public string LabelKey { get; set; } = string.Empty;

    public string DescriptionKey { get; set; } = string.Empty;

    public string TargetKind { get; set; } = GameActionTargetKinds.None;

    public string AnimationId { get; set; } = CharacterAnimationIds.Idle;

    public int TimeCostMinutes { get; set; }

    public string RequiredToolId { get; set; } = string.Empty;

    public string SuccessMessageKey { get; set; } = "action.result.completed";

    public List<string> Tags { get; set; } = new();

    public List<GameActionEffectDefinition> Effects { get; set; } = new();
}
