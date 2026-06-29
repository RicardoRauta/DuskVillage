using System;
using DuskVillage.Characters;

namespace DuskVillage.Needs;

public static class NeedsSystem
{
    public static CharacterNeedsBlock Normalize(CharacterNeedsBlock needs)
    {
        needs ??= new CharacterNeedsBlock();
        needs.MaxEnergy = Math.Max(1, needs.MaxEnergy);
        needs.MaxHunger = Math.Max(1, needs.MaxHunger);
        needs.MaxHealth = Math.Max(1, needs.MaxHealth);
        needs.MaxMood = Math.Max(1, needs.MaxMood);
        needs.Energy = Math.Clamp(needs.Energy, 0, needs.MaxEnergy);
        needs.Hunger = Math.Clamp(needs.Hunger, 0, needs.MaxHunger);
        needs.Health = Math.Clamp(needs.Health, 0, needs.MaxHealth);
        needs.Mood = Math.Clamp(needs.Mood, 0, needs.MaxMood);
        return needs;
    }

    public static NeedsSimulationResult ApplyElapsedTime(CharacterNeedsBlock needs, int minutes)
    {
        var before = Normalize(needs?.Clone());
        var next = before.Clone();
        var ticks = HourTicks(minutes);

        if (ticks <= 0)
        {
            return Result(before, next);
        }

        next.Hunger -= NeedsSimulationRules.HungerLossPerHour * ticks;
        next.Energy -= NeedsSimulationRules.EnergyLossPerHour * ticks;

        if (next.Hunger <= NeedsSimulationRules.LowHungerThreshold)
        {
            next.Mood -= NeedsSimulationRules.LowHungerMoodLossPerHour * ticks;
        }

        if (next.Hunger <= NeedsSimulationRules.StarvingThreshold)
        {
            next.Health -= NeedsSimulationRules.StarvingHealthLossPerHour * ticks;
        }

        if (next.Energy <= 0)
        {
            next.Health -= NeedsSimulationRules.ExhaustedHealthLossPerHour * ticks;
        }

        return Result(before, Normalize(next));
    }

    public static NeedsSimulationResult ApplySleep(CharacterNeedsBlock needs)
    {
        var before = Normalize(needs?.Clone());
        var next = before.Clone();

        next.Energy = next.MaxEnergy;
        next.Hunger -= NeedsSimulationRules.SleepHungerCost;
        next.Mood += NeedsSimulationRules.SleepMoodRecovery;

        if (next.Hunger > NeedsSimulationRules.LowHungerThreshold)
        {
            next.Health += NeedsSimulationRules.SleepHealthRecovery;
        }

        return Result(before, Normalize(next));
    }

    private static int HourTicks(int minutes)
    {
        return minutes <= 0 ? 0 : (int)Math.Ceiling(minutes / 60f);
    }

    private static NeedsSimulationResult Result(CharacterNeedsBlock before, CharacterNeedsBlock after)
    {
        return new NeedsSimulationResult(
            after,
            energyChanged: before.Energy != after.Energy,
            hungerChanged: before.Hunger != after.Hunger,
            healthChanged: before.Health != after.Health,
            moodChanged: before.Mood != after.Mood);
    }
}
