using DuskVillage.Screens;
using Microsoft.Xna.Framework;

namespace DuskVillage.UI;

public interface IUiTooltipProvider
{
    bool TryGetTooltip(GameScreenContext context, out string tooltipKey, out Point anchor);
}
