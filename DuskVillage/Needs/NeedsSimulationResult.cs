using DuskVillage.Characters;

namespace DuskVillage.Needs;

public sealed class NeedsSimulationResult
{
    public NeedsSimulationResult(
        CharacterNeedsBlock needs,
        bool energyChanged,
        bool hungerChanged,
        bool healthChanged,
        bool moodChanged)
    {
        Needs = needs;
        EnergyChanged = energyChanged;
        HungerChanged = hungerChanged;
        HealthChanged = healthChanged;
        MoodChanged = moodChanged;
    }

    public CharacterNeedsBlock Needs { get; }

    public bool EnergyChanged { get; }

    public bool HungerChanged { get; }

    public bool HealthChanged { get; }

    public bool MoodChanged { get; }

    public bool IsHungry => Needs.Hunger <= NeedsSimulationRules.LowHungerThreshold;

    public bool IsStarving => Needs.Hunger <= NeedsSimulationRules.StarvingThreshold;

    public bool IsExhausted => Needs.Energy <= 0;
}
