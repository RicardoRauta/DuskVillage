using System;
using DuskVillage.Characters;

namespace DuskVillage.Players;

public static class PlayerRuntimeFactory
{
    public const string DefaultPlayerEntityId = "player_main";
    public const string DefaultAreaId = "dusk_village";

    public static PlayerRuntimeState CreateNew(CharacterPreset preset)
    {
        var characterPreset = ClonePreset(preset);
        return Normalize(new PlayerRuntimeState
        {
            EntityId = DefaultPlayerEntityId,
            CharacterPreset = characterPreset,
            Needs = characterPreset.Needs.Clone(),
            Money = 0,
            Location = CreateDefaultLocation()
        });
    }

    public static PlayerRuntimeState Clone(PlayerRuntimeState state)
    {
        var normalized = Normalize(state);
        return new PlayerRuntimeState
        {
            EntityId = normalized.EntityId,
            CharacterPreset = ClonePreset(normalized.CharacterPreset),
            Needs = normalized.Needs.Clone(),
            Money = normalized.Money,
            Location = normalized.Location.Clone()
        };
    }

    public static PlayerRuntimeState Normalize(PlayerRuntimeState state)
    {
        state ??= CreateNew(CharacterPresetFactory.CreateDefault());
        state.EntityId = string.IsNullOrWhiteSpace(state.EntityId) ? DefaultPlayerEntityId : state.EntityId.Trim();
        state.CharacterPreset = ClonePreset(state.CharacterPreset);
        state.Needs = NormalizeNeeds(state.Needs ?? state.CharacterPreset.Needs.Clone());
        state.Money = Math.Max(0, state.Money);
        state.Location = NormalizeLocation(state.Location);
        return state;
    }

    private static CharacterPreset ClonePreset(CharacterPreset preset)
    {
        preset ??= CharacterPresetFactory.CreateDefault();
        preset.Appearance ??= CharacterAppearanceData.CreateDefault();
        preset.Attributes ??= new CharacterAttributeBlock();
        preset.Needs ??= new CharacterNeedsBlock();
        preset.Skills ??= [];
        CharacterPresetFactory.EnsureDefaultSkills(preset);
        return preset.Clone();
    }

    private static CharacterNeedsBlock NormalizeNeeds(CharacterNeedsBlock needs)
    {
        needs.MaxEnergy = Math.Max(1, needs.MaxEnergy);
        needs.MaxHunger = Math.Max(1, needs.MaxHunger);
        needs.MaxHealth = Math.Max(1, needs.MaxHealth);
        needs.MaxMood = Math.Max(1, needs.MaxMood);
        needs.Energy = Math.Clamp(needs.Energy, 0, needs.MaxEnergy);
        needs.Hunger = Math.Clamp(needs.Hunger, 0, needs.MaxHunger);
        needs.Health = Math.Clamp(needs.Health, 0, needs.MaxHealth);
        needs.Mood = Math.Clamp(needs.Mood, 0, needs.MaxMood);
        return needs;
    }

    private static PlayerLocationState NormalizeLocation(PlayerLocationState location)
    {
        location ??= CreateDefaultLocation();
        location.AreaId = string.IsNullOrWhiteSpace(location.AreaId) ? DefaultAreaId : location.AreaId.Trim();
        location.TileX = Math.Max(0, location.TileX);
        location.TileY = Math.Max(0, location.TileY);
        return location;
    }

    private static PlayerLocationState CreateDefaultLocation()
    {
        return new PlayerLocationState
        {
            AreaId = DefaultAreaId,
            TileX = 0,
            TileY = 0
        };
    }
}
