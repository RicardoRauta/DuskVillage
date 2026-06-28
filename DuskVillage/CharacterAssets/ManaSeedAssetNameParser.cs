using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace DuskVillage.CharacterAssets;

public static class ManaSeedAssetNameParser
{
    public static bool TryParse(string entryPath, out ManaSeedAssetNameParts parts)
    {
        var fileName = Path.GetFileNameWithoutExtension(entryPath);
        var tokens = fileName.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length < 4 || tokens[0] != "fbas")
        {
            parts = new ManaSeedAssetNameParts();
            return false;
        }

        var itemId = tokens[2];
        parts = new ManaSeedAssetNameParts
        {
            AssetId = fileName,
            SlotId = tokens[1],
            ItemId = itemId,
            VersionId = tokens[3],
            SpecialId = tokens.Length >= 5 ? tokens[4] : string.Empty,
            DisplayName = ToDisplayName(itemId, tokens[3])
        };

        return true;
    }

    private static string ToDisplayName(string itemId, string versionId)
    {
        var builder = new StringBuilder();
        var previousWasLower = false;
        foreach (var character in itemId)
        {
            if (char.IsUpper(character) && previousWasLower)
            {
                builder.Append(' ');
            }

            if (char.IsDigit(character) && builder.Length > 0 && !char.IsDigit(builder[^1]))
            {
                builder.Append(' ');
            }

            builder.Append(character);
            previousWasLower = char.IsLower(character);
        }

        var display = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(builder.ToString().Replace("boobs", "boobs", StringComparison.OrdinalIgnoreCase));
        if (versionId == "00" || versionId.StartsWith("00", StringComparison.OrdinalIgnoreCase))
        {
            return display;
        }

        return $"{display} {versionId}";
    }
}
