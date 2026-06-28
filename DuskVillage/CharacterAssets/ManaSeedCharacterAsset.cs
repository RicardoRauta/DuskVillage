namespace DuskVillage.CharacterAssets;

public sealed class ManaSeedCharacterAsset
{
    public ManaSeedCharacterAsset(
        string slotId,
        string assetId,
        string displayName,
        string zipEntryPath,
        bool hidesHair,
        bool hidesWhenHeadwearEquipped)
    {
        SlotId = slotId;
        AssetId = assetId;
        DisplayName = displayName;
        ZipEntryPath = zipEntryPath;
        HidesHair = hidesHair;
        HidesWhenHeadwearEquipped = hidesWhenHeadwearEquipped;
    }

    public string SlotId { get; }

    public string AssetId { get; }

    public string DisplayName { get; }

    public string ZipEntryPath { get; }

    public bool HidesHair { get; }

    public bool HidesWhenHeadwearEquipped { get; }
}
