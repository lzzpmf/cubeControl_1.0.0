using System.Windows;
using System.Windows.Controls;

namespace Cube.Desktop.Controls;

// Eight slots retain their reading order at every width, without compressed cards.
public sealed class IrPackGridPanel : Panel
{
    private const double Gap = 12;
    private int columns;
    private double rowHeight;
    protected override Size MeasureOverride(Size available)
    {
        double width = double.IsInfinity(available.Width) ? 1076 : available.Width;
        columns = width >= 1076 ? 4 : width >= 532 ? 2 : 1;
        double cellWidth = Math.Max(0, (width - Gap * (columns - 1)) / columns);
        rowHeight = 0;
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(new Size(cellWidth, double.PositiveInfinity));
            rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);
        }
        int rows = (InternalChildren.Count + columns - 1) / columns;
        return new Size(width, rows * rowHeight + Math.Max(0, rows - 1) * Gap);
    }
    protected override Size ArrangeOverride(Size final)
    {
        double cellWidth = Math.Max(0, (final.Width - Gap * (columns - 1)) / columns);
        for (int i = 0; i < InternalChildren.Count; i++)
            InternalChildren[i].Arrange(new Rect(i % columns * (cellWidth + Gap), i / columns * (rowHeight + Gap), cellWidth, rowHeight));
        return final;
    }
}
