using DuskVillage.Characters;
using DuskVillage.Players;

namespace DuskVillage.Saving;

public sealed class SavePlayerState : PlayerRuntimeState
{
    public static SavePlayerState CreateNew(CharacterPreset preset)
    {
        return FromRuntimeState(PlayerRuntimeFactory.CreateNew(preset));
    }

    public static SavePlayerState FromRuntimeState(PlayerRuntimeState state)
    {
        var normalized = PlayerRuntimeFactory.Clone(state);
        return new SavePlayerState
        {
            EntityId = normalized.EntityId,
            CharacterPreset = normalized.CharacterPreset,
            Needs = normalized.Needs,
            Money = normalized.Money,
            Location = normalized.Location,
            Inventory = normalized.Inventory
        };
    }

    public void Apply(PlayerRuntimeState state)
    {
        var normalized = PlayerRuntimeFactory.Clone(state);
        EntityId = normalized.EntityId;
        CharacterPreset = normalized.CharacterPreset;
        Needs = normalized.Needs;
        Money = normalized.Money;
        Location = normalized.Location;
        Inventory = normalized.Inventory;
    }
}
