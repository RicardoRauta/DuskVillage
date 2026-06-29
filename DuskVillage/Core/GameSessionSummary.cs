using DuskVillage.Characters;
using DuskVillage.Players;
using DuskVillage.Saving;
using DuskVillage.World;
using DuskVillage.WorldMap;

namespace DuskVillage.Core;

public sealed class GameSessionSummary
{
    public string Source { get; set; } = "new_game";

    public PlayerRuntimeState PlayerState { get; set; } = PlayerRuntimeFactory.CreateNew(CharacterPresetFactory.CreateDefault());

    public CharacterPreset PlayerPreset
    {
        get => PlayerState.CharacterPreset;
        set
        {
            PlayerState.CharacterPreset = value ?? CharacterPresetFactory.CreateDefault();
            PlayerRuntimeFactory.Normalize(PlayerState);
        }
    }

    public string PlayerName { get; set; } = "Alden";

    public string AgeCategoryId { get; set; } = "young_adult";

    public string OriginId { get; set; } = "newcomer";

    public string SlotId { get; set; } = string.Empty;

    public int SlotNumber { get; set; }

    public WorldTime WorldTime { get; set; } = WorldClock.CreateDefault();

    public WorldMapState WorldMap { get; set; } = WorldMapFactory.CreateDefault();

    public int CurrentDay => WorldTime.Day;

    public string CurrentTime => WorldTime.CurrentTime;

    public static GameSessionSummary FromNewGame(NewGameOptions options)
    {
        var preset = options.CharacterPreset.Clone();
        CharacterPresetSerializer.Normalize(preset);
        var playerState = PlayerRuntimeFactory.CreateNew(preset);
        return new GameSessionSummary
        {
            Source = "new_game",
            PlayerState = playerState,
            PlayerName = preset.Name,
            AgeCategoryId = preset.AgeCategoryId,
            OriginId = preset.OriginId,
            WorldTime = WorldClock.CreateDefault(),
            WorldMap = WorldMapFactory.CreateDefault()
        };
    }

    public static GameSessionSummary FromSaveSlot(SaveSlotSummary slot, SaveGame saveGame)
    {
        var playerState = PlayerRuntimeFactory.Clone(saveGame.PlayerState);
        var preset = playerState.CharacterPreset.Clone();
        CharacterPresetSerializer.Normalize(preset);
        playerState.CharacterPreset = preset;
        return new GameSessionSummary
        {
            Source = "save_slot",
            PlayerState = playerState,
            PlayerName = string.IsNullOrWhiteSpace(preset.Name) ? slot.PlayerName : preset.Name,
            SlotId = slot.SlotId,
            SlotNumber = slot.SlotNumber,
            AgeCategoryId = preset.AgeCategoryId,
            OriginId = preset.OriginId,
            WorldTime = WorldClock.Normalize(saveGame.WorldState),
            WorldMap = WorldMapFactory.Normalize(saveGame.WorldState.Map)
        };
    }
}
