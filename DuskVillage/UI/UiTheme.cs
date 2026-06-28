using Microsoft.Xna.Framework;

namespace DuskVillage.UI;

public sealed class UiTheme
{
    public static UiTheme Default { get; } = new();

    public Color BackgroundTop { get; } = new(19, 22, 24);

    public Color BackgroundBottom { get; } = new(32, 36, 34);

    public Color Panel { get; } = new(36, 39, 39, 230);

    public Color PanelAlt { get; } = new(49, 52, 50, 240);

    public Color Border { get; } = new(151, 139, 111);

    public Color Accent { get; } = new(195, 167, 95);

    public Color AccentMuted { get; } = new(104, 124, 117);

    public Color Text { get; } = new(236, 232, 218);

    public Color MutedText { get; } = new(174, 169, 153);

    public Color DisabledText { get; } = new(105, 105, 98);

    public Color Warning { get; } = new(217, 128, 86);
}
