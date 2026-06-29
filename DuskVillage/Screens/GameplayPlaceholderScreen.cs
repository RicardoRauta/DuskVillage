using System;
using DuskVillage.Animations;
using DuskVillage.Characters;
using DuskVillage.Core;
using DuskVillage.Input;
using DuskVillage.Needs;
using DuskVillage.Saving;
using DuskVillage.UI;
using DuskVillage.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DuskVillage.Screens;

public sealed class GameplayPlaceholderScreen : GameScreenBase
{
    private readonly GameSessionSummary _session;
    private readonly VerticalMenu _menu = new();
    private readonly CharacterAnimationState _playerAnimation = new();
    private double _savedMessageTimer;
    private string _savedMessage = string.Empty;

    public GameplayPlaceholderScreen(GameScreenContext context, GameSessionSummary session)
        : base(context)
    {
        _session = session;
        _menu.Add(new ButtonControl("gameplay.advance_hour", AdvanceOneHour));
        _menu.Add(new ButtonControl("gameplay.sleep", SleepToNextDay));
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

        LayoutMenu();
        UpdatePlayerAnimation(gameTime);
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

        var panel = new Rectangle(CenterX(Context.ViewBounds, 820), 124, 820, 410);
        draw.Fill(panel, draw.Theme.Panel);
        draw.Border(panel, draw.Theme.Border);

        var preview = new Rectangle(panel.Right - 230, panel.Y + 34, 176, 176);
        draw.Fill(preview, draw.Theme.BackgroundTop);
        draw.Border(preview, draw.Theme.Border);
        Context.CharacterSpriteRenderer.Draw(draw, _session.PlayerPreset.Appearance, _playerAnimation, preview);

        var y = panel.Y + 26;
        var sourceText = _session.Source == "save_slot"
            ? T("gameplay.from_save", _session.SlotId)
            : T("gameplay.from_new_game");

        DrawLine(draw, sourceText, panel.X + 28, ref y, draw.Theme.Accent);
        DrawLine(draw, T("gameplay.player", FullName(_session.PlayerPreset)), panel.X + 28, ref y, draw.Theme.Text);
        DrawLine(draw, T("gameplay.world_time", _session.WorldTime.Day, T("season." + _session.WorldTime.CurrentSeason), _session.WorldTime.DayOfSeason, _session.WorldTime.Year, _session.WorldTime.CurrentTime), panel.X + 28, ref y, draw.Theme.Text);
        DrawLine(draw, T("gameplay.age", T("age." + _session.PlayerPreset.AgeCategoryId)), panel.X + 28, ref y, draw.Theme.Text);
        DrawLine(draw, T("gameplay.origin", T("origin." + _session.PlayerPreset.OriginId)), panel.X + 28, ref y, draw.Theme.Text);
        DrawLine(draw, T("gameplay.money", _session.PlayerState.Money), panel.X + 28, ref y, draw.Theme.Text, 0.86f);
        DrawLine(draw, T("gameplay.location", _session.PlayerState.Location.AreaId, _session.PlayerState.Location.TileX, _session.PlayerState.Location.TileY), panel.X + 28, ref y, draw.Theme.Text, 0.86f);
        DrawLine(draw, T("character.birthday", T("season." + _session.PlayerPreset.BirthdaySeasonId), _session.PlayerPreset.BirthdayDay), panel.X + 28, ref y, draw.Theme.Text, 0.86f);
        DrawLine(draw, T("character.motivation", T(CharacterOptionCatalog.FindMotivation(_session.PlayerPreset.MotivationId).LabelKey)), panel.X + 28, ref y, draw.Theme.Text, 0.86f);
        DrawLine(draw, T("character.attribute.points", _session.PlayerPreset.Attributes.Total, CharacterPresetValidator.AttributePointBudget), panel.X + 28, ref y, draw.Theme.Text, 0.86f);
        DrawLine(draw, T("character.review.needs", _session.PlayerState.Needs.Energy, _session.PlayerState.Needs.Hunger, _session.PlayerState.Needs.Health, _session.PlayerState.Needs.Mood), panel.X + 28, ref y, draw.Theme.Text, 0.86f);
        DrawLine(draw, T("gameplay.note"), panel.X + 28, ref y, draw.Theme.MutedText, 0.8f);

        _menu.Draw(draw);

        if (_savedMessageTimer > 0)
        {
            var size = Context.Font.MeasureString(_savedMessage) * UiScale * 0.86f;
            draw.Text(_savedMessage, new Vector2((Context.ViewBounds.Width - size.X) / 2f, Context.ViewBounds.Bottom - 42), draw.Theme.Accent, 0.86f);
        }

        EndUi();
    }

