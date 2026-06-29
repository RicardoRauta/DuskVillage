namespace DuskVillage.World;

public sealed class WorldClockAdvanceResult
{
    public WorldClockAdvanceResult(WorldTime time, bool startedNewDay, bool forcedDayEnd)
    {
        Time = time;
        StartedNewDay = startedNewDay;
        ForcedDayEnd = forcedDayEnd;
    }

    public WorldTime Time { get; }

    public bool StartedNewDay { get; }

    public bool ForcedDayEnd { get; }
}
