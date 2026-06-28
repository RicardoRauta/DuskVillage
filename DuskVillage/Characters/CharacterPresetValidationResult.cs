using System.Collections.Generic;

namespace DuskVillage.Characters;

public sealed class CharacterPresetValidationResult
{
    private readonly List<CharacterPresetValidationError> _errors = new();

    public IReadOnlyList<CharacterPresetValidationError> Errors => _errors;

    public bool IsValid => _errors.Count == 0;

    public void Add(string messageKey, params object[] args)
    {
        _errors.Add(new CharacterPresetValidationError(messageKey, args));
    }
}
