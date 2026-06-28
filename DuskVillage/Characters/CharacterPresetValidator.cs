using System;
using System.Collections.Generic;
using System.Linq;

namespace DuskVillage.Characters;

public static class CharacterPresetValidator
{
    public const int AttributeMinimum = 1;
    public const int AttributeMaximum = 10;
    public const int AttributePointBudget = 30;
    public const int SkillMinimum = 0;
    public const int SkillMaximum = 10;
    public const int BirthdayDayMinimum = 1;
    public const int BirthdayDayMaximum = 28;

    public static CharacterPresetValidationResult Validate(CharacterPreset preset)
    {
        var result = new CharacterPresetValidationResult();

        if (preset.SchemaVersion <= 0 || preset.SchemaVersion > CharacterPreset.CurrentSchemaVersion)
        {
            result.Add("character.validation.schema", preset.SchemaVersion);
        }

        if (string.IsNullOrWhiteSpace(preset.Name))
        {
            result.Add("character.validation.name_required");
        }

        if (!CharacterOptionCatalog.IsKnownAgeCategory(preset.AgeCategoryId))
        {
            result.Add("character.validation.age", preset.AgeCategoryId);
        }

        if (!CharacterOptionCatalog.IsKnownOrigin(preset.OriginId))
        {
            result.Add("character.validation.origin", preset.OriginId);
        }

        if (!CharacterOptionCatalog.IsKnownBirthdaySeason(preset.BirthdaySeasonId))
        {
            result.Add("character.validation.birthday_season", preset.BirthdaySeasonId);
        }

        if (preset.BirthdayDay < BirthdayDayMinimum || preset.BirthdayDay > BirthdayDayMaximum)
        {
            result.Add("character.validation.birthday_day", BirthdayDayMinimum, BirthdayDayMaximum);
        }

        if (!CharacterOptionCatalog.IsKnownMotivation(preset.MotivationId))
        {
            result.Add("character.validation.motivation", preset.MotivationId);
        }

        ValidateAttributes(preset.Attributes, result);
        ValidateNeeds(preset.Needs, result);
        ValidateSkills(preset.Skills, result);
        ValidateAppearance(preset.Appearance, result);

        return result;
    }

    private static void ValidateAttributes(CharacterAttributeBlock attributes, CharacterPresetValidationResult result)
    {
        foreach (var attributeId in CharacterAttributeIds.All)
        {
            var value = attributes.GetValue(attributeId);
            if (value < AttributeMinimum || value > AttributeMaximum)
            {
                result.Add("character.validation.attribute_range", AttributeLabelKey(attributeId), AttributeMinimum, AttributeMaximum);
            }
        }

        if (attributes.Total > AttributePointBudget)
        {
            result.Add("character.validation.attribute_budget", AttributePointBudget);
        }
    }

    private static void ValidateNeeds(CharacterNeedsBlock needs, CharacterPresetValidationResult result)
    {
        ValidateNeed("character.need.energy", needs.Energy, needs.MaxEnergy, result);
        ValidateNeed("character.need.hunger", needs.Hunger, needs.MaxHunger, result);
        ValidateNeed("character.need.health", needs.Health, needs.MaxHealth, result);
        ValidateNeed("character.need.mood", needs.Mood, needs.MaxMood, result);
    }

    private static void ValidateNeed(string labelKey, int value, int max, CharacterPresetValidationResult result)
    {
        if (max <= 0 || value < 0 || value > max)
        {
            result.Add("character.validation.need_range", labelKey, max);
        }
    }

    private static void ValidateSkills(IReadOnlyList<CharacterSkillState> skills, CharacterPresetValidationResult result)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var skill in skills)
        {
            if (!CharacterOptionCatalog.IsKnownSkill(skill.SkillId))
            {
                result.Add("character.validation.skill", skill.SkillId);
            }

            if (!seen.Add(skill.SkillId))
            {
                result.Add("character.validation.skill_duplicate", skill.SkillId);
            }

            if (skill.Level < SkillMinimum || skill.Level > SkillMaximum)
            {
                result.Add("character.validation.skill_range", skill.SkillId, SkillMinimum, SkillMaximum);
            }
        }
    }

    private static void ValidateAppearance(CharacterAppearanceData appearance, CharacterPresetValidationResult result)
    {
        foreach (var slotId in appearance.Layers.Keys)
        {
            if (!CharacterAppearanceSlotIds.IsKnown(slotId))
            {
                result.Add("character.validation.appearance_slot", slotId);
            }
        }

        foreach (var slotId in CharacterAppearanceSlotIds.RequiredForPlayableCharacter)
        {
            if (appearance.GetLayer(slotId) == CharacterAppearanceData.NoneAssetId)
            {
                result.Add("character.validation.appearance_required", SlotLabelKey(slotId));
            }
        }
    }

    public static string AttributeLabelKey(string attributeId)
    {
        return attributeId switch
        {
            CharacterAttributeIds.Strength => "character.attribute.strength",
            CharacterAttributeIds.Agility => "character.attribute.agility",
            CharacterAttributeIds.Constitution => "character.attribute.constitution",
            CharacterAttributeIds.Intelligence => "character.attribute.intelligence",
            CharacterAttributeIds.Charisma => "character.attribute.charisma",
            CharacterAttributeIds.Wisdom => "character.attribute.wisdom",
            _ => attributeId
        };
    }

    public static string SlotLabelKey(string slotId)
    {
        return slotId switch
        {
            CharacterAppearanceSlotIds.Under => "character.slot.00undr",
            CharacterAppearanceSlotIds.Body => "character.slot.01body",
            CharacterAppearanceSlotIds.Socks => "character.slot.02sock",
            CharacterAppearanceSlotIds.FootwearLow => "character.slot.03fot1",
            CharacterAppearanceSlotIds.LowerOne => "character.slot.04lwr1",
            CharacterAppearanceSlotIds.Shirt => "character.slot.05shrt",
            CharacterAppearanceSlotIds.LowerTwo => "character.slot.06lwr2",
            CharacterAppearanceSlotIds.FootwearHigh => "character.slot.07fot2",
            CharacterAppearanceSlotIds.LowerThree => "character.slot.08lwr3",
            CharacterAppearanceSlotIds.Hands => "character.slot.09hand",
            CharacterAppearanceSlotIds.Outer => "character.slot.10outr",
            CharacterAppearanceSlotIds.Neck => "character.slot.11neck",
            CharacterAppearanceSlotIds.Face => "character.slot.12face",
            CharacterAppearanceSlotIds.Hair => "character.slot.13hair",
            CharacterAppearanceSlotIds.Head => "character.slot.14head",
            _ => slotId
        };
    }
}
