using System.Collections.Generic;

namespace DuskVillage.Characters;

public static class CharacterAttributeIds
{
    public const string Strength = "strength";
    public const string Agility = "agility";
    public const string Constitution = "constitution";
    public const string Intelligence = "intelligence";
    public const string Charisma = "charisma";
    public const string Wisdom = "wisdom";

    public static IReadOnlyList<string> All { get; } =
    [
        Strength,
        Agility,
        Constitution,
        Intelligence,
        Charisma,
        Wisdom
    ];
}
