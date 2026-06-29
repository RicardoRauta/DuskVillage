using System;
using DuskVillage.Actions;
using DuskVillage.Animations;
using DuskVillage.Characters;
using DuskVillage.Core;
using DuskVillage.Input;
using DuskVillage.Needs;
using DuskVillage.Players;
using DuskVillage.Rendering;
using DuskVillage.Saving;
using DuskVillage.UI;
using DuskVillage.World;
using DuskVillage.WorldMap;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DuskVillage.Screens;

public sealed class GameplayPlaceholderScreen : GameScreenBase
{
    private readonly GameSessionSummary _session;
    private readonly VerticalMenu _menu = new();
    private readonly CharacterAnimationState _playerAnimation = new();
    private double _messageTimer;
    private double _movementAnimationTimer;
    private string _message = string.Empty;

    public GameplayPlaceholderScreen(GameScreenContext context, GameSessionSummary session)
        : base(context)
    {
        _session = session;
        _session.WorldMap = WorldMapFactory.Normalize(_session.WorldMap);
        NormalizePlayerLocation();

        _menu.Add(new ButtonControl("action.plant_seeds", () => ExecuteMapAction("action_plant_seeds")));
        _menu.Add(new ButtonControl("action.water", () => ExecuteMapAction("action_water")));
        _menu.Add(new ButtonControl("gameplay.advance_hour", AdvanceOneHour));
        _menu.Add(new ButtonControl("gameplay.sleep", SleepToNextDay));
        _menu.Add(new ButtonControl("gameplay.actions", OpenActionPreview));
        _menu.Add(new ButtonControl("gameplay.animations", OpenAnimationPreview));
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
        var moved = HandleMovement();
        HandleActionHotkeys();
        UpdatePlayerAnimation(gameTime, moved);
        _menu.Update(Context);

        if (_messageTimer > 0)
        {
            _messageTimer -= gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        var draw = BeginUi();
        DrawBackdrop(draw);
        DrawScreenTitle(draw, "gameplay.title");

        var layout = Layout();
        draw.Fill(layout.MapPanel, draw.Theme.Panel);
        draw.Border(layout.MapPanel, draw.Theme.Border);
        draw.Fill(layout.InfoPanel, draw.Theme.Panel);
        draw.Border(layout.InfoPanel, draw.Theme.Border);

        var target = TargetTile();
        var viewport = Context.WorldMapRenderer.Draw(
            draw,
            _session.WorldMap,
            _session.WorldTime.CurrentSeason,
            new Rectangle(layout.MapPanel.X + 18, layout.MapPanel.Y + 18, layout.MapPanel.Width - 36, layout.MapPanel.Height - 36),
            new Point(target.X, target.Y));

        DrawPlayer(draw, viewport);
        DrawInfoPanel(draw, layout.InfoPanel, target);
        _menu.Draw(draw);

        if (_messageTimer > 0 && !string.IsNullOrWhiteSpace(_message))
        {
            var size = Context.Font.MeasureString(_message) * UiScale * 0.82f;
            draw.Text(_message, new Vector2(layout.MapPanel.X + (layout.MapPanel.Width - size.X) / 2f, layout.MapPanel.Bottom + 10), draw.Theme.Accent, 0.82f);
        }

        EndUi();
    }

    private bool HandleMovement()
    {
        var direction = ReadMovementDirection(Context.Input.Current);
        if (!direction.HasValue)
        {
            return false;
        }

        CharacterAnimationSystem.SetMotion(_playerAnimation, CharacterAnimationIds.Idle, direction.Value);
        var result = WorldMapTargetResolver.TryMove(_session.WorldMap, _session.PlayerState.Location, direction.Value);
        if (!result.Moved)
        {
            ShowMessage(T(result.MessageKey));
            return false;
        }

        _session.PlayerState.Location = result.Location;
        _movementAnimationTimer = 0.18;
        return true;
    }

    private void HandleActionHotkeys()
    {
        var input = Context.Input.Current;
        if (input.WasKeyPressed(Context.Settings.Current.Input.Interact) || input.WasButtonPressed(Context.Settings.Current.Input.ControllerConfirm))
        {
            ExecuteMapAction("action_plant_seeds");
        }

        if (input.WasKeyPressed(Keys.R) || input.WasButtonPressed(Buttons.X))
        {
            ExecuteMapAction("action_water");
        }
    }

    private void ExecuteMapAction(string actionId)
    {
        var target = TargetTile();
        var request = new GameActionRequest
        {
            ActionId = actionId,
            ActorEntityId = _session.PlayerState.EntityId,
            FacingDirection = _playerAnimation.FacingDirection,
            Target = GameActionTarget.Tile(_session.WorldMap.AreaId, target.X, target.Y)
        };

        var result = WorldMapActionSystem.Execute(
            Context.Actions,
            request,
            _session.WorldTime,
            _session.PlayerState,
            _session.WorldMap);

        _session.WorldTime = result.ActionResult.WorldTime;
        _session.PlayerState = result.ActionResult.PlayerState;
        _session.WorldMap = result.MapState;
        CharacterAnimationSystem.SetMotion(_playerAnimation, result.ActionResult.AnimationId, result.ActionResult.FacingDirection);
        _playerAnimation.ElapsedMilliseconds = 0;
        _movementAnimationTimer = result.Succeeded ? 0.35 : 0;
        ShowMessage(T(result.MessageKey));
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
        saveGame.WorldState.Map = WorldMapFactory.Normalize(_session.WorldMap);
        saveGame.PlayerState = SavePlayerState.FromRuntimeState(_session.PlayerState);
        var slotNumber = _session.SlotNumber > 0 ? _session.SlotNumber : Context.SaveSlots.FindFirstWritableSlotNumber();
        Context.SaveSlots.SaveGame(slotNumber, saveGame);

        _session.Source = "save_slot";
        _session.SlotNumber = slotNumber;
        _session.SlotId = $"slot_{slotNumber}";
        ShowMessage(T("gameplay.saved", slotNumber));
    }

    private void OpenAnimationPreview()
    {
        Context.Navigator.Push(new CharacterAnimationPreviewScreen(Context, _session.PlayerPreset));
    }

    private void OpenActionPreview()
    {
        Context.Navigator.Push(new GameActionPreviewScreen(Context, _session));
    }

    private void ReturnToMainMenu()
    {
        Context.Navigator.SetRoot(new MainMenuScreen(Context));
    }

    private void DrawPlayer(UiDrawContext draw, WorldMapViewport viewport)
    {
        var location = _session.PlayerState.Location;
        var tile = viewport.TileBounds(location.TileX, location.TileY);
        var size = Math.Clamp(viewport.TileSize * 2, 60, 92);
        var bounds = new Rectangle(tile.Center.X - size / 2, tile.Bottom - size + 6, size, size);
        Context.CharacterSpriteRenderer.Draw(draw, _session.PlayerPreset.Appearance, _playerAnimation, bounds);
    }

    private void DrawInfoPanel(UiDrawContext draw, Rectangle panel, (int X, int Y) target)
    {
        var y = panel.Y + 18;
        DrawLine(draw, FullName(_session.PlayerPreset), panel.X + 18, ref y, draw.Theme.Accent, 1.05f);
        DrawLine(draw, T("gameplay.world_time", _session.WorldTime.Day, T("season." + _session.WorldTime.CurrentSeason), _session.WorldTime.DayOfSeason, _session.WorldTime.Year, _session.WorldTime.CurrentTime), panel.X + 18, ref y, draw.Theme.Text, 0.72f);
        DrawLine(draw, T("gameplay.money", _session.PlayerState.Money), panel.X + 18, ref y, draw.Theme.Text, 0.78f);
        DrawLine(draw, T("character.review.needs", _session.PlayerState.Needs.Energy, _session.PlayerState.Needs.Hunger, _session.PlayerState.Needs.Health, _session.PlayerState.Needs.Mood), panel.X + 18, ref y, draw.Theme.Text, 0.68f);
        DrawLine(draw, T("gameplay.location", _session.PlayerState.Location.AreaId, _session.PlayerState.Location.TileX, _session.PlayerState.Location.TileY), panel.X + 18, ref y, draw.Theme.Text, 0.72f);
        DrawLine(draw, T("world.map.target", target.X, target.Y), panel.X + 18, ref y, draw.Theme.MutedText, 0.72f);

        var tile = WorldMapRules.GetTile(_session.WorldMap, target.X, target.Y);
        if (tile != null)
        {
            DrawLine(draw, T("world.map.tile_info", T("world.tile." + tile.TypeId), T("world.tile_state." + tile.StateId)), panel.X + 18, ref y, draw.Theme.MutedText, 0.68f);
        }

        DrawLine(draw, T("world.map.controls"), panel.X + 18, ref y, draw.Theme.MutedText, 0.66f);
    }

    private void DrawLine(UiDrawContext draw, string text, int x, ref int y, Color color, float scale = 1f)
    {
        draw.Text(text, new Vector2(x, y), color, scale);
        y += (int)(26 * UiScale * scale);
    }

    private void LayoutMenu()
    {
        var layout = Layout();
        _menu.Arrange(layout.InfoPanel.X + 18, layout.InfoPanel.Y + 260, layout.InfoPanel.Width - 36, 38, 8);
    }

    private ScreenLayout Layout()
    {
        var bounds = Context.ViewBounds;
        var totalWidth = Math.Min(bounds.Width - 64, 1160);
        var mapWidth = Math.Min(760, totalWidth - 360);
        var panelHeight = Math.Min(540, bounds.Height - 154);
        var x = CenterX(bounds, totalWidth);
        var y = 112;
        return new ScreenLayout(
            new Rectangle(x, y, mapWidth, panelHeight),
            new Rectangle(x + mapWidth + 18, y, totalWidth - mapWidth - 18, panelHeight));
    }

    private (int X, int Y) TargetTile()
    {
        return WorldMapTargetResolver.ResolveAdjacentTile(_session.PlayerState.Location, _playerAnimation.FacingDirection);
    }

    private void ShowMessage(string message)
    {
        _message = message;
        _messageTimer = 2.5;
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

    private void UpdatePlayerAnimation(GameTime gameTime, bool moved)
    {
        if (moved)
        {
            CharacterAnimationSystem.SetMotion(_playerAnimation, CharacterAnimationIds.Walk, _playerAnimation.FacingDirection);
        }
        else if (_movementAnimationTimer <= 0)
        {
            CharacterAnimationSystem.SetMotion(_playerAnimation, CharacterAnimationIds.Idle, _playerAnimation.FacingDirection);
        }

        if (_movementAnimationTimer > 0)
        {
            _movementAnimationTimer -= gameTime.ElapsedGameTime.TotalSeconds;
        }

        CharacterAnimationSystem.Advance(_playerAnimation, gameTime.ElapsedGameTime);
    }

    private CharacterFacingDirection? ReadMovementDirection(InputSnapshot input)
    {
        var settings = Context.Settings.Current.Input;
        if (input.WasKeyPressed(settings.MoveUp))
        {
            return CharacterFacingDirection.Up;
        }

        if (input.WasKeyPressed(settings.MoveDown))
        {
            return CharacterFacingDirection.Down;
        }

        if (input.WasKeyPressed(settings.MoveRight))
        {
            return CharacterFacingDirection.Right;
        }

        if (input.WasKeyPressed(settings.MoveLeft))
        {
            return CharacterFacingDirection.Left;
        }

        if (input.WasButtonPressed(Buttons.DPadUp))
        {
            return CharacterFacingDirection.Up;
        }

        if (input.WasButtonPressed(Buttons.DPadDown))
        {
            return CharacterFacingDirection.Down;
        }

        if (input.WasButtonPressed(Buttons.DPadRight))
        {
            return CharacterFacingDirection.Right;
        }

        if (input.WasButtonPressed(Buttons.DPadLeft))
        {
            return CharacterFacingDirection.Left;
        }

        return null;
    }

    private void NormalizePlayerLocation()
    {
        _session.PlayerState.Location ??= new PlayerLocationState();
        _session.PlayerState.Location.AreaId = _session.WorldMap.AreaId;
        if (!WorldMapRules.IsInside(_session.WorldMap, _session.PlayerState.Location.TileX, _session.PlayerState.Location.TileY) ||
            !WorldMapRules.IsPassable(WorldMapRules.GetTile(_session.WorldMap, _session.PlayerState.Location.TileX, _session.PlayerState.Location.TileY)))
        {
            _session.PlayerState.Location.TileX = WorldMapFactory.DefaultPlayerTileX;
            _session.PlayerState.Location.TileY = WorldMapFactory.DefaultPlayerTileY;
        }
    }

    private static string FullName(CharacterPreset preset)
    {
        return string.IsNullOrWhiteSpace(preset.FamilyName)
            ? preset.Name
            : $"{preset.Name} {preset.FamilyName}";
    }

    private readonly record struct ScreenLayout(Rectangle MapPanel, Rectangle InfoPanel);
}
