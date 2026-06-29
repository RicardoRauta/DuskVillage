using System;

namespace DuskVillage.Actions;

public static class GameActionEffectTypes
{
    public const string ChangeNeed = "changeNeed";
    public const string AddMoney = "addMoney";
    public const string SleepToNextDay = "sleepToNextDay";

    public static bool IsKnown(string effectType)
    {
        return effectType.Equals(ChangeNeed, StringComparison.OrdinalIgnoreCase) ||
            effectType.Equals(AddMoney, StringComparison.OrdinalIgnoreCase) ||
            effectType.Equals(SleepToNextDay, StringComparison.OrdinalIgnoreCase);
    }
}
