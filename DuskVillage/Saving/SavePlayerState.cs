using DuskVillage.Characters;

namespace DuskVillage.Saving;

public sealed class SavePlayerState
{
    public string EntityId { get; set; } = "player_main";

    public CharacterPreset CharacterPreset { get; set; } = CharacterPresetFactory.CreateDefault();
}
