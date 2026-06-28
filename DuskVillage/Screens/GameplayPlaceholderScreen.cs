using DuskVillage.Characters;
using DuskVillage.Core;
using DuskVillage.Saving;
using DuskVillage.UI;
using Microsoft.Xna.Framework;

namespace DuskVillage.Screens;

public sealed class GameplayPlaceholderScreen : GameScreenBase
{
    private readonly GameSessionSummary _session;
    private readonly VerticalMenu _menu = new();
    private double _savedMessageTimer;
    private string _savedMessage = string.Empty;

    public GameplayPlaceholderScreen(GameScreenContext context, GameSessionSummary session)
        : base(context)
    {
        _session = session;
        _menu.Add(new ButtonControl("gameplay.save", SaveCurrentGame));
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

        _menu.Arrange(CenterX(Context.ViewBounds, 420), Context.ViewBounds.Bottom - 194, 420, 48, 12);
        _menu.Update(Context);

        if (_savedMessageTimer > 0)
        {
            _savedMessageTimer -= gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        var draw = BeginUi();
        DrawBackdrop(draw);
        DrawScreenTitle(draw, "gameplay.title");

        var panel = new Rectangle(CenterX(Context.ViewBounds, 760), 142, 760, 330);
        draw.Fill(panel, draw.Theme.Panel);
        draw.Border(panel, draw.Theme.Border);

        var preview = new Rectangle(panel.Right - 230, panel.Y + 34, 176, 176);
        draw.Fill(preview, draw.Theme.BackgroundTop);
        draw.Border(preview, draw.Theme.Border);
        Context.CharacterPortraitRenderer.Draw(draw, _session.PlayerPreset.Appearance, preview);

        var y = panel.Y + 26;
        var sourceText = _session.Source == "save_slot"
            ? T("gameplay.from_save", _session.SlotId)
            : T("gameplay.from_new_game");

        DrawLine(draw, sourceText, panel.X + 28, ref y, draw.Theme.Accent);
        DrawLine(draw, T("gameplay.player", FullName(_session.PlayerPreset)), panel.X + 28, ref y, draw.Theme.Text);
        DrawLine(draw, T("gameplay.day_time", _session.CurrentDay, _session.CurrentTime), panel.X + 28, ref y, draw.Theme.Text);
        DrawLine(draw, T("gameplay.age", T("age." + _session.PlayerPreset.AgeCategoryId)), panel.X + 28, ref y, draw.Theme.Text);
        DrawLine(draw, T("gameplay.origin", T("origin." + _session.PlayerPreset.OriginId)), panel.X + 28, ref y, draw.Theme.Text);
        DrawLine(draw, T("character.birthday", T("season." + _session.PlayerPreset.BirthdaySeasonId), _session.PlayerPreset.BirthdayDay), panel.X + 28, ref y, draw.Theme.Text, 0.86f);
        DrawLine(draw, T("character.motivation", T(CharacterOptionCatalog.FindMotivation(_session.PlayerPreset.MotivationId).LabelKey)), panel.X + 28, ref y, draw.Theme.Text, 0.86f);
        DrawLine(draw, T("character.attribute.points", _session.PlayerPreset.Attributes.Total, CharacterPresetValidator.AttributePointBudget), panel.X + 28, ref y, draw.Theme.Text, 0.86f);
        DrawLine(draw, T("character.review.needs", _session.PlayerPreset.Needs.Energy, _session.PlayerPreset.Needs.Hunger, _session.PlayerPreset.Needs.Health, _session.PlayerPreset.Needs.Mood), panel.X + 28, ref y, draw.Theme.Text, 0.86f);
        DrawLine(draw, T("gameplay.note"), panel.X + 28, ref y, draw.Theme.MutedText, 0.8f);

        _menu.Draw(draw);

        if (_savedMessageTimer > 0)
        {
            var size = Context.Font.MeasureString(_savedMessage) * UiScale * 0.86f;
            draw.Text(_savedMessage, new Vector2((Context.ViewBounds.Width - size.X) / 2f, Context.ViewBounds.Bottom - 42), draw.Theme.Accent, 0.86f);
        }

        EndUi();
    }

    private void SaveCurrentGame()
    {
        var saveGame = SaveGame.CreateNew(_session.PlayerPreset);
        saveGame.WorldState.Day = _session.CurrentDay;
        saveGame.WorldState.TimeMinutes = ParseTime(_session.CurrentTime);
        var slotNumber = _session.SlotNumber > 0 ? _session.SlotNumber : Context.SaveSlots.FindFirstWritableSlotNumber();
        Context.SaveSlots.SaveGame(slotNumber, saveGame);

        _session.Source = "save_slot";
        _session.SlotNumber = slotNumber;
        _session.SlotId = $"slot_{slotNumber}";
        _savedMessage = T("gameplay.saved", slotNumber);
        _savedMessageTimer = 2.5;
    }

    private void ReturnToMainMenu()
    {
        Context.Navigator.SetRoot(new MainMenuScreen(Context));
    }

    private void DrawLine(UiDrawContext draw, string text, int x, ref int y, Color color, float scale = 1f)
    {
        draw.Text(text, new Vector2(x, y), color, scale);
        y += (int)(30 * UiScale * scale);
    }

    private static int ParseTime(string currentTime)
    {
        var parts = currentTime.Split(':');
        if (parts.Length == 2 && int.TryParse(parts[0], out var hours) && int.TryParse(parts[1], out var minutes))
        {
            return hours * 60 + minutes;
        }

        return 360;
    }

    private static string FullName(CharacterPreset preset)
    {
        return string.IsNullOrWhiteSpace(preset.FamilyName)
            ? preset.Name
            : $"{preset.Name} {preset.FamilyName}";
    }
}
