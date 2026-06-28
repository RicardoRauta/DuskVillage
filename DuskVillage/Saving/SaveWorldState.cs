namespace DuskVillage.Saving;

public sealed class SaveWorldState
{
    public int Day { get; set; } = 1;

    public int TimeMinutes { get; set; } = 360;

    public string CurrentTime => $"{TimeMinutes / 60:00}:{TimeMinutes % 60:00}";
}
