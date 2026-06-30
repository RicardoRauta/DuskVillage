using System;
using System.Collections.Generic;
using System.Globalization;
using DuskVillage.Inventory;
using DuskVillage.InventoryAssets;
using DuskVillage.Items;
using DuskVillage.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuskVillage.Rendering;

public sealed class InventoryHotbarRenderer
{
    private const int BackpackGridColumns = 5;
    private const int BackpackGridVisibleSlots = 15;

    private readonly InventoryUiAssetCatalog _assets;
    private readonly InventoryUiTextureProvider _textures;

    public InventoryHotbarRenderer(InventoryUiAssetCatalog assets, InventoryUiTextureProvider textures)
    {
        _assets = assets ?? InventoryUiAssetCatalog.Empty;
        _textures = textures;
    }

    public void Draw(
        UiDrawContext draw,
        InventoryState inventory,
        ItemDefinitionRegistry items,
        Rectangle bounds,
        string skinId = InventoryUiAssetIds.TravelerBackpackSkin,
        float scaleMultiplier = 1f)
    {
        var normalized = InventorySystem.Normalize(inventory, items);
        var skin = _assets.GetSkinOrFallback(skinId);
        var effectiveScale = draw.Scale * Math.Max(0.1f, scaleMultiplier);
        var assetScale = AssetScale(effectiveScale);
        var slotSize = PreferredSlotSizeForAssetScale(skin, assetScale);
        var gap = PreferredGapForAssetScale(assetScale);
        DrawBackpackIcon(draw, skin, bounds, slotSize, gap, assetScale);
        DrawSlots(draw, normalized, items, skin, bounds, normalized.HotbarSize, normalized.HotbarSize, 0, normalized.SelectedHotbarIndex, slotSize, gap, assetScale, showNumbers: true, showEmptyDash: true);
    }

    public void DrawGrid(
        UiDrawContext draw,
        InventoryState inventory,
        ItemDefinitionRegistry items,
        Rectangle bounds,
        int columns,
        string skinId = InventoryUiAssetIds.TravelerBackpackSkin)
    {
        var normalized = InventorySystem.Normalize(inventory, items);
        var skin = _assets.GetSkinOrFallback(skinId);
        var assetScale = AssetScale(draw.Scale);
        var slotSize = PreferredSlotSizeForAssetScale(skin, assetScale);
        var gap = PreferredGapForAssetScale(assetScale);
        var visibleSlots = Math.Min(normalized.Slots.Count, normalized.Capacity);
        DrawSlots(draw, normalized, items, skin, bounds, columns, visibleSlots, 0, normalized.SelectedHotbarIndex, slotSize, gap, assetScale, showNumbers: false, showEmptyDash: true);
    }

