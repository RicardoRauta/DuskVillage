using System.Collections.Generic;

namespace DuskVillage.Inventory;

public sealed class InventoryState
{
    public const int DefaultCapacity = 24;
    public const int DefaultHotbarSize = 8;

    public int Capacity { get; set; } = DefaultCapacity;

    public int HotbarSize { get; set; } = DefaultHotbarSize;

    public int SelectedHotbarIndex { get; set; }

    public List<InventorySlot> Slots { get; set; } = new();

    public InventoryState Clone()
    {
        return new InventoryState
        {
            Capacity = Capacity,
            HotbarSize = HotbarSize,
            SelectedHotbarIndex = SelectedHotbarIndex,
            Slots = (Slots ?? new List<InventorySlot>()).ConvertAll(slot => slot?.Clone() ?? new InventorySlot())
        };
    }
}
