using DuskVillage.Screens;
using Microsoft.Xna.Framework;

namespace DuskVillage.UI;

public abstract class UiControlBase : IUiControl
{
    public Rectangle Bounds { get; set; }

    public Rectangle? PointerClipBounds { get; set; }

    public bool IsEnabled { get; set; } = true;

    public virtual bool CanFocus => IsEnabled;

    public abstract void Update(GameScreenContext context, bool hasFocus);

    public abstract void Draw(UiDrawContext draw, bool hasFocus);

    protected bool IsHovered(GameScreenContext context)
    {
        if (PointerClipBounds.HasValue && !PointerClipBounds.Value.Contains(context.Input.Current.MousePosition))
        {
            return false;
        }

        return Bounds.Contains(context.Input.Current.MousePosition);
    }

    protected bool WasClicked(GameScreenContext context)
    {
        return IsEnabled && IsHovered(context) && context.Input.Current.LeftClickStarted;
    }

    protected Color TextColor(UiDrawContext draw, bool hasFocus)
    {
        if (!IsEnabled)
        {
            return draw.Theme.DisabledText;
        }

        return hasFocus ? draw.Theme.Accent : draw.Theme.Text;
    }
}
