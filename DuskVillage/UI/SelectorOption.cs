namespace DuskVillage.UI;

public sealed class SelectorOption
{
    public SelectorOption(string value, string labelKey)
    {
        Value = value;
        LabelKey = labelKey;
    }

    public string Value { get; }

    public string LabelKey { get; }
}
