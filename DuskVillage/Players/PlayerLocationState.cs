namespace DuskVillage.Players;

public sealed class PlayerLocationState
{
    public string AreaId { get; set; } = "dusk_village";

    public int TileX { get; set; }

    public int TileY { get; set; }

    public PlayerLocationState Clone()
    {
        return new PlayerLocationState
        {
            AreaId = AreaId,
            TileX = TileX,
            TileY = TileY
        };
    }
}
