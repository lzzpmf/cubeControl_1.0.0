using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cube.Desktop.Studio.Controls;

/// <summary>Fills a finite host, reducing one logical DAW surface when space is scarce.</summary>
public sealed class StudioFitPanel : Decorator
{
    public const double MinimumLogicalWidth = 720;
    public const double MinimumLogicalHeight = 424;
    private readonly ScaleTransform _scale = new();

    public StudioFitPanel() => ClipToBounds = true;

    private static (Size Logical, double Scale) Fit(Size available)
    {
        var width = double.IsFinite(available.Width) ? Math.Max(0, available.Width) : MinimumLogicalWidth;
        var height = double.IsFinite(available.Height) ? Math.Max(0, available.Height) : MinimumLogicalHeight;
        var scale = Math.Min(1, Math.Min(width / MinimumLogicalWidth, height / MinimumLogicalHeight));
        return scale <= 0 ? (new(MinimumLogicalWidth, MinimumLogicalHeight), 0)
            : (new(width / scale, height / scale), scale);
    }

    protected override Size MeasureOverride(Size constraint)
    {
        var (logical, scale) = Fit(constraint);
        Child?.Measure(logical);
        return new(logical.Width * scale, logical.Height * scale);
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        var (logical, scale) = Fit(arrangeSize);
        if (Child is { } child)
        {
            _scale.ScaleX = _scale.ScaleY = scale;
            child.RenderTransform = _scale;
            child.Arrange(new Rect(logical));
        }
        return arrangeSize;
    }
}
