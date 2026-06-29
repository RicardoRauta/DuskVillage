namespace DuskVillage.WorldAssets;

public sealed class SeasonalWorldAsset
{
    public SeasonalWorldAsset(
        string seasonId,
        string variantId,
        string id,
        string zipFileName,
        string zipPath,
        string entryPath,
        int tileWidth,
        int tileHeight,
        int frameDurationMilliseconds,
        bool zipExists,
        bool entryExists)
    {
        SeasonId = seasonId;
        VariantId = variantId;
        Id = id;
        ZipFileName = zipFileName;
        ZipPath = zipPath;
        EntryPath = entryPath;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        FrameDurationMilliseconds = frameDurationMilliseconds;
        ZipExists = zipExists;
        EntryExists = entryExists;
    }

    public string SeasonId { get; }

    public string VariantId { get; }

    public string Id { get; }

    public string StableId => $"{SeasonId}/{Id}";

    public string ZipFileName { get; }

    public string ZipPath { get; }

    public string EntryPath { get; }

    public int TileWidth { get; }

    public int TileHeight { get; }

    public int FrameDurationMilliseconds { get; }

    public bool ZipExists { get; }

    public bool EntryExists { get; }

    public bool IsAvailable => ZipExists && EntryExists;
}
