namespace DuskVillage.Settings;

public interface IGameSettingsService
{
    GameSettings Current { get; }

    void Load();

    void Save(GameSettings settings);
}
