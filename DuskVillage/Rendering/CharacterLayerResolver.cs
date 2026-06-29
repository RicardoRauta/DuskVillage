using System.Collections.Generic;
using System.Linq;
using DuskVillage.CharacterAssets;
using DuskVillage.Characters;

namespace DuskVillage.Rendering;

public static class CharacterLayerResolver
{
    public static IReadOnlyList<ManaSeedCharacterAsset> ResolveDrawableAssets(
        ManaSeedCharacterAssetCatalog catalog,
        CharacterAppearanceData appearance)
    {
        var selectedHeadwear = catalog.FindAsset(appearance.GetLayer(CharacterAppearanceSlotIds.Head));
        var headwearHidesHair = selectedHeadwear?.HidesHair == true;
        var anyHeadwear = selectedHeadwear != null;
        var resolved = new List<ManaSeedCharacterAsset>();

        foreach (var slot in catalog.Slots.OrderBy(slot => slot.LayerOrder))
        {
            var assetId = appearance.GetLayer(slot.SlotId);
            if (assetId == CharacterAppearanceData.NoneAssetId)
            {
                continue;
            }

            var asset = catalog.FindAsset(assetId);
            if (asset == null)
            {
                continue;
            }

            if (slot.SlotId == CharacterAppearanceSlotIds.Hair &&
                (headwearHidesHair || (asset.HidesWhenHeadwearEquipped && anyHeadwear)))
            {
                continue;
            }

            resolved.Add(asset);
        }

        return resolved;
    }
}
