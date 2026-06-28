using System.IO;
using System.Text.Json;
using DuskVillage.Characters;

namespace DuskVillage.Saving;

public static class SaveGameSerializer
{
    public static string Serialize(SaveGame saveGame)
    {
        saveGame.Touch();
        CharacterPresetFactory.EnsureDefaultSkills(saveGame.PlayerState.CharacterPreset);
        return JsonSerializer.Serialize(saveGame, CharacterPresetSerializer.JsonOptions);
    }

    public static SaveGame Deserialize(string json)
    {
        var saveGame = JsonSerializer.Deserialize<SaveGame>(json, CharacterPresetSerializer.JsonOptions)
            ?? throw new InvalidDataException("Save file is empty.");

        Normalize(saveGame);
        return saveGame;
    }

    public static void Normalize(SaveGame saveGame)
    {
        saveGame.Metadata ??= new SaveMetadata();
        saveGame.WorldState ??= new SaveWorldState();
        saveGame.PlayerState ??= new SavePlayerState();
        saveGame.PlayerState.CharacterPreset ??= CharacterPresetFactory.CreateDefault();
        CharacterPresetSerializer.Normalize(saveGame.PlayerState.CharacterPreset);
        saveGame.Metadata.PlayerName = string.IsNullOrWhiteSpace(saveGame.PlayerState.CharacterPreset.FamilyName)
            ? saveGame.PlayerState.CharacterPreset.Name
            : $"{saveGame.PlayerState.CharacterPreset.Name} {saveGame.PlayerState.CharacterPreset.FamilyName}";
        saveGame.Metadata.CurrentDay = saveGame.WorldState.Day;
        saveGame.Metadata.CurrentTime = saveGame.WorldState.CurrentTime;
    }
}
