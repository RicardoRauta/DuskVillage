using System;
using DuskVillage.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DuskVillage.UI;

public sealed class TextInputControl : UiControlBase
{
    private readonly string _labelKey;
    private readonly Func<string> _getText;
    private readonly Action<string> _setText;
    private readonly int _maxLength;

    public TextInputControl(string labelKey, Func<string> getText, Action<string> setText, int maxLength)
    {
        _labelKey = labelKey;
        _getText = getText;
        _setText = setText;
        _maxLength = maxLength;
    }

    public override void Update(GameScreenContext context, bool hasFocus)
    {
        if (!IsEnabled || !hasFocus)
        {
            return;
        }

        var text = _getText();
        if (context.Input.Current.WasKeyPressed(Keys.Back) && text.Length > 0)
        {
            text = text[..^1];
        }

        foreach (var character in context.Input.Current.TypedCharacters())
        {
            if (text.Length < _maxLength)
            {
                text += character;
            }
        }

        _setText(text);
    }

    public override void Draw(UiDrawContext draw, bool hasFocus)
    {
        draw.Fill(Bounds, hasFocus ? draw.Theme.PanelAlt : draw.Theme.Panel);
        draw.Border(Bounds, hasFocus ? draw.Theme.Accent : draw.Theme.Border);

        var label = draw.Localization.Text(_labelKey);
        var value = _getText();
        if (hasFocus)
        {
            value += "_";
        }

        draw.Text(label, new Vector2(Bounds.X + 12, Bounds.Y + 11), TextColor(draw, hasFocus));
        draw.Text(value, new Vector2(Bounds.X + 150, Bounds.Y + 11), TextColor(draw, hasFocus));
    }
}
