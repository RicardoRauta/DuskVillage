using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DuskVillage.Input;

public sealed class InputSnapshot
{
    public static InputSnapshot Empty { get; } = new(
        new KeyboardState(),
        new KeyboardState(),
        new MouseState(),
        new MouseState(),
        new GamePadState(),
        new GamePadState(),
        -1,
        false,
        false,
        false,
        false);

    public InputSnapshot(
        KeyboardState keyboard,
        KeyboardState previousKeyboard,
        MouseState mouse,
        MouseState previousMouse,
        GamePadState gamePad,
        GamePadState previousGamePad,
        int gamePadIndex,
        bool gamePadMenuUpPressed,
        bool gamePadMenuDownPressed,
        bool gamePadMenuLeftPressed,
        bool gamePadMenuRightPressed)
    {
        Keyboard = keyboard;
        PreviousKeyboard = previousKeyboard;
        Mouse = mouse;
        PreviousMouse = previousMouse;
        GamePad = gamePad;
        PreviousGamePad = previousGamePad;
        GamePadIndex = gamePadIndex;
        GamePadMenuUpPressed = gamePadMenuUpPressed;
        GamePadMenuDownPressed = gamePadMenuDownPressed;
        GamePadMenuLeftPressed = gamePadMenuLeftPressed;
        GamePadMenuRightPressed = gamePadMenuRightPressed;
    }

    public KeyboardState Keyboard { get; }

    public KeyboardState PreviousKeyboard { get; }

    public MouseState Mouse { get; }

    public MouseState PreviousMouse { get; }

    public GamePadState GamePad { get; }

    public GamePadState PreviousGamePad { get; }

    public int GamePadIndex { get; }

    public bool GamePadMenuUpPressed { get; }

    public bool GamePadMenuDownPressed { get; }

    public bool GamePadMenuLeftPressed { get; }

    public bool GamePadMenuRightPressed { get; }

    public Point MousePosition => new(Mouse.X, Mouse.Y);

    public int ScrollDelta => Mouse.ScrollWheelValue - PreviousMouse.ScrollWheelValue;

    public bool LeftClickStarted =>
        Mouse.LeftButton == ButtonState.Pressed &&
        PreviousMouse.LeftButton == ButtonState.Released;

    public bool IsGamePadConnected => GamePad.IsConnected;

    public bool ConfirmPressed => WasKeyPressed(Keys.Enter) || WasButtonPressed(Buttons.A) || WasButtonPressed(Buttons.Start);

    public bool ConfirmPressedFor(Buttons controllerButton)
    {
        return WasKeyPressed(Keys.Enter) || WasButtonPressed(controllerButton);
    }

    public bool MenuUpPressed =>
        WasKeyPressed(Keys.Up) ||
        GamePadMenuUpPressed;

    public bool MenuUpPressedFor(Buttons controllerButton)
    {
        return WasKeyPressed(Keys.Up) || GamePadMenuUpPressed || WasButtonPressed(controllerButton);
    }

    public bool MenuDownPressed =>
        WasKeyPressed(Keys.Down) ||
        WasKeyPressed(Keys.Tab) ||
        GamePadMenuDownPressed;

    public bool MenuDownPressedFor(Buttons controllerButton)
    {
        return WasKeyPressed(Keys.Down) ||
            WasKeyPressed(Keys.Tab) ||
            GamePadMenuDownPressed ||
            WasButtonPressed(controllerButton);
    }

    public bool MenuLeftPressed =>
        WasKeyPressed(Keys.Left) ||
        GamePadMenuLeftPressed;

    public bool MenuLeftPressedFor(Buttons controllerButton)
    {
        return WasKeyPressed(Keys.Left) || GamePadMenuLeftPressed || WasButtonPressed(controllerButton);
    }

    public bool MenuRightPressed =>
        WasKeyPressed(Keys.Right) ||
        GamePadMenuRightPressed;

