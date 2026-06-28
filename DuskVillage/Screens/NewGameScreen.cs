using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DuskVillage.CharacterAssets;
using DuskVillage.Characters;
using DuskVillage.Core;
using DuskVillage.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WinForms = System.Windows.Forms;

namespace DuskVillage.Screens;

public sealed class NewGameScreen : GameScreenBase
{
    private const int RowHeight = 50;
    private const int RowSpacing = 8;
    private const int FooterButtonCount = 5;
    private const int PreviewInsetX = 26;
    private const int PreviewInsetY = 58;
    private const int PreviewToContentGap = 22;
    private const int ContentRightPadding = 32;

    private readonly NewGameOptions _options = new();
    private readonly VerticalMenu _menu = new();
    private readonly RasterizerState _scissorRasterizerState = new() { ScissorTestEnable = true };
    private readonly List<AppearanceControlEntry> _appearanceControlEntries = new();
    private readonly List<AppearanceGroupHeader> _appearanceGroupHeaders = new();

    private CharacterCreationTab _activeTab = CharacterCreationTab.Identity;
    private FocusRegion _focusRegion = FocusRegion.Content;
    private int _footerFocusIndex = 3;
    private int _scrollOffset;
    private int _maxScrollOffset;
    private string _message = string.Empty;
    private bool _messageIsWarning;
    private double _messageTimer;
    private string _tooltipText = string.Empty;
    private Point _tooltipAnchor;
    private readonly Random _random = new();

    public NewGameScreen(GameScreenContext context)
        : base(context)
    {
        BuildControls();
    }

    private CharacterPreset Preset => _options.CharacterPreset;

    private Rectangle ContentPanel
    {
        get
        {
            var width = Math.Min(1120, Context.ViewBounds.Width - 80);
            var height = Math.Max(420, Context.ViewBounds.Height - 210);
            return new Rectangle(CenterX(Context.ViewBounds, width), 104, width, height);
        }
    }

    private bool CompactLayout => ContentPanel.Width < 900;

    private Rectangle PreviewPanel
    {
        get
        {
            var panel = ContentPanel;
            var size = PreviewPanelSize;
            return new Rectangle(panel.X + PreviewInsetX, panel.Y + PreviewInsetY, size, size);
        }
    }

    private int PreviewPanelSize
    {
        get
        {
            if (CompactLayout)
            {
                return _activeTab == CharacterCreationTab.Appearance ? 156 : 188;
            }

            return _activeTab == CharacterCreationTab.Appearance ? 208 : 256;
        }
    }

    private Rectangle ContentViewport
    {
        get
        {
            var panel = ContentPanel;
            if (_activeTab == CharacterCreationTab.Appearance)
            {
                var top = PreviewPanel.Bottom + 22;
                return new Rectangle(panel.X + 32, top, panel.Width - 64, Math.Max(80, panel.Bottom - top - 54));
            }

            if (CompactLayout)
            {
                var preview = PreviewPanel;
                return new Rectangle(panel.X + 32, preview.Bottom + 18, panel.Width - 64, Math.Max(80, panel.Bottom - preview.Bottom - 76));
            }

            var left = PreviewPanel.Right + PreviewToContentGap;
            return new Rectangle(left, panel.Y + 62, Math.Max(80, panel.Right - left - ContentRightPadding), Math.Max(80, panel.Height - 120));
        }
    }

    private int ContentControlCount => Math.Max(0, _menu.Controls.Count - FooterButtonCount);

