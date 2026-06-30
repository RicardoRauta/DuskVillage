namespace DuskVillage.InventoryAssets;

public sealed class InventoryUiAsset
{
    public InventoryUiAsset(
        string skinId,
        string variantId,
        string id,
        string zipFileName,
        string zipPath,
        string entryPath,
        int width,
        int height,
        int frameDurationMilliseconds,
        bool zipExists,
        bool entryExists)
    {
        SkinId = skinId;
        VariantId = variantId;
        Id = id;
        ZipFileName = zipFileName;
        ZipPath = zipPath;
        EntryPath = entryPath;
        Width = width;
        Height = height;
        FrameDurationMilliseconds = frameDurationMilliseconds;
        ZipExists = zipExists;
        EntryExists = entryExists;
    }

    public string SkinId { get; }

    public string VariantId { get; }

    public string Id { get; }

    public string StableId => $"{SkinId}/{Id}";

    public string ZipFileName { get; }

    public string ZipPath { get; }

    public string EntryPath { get; }

    public int Width { get; }

    public int Height { get; }

    public int FrameDurationMilliseconds { get; }

    public bool ZipExists { get; }

    public bool EntryExists { get; }

    public bool IsAvailable => ZipExists && EntryExists;
}
