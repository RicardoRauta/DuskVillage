namespace DuskVillage.Characters;

public sealed class CharacterOption
{
    public CharacterOption(string value, string labelKey)
    {
        Value = value;
        LabelKey = labelKey;
    }

    public string Value { get; }

    public string LabelKey { get; }
}
