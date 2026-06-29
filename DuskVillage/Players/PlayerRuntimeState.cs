using DuskVillage.Characters;

namespace DuskVillage.Players;

public class PlayerRuntimeState
{
    public string EntityId { get; set; } = PlayerRuntimeFactory.DefaultPlayerEntityId;

    public CharacterPreset CharacterPreset { get; set; } = CharacterPresetFactory.CreateDefault();

    public CharacterNeedsBlock Needs { get; set; }

    public int Money { get; set; }

    public PlayerLocationState Location { get; set; }
}