    public bool MenuRightPressedFor(Buttons controllerButton)
    {
        return WasKeyPressed(Keys.Right) || GamePadMenuRightPressed || WasButtonPressed(controllerButton);
    }

    public bool GamePadCancelPressed => WasButtonPressed(Buttons.B) || WasButtonPressed(Buttons.Back);

    public bool GamePadCancelPressedFor(Buttons controllerButton)
    {
        return WasButtonPressed(controllerButton);
    }

    public bool GamePadStartPressed => WasButtonPressed(Buttons.Start);

    public bool PreviousTabPressed => WasKeyPressed(Keys.PageUp) || WasButtonPressed(Buttons.LeftShoulder);

    public bool NextTabPressed => WasKeyPressed(Keys.PageDown) || WasButtonPressed(Buttons.RightShoulder);

    public bool IsKeyDown(Keys key)
    {
        return Keyboard.IsKeyDown(key);
    }

    public bool WasKeyPressed(Keys key)
    {
        return Keyboard.IsKeyDown(key) && !PreviousKeyboard.IsKeyDown(key);
    }

    public bool WasButtonPressed(Buttons button)
    {
        return GamePad.IsButtonDown(button) && !PreviousGamePad.IsButtonDown(button);
    }

    public Buttons? FirstNewButtonPress()
    {
        foreach (var button in GamePadButtons)
        {
            if (WasButtonPressed(button))
            {
                return button;
            }
        }

        return null;
    }

    public Keys? FirstNewKeyPress()
    {
        foreach (var key in Keyboard.GetPressedKeys())
        {
            if (!PreviousKeyboard.IsKeyDown(key))
            {
                return key;
            }
        }

        return null;
    }

    public IReadOnlyList<char> TypedCharacters()
    {
        var characters = new List<char>();
        var shift = Keyboard.IsKeyDown(Keys.LeftShift) || Keyboard.IsKeyDown(Keys.RightShift);

        foreach (var key in Keyboard.GetPressedKeys())
        {
            if (PreviousKeyboard.IsKeyDown(key))
            {
                continue;
            }

            var character = KeyToCharacter(key, shift);
            if (character.HasValue)
            {
                characters.Add(character.Value);
            }
        }

        return characters;
    }

    private static char? KeyToCharacter(Keys key, bool shift)
    {
        if (key >= Keys.A && key <= Keys.Z)
        {
            var letter = (char)('a' + (key - Keys.A));
            return shift ? char.ToUpperInvariant(letter) : letter;
        }

        if (key >= Keys.D0 && key <= Keys.D9)
        {
            const string normal = "0123456789";
            const string shifted = ")!@#$%^&*(";
            var index = key - Keys.D0;
            return shift ? shifted[index] : normal[index];
        }

        if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
        {
            return (char)('0' + (key - Keys.NumPad0));
        }

        return key switch
        {
            Keys.Space => ' ',
            Keys.OemMinus => shift ? '_' : '-',
            Keys.OemPlus => shift ? '+' : '=',
            Keys.OemPeriod => shift ? '>' : '.',
            Keys.OemComma => shift ? '<' : ',',
            Keys.OemQuestion => shift ? '?' : '/',
            Keys.OemSemicolon => shift ? ':' : ';',
            Keys.OemQuotes => shift ? '"' : '\'',
            _ => null
        };
    }

    private static readonly Buttons[] GamePadButtons =
    {
        Buttons.A,
        Buttons.B,
        Buttons.X,
        Buttons.Y,
        Buttons.Back,
        Buttons.Start,
        Buttons.LeftShoulder,
        Buttons.RightShoulder,
        Buttons.LeftStick,
        Buttons.RightStick,
        Buttons.DPadUp,
        Buttons.DPadDown,
        Buttons.DPadLeft,
        Buttons.DPadRight,
        Buttons.LeftTrigger,
        Buttons.RightTrigger
    };

}
