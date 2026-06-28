using DuskVillage.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DuskVillage.UI;

public sealed class UiDrawContext
{
    public UiDrawContext(
        SpriteBatch spriteBatch,
        SpriteFont font,
        Texture2D pixel,
        ILocalizationService localization,
        UiTheme theme,
        float scale)
    {
        SpriteBatch = spriteBatch;
        Font = font;
        Pixel = pixel;
        Localization = localization;
        Theme = theme;
        Scale = scale;
    }

    public SpriteBatch SpriteBatch { get; }

    public SpriteFont Font { get; }

    public Texture2D Pixel { get; }

    public ILocalizationService Localization { get; }

    public UiTheme Theme { get; }

    public float Scale { get; }

    public void Fill(Rectangle rectangle, Color color)
    {
        SpriteBatch.Draw(Pixel, rectangle, color);
    }

    public void Border(Rectangle rectangle, Color color, int thickness = 2)
    {
        Fill(new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
        Fill(new Rectangle(rectangle.X, rectangle.Bottom - thickness, rectangle.Width, thickness), color);
        Fill(new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
        Fill(new Rectangle(rectangle.Right - thickness, rectangle.Y, thickness, rectangle.Height), color);
    }

    public void Text(string text, Vector2 position, Color color, float scaleMultiplier = 1f)
    {
        SpriteBatch.DrawString(Font, text, position, color, 0f, Vector2.Zero, Scale * scaleMultiplier, SpriteEffects.None, 0f);
    }

    public void RightAlignedText(string text, float rightX, float y, Color color, float scaleMultiplier = 1f)
    {
        var textSize = Font.MeasureString(text) * Scale * scaleMultiplier;
        Text(text, new Vector2(rightX - textSize.X, y), color, scaleMultiplier);
    }

    public void CenteredText(string text, Rectangle rectangle, Color color, float scaleMultiplier = 1f)
    {
        var textSize = Font.MeasureString(text) * Scale * scaleMultiplier;
        var position = new Vector2(
            rectangle.X + (rectangle.Width - textSize.X) / 2f,
            rectangle.Y + (rectangle.Height - textSize.Y) / 2f);

        Text(text, position, color, scaleMultiplier);
    }
}
