using System.Collections.Generic;

namespace DuskVillage.WorldAssets;

public static class SeasonalWorldAssetIds
{
    public const string TerrainWang = "terrain_wang";
    public const string TerrainObjects = "terrain_objects";
    public const string Trees = "trees";
    public const string TallGrassEffect = "tall_grass_effect";

    public static IReadOnlyList<string> BuiltInIds { get; } =
    [
        TerrainWang,
        TerrainObjects,
        Trees,
        TallGrassEffect
    ];
}
