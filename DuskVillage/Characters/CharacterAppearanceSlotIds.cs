using System.Collections.Generic;

namespace DuskVillage.Characters;

public static class CharacterAppearanceSlotIds
{
    public const string Under = "00undr";
    public const string Body = "01body";
    public const string Socks = "02sock";
    public const string FootwearLow = "03fot1";
    public const string LowerOne = "04lwr1";
    public const string Shirt = "05shrt";
    public const string LowerTwo = "06lwr2";
    public const string FootwearHigh = "07fot2";
    public const string LowerThree = "08lwr3";
    public const string Hands = "09hand";
    public const string Outer = "10outr";
    public const string Neck = "11neck";
    public const string Face = "12face";
    public const string Hair = "13hair";
    public const string Head = "14head";

    public static IReadOnlyList<string> All { get; } =
    [
        Under,
        Body,
        Socks,
        FootwearLow,
        LowerOne,
        Shirt,
        LowerTwo,
        FootwearHigh,
        LowerThree,
        Hands,
        Outer,
        Neck,
        Face,
        Hair,
        Head
    ];

    public static IReadOnlyList<string> RequiredForPlayableCharacter { get; } =
    [
        Body,
        FootwearLow,
        LowerOne,
        Shirt
    ];

    public static bool IsKnown(string slotId)
    {
        foreach (var knownSlotId in All)
        {
            if (knownSlotId == slotId)
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsRequiredForPlayableCharacter(string slotId)
    {
        foreach (var requiredSlotId in RequiredForPlayableCharacter)
        {
            if (requiredSlotId == slotId)
            {
                return true;
            }
        }

        return false;
    }
}
