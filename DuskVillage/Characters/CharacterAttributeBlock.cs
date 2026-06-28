namespace DuskVillage.Characters;

public sealed class CharacterAttributeBlock
{
    public int Strength { get; set; } = 4;

    public int Agility { get; set; } = 4;

    public int Constitution { get; set; } = 4;

    public int Intelligence { get; set; } = 4;

    public int Charisma { get; set; } = 4;

    public int Wisdom { get; set; } = 4;

    public int Total => Strength + Agility + Constitution + Intelligence + Charisma + Wisdom;

    public CharacterAttributeBlock Clone()
    {
        return new CharacterAttributeBlock
        {
            Strength = Strength,
            Agility = Agility,
            Constitution = Constitution,
            Intelligence = Intelligence,
            Charisma = Charisma,
            Wisdom = Wisdom
        };
    }

    public int GetValue(string attributeId)
    {
        return attributeId switch
        {
            CharacterAttributeIds.Strength => Strength,
            CharacterAttributeIds.Agility => Agility,
            CharacterAttributeIds.Constitution => Constitution,
            CharacterAttributeIds.Intelligence => Intelligence,
            CharacterAttributeIds.Charisma => Charisma,
            CharacterAttributeIds.Wisdom => Wisdom,
            _ => 0
        };
    }

    public void SetValue(string attributeId, int value)
    {
        switch (attributeId)
        {
            case CharacterAttributeIds.Strength:
                Strength = value;
                break;
            case CharacterAttributeIds.Agility:
                Agility = value;
                break;
            case CharacterAttributeIds.Constitution:
                Constitution = value;
                break;
            case CharacterAttributeIds.Intelligence:
                Intelligence = value;
                break;
            case CharacterAttributeIds.Charisma:
                Charisma = value;
                break;
            case CharacterAttributeIds.Wisdom:
                Wisdom = value;
                break;
        }
    }
}
