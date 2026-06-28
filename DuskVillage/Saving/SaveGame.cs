using System;
using DuskVillage.Characters;
using DuskVillage.Core;

namespace DuskVillage.Saving;

public sealed class SaveGame
{
    public SaveMetadata Metadata { get; set; } = new();

    public SaveWorldState WorldState { get; set; } = new();

    public SavePlayerState PlayerState { get; set; } = new();

    public static SaveGame CreateNew(CharacterPreset preset)
    {
        var now = DateTimeOffset.UtcNow;
        var playerPreset = preset.Clone();
        return new SaveGame
        {
            Metadata = new SaveMetadata
            {
                SaveVersion = SaveMetadata.CurrentSaveVersion,
                GameVersion = GameDirectories.GameVersion,
                CreatedAt = now,
                LastPlayedAt = now,
                PlayerName = FullName(playerPreset),
                CurrentDay = 1,
                CurrentTime = "06:00"
            },
            WorldState = new SaveWorldState
            {
                Day = 1,
                TimeMinutes = 360
            },
            PlayerState = new SavePlayerState
            {
                EntityId = "player_main",
                CharacterPreset = playerPreset
            }
        };
    }

    public void Touch()
    {
        Metadata.LastPlayedAt = DateTimeOffset.UtcNow;
        Metadata.PlayerName = FullName(PlayerState.CharacterPreset);
        Metadata.CurrentDay = WorldState.Day;
        Metadata.CurrentTime = WorldState.CurrentTime;
    }

    private static string FullName(CharacterPreset preset)
    {
        return string.IsNullOrWhiteSpace(preset.FamilyName)
            ? preset.Name
            : $"{preset.Name} {preset.FamilyName}";
    }
}
