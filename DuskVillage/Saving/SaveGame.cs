using System;
using DuskVillage.Characters;
using DuskVillage.Core;
using DuskVillage.Players;
using DuskVillage.World;

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
        var worldTime = WorldClock.CreateDefault();
        return new SaveGame
        {
            Metadata = new SaveMetadata
            {
                SaveVersion = SaveMetadata.CurrentSaveVersion,
                GameVersion = GameDirectories.GameVersion,
                CreatedAt = now,
                LastPlayedAt = now,
                PlayerName = FullName(playerPreset),
                CurrentDay = worldTime.Day,
                CurrentTime = worldTime.CurrentTime
            },
            WorldState = SaveWorldState.FromWorldTime(worldTime),
            PlayerState = SavePlayerState.CreateNew(playerPreset)
        };
    }

    public void Touch()
    {
        Metadata ??= new SaveMetadata();
        WorldState ??= SaveWorldState.CreateDefault();
        PlayerState ??= SavePlayerState.CreateNew(CharacterPresetFactory.CreateDefault());
        PlayerRuntimeFactory.Normalize(PlayerState);
        WorldState.Apply(WorldState);
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
