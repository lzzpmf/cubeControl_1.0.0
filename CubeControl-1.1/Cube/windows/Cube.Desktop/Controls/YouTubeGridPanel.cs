using System.Windows;
using System.Windows.Controls;

namespace Cube.Desktop.Controls;

public sealed class YouTubeGridPanel : Panel
{
    private int columns = 1;
    private double rowHeight;
    protected override Size MeasureOverride(Size available)
    {
        double width = double.IsFinite(available.Width) ? available.Width : 676;
        columns = Math.Max(1, (int)((width + 8) / 228));
        double cell = Math.Max(0, (width - (columns - 1) * 8) / columns);
        rowHeight = 0;
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(new Size(cell, double.PositiveInfinity));
            rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);
        }
        int rows = (InternalChildren.Count + columns - 1) / columns;
        return new Size(width, rows * rowHeight + Math.Max(0, rows - 1) * 8);
    }
    protected override Size ArrangeOverride(Size final)
    {
        double cell = Math.Max(0, (final.Width - (columns - 1) * 8) / columns);
        for (int i = 0; i < InternalChildren.Count; i++)
            InternalChildren[i].Arrange(new Rect(i % columns * (cell + 8), i / columns * (rowHeight + 8), cell, rowHeight));
        return final;
    }
}

// Measure the thumbnail from its available width, including before the first arrange.
public sealed class VideoThumbnailPanel : Grid
{
    protected override Size MeasureOverride(Size available)
    {
        double width = double.IsFinite(available.Width) ? available.Width : 220;
        var size = new Size(width, width * 9 / 16);
        base.MeasureOverride(size);
        return size;
    }
}
