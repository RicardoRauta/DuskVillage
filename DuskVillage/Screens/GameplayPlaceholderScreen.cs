using System;
using System.Globalization;
using DuskVillage.Actions;
using DuskVillage.Animations;
using DuskVillage.Characters;
using DuskVillage.Core;
using DuskVillage.Inventory;
using DuskVillage.Input;
using DuskVillage.Items;
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
    private const double PlayerMoveSpeedTilesPerSecond = 4.5;
    private const int TerrainTileSourceSize = 16;
    private const float PlayerFeetAnchorYInCell = 44f;
    private const float GameplayHotbarScaleMultiplier = 1.45f;

    private readonly GameSessionSummary _session;
    private readonly VerticalMenu _menu = new();
    private readonly CharacterAnimationState _playerAnimation = new();
    private Vector2 _visualTilePosition;
    private WorldMapActionResult _pendingActionResult;
    private double _messageTimer;
    private double _actionAnimationTimer;
    private double _blockedMovementTimer;
    private bool _inventoryOpen;
    private int _inventoryDetailSlotIndex = -1;
    private string _message = string.Empty;

    public GameplayPlaceholderScreen(GameScreenContext context, GameSessionSummary session)
        : base(context)
    {
        _session = session;
        _session.WorldMap = WorldMapFactory.Normalize(_session.WorldMap);
        _session.PlayerState = PlayerRuntimeFactory.Normalize(_session.PlayerState);
        NormalizePlayerLocation();
        SyncVisualPosition();

        _menu.Add(new ButtonControl("action.plant_seeds", () => ExecuteMapAction("action_plant_seeds")));
        _menu.Add(new ButtonControl("action.water", () => ExecuteMapAction("action_water")));
        _menu.Add(new ButtonControl("inventory.open", ToggleInventory));
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
        if (_inventoryOpen)
        {
            HandleInventoryMouseInput();
            if (InventoryRequested() || BackRequested())
            {
                _inventoryOpen = false;
            }

            UpdateTimers(gameTime);
            CharacterAnimationSystem.SetMotion(_playerAnimation, CharacterAnimationIds.Idle, _playerAnimation.FacingDirection);
            CharacterAnimationSystem.Advance(_playerAnimation, gameTime.ElapsedGameTime);
            return;
        }

        if (BackRequested())
        {
            ReturnToMainMenu();
            return;
        }

        if (InventoryRequested())
        {
            _inventoryOpen = true;
            _inventoryDetailSlotIndex = InventorySystem.Normalize(_session.PlayerState.Inventory, Context.Items).SelectedHotbarIndex;
            return;
        }

        LayoutMenu();
        var moved = HandleMovement(gameTime);
        HandleActionHotkeys();
        UpdatePlayerAnimation(gameTime, moved);
        if (!IsActionAnimationActive())
        {
            _menu.Update(Context);
        }

        UpdateTimers(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        var draw = BeginUi();
        DrawBackdrop(draw);

        var layout = Layout();
        var target = TargetTile();
        var hotbarBounds = HotbarBounds();
        var viewport = Context.WorldMapRenderer.Draw(
            draw,
            _session.WorldMap,
            _session.WorldTime.CurrentSeason,
            Context.ViewBounds,
            new Point(target.X, target.Y),
            _visualTilePosition,
            fillViewport: true);

        DrawPlayer(draw, viewport);

        draw.Fill(layout.InfoPanel, draw.Theme.Panel);
        draw.Border(layout.InfoPanel, draw.Theme.Border);
        draw.Fill(layout.MenuPanel, draw.Theme.Panel);
        draw.Border(layout.MenuPanel, draw.Theme.Border);
        DrawInfoPanel(draw, layout.InfoPanel, target);
        _menu.Draw(draw);
        DrawHotbar(draw, hotbarBounds);
        if (_inventoryOpen)
        {
            DrawInventoryOverlay(draw);
        }

        if (_messageTimer > 0 && !string.IsNullOrWhiteSpace(_message))
        {
            var size = Context.Font.MeasureString(_message) * UiScale * 0.82f;
            draw.Text(_message, new Vector2((Context.ViewBounds.Width - size.X) / 2f, hotbarBounds.Y - 36), draw.Theme.Accent, 0.82f);
        }

        EndUi();
    }

    private bool HandleMovement(GameTime gameTime)
    {
        if (IsActionAnimationActive())
        {
            return false;
        }

        var input = ReadMovementInput(Context.Input.Current);
        if (input.LengthSquared() <= 0.0001f)
        {
            return false;
        }

        var result = WorldMapContinuousMovementSystem.Move(
            _session.WorldMap,
            _session.PlayerState.Location,
            input.X,
            input.Y,
            gameTime.ElapsedGameTime.TotalSeconds,
            PlayerMoveSpeedTilesPerSecond);

        if (result.FacingDirection.HasValue)
        {
            CharacterAnimationSystem.SetMotion(
                _playerAnimation,
                result.Moved ? CharacterAnimationIds.Walk : CharacterAnimationIds.Idle,
                result.FacingDirection.Value);
        }

        if (result.Blocked && _blockedMovementTimer <= 0)
        {
            ShowMessage(T(result.MessageKey));
            _blockedMovementTimer = 0.2;
        }

        _session.PlayerState.Location = result.Location;
        SyncVisualPosition();

        if (result.Moved)
        {
            _blockedMovementTimer = 0;
        }

        return result.Moved;
    }

    private void HandleActionHotkeys()
    {
        if (IsActionAnimationActive())
        {
            return;
        }

        var input = Context.Input.Current;
        HandleHotbarSelection(input);
        if (InventoryRequested())
        {
            _inventoryOpen = true;
            return;
        }

        if (input.WasKeyPressed(Context.Settings.Current.Input.Interact) || input.WasButtonPressed(Context.Settings.Current.Input.ControllerConfirm))
        {
            ExecuteSelectedItemAction();
        }

        if (input.WasKeyPressed(Keys.R) || input.WasButtonPressed(Buttons.X))
        {
            ExecuteMapAction("action_water");
        }
    }

    private void ExecuteMapAction(string actionId)
    {
        if (IsActionAnimationActive())
        {
            return;
        }

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

        CharacterAnimationSystem.SetMotion(_playerAnimation, result.ActionResult.AnimationId, result.ActionResult.FacingDirection);
        _playerAnimation.ElapsedMilliseconds = 0;
        if (result.Succeeded)
        {
            _pendingActionResult = result;
            _actionAnimationTimer = AnimationDurationSeconds(result.ActionResult.AnimationId, result.ActionResult.FacingDirection);
        }
        else
        {
            _pendingActionResult = null;
            _actionAnimationTimer = 0;
            ShowMessage(T(result.MessageKey));
        }
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
        var feetX = viewport.Bounds.X + _visualTilePosition.X * viewport.TileSize;
        var feetY = viewport.Bounds.Y + _visualTilePosition.Y * viewport.TileSize;
        var size = Math.Max(
            CharacterAnimationCatalog.CellSize,
            (int)Math.Round(viewport.TileSize * CharacterAnimationCatalog.CellSize / (float)TerrainTileSourceSize));
        var bounds = new Rectangle(
            (int)Math.Round(feetX - size / 2f),
            (int)Math.Round(feetY - size * PlayerFeetAnchorYInCell / CharacterAnimationCatalog.CellSize),
            size,
            size);
        Context.CharacterSpriteRenderer.Draw(draw, _session.PlayerPreset.Appearance, _playerAnimation, bounds, padding: 0);
    }

    private void DrawInfoPanel(UiDrawContext draw, Rectangle panel, (int X, int Y) target)
    {
        var y = panel.Y + 18;
        DrawLine(draw, FullName(_session.PlayerPreset), panel.X + 18, ref y, draw.Theme.Accent, 1.05f);
        DrawLine(draw, T("gameplay.world_time", _session.WorldTime.Day, T("season." + _session.WorldTime.CurrentSeason), _session.WorldTime.DayOfSeason, _session.WorldTime.Year, _session.WorldTime.CurrentTime), panel.X + 18, ref y, draw.Theme.Text, 0.72f);
        DrawLine(draw, T("gameplay.money", _session.PlayerState.Money), panel.X + 18, ref y, draw.Theme.Text, 0.78f);
        DrawLine(draw, T("character.review.needs", _session.PlayerState.Needs.Energy, _session.PlayerState.Needs.Hunger, _session.PlayerState.Needs.Health, _session.PlayerState.Needs.Mood), panel.X + 18, ref y, draw.Theme.Text, 0.68f);
        DrawLine(draw, T(
            "gameplay.location",
            _session.PlayerState.Location.AreaId,
            PositionText(_session.PlayerState.Location.GetPositionX()),
            PositionText(_session.PlayerState.Location.GetPositionY())), panel.X + 18, ref y, draw.Theme.Text, 0.72f);
        DrawLine(draw, T("world.map.target", target.X, target.Y), panel.X + 18, ref y, draw.Theme.MutedText, 0.72f);
        DrawLine(draw, T("inventory.selected", SelectedHotbarLabel()), panel.X + 18, ref y, draw.Theme.MutedText, 0.68f);

        var tile = WorldMapRules.GetTile(_session.WorldMap, target.X, target.Y);
        if (tile != null)
        {
            DrawLine(draw, T("world.map.tile_info", T("world.tile." + tile.TypeId), T("world.tile_state." + tile.StateId)), panel.X + 18, ref y, draw.Theme.MutedText, 0.68f);
        }

        DrawLine(draw, T("world.map.controls"), panel.X + 18, ref y, draw.Theme.MutedText, 0.66f);
    }

    private void DrawHotbar(UiDrawContext draw, Rectangle bounds)
    {
        Context.InventoryHotbarRenderer.Draw(draw, _session.PlayerState.Inventory, Context.Items, bounds, scaleMultiplier: GameplayHotbarScaleMultiplier);
    }

    private void DrawLine(UiDrawContext draw, string text, int x, ref int y, Color color, float scale = 1f)
    {
        draw.Text(text, new Vector2(x, y), color, scale);
        y += (int)(26 * UiScale * scale);
    }

    private void LayoutMenu()
    {
        var layout = Layout();
        _menu.Arrange(layout.MenuPanel.X + 12, layout.MenuPanel.Y + 12, layout.MenuPanel.Width - 24, 34, 6);
    }

    private ScreenLayout Layout()
    {
        var bounds = Context.ViewBounds;
        var margin = Math.Max(16, (int)(24 * UiScale));
        var availableWidth = Math.Max(220, bounds.Width - margin * 2);
        var availableHeight = Math.Max(220, bounds.Height - margin * 2);
        var infoWidth = Math.Min(380, availableWidth);
        var infoHeight = Math.Min(250, availableHeight);
        var menuWidth = Math.Min(268, availableWidth);
        var menuHeight = Math.Min(410, availableHeight);
        return new ScreenLayout(
            new Rectangle(bounds.X + margin, bounds.Y + margin, infoWidth, infoHeight),
            new Rectangle(bounds.Right - margin - menuWidth, bounds.Y + margin, menuWidth, menuHeight));
    }

    private Rectangle HotbarBounds()
    {
        var bounds = Context.ViewBounds;
        var hotbarScale = UiScale * GameplayHotbarScaleMultiplier;
        var slotSize = Context.InventoryHotbarRenderer.PreferredSlotSize(hotbarScale);
        var gap = Context.InventoryHotbarRenderer.PreferredGap(hotbarScale);
        var inventory = InventorySystem.Normalize(_session.PlayerState.Inventory, Context.Items);
        var width = inventory.HotbarSize * slotSize.X + (inventory.HotbarSize - 1) * gap;
        var y = bounds.Bottom - Math.Max(14, (int)(18 * UiScale)) - slotSize.Y;
        return new Rectangle(bounds.X + (bounds.Width - width) / 2, y, width, slotSize.Y);
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

    private void DrawInventoryOverlay(UiDrawContext draw)
    {
        var bounds = Context.ViewBounds;
        draw.Fill(bounds, new Color(0, 0, 0, 126));

        if (Context.InventoryHotbarRenderer.DrawBackpackScreen(
            draw,
            _session.PlayerState.Inventory,
            Context.Items,
            bounds,
            T("inventory.title"),
            T("inventory.close_hint"),
            _inventoryDetailSlotIndex))
        {
            return;
        }

        DrawInventoryFallbackOverlay(draw, bounds);
    }

    private void DrawInventoryFallbackOverlay(UiDrawContext draw, Rectangle bounds)
    {
        var inventory = InventorySystem.Normalize(_session.PlayerState.Inventory, Context.Items);
        var columns = inventory.HotbarSize;
        var rows = Math.Max(1, (int)Math.Ceiling(inventory.Capacity / (double)columns));
        var slotSize = Context.InventoryHotbarRenderer.PreferredSlotSize(UiScale);
        var gap = Context.InventoryHotbarRenderer.PreferredGap(UiScale);
        var padding = Math.Max(20, 24 * PixelScale());
        var headerHeight = Math.Max(58, 46 * PixelScale());
        var gridWidth = columns * slotSize.X + (columns - 1) * gap;
        var gridHeight = rows * slotSize.Y + (rows - 1) * gap;
        var panel = new Rectangle(
            bounds.X + (bounds.Width - gridWidth - padding * 2) / 2,
            bounds.Y + (bounds.Height - gridHeight - headerHeight - padding * 2) / 2,
            gridWidth + padding * 2,
            gridHeight + headerHeight + padding * 2);

        draw.Fill(panel, new Color(28, 31, 30, 244));
        draw.Border(panel, draw.Theme.Border, 2);
        draw.Text(T("inventory.title"), new Vector2(panel.X + padding, panel.Y + padding - 4), draw.Theme.Accent, 1.15f);
        draw.RightAlignedText(T("inventory.close_hint"), panel.Right - padding, panel.Y + padding + 4, draw.Theme.MutedText, 0.62f);

        var grid = new Rectangle(panel.X + padding, panel.Y + padding + headerHeight, gridWidth, gridHeight);
        Context.InventoryHotbarRenderer.DrawGrid(draw, inventory, Context.Items, grid, columns);
    }

    private int PixelScale()
    {
        return Math.Clamp((int)Math.Round(UiScale), 1, 4);
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
        if (IsActionAnimationActive())
        {
            _actionAnimationTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            CharacterAnimationSystem.Advance(_playerAnimation, gameTime.ElapsedGameTime);
            if (_actionAnimationTimer <= 0)
            {
                CompletePendingAction();
            }

            return;
        }

        if (moved)
        {
            CharacterAnimationSystem.SetMotion(_playerAnimation, CharacterAnimationIds.Walk, _playerAnimation.FacingDirection);
        }
        else if (!IsActionAnimationActive())
        {
            CharacterAnimationSystem.SetMotion(_playerAnimation, CharacterAnimationIds.Idle, _playerAnimation.FacingDirection);
        }

        CharacterAnimationSystem.Advance(_playerAnimation, gameTime.ElapsedGameTime);
    }

    private Vector2 ReadMovementInput(InputSnapshot input)
    {
        var settings = Context.Settings.Current.Input;
        var x = 0f;
        var y = 0f;

        if (input.IsKeyDown(settings.MoveLeft) || input.GamePad.IsButtonDown(settings.ControllerMoveLeft))
        {
            x -= 1f;
        }

        if (input.IsKeyDown(settings.MoveRight) || input.GamePad.IsButtonDown(settings.ControllerMoveRight))
        {
            x += 1f;
        }

        if (input.IsKeyDown(settings.MoveUp) || input.GamePad.IsButtonDown(settings.ControllerMoveUp))
        {
            y -= 1f;
        }

        if (input.IsKeyDown(settings.MoveDown) || input.GamePad.IsButtonDown(settings.ControllerMoveDown))
        {
            y += 1f;
        }

        var stick = input.GamePad.ThumbSticks.Left;
        if (Math.Abs(stick.X) > 0.25f || Math.Abs(stick.Y) > 0.25f)
        {
            x = stick.X;
            y = -stick.Y;
        }

        var movement = new Vector2(x, y);
        if (movement.LengthSquared() > 1f)
        {
            movement.Normalize();
        }

        return movement;
    }

    private void NormalizePlayerLocation()
    {
        _session.PlayerState.Location ??= new PlayerLocationState();
        _session.PlayerState.Location.AreaId = _session.WorldMap.AreaId;
        _session.PlayerState.Location.EnsurePosition();
        if (!WorldMapContinuousMovementSystem.CanStandAt(
            _session.WorldMap,
            _session.PlayerState.Location.AreaId,
            _session.PlayerState.Location.GetPositionX(),
            _session.PlayerState.Location.GetPositionY()))
        {
            _session.PlayerState.Location.SetTile(WorldMapFactory.DefaultPlayerTileX, WorldMapFactory.DefaultPlayerTileY);
        }
    }

    private void SyncVisualPosition()
    {
        _visualTilePosition = new Vector2(
            (float)_session.PlayerState.Location.GetPositionX(),
            (float)_session.PlayerState.Location.GetPositionY());
    }

    private void HandleHotbarSelection(InputSnapshot input)
    {
        for (var i = 0; i < HotbarKeys.Length; i++)
        {
            if (input.WasKeyPressed(HotbarKeys[i]))
            {
                _session.PlayerState.Inventory = InventorySystem.SelectHotbarSlot(_session.PlayerState.Inventory, i, Context.Items);
                _inventoryDetailSlotIndex = i;
                return;
            }
        }
    }

    private void HandleInventoryMouseInput()
    {
        var input = Context.Input.Current;
        if (!input.LeftClickStarted)
        {
            return;
        }

        var backpackSlot = Context.InventoryHotbarRenderer.HitBackpackSlot(
            _session.PlayerState.Inventory,
            Context.Items,
            Context.ViewBounds,
            input.MousePosition);
        if (backpackSlot >= 0)
        {
            _inventoryDetailSlotIndex = backpackSlot;
            return;
        }

        var hotbarSlot = HitHotbarSlot(input.MousePosition);
        if (hotbarSlot >= 0)
        {
            _session.PlayerState.Inventory = InventorySystem.SelectHotbarSlot(_session.PlayerState.Inventory, hotbarSlot, Context.Items);
            _inventoryDetailSlotIndex = hotbarSlot;
        }
    }

    private int HitHotbarSlot(Point pointer)
    {
        var bounds = HotbarBounds();
        var hotbarScale = UiScale * GameplayHotbarScaleMultiplier;
        var slotSize = Context.InventoryHotbarRenderer.PreferredSlotSize(hotbarScale);
        var gap = Context.InventoryHotbarRenderer.PreferredGap(hotbarScale);
        var inventory = InventorySystem.Normalize(_session.PlayerState.Inventory, Context.Items);

        for (var i = 0; i < inventory.HotbarSize; i++)
        {
            var slotBounds = new Rectangle(
                bounds.X + i * (slotSize.X + gap),
                bounds.Y + (bounds.Height - slotSize.Y) / 2,
                slotSize.X,
                slotSize.Y);
            if (slotBounds.Contains(pointer))
            {
                return i;
            }
        }

        return -1;
    }

    private bool InventoryRequested()
    {
        var input = Context.Input.Current;
        return input.WasKeyPressed(Context.Settings.Current.Input.Inventory) ||
            input.WasButtonPressed(Context.Settings.Current.Input.ControllerInventory);
    }

    private void ToggleInventory()
    {
        _inventoryOpen = !_inventoryOpen;
        if (_inventoryOpen)
        {
            _inventoryDetailSlotIndex = InventorySystem.Normalize(_session.PlayerState.Inventory, Context.Items).SelectedHotbarIndex;
        }
    }

    private void ExecuteSelectedItemAction()
    {
        var slot = InventorySystem.GetSelectedHotbarSlot(_session.PlayerState.Inventory, Context.Items);
        if (slot.IsEmpty)
        {
            ShowMessage(T("inventory.no_selected_action"));
            return;
        }

        if (slot.ItemId.Equals(ItemIds.StarterSeeds, StringComparison.OrdinalIgnoreCase))
        {
            ExecuteMapAction("action_plant_seeds");
            return;
        }

        if (slot.ItemId.Equals(ItemIds.WateringCanBasic, StringComparison.OrdinalIgnoreCase))
        {
            ExecuteMapAction("action_water");
            return;
        }

        ShowMessage(T("inventory.no_selected_action"));
    }

    private string SelectedHotbarLabel()
    {
        var slot = InventorySystem.GetSelectedHotbarSlot(_session.PlayerState.Inventory, Context.Items);
        return slot.IsEmpty ? T("inventory.empty_slot") : ItemLabel(slot.ItemId);
    }

    private string ItemLabel(string itemId)
    {
        var definition = Context.Items.Find(itemId);
        return definition == null ? itemId : T(definition.LabelKey);
    }

    private bool IsActionAnimationActive()
    {
        return _actionAnimationTimer > 0;
    }

    private void UpdateTimers(GameTime gameTime)
    {
        if (_messageTimer > 0)
        {
            _messageTimer -= gameTime.ElapsedGameTime.TotalSeconds;
        }

        if (_blockedMovementTimer > 0)
        {
            _blockedMovementTimer -= gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    private void CompletePendingAction()
    {
        _actionAnimationTimer = 0;
        if (_pendingActionResult == null)
        {
            return;
        }

        _session.WorldTime = _pendingActionResult.ActionResult.WorldTime;
        _session.PlayerState = _pendingActionResult.ActionResult.PlayerState;
        _session.WorldMap = _pendingActionResult.MapState;
        ShowMessage(T(_pendingActionResult.MessageKey));
        _pendingActionResult = null;
        NormalizePlayerLocation();
        SyncVisualPosition();
    }

    private static double AnimationDurationSeconds(string animationId, CharacterFacingDirection facingDirection)
    {
        var clip = CharacterAnimationCatalog.GetClip(animationId, facingDirection);
        return clip.DurationMilliseconds / 1000.0;
    }

    private static string PositionText(double position)
    {
        return position.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static string FullName(CharacterPreset preset)
    {
        return string.IsNullOrWhiteSpace(preset.FamilyName)
            ? preset.Name
            : $"{preset.Name} {preset.FamilyName}";
    }

    private readonly record struct ScreenLayout(Rectangle InfoPanel, Rectangle MenuPanel);

    private static readonly Keys[] HotbarKeys =
    [
        Keys.D1,
        Keys.D2,
        Keys.D3,
        Keys.D4,
        Keys.D5,
        Keys.D6,
        Keys.D7,
        Keys.D8
    ];
}
