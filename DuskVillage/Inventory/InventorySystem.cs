using System;
using System.Linq;
using DuskVillage.Items;

namespace DuskVillage.Inventory;

public static class InventorySystem
{
    public static InventoryState CreateDefault()
    {
        return Normalize(new InventoryState(), ItemDefinitionRegistry.Empty);
    }

    public static InventoryState CreateStarterInventory(ItemDefinitionRegistry items = null)
    {
        var inventory = CreateDefault();
        inventory = AddItem(inventory, ItemIds.StarterSeeds, 15, items).Inventory;
        inventory = AddItem(inventory, ItemIds.WateringCanBasic, 1, items).Inventory;
        inventory.SelectedHotbarIndex = 0;
        return Normalize(inventory, items);
    }

    public static InventoryState Normalize(InventoryState inventory, ItemDefinitionRegistry items = null)
    {
        items ??= ItemDefinitionRegistry.Empty;
        inventory ??= new InventoryState();
        inventory.Capacity = Math.Clamp(inventory.Capacity <= 0 ? InventoryState.DefaultCapacity : inventory.Capacity, 1, 256);
        inventory.HotbarSize = Math.Clamp(inventory.HotbarSize <= 0 ? InventoryState.DefaultHotbarSize : inventory.HotbarSize, 1, inventory.Capacity);
        inventory.SelectedHotbarIndex = Math.Clamp(inventory.SelectedHotbarIndex, 0, inventory.HotbarSize - 1);
        inventory.Slots ??= new();

        while (inventory.Slots.Count < inventory.Capacity)
        {
            inventory.Slots.Add(new InventorySlot());
        }

        if (inventory.Slots.Count > inventory.Capacity)
        {
            inventory.Slots.RemoveRange(inventory.Capacity, inventory.Slots.Count - inventory.Capacity);
        }

        for (var i = 0; i < inventory.Slots.Count; i++)
        {
            var slot = inventory.Slots[i] ?? new InventorySlot();
            var itemId = slot.ItemId?.Trim() ?? string.Empty;
            var quantity = Math.Max(0, slot.Quantity);
            if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
            {
                inventory.Slots[i] = new InventorySlot();
                continue;
            }

            slot.ItemId = itemId;
            slot.Quantity = Math.Min(quantity, items.MaxStackFor(itemId));
            inventory.Slots[i] = slot;
        }

        return inventory;
    }

    public static InventoryOperationResult AddItem(InventoryState inventory, string itemId, int quantity, ItemDefinitionRegistry items = null)
    {
        items ??= ItemDefinitionRegistry.Empty;
        var next = Normalize(inventory?.Clone(), items);
        itemId = itemId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
        {
            return new InventoryOperationResult(false, next, "inventory.invalid_item");
        }

        var remaining = quantity;
        var maxStack = items.MaxStackFor(itemId);
        foreach (var slot in next.Slots.Where(slot => !slot.IsEmpty && slot.ItemId.Equals(itemId, StringComparison.OrdinalIgnoreCase)))
        {
            var room = Math.Max(0, maxStack - slot.Quantity);
            var moved = Math.Min(room, remaining);
            slot.Quantity += moved;
            remaining -= moved;
            if (remaining <= 0)
            {
                return new InventoryOperationResult(true, next, "inventory.added", quantity);
            }
        }

        foreach (var slot in next.Slots.Where(slot => slot.IsEmpty))
        {
            var moved = Math.Min(maxStack, remaining);
            slot.ItemId = itemId;
            slot.Quantity = moved;
            remaining -= moved;
            if (remaining <= 0)
            {
                return new InventoryOperationResult(true, next, "inventory.added", quantity);
            }
        }

        return new InventoryOperationResult(false, next, "inventory.full", quantity - remaining);
    }

    public static InventoryOperationResult RemoveItem(InventoryState inventory, string itemId, int quantity, ItemDefinitionRegistry items = null)
    {
        var next = Normalize(inventory?.Clone(), items);
        itemId = itemId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
        {
            return new InventoryOperationResult(false, next, "inventory.invalid_item");
        }

        if (!HasItem(next, itemId, quantity, items))
        {
            return new InventoryOperationResult(false, next, "inventory.missing_item");
        }

        var remaining = quantity;
        for (var i = next.Slots.Count - 1; i >= 0 && remaining > 0; i--)
        {
            var slot = next.Slots[i];
            if (slot.IsEmpty || !slot.ItemId.Equals(itemId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var removed = Math.Min(slot.Quantity, remaining);
            slot.Quantity -= removed;
            remaining -= removed;
            if (slot.Quantity <= 0)
            {
                next.Slots[i] = new InventorySlot();
            }
        }

        return new InventoryOperationResult(true, next, "inventory.removed", quantity);
    }

    public static bool HasItem(InventoryState inventory, string itemId, int quantity, ItemDefinitionRegistry items = null)
    {
        itemId = itemId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
        {
            return false;
        }

        var normalized = Normalize(inventory?.Clone(), items);
        var total = normalized.Slots
            .Where(slot => !slot.IsEmpty && slot.ItemId.Equals(itemId, StringComparison.OrdinalIgnoreCase))
            .Sum(slot => slot.Quantity);
        return total >= quantity;
    }

    public static InventorySlot GetSelectedHotbarSlot(InventoryState inventory, ItemDefinitionRegistry items = null)
    {
        var normalized = Normalize(inventory, items);
        return normalized.SelectedHotbarIndex >= 0 && normalized.SelectedHotbarIndex < normalized.Slots.Count
            ? normalized.Slots[normalized.SelectedHotbarIndex]
            : new InventorySlot();
    }

    public static InventoryState SelectHotbarSlot(InventoryState inventory, int selectedHotbarIndex, ItemDefinitionRegistry items = null)
    {
        var next = Normalize(inventory?.Clone(), items);
        next.SelectedHotbarIndex = Math.Clamp(selectedHotbarIndex, 0, next.HotbarSize - 1);
        return next;
    }
}
