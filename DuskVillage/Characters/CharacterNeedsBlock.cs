namespace DuskVillage.Characters;

public sealed class CharacterNeedsBlock
{
    public int Energy { get; set; } = 100;

    public int MaxEnergy { get; set; } = 100;

    public int Hunger { get; set; } = 100;

    public int MaxHunger { get; set; } = 100;

    public int Health { get; set; } = 100;

    public int MaxHealth { get; set; } = 100;

    public int Mood { get; set; } = 75;

    public int MaxMood { get; set; } = 100;

    public CharacterNeedsBlock Clone()
    {
        return new CharacterNeedsBlock
        {
            Energy = Energy,
            MaxEnergy = MaxEnergy,
            Hunger = Hunger,
            MaxHunger = MaxHunger,
            Health = Health,
            MaxHealth = MaxHealth,
            Mood = Mood,
            MaxMood = MaxMood
        };
    }
}
