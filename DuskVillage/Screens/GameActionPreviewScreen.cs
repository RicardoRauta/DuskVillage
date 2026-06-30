using System;
using System.Collections.Generic;
using System.Linq;
using DuskVillage.Actions;
using DuskVillage.Animations;
using DuskVillage.Characters;
using DuskVillage.Core;
using DuskVillage.UI;
using Microsoft.Xna.Framework;

namespace DuskVillage.Screens;

public sealed class GameActionPreviewScreen : GameScreenBase
{
    private const string DirectionDown = "down";
    private const string DirectionUp = "up";
    private const string DirectionRight = "right";
    private const string DirectionLeft = "left";

    private static readonly IReadOnlyList<SelectorOption> DirectionOptions =
    [
        new(DirectionDown, "animation.direction.down"),
        new(DirectionUp, "animation.direction.up"),
        new(DirectionRight, "animation.direction.right"),
        new(DirectionLeft, "animation.direction.left")
    ];

    private readonly GameSessionSummary _session;
    private readonly CharacterAnimationState _animation = new();
    private readonly VerticalMenu _menu = new();
    private readonly IReadOnlyList<SelectorOption> _actionOptions;
    private string _actionId = string.Empty;
    private CharacterFacingDirection _facingDirection = CharacterFacingDirection.Down;
    private string _messageKey = "action.preview.ready";
    private bool _isPlaying = true;

    public GameActionPreviewScreen(GameScreenContext context, GameSessionSummary session)
        : base(context)
    {
        _session = session;
        _actionOptions = BuildActionOptions(context.Actions);
        _actionId = _actionOptions.Count > 0 ? _actionOptions[0].Value : string.Empty;

        _menu.Add(new SelectorControl("action.preview.action", _actionOptions, () => _actionId, SetActionId));
        _menu.Add(new SelectorControl("action.preview.direction", DirectionOptions, () => DirectionId(_facingDirection), SetDirection));
        _menu.Add(new ButtonControl("action.preview.execute", ExecuteAction));
        _menu.Add(new ButtonControl(localization => localization.Text(_isPlaying ? "animation.preview.pause" : "animation.preview.play"), TogglePlayback));
        _menu.Add(new ButtonControl("common.back", () => Context.Navigator.Back()));
    }

    public override void Update(GameTime gameTime)
    {
        if (BackRequested())
        {
            Context.Navigator.Back();
            return;
        }

        LayoutMenu();
        _menu.Update(Context);

        if (_isPlaying)
        {
            CharacterAnimationSystem.Advance(_animation, gameTime.ElapsedGameTime);
        }
    }

    public override void Draw(GameTime gameTime)
    {
        var draw = BeginUi();
        DrawBackdrop(draw);
        DrawScreenTitle(draw, "action.preview.title");

        var panel = new Rectangle(CenterX(Context.ViewBounds, 1040), 118, 1040, 480);
        draw.Fill(panel, draw.Theme.Panel);
        draw.Border(panel, draw.Theme.Border);

        var preview = new Rectangle(panel.X + 34, panel.Y + 36, 286, 286);
        draw.Fill(preview, draw.Theme.BackgroundTop);
        draw.Border(preview, draw.Theme.Border);
        Context.CharacterSpriteRenderer.Draw(draw, _session.PlayerPreset.Appearance, _animation, preview);

        var infoX = panel.X + 34;
        var infoY = preview.Bottom + 16;
        DrawInfo(draw, FullName(_session.PlayerPreset), infoX, ref infoY, draw.Theme.Accent);
        DrawInfo(draw, T("gameplay.world_time", _session.WorldTime.Day, T("season." + _session.WorldTime.CurrentSeason), _session.WorldTime.DayOfSeason, _session.WorldTime.Year, _session.WorldTime.CurrentTime), infoX, ref infoY, draw.Theme.Text, 0.78f);
        DrawInfo(draw, T("character.review.needs", _session.PlayerState.Needs.Energy, _session.PlayerState.Needs.Hunger, _session.PlayerState.Needs.Health, _session.PlayerState.Needs.Mood), infoX, ref infoY, draw.Theme.Text, 0.78f);
        DrawInfo(draw, T("gameplay.money", _session.PlayerState.Money), infoX, ref infoY, draw.Theme.Text, 0.78f);
        DrawInfo(draw, T(_messageKey), infoX, ref infoY, draw.Theme.MutedText, 0.74f);

        var detailsX = panel.X + 364;
        var detailsY = panel.Y + 34;
        var definition = Context.Actions.Find(_actionId);
        if (definition != null)
        {
            DrawInfo(draw, T(definition.LabelKey), detailsX, ref detailsY, draw.Theme.Accent, 1.05f);
            DrawWrappedDescription(draw, T(definition.DescriptionKey), new Rectangle(detailsX, detailsY + 6, 600, 76));
            detailsY += 100;
            DrawInfo(draw, T("action.preview.target", T("action.target." + definition.TargetKind)), detailsX, ref detailsY, draw.Theme.Text, 0.82f);
            DrawInfo(draw, T("action.preview.animation", T("animation." + definition.AnimationId)), detailsX, ref detailsY, draw.Theme.Text, 0.82f);
            DrawInfo(draw, T("action.preview.time_cost", definition.TimeCostMinutes), detailsX, ref detailsY, draw.Theme.Text, 0.82f);
            DrawInfo(draw, T("action.preview.effects", EffectSummary(definition)), detailsX, ref detailsY, draw.Theme.Text, 0.82f);
        }
        else
        {
            DrawInfo(draw, T("action.preview.no_actions"), detailsX, ref detailsY, draw.Theme.Warning);
        }

        _menu.Draw(draw);
        EndUi();
    }

