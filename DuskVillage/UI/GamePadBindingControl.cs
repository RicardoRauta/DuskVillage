using System;
using DuskVillage.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DuskVillage.UI;

public sealed class GamePadBindingControl : UiControlBase, IInputCaptureControl
{
    private readonly string _labelKey;
    private readonly Func<Buttons> _getButton;
    private readonly Action<Buttons> _setButton;
    private bool _isCapturing;

    public bool IsCapturingInput => _isCapturing;

    public GamePadBindingControl(string labelKey, Func<Buttons> getButton, Action<Buttons> setButton)
    {
        _labelKey = labelKey;
        _getButton = getButton;
        _setButton = setButton;
    }

    public override void Update(GameScreenContext context, bool hasFocus)
    {
        if (!IsEnabled)
        {
            return;
        }

        if (_isCapturing)
        {
            if (context.Input.Current.WasKeyPressed(Keys.Escape))
            {
                _isCapturing = false;
                return;
            }

            var button = context.Input.Current.FirstNewButtonPress();
            if (!button.HasValue)
            {
                return;
            }

            _setButton(button.Value);
            _isCapturing = false;
            return;
        }

        if ((hasFocus && context.Input.Current.ConfirmPressedFor(context.Settings.Current.Input.ControllerConfirm)) || WasClicked(context))
        {
            _isCapturing = true;
        }
    }

    public override void Draw(UiDrawContext draw, bool hasFocus)
    {
        draw.Fill(Bounds, hasFocus ? draw.Theme.PanelAlt : draw.Theme.Panel);
        draw.Border(Bounds, hasFocus ? draw.Theme.Accent : draw.Theme.Border);

        var label = draw.Localization.Text(_labelKey);
        var value = _isCapturing ? draw.Localization.Text("settings.press_button") : _getButton().ToString();
        draw.Text(label, new Vector2(Bounds.X + 12, Bounds.Y + 11), TextColor(draw, hasFocus), 0.9f);
        draw.RightAlignedText(value, Bounds.Right - 36, Bounds.Y + 11, TextColor(draw, hasFocus), 0.9f);
    }
}
