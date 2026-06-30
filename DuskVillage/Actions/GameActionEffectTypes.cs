using System;

namespace DuskVillage.Actions;

public static class GameActionEffectTypes
{
    public const string ChangeNeed = "changeNeed";
    public const string AddMoney = "addMoney";
    public const string SleepToNextDay = "sleepToNextDay";
    public const string SetTileState = "setTileState";
    public const string PlantCrop = "plantCrop";
    public const string WaterTile = "waterTile";
    public const string RequireItem = "requireItem";
    public const string ConsumeItem = "consumeItem";

    public static bool IsKnown(string effectType)
    {
        return effectType.Equals(ChangeNeed, StringComparison.OrdinalIgnoreCase) ||
            effectType.Equals(AddMoney, StringComparison.OrdinalIgnoreCase) ||
            effectType.Equals(SleepToNextDay, StringComparison.OrdinalIgnoreCase) ||
            effectType.Equals(SetTileState, StringComparison.OrdinalIgnoreCase) ||
            effectType.Equals(PlantCrop, StringComparison.OrdinalIgnoreCase) ||
            effectType.Equals(WaterTile, StringComparison.OrdinalIgnoreCase) ||
            effectType.Equals(RequireItem, StringComparison.OrdinalIgnoreCase) ||
            effectType.Equals(ConsumeItem, StringComparison.OrdinalIgnoreCase);
    }
}
