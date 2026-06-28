using System;
using DuskVillage.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DuskVillage.UI;

public sealed class ToggleControl : UiControlBase
{
    private readonly string _labelKey;
    private readonly Func<bool> _getValue;
    private readonly Action<bool> _setValue;

    public ToggleControl(string labelKey, Func<bool> getValue, Action<bool> setValue)
    {
        _labelKey = labelKey;
        _getValue = getValue;
        _setValue = setValue;
    }

    public override void Update(GameScreenContext context, bool hasFocus)
    {
        if (!IsEnabled)
        {
            return;
        }

        if ((hasFocus && (context.Input.Current.ConfirmPressedFor(context.Settings.Current.Input.ControllerConfirm) || context.Input.Current.WasKeyPressed(Keys.Space))) || WasClicked(context))
        {
            _setValue(!_getValue());
        }
    }

    public override void Draw(UiDrawContext draw, bool hasFocus)
    {
        draw.Fill(Bounds, hasFocus ? draw.Theme.PanelAlt : draw.Theme.Panel);
        draw.Border(Bounds, hasFocus ? draw.Theme.Accent : draw.Theme.Border);

        var label = draw.Localization.Text(_labelKey);
        var value = _getValue() ? draw.Localization.Text("common.on") : draw.Localization.Text("common.off");
        draw.Text(label, new Vector2(Bounds.X + 12, Bounds.Y + 11), TextColor(draw, hasFocus));
        draw.RightAlignedText(value, Bounds.Right - 36, Bounds.Y + 11, TextColor(draw, hasFocus));
    }
}
