using System;
using System.Collections.Generic;
using DuskVillage.Screens;
using Microsoft.Xna.Framework;

namespace DuskVillage.UI;

public sealed class SelectorControl : UiControlBase
{
    private readonly string _labelKey;
    private readonly IReadOnlyList<SelectorOption> _options;
    private readonly Func<string> _getValue;
    private readonly Action<string> _setValue;

    public SelectorControl(
        string labelKey,
        IReadOnlyList<SelectorOption> options,
        Func<string> getValue,
        Action<string> setValue)
    {
        _labelKey = labelKey;
        _options = options;
        _getValue = getValue;
        _setValue = setValue;
    }

    public override void Update(GameScreenContext context, bool hasFocus)
    {
        if (!IsEnabled || _options.Count == 0)
        {
            return;
        }

        if (hasFocus && context.Input.Current.MenuLeftPressedFor(context.Settings.Current.Input.ControllerMoveLeft))
        {
            Move(-1);
        }

        if (hasFocus && (context.Input.Current.MenuRightPressedFor(context.Settings.Current.Input.ControllerMoveRight) || context.Input.Current.ConfirmPressedFor(context.Settings.Current.Input.ControllerConfirm)))
        {
            Move(1);
        }

        if (WasClicked(context))
        {
            Move(1);
        }
    }

    public override void Draw(UiDrawContext draw, bool hasFocus)
    {
        draw.Fill(Bounds, hasFocus ? draw.Theme.PanelAlt : draw.Theme.Panel);
        draw.Border(Bounds, hasFocus ? draw.Theme.Accent : draw.Theme.Border);

        var label = draw.Localization.Text(_labelKey);
        var value = draw.Localization.Text(CurrentOption.LabelKey);
        var valueText = "< " + value + " >";
        draw.Text(label, new Vector2(Bounds.X + 12, Bounds.Y + 11), TextColor(draw, hasFocus));
        draw.RightAlignedText(valueText, Bounds.Right - 36, Bounds.Y + 11, TextColor(draw, hasFocus));
    }

    private SelectorOption CurrentOption
    {
        get
        {
            var index = CurrentIndex;
            return _options[Math.Clamp(index, 0, _options.Count - 1)];
        }
    }

    private int CurrentIndex
    {
        get
        {
            var value = _getValue();
            for (var i = 0; i < _options.Count; i++)
            {
                if (_options[i].Value == value)
                {
                    return i;
                }
            }

            return 0;
        }
    }

    private void Move(int direction)
    {
        var next = (CurrentIndex + direction) % _options.Count;
        if (next < 0)
        {
            next = _options.Count - 1;
        }

        _setValue(_options[next].Value);
    }
}