    public bool DrawBackpackScreen(
        UiDrawContext draw,
        InventoryState inventory,
        ItemDefinitionRegistry items,
        Rectangle viewport,
        string title,
        string closeHint,
        int selectedSlotIndex = -1,
        string skinId = InventoryUiAssetIds.TravelerBackpackSkin)
    {
        var normalized = InventorySystem.Normalize(inventory, items);
        var skin = _assets.GetSkinOrFallback(skinId);
        var backgroundAsset = skin?.FindAsset(InventoryUiAssetIds.BackpackBackground);
        var backgroundTexture = _textures?.GetTexture(backgroundAsset);
        if (backgroundTexture == null)
        {
            return false;
        }

        var source = Source(backgroundAsset, backgroundTexture);
        var screenScale = BackpackScreenScale(viewport, source.Width, source.Height);
        var background = BackpackBackgroundBounds(viewport, source.Width, source.Height);

        draw.SpriteBatch.Draw(backgroundTexture, background, source, Color.White);
        DrawBackpackTitle(draw, background, title, closeHint);
        DrawBackpackSideTabs(draw, skin, background, screenScale);

        selectedSlotIndex = NormalizeSelectedSlotIndex(normalized, selectedSlotIndex);
        var slotScale = BackpackSlotScale(screenScale);
        var slotSize = PreferredSlotSizeForAssetScale(skin, slotScale);
        var gap = PreferredGapForAssetScale(slotScale);
        var firstGridSlotIndex = BackpackGridFirstSlot(normalized);
        var visibleSlots = BackpackGridVisibleSlotCount(normalized);
        var gridRows = Math.Max(1, (int)Math.Ceiling(visibleSlots / (double)BackpackGridColumns));
        var gridWidth = BackpackGridColumns * slotSize.X + (BackpackGridColumns - 1) * gap;
        var gridHeight = gridRows * slotSize.Y + (gridRows - 1) * gap;
        var gridArea = BackpackGridArea(background);
        var gridBounds = new Rectangle(
            gridArea.X + (gridArea.Width - gridWidth) / 2,
            gridArea.Y + (gridArea.Height - gridHeight) / 2,
            gridWidth,
            gridHeight);

        DrawSlots(draw, normalized, items, skin, gridBounds, BackpackGridColumns, visibleSlots, firstGridSlotIndex, selectedSlotIndex, slotSize, gap, slotScale, showNumbers: false, showEmptyDash: false);

        var detailPanel = BackpackDetailPanel(background);
        DrawSelectedItemPanel(draw, normalized, items, skin, detailPanel, selectedSlotIndex, PanelPixelScale(slotScale));
        return true;
    }

    public int HitBackpackSlot(
        InventoryState inventory,
        ItemDefinitionRegistry items,
        Rectangle viewport,
        Point pointer,
        string skinId = InventoryUiAssetIds.TravelerBackpackSkin)
    {
        var normalized = InventorySystem.Normalize(inventory, items);
        var skin = _assets.GetSkinOrFallback(skinId);
        var backgroundAsset = skin?.FindAsset(InventoryUiAssetIds.BackpackBackground);
        if (backgroundAsset == null)
        {
            return -1;
        }

        var background = BackpackBackgroundBounds(viewport, backgroundAsset.Width, backgroundAsset.Height);
        var screenScale = BackpackScreenScale(viewport, backgroundAsset.Width, backgroundAsset.Height);
        var slotScale = BackpackSlotScale(screenScale);
        var slotSize = PreferredSlotSizeForAssetScale(skin, slotScale);
        var gap = PreferredGapForAssetScale(slotScale);
        var visibleSlots = BackpackGridVisibleSlotCount(normalized);
        var gridRows = Math.Max(1, (int)Math.Ceiling(visibleSlots / (double)BackpackGridColumns));
        var gridWidth = BackpackGridColumns * slotSize.X + (BackpackGridColumns - 1) * gap;
        var gridHeight = gridRows * slotSize.Y + (gridRows - 1) * gap;
        var gridArea = BackpackGridArea(background);
        var gridBounds = new Rectangle(
            gridArea.X + (gridArea.Width - gridWidth) / 2,
            gridArea.Y + (gridArea.Height - gridHeight) / 2,
            gridWidth,
            gridHeight);

        return HitSlot(pointer, gridBounds, BackpackGridColumns, visibleSlots, BackpackGridFirstSlot(normalized), slotSize, gap);
    }

    public Point PreferredSlotSize(float uiScale, string skinId = InventoryUiAssetIds.TravelerBackpackSkin)
    {
        return PreferredSlotSizeForAssetScale(_assets.GetSkinOrFallback(skinId), AssetScale(uiScale));
    }

    public int PreferredGap(float uiScale)
    {
        return PreferredGapForAssetScale(AssetScale(uiScale));
    }

