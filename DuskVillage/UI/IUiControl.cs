using DuskVillage.Screens;
using Microsoft.Xna.Framework;

namespace DuskVillage.UI;

public interface IUiControl
{
    Rectangle Bounds { get; set; }

    Rectangle? PointerClipBounds { get; set; }

    bool IsEnabled { get; set; }

    bool CanFocus { get; }

    void Update(GameScreenContext context, bool hasFocus);

    void Draw(UiDrawContext draw, bool hasFocus);
}
