using System;
using DuskVillage.Actions;
using DuskVillage.CharacterAssets;
using DuskVillage.Input;
using DuskVillage.Localization;
using DuskVillage.Rendering;
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
        ICharacterPresetStorage characterPresetStorage,
        GameActionRegistry actions,
        ManaSeedCharacterAssetCatalog characterAssets,
        CharacterPortraitRenderer characterPortraitRenderer,
        CharacterSpriteRenderer characterSpriteRenderer,
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
        CharacterPresetStorage = characterPresetStorage;
        Actions = actions;
        CharacterAssets = characterAssets;
        CharacterPortraitRenderer = characterPortraitRenderer;
        CharacterSpriteRenderer = characterSpriteRenderer;
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

    public ICharacterPresetStorage CharacterPresetStorage { get; }

    public GameActionRegistry Actions { get; }

    public ManaSeedCharacterAssetCatalog CharacterAssets { get; }

    public CharacterPortraitRenderer CharacterPortraitRenderer { get; }

    public CharacterSpriteRenderer CharacterSpriteRenderer { get; }

    public IScreenNavigator Navigator { get; }

    public Action ExitGame { get; }

    public Action<GameSettings> ApplySettings { get; }

    public Rectangle ViewBounds => GraphicsDevice.Viewport.Bounds;
}
