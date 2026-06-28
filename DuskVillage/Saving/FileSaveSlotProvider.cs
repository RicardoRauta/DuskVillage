using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DuskVillage.Saving;

public sealed class FileSaveSlotProvider : ISaveSlotProvider
{
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

    public SaveGame LoadGame(int slotNumber)
    {
        var filePath = SlotFilePath(slotNumber);
        var saveGame = SaveGameSerializer.Deserialize(File.ReadAllText(filePath));
        if (saveGame.Metadata.SaveVersion > SaveMetadata.CurrentSaveVersion)
        {
            throw new InvalidDataException("Save file was created by a newer save version.");
        }

        return saveGame;
    }

    public void SaveGame(int slotNumber, SaveGame saveGame)
    {
        Directory.CreateDirectory(_saveDirectory);
        saveGame.Touch();
        File.WriteAllText(SlotFilePath(slotNumber), SaveGameSerializer.Serialize(saveGame));
    }

    public int FindFirstWritableSlotNumber()
    {
        var slots = GetSlots();
        foreach (var slot in slots)
        {
            if (slot.Status == SaveSlotStatus.Empty)
            {
                return slot.SlotNumber;
            }
        }

        return 1;
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
            var status = saveVersion > SaveMetadata.CurrentSaveVersion ? SaveSlotStatus.Incompatible : SaveSlotStatus.Valid;

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

    private string SlotFilePath(int slotNumber)
    {
        var clamped = Math.Clamp(slotNumber, 1, SlotCount);
        return Path.Combine(_saveDirectory, $"slot_{clamped}.json");
    }
}
