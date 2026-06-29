using DuskVillage.Characters;
using DuskVillage.Saving;
using DuskVillage.World;

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

    public WorldTime WorldTime { get; set; } = WorldClock.CreateDefault();

    public int CurrentDay => WorldTime.Day;

    public string CurrentTime => WorldTime.CurrentTime;

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
            WorldTime = WorldClock.CreateDefault()
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
            WorldTime = WorldClock.Normalize(saveGame.WorldState)
        };
    }
}
