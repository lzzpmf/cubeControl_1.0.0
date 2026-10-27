using System.Windows;
using System.Windows.Controls;

namespace Cube.Desktop.Controls;

// Equal-width signal-chain tiles with a predictable reading order on narrow windows.
public sealed class StagePanel : Panel
{
    private int columns = 6;
    private double rowHeight;
    protected override Size MeasureOverride(Size available)
    {
        double width = double.IsInfinity(available.Width) ? 1100 : available.Width;
        // Include the tile's padding and margin, not only its text, in the minimum.
        // Six modules always form complete rows (6, 3, 2 or 1 columns).
        columns = width >= 1080 ? 6 : width >= 540 ? 3 : width >= 360 ? 2 : 1;
        rowHeight = 0;
        foreach (UIElement child in InternalChildren)
        { child.Measure(new Size(width / columns, double.PositiveInfinity)); rowHeight = Math.Max(rowHeight, child.DesiredSize.Height); }
        return new Size(width, Math.Ceiling((double)InternalChildren.Count / columns) * rowHeight);
    }
    protected override Size ArrangeOverride(Size final)
    {
        for (int i = 0; i < InternalChildren.Count; i++)
            InternalChildren[i].Arrange(new Rect(i % columns * final.Width / columns, i / columns * rowHeight, final.Width / columns, rowHeight));
        return final;
    }
}
