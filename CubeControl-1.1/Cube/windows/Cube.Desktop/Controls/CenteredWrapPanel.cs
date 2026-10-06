using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cube.Desktop.Controls;

// Keep functional groups intact and center every wrapped row independently.
public sealed class CenteredWrapPanel : Panel
{
    public static readonly DependencyProperty SeparatorBrushProperty = DependencyProperty.Register(
        nameof(SeparatorBrush), typeof(Brush), typeof(CenteredWrapPanel),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
    public Brush? SeparatorBrush { get => (Brush?)GetValue(SeparatorBrushProperty); set => SetValue(SeparatorBrushProperty, value); }
    private readonly List<(Point Start, Point End)> separators = new();

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        if (SeparatorBrush is not { } brush) return;
        var pen = new Pen(brush, 1);
        foreach (var line in separators) drawingContext.DrawLine(pen, line.Start, line.End);
    }

    protected override Size MeasureOverride(Size available)
    {
        double width = 0, height = 0, rowWidth = 0, rowHeight = 0;
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(new Size(available.Width, double.PositiveInfinity));
            var size = child.DesiredSize;
            if (rowWidth > 0 && rowWidth + size.Width > available.Width)
            { width = Math.Max(width, rowWidth); height += rowHeight + 12; rowWidth = rowHeight = 0; }
            rowWidth += size.Width; rowHeight = Math.Max(rowHeight, size.Height);
        }
        return new Size(Math.Min(available.Width, Math.Max(width, rowWidth)), height + rowHeight);
    }
    protected override Size ArrangeOverride(Size final)
    {
        var previousSeparators = separators.ToArray();
        separators.Clear();
        var row = new List<UIElement>(); double width = 0, height = 0, y = 0;
        void Flush()
        {
            double x = Math.Max(0, (final.Width - width) / 2);
            bool first = true;
            foreach (var child in row) {
                if (!first) separators.Add((new Point(x, y + 8), new Point(x, y + Math.Max(8, height - 8))));
                first = false;
                child.Arrange(new Rect(x, y, Math.Min(child.DesiredSize.Width, final.Width), height)); x += child.DesiredSize.Width; }
            y += height + 12; row.Clear(); width = height = 0;
        }
        foreach (UIElement child in InternalChildren)
        {
            if (row.Count > 0 && width + child.DesiredSize.Width > final.Width) Flush();
            row.Add(child); width += child.DesiredSize.Width; height = Math.Max(height, child.DesiredSize.Height);
        }
        if (row.Count > 0) Flush();
        if (!previousSeparators.SequenceEqual(separators)) InvalidateVisual();
        return final;
    }
}