    private void AdvanceOneHour()
    {
        var clockResult = WorldClock.Advance(_session.WorldTime, 60);
        _session.WorldTime = clockResult.Time;

        var needsResult = NeedsSystem.ApplyElapsedTime(_session.PlayerState.Needs, 60);
        if (clockResult.ForcedDayEnd)
        {
            needsResult = NeedsSystem.ApplySleep(needsResult.Needs);
        }

        _session.PlayerState.Needs = needsResult.Needs;
        ShowMessage(NeedsMessage(clockResult.ForcedDayEnd, needsResult));
    }

    private void SleepToNextDay()
    {
        var clockResult = WorldClock.SleepToNextDay(_session.WorldTime);
        var needsResult = NeedsSystem.ApplySleep(_session.PlayerState.Needs);
        _session.WorldTime = clockResult.Time;
        _session.PlayerState.Needs = needsResult.Needs;
        ShowMessage(T("gameplay.slept"));
    }

    private void SaveCurrentGame()
    {
        var saveGame = SaveGame.CreateNew(_session.PlayerPreset);
        saveGame.WorldState = SaveWorldState.FromWorldTime(_session.WorldTime);
        saveGame.PlayerState = SavePlayerState.FromRuntimeState(_session.PlayerState);
        var slotNumber = _session.SlotNumber > 0 ? _session.SlotNumber : Context.SaveSlots.FindFirstWritableSlotNumber();
        Context.SaveSlots.SaveGame(slotNumber, saveGame);

        _session.Source = "save_slot";
        _session.SlotNumber = slotNumber;
        _session.SlotId = $"slot_{slotNumber}";
        ShowMessage(T("gameplay.saved", slotNumber));
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

    private void LayoutMenu()
    {
        var controls = _menu.Controls;
        var buttonCount = controls.Count;
        var footerWidth = Math.Min(Context.ViewBounds.Width - 80, 940);
        var buttonGap = 14;
        var buttonWidth = (footerWidth - buttonGap * (buttonCount - 1)) / buttonCount;
        var startX = CenterX(Context.ViewBounds, footerWidth);
        var buttonY = Context.ViewBounds.Bottom - 82;

        for (var i = 0; i < buttonCount; i++)
        {
            controls[i].Bounds = new Rectangle(startX + i * (buttonWidth + buttonGap), buttonY, buttonWidth, 48);
            controls[i].PointerClipBounds = null;
        }
    }

    private void ShowMessage(string message)
    {
        _savedMessage = message;
        _savedMessageTimer = 2.5;
    }

    private string NeedsMessage(bool forcedDayEnd, NeedsSimulationResult result)
    {
        if (forcedDayEnd)
        {
            return T("gameplay.forced_day_end");
        }

        if (result.IsStarving)
        {
            return T("gameplay.needs_starving");
        }

        if (result.IsExhausted)
        {
            return T("gameplay.needs_exhausted");
        }

        if (result.IsHungry)
        {
            return T("gameplay.needs_hungry");
        }

        return T("gameplay.time_advanced");
    }

    private static string FullName(CharacterPreset preset)
    {
        return string.IsNullOrWhiteSpace(preset.FamilyName)
            ? preset.Name
            : $"{preset.Name} {preset.FamilyName}";
    }

    private void UpdatePlayerAnimation(GameTime gameTime)
    {
        var movementDirection = ReadMovementDirection(Context.Input.Current);
        var isMoving = movementDirection.HasValue;
        var facingDirection = movementDirection ?? _playerAnimation.FacingDirection;
        var animationId = isMoving ? CharacterAnimationIds.Walk : CharacterAnimationIds.Idle;

        CharacterAnimationSystem.SetMotion(_playerAnimation, animationId, facingDirection);
        CharacterAnimationSystem.Advance(_playerAnimation, gameTime.ElapsedGameTime);
    }

    private static CharacterFacingDirection? ReadMovementDirection(InputSnapshot input)
    {
        if (input.IsKeyDown(Keys.W))
        {
            return CharacterFacingDirection.Up;
        }

        if (input.IsKeyDown(Keys.S))
        {
            return CharacterFacingDirection.Down;
        }

        if (input.IsKeyDown(Keys.D))
        {
            return CharacterFacingDirection.Right;
        }

        if (input.IsKeyDown(Keys.A))
        {
            return CharacterFacingDirection.Left;
        }

        var thumbstick = input.GamePad.ThumbSticks.Left;
        if (Math.Abs(thumbstick.X) > Math.Abs(thumbstick.Y) && Math.Abs(thumbstick.X) > 0.35f)
        {
            return thumbstick.X > 0 ? CharacterFacingDirection.Right : CharacterFacingDirection.Left;
        }

        if (Math.Abs(thumbstick.Y) > 0.35f)
        {
            return thumbstick.Y > 0 ? CharacterFacingDirection.Up : CharacterFacingDirection.Down;
        }

        return null;
    }
}
