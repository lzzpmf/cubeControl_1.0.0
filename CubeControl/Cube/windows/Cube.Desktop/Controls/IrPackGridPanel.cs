using System.Windows;
using System.Windows.Controls;

namespace Cube.Desktop.Controls;

// Eight slots retain their reading order at every width, without compressed cards.
public sealed class IrPackGridPanel : Panel
{
    private const double Gap = 12;
    public static readonly DependencyProperty MinimumCardWidthProperty = DependencyProperty.Register(
        nameof(MinimumCardWidth), typeof(double), typeof(IrPackGridPanel),
        new FrameworkPropertyMetadata(260d, FrameworkPropertyMetadataOptions.AffectsMeasure),
        value => value is double width && double.IsFinite(width) && width > 0);
    public double MinimumCardWidth { get => (double)GetValue(MinimumCardWidthProperty); set => SetValue(MinimumCardWidthProperty, value); }
    private int columns;
    private double[] rowHeights = [];
    protected override Size MeasureOverride(Size available)
    {
        double width = double.IsInfinity(available.Width) ? MinimumCardWidth * 4 + Gap * 3 : available.Width;
        columns = width >= MinimumCardWidth * 4 + Gap * 3 ? 4 : width >= MinimumCardWidth * 2 + Gap ? 2 : 1;
        double cellWidth = Math.Max(0, (width - Gap * (columns - 1)) / columns);
        int rows = (InternalChildren.Count + columns - 1) / columns;
        rowHeights = new double[rows];
        for (int i = 0; i < InternalChildren.Count; i++)
        {
            UIElement child = InternalChildren[i];
            child.Measure(new Size(cellWidth, double.PositiveInfinity));
            int row = i / columns;
            rowHeights[row] = Math.Max(rowHeights[row], child.DesiredSize.Height);
        }
        double height = Math.Max(0, rows - 1) * Gap;
        foreach (double rowHeight in rowHeights) height += rowHeight;
        return new Size(width, height);
    }
    protected override Size ArrangeOverride(Size final)
    {
        double cellWidth = Math.Max(0, (final.Width - Gap * (columns - 1)) / columns);
        double top = 0;
        for (int row = 0; row < rowHeights.Length; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                int index = row * columns + column;
                if (index >= InternalChildren.Count) break;
                InternalChildren[index].Arrange(new Rect(column * (cellWidth + Gap), top, cellWidth, rowHeights[row]));
            }
            top += rowHeights[row] + Gap;
        }
        return final;
    }
}
