using DuskVillage.Saving;

namespace DuskVillage.Core;

public sealed class GameSessionSummary
{
    public string Source { get; set; } = "new_game";

    public string PlayerName { get; set; } = "Alden";

    public string AgeCategoryId { get; set; } = "young_adult";

    public string OriginId { get; set; } = "newcomer";

    public string SlotId { get; set; } = string.Empty;

    public int CurrentDay { get; set; } = 1;

    public string CurrentTime { get; set; } = "06:00";

    public static GameSessionSummary FromNewGame(NewGameOptions options)
    {
        return new GameSessionSummary
        {
            Source = "new_game",
            PlayerName = string.IsNullOrWhiteSpace(options.PlayerName) ? "Alden" : options.PlayerName.Trim(),
            AgeCategoryId = options.AgeCategoryId,
            OriginId = options.OriginId,
            CurrentDay = 1,
            CurrentTime = "06:00"
        };
    }

    public static GameSessionSummary FromSaveSlot(SaveSlotSummary slot)
    {
        return new GameSessionSummary
        {
            Source = "save_slot",
            PlayerName = string.IsNullOrWhiteSpace(slot.PlayerName) ? "Unknown" : slot.PlayerName,
            SlotId = slot.SlotId,
            CurrentDay = slot.CurrentDay,
            CurrentTime = string.IsNullOrWhiteSpace(slot.CurrentTime) ? "06:00" : slot.CurrentTime
        };
    }
}
