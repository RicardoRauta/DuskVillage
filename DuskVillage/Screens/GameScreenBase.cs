using DuskVillage.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DuskVillage.Screens;

public abstract class GameScreenBase : IGameScreen
{
    protected GameScreenBase(GameScreenContext context)
    {
        Context = context;
    }

    protected GameScreenContext Context { get; }

    protected float UiScale => Context.Settings.Current.Display.UiScale;

    public virtual void OnEnter()
    {
    }

    public virtual void OnExit()
    {
    }

    public abstract void Update(GameTime gameTime);

    public abstract void Draw(GameTime gameTime);

    protected string T(string key, params object[] args)
    {
        return Context.Localization.Text(key, args);
    }

    protected UiDrawContext BeginUi(RasterizerState rasterizerState = null)
    {
        Context.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, rasterizerState: rasterizerState);
        return new UiDrawContext(
            Context.SpriteBatch,
            Context.Font,
            Context.Pixel,
            Context.Localization,
            UiTheme.Default,
            UiScale);
    }

    protected void EndUi()
    {
        Context.SpriteBatch.End();
    }

    protected bool BackRequested()
    {
        var input = Context.Input.Current;
        return input.WasKeyPressed(Keys.Escape) ||
            input.WasKeyPressed(Context.Settings.Current.Input.Back) ||
            input.GamePadCancelPressedFor(Context.Settings.Current.Input.ControllerBack);
    }

    protected void DrawBackdrop(UiDrawContext draw)
    {
        var bounds = Context.ViewBounds;
        draw.Fill(bounds, draw.Theme.BackgroundTop);
        draw.Fill(new Rectangle(0, bounds.Height / 2, bounds.Width, bounds.Height / 2), draw.Theme.BackgroundBottom);
    }

    protected void DrawScreenTitle(UiDrawContext draw, string titleKey, string subtitleKey = "")
    {
        var bounds = Context.ViewBounds;
        var title = T(titleKey);
        var titleSize = Context.Font.MeasureString(title) * UiScale * 1.9f;
        draw.Text(
            title,
            new Vector2((bounds.Width - titleSize.X) / 2f, 46),
            draw.Theme.Text,
            1.9f);

        if (!string.IsNullOrWhiteSpace(subtitleKey))
        {
            var subtitle = T(subtitleKey);
            var subtitleSize = Context.Font.MeasureString(subtitle) * UiScale;
            draw.Text(
                subtitle,
                new Vector2((bounds.Width - subtitleSize.X) / 2f, 96),
                draw.Theme.MutedText);
        }
    }

    protected static int CenterX(Rectangle bounds, int width)
    {
        return bounds.X + (bounds.Width - width) / 2;
    }
}
