using System;
using System.Collections.Generic;
using DuskVillage.Animations;
using DuskVillage.Characters;
using DuskVillage.UI;
using Microsoft.Xna.Framework;

namespace DuskVillage.Screens;

public sealed class CharacterAnimationPreviewScreen : GameScreenBase
{
    private const string DirectionDown = "down";
    private const string DirectionUp = "up";
    private const string DirectionRight = "right";
    private const string DirectionLeft = "left";

    private static readonly IReadOnlyList<SelectorOption> AnimationOptions =
    [
        new(CharacterAnimationIds.Walk, "animation.walk"),
        new(CharacterAnimationIds.Idle, "animation.idle")
    ];

    private static readonly IReadOnlyList<SelectorOption> DirectionOptions =
    [
        new(DirectionDown, "animation.direction.down"),
        new(DirectionUp, "animation.direction.up"),
        new(DirectionRight, "animation.direction.right"),
        new(DirectionLeft, "animation.direction.left")
    ];

    private readonly CharacterPreset _preset;
    private readonly CharacterAnimationState _animation = new()
    {
        AnimationId = CharacterAnimationIds.Walk,
        FacingDirection = CharacterFacingDirection.Down
    };

    private readonly VerticalMenu _menu = new();
    private string _animationId = CharacterAnimationIds.Walk;
    private CharacterFacingDirection _facingDirection = CharacterFacingDirection.Down;
    private bool _isPlaying = true;

    public CharacterAnimationPreviewScreen(GameScreenContext context, CharacterPreset preset)
        : base(context)
    {
        _preset = preset;
        _menu.Add(new SelectorControl("animation.preview.clip", AnimationOptions, () => _animationId, SetAnimationId));
        _menu.Add(new SelectorControl("animation.preview.direction", DirectionOptions, () => DirectionId(_facingDirection), SetDirection));
        _menu.Add(new ButtonControl(localization => localization.Text(_isPlaying ? "animation.preview.pause" : "animation.preview.play"), TogglePlayback));
        _menu.Add(new ButtonControl("animation.preview.reset", ResetAnimation));
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
        DrawScreenTitle(draw, "animation.preview.title");

        var panel = new Rectangle(CenterX(Context.ViewBounds, 980), 122, 980, 462);
        draw.Fill(panel, draw.Theme.Panel);
        draw.Border(panel, draw.Theme.Border);

        var preview = new Rectangle(panel.X + 38, panel.Y + 38, 316, 316);
        draw.Fill(preview, draw.Theme.BackgroundTop);
        draw.Border(preview, draw.Theme.Border);
        Context.CharacterSpriteRenderer.Draw(draw, _preset.Appearance, _animation, preview);

        var infoX = preview.X;
        var infoY = preview.Bottom + 18;
        var frame = CharacterAnimationSystem.GetCurrentFrame(_animation);
        var clip = CharacterAnimationCatalog.GetClip(_animation.AnimationId, _animation.FacingDirection);

        DrawInfo(draw, FullName(_preset), infoX, ref infoY, draw.Theme.Accent);
        DrawInfo(draw, T("animation.preview.frame", frame.CellIndex), infoX, ref infoY, draw.Theme.Text, 0.86f);
        DrawInfo(draw, T("animation.preview.duration", frame.DurationMilliseconds), infoX, ref infoY, draw.Theme.Text, 0.86f);
        DrawInfo(draw, T("animation.preview.flipx", T(frame.FlipX ? "common.on" : "common.off")), infoX, ref infoY, draw.Theme.Text, 0.86f);
        DrawInfo(draw, T("animation.preview.timeline", _animation.ElapsedMilliseconds, clip.DurationMilliseconds), infoX, ref infoY, draw.Theme.MutedText, 0.78f);

        if (!Context.CharacterAssets.IsAvailable)
        {
            draw.CenteredText(T("new_game.sprite_zip_missing"), new Rectangle(preview.X + 10, preview.Bottom - 44, preview.Width - 20, 34), draw.Theme.Warning, 0.72f);
        }

        _menu.Draw(draw);
        EndUi();
    }

    private void LayoutMenu()
    {
        var menuWidth = 520;
        var menuX = CenterX(Context.ViewBounds, 980) + 420;
        var menuY = 166;
        _menu.Arrange(menuX, menuY, menuWidth, 52, 12);
    }

    private void SetAnimationId(string animationId)
    {
        _animationId = animationId;
        CharacterAnimationSystem.SetMotion(_animation, _animationId, _facingDirection);
    }

    private void SetDirection(string directionId)
    {
        _facingDirection = ParseDirection(directionId);
        CharacterAnimationSystem.SetMotion(_animation, _animationId, _facingDirection);
    }

    private void TogglePlayback()
    {
        _isPlaying = !_isPlaying;
    }

    private void ResetAnimation()
    {
        _animation.ElapsedMilliseconds = 0;
    }

    private void DrawInfo(UiDrawContext draw, string text, int x, ref int y, Color color, float scale = 1f)
    {
        draw.Text(text, new Vector2(x, y), color, scale);
        y += (int)(25 * UiScale * scale);
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