    private void ExecuteAction()
    {
        var request = new GameActionRequest
        {
            ActionId = _actionId,
            ActorEntityId = _session.PlayerState.EntityId,
            FacingDirection = _facingDirection,
            Target = CreateTarget()
        };

        var result = GameActionSystem.Execute(Context.Actions, request, _session.WorldTime, _session.PlayerState);
        _session.WorldTime = result.WorldTime;
        _session.PlayerState = result.PlayerState;
        _messageKey = result.MessageKey;

        CharacterAnimationSystem.SetMotion(_animation, result.AnimationId, result.FacingDirection);
        _animation.ElapsedMilliseconds = 0;
        _isPlaying = true;
    }

    private GameActionTarget CreateTarget()
    {
        var definition = Context.Actions.Find(_actionId);
        if (definition == null)
        {
            return GameActionTarget.None();
        }

        if (definition.TargetKind.Equals(GameActionTargetKinds.Tile, StringComparison.OrdinalIgnoreCase))
        {
            var location = _session.PlayerState.Location;
            var targetX = location.TileX;
            var targetY = location.TileY;
            switch (_facingDirection)
            {
                case CharacterFacingDirection.Up:
                    targetY--;
                    break;
                case CharacterFacingDirection.Right:
                    targetX++;
                    break;
                case CharacterFacingDirection.Left:
                    targetX--;
                    break;
                default:
                    targetY++;
                    break;
            }

            return GameActionTarget.Tile(location.AreaId, targetX, targetY);
        }

        if (definition.TargetKind.Equals(GameActionTargetKinds.Self, StringComparison.OrdinalIgnoreCase))
        {
            return GameActionTarget.Self(_session.PlayerState.EntityId);
        }

        return GameActionTarget.None();
    }

    private void LayoutMenu()
    {
        var menuWidth = 600;
        var menuX = CenterX(Context.ViewBounds, 1040) + 364;
        var menuY = 372;
        _menu.Arrange(menuX, menuY, menuWidth, 46, 10);
    }

    private void SetActionId(string actionId)
    {
        _actionId = actionId;
    }

    private void SetDirection(string directionId)
    {
        _facingDirection = ParseDirection(directionId);
    }

    private void TogglePlayback()
    {
        _isPlaying = !_isPlaying;
    }

    private void DrawInfo(UiDrawContext draw, string text, int x, ref int y, Color color, float scale = 1f)
    {
        draw.Text(text, new Vector2(x, y), color, scale);
        y += (int)(25 * UiScale * scale);
    }

