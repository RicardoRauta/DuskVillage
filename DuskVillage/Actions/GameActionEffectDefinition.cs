namespace DuskVillage.Actions;

public sealed class GameActionEffectDefinition
{
    public string Type { get; set; } = string.Empty;

    public string NeedId { get; set; } = string.Empty;

    public string TileStateId { get; set; } = string.Empty;

    public string CropId { get; set; } = string.Empty;

    public string ItemId { get; set; } = string.Empty;

    public int Amount { get; set; }
}
