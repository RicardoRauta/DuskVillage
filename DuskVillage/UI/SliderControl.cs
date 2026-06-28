using System;
using DuskVillage.Screens;
using Microsoft.Xna.Framework;

namespace DuskVillage.UI;

public sealed class SliderControl : UiControlBase
{
    private readonly string _labelKey;
    private readonly float _min;
    private readonly float _max;
    private readonly float _step;
    private readonly Func<float> _getValue;
    private readonly Action<float> _setValue;
    private readonly Func<float, string> _formatValue;

    public SliderControl(
        string labelKey,
        float min,
        float max,
        float step,
        Func<float> getValue,
        Action<float> setValue,
        Func<float, string> formatValue)
    {
        _labelKey = labelKey;
        _min = min;
        _max = max;
        _step = step;
        _getValue = getValue;
        _setValue = setValue;
        _formatValue = formatValue;
    }

    public override void Update(GameScreenContext context, bool hasFocus)
    {
        if (!IsEnabled)
        {
            return;
        }

        if (hasFocus && context.Input.Current.MenuLeftPressedFor(context.Settings.Current.Input.ControllerMoveLeft))
        {
            Adjust(-_step);
        }

        if (hasFocus && context.Input.Current.MenuRightPressedFor(context.Settings.Current.Input.ControllerMoveRight))
        {
            Adjust(_step);
        }

        if (WasClicked(context))
        {
            var track = TrackBounds;
            var normalized = Math.Clamp((context.Input.Current.MousePosition.X - track.X) / (float)track.Width, 0f, 1f);
            Set(_min + (_max - _min) * normalized);
        }
    }

    public override void Draw(UiDrawContext draw, bool hasFocus)
    {
        draw.Fill(Bounds, hasFocus ? draw.Theme.PanelAlt : draw.Theme.Panel);
        draw.Border(Bounds, hasFocus ? draw.Theme.Accent : draw.Theme.Border);

        var label = draw.Localization.Text(_labelKey);
        var value = _formatValue(_getValue());
        draw.Text(label, new Vector2(Bounds.X + 12, Bounds.Y + 8), TextColor(draw, hasFocus), 0.9f);
        draw.RightAlignedText(value, Bounds.Right - 36, Bounds.Y + 8, TextColor(draw, hasFocus), 0.9f);

        var track = TrackBounds;
        draw.Fill(track, draw.Theme.BackgroundTop);

        var normalized = (_getValue() - _min) / (_max - _min);
        var filledWidth = (int)(track.Width * Math.Clamp(normalized, 0f, 1f));
        draw.Fill(new Rectangle(track.X, track.Y, filledWidth, track.Height), draw.Theme.AccentMuted);
    }

    private Rectangle TrackBounds => new(Bounds.X + 12, Bounds.Bottom - 14, Bounds.Width - 24, 5);

    private void Adjust(float amount)
    {
        Set(_getValue() + amount);
    }

    private void Set(float value)
    {
        var steps = MathF.Round((value - _min) / _step);
        var stepped = _min + steps * _step;
        _setValue(Math.Clamp(stepped, _min, _max));
    }
}
