using DuskVillage.Characters;
using DuskVillage.Saving;

namespace DuskVillage.Core;

public sealed class GameSessionSummary
{
    public string Source { get; set; } = "new_game";

    public CharacterPreset PlayerPreset { get; set; } = CharacterPresetFactory.CreateDefault();

    public string PlayerName { get; set; } = "Alden";

    public string AgeCategoryId { get; set; } = "young_adult";

    public string OriginId { get; set; } = "newcomer";

    public string SlotId { get; set; } = string.Empty;

    public int SlotNumber { get; set; }

    public int CurrentDay { get; set; } = 1;

    public string CurrentTime { get; set; } = "06:00";

    public static GameSessionSummary FromNewGame(NewGameOptions options)
    {
        var preset = options.CharacterPreset.Clone();
        CharacterPresetSerializer.Normalize(preset);
        return new GameSessionSummary
        {
            Source = "new_game",
            PlayerPreset = preset,
            PlayerName = preset.Name,
            AgeCategoryId = preset.AgeCategoryId,
            OriginId = preset.OriginId,
            CurrentDay = 1,
            CurrentTime = "06:00"
        };
    }

    public static GameSessionSummary FromSaveSlot(SaveSlotSummary slot, SaveGame saveGame)
    {
        var preset = saveGame.PlayerState.CharacterPreset.Clone();
        CharacterPresetSerializer.Normalize(preset);
        return new GameSessionSummary
        {
            Source = "save_slot",
            PlayerPreset = preset,
            PlayerName = string.IsNullOrWhiteSpace(preset.Name) ? slot.PlayerName : preset.Name,
            SlotId = slot.SlotId,
            SlotNumber = slot.SlotNumber,
            AgeCategoryId = preset.AgeCategoryId,
            OriginId = preset.OriginId,
            CurrentDay = saveGame.WorldState.Day,
            CurrentTime = saveGame.WorldState.CurrentTime
        };
    }
}
