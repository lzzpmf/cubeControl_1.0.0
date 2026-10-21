using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using GongSolutions.Wpf.DragDrop;
using GongDrag = GongSolutions.Wpf.DragDrop.DragDrop;

namespace Cube.Desktop.Controls;

// Adapts Gong's gesture to the existing domain operations. Never uses DefaultDropHandler:
// playlist indices can be filtered, while IR slots exchange contents, not containers.
internal sealed class CubeDragDrop : DefaultDragHandler, IDropTarget
{
    private readonly FrameworkElement host;
    private readonly Func<IDragInfo, (object? Data, DragDropEffects Effect)>? source;
    private readonly Action<object, CubeDropContext>? over;
    private readonly Action<object, CubeDropContext>? drop;
    private UIElement? faded;
    private CubeDragDrop(FrameworkElement host, Func<IDragInfo, (object?, DragDropEffects)>? source,
        Action<object, CubeDropContext>? over, Action<object, CubeDropContext>? drop)
    {
        this.host = host; this.source = source; this.over = over; this.drop = drop;
        if (source != null) { GongDrag.SetDragHandler(host, this); GongDrag.SetIsDragSource(host, true); }
        if (over != null)
        {
            GongDrag.SetDropHandler(host, this);
            GongDrag.SetDropEventType(host, EventType.Bubbled);
            GongDrag.SetIsDropTarget(host, true);
            GongDrag.SetDropScrollingMode(host, ScrollingMode.None);
        }
        host.Unloaded += (_, _) => { Finish(); DragMotion.Clear(host); };
    }
    internal static CubeDragDrop Attach(FrameworkElement host,
        Func<IDragInfo, (object? Data, DragDropEffects Effect)>? source = null,
        Action<object, CubeDropContext>? over = null, Action<object, CubeDropContext>? drop = null)
        => new(host, source, over, drop);

    internal static bool IsInteractive(DependencyObject? node, DependencyObject boundary)
    {
        while (node != null && node != boundary)
        {
            if (node is ButtonBase or TextBoxBase or PasswordBox or Slider or ComboBox or Thumb
                or Knob or BackingClipLane or BackingWaveform or BackingGainControl or BackingFader) return true;
            node = node is Visual ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node);
        }
        return false;
    }
    public override bool CanStartDrag(IDragInfo info) => info.MouseButton == MouseButton.Left &&
        !IsInteractive(host.InputHitTest(info.DragStartPosition) as DependencyObject, host) && source?.Invoke(info).Data != null;
    public override void StartDrag(IDragInfo info)
    {
        Finish(); DragMotion.Clear(host);
        var value = source?.Invoke(info) ?? (null, DragDropEffects.None);
        if (value.Item1 == null) { info.Effects = DragDropEffects.None; return; }
        info.Data = value.Item1; info.Effects = value.Item2;
        var visual = info.VisualSourceItem as FrameworkElement ?? host;
        var bitmap = DragMotion.Snapshot(visual);
        if (bitmap != null)
        {
            var image = new FrameworkElementFactory(typeof(Image));
            image.SetValue(Image.SourceProperty, bitmap);
            image.SetValue(FrameworkElement.WidthProperty, visual.ActualWidth);
            image.SetValue(FrameworkElement.HeightProperty, visual.ActualHeight);
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetResourceReference(Border.BackgroundProperty, "CubeFloatingSurface");
            border.SetResourceReference(Border.CornerRadiusProperty, "CubeCardCornerRadius");
            border.SetValue(UIElement.OpacityProperty, SystemParameters.HighContrast ? 1d : .75);
            border.SetValue(UIElement.RenderTransformOriginProperty, new Point(.5, .5));
            if (SurfaceMotion.Allowed) border.SetValue(UIElement.RenderTransformProperty, new ScaleTransform(1.03, 1.03));
            if (!SystemParameters.HighContrast) border.SetValue(UIElement.EffectProperty,
                new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 12, ShadowDepth = 3, Opacity = .2 });
            border.AppendChild(image);
            GongDrag.SetDragAdornerTemplate(host, new DataTemplate { VisualTree = border });
        }
        if (info.Effects == DragDropEffects.Move && !SystemParameters.HighContrast)
        {
            faded = visual;
            visual.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(.35, .35, TimeSpan.Zero));
        }
    }
    internal void RestoreSource()
    {
        faded?.BeginAnimation(UIElement.OpacityProperty, null); faded = null;
    }
    private void Finish()
    {
        RestoreSource(); DragFeedback.Clear();
        host.ClearValue(GongDrag.DragAdornerTemplateProperty);
    }
    public override void DragCancelled() => Finish();
    public override void DragDropOperationFinished(DragDropEffects result, IDragInfo info) => Finish();
    public override bool TryCatchOccurredException(Exception exception) { Finish(); return false; }
    public void DragEnter(IDropInfo info) => DragOver(info);
    public void DragOver(IDropInfo info)
    {
        info.NotHandled = false;
        var context = new CubeDropContext(info);
        over?.Invoke(host, context);
    }
    public void DragLeave(IDropInfo info) => DragFeedback.Clear();
    public void Drop(IDropInfo info)
    {
        info.NotHandled = false;
        if (info.DragInfo?.VisualSource is { } visual && GongDrag.GetDragHandler(visual) is CubeDragDrop origin) origin.RestoreSource();
        DragFeedback.Clear();
        try { drop?.Invoke(host, new CubeDropContext(info)); }
        catch { DragFeedback.Clear(); DragMotion.Clear(host); throw; }
    }
}

internal sealed class CubeDropContext
{
    private readonly IDropInfo info;
    internal CubeDropContext(IDropInfo info)
    {
        this.info = info;
        Data = info.Data as IDataObject ?? (info.Data == null ? new DataObject() : new DataObject(info.Data.GetType(), info.Data));
        OriginalSource = info.VisualTarget.InputHitTest(info.DropPosition) as DependencyObject ?? info.VisualTarget;
    }
    public IDataObject Data { get; }
    public DependencyObject OriginalSource { get; }
    public DragDropEffects Effects { get => info.Effects; set => info.Effects = value; }
    public bool Handled { set => info.NotHandled = !value; }
    public Point GetPosition(UIElement element) => info.VisualTarget.TranslatePoint(info.DropPosition, element);
}
