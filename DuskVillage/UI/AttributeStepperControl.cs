using System;
using DuskVillage.Screens;
using Microsoft.Xna.Framework;

namespace DuskVillage.UI;

public sealed class AttributeStepperControl : UiControlBase, IUiTooltipProvider
{
    private readonly string _labelKey;
    private readonly string _tooltipKey;
    private readonly int _minimum;
    private readonly int _maximum;
    private readonly Func<int> _getValue;
    private readonly Action<int> _setValue;
    private readonly Func<bool> _canDecrease;
    private readonly Func<bool> _canIncrease;

    public AttributeStepperControl(
        string labelKey,
        string tooltipKey,
        int minimum,
        int maximum,
        Func<int> getValue,
        Action<int> setValue,
        Func<bool> canDecrease,
        Func<bool> canIncrease)
    {
        _labelKey = labelKey;
        _tooltipKey = tooltipKey;
        _minimum = minimum;
        _maximum = maximum;
        _getValue = getValue;
        _setValue = setValue;
        _canDecrease = canDecrease;
        _canIncrease = canIncrease;
    }

    public override void Update(GameScreenContext context, bool hasFocus)
    {
        if (!IsEnabled)
        {
            return;
        }

        if (hasFocus && context.Input.Current.MenuLeftPressedFor(context.Settings.Current.Input.ControllerMoveLeft))
        {
            Decrease();
        }

        if (hasFocus && (context.Input.Current.MenuRightPressedFor(context.Settings.Current.Input.ControllerMoveRight) || context.Input.Current.ConfirmPressedFor(context.Settings.Current.Input.ControllerConfirm)))
        {
            Increase();
        }

        if (!context.Input.Current.LeftClickStarted)
        {
            return;
        }

        var pointer = context.Input.Current.MousePosition;
        if (MinusBounds.Contains(pointer))
        {
            Decrease();
        }
        else if (PlusBounds.Contains(pointer))
        {
            Increase();
        }
    }

    public override void Draw(UiDrawContext draw, bool hasFocus)
    {
        draw.Fill(Bounds, hasFocus ? draw.Theme.PanelAlt : draw.Theme.Panel);
        draw.Border(Bounds, hasFocus ? draw.Theme.Accent : draw.Theme.Border);

        var label = draw.Localization.Text(_labelKey);
        draw.Text(label, new Vector2(Bounds.X + 12, Bounds.Y + 11), TextColor(draw, hasFocus), 0.9f);

        DrawStepButton(draw, MinusBounds, "-", _canDecrease(), hasFocus);
        draw.CenteredText(_getValue().ToString(), ValueBounds, TextColor(draw, hasFocus), 0.92f);
        DrawStepButton(draw, PlusBounds, "+", _canIncrease(), hasFocus);
    }

    public bool TryGetTooltip(GameScreenContext context, out string tooltipKey, out Point anchor)
    {
        tooltipKey = string.Empty;
        anchor = context.Input.Current.MousePosition;

        if (PointerClipBounds.HasValue && !PointerClipBounds.Value.Contains(anchor))
        {
            return false;
        }

        if (!LabelHoverBounds.Contains(anchor))
        {
            return false;
        }

        tooltipKey = _tooltipKey;
        return true;
    }

    private Rectangle MinusBounds => new(Bounds.Right - 154, Bounds.Y + 8, 36, Bounds.Height - 16);

    private Rectangle ValueBounds => new(Bounds.Right - 108, Bounds.Y + 8, 48, Bounds.Height - 16);

    private Rectangle PlusBounds => new(Bounds.Right - 50, Bounds.Y + 8, 36, Bounds.Height - 16);

    private Rectangle LabelHoverBounds => new(Bounds.X + 12, Bounds.Y, Math.Max(120, Bounds.Width - 190), Bounds.Height);

    private void DrawStepButton(UiDrawContext draw, Rectangle bounds, string text, bool enabled, bool hasFocus)
    {
        draw.Fill(bounds, enabled ? draw.Theme.BackgroundTop : draw.Theme.Panel);
        draw.Border(bounds, enabled && hasFocus ? draw.Theme.Accent : draw.Theme.Border, 1);
        draw.CenteredText(text, bounds, enabled ? TextColor(draw, hasFocus) : draw.Theme.DisabledText, 0.95f);
    }

    private void Decrease()
    {
        if (!_canDecrease())
        {
            return;
        }

        _setValue(Math.Clamp(_getValue() - 1, _minimum, _maximum));
    }

    private void Increase()
    {
        if (!_canIncrease())
        {
            return;
        }

        _setValue(Math.Clamp(_getValue() + 1, _minimum, _maximum));
    }
}
