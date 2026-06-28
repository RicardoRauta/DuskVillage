using DuskVillage.CharacterAssets;
using DuskVillage.Core;
using DuskVillage.Input;
using DuskVillage.Localization;
using DuskVillage.Rendering;
using DuskVillage.Saving;
using DuskVillage.Screens;
using DuskVillage.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuskVillage
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly InputService _input;
        private readonly ScreenManager _screenManager;

        private SpriteBatch _spriteBatch;
        private Texture2D _pixel;
        private SpriteFont _menuFont;
        private GameSettingsService _settings;
        private LocalizationService _localization;
        private FileSaveSlotProvider _saveSlots;
        private FileCharacterPresetStorage _characterPresetStorage;
        private ManaSeedCharacterAssetCatalog _characterAssetCatalog;
        private CharacterPortraitRenderer _characterPortraitRenderer;
        private GameScreenContext _screenContext;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720
            };

            _input = new InputService();
            _screenManager = new ScreenManager();

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.Title = "Dusk Village";
        }

        protected override void Initialize()
        {
            _settings = new GameSettingsService(GameDirectories.SettingsFilePath);
            _settings.Load();
            ConfigureDisplay(_settings.Current.Display, applyChanges: false);

            base.Initialize();
            ConfigureDisplay(_settings.Current.Display, applyChanges: true);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            _menuFont = Content.Load<SpriteFont>("Fonts/Menu");
            _localization = new LocalizationService(GameDirectories.LocalizationDirectory, _settings.Current.General.LanguageCode);
            _saveSlots = new FileSaveSlotProvider(GameDirectories.SavesDirectory);
            _characterPresetStorage = new FileCharacterPresetStorage(GameDirectories.CharacterPresetsDirectory);
            _characterAssetCatalog = ManaSeedCharacterAssetCatalog.Load(GameDirectories.ManaSeedFarmerSpriteZipPath);
            _characterPortraitRenderer = new CharacterPortraitRenderer(GraphicsDevice, _characterAssetCatalog);

            _screenContext = new GameScreenContext(
                _graphics,
                GraphicsDevice,
                _spriteBatch,
                _menuFont,
                _pixel,
                _input,
                _localization,
                _settings,
                _saveSlots,
                _characterPresetStorage,
                _characterAssetCatalog,
                _characterPortraitRenderer,
                _screenManager,
                Exit,
                ApplySettings);

            _screenManager.SetRoot(new MainMenuScreen(_screenContext));
        }

        protected override void Update(GameTime gameTime)
        {
            _input.Update();
            _screenManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _screenManager.Draw(gameTime);

            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            _characterPortraitRenderer?.Dispose();
            _pixel?.Dispose();
            _spriteBatch?.Dispose();
            base.UnloadContent();
        }

        private void ApplySettings(GameSettings settings)
        {
            ConfigureDisplay(settings.Display, applyChanges: true);
            _localization?.SetLanguage(settings.General.LanguageCode);
        }

        private void ConfigureDisplay(DisplaySettings display, bool applyChanges)
        {
            _graphics.PreferredBackBufferWidth = display.Width;
            _graphics.PreferredBackBufferHeight = display.Height;
            _graphics.IsFullScreen = display.Fullscreen;

            if (applyChanges)
            {
                _graphics.ApplyChanges();
            }
        }
    }
}
