using System.Collections.Generic;
using DuskVillage.Screens;
using Microsoft.Xna.Framework;

namespace DuskVillage.UI;

public sealed class VerticalMenu
{
    private readonly List<IUiControl> _controls = new();
    private int _focusedIndex;

    public IReadOnlyList<IUiControl> Controls => _controls;

    public int FocusedIndex => _focusedIndex;

    public void SetFocusedIndex(int index)
    {
        if (_controls.Count == 0)
        {
            _focusedIndex = 0;
            return;
        }

        _focusedIndex = System.Math.Clamp(index, 0, _controls.Count - 1);
        if (!_controls[_focusedIndex].CanFocus)
        {
            MoveFocus(1);
        }
    }

    public void Clear()
    {
        _controls.Clear();
        _focusedIndex = 0;
    }

    public void Add(IUiControl control)
    {
        _controls.Add(control);
        if (_controls.Count == 1 && !control.CanFocus)
        {
            MoveFocus(1);
        }
    }

    public void Arrange(int x, int y, int width, int rowHeight, int spacing)
    {
        for (var i = 0; i < _controls.Count; i++)
        {
            _controls[i].Bounds = new Rectangle(x, y + i * (rowHeight + spacing), width, rowHeight);
        }
    }

    public void Update(GameScreenContext context)
    {
        if (_controls.Count == 0)
        {
            return;
        }

        if (!_controls[_focusedIndex].CanFocus)
        {
            MoveFocus(1);
        }

        for (var i = 0; i < _controls.Count; i++)
        {
            if (_controls[i].CanFocus && IsPointerOverControl(_controls[i], context.Input.Current.MousePosition))
            {
                _focusedIndex = i;
                break;
            }
        }

        if (context.Input.Current.MenuDownPressedFor(context.Settings.Current.Input.ControllerMoveDown))
        {
            MoveFocus(1);
        }

        if (context.Input.Current.MenuUpPressedFor(context.Settings.Current.Input.ControllerMoveUp))
        {
            MoveFocus(-1);
        }

        for (var i = 0; i < _controls.Count; i++)
        {
            _controls[i].Update(context, i == _focusedIndex);
        }
    }

    public void UpdateFocusedControl(GameScreenContext context, bool allowMouseFocus = true)
    {
        if (_controls.Count == 0)
        {
            return;
        }

        if (!_controls[_focusedIndex].CanFocus)
        {
            MoveFocus(1);
        }

        if (allowMouseFocus)
        {
            for (var i = 0; i < _controls.Count; i++)
            {
                if (_controls[i].CanFocus && IsPointerOverControl(_controls[i], context.Input.Current.MousePosition))
                {
                    _focusedIndex = i;
                    break;
                }
            }
        }

        for (var i = 0; i < _controls.Count; i++)
        {
            _controls[i].Update(context, i == _focusedIndex);
        }
    }

    public void Draw(UiDrawContext draw)
    {
        for (var i = 0; i < _controls.Count; i++)
        {
            _controls[i].Draw(draw, i == _focusedIndex);
        }
    }

    private void MoveFocus(int direction)
    {
        if (_controls.Count == 0)
        {
            return;
        }

        for (var attempt = 0; attempt < _controls.Count; attempt++)
        {
            _focusedIndex = (_focusedIndex + direction) % _controls.Count;
            if (_focusedIndex < 0)
            {
                _focusedIndex = _controls.Count - 1;
            }

            if (_controls[_focusedIndex].CanFocus)
            {
                return;
            }
        }
    }

    private static bool IsPointerOverControl(IUiControl control, Point pointer)
    {
        if (control.PointerClipBounds.HasValue && !control.PointerClipBounds.Value.Contains(pointer))
        {
            return false;
        }

        return control.Bounds.Contains(pointer);
    }
}
