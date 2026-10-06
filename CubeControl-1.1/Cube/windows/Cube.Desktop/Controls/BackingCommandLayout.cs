using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cube.Desktop.Controls;

// Equal side columns keep playback on the card's center axis.
public sealed class BackingCommandLayout : Panel
{
    private bool wide;
    private const double Gap = 16;
    public BackingCommandLayout() => SetResourceReference(SeparatorBrushProperty, "CubeBorder");
    private static readonly DependencyProperty SeparatorBrushProperty = DependencyProperty.Register(
        "SeparatorBrush", typeof(Brush), typeof(BackingCommandLayout), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        if (InternalChildren.Count != 3 || GetValue(SeparatorBrushProperty) is not Brush brush) return;
        var pen = new Pen(brush, 1);
        var middle = InternalChildren[1];
        if (wide)
        {
            double edge = (ActualWidth - middle.DesiredSize.Width) / 2;
            foreach (double x in new[] { edge - Gap / 2, ActualWidth - edge + Gap / 2 })
                dc.DrawLine(pen, new Point(x, 8), new Point(x, Math.Max(8, ActualHeight - 8)));
        }
        else
        {
            double y = middle.DesiredSize.Height + Gap / 2;
            dc.DrawLine(pen, new Point(8, y), new Point(Math.Max(8, ActualWidth - 8), y));
        }
    }
    protected override Size MeasureOverride(Size available)
    {
        if (InternalChildren.Count != 3) return new Size();
        foreach (UIElement child in InternalChildren)
            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double middle = InternalChildren[1].DesiredSize.Width;
        double side = Math.Max(InternalChildren[0].DesiredSize.Width, InternalChildren[2].DesiredSize.Width);
        wide = available.Width >= middle + 2 * (side + Gap);
        if (wide) return new Size(middle + 2 * (side + Gap), InternalChildren.Cast<UIElement>().Max(c => c.DesiredSize.Height));
        foreach (UIElement child in InternalChildren)
            child.Measure(new Size(available.Width, double.PositiveInfinity));
        return new Size(InternalChildren.Cast<UIElement>().Max(c => c.DesiredSize.Width),
            InternalChildren.Cast<UIElement>().Sum(c => c.DesiredSize.Height) + 2 * Gap);
    }
    protected override Size ArrangeOverride(Size final)
    {
        if (InternalChildren.Count != 3) return final;
        double center = InternalChildren[1].DesiredSize.Width;
        if (wide)
        {
            double side = (final.Width - center) / 2 - Gap;
            InternalChildren[0].Arrange(new Rect(0, 0, side, final.Height));
            InternalChildren[1].Arrange(new Rect(side + Gap, 0, center, final.Height));
            InternalChildren[2].Arrange(new Rect(side + center + 2 * Gap, 0, side, final.Height));
        }
        else
        {
            double y = 0;
            foreach (int index in new[] { 1, 0, 2 })
            {
                var child = InternalChildren[index];
                double width = Math.Min(final.Width, child.DesiredSize.Width);
                child.Arrange(new Rect((final.Width - width) / 2, y, width, child.DesiredSize.Height));
                y += child.DesiredSize.Height + Gap;
            }
        }
        return final;
    }
}
