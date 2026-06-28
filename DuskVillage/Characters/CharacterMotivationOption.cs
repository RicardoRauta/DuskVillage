namespace DuskVillage.Characters;

public sealed class CharacterMotivationOption
{
    public CharacterMotivationOption(string value, string labelKey, string descriptionKey, string futureTraitId)
    {
        Value = value;
        LabelKey = labelKey;
        DescriptionKey = descriptionKey;
        FutureTraitId = futureTraitId;
    }

    public string Value { get; }

    public string LabelKey { get; }

    public string DescriptionKey { get; }

    public string FutureTraitId { get; }
}
