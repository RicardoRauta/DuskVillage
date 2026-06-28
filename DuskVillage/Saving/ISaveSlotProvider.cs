using System.Collections.Generic;

namespace DuskVillage.Saving;

public interface ISaveSlotProvider
{
    IReadOnlyList<SaveSlotSummary> GetSlots();
}
