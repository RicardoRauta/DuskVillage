using System;
using System.Collections.Generic;
using DuskVillage.Settings;
using DuskVillage.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DuskVillage.Screens;

public sealed class SettingsScreen : GameScreenBase
{
    private const int RowHeight = 50;
    private const int RowSpacing = 8;
    private readonly VerticalMenu _menu = new();
    private readonly RasterizerState _scissorRasterizerState = new() { ScissorTestEnable = true };
    private readonly string _originalLanguage;
    private SettingsTab _activeTab = SettingsTab.Display;
    private ControlsTab _activeControlsTab = ControlsTab.MouseKeyboard;
    private FocusRegion _focusRegion = FocusRegion.Content;
    private int _footerFocusIndex = 1;
    private GameSettings _draft;
    private double _savedMessageTimer;
    private int _scrollOffset;
    private int _maxScrollOffset;

    public SettingsScreen(GameScreenContext context)
        : base(context)
    {
        _draft = context.Settings.Current.Clone();
        _originalLanguage = _draft.General.LanguageCode;
        BuildControls();
    }

    public override void Update(GameTime gameTime)
    {
        var isCapturingInput = IsFocusedControlCapturingInput();
        if (!isCapturingInput && BackRequested())
        {
            Cancel();
            return;
        }

        if (!isCapturingInput)
        {
            HandleTabInput();
            HandleScrollInput();
        }

        LayoutControls();
        if (!isCapturingInput)
        {
            HandleSettingsNavigation();
        }

        if (_focusRegion != FocusRegion.ControlsTabs)
        {
            _menu.UpdateFocusedControl(Context, !isCapturingInput);
        }

        SyncFocusRegionFromFocusedIndex();
        EnsureFocusedControlVisible();
        LayoutControls();

        if (_savedMessageTimer > 0)
        {
            _savedMessageTimer -= gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        var draw = BeginUi();
        DrawBackdrop(draw);
        DrawTabs(draw);

        var contentPanel = ContentPanel;
        draw.Fill(contentPanel, draw.Theme.Panel);
        draw.Border(contentPanel, draw.Theme.Border);
        draw.Text(T(TabTitleKey(_activeTab)), new Vector2(contentPanel.X + 18, contentPanel.Y + 12), draw.Theme.Accent, 0.96f);
        if (_activeTab == SettingsTab.Controls)
        {
            DrawControlsTabs(draw);
        }

        EndUi();

        var previousScissor = Context.GraphicsDevice.ScissorRectangle;
        Context.GraphicsDevice.ScissorRectangle = ContentViewport;
        var clippedDraw = BeginUi(_scissorRasterizerState);
        DrawMenuRange(clippedDraw, 0, ContentControlCount);
        EndUi();
        Context.GraphicsDevice.ScissorRectangle = previousScissor;

        draw = BeginUi();
        DrawScrollBar(draw);
        DrawMenuRange(draw, ContentControlCount, _menu.Controls.Count);

        var hint = T("settings.tab_hint");
        draw.Text(hint, new Vector2(contentPanel.X + 18, contentPanel.Bottom - 32), draw.Theme.MutedText, 0.78f);

        if (_savedMessageTimer > 0)
        {
            var text = T("settings.saved");
            var size = Context.Font.MeasureString(text) * UiScale;
            draw.Text(text, new Vector2((Context.ViewBounds.Width - size.X) / 2f, Context.ViewBounds.Bottom - 42), draw.Theme.Accent);
        }

        EndUi();
    }

    private Rectangle ContentPanel
    {
        get
        {
            var width = Math.Min(720, Context.ViewBounds.Width - 96);
            var height = Math.Max(320, Context.ViewBounds.Height - 220);
            return new Rectangle(CenterX(Context.ViewBounds, width), 104, width, height);
        }
    }

    private Rectangle ContentViewport
    {
        get
        {
            var panel = ContentPanel;
            var topOffset = _activeTab == SettingsTab.Controls ? 110 : 56;
            return new Rectangle(panel.X + 32, panel.Y + topOffset, panel.Width - 64, Math.Max(80, panel.Height - topOffset - 48));
        }
    }

    private int ContentControlCount => Math.Max(0, _menu.Controls.Count - 3);

    private void BuildControls()
    {
        _menu.Clear();

        switch (_activeTab)
        {
            case SettingsTab.Display:
                AddDisplayControls();
                break;
            case SettingsTab.Audio:
                AddAudioControls();
                break;
            case SettingsTab.Controls:
                AddInputControls();
                break;
        }

        _menu.Add(new ButtonControl("common.defaults", RestoreDefaults));
        _menu.Add(new ButtonControl("common.apply", Apply));
        _menu.Add(new ButtonControl("common.cancel", Cancel));
    }

    private void AddDisplayControls()
    {
        _menu.Add(new SelectorControl(
            "settings.language",
            new List<SelectorOption>
            {
                new("en", "language.en"),
                new("pt-BR", "language.pt-BR")
            },
            () => _draft.General.LanguageCode,
            value =>
            {
                _draft.General.LanguageCode = value;
                Context.Localization.SetLanguage(value);
            }));

        _menu.Add(new SelectorControl(
            "settings.resolution",
            new List<SelectorOption>
            {
                new("1280x720", "1280x720"),
                new("1600x900", "1600x900"),
                new("1920x1080", "1920x1080")
            },
            () => $"{_draft.Display.Width}x{_draft.Display.Height}",
            SetResolution));

        _menu.Add(new ToggleControl(
            "settings.fullscreen",
            () => _draft.Display.Fullscreen,
            value => _draft.Display.Fullscreen = value));

        _menu.Add(new SliderControl(
            "settings.ui_scale",
            0.75f,
            2f,
            0.25f,
            () => _draft.Display.UiScale,
            value => _draft.Display.UiScale = value,
            FormatScale));
    }

    private void AddAudioControls()
    {
        _menu.Add(new SliderControl(
            "settings.master_volume",
            0f,
            1f,
            0.05f,
            () => _draft.Audio.MasterVolume,
            value => _draft.Audio.MasterVolume = value,
            FormatPercent));

        _menu.Add(new SliderControl(
            "settings.music_volume",
            0f,
            1f,
            0.05f,
            () => _draft.Audio.MusicVolume,
            value => _draft.Audio.MusicVolume = value,
            FormatPercent));

        _menu.Add(new SliderControl(
            "settings.effects_volume",
            0f,
            1f,
            0.05f,
            () => _draft.Audio.EffectsVolume,
            value => _draft.Audio.EffectsVolume = value,
            FormatPercent));
    }

    private void AddInputControls()
    {
        if (_activeControlsTab == ControlsTab.Controller)
        {
            _menu.Add(new SliderControl(
                "settings.controller_sensitivity",
                0.25f,
                2f,
                0.05f,
                () => _draft.Input.ControllerSensitivity,
                value => _draft.Input.ControllerSensitivity = value,
                FormatScale));

            _menu.Add(new GamePadBindingControl("settings.controller.confirm", () => _draft.Input.ControllerConfirm, value => _draft.Input.ControllerConfirm = value));
            _menu.Add(new GamePadBindingControl("settings.controller.back", () => _draft.Input.ControllerBack, value => _draft.Input.ControllerBack = value));
            _menu.Add(new GamePadBindingControl("settings.controller.pause", () => _draft.Input.ControllerPause, value => _draft.Input.ControllerPause = value));
            _menu.Add(new GamePadBindingControl("settings.controller.move_up", () => _draft.Input.ControllerMoveUp, value => _draft.Input.ControllerMoveUp = value));
            _menu.Add(new GamePadBindingControl("settings.controller.move_down", () => _draft.Input.ControllerMoveDown, value => _draft.Input.ControllerMoveDown = value));
            _menu.Add(new GamePadBindingControl("settings.controller.move_left", () => _draft.Input.ControllerMoveLeft, value => _draft.Input.ControllerMoveLeft = value));
            _menu.Add(new GamePadBindingControl("settings.controller.move_right", () => _draft.Input.ControllerMoveRight, value => _draft.Input.ControllerMoveRight = value));
            return;
        }

        _menu.Add(new SliderControl(
            "settings.mouse_sensitivity",
            0.25f,
            2f,
            0.05f,
            () => _draft.Input.MouseSensitivity,
            value => _draft.Input.MouseSensitivity = value,
            FormatScale));

        _menu.Add(new KeyBindingControl("settings.key.move_up", () => _draft.Input.MoveUp, value => _draft.Input.MoveUp = value));
        _menu.Add(new KeyBindingControl("settings.key.move_down", () => _draft.Input.MoveDown, value => _draft.Input.MoveDown = value));
        _menu.Add(new KeyBindingControl("settings.key.move_left", () => _draft.Input.MoveLeft, value => _draft.Input.MoveLeft = value));
        _menu.Add(new KeyBindingControl("settings.key.move_right", () => _draft.Input.MoveRight, value => _draft.Input.MoveRight = value));
        _menu.Add(new KeyBindingControl("settings.key.interact", () => _draft.Input.Interact, value => _draft.Input.Interact = value));
        _menu.Add(new KeyBindingControl("settings.key.back", () => _draft.Input.Back, value => _draft.Input.Back = value));
        _menu.Add(new KeyBindingControl("settings.key.pause", () => _draft.Input.Pause, value => _draft.Input.Pause = value));
    }

    private void LayoutControls()
    {
        var controls = _menu.Controls;
        var viewport = ContentViewport;
        var controlX = viewport.X + 8;
        var controlY = viewport.Y - _scrollOffset;
        var controlWidth = viewport.Width - 32;

        var contentCount = ContentControlCount;
        var contentHeight = contentCount > 0 ? contentCount * (RowHeight + RowSpacing) - RowSpacing : 0;
        _maxScrollOffset = Math.Max(0, contentHeight - viewport.Height);
        _scrollOffset = Math.Clamp(_scrollOffset, 0, _maxScrollOffset);

        for (var i = 0; i < contentCount; i++)
        {
            controls[i].Bounds = new Rectangle(controlX, controlY + i * (RowHeight + RowSpacing), controlWidth, RowHeight);
            controls[i].PointerClipBounds = viewport;
        }

        var buttonY = Context.ViewBounds.Bottom - 86;
        var buttonWidth = 190;
        var buttonGap = 38;
        var startX = CenterX(Context.ViewBounds, buttonWidth * 3 + buttonGap * 2);
        SetButton(contentCount, startX, buttonY, buttonWidth);
        SetButton(contentCount + 1, startX + buttonWidth + buttonGap, buttonY, buttonWidth);
        SetButton(contentCount + 2, startX + (buttonWidth + buttonGap) * 2, buttonY, buttonWidth);

        if (_focusRegion == FocusRegion.Footer)
        {
            _menu.SetFocusedIndex(contentCount + _footerFocusIndex);
        }

        void SetButton(int index, int x, int y, int width)
        {
            if (index < controls.Count)
            {
                controls[index].Bounds = new Rectangle(x, y, width, 48);
                controls[index].PointerClipBounds = null;
            }
        }
    }

    private void HandleSettingsNavigation()
    {
        var contentCount = ContentControlCount;
        var input = Context.Input.Current;
        var settings = Context.Settings.Current.Input;
        var upPressed = input.MenuUpPressedFor(settings.ControllerMoveUp);
        var downPressed = input.MenuDownPressedFor(settings.ControllerMoveDown);
        var leftPressed = input.MenuLeftPressedFor(settings.ControllerMoveLeft);
        var rightPressed = input.MenuRightPressedFor(settings.ControllerMoveRight);

        if (contentCount == 0)
        {
            _focusRegion = FocusRegion.Footer;
        }

        if (_focusRegion == FocusRegion.ControlsTabs)
        {
            if (_activeTab != SettingsTab.Controls)
            {
                _focusRegion = FocusRegion.Content;
                return;
            }

            if (leftPressed)
            {
                ChangeControlsTab(-1);
                return;
            }

            if (rightPressed)
            {
                ChangeControlsTab(1);
                return;
            }

            if (downPressed || upPressed)
            {
                _focusRegion = FocusRegion.Content;
                _menu.SetFocusedIndex(0);
            }

            return;
        }

        if (_focusRegion == FocusRegion.Content)
        {
            if (_activeTab == SettingsTab.Controls && upPressed && _menu.FocusedIndex <= 0)
            {
                _focusRegion = FocusRegion.ControlsTabs;
                return;
            }

            if (downPressed && _menu.FocusedIndex >= contentCount - 1)
            {
                _focusRegion = FocusRegion.Footer;
                _footerFocusIndex = 1;
                _menu.SetFocusedIndex(contentCount + _footerFocusIndex);
                return;
            }

            if (upPressed && _menu.FocusedIndex <= 0)
            {
                _focusRegion = FocusRegion.Footer;
                _footerFocusIndex = 1;
                _menu.SetFocusedIndex(contentCount + _footerFocusIndex);
                return;
            }

            if (downPressed)
            {
                _menu.SetFocusedIndex(Math.Min(contentCount - 1, _menu.FocusedIndex + 1));
                return;
            }

            if (upPressed)
            {
                _menu.SetFocusedIndex(Math.Max(0, _menu.FocusedIndex - 1));
                return;
            }
        }
        else
        {
            if (leftPressed)
            {
                _footerFocusIndex = Math.Max(0, _footerFocusIndex - 1);
                _menu.SetFocusedIndex(contentCount + _footerFocusIndex);
                return;
            }

            if (rightPressed)
            {
                _footerFocusIndex = Math.Min(2, _footerFocusIndex + 1);
                _menu.SetFocusedIndex(contentCount + _footerFocusIndex);
                return;
            }

            if (upPressed || downPressed)
            {
                _focusRegion = FocusRegion.Content;
                _menu.SetFocusedIndex(upPressed ? Math.Max(0, contentCount - 1) : 0);
            }
        }
    }

    private bool IsFocusedControlCapturingInput()
    {
        var focusedIndex = _menu.FocusedIndex;
        if (focusedIndex < 0 || focusedIndex >= _menu.Controls.Count)
        {
            return false;
        }

        return _menu.Controls[focusedIndex] is IInputCaptureControl { IsCapturingInput: true };
    }

    private void SyncFocusRegionFromFocusedIndex()
    {
        if (_focusRegion == FocusRegion.ControlsTabs)
        {
            return;
        }

        var contentCount = ContentControlCount;
        if (_menu.FocusedIndex >= contentCount)
        {
            _focusRegion = FocusRegion.Footer;
            _footerFocusIndex = Math.Clamp(_menu.FocusedIndex - contentCount, 0, 2);
            return;
        }

        _focusRegion = FocusRegion.Content;
    }

    private void HandleScrollInput()
    {
        var viewport = ContentViewport;
        if (Context.Input.Current.ScrollDelta != 0 && viewport.Contains(Context.Input.Current.MousePosition))
        {
            _scrollOffset -= Math.Sign(Context.Input.Current.ScrollDelta) * 48;
        }

        if (Context.Input.Current.IsKeyDown(Keys.LeftControl) || Context.Input.Current.IsKeyDown(Keys.RightControl))
        {
            if (Context.Input.Current.MenuUpPressed)
            {
                _scrollOffset -= 48;
            }

            if (Context.Input.Current.MenuDownPressed)
            {
                _scrollOffset += 48;
            }
        }

        _scrollOffset = Math.Clamp(_scrollOffset, 0, _maxScrollOffset);
    }

    private void EnsureFocusedControlVisible()
    {
        var focusedIndex = _menu.FocusedIndex;
        if (focusedIndex < 0 || focusedIndex >= ContentControlCount)
        {
            return;
        }

        var viewport = ContentViewport;
        var focusedTop = focusedIndex * (RowHeight + RowSpacing);
        var focusedBottom = focusedTop + RowHeight;

        if (focusedTop < _scrollOffset)
        {
            _scrollOffset = focusedTop;
        }
        else if (focusedBottom > _scrollOffset + viewport.Height)
        {
            _scrollOffset = focusedBottom - viewport.Height;
        }

        _scrollOffset = Math.Clamp(_scrollOffset, 0, _maxScrollOffset);
    }

    private void DrawMenuRange(UiDrawContext draw, int startIndex, int endIndex)
    {
        var controls = _menu.Controls;
        var max = Math.Min(endIndex, controls.Count);
        for (var i = startIndex; i < max; i++)
        {
            controls[i].Draw(draw, i == _menu.FocusedIndex);
        }
    }

    private void DrawScrollBar(UiDrawContext draw)
    {
        if (_maxScrollOffset <= 0)
        {
            return;
        }

        var viewport = ContentViewport;
        var track = new Rectangle(viewport.Right - 10, viewport.Y, 6, viewport.Height);
        draw.Fill(track, draw.Theme.BackgroundTop);

        var thumbHeight = Math.Max(28, (int)(viewport.Height * (viewport.Height / (float)(viewport.Height + _maxScrollOffset))));
        var travel = viewport.Height - thumbHeight;
        var thumbY = track.Y + (travel <= 0 ? 0 : (int)(travel * (_scrollOffset / (float)_maxScrollOffset)));
        draw.Fill(new Rectangle(track.X, thumbY, track.Width, thumbHeight), draw.Theme.AccentMuted);
    }

    private void HandleTabInput()
    {
        if (Context.Input.Current.PreviousTabPressed)
        {
            ChangeTab(-1);
            return;
        }

        if (Context.Input.Current.NextTabPressed)
        {
            ChangeTab(1);
            return;
        }

        if (!Context.Input.Current.LeftClickStarted)
        {
            return;
        }

        foreach (var tab in Enum.GetValues<SettingsTab>())
        {
            if (TabBounds(tab).Contains(Context.Input.Current.MousePosition))
            {
                SetTab(tab);
                return;
            }
        }

        if (_activeTab == SettingsTab.Controls)
        {
            foreach (var tab in Enum.GetValues<ControlsTab>())
            {
                if (ControlsTabBounds(tab).Contains(Context.Input.Current.MousePosition))
                {
                    SetControlsTab(tab);
                    return;
                }
            }
        }
    }

    private void DrawTabs(UiDrawContext draw)
    {
        foreach (var tab in Enum.GetValues<SettingsTab>())
        {
            var bounds = TabBounds(tab);
            var isActive = tab == _activeTab;
            draw.Fill(bounds, isActive ? draw.Theme.PanelAlt : draw.Theme.Panel);
            draw.Border(bounds, isActive ? draw.Theme.Accent : draw.Theme.Border);
            draw.CenteredText(T(TabTitleKey(tab)), bounds, isActive ? draw.Theme.Accent : draw.Theme.Text, 0.92f);
        }
    }

    private void DrawControlsTabs(UiDrawContext draw)
    {
        foreach (var tab in Enum.GetValues<ControlsTab>())
        {
            var bounds = ControlsTabBounds(tab);
            var isActive = tab == _activeControlsTab;
            var hasFocus = _focusRegion == FocusRegion.ControlsTabs && isActive;
            draw.Fill(bounds, isActive || hasFocus ? draw.Theme.PanelAlt : draw.Theme.BackgroundTop);
            draw.Border(bounds, isActive || hasFocus ? draw.Theme.Accent : draw.Theme.Border);
            draw.CenteredText(T(ControlsTabTitleKey(tab)), bounds, isActive || hasFocus ? draw.Theme.Accent : draw.Theme.Text, 0.82f);
        }
    }

    private Rectangle TabBounds(SettingsTab tab)
    {
        const int tabWidth = 190;
        const int tabHeight = 44;
        const int gap = 12;

        var startX = CenterX(Context.ViewBounds, tabWidth * 3 + gap * 2);
        var index = (int)tab;
        return new Rectangle(startX + index * (tabWidth + gap), 34, tabWidth, tabHeight);
    }

    private Rectangle ControlsTabBounds(ControlsTab tab)
    {
        const int tabWidth = 260;
        const int tabHeight = 38;
        const int gap = 12;

        var panel = ContentPanel;
        var startX = panel.X + 40;
        var index = (int)tab;
        return new Rectangle(startX + index * (tabWidth + gap), panel.Y + 54, tabWidth, tabHeight);
    }

    private void ChangeTab(int direction)
    {
        var tabCount = Enum.GetValues<SettingsTab>().Length;
        var next = ((int)_activeTab + direction) % tabCount;
        if (next < 0)
        {
            next = tabCount - 1;
        }

        SetTab((SettingsTab)next);
    }

    private void SetTab(SettingsTab tab)
    {
        if (_activeTab == tab)
        {
            return;
        }

        _activeTab = tab;
        _focusRegion = FocusRegion.Content;
        _scrollOffset = 0;
        BuildControls();
    }

    private void SetControlsTab(ControlsTab tab)
    {
        if (_activeControlsTab == tab)
        {
            return;
        }

        _activeControlsTab = tab;
        _focusRegion = FocusRegion.Content;
        _scrollOffset = 0;
        BuildControls();
    }

    private void ChangeControlsTab(int direction)
    {
        var tabCount = Enum.GetValues<ControlsTab>().Length;
        var next = ((int)_activeControlsTab + direction) % tabCount;
        if (next < 0)
        {
            next = tabCount - 1;
        }

        _activeControlsTab = (ControlsTab)next;
        _focusRegion = FocusRegion.ControlsTabs;
        _scrollOffset = 0;
        BuildControls();
    }

    private void Apply()
    {
        Context.Settings.Save(_draft);
        Context.ApplySettings(Context.Settings.Current);
        _draft = Context.Settings.Current.Clone();
        _savedMessageTimer = 2.5;
        BuildControls();
    }

    private void Cancel()
    {
        Context.Localization.SetLanguage(_originalLanguage);
        Context.Navigator.Back();
    }

    private void RestoreDefaults()
    {
        _draft = GameSettings.CreateDefault();
        Context.Localization.SetLanguage(_draft.General.LanguageCode);
        _scrollOffset = 0;
        BuildControls();
    }

    private void SetResolution(string value)
    {
        var parts = value.Split('x', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return;
        }

        if (int.TryParse(parts[0], out var width) && int.TryParse(parts[1], out var height))
        {
            _draft.Display.Width = width;
            _draft.Display.Height = height;
        }
    }

    private static string FormatPercent(float value)
    {
        return $"{MathF.Round(value * 100f):0}%";
    }

    private static string FormatScale(float value)
    {
        return $"{value:0.00}x";
    }

    private static string TabTitleKey(SettingsTab tab)
    {
        return tab switch
        {
            SettingsTab.Audio => "settings.audio",
            SettingsTab.Controls => "settings.controls",
            _ => "settings.display"
        };
    }

    private static string ControlsTabTitleKey(ControlsTab tab)
    {
        return tab switch
        {
            ControlsTab.Controller => "settings.controls.controller",
            _ => "settings.controls.mouse_keyboard"
        };
    }

    private enum SettingsTab
    {
        Display,
        Audio,
        Controls
    }

    private enum ControlsTab
    {
        MouseKeyboard,
        Controller
    }

    private enum FocusRegion
    {
        ControlsTabs,
        Content,
        Footer
    }
}
