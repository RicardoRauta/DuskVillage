using DuskVillage.World;

namespace DuskVillage.Saving;

public sealed class SaveWorldState : WorldTime
{
    public static SaveWorldState CreateDefault()
    {
        return FromWorldTime(WorldClock.CreateDefault());
    }

    public static SaveWorldState FromWorldTime(WorldTime time)
    {
        var normalized = WorldClock.Normalize(time);
        return new SaveWorldState
        {
            Day = normalized.Day,
            TimeMinutes = normalized.TimeMinutes,
            CurrentSeason = normalized.CurrentSeason,
            Year = normalized.Year
        };
    }

    public void Apply(WorldTime time)
    {
        var normalized = WorldClock.Normalize(time);
        Day = normalized.Day;
        TimeMinutes = normalized.TimeMinutes;
        CurrentSeason = normalized.CurrentSeason;
        Year = normalized.Year;
    }
}
