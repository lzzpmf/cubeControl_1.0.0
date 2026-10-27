using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace Cube.Desktop.Controls;

// Enabled on the ComboBox style and forwarded to its popup by the template.
// Each popup owns its handlers, so replacing a template cannot retain the old owner.
public static class ComboBoxPopupPlacement
{
    public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
        "Enabled", typeof(bool), typeof(ComboBoxPopupPlacement), new PropertyMetadata(false, Changed));
    private static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
        "State", typeof(State), typeof(ComboBoxPopupPlacement));
    public static bool GetEnabled(DependencyObject target) => (bool)target.GetValue(EnabledProperty);
    public static void SetEnabled(DependencyObject target, bool value) => target.SetValue(EnabledProperty, value);

    private static void Changed(DependencyObject target, DependencyPropertyChangedEventArgs args)
    {
        if (target is not Popup popup) return;
        if (popup.GetValue(StateProperty) is State previous) previous.Dispose();
        popup.ClearValue(StateProperty);
        if ((bool)args.NewValue) popup.SetValue(StateProperty, new State(popup));
    }

    private sealed class State
    {
        private readonly Popup popup;
        private CustomPopupPlacementCallback? previousCallback;
        private DispatcherOperation? pending;
        private Point? anchor;
        private bool attached;
        private ScrollViewer? viewer;
        private double originalMaxHeight;

        internal State(Popup popup)
        {
            this.popup = popup;
            popup.Loaded += Loaded;
            popup.Unloaded += Unloaded;
            if (popup.IsLoaded) Attach();
        }

        private void Loaded(object sender, RoutedEventArgs args) => Attach();
        private void Unloaded(object sender, RoutedEventArgs args) => Detach();
        private void Attach()
        {
            if (attached) return;
            attached = true;
            previousCallback = popup.CustomPopupPlacementCallback;
            popup.CustomPopupPlacementCallback = Place;
            popup.Opened += Opened;
            popup.Closed += Closed;
            if (popup.IsOpen) Opened(popup, EventArgs.Empty);
        }

        private void Opened(object? sender, EventArgs args)
        {
            Reset();
            pending = popup.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(Prepare));
        }

        private void Prepare()
        {
            pending = null;
            if (!popup.IsOpen || popup.TemplatedParent is not ComboBox owner ||
                popup.Child is not FrameworkElement root || popup.PlacementTarget is not FrameworkElement target)
                return;

            root.UpdateLayout();
            viewer = Find<ScrollViewer>(root);
            if (viewer != null)
            {
                originalMaxHeight = viewer.MaxHeight;
                var work = WorkArea(target);
                // PointToScreen includes the window DPI and ancestor zoom transforms.
                var origin = target.PointToScreen(new Point());
                var unit = target.PointToScreen(new Point(0, 1));
                double scale = Math.Max(.01, Math.Abs(unit.Y - origin.Y));
                double chrome = Math.Max(0, root.ActualHeight - viewer.ActualHeight);
                viewer.SetCurrentValue(FrameworkElement.MaxHeightProperty,
                    Math.Max(1, Math.Min(owner.MaxDropDownHeight, work.Height / scale - chrome)));
                root.UpdateLayout();
            }

            int index = owner.SelectedIndex >= 0 ? owner.SelectedIndex : 0;
            var item = index < owner.Items.Count
                ? owner.ItemContainerGenerator.ContainerFromIndex(index) as ComboBoxItem : null;
            if (item != null)
            {
                item.BringIntoView();
                root.UpdateLayout();
                item.ApplyTemplate();
                var text = item.Template.FindName("PART_ContentPresenter", item) as FrameworkElement ?? item;
                var row = item.Template.FindName("ContentBorder", item) as FrameworkElement ?? item;
                Point textOrigin = text.TranslatePoint(new Point(), root);
                Point rowCenter = row.TranslatePoint(new Point(0, row.ActualHeight / 2), root);
                var field = owner.Template.FindName(owner.IsEditable ? "PART_EditableTextBox" : "PART_ContentPresenter", owner) as FrameworkElement;
                double fieldX = field?.TranslatePoint(new Point(), target).X ?? owner.Padding.Left;
                anchor = new Point(fieldX - textOrigin.X, target.ActualHeight / 2 - rowCenter.Y);
            }
            // Request one placement refresh. Subsequent keyboard/scroll changes keep this anchor.
            double offset = popup.HorizontalOffset;
            popup.SetCurrentValue(Popup.HorizontalOffsetProperty, offset + .01);
            popup.SetCurrentValue(Popup.HorizontalOffsetProperty, offset);
        }

        private CustomPopupPlacement[] Place(Size popupSize, Size targetSize, Point offset)
        {
            Point point = anchor ?? new Point(-30, targetSize.Height / 2 - 26);
            if (popup.PlacementTarget is FrameworkElement target && PresentationSource.FromVisual(target) != null)
            {
                Rect work = WorkArea(target);
                Point origin = target.PointToScreen(new Point());
                Point unit = target.PointToScreen(new Point(1, 1));
                double sx = Math.Max(.01, Math.Abs(unit.X - origin.X));
                double sy = Math.Max(.01, Math.Abs(unit.Y - origin.Y));
                double left = (work.Left - origin.X) / sx, top = (work.Top - origin.Y) / sy;
                point.X = Math.Clamp(point.X, left, Math.Max(left, (work.Right - origin.X) / sx - popupSize.Width));
                point.Y = Math.Clamp(point.Y, top, Math.Max(top, (work.Bottom - origin.Y) / sy - popupSize.Height));
            }
            return [new CustomPopupPlacement(point, PopupPrimaryAxis.None)];
        }

        private void Closed(object? sender, EventArgs args) => Reset();
        private void Reset()
        {
            pending?.Abort();
            pending = null;
            anchor = null;
            if (viewer != null)
            {
                viewer.SetCurrentValue(FrameworkElement.MaxHeightProperty, originalMaxHeight);
            }
            viewer = null;
        }

        private void Detach()
        {
            Reset();
            if (!attached) return;
            popup.Opened -= Opened;
            popup.Closed -= Closed;
            popup.CustomPopupPlacementCallback = previousCallback;
            attached = false;
        }

        internal void Dispose()
        {
            Detach();
            popup.Loaded -= Loaded;
            popup.Unloaded -= Unloaded;
        }
    }

    private static T? Find<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match) return match;
            if (Find<T>(child) is { } descendant) return descendant;
        }
        return null;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint { public int X, Y; }
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo { public int Size; public NativeRect Monitor, Work; public uint Flags; }
    [DllImport("user32.dll")]
    private static extern nint MonitorFromPoint(NativePoint point, uint flags);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(nint monitor, ref MonitorInfo info);

    private static Rect WorkArea(FrameworkElement target)
    {
        Point center = target.PointToScreen(new Point(target.ActualWidth / 2, target.ActualHeight / 2));
        var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
        if (GetMonitorInfo(MonitorFromPoint(new NativePoint { X = (int)center.X, Y = (int)center.Y }, 2), ref info))
            return new Rect(info.Work.Left, info.Work.Top, info.Work.Right - info.Work.Left, info.Work.Bottom - info.Work.Top);
        var dpi = VisualTreeHelper.GetDpi(target);
        Rect fallback = SystemParameters.WorkArea;
        return new Rect(fallback.X * dpi.DpiScaleX, fallback.Y * dpi.DpiScaleY,
            fallback.Width * dpi.DpiScaleX, fallback.Height * dpi.DpiScaleY);
    }
}
