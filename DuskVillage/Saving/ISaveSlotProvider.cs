using System.Collections.Generic;

namespace DuskVillage.Saving;

public interface ISaveSlotProvider
{
    IReadOnlyList<SaveSlotSummary> GetSlots();

    SaveGame LoadGame(int slotNumber);

    void SaveGame(int slotNumber, SaveGame saveGame);

    int FindFirstWritableSlotNumber();
}
