using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace Cube.Desktop.Appearance;

internal static class WindowWorkArea
{
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo { public int Size; public NativeRect Monitor, Work; public uint Flags; }
    [DllImport("user32.dll")]
    private static extern nint MonitorFromWindow(nint hwnd, uint flags);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(nint monitor, ref MonitorInfo info);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(nint hwnd, out NativeRect rect);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(nint hwnd, nint after, int x, int y, int width, int height, uint flags);

    internal static Size Fit(Size requested, Size available) =>
        new(Math.Min(requested.Width, available.Width), Math.Min(requested.Height, available.Height));

    public static void Attach(Window window)
    {
        var source = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
        bool pending = false;
        (nint Monitor, NativeRect Work, double X, double Y)? previous = null;
        void Refresh()
        {
            var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
            var monitor = MonitorFromWindow(new WindowInteropHelper(window).Handle, 2);
            if (!GetMonitorInfo(monitor, ref info)) return;
            var dpi = VisualTreeHelper.GetDpi(window);
            var current = (monitor, info.Work, dpi.DpiScaleX, dpi.DpiScaleY);
            // Do not constrain position on each drag: users must be able to cross monitors.
            if (previous.HasValue && previous.Value.Equals(current)) return;
            previous = current;
            Update(window, info, dpi);
        }
        void Schedule()
        {
            if (pending) return;
            pending = true;
            window.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                pending = false;
                if (window.IsLoaded) Refresh();
            }));
        }
        nint Hook(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
        {
            // Re-evaluate after WPF has applied DPI and suggested window bounds.
            if (message is 0x02E0 or 0x007E or 0x001A or 0x0047) Schedule();
            return 0;
        }
        source?.AddHook(Hook);
        window.StateChanged += (_, _) => { previous = null; Schedule(); };
        window.Closed += (_, _) => source?.RemoveHook(Hook);
        window.Loaded += (_, _) => Refresh();
        Refresh();
    }

    internal static Size Minimum(Size available) => Fit(new Size(1280, 720), available);

    private static void Update(Window window, MonitorInfo info, DpiScale dpi)
    {
        var available = new Size((info.Work.Right - info.Work.Left) / dpi.DpiScaleX,
            (info.Work.Bottom - info.Work.Top) / dpi.DpiScaleY);
        var minimum = Minimum(available);
        window.MinWidth = minimum.Width;
        window.MinHeight = minimum.Height;
        if (window.WindowState != WindowState.Normal) return;
        window.Width = Math.Clamp(window.Width, minimum.Width, available.Width);
        window.Height = Math.Clamp(window.Height, minimum.Height, available.Height);
        // Screen origins on mixed-DPI desktops must stay in physical pixels.
        var handle = new WindowInteropHelper(window).Handle;
        if (GetWindowRect(handle, out var rect))
        {
            int x = Math.Clamp(rect.Left, info.Work.Left, Math.Max(info.Work.Left, info.Work.Right - (rect.Right - rect.Left)));
            int y = Math.Clamp(rect.Top, info.Work.Top, Math.Max(info.Work.Top, info.Work.Bottom - (rect.Bottom - rect.Top)));
            if (x != rect.Left || y != rect.Top) SetWindowPos(handle, 0, x, y, 0, 0, 0x0015); // no size/z-order/activation
        }
    }
}
