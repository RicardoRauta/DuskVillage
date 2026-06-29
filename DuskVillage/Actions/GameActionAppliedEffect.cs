namespace DuskVillage.Actions;

public sealed class GameActionAppliedEffect
{
    public GameActionAppliedEffect(string type, string needId, int amount)
    {
        Type = type;
        NeedId = needId;
        Amount = amount;
    }

    public string Type { get; }

    public string NeedId { get; }

    public int Amount { get; }
}
