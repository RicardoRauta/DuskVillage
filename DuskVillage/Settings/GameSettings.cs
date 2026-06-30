using System;
using Microsoft.Xna.Framework.Input;

namespace DuskVillage.Settings;

public sealed class GameSettings
{
    public GeneralSettings General { get; set; } = new();

    public DisplaySettings Display { get; set; } = new();

    public AudioSettings Audio { get; set; } = new();

    public InputSettings Input { get; set; } = new();

    public static GameSettings CreateDefault()
    {
        return new GameSettings
        {
            General = new GeneralSettings
            {
                LanguageCode = "en"
            },
            Display = new DisplaySettings
            {
                Width = 1280,
                Height = 720,
                Fullscreen = false,
                UiScale = 1f
            },
            Audio = new AudioSettings
            {
                MasterVolume = 1f,
                MusicVolume = 0.8f,
                EffectsVolume = 0.8f
            },
            Input = new InputSettings
            {
                MoveUp = Keys.W,
                MoveDown = Keys.S,
                MoveLeft = Keys.A,
                MoveRight = Keys.D,
                Interact = Keys.F,
                Inventory = Keys.E,
                Back = Keys.Escape,
                Pause = Keys.P,
                MouseSensitivity = 1f,
                ControllerSensitivity = 1f,
                ControllerConfirm = Buttons.A,
                ControllerBack = Buttons.B,
                ControllerInventory = Buttons.Y,
                ControllerPause = Buttons.Start,
                ControllerMoveUp = Buttons.DPadUp,
                ControllerMoveDown = Buttons.DPadDown,
                ControllerMoveLeft = Buttons.DPadLeft,
                ControllerMoveRight = Buttons.DPadRight
            }
        };
    }

    public GameSettings Clone()
    {
        return new GameSettings
        {
            General = General.Clone(),
            Display = Display.Clone(),
            Audio = Audio.Clone(),
            Input = Input.Clone()
        };
    }

    public void Normalize()
    {
        var defaults = CreateDefault();

        if (General == null)
        {
            General = defaults.General;
        }

        if (Display == null)
        {
            Display = defaults.Display;
        }

        if (Audio == null)
        {
            Audio = defaults.Audio;
        }

        if (Input == null)
        {
            Input = defaults.Input;
        }

        if (string.IsNullOrWhiteSpace(General.LanguageCode))
        {
            General.LanguageCode = defaults.General.LanguageCode;
        }

        if (Display.Width <= 0 || Display.Height <= 0)
        {
            Display.Width = defaults.Display.Width;
            Display.Height = defaults.Display.Height;
        }

        Display.UiScale = Math.Clamp(Display.UiScale, 0.75f, 2f);
        Audio.MasterVolume = Math.Clamp(Audio.MasterVolume, 0f, 1f);
        Audio.MusicVolume = Math.Clamp(Audio.MusicVolume, 0f, 1f);
        Audio.EffectsVolume = Math.Clamp(Audio.EffectsVolume, 0f, 1f);
        Input.MouseSensitivity = Math.Clamp(Input.MouseSensitivity, 0.25f, 2f);
        Input.ControllerSensitivity = Math.Clamp(Input.ControllerSensitivity, 0.25f, 2f);
        if (Input.Inventory == Keys.None)
        {
            Input.Inventory = defaults.Input.Inventory;
        }

        if (Input.Interact == Keys.None)
        {
            Input.Interact = defaults.Input.Interact;
        }

        if (Input.Inventory == Input.Interact)
        {
            Input.Inventory = defaults.Input.Inventory;
            Input.Interact = defaults.Input.Interact;
        }

        if (Input.Interact == Keys.E && Input.Inventory == Keys.I)
        {
            Input.Inventory = defaults.Input.Inventory;
            Input.Interact = defaults.Input.Interact;
        }

        if (Input.ControllerInventory == Input.ControllerConfirm)
        {
            Input.ControllerInventory = defaults.Input.ControllerInventory;
        }
    }
}

public sealed class GeneralSettings
{
    public string LanguageCode { get; set; } = "en";

    public GeneralSettings Clone()
    {
        return new GeneralSettings
        {
            LanguageCode = LanguageCode
        };
    }
}

public sealed class DisplaySettings
{
    public int Width { get; set; } = 1280;

    public int Height { get; set; } = 720;

    public bool Fullscreen { get; set; }

    public float UiScale { get; set; } = 1f;

    public DisplaySettings Clone()
    {
        return new DisplaySettings
        {
            Width = Width,
            Height = Height,
            Fullscreen = Fullscreen,
            UiScale = UiScale
        };
    }
}

public sealed class AudioSettings
{
    public float MasterVolume { get; set; } = 1f;

    public float MusicVolume { get; set; } = 0.8f;

    public float EffectsVolume { get; set; } = 0.8f;

    public AudioSettings Clone()
    {
        return new AudioSettings
        {
            MasterVolume = MasterVolume,
            MusicVolume = MusicVolume,
            EffectsVolume = EffectsVolume
        };
    }
}

public sealed class InputSettings
{
    public Keys MoveUp { get; set; } = Keys.W;

    public Keys MoveDown { get; set; } = Keys.S;

    public Keys MoveLeft { get; set; } = Keys.A;

    public Keys MoveRight { get; set; } = Keys.D;

    public Keys Interact { get; set; } = Keys.F;

    public Keys Inventory { get; set; } = Keys.E;

    public Keys Back { get; set; } = Keys.Escape;

    public Keys Pause { get; set; } = Keys.P;

    public float MouseSensitivity { get; set; } = 1f;

    public float ControllerSensitivity { get; set; } = 1f;

    public Buttons ControllerConfirm { get; set; } = Buttons.A;

    public Buttons ControllerBack { get; set; } = Buttons.B;

    public Buttons ControllerInventory { get; set; } = Buttons.Y;

    public Buttons ControllerPause { get; set; } = Buttons.Start;

    public Buttons ControllerMoveUp { get; set; } = Buttons.DPadUp;

    public Buttons ControllerMoveDown { get; set; } = Buttons.DPadDown;

    public Buttons ControllerMoveLeft { get; set; } = Buttons.DPadLeft;

    public Buttons ControllerMoveRight { get; set; } = Buttons.DPadRight;

    public InputSettings Clone()
    {
        return new InputSettings
        {
            MoveUp = MoveUp,
            MoveDown = MoveDown,
            MoveLeft = MoveLeft,
            MoveRight = MoveRight,
            Interact = Interact,
            Inventory = Inventory,
            Back = Back,
            Pause = Pause,
            MouseSensitivity = MouseSensitivity,
            ControllerSensitivity = ControllerSensitivity,
            ControllerConfirm = ControllerConfirm,
            ControllerBack = ControllerBack,
            ControllerInventory = ControllerInventory,
            ControllerPause = ControllerPause,
            ControllerMoveUp = ControllerMoveUp,
            ControllerMoveDown = ControllerMoveDown,
            ControllerMoveLeft = ControllerMoveLeft,
            ControllerMoveRight = ControllerMoveRight
        };
    }
}
