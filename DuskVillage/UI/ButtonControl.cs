using System;
using DuskVillage.Localization;
using DuskVillage.Screens;
using Microsoft.Xna.Framework;

namespace DuskVillage.UI;

public sealed class ButtonControl : UiControlBase
{
    private readonly Func<ILocalizationService, string> _textProvider;
    private readonly Action _clicked;

    public ButtonControl(string labelKey, Action clicked)
        : this(localization => localization.Text(labelKey), clicked)
    {
    }

    public ButtonControl(Func<ILocalizationService, string> textProvider, Action clicked)
    {
        _textProvider = textProvider;
        _clicked = clicked;
    }

    public override void Update(GameScreenContext context, bool hasFocus)
    {
        if (!IsEnabled)
        {
            return;
        }

        if ((hasFocus && context.Input.Current.ConfirmPressedFor(context.Settings.Current.Input.ControllerConfirm)) || WasClicked(context))
        {
            _clicked();
        }
    }

    public override void Draw(UiDrawContext draw, bool hasFocus)
    {
        var fill = hasFocus && IsEnabled ? draw.Theme.PanelAlt : draw.Theme.Panel;
        draw.Fill(Bounds, fill);
        draw.Border(Bounds, hasFocus && IsEnabled ? draw.Theme.Accent : draw.Theme.Border);
        draw.CenteredText(_textProvider(draw.Localization), Bounds, TextColor(draw, hasFocus));
    }
}
