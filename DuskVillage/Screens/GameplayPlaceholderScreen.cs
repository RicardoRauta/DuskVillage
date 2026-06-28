using DuskVillage.Core;
using DuskVillage.UI;
using Microsoft.Xna.Framework;

namespace DuskVillage.Screens;

public sealed class GameplayPlaceholderScreen : GameScreenBase
{
    private readonly GameSessionSummary _session;
    private readonly VerticalMenu _menu = new();

    public GameplayPlaceholderScreen(GameScreenContext context, GameSessionSummary session)
        : base(context)
    {
        _session = session;
        _menu.Add(new ButtonControl("menu.settings", () => Context.Navigator.Push(new SettingsScreen(Context))));
        _menu.Add(new ButtonControl("gameplay.return_menu", ReturnToMainMenu));
    }

    public override void Update(GameTime gameTime)
    {
        if (BackRequested())
        {
            ReturnToMainMenu();
            return;
        }

        _menu.Arrange(CenterX(Context.ViewBounds, 420), Context.ViewBounds.Bottom - 150, 420, 48, 12);
        _menu.Update(Context);
    }

    public override void Draw(GameTime gameTime)
    {
        var draw = BeginUi();
        DrawBackdrop(draw);
        DrawScreenTitle(draw, "gameplay.title");

        var panel = new Rectangle(CenterX(Context.ViewBounds, 640), 160, 640, 260);
        draw.Fill(panel, draw.Theme.Panel);
        draw.Border(panel, draw.Theme.Border);

        var y = panel.Y + 26;
        var sourceText = _session.Source == "save_slot"
            ? T("gameplay.from_save", _session.SlotId)
            : T("gameplay.from_new_game");

        DrawLine(draw, sourceText, panel.X + 28, ref y, draw.Theme.Accent);
        DrawLine(draw, T("gameplay.player", _session.PlayerName), panel.X + 28, ref y, draw.Theme.Text);
        DrawLine(draw, T("gameplay.day_time", _session.CurrentDay, _session.CurrentTime), panel.X + 28, ref y, draw.Theme.Text);

        if (_session.Source == "new_game")
        {
            DrawLine(draw, T("gameplay.age", T("age." + _session.AgeCategoryId)), panel.X + 28, ref y, draw.Theme.Text);
            DrawLine(draw, T("gameplay.origin", T("origin." + _session.OriginId)), panel.X + 28, ref y, draw.Theme.Text);
        }

        DrawLine(draw, T("gameplay.note"), panel.X + 28, ref y, draw.Theme.MutedText, 0.86f);

        _menu.Draw(draw);
        EndUi();
    }

    private void ReturnToMainMenu()
    {
        Context.Navigator.SetRoot(new MainMenuScreen(Context));
    }

    private void DrawLine(UiDrawContext draw, string text, int x, ref int y, Color color, float scale = 1f)
    {
        draw.Text(text, new Vector2(x, y), color, scale);
        y += (int)(34 * UiScale * scale);
    }
}
