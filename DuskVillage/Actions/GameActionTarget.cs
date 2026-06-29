namespace DuskVillage.Actions;

public sealed class GameActionTarget
{
    public string Kind { get; set; } = GameActionTargetKinds.None;

    public string EntityId { get; set; } = string.Empty;

    public string AreaId { get; set; } = string.Empty;

    public int TileX { get; set; }

    public int TileY { get; set; }

    public static GameActionTarget None()
    {
        return new GameActionTarget();
    }

    public static GameActionTarget Self(string entityId)
    {
        return new GameActionTarget
        {
            Kind = GameActionTargetKinds.Self,
            EntityId = entityId
        };
    }

    public static GameActionTarget Tile(string areaId, int tileX, int tileY)
    {
        return new GameActionTarget
        {
            Kind = GameActionTargetKinds.Tile,
            AreaId = areaId,
            TileX = tileX,
            TileY = tileY
        };
    }
}
