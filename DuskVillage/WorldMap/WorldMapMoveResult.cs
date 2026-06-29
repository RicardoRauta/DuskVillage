using DuskVillage.Players;

namespace DuskVillage.WorldMap;

public sealed class WorldMapMoveResult
{
    public WorldMapMoveResult(bool moved, PlayerLocationState location, string messageKey)
    {
        Moved = moved;
        Location = location;
        MessageKey = messageKey;
    }

    public bool Moved { get; }

    public PlayerLocationState Location { get; }

    public string MessageKey { get; }
}
