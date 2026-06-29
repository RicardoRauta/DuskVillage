using DuskVillage.Actions;

namespace DuskVillage.WorldMap;

public sealed class WorldMapActionResult
{
    public WorldMapActionResult(GameActionResult actionResult, WorldMapState mapState, string messageKey)
    {
        ActionResult = actionResult;
        MapState = mapState;
        MessageKey = messageKey;
    }

    public bool Succeeded => ActionResult?.Succeeded == true;

    public GameActionResult ActionResult { get; }

    public WorldMapState MapState { get; }

    public string MessageKey { get; }
}
