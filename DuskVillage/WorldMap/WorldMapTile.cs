namespace DuskVillage.WorldMap;

public sealed class WorldMapTile
{
    public int X { get; set; }

    public int Y { get; set; }

    public string TypeId { get; set; } = WorldMapTileTypeIds.Grass;

    public string StateId { get; set; } = WorldMapTileStateIds.Empty;

    public string CropId { get; set; } = string.Empty;

    public WorldMapTile Clone()
    {
        return new WorldMapTile
        {
            X = X,
            Y = Y,
            TypeId = TypeId,
            StateId = StateId,
            CropId = CropId
        };
    }
}
