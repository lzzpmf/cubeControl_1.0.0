using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Cube.Desktop.Controls;
using Wpf.Ui.Controls;

namespace Cube.Desktop.Appearance;

internal static class PopupMotion
{
    private static readonly DependencyProperty StateProperty =
        DependencyProperty.RegisterAttached("State", typeof(DialogState), typeof(PopupMotion));

    internal static void Initialize()
    {
        // Let WPF animate detached menus, tooltips and dropdowns using system preferences.
        EventManager.RegisterClassHandler(typeof(Popup), FrameworkElement.LoadedEvent,
            new RoutedEventHandler((sender, args) =>
            {
                if (!ReferenceEquals(sender, args.OriginalSource)) return;
                var popup = (Popup)sender;
                popup.SetCurrentValue(Popup.PopupAnimationProperty,
                    SystemParameters.ClientAreaAnimation && !SystemParameters.HighContrast
                        ? PopupAnimation.Fade : PopupAnimation.None);
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
        private FrameworkElement? surface;
        private readonly ScaleTransform scale = new(1, 1);
        private bool closing, completing;
        private int generation;

        internal DialogState(ContentDialog dialog)
        {
            this.dialog = dialog;
            dialog.Closing += Closing;
            dialog.Unloaded += (_, _) => { generation++; Reset(); };
        }

        internal void Open()
        {
            generation++;
            Reset();
            dialog.ApplyTemplate();
            surface = dialog.Template.FindName("DialogRootGrid", dialog) as FrameworkElement;
            if (surface == null || !SurfaceMotion.Allowed) return;
            surface.RenderTransformOrigin = new Point(.5, .5);
            surface.RenderTransform = scale;
            Animate(true);
        }

        private void Animate(bool opening)
        {
            if (surface == null) return;
            var duration = TimeSpan.FromMilliseconds(opening ? 200 : 120);
            var ease = new CubicEase { EasingMode = opening ? EasingMode.EaseOut : EasingMode.EaseIn };
            surface.BeginAnimation(UIElement.OpacityProperty,
                new DoubleAnimation(opening ? 0 : surface.Opacity, opening ? 1 : 0, duration) { EasingFunction = ease });
            foreach (var property in new[] { ScaleTransform.ScaleXProperty, ScaleTransform.ScaleYProperty })
                scale.BeginAnimation(property,
                    new DoubleAnimation(opening ? .96 : (double)scale.GetValue(property), opening ? 1 : .96, duration)
                    { EasingFunction = ease });
        }

        private void Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (completing || args.Cancel || surface == null || !SurfaceMotion.Allowed) return;
            args.Cancel = true;
            if (closing) return;
            closing = true;
            int current = generation;
            var result = args.Result;
            Animate(false);
            var timer = new DispatcherTimer(DispatcherPriority.Render, dialog.Dispatcher)
            { Interval = TimeSpan.FromMilliseconds(120) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                if (current != generation || !dialog.IsLoaded) return;
                completing = true;
                try { dialog.Hide(result); }
                finally
                {
                    completing = false;
                    // A different Closing handler may veto dismissal.
                    Reset();
                }
            };
            timer.Start();
        }

        private void Reset()
        {
            closing = false;
            surface?.BeginAnimation(UIElement.OpacityProperty, null);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        }
    }
}