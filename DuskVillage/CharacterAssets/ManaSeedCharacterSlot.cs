using System.Collections.Generic;
using DuskVillage.Characters;

namespace DuskVillage.CharacterAssets;

public sealed class ManaSeedCharacterSlot
{
    public ManaSeedCharacterSlot(string slotId, string labelKey, int layerOrder, bool isRequired)
    {
        SlotId = slotId;
        LabelKey = labelKey;
        LayerOrder = layerOrder;
        IsRequired = isRequired;
    }

    public string SlotId { get; }

    public string LabelKey { get; }

    public int LayerOrder { get; }

    public bool IsRequired { get; }

    public static IReadOnlyList<ManaSeedCharacterSlot> CreateDefaultSlots()
    {
        return
        [
            new(CharacterAppearanceSlotIds.Under, "character.slot.00undr", 0, false),
            new(CharacterAppearanceSlotIds.Body, "character.slot.01body", 1, true),
            new(CharacterAppearanceSlotIds.Socks, "character.slot.02sock", 2, false),
            new(CharacterAppearanceSlotIds.FootwearLow, "character.slot.03fot1", 3, true),
            new(CharacterAppearanceSlotIds.LowerOne, "character.slot.04lwr1", 4, true),
            new(CharacterAppearanceSlotIds.Shirt, "character.slot.05shrt", 5, true),
            new(CharacterAppearanceSlotIds.LowerTwo, "character.slot.06lwr2", 6, false),
            new(CharacterAppearanceSlotIds.FootwearHigh, "character.slot.07fot2", 7, false),
            new(CharacterAppearanceSlotIds.LowerThree, "character.slot.08lwr3", 8, false),
            new(CharacterAppearanceSlotIds.Hands, "character.slot.09hand", 9, false),
            new(CharacterAppearanceSlotIds.Outer, "character.slot.10outr", 10, false),
            new(CharacterAppearanceSlotIds.Neck, "character.slot.11neck", 11, false),
            new(CharacterAppearanceSlotIds.Face, "character.slot.12face", 12, false),
            new(CharacterAppearanceSlotIds.Hair, "character.slot.13hair", 13, false),
            new(CharacterAppearanceSlotIds.Head, "character.slot.14head", 14, false)
        ];
    }
}