    private void DrawSlots(
        UiDrawContext draw,
        InventoryState inventory,
        ItemDefinitionRegistry items,
        InventoryUiSkin skin,
        Rectangle bounds,
        int columns,
        int visibleSlots,
        int firstSlotIndex,
        int selectedSlotIndex,
        Point slotSize,
        int gap,
        float assetScale,
        bool showNumbers,
        bool showEmptyDash)
    {
        var safeColumns = Math.Max(1, columns);
        for (var i = 0; i < visibleSlots; i++)
        {
            var column = i % safeColumns;
            var row = i / safeColumns;
            var slotBounds = new Rectangle(
                bounds.X + column * (slotSize.X + gap),
                bounds.Y + row * (slotSize.Y + gap),
                slotSize.X,
                slotSize.Y);
            var slotIndex = firstSlotIndex + i;
            var selected = slotIndex == selectedSlotIndex;
            DrawSlotFrame(draw, skin, slotBounds, selected, assetScale);
            if (showNumbers)
            {
                draw.Text((i + 1).ToString(CultureInfo.InvariantCulture), new Vector2(slotBounds.X + 7, slotBounds.Y + 5), draw.Theme.MutedText, SlotTextScale(assetScale, 0.46f));
            }

            if (slotIndex >= inventory.Slots.Count || inventory.Slots[slotIndex].IsEmpty)
            {
                if (showEmptyDash)
                {
                    draw.CenteredText("-", slotBounds, draw.Theme.MutedText, SlotTextScale(assetScale, 0.72f));
                }

                continue;
            }

            DrawSlotContent(draw, skin, inventory.Slots[slotIndex], items, slotBounds, assetScale);
        }
    }

    private void DrawBackpackTitle(UiDrawContext draw, Rectangle background, string title, string closeHint)
    {
        var titleBounds = Relative(background, 0.15f, 0.08f, 0.70f, 0.08f);
        draw.CenteredText(title?.ToUpperInvariant() ?? string.Empty, titleBounds, draw.Theme.Accent, 1.26f);

        if (!string.IsNullOrWhiteSpace(closeHint))
        {
            draw.CenteredText(closeHint, Relative(background, 0.54f, 0.20f, 0.32f, 0.04f), draw.Theme.MutedText, 0.52f);
        }
    }

    private void DrawBackpackSideTabs(UiDrawContext draw, InventoryUiSkin skin, Rectangle background, float screenScale)
    {
        var asset = skin?.FindAsset(InventoryUiAssetIds.SideTab);
        var texture = _textures?.GetTexture(asset);
        var scale = Math.Clamp(screenScale * 1.12f, 1.05f, 1.35f);
        var iconSource = texture == null ? Rectangle.Empty : Source(asset, texture);
        var tabSize = Math.Max(44, (int)Math.Round(42 * scale));
        var activeWidth = Math.Max(122, (int)Math.Round(124 * scale));
        var x = background.X + (int)Math.Round(background.Width * 0.03f);
        var y = background.Y + (int)Math.Round(background.Height * 0.36f);
        var gap = Math.Max(10, (int)Math.Round(tabSize * 0.34f));
        for (var i = 0; i < 5; i++)
        {
            var tab = new Rectangle(
                x,
                y + i * (tabSize + gap),
                i == 0 ? activeWidth : tabSize,
                tabSize);
            DrawPaperPanel(draw, skin, tab, PanelPixelScale(scale));
            if (texture != null)
            {
                var iconSize = Math.Max(20, (int)Math.Round(24 * scale));
                var icon = new Rectangle(tab.X + 8, tab.Y + (tab.Height - iconSize) / 2, iconSize, iconSize);
                draw.SpriteBatch.Draw(texture, icon, iconSource, Color.White);
            }

            if (i == 0)
            {
                draw.Text(draw.Localization.Text("inventory.title").ToUpperInvariant(), new Vector2(tab.X + tabSize + 8, tab.Y + tab.Height / 2f - 8 * draw.Scale), draw.Theme.Text, 0.54f);
            }
        }
    }

