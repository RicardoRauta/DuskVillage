namespace DuskVillage.Characters;

public sealed class CharacterPresetValidationError
{
    public CharacterPresetValidationError(string messageKey, params object[] args)
    {
        MessageKey = messageKey;
        Args = args;
    }

    public string MessageKey { get; }

    public object[] Args { get; }
}
