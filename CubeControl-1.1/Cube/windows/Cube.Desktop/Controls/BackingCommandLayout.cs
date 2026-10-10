using System.Windows;
using System.Windows.Controls;

namespace Cube.Desktop.Controls;

// Pack complete groups in reading order with equal spacing and aligned row heights.
public sealed class BackingCommandLayout : Panel
{
    private const double Gap = 16;
    private IEnumerable<List<UIElement>> Rows(double width)
    {
        var row = new List<UIElement>();
        double used = 0;
        foreach (UIElement child in InternalChildren)
        {
            if (child.Visibility == Visibility.Collapsed) continue;
            double next = child.DesiredSize.Width + (row.Count > 0 ? Gap : 0);
            if (row.Count > 0 && used + next > width)
            {
                yield return row;
                row = new List<UIElement>();
                used = 0;
            }
            used += child.DesiredSize.Width + (row.Count > 0 ? Gap : 0);
            row.Add(child);
        }
        if (row.Count > 0) yield return row;
    }

    protected override Size MeasureOverride(Size available)
    {
        foreach (UIElement child in InternalChildren)
            child.Measure(new Size(available.Width, double.PositiveInfinity));
        double width = 0, height = 0;
        foreach (var row in Rows(available.Width))
        {
            width = Math.Max(width, row.Sum(c => c.DesiredSize.Width) + Gap * (row.Count - 1));
            height += (height > 0 ? Gap : 0) + row.Max(c => c.DesiredSize.Height);
        }
        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size final)
    {
        double y = 0;
        foreach (var row in Rows(final.Width))
        {
            double height = row.Max(c => c.DesiredSize.Height);
            double width = row.Sum(c => c.DesiredSize.Width) + Gap * (row.Count - 1);
            double spacing = row.Count > 1 ? Gap + Math.Max(0, final.Width - width) / (row.Count - 1) : Gap;
            double x = row.Count == 1 ? Math.Max(0, (final.Width - width) / 2) : 0;
            foreach (var child in row)
            {
                child.Arrange(new Rect(x, y, child.DesiredSize.Width, height));
                x += child.DesiredSize.Width + spacing;
            }
            y += height + Gap;
        }
        return final;
    }

}
