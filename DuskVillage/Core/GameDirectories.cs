using System;
using System.IO;

namespace DuskVillage.Core;

public static class GameDirectories
{
    private const string AppFolderName = "DuskVillage";

    public static string UserDataRoot { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppFolderName);

    public static string SettingsFilePath => Path.Combine(UserDataRoot, "settings.json");

    public static string SavesDirectory => Path.Combine(UserDataRoot, "Saves");

    public static string LocalizationDirectory => Path.Combine(
        AppContext.BaseDirectory,
        "Data",
        "Localization");
}
