using System.Collections.Generic;

namespace DuskVillage.World;

public static class WorldCalendarRules
{
    public const string Spring = "spring";
    public const string Summer = "summer";
    public const string Autumn = "autumn";
    public const string Winter = "winter";

    public const int MinutesPerDay = 1440;
    public const int StartOfDayMinutes = 360;
    public const int LateNightLimitMinutes = 120;
    public const int DaysPerSeason = 28;
    public const int SeasonsPerYear = 4;
    public const int DaysPerYear = DaysPerSeason * SeasonsPerYear;

    public static IReadOnlyList<string> Seasons { get; } =
    [
        Spring,
        Summer,
        Autumn,
        Winter
    ];

    public static int GetYear(int day)
    {
        var normalizedDay = NormalizeDay(day);
        return ((normalizedDay - 1) / DaysPerYear) + 1;
    }

    public static int GetDayOfSeason(int day)
    {
        var normalizedDay = NormalizeDay(day);
        return ((normalizedDay - 1) % DaysPerSeason) + 1;
    }

    public static string GetSeasonForDay(int day)
    {
        var normalizedDay = NormalizeDay(day);
        var seasonIndex = ((normalizedDay - 1) / DaysPerSeason) % SeasonsPerYear;
        return Seasons[seasonIndex];
    }

    public static int NormalizeDay(int day)
    {
        return day < 1 ? 1 : day;
    }

    public static int NormalizeTimeMinutes(int timeMinutes)
    {
        var normalized = timeMinutes % MinutesPerDay;
        return normalized < 0 ? normalized + MinutesPerDay : normalized;
    }
}