    public override void Update(GameTime gameTime)
    {
        if (BackRequested())
        {
            Context.Navigator.Back();
            return;
        }

        HandleTabInput();
        HandleScrollInput();
        LayoutControls();
        HandleCreationNavigation();
        _menu.UpdateFocusedControl(Context);
        SyncFocusRegionFromFocusedIndex();
        EnsureFocusedControlVisible();
        LayoutControls();
        UpdateTooltip();

        if (_messageTimer > 0)
        {
            _messageTimer -= gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        var draw = BeginUi();
        DrawBackdrop(draw);
        DrawTabs(draw);
        DrawPanel(draw);
        EndUi();

        var previousScissor = Context.GraphicsDevice.ScissorRectangle;
        Context.GraphicsDevice.ScissorRectangle = ContentViewport;
        var clippedDraw = BeginUi(_scissorRasterizerState);
        DrawAppearanceGroupHeaders(clippedDraw);
        DrawMenuRange(clippedDraw, 0, ContentControlCount);
        EndUi();
        Context.GraphicsDevice.ScissorRectangle = previousScissor;

        draw = BeginUi();
        DrawScrollBar(draw);
        DrawMenuRange(draw, ContentControlCount, _menu.Controls.Count);
        DrawTooltip(draw);
        DrawMessage(draw);
        EndUi();
    }

    private void BuildControls()
    {
        _menu.Clear();
        _appearanceControlEntries.Clear();
        _appearanceGroupHeaders.Clear();

        switch (_activeTab)
        {
            case CharacterCreationTab.Appearance:
                AddAppearanceControls();
                break;
            case CharacterCreationTab.Status:
                AddStatusControls();
                break;
            case CharacterCreationTab.Review:
                break;
            default:
                AddIdentityControls();
                break;
        }

        _menu.Add(new ButtonControl("new_game.import_preset", ImportPreset));
        _menu.Add(new ButtonControl("new_game.export_preset", ExportPreset));
        _menu.Add(new ButtonControl("common.defaults", RestoreDefaults));
        _menu.Add(new ButtonControl("new_game.begin", BeginNewGame));
        _menu.Add(new ButtonControl("common.back", Context.Navigator.Back));

        _focusRegion = ContentControlCount == 0 ? FocusRegion.Footer : FocusRegion.Content;
        _scrollOffset = 0;
    }

    private void AddIdentityControls()
    {
        _menu.Add(new TextInputControl(
            "new_game.player_name",
            () => Preset.Name,
            value => Preset.Name = value,
            18));

        _menu.Add(new TextInputControl(
            "new_game.family_name",
            () => Preset.FamilyName,
            value => Preset.FamilyName = value,
            18));

        _menu.Add(new SelectorControl(
            "new_game.age",
            CharacterOptionCatalog.AgeCategories.Select(option => new SelectorOption(option.Value, option.LabelKey)).ToList(),
            () => Preset.AgeCategoryId,
            SetAgeCategory));

        _menu.Add(new SelectorControl(
            "new_game.origin",
            CharacterOptionCatalog.Origins.Select(option => new SelectorOption(option.Value, option.LabelKey)).ToList(),
            () => Preset.OriginId,
            SetOrigin));

        _menu.Add(new SelectorControl(
            "new_game.birthday_season",
            CharacterOptionCatalog.BirthdaySeasons.Select(option => new SelectorOption(option.Value, option.LabelKey)).ToList(),
            () => Preset.BirthdaySeasonId,
            value => Preset.BirthdaySeasonId = value));

        _menu.Add(new SliderControl(
            "new_game.birthday_day",
            CharacterPresetValidator.BirthdayDayMinimum,
            CharacterPresetValidator.BirthdayDayMaximum,
            1,
            () => Preset.BirthdayDay,
            value => Preset.BirthdayDay = (int)MathF.Round(value),
            value => $"{MathF.Round(value):0}"));

        _menu.Add(new SelectorControl(
            "new_game.motivation",
            CharacterOptionCatalog.Motivations.Select(option => new SelectorOption(option.Value, option.LabelKey)).ToList(),
            () => Preset.MotivationId,
            value => Preset.MotivationId = value));
    }

    private void AddAppearanceControls()
    {
        AddAppearanceColorOnly("new_game.appearance.group.character", CharacterAppearanceSlotIds.Body);
        AddAppearanceSlotPair("new_game.appearance.group.character", CharacterAppearanceSlotIds.Hair);

        AddAppearanceSlotPair("new_game.appearance.group.clothes", CharacterAppearanceSlotIds.Shirt);
        AddAppearanceSlotPair("new_game.appearance.group.clothes", CharacterAppearanceSlotIds.LowerOne);
        AddAppearanceSlotPair("new_game.appearance.group.clothes", CharacterAppearanceSlotIds.FootwearLow);

        AddAppearanceSlotPair("new_game.appearance.group.layers", CharacterAppearanceSlotIds.Socks);
        AddAppearanceSlotPair("new_game.appearance.group.layers", CharacterAppearanceSlotIds.LowerTwo);
        AddAppearanceSlotPair("new_game.appearance.group.layers", CharacterAppearanceSlotIds.FootwearHigh);
        AddAppearanceSlotPair("new_game.appearance.group.layers", CharacterAppearanceSlotIds.LowerThree);

        AddAppearanceSlotPair("new_game.appearance.group.accessories", CharacterAppearanceSlotIds.Hands);
        AddAppearanceSlotPair("new_game.appearance.group.accessories", CharacterAppearanceSlotIds.Outer);
        AddAppearanceSlotPair("new_game.appearance.group.accessories", CharacterAppearanceSlotIds.Neck);
        AddAppearanceSlotPair("new_game.appearance.group.accessories", CharacterAppearanceSlotIds.Face);
        AddAppearanceSlotPair("new_game.appearance.group.accessories", CharacterAppearanceSlotIds.Head);
    }

    private void AddStatusControls()
    {
        foreach (var attributeId in CharacterAttributeIds.All)
        {
            var id = attributeId;
            _menu.Add(new AttributeStepperControl(
                CharacterPresetValidator.AttributeLabelKey(id),
                AttributeTooltipKey(id),
                CharacterPresetValidator.AttributeMinimum,
                CharacterPresetValidator.AttributeMaximum,
                () => Preset.Attributes.GetValue(id),
                value => SetAttribute(id, value),
                () => CharacterAttributePointBuy.CanDecrease(Preset.Attributes, id),
                () => CharacterAttributePointBuy.CanIncrease(Preset.Attributes, id)));
        }

        _menu.Add(new ButtonControl("new_game.randomize_attributes", RandomizeAttributes));
        CharacterPresetFactory.EnsureDefaultSkills(Preset);
    }

    private void LayoutControls()
    {
        var controls = _menu.Controls;
        var viewport = ContentViewport;
        var contentCount = ContentControlCount;

        if (_activeTab == CharacterCreationTab.Appearance)
        {
            LayoutAppearanceControls(controls, viewport, contentCount);
            LayoutFooterControls(controls, contentCount);
            return;
        }

        var controlX = viewport.X + 8;
        var controlY = viewport.Y - _scrollOffset;
        var controlWidth = viewport.Width - 32;

        var contentHeight = contentCount > 0 ? contentCount * (RowHeight + RowSpacing) - RowSpacing : 0;
        _maxScrollOffset = Math.Max(0, contentHeight - viewport.Height);
        _scrollOffset = Math.Clamp(_scrollOffset, 0, _maxScrollOffset);

        for (var i = 0; i < contentCount; i++)
        {
            controls[i].Bounds = new Rectangle(controlX, controlY + i * (RowHeight + RowSpacing), controlWidth, RowHeight);
            controls[i].PointerClipBounds = viewport;
        }

        LayoutFooterControls(controls, contentCount);

        if (_focusRegion == FocusRegion.Footer)
        {
            _menu.SetFocusedIndex(contentCount + _footerFocusIndex);
        }
    }

    private void LayoutAppearanceControls(IReadOnlyList<IUiControl> controls, Rectangle viewport, int contentCount)
    {
        _appearanceGroupHeaders.Clear();

        var contentHeight = MeasureAppearanceContentHeight(viewport.Width);
        _maxScrollOffset = Math.Max(0, contentHeight - viewport.Height);
        _scrollOffset = Math.Clamp(_scrollOffset, 0, _maxScrollOffset);

        var x = viewport.X + 8;
        var y = viewport.Y - _scrollOffset;
        var width = viewport.Width - 32;
        var twoColumns = width >= 620;
        var columnGap = 14;
        var columnWidth = twoColumns ? (width - columnGap) / 2 : width;

        foreach (var groupKey in AppearanceGroupOrder)
        {
            var groupEntries = _appearanceControlEntries.Where(entry => entry.GroupKey == groupKey).ToList();
            if (groupEntries.Count == 0)
            {
                continue;
            }

            _appearanceGroupHeaders.Add(new AppearanceGroupHeader(groupKey, new Rectangle(x, y, width, 24)));
            y += 32;

            foreach (var entry in groupEntries)
            {
                if (twoColumns)
                {
                    if (entry.ItemControlIndex >= 0)
                    {
                        controls[entry.ItemControlIndex].Bounds = new Rectangle(x, y, columnWidth, RowHeight);
                        controls[entry.ItemControlIndex].PointerClipBounds = viewport;
                        controls[entry.ColorControlIndex].Bounds = new Rectangle(x + columnWidth + columnGap, y, columnWidth, RowHeight);
                    }
                    else
                    {
                        controls[entry.ColorControlIndex].Bounds = new Rectangle(x, y, width, RowHeight);
                    }

                    controls[entry.ColorControlIndex].PointerClipBounds = viewport;
                    y += RowHeight + RowSpacing;
                    continue;
                }

                if (entry.ItemControlIndex >= 0)
                {
                    controls[entry.ItemControlIndex].Bounds = new Rectangle(x, y, width, RowHeight);
                    controls[entry.ItemControlIndex].PointerClipBounds = viewport;
                    y += RowHeight + RowSpacing;
                }

                controls[entry.ColorControlIndex].Bounds = new Rectangle(x, y, width, RowHeight);
                controls[entry.ColorControlIndex].PointerClipBounds = viewport;
                y += RowHeight + RowSpacing;
            }

            y += 12;
        }

        if (_focusRegion == FocusRegion.Footer)
        {
            _menu.SetFocusedIndex(contentCount + _footerFocusIndex);
        }
    }

    private int MeasureAppearanceContentHeight(int viewportWidth)
    {
        var width = viewportWidth - 32;
        var twoColumns = width >= 620;
        var height = 0;

        foreach (var groupKey in AppearanceGroupOrder)
        {
            var groupEntries = _appearanceControlEntries.Where(entry => entry.GroupKey == groupKey).ToList();
            if (groupEntries.Count == 0)
            {
                continue;
            }

            height += 32;
            foreach (var entry in groupEntries)
            {
                height += twoColumns || entry.ItemControlIndex < 0
                    ? RowHeight + RowSpacing
                    : RowHeight * 2 + RowSpacing * 2;
            }

            height += 12;
        }

        return Math.Max(0, height - RowSpacing);
    }

    private void LayoutFooterControls(IReadOnlyList<IUiControl> controls, int contentCount)
    {
        var footerWidth = Math.Min(Context.ViewBounds.Width - 80, 930);
        var buttonGap = 14;
        var buttonWidth = (footerWidth - buttonGap * (FooterButtonCount - 1)) / FooterButtonCount;
        var startX = CenterX(Context.ViewBounds, footerWidth);
        var buttonY = Context.ViewBounds.Bottom - 82;

        for (var i = 0; i < FooterButtonCount; i++)
        {
            var index = contentCount + i;
            if (index >= controls.Count)
            {
                continue;
            }

            controls[index].Bounds = new Rectangle(startX + i * (buttonWidth + buttonGap), buttonY, buttonWidth, 48);
            controls[index].PointerClipBounds = null;
        }
    }

    private void DrawTabs(UiDrawContext draw)
    {
        foreach (var tab in Enum.GetValues<CharacterCreationTab>())
        {
            var bounds = TabBounds(tab);
            var isActive = tab == _activeTab;
            draw.Fill(bounds, isActive ? draw.Theme.PanelAlt : draw.Theme.Panel);
            draw.Border(bounds, isActive ? draw.Theme.Accent : draw.Theme.Border);
            draw.CenteredText(T(TabTitleKey(tab)), bounds, isActive ? draw.Theme.Accent : draw.Theme.Text, 0.88f);
        }
    }

    private void DrawPanel(UiDrawContext draw)
    {
        var panel = ContentPanel;
        draw.Fill(panel, draw.Theme.Panel);
        draw.Border(panel, draw.Theme.Border);
        draw.Text(T(TabTitleKey(_activeTab)), new Vector2(panel.X + 18, panel.Y + 14), draw.Theme.Accent, 0.96f);

        var preview = PreviewPanel;
        draw.Fill(preview, draw.Theme.BackgroundTop);
        draw.Border(preview, draw.Theme.Border);
        Context.CharacterPortraitRenderer.Draw(draw, Preset.Appearance, preview);

        if (!Context.CharacterAssets.IsAvailable)
        {
            draw.CenteredText(T("new_game.sprite_zip_missing"), new Rectangle(preview.X + 8, preview.Bottom - 44, preview.Width - 16, 34), draw.Theme.Warning, 0.68f);
        }

        if (_activeTab == CharacterCreationTab.Appearance)
        {
            draw.Text(
                T("new_game.appearance.summary"),
                new Vector2(preview.Right + 20, preview.Y + 8),
                draw.Theme.MutedText,
                0.78f);
        }

        if (_activeTab == CharacterCreationTab.Identity)
        {
            DrawIdentitySummary(draw);
        }

        if (_activeTab == CharacterCreationTab.Status)
        {
            var pointsText = T("character.attribute.points", Preset.Attributes.Total, CharacterPresetValidator.AttributePointBudget);
            draw.Text(pointsText, new Vector2(ContentViewport.X + 8, panel.Y + 30), Preset.Attributes.Total <= CharacterPresetValidator.AttributePointBudget ? draw.Theme.MutedText : draw.Theme.Warning, 0.78f);
        }

        if (_activeTab == CharacterCreationTab.Review)
        {
            DrawReview(draw);
        }
    }

    private void DrawReview(UiDrawContext draw)
    {
        var area = ContentViewport;
        var y = area.Y + 8;
        DrawLine(draw, T("gameplay.player", FullName(Preset)), area.X + 10, ref y, draw.Theme.Text);
        DrawLine(draw, T("gameplay.age", T("age." + Preset.AgeCategoryId)), area.X + 10, ref y, draw.Theme.Text);
        DrawLine(draw, T("gameplay.origin", T("origin." + Preset.OriginId)), area.X + 10, ref y, draw.Theme.Text);
        DrawLine(draw, T("character.birthday", T("season." + Preset.BirthdaySeasonId), Preset.BirthdayDay), area.X + 10, ref y, draw.Theme.Text);
        DrawLine(draw, T("character.motivation", T(CharacterOptionCatalog.FindMotivation(Preset.MotivationId).LabelKey)), area.X + 10, ref y, draw.Theme.Text, 0.86f);
        DrawLine(draw, T("character.attribute.points", Preset.Attributes.Total, CharacterPresetValidator.AttributePointBudget), area.X + 10, ref y, draw.Theme.Accent);
        DrawLine(draw, T("character.review.needs", Preset.Needs.Energy, Preset.Needs.Hunger, Preset.Needs.Health, Preset.Needs.Mood), area.X + 10, ref y, draw.Theme.Text, 0.86f);

        var topSkills = Preset.Skills
            .Where(skill => skill.Level > 0)
            .OrderByDescending(skill => skill.Level)
            .Take(3)
            .Select(skill => $"{T(SkillLabelKey(skill.SkillId))} {skill.Level}")
            .ToList();

        DrawLine(draw, T("character.review.skills", topSkills.Count == 0 ? T("common.empty") : string.Join(", ", topSkills)), area.X + 10, ref y, draw.Theme.Text, 0.86f);
        DrawLine(draw, T("new_game.review_hint"), area.X + 10, ref y, draw.Theme.MutedText, 0.78f);
    }

    private void DrawMessage(UiDrawContext draw)
    {
        if (_messageTimer <= 0 || string.IsNullOrWhiteSpace(_message))
        {
            return;
        }

        var size = Context.Font.MeasureString(_message) * UiScale * 0.82f;
        draw.Text(
            _message,
            new Vector2((Context.ViewBounds.Width - size.X) / 2f, Context.ViewBounds.Bottom - 28),
            _messageIsWarning ? draw.Theme.Warning : draw.Theme.Accent,
            0.82f);
    }

    private void DrawIdentitySummary(UiDrawContext draw)
    {
        var preview = PreviewPanel;
        var panel = new Rectangle(preview.X, preview.Bottom + 14, preview.Width, Math.Max(116, ContentPanel.Bottom - preview.Bottom - 84));
        draw.Fill(panel, new Color(21, 24, 24, 180));
        draw.Border(panel, draw.Theme.Border, 1);

        var motivation = CharacterOptionCatalog.FindMotivation(Preset.MotivationId);
        var y = panel.Y + 12;
        DrawClippedLine(draw, FullName(Preset), panel.X + 12, panel.Right - 12, ref y, draw.Theme.Accent, 0.88f);
        DrawClippedLine(draw, T("character.birthday.short", T("season." + Preset.BirthdaySeasonId), Preset.BirthdayDay), panel.X + 12, panel.Right - 12, ref y, draw.Theme.Text, 0.78f);
        DrawClippedLine(draw, T(motivation.LabelKey), panel.X + 12, panel.Right - 12, ref y, draw.Theme.Text, 0.78f);
        DrawClippedLine(draw, T("character.future_trait", T("trait." + motivation.FutureTraitId)), panel.X + 12, panel.Right - 12, ref y, draw.Theme.MutedText, 0.68f);
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

    private void DrawAppearanceGroupHeaders(UiDrawContext draw)
    {
        if (_activeTab != CharacterCreationTab.Appearance)
        {
            return;
        }

        foreach (var header in _appearanceGroupHeaders)
        {
            draw.Text(T(header.LabelKey), new Vector2(header.Bounds.X, header.Bounds.Y + 2), draw.Theme.Accent, 0.82f);
            draw.Fill(new Rectangle(header.Bounds.X, header.Bounds.Bottom - 2, header.Bounds.Width, 1), draw.Theme.Border);
        }
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

        foreach (var tab in Enum.GetValues<CharacterCreationTab>())
        {
            if (TabBounds(tab).Contains(Context.Input.Current.MousePosition))
            {
                SetTab(tab);
                return;
            }
        }
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

    private void HandleCreationNavigation()
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

        if (_focusRegion == FocusRegion.Content)
        {
            if (downPressed && _menu.FocusedIndex >= contentCount - 1)
            {
                _focusRegion = FocusRegion.Footer;
                _footerFocusIndex = 3;
                _menu.SetFocusedIndex(contentCount + _footerFocusIndex);
                return;
            }

            if (upPressed && _menu.FocusedIndex <= 0)
            {
                _focusRegion = FocusRegion.Footer;
                _footerFocusIndex = 3;
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

            return;
        }

        if (leftPressed)
        {
            _footerFocusIndex = Math.Max(0, _footerFocusIndex - 1);
            _menu.SetFocusedIndex(contentCount + _footerFocusIndex);
            return;
        }

        if (rightPressed)
        {
            _footerFocusIndex = Math.Min(FooterButtonCount - 1, _footerFocusIndex + 1);
            _menu.SetFocusedIndex(contentCount + _footerFocusIndex);
            return;
        }

        if ((upPressed || downPressed) && contentCount > 0)
        {
            _focusRegion = FocusRegion.Content;
            _menu.SetFocusedIndex(upPressed ? contentCount - 1 : 0);
        }
    }

    private void SyncFocusRegionFromFocusedIndex()
    {
        var contentCount = ContentControlCount;
        if (_menu.FocusedIndex >= contentCount)
        {
            _focusRegion = FocusRegion.Footer;
            _footerFocusIndex = Math.Clamp(_menu.FocusedIndex - contentCount, 0, FooterButtonCount - 1);
            return;
        }

        _focusRegion = FocusRegion.Content;
    }

    private void EnsureFocusedControlVisible()
    {
        var focusedIndex = _menu.FocusedIndex;
        if (focusedIndex < 0 || focusedIndex >= ContentControlCount)
        {
            return;
        }

        var viewport = ContentViewport;
        if (_activeTab == CharacterCreationTab.Appearance)
        {
            var focused = _menu.Controls[focusedIndex].Bounds;
            if (focused.Y < viewport.Y)
            {
                _scrollOffset -= viewport.Y - focused.Y;
            }
            else if (focused.Bottom > viewport.Bottom)
            {
                _scrollOffset += focused.Bottom - viewport.Bottom;
            }

            _scrollOffset = Math.Clamp(_scrollOffset, 0, _maxScrollOffset);
            return;
        }

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

    private void ChangeTab(int direction)
    {
        var tabCount = Enum.GetValues<CharacterCreationTab>().Length;
        var next = ((int)_activeTab + direction) % tabCount;
        if (next < 0)
        {
            next = tabCount - 1;
        }

        SetTab((CharacterCreationTab)next);
    }

    private void SetTab(CharacterCreationTab tab)
    {
        if (_activeTab == tab)
        {
            return;
        }

        _activeTab = tab;
        BuildControls();
    }

    private void SetAgeCategory(string value)
    {
        Preset.AgeCategoryId = value;
        CharacterPresetFactory.ApplyStartingProfile(Preset);
        CharacterPresetFactory.EnsureDefaultSkills(Preset);
    }

    private void SetOrigin(string value)
    {
        Preset.OriginId = value;
        CharacterPresetFactory.ApplyStartingProfile(Preset);
        CharacterPresetFactory.EnsureDefaultSkills(Preset);
    }

    private void SetAttribute(string attributeId, float value)
    {
        SetAttribute(attributeId, (int)MathF.Round(value));
    }

    private void SetAttribute(string attributeId, int value)
    {
        var current = Preset.Attributes.GetValue(attributeId);
        var target = Math.Clamp(value, CharacterPresetValidator.AttributeMinimum, CharacterPresetValidator.AttributeMaximum);
        if (target > current)
        {
            var available = Math.Max(0, CharacterPresetValidator.AttributePointBudget - Preset.Attributes.Total);
            target = current + Math.Min(target - current, available);
        }

        Preset.Attributes.SetValue(attributeId, target);
    }

    private void RandomizeAttributes()
    {
        Preset.Attributes = CharacterAttributePointBuy.Randomize(_random);
        ShowMessage(T("new_game.attributes_randomized"));
    }

    private void AddAppearanceColorOnly(string groupKey, string slotId)
    {
        var colorIndex = _menu.Controls.Count;
        _menu.Add(new SelectorControl(
            ColorLabelKey(slotId),
            CharacterColorPaletteCatalog.GetSelectorOptions(slotId),
            () => Preset.Appearance.GetPalette(slotId),
            value => Preset.Appearance.SetPalette(slotId, value)));

        _appearanceControlEntries.Add(new AppearanceControlEntry(groupKey, slotId, -1, colorIndex));
    }

    private void AddAppearanceSlotPair(string groupKey, string slotId)
    {
        var slot = Context.CharacterAssets.Slots.FirstOrDefault(existing => existing.SlotId == slotId);
        if (slot == null)
        {
            return;
        }

        var itemIndex = _menu.Controls.Count;
        _menu.Add(new SelectorControl(
            slot.LabelKey,
            Context.CharacterAssets.GetSelectorOptions(slot),
            () => Preset.Appearance.GetLayer(slot.SlotId),
            value => Preset.Appearance.SetLayer(slot.SlotId, value)));

        var colorIndex = _menu.Controls.Count;
        _menu.Add(new SelectorControl(
            ColorLabelKey(slot.SlotId),
            CharacterColorPaletteCatalog.GetSelectorOptions(slot.SlotId),
            () => Preset.Appearance.GetPalette(slot.SlotId),
            value => Preset.Appearance.SetPalette(slot.SlotId, value)));

        _appearanceControlEntries.Add(new AppearanceControlEntry(groupKey, slotId, itemIndex, colorIndex));
    }

    private void RestoreDefaults()
    {
        _options.CharacterPreset = CharacterPresetFactory.CreateDefault();
        BuildControls();
        ShowMessage(T("new_game.defaults_restored"));
    }

    private void ImportPreset()
    {
        try
        {
            Directory.CreateDirectory(Context.CharacterPresetStorage.DefaultDirectory);
            using var dialog = new WinForms.OpenFileDialog
            {
                Title = T("new_game.import_preset"),
                Filter = "Dusk Village Character (*.dvchar.json)|*.dvchar.json|JSON (*.json)|*.json|All files (*.*)|*.*",
                InitialDirectory = Context.CharacterPresetStorage.DefaultDirectory,
                CheckFileExists = true
            };

            if (dialog.ShowDialog() != WinForms.DialogResult.OK)
            {
                return;
            }

            var imported = Context.CharacterPresetStorage.Load(dialog.FileName);
            var validation = CharacterPresetValidator.Validate(imported);
            if (!validation.IsValid)
            {
                ShowMessage(FormatValidationError(validation.Errors[0]), warning: true);
                return;
            }

            _options.CharacterPreset = imported;
            BuildControls();
            ShowMessage(T("new_game.imported"));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            ShowMessage(T("new_game.import_failed", exception.Message), warning: true);
        }
    }

    private void ExportPreset()
    {
        var validation = CharacterPresetValidator.Validate(Preset);
        if (!validation.IsValid)
        {
            ShowMessage(FormatValidationError(validation.Errors[0]), warning: true);
            return;
        }

        try
        {
            Directory.CreateDirectory(Context.CharacterPresetStorage.DefaultDirectory);
            using var dialog = new WinForms.SaveFileDialog
            {
                Title = T("new_game.export_preset"),
                Filter = "Dusk Village Character (*.dvchar.json)|*.dvchar.json|JSON (*.json)|*.json|All files (*.*)|*.*",
                InitialDirectory = Context.CharacterPresetStorage.DefaultDirectory,
                FileName = Path.GetFileName(Context.CharacterPresetStorage.CreateDefaultExportPath(Preset)),
                AddExtension = true,
                DefaultExt = "dvchar.json",
                OverwritePrompt = true
            };

            if (dialog.ShowDialog() != WinForms.DialogResult.OK)
            {
                return;
            }

            Context.CharacterPresetStorage.Save(Preset, dialog.FileName);
            ShowMessage(T("new_game.exported", dialog.FileName));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            ShowMessage(T("new_game.export_failed", exception.Message), warning: true);
        }
    }

    private void BeginNewGame()
    {
        var validation = CharacterPresetValidator.Validate(Preset);
        if (!validation.IsValid)
        {
            ShowMessage(FormatValidationError(validation.Errors[0]), warning: true);
            return;
        }

        var session = GameSessionSummary.FromNewGame(_options);
        Context.Navigator.SetRoot(new GameplayPlaceholderScreen(Context, session));
    }

    private string FormatValidationError(CharacterPresetValidationError error)
    {
        var args = error.Args
            .Select(argument => argument is string text && (text.StartsWith("character.") || text.StartsWith("skill.") || text.StartsWith("season.") || text.StartsWith("motivation."))
                ? T(text)
                : argument)
            .ToArray();

        return T(error.MessageKey, args);
    }

    private void ShowMessage(string message, bool warning = false)
    {
        _message = message;
        _messageIsWarning = warning;
        _messageTimer = 3.5;
    }

    private void UpdateTooltip()
    {
        _tooltipText = string.Empty;
        _tooltipAnchor = Context.Input.Current.MousePosition;

        if (_activeTab != CharacterCreationTab.Status)
        {
            return;
        }

        foreach (var control in _menu.Controls.Take(ContentControlCount))
        {
            if (control is IUiTooltipProvider provider &&
                provider.TryGetTooltip(Context, out var tooltipKey, out var anchor))
            {
                _tooltipText = T(tooltipKey);
                _tooltipAnchor = anchor;
                return;
            }
        }
    }

    private void DrawTooltip(UiDrawContext draw)
    {
        if (string.IsNullOrWhiteSpace(_tooltipText))
        {
            return;
        }

        const float textScale = 0.72f;
        var maxTextWidth = Math.Min(420, ContentPanel.Width - 64);
        var text = FitText(draw, _tooltipText, maxTextWidth, textScale);
        var textSize = draw.Font.MeasureString(text) * draw.Scale * textScale;
        var bounds = new Rectangle(
            _tooltipAnchor.X + 18,
            _tooltipAnchor.Y + 20,
            (int)MathF.Ceiling(textSize.X) + 24,
            (int)MathF.Ceiling(textSize.Y) + 18);

        var panel = ContentPanel;
        if (bounds.Right > panel.Right - 10)
        {
            bounds.X = panel.Right - bounds.Width - 10;
        }

        if (bounds.Bottom > panel.Bottom - 10)
        {
            bounds.Y = _tooltipAnchor.Y - bounds.Height - 16;
        }

        bounds.X = Math.Clamp(bounds.X, panel.X + 10, Math.Max(panel.X + 10, panel.Right - bounds.Width - 10));
        bounds.Y = Math.Clamp(bounds.Y, panel.Y + 10, Math.Max(panel.Y + 10, panel.Bottom - bounds.Height - 10));

        draw.Fill(bounds, new Color(18, 20, 20, 238));
        draw.Border(bounds, draw.Theme.AccentMuted, 1);
        draw.Text(text, new Vector2(bounds.X + 12, bounds.Y + 8), draw.Theme.Text, textScale);
    }

    private Rectangle TabBounds(CharacterCreationTab tab)
    {
        const int tabWidth = 170;
        const int tabHeight = 44;
        const int gap = 12;

        var startX = CenterX(Context.ViewBounds, tabWidth * 4 + gap * 3);
        var index = (int)tab;
        return new Rectangle(startX + index * (tabWidth + gap), 34, tabWidth, tabHeight);
    }

    private static string TabTitleKey(CharacterCreationTab tab)
    {
        return tab switch
        {
            CharacterCreationTab.Appearance => "new_game.tab.appearance",
            CharacterCreationTab.Status => "new_game.tab.status",
            CharacterCreationTab.Review => "new_game.tab.review",
            _ => "new_game.tab.identity"
        };
    }

    private static string SkillLabelKey(string skillId)
    {
        return CharacterOptionCatalog.Skills.FirstOrDefault(skill => skill.Value == skillId)?.LabelKey ?? skillId;
    }

    private static string AttributeTooltipKey(string attributeId)
    {
        return attributeId switch
        {
            CharacterAttributeIds.Strength => "character.attribute.strength.tooltip",
            CharacterAttributeIds.Agility => "character.attribute.agility.tooltip",
            CharacterAttributeIds.Constitution => "character.attribute.constitution.tooltip",
            CharacterAttributeIds.Intelligence => "character.attribute.intelligence.tooltip",
            CharacterAttributeIds.Charisma => "character.attribute.charisma.tooltip",
            CharacterAttributeIds.Wisdom => "character.attribute.wisdom.tooltip",
            _ => attributeId
        };
    }

    private static string ColorLabelKey(string slotId)
    {
        return slotId switch
        {
            CharacterAppearanceSlotIds.Under => "character.color.00undr",
            CharacterAppearanceSlotIds.Body => "character.color.skin",
            CharacterAppearanceSlotIds.Socks => "character.color.02sock",
            CharacterAppearanceSlotIds.FootwearLow => "character.color.03fot1",
            CharacterAppearanceSlotIds.LowerOne => "character.color.04lwr1",
            CharacterAppearanceSlotIds.Shirt => "character.color.05shrt",
            CharacterAppearanceSlotIds.LowerTwo => "character.color.06lwr2",
            CharacterAppearanceSlotIds.FootwearHigh => "character.color.07fot2",
            CharacterAppearanceSlotIds.LowerThree => "character.color.08lwr3",
            CharacterAppearanceSlotIds.Hands => "character.color.09hand",
            CharacterAppearanceSlotIds.Outer => "character.color.10outr",
            CharacterAppearanceSlotIds.Neck => "character.color.11neck",
            CharacterAppearanceSlotIds.Face => "character.color.12face",
            CharacterAppearanceSlotIds.Hair => "character.color.hair",
            CharacterAppearanceSlotIds.Head => "character.color.14head",
            _ => "character.color.default"
        };
    }

    private static string FullName(CharacterPreset preset)
    {
        return string.IsNullOrWhiteSpace(preset.FamilyName)
            ? preset.Name
            : $"{preset.Name} {preset.FamilyName}";
    }

    private void DrawLine(UiDrawContext draw, string text, int x, ref int y, Color color, float scale = 0.92f)
    {
        draw.Text(text, new Vector2(x, y), color, scale);
        y += (int)(34 * UiScale * scale);
    }

    private void DrawClippedLine(UiDrawContext draw, string text, int x, int rightX, ref int y, Color color, float scale)
    {
        draw.Text(FitText(draw, text, Math.Max(24, rightX - x), scale), new Vector2(x, y), color, scale);
        y += (int)(34 * UiScale * scale);
    }

    private static string FitText(UiDrawContext draw, string text, int maxWidth, float scale)
    {
        if (draw.Font.MeasureString(text).X * draw.Scale * scale <= maxWidth)
        {
            return text;
        }

        const string ellipsis = "...";
        var trimmed = text;
        while (trimmed.Length > 0)
        {
            trimmed = trimmed[..^1];
            var candidate = trimmed.TrimEnd() + ellipsis;
            if (draw.Font.MeasureString(candidate).X * draw.Scale * scale <= maxWidth)
            {
                return candidate;
            }
        }

        return ellipsis;
    }

    private static readonly IReadOnlyList<string> AppearanceGroupOrder =
    [
        "new_game.appearance.group.character",
        "new_game.appearance.group.clothes",
        "new_game.appearance.group.layers",
        "new_game.appearance.group.accessories"
    ];

    private sealed class AppearanceControlEntry
    {
        public AppearanceControlEntry(string groupKey, string slotId, int itemControlIndex, int colorControlIndex)
        {
            GroupKey = groupKey;
            SlotId = slotId;
            ItemControlIndex = itemControlIndex;
            ColorControlIndex = colorControlIndex;
        }

        public string GroupKey { get; }

        public string SlotId { get; }

        public int ItemControlIndex { get; }

        public int ColorControlIndex { get; }
    }

    private sealed class AppearanceGroupHeader
    {
        public AppearanceGroupHeader(string labelKey, Rectangle bounds)
        {
            LabelKey = labelKey;
            Bounds = bounds;
        }

        public string LabelKey { get; }

        public Rectangle Bounds { get; }
    }

    private enum CharacterCreationTab
    {
        Identity,
        Appearance,
        Status,
        Review
    }

    private enum FocusRegion
    {
        Content,
        Footer
    }
}
