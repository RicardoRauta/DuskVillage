using DuskVillage.UI;
using Microsoft.Xna.Framework;

namespace DuskVillage.Screens;

public sealed class MainMenuScreen : GameScreenBase
{
    private readonly VerticalMenu _menu = new();

    public MainMenuScreen(GameScreenContext context)
        : base(context)
    {
        _menu.Add(new ButtonControl("menu.start_game", () => Context.Navigator.Push(new NewGameScreen(Context))));
        _menu.Add(new ButtonControl("menu.load_game", () => Context.Navigator.Push(new LoadGameScreen(Context))));
        _menu.Add(new ButtonControl("menu.settings", () => Context.Navigator.Push(new SettingsScreen(Context))));
        _menu.Add(new ButtonControl("menu.exit", Context.ExitGame));
    }

    public override void Update(GameTime gameTime)
    {
        const int buttonWidth = 360;
        const int buttonHeight = 52;
        const int buttonSpacing = 14;
        const int buttonCount = 4;

        var menuHeight = buttonCount * buttonHeight + (buttonCount - 1) * buttonSpacing;
        var menuY = Context.ViewBounds.Y + (Context.ViewBounds.Height - menuHeight) / 2;
        _menu.Arrange(CenterX(Context.ViewBounds, buttonWidth), menuY, buttonWidth, buttonHeight, buttonSpacing);
        _menu.Update(Context);
    }

    public override void Draw(GameTime gameTime)
    {
        var draw = BeginUi();
        DrawBackdrop(draw);
        DrawScreenTitle(draw, "app.title", "app.subtitle");

        _menu.Draw(draw);

        var footer = T("menu.footer");
        var footerSize = Context.Font.MeasureString(footer) * UiScale * 0.85f;
        draw.Text(
            footer,
            new Vector2((Context.ViewBounds.Width - footerSize.X) / 2f, Context.ViewBounds.Bottom - 54),
            draw.Theme.MutedText,
            0.85f);

        EndUi();
    }
}
