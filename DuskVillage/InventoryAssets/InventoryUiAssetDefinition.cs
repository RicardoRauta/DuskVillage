namespace DuskVillage.InventoryAssets;

public sealed class InventoryUiAssetDefinition
{
    public string Id { get; set; } = string.Empty;

    public string EntryPath { get; set; } = string.Empty;

    public int Width { get; set; }

    public int Height { get; set; }

    public int FrameDurationMilliseconds { get; set; }
}
