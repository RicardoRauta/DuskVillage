using System.Collections.Generic;
using System.Linq;

namespace DuskVillage.Characters;

public static class CharacterOptionCatalog
{
    public const string YoungAdult = "young_adult";
    public const string Adult = "adult";
    public const string OlderAdult = "older_adult";

    public const string Newcomer = "newcomer";
    public const string LocalVillager = "local_villager";
    public const string FormerLaborer = "former_laborer";
    public const string PoorWanderer = "poor_wanderer";

    public const string Spring = "spring";
    public const string Summer = "summer";
    public const string Autumn = "autumn";
    public const string Winter = "winter";

    public const string MotivationFreshStart = "fresh_start";
    public const string MotivationFamilyLegacy = "family_legacy";
    public const string MotivationHonestWork = "honest_work";
    public const string MotivationVillageGuardian = "village_guardian";
    public const string MotivationLostKnowledge = "lost_knowledge";
    public const string MotivationWealthSecurity = "wealth_security";
    public const string MotivationBelonging = "belonging";
    public const string MotivationDebtEscape = "debt_escape";

    public static IReadOnlyList<CharacterOption> AgeCategories { get; } =
    [
        new(YoungAdult, "age.young_adult"),
        new(Adult, "age.adult"),
        new(OlderAdult, "age.older_adult")
    ];

    public static IReadOnlyList<CharacterOption> Origins { get; } =
    [
        new(Newcomer, "origin.newcomer"),
        new(LocalVillager, "origin.local_villager"),
        new(FormerLaborer, "origin.former_laborer"),
        new(PoorWanderer, "origin.poor_wanderer")
    ];

    public static IReadOnlyList<CharacterOption> BirthdaySeasons { get; } =
    [
        new(Spring, "season.spring"),
        new(Summer, "season.summer"),
        new(Autumn, "season.autumn"),
        new(Winter, "season.winter")
    ];

    public static IReadOnlyList<CharacterMotivationOption> Motivations { get; } =
    [
        new(MotivationFreshStart, "motivation.fresh_start", "motivation.fresh_start.description", "trait_adaptable"),
        new(MotivationFamilyLegacy, "motivation.family_legacy", "motivation.family_legacy.description", "trait_family_minded"),
        new(MotivationHonestWork, "motivation.honest_work", "motivation.honest_work.description", "trait_diligent"),
        new(MotivationVillageGuardian, "motivation.village_guardian", "motivation.village_guardian.description", "trait_protective"),
        new(MotivationLostKnowledge, "motivation.lost_knowledge", "motivation.lost_knowledge.description", "trait_curious"),
        new(MotivationWealthSecurity, "motivation.wealth_security", "motivation.wealth_security.description", "trait_pragmatic"),
        new(MotivationBelonging, "motivation.belonging", "motivation.belonging.description", "trait_warmhearted"),
        new(MotivationDebtEscape, "motivation.debt_escape", "motivation.debt_escape.description", "trait_desperate")
    ];

    public static IReadOnlyList<CharacterOption> Skills { get; } =
    [
        new("skill_farming", "skill.farming"),
        new("skill_woodcutting", "skill.woodcutting"),
        new("skill_crafting", "skill.crafting"),
        new("skill_social", "skill.social"),
        new("skill_survival", "skill.survival"),
        new("skill_combat", "skill.combat")
    ];

    public static bool IsKnownAgeCategory(string value)
    {
        return AgeCategories.Any(option => option.Value == value);
    }

    public static bool IsKnownOrigin(string value)
    {
        return Origins.Any(option => option.Value == value);
    }

    public static bool IsKnownBirthdaySeason(string value)
    {
        return BirthdaySeasons.Any(option => option.Value == value);
    }

    public static bool IsKnownMotivation(string value)
    {
        return Motivations.Any(option => option.Value == value);
    }

    public static CharacterMotivationOption FindMotivation(string value)
    {
        return Motivations.FirstOrDefault(option => option.Value == value) ?? Motivations[0];
    }

    public static bool IsKnownSkill(string value)
    {
        return Skills.Any(option => option.Value == value);
    }
}
