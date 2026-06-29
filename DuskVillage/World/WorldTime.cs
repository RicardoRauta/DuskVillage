namespace DuskVillage.World;

public class WorldTime
{
    public int Day { get; set; } = 1;

    public int TimeMinutes { get; set; } = WorldCalendarRules.StartOfDayMinutes;

    public string CurrentSeason { get; set; } = WorldCalendarRules.Spring;

    public int Year { get; set; } = 1;

    public string CurrentTime => WorldClock.FormatTime(TimeMinutes);

    public int DayOfSeason => WorldCalendarRules.GetDayOfSeason(Day);

    public WorldTime Clone()
    {
        return new WorldTime
        {
            Day = Day,
            TimeMinutes = TimeMinutes,
            CurrentSeason = CurrentSeason,
            Year = Year
        };
    }
}