    private void DrawSelectedItemPanel(
        UiDrawContext draw,
        InventoryState inventory,
        ItemDefinitionRegistry items,
        InventoryUiSkin skin,
        Rectangle panel,
        int selectedSlotIndex,
        int pixelScale)
    {
        DrawPaperPanel(draw, skin, panel, pixelScale);

        var slot = SlotAt(inventory, selectedSlotIndex);
        var definition = slot.IsEmpty ? null : items?.Find(slot.ItemId);
        var label = slot.IsEmpty ? draw.Localization.Text("inventory.empty_slot") : ItemLabel(draw, definition, slot.ItemId);
        var description = slot.IsEmpty
            ? string.Empty
            : string.IsNullOrWhiteSpace(definition?.DescriptionKey)
                ? slot.ItemId
                : draw.Localization.Text(definition.DescriptionKey);

        var padding = Math.Max(10, 12 * pixelScale);
        var textWidth = Math.Max(24, panel.Width - padding * 2);
        draw.CenteredText(label.ToUpperInvariant(), new Rectangle(panel.X + padding, panel.Y + padding, textWidth, 34 * pixelScale), new Color(34, 30, 26), 0.68f);
        if (!slot.IsEmpty && slot.Quantity > 1)
        {
            draw.CenteredText("x" + slot.Quantity.ToString(CultureInfo.InvariantCulture), new Rectangle(panel.X + padding, panel.Y + padding + 38 * pixelScale, textWidth, 24 * pixelScale), new Color(60, 48, 38), 0.56f);
        }

        var descriptionBounds = new Rectangle(panel.X + padding, panel.Y + padding + 72 * pixelScale, textWidth, Math.Max(20, panel.Height - padding * 2 - 72 * pixelScale));
        DrawPaperLines(draw, descriptionBounds, pixelScale);
        DrawWrappedText(draw, description, descriptionBounds, new Color(34, 30, 26), 0.54f, 25 * pixelScale);
    }

    private void DrawBackpackIcon(UiDrawContext draw, InventoryUiSkin skin, Rectangle bounds, Point slotSize, int gap, float assetScale)
    {
        var asset = skin?.FindAsset(InventoryUiAssetIds.BackpackIdle);
        var texture = _textures?.GetTexture(asset);
        if (texture == null)
        {
            return;
        }

        var width = Math.Max(1, (int)Math.Round((asset?.Width ?? texture.Width) * assetScale));
        var height = Math.Max(1, (int)Math.Round((asset?.Height ?? texture.Height) * assetScale));
        var destination = new Rectangle(bounds.X - gap - width, bounds.Y + (slotSize.Y - height) / 2, width, height);
        draw.SpriteBatch.Draw(texture, destination, Source(asset, texture), Color.White);
    }

    private void DrawSlotFrame(UiDrawContext draw, InventoryUiSkin skin, Rectangle slotBounds, bool selected, float assetScale)
    {
        var holderAsset = skin?.FindAsset(InventoryUiAssetIds.SlotHolder);
        var holderTexture = _textures?.GetTexture(holderAsset);
        if (holderTexture != null)
        {
            draw.SpriteBatch.Draw(holderTexture, slotBounds, Source(holderAsset, holderTexture), Color.White);
        }
        else
        {
            draw.Fill(slotBounds, selected ? new Color(44, 48, 42, 236) : draw.Theme.Panel);
            draw.Border(slotBounds, selected ? draw.Theme.Accent : draw.Theme.Border, selected ? Math.Max(3, PanelPixelScale(assetScale) + 1) : Math.Max(2, PanelPixelScale(assetScale)));
        }

        if (!selected)
        {
            return;
        }

        var highlightAsset = skin?.FindAsset(InventoryUiAssetIds.SlotHighlight);
        var highlightTexture = _textures?.GetTexture(highlightAsset);
        if (highlightTexture != null)
        {
            var width = Math.Max(1, (int)Math.Round((highlightAsset?.Width ?? highlightTexture.Width) * assetScale));
            var height = Math.Max(1, (int)Math.Round((highlightAsset?.Height ?? highlightTexture.Height) * assetScale));
            var highlight = CenteredOn(slotBounds, width, height);
            draw.SpriteBatch.Draw(highlightTexture, highlight, Source(highlightAsset, highlightTexture), Color.White);
        }
        else
        {
            draw.Border(slotBounds, draw.Theme.Accent, 3);
        }
    }

