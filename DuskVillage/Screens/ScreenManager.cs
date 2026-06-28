using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DuskVillage.Screens;

public sealed class ScreenManager : IScreenNavigator
{
    private readonly Stack<IGameScreen> _screens = new();

    public bool CanGoBack => _screens.Count > 1;

    public void SetRoot(IGameScreen screen)
    {
        while (_screens.Count > 0)
        {
            _screens.Pop().OnExit();
        }

        _screens.Push(screen);
        screen.OnEnter();
    }

    public void Push(IGameScreen screen)
    {
        _screens.Push(screen);
        screen.OnEnter();
    }

    public void Replace(IGameScreen screen)
    {
        if (_screens.Count > 0)
        {
            _screens.Pop().OnExit();
        }

        _screens.Push(screen);
        screen.OnEnter();
    }

    public void Back()
    {
        if (!CanGoBack)
        {
            return;
        }

        _screens.Pop().OnExit();
    }

    public void Update(GameTime gameTime)
    {
        if (_screens.Count > 0)
        {
            _screens.Peek().Update(gameTime);
        }
    }

    public void Draw(GameTime gameTime)
    {
        if (_screens.Count > 0)
        {
            _screens.Peek().Draw(gameTime);
        }
    }
}
