using DuskVillage.Characters;

namespace DuskVillage.Core;

public sealed class NewGameOptions
{
    public CharacterPreset CharacterPreset { get; set; } = CharacterPresetFactory.CreateDefault();

    public string PlayerName
    {
        get => CharacterPreset.Name;
        set => CharacterPreset.Name = value;
    }

    public string AgeCategoryId
    {
        get => CharacterPreset.AgeCategoryId;
        set => CharacterPreset.AgeCategoryId = value;
    }

    public string OriginId
    {
        get => CharacterPreset.OriginId;
        set => CharacterPreset.OriginId = value;
    }
}
