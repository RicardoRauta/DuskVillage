using System;

namespace DuskVillage.World;

public static class WorldClock
{
    private const int ActiveLateNightLimitMinutes =
        WorldCalendarRules.MinutesPerDay + WorldCalendarRules.LateNightLimitMinutes;

    public static WorldTime CreateDefault()
    {
        return Normalize(new WorldTime
        {
            Day = 1,
            TimeMinutes = WorldCalendarRules.StartOfDayMinutes
        });
    }

    public static WorldClockAdvanceResult Advance(WorldTime time, int minutes)
    {
        var normalized = Normalize(time);
        var advanceMinutes = Math.Max(0, minutes);
        var activeTargetMinutes = ToActiveDayMinutes(normalized.TimeMinutes) + advanceMinutes;

        if (activeTargetMinutes > ActiveLateNightLimitMinutes)
        {
            return new WorldClockAdvanceResult(StartNextDay(normalized), startedNewDay: true, forcedDayEnd: true);
        }

        var next = Normalize(new WorldTime
        {
            Day = normalized.Day,
            TimeMinutes = WorldCalendarRules.NormalizeTimeMinutes(activeTargetMinutes)
        });

        return new WorldClockAdvanceResult(next, startedNewDay: false, forcedDayEnd: false);
    }

    public static WorldClockAdvanceResult SleepToNextDay(WorldTime time)
    {
        return new WorldClockAdvanceResult(StartNextDay(Normalize(time)), startedNewDay: true, forcedDayEnd: false);
    }

    public static string FormatTime(int timeMinutes)
    {
        var normalized = WorldCalendarRules.NormalizeTimeMinutes(timeMinutes);
        return $"{normalized / 60:00}:{normalized % 60:00}";
    }

    public static WorldTime Normalize(WorldTime time)
    {
        var day = WorldCalendarRules.NormalizeDay(time?.Day ?? 1);
        return new WorldTime
        {
            Day = day,
            TimeMinutes = WorldCalendarRules.NormalizeTimeMinutes(time?.TimeMinutes ?? WorldCalendarRules.StartOfDayMinutes),
            CurrentSeason = WorldCalendarRules.GetSeasonForDay(day),
            Year = WorldCalendarRules.GetYear(day)
        };
    }

    private static WorldTime StartNextDay(WorldTime time)
    {
        var nextDay = WorldCalendarRules.NormalizeDay(time.Day + 1);
        return Normalize(new WorldTime
        {
            Day = nextDay,
            TimeMinutes = WorldCalendarRules.StartOfDayMinutes
        });
    }

    private static int ToActiveDayMinutes(int timeMinutes)
    {
        var normalized = WorldCalendarRules.NormalizeTimeMinutes(timeMinutes);
        return normalized < WorldCalendarRules.StartOfDayMinutes
            ? normalized + WorldCalendarRules.MinutesPerDay
            : normalized;
    }
}
