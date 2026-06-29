using Microsoft.Xna.Framework;

namespace DuskVillage.Rendering;

public sealed class WorldMapViewport
{
    public WorldMapViewport(Rectangle bounds, int tileSize)
    {
        Bounds = bounds;
        TileSize = tileSize;
    }

    public Rectangle Bounds { get; }

    public int TileSize { get; }

    public Rectangle TileBounds(int tileX, int tileY)
    {
        return new Rectangle(
            Bounds.X + tileX * TileSize,
            Bounds.Y + tileY * TileSize,
            TileSize,
            TileSize);
    }
}
