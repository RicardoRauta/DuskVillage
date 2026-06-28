using DuskVillage.Characters;

namespace DuskVillage.Saving;

public interface ICharacterPresetStorage
{
    string DefaultDirectory { get; }

    string CreateDefaultExportPath(CharacterPreset preset);

    CharacterPreset Load(string filePath);

    void Save(CharacterPreset preset, string filePath);
}
