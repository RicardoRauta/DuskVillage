namespace DuskVillage.Characters;

public sealed class CharacterSkillState
{
    public string SkillId { get; set; } = string.Empty;

    public int Level { get; set; }

    public int Experience { get; set; }

    public CharacterSkillState Clone()
    {
        return new CharacterSkillState
        {
            SkillId = SkillId,
            Level = Level,
            Experience = Experience
        };
    }
}
