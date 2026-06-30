using System.Collections.Generic;

namespace DuskVillage.InventoryAssets;

public sealed class InventoryUiSkinDefinition
{
    public string Id { get; set; } = string.Empty;

    public string LabelKey { get; set; } = string.Empty;

    public string ZipFileName { get; set; } = string.Empty;

    public string VariantId { get; set; } = string.Empty;

    public List<InventoryUiAssetDefinition> Assets { get; set; } = new();
}
