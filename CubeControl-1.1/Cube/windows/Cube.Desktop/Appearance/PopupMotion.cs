using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using Cube.Desktop.Controls;
using Wpf.Ui.Controls;

namespace Cube.Desktop.Appearance;

internal static class PopupMotion
{
    private static readonly DependencyProperty StateProperty =
        DependencyProperty.RegisterAttached("State", typeof(DialogState), typeof(PopupMotion));

    internal static void Initialize()
    {
        // Popup surfaces animate through SurfaceMotion, without a second native fade.
        EventManager.RegisterClassHandler(typeof(Popup), FrameworkElement.LoadedEvent,
            new RoutedEventHandler((sender, args) =>
            {
                if (!ReferenceEquals(sender, args.OriginalSource)) return;
                var popup = (Popup)sender;
                popup.SetCurrentValue(Popup.PopupAnimationProperty,
                    PopupAnimation.None);
            }));
        EventManager.RegisterClassHandler(typeof(ContentDialog), FrameworkElement.LoadedEvent,
            new RoutedEventHandler((sender, args) =>
            {
                if (!ReferenceEquals(sender, args.OriginalSource)) return;
                var dialog = (ContentDialog)sender;
                if (dialog.GetValue(StateProperty) is not DialogState state)
                {
                    state = new DialogState(dialog);
                    dialog.SetValue(StateProperty, state);
                }
                state.Open();
            }));
        EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent,
            new RoutedEventHandler((sender, args) =>
            {
                if (!ReferenceEquals(sender, args.OriginalSource)) return;
                // Native window open/close/minimize/restore stays with the compositor.
                var handle = new WindowInteropHelper((Window)sender).Handle;
                if (handle == IntPtr.Zero) return;
                int disabled = 0;
                DwmSetWindowAttribute(handle, 3 /* DWMWA_TRANSITIONS_FORCEDISABLED */, ref disabled, sizeof(int));
            }));
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);

    private sealed class DialogState
    {
        private readonly ContentDialog dialog;
        internal DialogState(ContentDialog dialog) => this.dialog = dialog;
        internal void Open()
        {
            dialog.ApplyTemplate();
            if (dialog.Template.FindName("DialogRootGrid", dialog) is FrameworkElement surface)
                SurfaceMotion.SetEnabled(surface, true);
        }
    }
}
