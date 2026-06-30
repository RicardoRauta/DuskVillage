namespace DuskVillage.Inventory;

public sealed class InventoryOperationResult
{
    public InventoryOperationResult(bool succeeded, InventoryState inventory, string messageKey, int quantityChanged = 0)
    {
        Succeeded = succeeded;
        Inventory = inventory;
        MessageKey = messageKey;
        QuantityChanged = quantityChanged;
    }

    public bool Succeeded { get; }

    public InventoryState Inventory { get; }

    public string MessageKey { get; }

    public int QuantityChanged { get; }
}
