using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Cube.Desktop.Controls;

internal static class DragMotion
{
    private static readonly List<Flight> flights = [];
    internal static int ActiveCount => flights.Count;
    internal static BitmapSource? Snapshot(FrameworkElement element)
    {
        if (!element.IsVisible || element.ActualWidth < 1 || element.ActualHeight < 1) return null;
        var dpi = VisualTreeHelper.GetDpi(element);
        var image = new RenderTargetBitmap((int)Math.Ceiling(element.ActualWidth * dpi.DpiScaleX),
            (int)Math.Ceiling(element.ActualHeight * dpi.DpiScaleY), dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32);
        image.Render(element); image.Freeze(); return image;
    }
    internal static void Clear(FrameworkElement host)
    {
        foreach (var flight in flights.Where(x => x.Host == host || host.IsAncestorOf(x.Host) || x.Host.IsAncestorOf(host)).ToArray()) flight.Stop();
    }
    private static Rect Bounds(FrameworkElement host, FrameworkElement item) => item.TransformToAncestor(host).TransformBounds(new Rect(item.RenderSize));
    private static bool Visible(FrameworkElement host, FrameworkElement item) => item.IsVisible &&
        ClipBounds(host).IntersectsWith(Bounds(host, item));
    private static Rect ClipBounds(FrameworkElement host)
    {
        var bounds = new Rect(host.RenderSize);
        for (DependencyObject? node = VisualTreeHelper.GetParent(host); node != null; node = VisualTreeHelper.GetParent(node))
            if (node is FrameworkElement ancestor && (ancestor.ClipToBounds || ancestor is ScrollContentPresenter))
                bounds.Intersect(ancestor.TransformToDescendant(host).TransformBounds(new Rect(ancestor.RenderSize)));
        return bounds;
    }
    internal static Action CaptureList(ItemsControl list)
    {
        Clear(list);
        if (!SurfaceMotion.Allowed) return () => { };
        var collection = list.ItemsSource;
        var old = new Dictionary<object, Rect>(ReferenceEqualityComparer.Instance);
        foreach (var item in list.Items)
            if (list.ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement container && Visible(list, container))
                old[item] = Bounds(list, container);
        return () =>
        {
            list.UpdateLayout();
            if (!list.IsLoaded || !SurfaceMotion.Allowed || !ReferenceEquals(collection, list.ItemsSource)) return;
            foreach (var item in list.Items)
                if (list.ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement container && Visible(list, container))
                {
                    var destination = Bounds(list, container);
                    if (old.TryGetValue(item, out var origin) && origin != destination && Snapshot(container) is { } bitmap)
                        Play(list, container, bitmap, origin, destination);
                    else if (!old.ContainsKey(item)) Settle(container);
                }
        };
    }
    internal static bool Swap(FrameworkElement host, FrameworkElement source, FrameworkElement target, Func<bool> commit)
    {
        Clear(host);
        var animate = SurfaceMotion.Allowed && Visible(host, source) && Visible(host, target);
        var first = animate ? Snapshot(source) : null;
        var second = animate ? Snapshot(target) : null;
        var from = animate ? Bounds(host, source) : Rect.Empty;
        var to = animate ? Bounds(host, target) : Rect.Empty;
        if (!commit()) return false;
        host.UpdateLayout();
        if (first != null && second != null)
        {
            Play(host, target, first, from, to);
            Play(host, source, second, to, from);
        }
        return true;
    }
    internal static void Settle(FrameworkElement target)
    {
        Clear(target);
        if (!SurfaceMotion.Allowed || !target.IsLoaded) return;
        if (Snapshot(target) is not { } bitmap) return;
        var to = new Rect(target.RenderSize);
        var from = new Rect(-to.Width * .015, -to.Height * .015, to.Width * 1.03, to.Height * 1.03);
        Play(target, target, bitmap, from, to);
    }
    private static void Play(FrameworkElement host, FrameworkElement target, BitmapSource bitmap, Rect from, Rect to)
    {
        if (AdornerLayer.GetAdornerLayer(host) is { } layer)
        {
            var flight = new Flight(host, target, bitmap, from, to, layer);
            flights.Add(flight);
            try { flight.Start(); }
            catch { flight.Stop(); throw; }
        }
    }
    private sealed class Flight : Adorner
    {
        public FrameworkElement Host { get; }
        private readonly FrameworkElement target;
        private readonly BitmapSource bitmap;
        private readonly Rect from, to;
        private readonly AdornerLayer layer;
        private readonly System.Windows.Threading.DispatcherTimer cleanup;
        private bool stopped;
        private static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress", typeof(double), typeof(Flight),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));
        public Flight(FrameworkElement host, FrameworkElement target, BitmapSource bitmap, Rect from, Rect to, AdornerLayer layer) : base(host)
        {
            Host = host; this.target = target; this.bitmap = bitmap; this.from = from; this.to = to; this.layer = layer;
            IsHitTestVisible = false;
            cleanup = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Render, host.Dispatcher)
                { Interval = TimeSpan.FromMilliseconds(240) };
            cleanup.Tick += (_, _) => Stop();
        }
        public void Start()
        {
            target.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 0, TimeSpan.Zero));
            target.Unloaded += UnloadedTarget; Host.Unloaded += UnloadedTarget;
            Host.IsVisibleChanged += VisibilityChanged;
            SystemParameters.StaticPropertyChanged += SettingsChanged;
            layer.Add(this);
            var animation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            animation.Completed += (_, _) => Stop();
            BeginAnimation(ProgressProperty, animation);
            cleanup.Start();
        }
        private void UnloadedTarget(object sender, RoutedEventArgs e) => Stop();
        private void VisibilityChanged(object sender, DependencyPropertyChangedEventArgs e) { if (!Host.IsVisible) Stop(); }
        private void SettingsChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) { if (!SurfaceMotion.Allowed) Stop(); }
        public void Stop()
        {
            if (stopped) return; stopped = true;
            cleanup.Stop();
            BeginAnimation(ProgressProperty, null); target.BeginAnimation(OpacityProperty, null);
            layer.Remove(this); flights.Remove(this);
            target.Unloaded -= UnloadedTarget; Host.Unloaded -= UnloadedTarget;
            Host.IsVisibleChanged -= VisibilityChanged;
            SystemParameters.StaticPropertyChanged -= SettingsChanged;
        }
        protected override void OnRender(DrawingContext dc)
        {
            double p = (double)GetValue(ProgressProperty);
            dc.PushClip(new RectangleGeometry(ClipBounds(Host)));
            dc.DrawImage(bitmap, new Rect(from.X + (to.X - from.X) * p, from.Y + (to.Y - from.Y) * p,
                from.Width + (to.Width - from.Width) * p, from.Height + (to.Height - from.Height) * p));
            dc.Pop();
        }
    }
}
