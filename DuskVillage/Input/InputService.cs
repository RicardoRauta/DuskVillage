using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DuskVillage.Input;

public sealed class InputService : IInputService
{
    private const float StickThreshold = 0.5f;
    private const int InitialRepeatDelayFrames = 18;
    private const int RepeatIntervalFrames = 6;

    private readonly PlayerIndex[] _playerIndices =
    {
        PlayerIndex.One,
        PlayerIndex.Two,
        PlayerIndex.Three,
        PlayerIndex.Four
    };

    private readonly GamePadState[] _previousGamePads = new GamePadState[4];

    private KeyboardState _previousKeyboard;
    private MouseState _previousMouse;
    private int _activeGamePadIndex = -1;
    private int _upHeldFrames;
    private int _downHeldFrames;
    private int _leftHeldFrames;
    private int _rightHeldFrames;

    public InputSnapshot Current { get; private set; } = InputSnapshot.Empty;

    public void Update()
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();
        var gamePads = ReadGamePads();
        var activeIndex = ResolveActiveGamePadIndex(gamePads);
        var gamePad = activeIndex >= 0 ? gamePads[activeIndex] : new GamePadState();
        var previousGamePad = activeIndex >= 0 ? _previousGamePads[activeIndex] : new GamePadState();

        var gamePadMenuUpPressed = UpdateRepeat(ref _upHeldFrames, IsGamePadUpHeld(gamePad));
        var gamePadMenuDownPressed = UpdateRepeat(ref _downHeldFrames, IsGamePadDownHeld(gamePad));
        var gamePadMenuLeftPressed = UpdateRepeat(ref _leftHeldFrames, IsGamePadLeftHeld(gamePad));
        var gamePadMenuRightPressed = UpdateRepeat(ref _rightHeldFrames, IsGamePadRightHeld(gamePad));

        Current = new InputSnapshot(
            keyboard,
            _previousKeyboard,
            mouse,
            _previousMouse,
            gamePad,
            previousGamePad,
            activeIndex,
            gamePadMenuUpPressed,
            gamePadMenuDownPressed,
            gamePadMenuLeftPressed,
            gamePadMenuRightPressed);

        _previousKeyboard = keyboard;
        _previousMouse = mouse;

        for (var i = 0; i < _previousGamePads.Length; i++)
        {
            _previousGamePads[i] = gamePads[i];
        }
    }

    private GamePadState[] ReadGamePads()
    {
        var states = new GamePadState[_playerIndices.Length];
        for (var i = 0; i < _playerIndices.Length; i++)
        {
            states[i] = GamePad.GetState(_playerIndices[i], GamePadDeadZone.Circular);
        }

        return states;
    }

    private int ResolveActiveGamePadIndex(GamePadState[] gamePads)
    {
        if (_activeGamePadIndex >= 0 &&
            _activeGamePadIndex < gamePads.Length &&
            gamePads[_activeGamePadIndex].IsConnected)
        {
            for (var i = 0; i < gamePads.Length; i++)
            {
                if (i != _activeGamePadIndex && HasNewActivity(gamePads[i], _previousGamePads[i]))
                {
                    _activeGamePadIndex = i;
                    return _activeGamePadIndex;
                }
            }

            return _activeGamePadIndex;
        }

        for (var i = 0; i < gamePads.Length; i++)
        {
            if (gamePads[i].IsConnected)
            {
                _activeGamePadIndex = i;
                ResetNavigationRepeat();
                return _activeGamePadIndex;
            }
        }

        _activeGamePadIndex = -1;
        ResetNavigationRepeat();
        return _activeGamePadIndex;
    }

    private static bool HasNewActivity(GamePadState current, GamePadState previous)
    {
        if (!current.IsConnected)
        {
            return false;
        }

        return (current.Buttons.A == ButtonState.Pressed && previous.Buttons.A == ButtonState.Released) ||
            (current.Buttons.B == ButtonState.Pressed && previous.Buttons.B == ButtonState.Released) ||
            (current.Buttons.Start == ButtonState.Pressed && previous.Buttons.Start == ButtonState.Released) ||
            (current.Buttons.Back == ButtonState.Pressed && previous.Buttons.Back == ButtonState.Released) ||
            IsGamePadUpHeld(current) ||
            IsGamePadDownHeld(current) ||
            IsGamePadLeftHeld(current) ||
            IsGamePadRightHeld(current);
    }

    private static bool IsGamePadUpHeld(GamePadState state)
    {
        return state.DPad.Up == ButtonState.Pressed || state.ThumbSticks.Left.Y > StickThreshold;
    }

    private static bool IsGamePadDownHeld(GamePadState state)
    {
        return state.DPad.Down == ButtonState.Pressed || state.ThumbSticks.Left.Y < -StickThreshold;
    }

    private static bool IsGamePadLeftHeld(GamePadState state)
    {
        return state.DPad.Left == ButtonState.Pressed || state.ThumbSticks.Left.X < -StickThreshold;
    }

    private static bool IsGamePadRightHeld(GamePadState state)
    {
        return state.DPad.Right == ButtonState.Pressed || state.ThumbSticks.Left.X > StickThreshold;
    }

    private static bool UpdateRepeat(ref int heldFrames, bool isHeld)
    {
        if (!isHeld)
        {
            heldFrames = 0;
            return false;
        }

        heldFrames++;
        if (heldFrames == 1)
        {
            return true;
        }

        return heldFrames > InitialRepeatDelayFrames &&
            (heldFrames - InitialRepeatDelayFrames) % RepeatIntervalFrames == 0;
    }

    private void ResetNavigationRepeat()
    {
        _upHeldFrames = 0;
        _downHeldFrames = 0;
        _leftHeldFrames = 0;
        _rightHeldFrames = 0;
    }
}
