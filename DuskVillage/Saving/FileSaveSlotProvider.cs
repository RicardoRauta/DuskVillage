using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DuskVillage.Saving;

public sealed class FileSaveSlotProvider : ISaveSlotProvider
{
    private const int CurrentSaveVersion = 1;
    private const int SlotCount = 5;
    private readonly string _saveDirectory;

    public FileSaveSlotProvider(string saveDirectory)
    {
        _saveDirectory = saveDirectory;
    }

    public IReadOnlyList<SaveSlotSummary> GetSlots()
    {
        Directory.CreateDirectory(_saveDirectory);

        var slots = new List<SaveSlotSummary>();
        for (var slotNumber = 1; slotNumber <= SlotCount; slotNumber++)
        {
            var filePath = Path.Combine(_saveDirectory, $"slot_{slotNumber}.json");
            slots.Add(ReadSlot(slotNumber, filePath));
        }

        return slots;
    }

    private static SaveSlotSummary ReadSlot(int slotNumber, string filePath)
    {
        if (!File.Exists(filePath))
        {
            return SaveSlotSummary.Empty(slotNumber, filePath);
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(filePath));
            var root = document.RootElement;
            var metadata = root.TryGetProperty("metadata", out var metadataElement) ? metadataElement : root;

            var saveVersion = ReadInt(metadata, "saveVersion", 0);
            var status = saveVersion > CurrentSaveVersion ? SaveSlotStatus.Incompatible : SaveSlotStatus.Valid;

            if (saveVersion <= 0)
            {
                status = SaveSlotStatus.Corrupted;
            }

            return new SaveSlotSummary
            {
                SlotNumber = slotNumber,
                SlotId = $"slot_{slotNumber}",
                FilePath = filePath,
                Status = status,
                SaveVersion = saveVersion,
                GameVersion = ReadString(metadata, "gameVersion", "unknown"),
                PlayerName = ReadString(metadata, "playerName", "Unknown"),
                CurrentDay = ReadInt(metadata, "currentDay", 1),
                CurrentTime = ReadString(metadata, "currentTime", "06:00"),
                LastPlayedAt = ReadDate(metadata, "lastPlayedAt"),
                ErrorMessage = status == SaveSlotStatus.Incompatible ? "save.error.incompatible" : string.Empty
            };
        }
        catch (JsonException)
        {
            return Corrupted(slotNumber, filePath);
        }
        catch (IOException)
        {
            return Corrupted(slotNumber, filePath);
        }
        catch (UnauthorizedAccessException)
        {
            return Corrupted(slotNumber, filePath);
        }
    }

    private static SaveSlotSummary Corrupted(int slotNumber, string filePath)
    {
        return new SaveSlotSummary
        {
            SlotNumber = slotNumber,
            SlotId = $"slot_{slotNumber}",
            FilePath = filePath,
            Status = SaveSlotStatus.Corrupted,
            ErrorMessage = "save.error.corrupted"
        };
    }

    private static string ReadString(JsonElement element, string propertyName, string fallback)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? fallback
            : fallback;
    }

    private static int ReadInt(JsonElement element, string propertyName, int fallback)
    {
        return element.TryGetProperty(propertyName, out var property) && property.TryGetInt32(out var value)
            ? value
            : fallback;
    }

    private static DateTimeOffset? ReadDate(JsonElement element, string propertyName)
    {
        var value = ReadString(element, propertyName, string.Empty);
        return DateTimeOffset.TryParse(value, out var parsed) ? parsed : null;
    }
}
