namespace DuskVillage.Screens;

public interface IScreenNavigator
{
    bool CanGoBack { get; }

    void SetRoot(IGameScreen screen);

    void Push(IGameScreen screen);

    void Replace(IGameScreen screen);

    void Back();
}
