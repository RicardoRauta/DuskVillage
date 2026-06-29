namespace DuskVillage.Actions;

public sealed class GameActionEffectDefinition
{
    public string Type { get; set; } = string.Empty;

    public string NeedId { get; set; } = string.Empty;

    public int Amount { get; set; }
}
