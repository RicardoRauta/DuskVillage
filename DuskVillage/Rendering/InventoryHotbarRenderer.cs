using System;
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
        string skinId = InventoryUiAssetIds.TravelerBackpackSkin)
    {
        var normalized = InventorySystem.Normalize(inventory, items);
        var skin = _assets.GetSkinOrFallback(skinId);
        var pixelScale = PixelScale(draw.Scale);
        var slotSize = PreferredSlotSize(draw.Scale, skinId);
        var gap = PreferredGap(draw.Scale);
        DrawBackpackIcon(draw, skin, bounds, slotSize, gap, pixelScale);

        for (var i = 0; i < normalized.HotbarSize; i++)
        {
            var slotBounds = new Rectangle(
                bounds.X + i * (slotSize.X + gap),
                bounds.Y + (bounds.Height - slotSize.Y) / 2,
                slotSize.X,
                slotSize.Y);
            var selected = i == normalized.SelectedHotbarIndex;
            DrawSlotFrame(draw, skin, slotBounds, selected, pixelScale);
            draw.Text((i + 1).ToString(CultureInfo.InvariantCulture), new Vector2(slotBounds.X + 7, slotBounds.Y + 5), draw.Theme.MutedText, 0.46f);

            if (i >= normalized.Slots.Count || normalized.Slots[i].IsEmpty)
            {
                draw.CenteredText("-", slotBounds, draw.Theme.MutedText, 0.72f);
                continue;
            }

            DrawSlotContent(draw, skin, normalized.Slots[i], items, slotBounds, pixelScale);
        }
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
        var pixelScale = PixelScale(draw.Scale);
        var slotSize = PreferredSlotSize(draw.Scale, skinId);
        var gap = PreferredGap(draw.Scale);
        var visibleSlots = Math.Min(normalized.Slots.Count, normalized.Capacity);

        for (var i = 0; i < visibleSlots; i++)
        {
            var column = i % Math.Max(1, columns);
            var row = i / Math.Max(1, columns);
            var slotBounds = new Rectangle(
                bounds.X + column * (slotSize.X + gap),
                bounds.Y + row * (slotSize.Y + gap),
                slotSize.X,
                slotSize.Y);
            var selected = i == normalized.SelectedHotbarIndex;
            DrawSlotFrame(draw, skin, slotBounds, selected, pixelScale);

            if (normalized.Slots[i].IsEmpty)
            {
                draw.CenteredText("-", slotBounds, draw.Theme.MutedText, 0.72f);
                continue;
            }

            DrawSlotContent(draw, skin, normalized.Slots[i], items, slotBounds, pixelScale);
        }
    }

    public Point PreferredSlotSize(float uiScale, string skinId = InventoryUiAssetIds.TravelerBackpackSkin)
    {
        var scale = PixelScale(uiScale);
        var asset = _assets.FindAsset(skinId, InventoryUiAssetIds.SlotHolder);
        return new Point(
            Math.Max(1, (asset?.Width ?? 48) * scale),
            Math.Max(1, (asset?.Height ?? 48) * scale));
    }

    public int PreferredGap(float uiScale)
    {
        return Math.Max(4, 6 * PixelScale(uiScale));
    }

    private void DrawBackpackIcon(UiDrawContext draw, InventoryUiSkin skin, Rectangle bounds, Point slotSize, int gap, int pixelScale)
    {
        var asset = skin?.FindAsset(InventoryUiAssetIds.BackpackIdle);
        var texture = _textures?.GetTexture(asset);
        if (texture == null)
        {
            return;
        }

        var width = Math.Max(1, (asset?.Width ?? texture.Width) * pixelScale);
        var height = Math.Max(1, (asset?.Height ?? texture.Height) * pixelScale);
        var destination = new Rectangle(bounds.X - gap - width, bounds.Y + (slotSize.Y - height) / 2, width, height);
        draw.SpriteBatch.Draw(texture, destination, Source(asset, texture), Color.White);
    }

    private void DrawSlotFrame(UiDrawContext draw, InventoryUiSkin skin, Rectangle slotBounds, bool selected, int pixelScale)
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
            draw.Border(slotBounds, selected ? draw.Theme.Accent : draw.Theme.Border, selected ? 3 : 2);
        }

        if (!selected)
        {
            return;
        }

        var highlightAsset = skin?.FindAsset(InventoryUiAssetIds.SlotHighlight);
        var highlightTexture = _textures?.GetTexture(highlightAsset);
        if (highlightTexture != null)
        {
            var width = Math.Max(1, (highlightAsset?.Width ?? highlightTexture.Width) * pixelScale);
            var height = Math.Max(1, (highlightAsset?.Height ?? highlightTexture.Height) * pixelScale);
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
        int pixelScale)
    {
        var definition = items?.Find(slot.ItemId);
        var iconAssetId = string.IsNullOrWhiteSpace(definition?.IconAssetId)
            ? "backpack_icon_unknown"
            : definition.IconAssetId;
        var iconAsset = skin?.FindAsset(iconAssetId);
        var iconTexture = _textures?.GetTexture(iconAsset);
        if (iconTexture != null)
        {
            var iconSize = Math.Max(1, (iconAsset?.Width ?? 16) * pixelScale);
            var iconBounds = new Rectangle(
                slotBounds.X + (slotBounds.Width - iconSize) / 2,
                slotBounds.Y + (slotBounds.Height - iconSize) / 2,
                iconSize,
                iconSize);
            draw.SpriteBatch.Draw(iconTexture, iconBounds, Source(iconAsset, iconTexture), Color.White);
        }
        else
        {
            draw.CenteredText(ShortItemLabel(draw, definition, slot.ItemId), new Rectangle(slotBounds.X + 4, slotBounds.Y + 12, slotBounds.Width - 8, slotBounds.Height - 20), draw.Theme.Text, 0.5f);
        }

        if (slot.Quantity > 1)
        {
            draw.RightAlignedText(slot.Quantity.ToString(CultureInfo.InvariantCulture), slotBounds.Right - 7, slotBounds.Bottom - 20, draw.Theme.Accent, 0.52f);
        }
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

    private static int PixelScale(float uiScale)
    {
        return Math.Clamp((int)Math.Round(uiScale), 1, 4);
    }

    private static string ShortItemLabel(UiDrawContext draw, ItemDefinition definition, string itemId)
    {
        var label = definition == null ? itemId : draw.Localization.Text(definition.LabelKey);
        return label.Length <= 10 ? label : label[..10];
    }
}