    private void DrawSlotContent(
        UiDrawContext draw,
        InventoryUiSkin skin,
        InventorySlot slot,
        ItemDefinitionRegistry items,
        Rectangle slotBounds,
        float assetScale)
    {
        var definition = items?.Find(slot.ItemId);
        var iconAssetId = string.IsNullOrWhiteSpace(definition?.IconAssetId)
            ? "backpack_icon_unknown"
            : definition.IconAssetId;
        var iconAsset = skin?.FindAsset(iconAssetId);
        var iconTexture = _textures?.GetTexture(iconAsset);
        if (iconTexture != null)
        {
            var iconSize = Math.Max(1, (int)Math.Round((iconAsset?.Width ?? 16) * assetScale));
            var iconBounds = new Rectangle(
                slotBounds.X + (slotBounds.Width - iconSize) / 2,
                slotBounds.Y + (slotBounds.Height - iconSize) / 2,
                iconSize,
                iconSize);
            draw.SpriteBatch.Draw(iconTexture, iconBounds, Source(iconAsset, iconTexture), Color.White);
        }
        else
        {
            draw.CenteredText(ShortItemLabel(draw, definition, slot.ItemId), new Rectangle(slotBounds.X + 4, slotBounds.Y + 12, slotBounds.Width - 8, slotBounds.Height - 20), draw.Theme.Text, SlotTextScale(assetScale, 0.5f));
        }

        if (slot.Quantity > 1)
        {
            draw.RightAlignedText(slot.Quantity.ToString(CultureInfo.InvariantCulture), slotBounds.Right - 7, slotBounds.Bottom - 20, draw.Theme.Accent, SlotTextScale(assetScale, 0.52f));
        }
    }

    private void DrawPaperPanel(UiDrawContext draw, InventoryUiSkin skin, Rectangle bounds, int pixelScale)
    {
        if (!TryPaperTexture(skin, InventoryUiAssetIds.PaperTopLeft, out var topLeftAsset, out var topLeft) ||
            !TryPaperTexture(skin, InventoryUiAssetIds.PaperTop, out var topAsset, out var top) ||
            !TryPaperTexture(skin, InventoryUiAssetIds.PaperTopRight, out var topRightAsset, out var topRight) ||
            !TryPaperTexture(skin, InventoryUiAssetIds.PaperLeft, out var leftAsset, out var left) ||
            !TryPaperTexture(skin, InventoryUiAssetIds.PaperPanel, out var centerAsset, out var center) ||
            !TryPaperTexture(skin, InventoryUiAssetIds.PaperRight, out var rightAsset, out var right) ||
            !TryPaperTexture(skin, InventoryUiAssetIds.PaperBottomLeft, out var bottomLeftAsset, out var bottomLeft) ||
            !TryPaperTexture(skin, InventoryUiAssetIds.PaperBottom, out var bottomAsset, out var bottom) ||
            !TryPaperTexture(skin, InventoryUiAssetIds.PaperBottomRight, out var bottomRightAsset, out var bottomRight))
        {
            draw.Fill(bounds, new Color(228, 204, 157, 236));
            draw.Border(bounds, new Color(94, 75, 58), Math.Max(2, pixelScale));
            return;
        }

        var borderX = Math.Clamp(20 * pixelScale, 8, Math.Max(8, bounds.Width / 3));
        var borderY = Math.Clamp(18 * pixelScale, 8, Math.Max(8, bounds.Height / 3));
        var centerWidth = Math.Max(1, bounds.Width - borderX * 2);
        var centerHeight = Math.Max(1, bounds.Height - borderY * 2);

        DrawTile(draw, topLeft, topLeftAsset, new Rectangle(bounds.X, bounds.Y, borderX, borderY));
        DrawTile(draw, top, topAsset, new Rectangle(bounds.X + borderX, bounds.Y, centerWidth, borderY));
        DrawTile(draw, topRight, topRightAsset, new Rectangle(bounds.Right - borderX, bounds.Y, borderX, borderY));
        DrawTile(draw, left, leftAsset, new Rectangle(bounds.X, bounds.Y + borderY, borderX, centerHeight));
        DrawTile(draw, center, centerAsset, new Rectangle(bounds.X + borderX, bounds.Y + borderY, centerWidth, centerHeight));
        DrawTile(draw, right, rightAsset, new Rectangle(bounds.Right - borderX, bounds.Y + borderY, borderX, centerHeight));
        DrawTile(draw, bottomLeft, bottomLeftAsset, new Rectangle(bounds.X, bounds.Bottom - borderY, borderX, borderY));
        DrawTile(draw, bottom, bottomAsset, new Rectangle(bounds.X + borderX, bounds.Bottom - borderY, centerWidth, borderY));
        DrawTile(draw, bottomRight, bottomRightAsset, new Rectangle(bounds.Right - borderX, bounds.Bottom - borderY, borderX, borderY));
    }

