using System;
using System.Linq;

namespace DuskVillage.Characters;

public static class CharacterPresetFactory
{
    public static CharacterPreset CreateDefault()
    {
        var preset = new CharacterPreset
        {
            SchemaVersion = CharacterPreset.CurrentSchemaVersion,
            PresetId = CreatePresetId(),
            Name = "Alden",
            FamilyName = "Vale",
            AgeCategoryId = CharacterOptionCatalog.YoungAdult,
            OriginId = CharacterOptionCatalog.Newcomer,
            BirthdaySeasonId = CharacterOptionCatalog.Spring,
            BirthdayDay = 1,
            MotivationId = CharacterOptionCatalog.MotivationFreshStart,
            Appearance = CharacterAppearanceData.CreateDefault()
        };

        ApplyStartingProfile(preset);
        EnsureDefaultSkills(preset);
        return preset;
    }

    public static void ApplyStartingProfile(CharacterPreset preset)
    {
        preset.Attributes = preset.AgeCategoryId switch
        {
            CharacterOptionCatalog.Adult => new CharacterAttributeBlock
            {
                Strength = 5,
                Agility = 4,
                Constitution = 5,
                Intelligence = 4,
                Charisma = 4,
                Wisdom = 4
            },
            CharacterOptionCatalog.OlderAdult => new CharacterAttributeBlock
            {
                Strength = 3,
                Agility = 3,
                Constitution = 3,
                Intelligence = 5,
                Charisma = 5,
                Wisdom = 6
            },
            _ => new CharacterAttributeBlock
            {
                Strength = 4,
                Agility = 5,
                Constitution = 6,
                Intelligence = 4,
                Charisma = 3,
                Wisdom = 3
            }
        };

        ApplyOriginModifier(preset);
        preset.Needs = preset.AgeCategoryId switch
        {
            CharacterOptionCatalog.OlderAdult => new CharacterNeedsBlock
            {
                Energy = 85,
                MaxEnergy = 85,
                Hunger = 90,
                MaxHunger = 100,
                Health = 90,
                MaxHealth = 90,
                Mood = 72,
                MaxMood = 100
            },
            CharacterOptionCatalog.Adult => new CharacterNeedsBlock
            {
                Energy = 95,
                MaxEnergy = 95,
                Hunger = 95,
                MaxHunger = 100,
                Health = 100,
                MaxHealth = 100,
                Mood = 75,
                MaxMood = 100
            },
            _ => new CharacterNeedsBlock
            {
                Energy = 100,
                MaxEnergy = 100,
                Hunger = 100,
                MaxHunger = 100,
                Health = 100,
                MaxHealth = 100,
                Mood = 78,
                MaxMood = 100
            }
        };
    }

    public static void EnsureDefaultSkills(CharacterPreset preset)
    {
        foreach (var option in CharacterOptionCatalog.Skills)
        {
            if (preset.Skills.All(skill => skill.SkillId != option.Value))
            {
                preset.Skills.Add(new CharacterSkillState { SkillId = option.Value });
            }
        }
    }

    private static void ApplyOriginModifier(CharacterPreset preset)
    {
        switch (preset.OriginId)
        {
            case CharacterOptionCatalog.LocalVillager:
                preset.Attributes.Charisma += 1;
                preset.Attributes.Wisdom += 1;
                break;
            case CharacterOptionCatalog.FormerLaborer:
                preset.Attributes.Strength += 1;
                preset.Attributes.Constitution += 1;
                break;
            case CharacterOptionCatalog.PoorWanderer:
                preset.Attributes.Agility += 1;
                preset.Attributes.Wisdom += 1;
                break;
        }
    }

    private static string CreatePresetId()
    {
        return $"character_{Guid.NewGuid():N}";
    }
}
