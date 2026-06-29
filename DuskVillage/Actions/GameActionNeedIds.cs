using System;

namespace DuskVillage.Actions;

public static class GameActionNeedIds
{
    public const string Energy = "energy";
    public const string Hunger = "hunger";
    public const string Health = "health";
    public const string Mood = "mood";

    public static bool IsKnown(string needId)
    {
        return needId.Equals(Energy, StringComparison.OrdinalIgnoreCase) ||
            needId.Equals(Hunger, StringComparison.OrdinalIgnoreCase) ||
            needId.Equals(Health, StringComparison.OrdinalIgnoreCase) ||
            needId.Equals(Mood, StringComparison.OrdinalIgnoreCase);
    }
}
