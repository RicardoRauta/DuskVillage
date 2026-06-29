namespace DuskVillage.Needs;

public static class NeedsSimulationRules
{
    public const int HungerLossPerHour = 3;
    public const int EnergyLossPerHour = 2;
    public const int LowHungerThreshold = 25;
    public const int StarvingThreshold = 0;
    public const int LowHungerMoodLossPerHour = 2;
    public const int StarvingHealthLossPerHour = 3;
    public const int ExhaustedHealthLossPerHour = 1;
    public const int SleepHungerCost = 8;
    public const int SleepMoodRecovery = 6;
    public const int SleepHealthRecovery = 4;
}
