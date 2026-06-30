namespace DuskVillage.Inventory;

public sealed class InventorySlot
{
    public string ItemId { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public bool IsEmpty => string.IsNullOrWhiteSpace(ItemId) || Quantity <= 0;

    public InventorySlot Clone()
    {
        return new InventorySlot
        {
            ItemId = ItemId,
            Quantity = Quantity
        };
    }
}
