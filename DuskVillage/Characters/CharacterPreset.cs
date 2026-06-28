using System.Collections.Generic;
using System.Linq;

namespace DuskVillage.Characters;

public sealed class CharacterPreset
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    public string PresetId { get; set; } = string.Empty;

    public string Name { get; set; } = "Alden";

    public string FamilyName { get; set; } = "Vale";

    public string AgeCategoryId { get; set; } = CharacterOptionCatalog.YoungAdult;

    public string OriginId { get; set; } = CharacterOptionCatalog.Newcomer;

    public string BirthdaySeasonId { get; set; } = CharacterOptionCatalog.Spring;

    public int BirthdayDay { get; set; } = 1;

    public string MotivationId { get; set; } = CharacterOptionCatalog.MotivationFreshStart;

    public CharacterAppearanceData Appearance { get; set; } = CharacterAppearanceData.CreateDefault();

    public CharacterAttributeBlock Attributes { get; set; } = new();

    public CharacterNeedsBlock Needs { get; set; } = new();

    public List<CharacterSkillState> Skills { get; set; } = new();

    public CharacterPreset Clone()
    {
        return new CharacterPreset
        {
            SchemaVersion = SchemaVersion,
            PresetId = PresetId,
            Name = Name,
            FamilyName = FamilyName,
            AgeCategoryId = AgeCategoryId,
            OriginId = OriginId,
            BirthdaySeasonId = BirthdaySeasonId,
            BirthdayDay = BirthdayDay,
            MotivationId = MotivationId,
            Appearance = Appearance.Clone(),
            Attributes = Attributes.Clone(),
            Needs = Needs.Clone(),
            Skills = Skills.Select(skill => skill.Clone()).ToList()
        };
    }
}