    private void DrawWrappedDescription(UiDrawContext draw, string text, Rectangle bounds)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var line = string.Empty;
        var y = bounds.Y;
        foreach (var word in words)
        {
            var candidate = string.IsNullOrWhiteSpace(line) ? word : $"{line} {word}";
            var width = Context.Font.MeasureString(candidate).X * UiScale * 0.76f;
            if (width > bounds.Width && !string.IsNullOrWhiteSpace(line))
            {
                draw.Text(line, new Vector2(bounds.X, y), draw.Theme.MutedText, 0.76f);
                y += (int)(22 * UiScale);
                line = word;
                continue;
            }

            line = candidate;
        }

        if (!string.IsNullOrWhiteSpace(line) && y < bounds.Bottom)
        {
            draw.Text(line, new Vector2(bounds.X, y), draw.Theme.MutedText, 0.76f);
        }
    }

    private string EffectSummary(GameActionDefinition definition)
    {
        if (definition.Effects.Count == 0)
        {
            return T("common.empty");
        }

        return string.Join(", ", definition.Effects.Select(EffectText));
    }

    private string EffectText(GameActionEffectDefinition effect)
    {
        if (effect.Type.Equals(GameActionEffectTypes.ChangeNeed, StringComparison.OrdinalIgnoreCase))
        {
            var sign = effect.Amount > 0 ? "+" : string.Empty;
            return $"{T("character.need." + effect.NeedId)} {sign}{effect.Amount}";
        }

        if (effect.Type.Equals(GameActionEffectTypes.AddMoney, StringComparison.OrdinalIgnoreCase))
        {
            var sign = effect.Amount > 0 ? "+" : string.Empty;
            return $"{T("gameplay.money", sign + effect.Amount)}";
        }

        if (effect.Type.Equals(GameActionEffectTypes.PlantCrop, StringComparison.OrdinalIgnoreCase))
        {
            return T("action.effect.plantCrop");
        }

        if (effect.Type.Equals(GameActionEffectTypes.WaterTile, StringComparison.OrdinalIgnoreCase))
        {
            return T("action.effect.waterTile");
        }

        if (effect.Type.Equals(GameActionEffectTypes.SetTileState, StringComparison.OrdinalIgnoreCase))
        {
            return T("action.effect.setTileState");
        }

        if (effect.Type.Equals(GameActionEffectTypes.RequireItem, StringComparison.OrdinalIgnoreCase))
        {
            return T("action.effect.requireItem", ItemLabel(effect.ItemId), Math.Max(1, effect.Amount));
        }

        if (effect.Type.Equals(GameActionEffectTypes.ConsumeItem, StringComparison.OrdinalIgnoreCase))
        {
            return T("action.effect.consumeItem", ItemLabel(effect.ItemId), Math.Max(1, effect.Amount));
        }

        return T("action.effect." + effect.Type);
    }

    private string ItemLabel(string itemId)
    {
        var definition = Context.Items.Find(itemId);
        return definition == null ? itemId : T(definition.LabelKey);
    }

    private static IReadOnlyList<SelectorOption> BuildActionOptions(GameActionRegistry registry)
    {
        var definitions = registry?.Definitions ?? Array.Empty<GameActionDefinition>();
        var options = definitions
            .Select(definition => new SelectorOption(definition.Id, definition.LabelKey))
            .ToList();

        if (options.Count == 0)
        {
            options.Add(new SelectorOption(string.Empty, "action.preview.no_actions"));
        }

        return options;
    }

    private static string DirectionId(CharacterFacingDirection direction)
    {
        return direction switch
        {
            CharacterFacingDirection.Up => DirectionUp,
            CharacterFacingDirection.Right => DirectionRight,
            CharacterFacingDirection.Left => DirectionLeft,
            _ => DirectionDown
        };
    }

    private static CharacterFacingDirection ParseDirection(string directionId)
    {
        return directionId switch
        {
            DirectionUp => CharacterFacingDirection.Up,
            DirectionRight => CharacterFacingDirection.Right,
            DirectionLeft => CharacterFacingDirection.Left,
            _ => CharacterFacingDirection.Down
        };
    }

    private static string FullName(CharacterPreset preset)
    {
        return string.IsNullOrWhiteSpace(preset.FamilyName)
            ? preset.Name
            : $"{preset.Name} {preset.FamilyName}";
    }
}