    private void DrawPaperLines(UiDrawContext draw, Rectangle bounds, int pixelScale)
    {
        var lineGap = Math.Max(18, 23 * pixelScale);
        var thickness = Math.Max(1, pixelScale);
        var color = new Color(125, 86, 70, 116);
        for (var y = bounds.Y + lineGap; y < bounds.Bottom - thickness; y += lineGap)
        {
            draw.Fill(new Rectangle(bounds.X, y, bounds.Width, thickness), color);
        }
    }

    private bool TryPaperTexture(InventoryUiSkin skin, string assetId, out InventoryUiAsset asset, out Texture2D texture)
    {
        asset = skin?.FindAsset(assetId);
        texture = _textures?.GetTexture(asset);
        return texture != null;
    }

    private static void DrawTile(UiDrawContext draw, Texture2D texture, InventoryUiAsset asset, Rectangle destination)
    {
        draw.SpriteBatch.Draw(texture, destination, Source(asset, texture), Color.White);
    }

    private static Rectangle Source(InventoryUiAsset asset, Texture2D texture)
    {
        if (asset == null)
        {
            return new Rectangle(0, 0, texture.Width, texture.Height);
        }

        return new Rectangle(0, 0, Math.Min(asset.Width, texture.Width), Math.Min(asset.Height, texture.Height));
    }

    private static Rectangle CenteredOn(Rectangle bounds, int width, int height)
    {
        return new Rectangle(
            bounds.X + (bounds.Width - width) / 2,
            bounds.Y + (bounds.Height - height) / 2,
            width,
            height);
    }

    private static Rectangle BackpackBackgroundBounds(Rectangle viewport, int sourceWidth, int sourceHeight)
    {
        var scale = BackpackScreenScale(viewport, sourceWidth, sourceHeight);
        return CenteredOn(
            viewport,
            Math.Max(1, (int)Math.Round(sourceWidth * scale)),
            Math.Max(1, (int)Math.Round(sourceHeight * scale)));
    }

    private static float BackpackScreenScale(Rectangle viewport, int sourceWidth, int sourceHeight)
    {
        var fitScale = Math.Min(viewport.Width * 0.96f / sourceWidth, viewport.Height * 1.02f / sourceHeight);
        return fitScale <= 0 ? 1f : Math.Min(fitScale, 1.42f);
    }

    private static float BackpackSlotScale(float screenScale)
    {
        return Math.Clamp(screenScale * 1.02f, 1.02f, 1.16f);
    }

    private static Rectangle BackpackGridArea(Rectangle background)
    {
        return Relative(background, 0.315f, 0.445f, 0.40f, 0.255f);
    }

    private static Rectangle BackpackDetailPanel(Rectangle background)
    {
        return Relative(background, 0.715f, 0.375f, 0.245f, 0.405f);
    }

    private static int BackpackGridFirstSlot(InventoryState inventory)
    {
        return Math.Min(inventory.HotbarSize, inventory.Capacity);
    }

    private static int BackpackGridVisibleSlotCount(InventoryState inventory)
    {
        return Math.Min(BackpackGridVisibleSlots, Math.Max(0, inventory.Capacity - BackpackGridFirstSlot(inventory)));
    }

