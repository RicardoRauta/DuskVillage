using System;
using DuskVillage.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DuskVillage.UI;

public sealed class KeyBindingControl : UiControlBase
{
    private readonly string _labelKey;
    private readonly Func<Keys> _getKey;
    private readonly Action<Keys> _setKey;
    private bool _isCapturing;

    public KeyBindingControl(string labelKey, Func<Keys> getKey, Action<Keys> setKey)
    {
        _labelKey = labelKey;
        _getKey = getKey;
        _setKey = setKey;
    }

    public override void Update(GameScreenContext context, bool hasFocus)
    {
        if (!IsEnabled)
        {
            return;
        }

        if (_isCapturing)
        {
            if (context.Input.Current.GamePadCancelPressed)
            {
                _isCapturing = false;
                return;
            }

            var key = context.Input.Current.FirstNewKeyPress();
            if (!key.HasValue)
            {
                return;
            }

            if (key.Value != Keys.Escape)
            {
                _setKey(key.Value);
            }

            _isCapturing = false;
            return;
        }

        if ((hasFocus && context.Input.Current.ConfirmPressed) || WasClicked(context))
        {
            _isCapturing = true;
        }
    }

    public override void Draw(UiDrawContext draw, bool hasFocus)
    {
        draw.Fill(Bounds, hasFocus ? draw.Theme.PanelAlt : draw.Theme.Panel);
        draw.Border(Bounds, hasFocus ? draw.Theme.Accent : draw.Theme.Border);

        var label = draw.Localization.Text(_labelKey);
        var value = _isCapturing ? draw.Localization.Text("settings.press_key") : _getKey().ToString();
        draw.Text(label, new Vector2(Bounds.X + 12, Bounds.Y + 11), TextColor(draw, hasFocus), 0.9f);
        draw.RightAlignedText(value, Bounds.Right - 36, Bounds.Y + 11, TextColor(draw, hasFocus), 0.9f);
    }
}
