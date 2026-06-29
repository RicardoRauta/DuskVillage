namespace DuskVillage.WorldAssets;

public sealed class SeasonalWorldAssetDefinition
{
    public string Id { get; set; } = string.Empty;

    public string EntryPath { get; set; } = string.Empty;

    public int TileWidth { get; set; } = 16;

    public int TileHeight { get; set; } = 16;

    public int FrameDurationMilliseconds { get; set; }
}
