using System.Collections.Generic;

namespace DuskVillage.WorldAssets;

public sealed class SeasonalWorldAssetPackDefinition
{
    public string SeasonId { get; set; } = string.Empty;

    public string ZipFileName { get; set; } = string.Empty;

    public string VariantId { get; set; } = "default";

    public List<SeasonalWorldAssetDefinition> Assets { get; set; } = [];
}
