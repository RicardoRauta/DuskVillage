using System;
using DuskVillage.Input;
using DuskVillage.Localization;
using DuskVillage.Saving;
using DuskVillage.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuskVillage.Screens;

public sealed class GameScreenContext
{
    public GameScreenContext(
        GraphicsDeviceManager graphics,
        GraphicsDevice graphicsDevice,
        SpriteBatch spriteBatch,
        SpriteFont font,
        Texture2D pixel,
        IInputService input,
        ILocalizationService localization,
        IGameSettingsService settings,
        ISaveSlotProvider saveSlots,
        IScreenNavigator navigator,
        Action exitGame,
        Action<GameSettings> applySettings)
    {
        Graphics = graphics;
        GraphicsDevice = graphicsDevice;
        SpriteBatch = spriteBatch;
        Font = font;
        Pixel = pixel;
        Input = input;
        Localization = localization;
        Settings = settings;
        SaveSlots = saveSlots;
        Navigator = navigator;
        ExitGame = exitGame;
        ApplySettings = applySettings;
    }

    public GraphicsDeviceManager Graphics { get; }

    public GraphicsDevice GraphicsDevice { get; }

    public SpriteBatch SpriteBatch { get; }

    public SpriteFont Font { get; }

    public Texture2D Pixel { get; }

    public IInputService Input { get; }

    public ILocalizationService Localization { get; }

    public IGameSettingsService Settings { get; }

    public ISaveSlotProvider SaveSlots { get; }

    public IScreenNavigator Navigator { get; }

    public Action ExitGame { get; }

    public Action<GameSettings> ApplySettings { get; }

    public Rectangle ViewBounds => GraphicsDevice.Viewport.Bounds;
}
