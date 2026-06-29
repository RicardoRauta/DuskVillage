using DuskVillage.World;
using DuskVillage.WorldMap;

namespace DuskVillage.Saving;

public sealed class SaveWorldState : WorldTime
{
    public WorldMapState Map { get; set; } = WorldMapFactory.CreateDefault();

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
            Year = normalized.Year,
            Map = WorldMapFactory.CreateDefault()
        };
    }

    public void Apply(WorldTime time)
    {
        var normalized = WorldClock.Normalize(time);
        Day = normalized.Day;
        TimeMinutes = normalized.TimeMinutes;
        CurrentSeason = normalized.CurrentSeason;
        Year = normalized.Year;
        Map = WorldMapFactory.Normalize(Map);
    }
}
