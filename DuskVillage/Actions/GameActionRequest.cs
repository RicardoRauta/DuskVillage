using DuskVillage.Animations;
using DuskVillage.Players;

namespace DuskVillage.Actions;

public sealed class GameActionRequest
{
    public string ActionId { get; set; } = string.Empty;

    public string ActorEntityId { get; set; } = PlayerRuntimeFactory.DefaultPlayerEntityId;

    public CharacterFacingDirection FacingDirection { get; set; } = CharacterFacingDirection.Down;

    public GameActionTarget Target { get; set; } = GameActionTarget.None();
}