    private static int NormalizeSelectedSlotIndex(InventoryState inventory, int selectedSlotIndex)
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < inventory.Slots.Count)
        {
            return selectedSlotIndex;
        }

        return Math.Clamp(inventory.SelectedHotbarIndex, 0, Math.Max(0, inventory.Slots.Count - 1));
    }

    private static InventorySlot SlotAt(InventoryState inventory, int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < inventory.Slots.Count
            ? inventory.Slots[slotIndex]
            : new InventorySlot();
    }

    private static int HitSlot(Point pointer, Rectangle bounds, int columns, int visibleSlots, int firstSlotIndex, Point slotSize, int gap)
    {
        if (!bounds.Contains(pointer))
        {
            return -1;
        }

        var safeColumns = Math.Max(1, columns);
        for (var i = 0; i < visibleSlots; i++)
        {
            var column = i % safeColumns;
            var row = i / safeColumns;
            var slotBounds = new Rectangle(
                bounds.X + column * (slotSize.X + gap),
                bounds.Y + row * (slotSize.Y + gap),
                slotSize.X,
                slotSize.Y);
            if (slotBounds.Contains(pointer))
            {
                return firstSlotIndex + i;
            }
        }

        return -1;
    }

    private static Rectangle Relative(Rectangle bounds, float x, float y, float width, float height)
    {
        return new Rectangle(
            bounds.X + (int)Math.Round(bounds.Width * x),
            bounds.Y + (int)Math.Round(bounds.Height * y),
            Math.Max(1, (int)Math.Round(bounds.Width * width)),
            Math.Max(1, (int)Math.Round(bounds.Height * height)));
    }

    private static void DrawWrappedText(UiDrawContext draw, string text, Rectangle bounds, Color color, float scale, int lineHeight)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var lines = WrapText(draw, text, bounds.Width, scale);
        var y = bounds.Y;
        foreach (var line in lines)
        {
            if (y + lineHeight > bounds.Bottom)
            {
                break;
            }

            draw.Text(line, new Vector2(bounds.X, y), color, scale);
            y += lineHeight;
        }
    }

    private static IReadOnlyList<string> WrapText(UiDrawContext draw, string text, int maxWidth, float scale)
    {
        var lines = new List<string>();
        var current = string.Empty;
        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var next = string.IsNullOrWhiteSpace(current) ? word : current + " " + word;
            if (draw.Font.MeasureString(next).X * draw.Scale * scale <= maxWidth || string.IsNullOrWhiteSpace(current))
            {
                current = next;
                continue;
            }

            lines.Add(current);
            current = word;
        }

        if (!string.IsNullOrWhiteSpace(current))
        {
            lines.Add(current);
        }

        return lines;
    }

    private static Point PreferredSlotSizeForAssetScale(InventoryUiSkin skin, float assetScale)
    {
        var asset = skin?.FindAsset(InventoryUiAssetIds.SlotHolder);
        return new Point(
            Math.Max(1, (int)Math.Round((asset?.Width ?? 48) * assetScale)),
            Math.Max(1, (int)Math.Round((asset?.Height ?? 48) * assetScale)));
    }

    private static int PreferredGapForAssetScale(float assetScale)
    {
        return Math.Max(4, (int)Math.Round(6 * assetScale));
    }

    private static float AssetScale(float uiScale)
    {
        return Math.Clamp(uiScale, 1f, 4f);
    }

    private static int PanelPixelScale(float assetScale)
    {
        return Math.Clamp((int)Math.Round(assetScale), 1, 4);
    }

    private static float SlotTextScale(float assetScale, float baseScale)
    {
        return baseScale * Math.Clamp(assetScale, 1f, 1.4f);
    }

    private static string ItemLabel(UiDrawContext draw, ItemDefinition definition, string itemId)
    {
        return definition == null ? itemId : draw.Localization.Text(definition.LabelKey);
    }

    private static string ShortItemLabel(UiDrawContext draw, ItemDefinition definition, string itemId)
    {
        var label = ItemLabel(draw, definition, itemId);
        return label.Length <= 10 ? label : label[..10];
    }
}
