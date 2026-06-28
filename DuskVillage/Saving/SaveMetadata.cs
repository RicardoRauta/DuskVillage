using System;
using DuskVillage.Core;

namespace DuskVillage.Saving;

public sealed class SaveMetadata
{
    public const int CurrentSaveVersion = 1;

    public int SaveVersion { get; set; } = CurrentSaveVersion;

    public string GameVersion { get; set; } = GameDirectories.GameVersion;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset LastPlayedAt { get; set; } = DateTimeOffset.UtcNow;

    public string PlayerName { get; set; } = "Alden";

    public int CurrentDay { get; set; } = 1;

    public string CurrentTime { get; set; } = "06:00";
}
