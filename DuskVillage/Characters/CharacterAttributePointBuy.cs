using System;
using System.Linq;

namespace DuskVillage.Characters;

public static class CharacterAttributePointBuy
{
    public static bool CanIncrease(CharacterAttributeBlock attributes, string attributeId)
    {
        return attributes.GetValue(attributeId) < CharacterPresetValidator.AttributeMaximum &&
            attributes.Total < CharacterPresetValidator.AttributePointBudget;
    }

    public static bool CanDecrease(CharacterAttributeBlock attributes, string attributeId)
    {
        return attributes.GetValue(attributeId) > CharacterPresetValidator.AttributeMinimum;
    }

    public static CharacterAttributeBlock Randomize(Random random)
    {
        ArgumentNullException.ThrowIfNull(random);

        var attributes = new CharacterAttributeBlock();
        foreach (var attributeId in CharacterAttributeIds.All)
        {
            attributes.SetValue(attributeId, CharacterPresetValidator.AttributeMinimum);
        }

        var remainingPoints = CharacterPresetValidator.AttributePointBudget -
            CharacterAttributeIds.All.Count * CharacterPresetValidator.AttributeMinimum;

        while (remainingPoints > 0)
        {
            var candidates = CharacterAttributeIds.All
                .Where(attributeId => attributes.GetValue(attributeId) < CharacterPresetValidator.AttributeMaximum)
                .ToList();

            if (candidates.Count == 0)
            {
                break;
            }

            var selected = candidates[random.Next(candidates.Count)];
            attributes.SetValue(selected, attributes.GetValue(selected) + 1);
            remainingPoints--;
        }

        return attributes;
    }
}
