using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cube.Desktop.Controls;

public sealed class AdaptivePanel : Panel
{
    public static readonly DependencyProperty FirstShareProperty = DependencyProperty.Register(nameof(FirstShare), typeof(double), typeof(AdaptivePanel), new FrameworkPropertyMetadata(.4, FrameworkPropertyMetadataOptions.AffectsMeasure));
    public static readonly DependencyProperty BreakpointProperty = DependencyProperty.Register(nameof(Breakpoint), typeof(double), typeof(AdaptivePanel), new FrameworkPropertyMetadata(900d, FrameworkPropertyMetadataOptions.AffectsMeasure));
    public double FirstShare { get => (double)GetValue(FirstShareProperty); set => SetValue(FirstShareProperty, value); }
    public double Breakpoint { get => (double)GetValue(BreakpointProperty); set => SetValue(BreakpointProperty, value); }
    public bool ControlsFirstWhenStacked { get; set; }
    private bool stacked;
    protected override Size MeasureOverride(Size available)
    {
        double width = double.IsInfinity(available.Width) ? 1100 : available.Width;
        stacked = width < Breakpoint;
        double height = 0;
        for (int i = 0; i < InternalChildren.Count; i++)
        {
            var child = InternalChildren[i]; child.Measure(new(stacked ? width : width * (i == 0 ? FirstShare : 1 - FirstShare), double.PositiveInfinity));
            KeyboardNavigation.SetTabIndex(child, stacked && ControlsFirstWhenStacked ? InternalChildren.Count - 1 - i : i);
            KeyboardNavigation.SetTabNavigation(child, KeyboardNavigationMode.Local);
            height = stacked ? height + child.DesiredSize.Height : Math.Max(height, child.DesiredSize.Height);
        }
        return new(width, height);
    }
    protected override Size ArrangeOverride(Size final)
    {
        double y = 0;
        for (int i = 0; i < InternalChildren.Count; i++)
        {
            var child = InternalChildren[stacked && ControlsFirstWhenStacked ? InternalChildren.Count - 1 - i : i];
            child.Arrange(stacked ? new Rect(0, y, final.Width, child.DesiredSize.Height) : new Rect(i == 0 ? 0 : final.Width * FirstShare, 0, final.Width * (i == 0 ? FirstShare : 1 - FirstShare), final.Height));
            y += child.DesiredSize.Height;
        }
        return final;
    }
}
