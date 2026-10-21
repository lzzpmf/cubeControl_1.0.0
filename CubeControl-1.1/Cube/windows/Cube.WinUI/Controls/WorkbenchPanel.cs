using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Cube.WinUI.Controls;

/// <summary>Responsive instrument rack; keeps source/tab order and never clips narrow windows.</summary>
public sealed class WorkbenchPanel : Panel
{
    public double CellWidth { get; set; } = 460;
    public int MaximumColumns { get; set; } = 2;
    public double Gap { get; set; } = 16;
    public static readonly DependencyProperty FullWidthProperty = DependencyProperty.RegisterAttached("FullWidth", typeof(bool), typeof(WorkbenchPanel), new PropertyMetadata(false));
    public static void SetFullWidth(DependencyObject element, bool value) => element.SetValue(FullWidthProperty, value);
    public static bool GetFullWidth(DependencyObject element) => (bool)element.GetValue(FullWidthProperty);
    protected override Size MeasureOverride(Size availableSize) => Layout(availableSize.Width, false);
    protected override Size ArrangeOverride(Size finalSize) { Layout(finalSize.Width, true); return finalSize; }
    private Size Layout(double width, bool arrange)
    {
        if (!double.IsFinite(width)) width = CellWidth;
        int columns = Math.Clamp((int)((width + Gap) / (CellWidth + Gap)), 1, MaximumColumns);
        columns = Math.Min(columns, Math.Max(1, Children.Count(child => child.Visibility != Visibility.Collapsed)));
        double cell = Math.Max(0, (width - Gap * (columns - 1)) / columns), y = 0, rowHeight = 0; int column = 0;
        foreach (var child in Children)
        {
            if (child.Visibility == Visibility.Collapsed) continue;
            bool full = GetFullWidth(child);
            if (full && column > 0) { y += rowHeight + Gap; column = 0; rowHeight = 0; }
            double childWidth = full ? width : cell;
            if (!arrange) child.Measure(new Size(childWidth, double.PositiveInfinity));
            if (arrange) child.Arrange(new Rect(column * (cell + Gap), y, childWidth, child.DesiredSize.Height));
            rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);
            if (full || ++column == columns) { y += rowHeight + Gap; column = 0; rowHeight = 0; }
        }
        return new Size(width, column > 0 ? y + rowHeight : Math.Max(0, y - Gap));
    }
}
