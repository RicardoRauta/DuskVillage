using DuskVillage.Animations;
using DuskVillage.Players;

namespace DuskVillage.WorldMap;

public sealed class WorldMapContinuousMovementResult
{
    public WorldMapContinuousMovementResult(
        bool moved,
        bool blocked,
        PlayerLocationState location,
        CharacterFacingDirection? facingDirection,
        string messageKey)
    {
        Moved = moved;
        Blocked = blocked;
        Location = location;
        FacingDirection = facingDirection;
        MessageKey = messageKey;
    }

    public bool Moved { get; }

    public bool Blocked { get; }

    public PlayerLocationState Location { get; }

    public CharacterFacingDirection? FacingDirection { get; }

    public string MessageKey { get; }
}
