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
    public static readonly DependencyProperty FirstMaxWidthProperty = DependencyProperty.Register(nameof(FirstMaxWidth), typeof(double), typeof(AdaptivePanel), new FrameworkPropertyMetadata(double.PositiveInfinity, FrameworkPropertyMetadataOptions.AffectsMeasure));
    public static readonly DependencyProperty FirstMinWidthProperty = DependencyProperty.Register(nameof(FirstMinWidth), typeof(double), typeof(AdaptivePanel), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure));
    public double FirstMaxWidth { get => (double)GetValue(FirstMaxWidthProperty); set => SetValue(FirstMaxWidthProperty, value); }
    public double FirstMinWidth { get => (double)GetValue(FirstMinWidthProperty); set => SetValue(FirstMinWidthProperty, value); }
    private double FirstWidth(double width) => Math.Min(width, Math.Clamp(width * FirstShare, FirstMinWidth, FirstMaxWidth));
    public bool ControlsFirstWhenStacked { get; set; }
    private bool stacked;
    protected override Size MeasureOverride(Size available)
    {
        double width = double.IsInfinity(available.Width) ? 1100 : available.Width;
        stacked = width < Breakpoint;
        double height = 0;
        for (int i = 0; i < InternalChildren.Count; i++)
        {
            var child = InternalChildren[i]; child.Measure(new(stacked ? width : (i == 0 ? FirstWidth(width) : width - FirstWidth(width)), double.PositiveInfinity));
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
            child.Arrange(stacked ? new Rect(0, y, final.Width, child.DesiredSize.Height) : new Rect(i == 0 ? 0 : FirstWidth(final.Width), 0, i == 0 ? FirstWidth(final.Width) : final.Width - FirstWidth(final.Width), final.Height));
            y += child.DesiredSize.Height;
        }
        return final;
    }
}
