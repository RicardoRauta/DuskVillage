using System;

namespace DuskVillage.Saving;

public sealed class SaveSlotSummary
{
    public int SlotNumber { get; set; }

    public string SlotId { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public SaveSlotStatus Status { get; set; }

    public int SaveVersion { get; set; }

    public string GameVersion { get; set; } = string.Empty;

    public string PlayerName { get; set; } = string.Empty;

    public int CurrentDay { get; set; }

    public string CurrentTime { get; set; } = string.Empty;

    public DateTimeOffset? LastPlayedAt { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public static SaveSlotSummary Empty(int slotNumber, string filePath)
    {
        return new SaveSlotSummary
        {
            SlotNumber = slotNumber,
            SlotId = $"slot_{slotNumber}",
            FilePath = filePath,
            Status = SaveSlotStatus.Empty
        };
    }
}
