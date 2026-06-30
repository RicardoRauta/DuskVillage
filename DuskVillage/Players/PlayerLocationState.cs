namespace DuskVillage.Players;

public sealed class PlayerLocationState
{
    public string AreaId { get; set; } = "dusk_village";

    public int TileX { get; set; }

    public int TileY { get; set; }

    public double? PositionX { get; set; }

    public double? PositionY { get; set; }

    public double GetPositionX()
    {
        return PositionX ?? TileX + 0.5;
    }

    public double GetPositionY()
    {
        return PositionY ?? TileY + 0.5;
    }

    public void EnsurePosition()
    {
        PositionX ??= TileX + 0.5;
        PositionY ??= TileY + 0.5;
        SyncTileFromPosition();
    }

    public void SetPosition(double x, double y)
    {
        PositionX = x;
        PositionY = y;
        SyncTileFromPosition();
    }

    public void SetTile(int tileX, int tileY)
    {
        TileX = tileX;
        TileY = tileY;
        PositionX = tileX + 0.5;
        PositionY = tileY + 0.5;
    }

    public void SyncTileFromPosition()
    {
        TileX = PositionToTile(GetPositionX());
        TileY = PositionToTile(GetPositionY());
    }

    public PlayerLocationState Clone()
    {
        return new PlayerLocationState
        {
            AreaId = AreaId,
            TileX = TileX,
            TileY = TileY,
            PositionX = PositionX,
            PositionY = PositionY
        };
    }

    public static int PositionToTile(double position)
    {
        return (int)System.Math.Floor(position);
    }
}
