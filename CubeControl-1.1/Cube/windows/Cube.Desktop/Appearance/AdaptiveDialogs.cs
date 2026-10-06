using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace Cube.Desktop.Appearance;

internal static class AdaptiveDialogs
{
    // Retain WPF UI's commands, focus handling and content scroller; reduce chrome
    // only when the scaled host cannot accommodate the desktop spacing.
    internal static IDisposable Attach(ContentDialog dialog) => new State(dialog);
    private sealed class State : IDisposable
    {
        private readonly ContentDialog dialog;
        private readonly FrameworkElement? host;
        private readonly List<Action> restore = new();
        private readonly HashSet<(DependencyObject, DependencyProperty)> changed = new();
        private bool compact, disposed;
        public State(ContentDialog dialog)
        {
            this.dialog = dialog;
            host = dialog.DialogHostEx;
            dialog.Loaded += Loaded;
            if (host != null) host.SizeChanged += Resized;
        }
        private void Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string name in new[] { "PrimaryButton", "SecondaryButton", "CloseButton" })
                if (dialog.Template.FindName(name, dialog) is Wpf.Ui.Controls.Button button)
                {
                    button.SetResourceReference(FrameworkElement.StyleProperty, "CubeButtonStyle");
                    button.Margin = new Thickness(0);
                    button.VerticalAlignment = VerticalAlignment.Stretch;
                }
            // Keep text out of the shadow's off-screen bitmap, especially at fractional zoom.
            if (dialog.Template.FindName("DialogRootGrid", dialog) is Grid root
                && root.Children.OfType<Border>().FirstOrDefault() is Border surface
                && surface.Effect != null)
            {
                var shadow = new Border { Effect = surface.Effect.Clone(), IsHitTestVisible = false };
                shadow.SetBinding(Border.BackgroundProperty, new Binding(nameof(Border.Background)) { Source = surface });
                shadow.SetBinding(Border.CornerRadiusProperty, new Binding(nameof(Border.CornerRadius)) { Source = surface });
                if (shadow.Effect is System.Windows.Media.Effects.DropShadowEffect effect)
                    effect.Opacity = ThemeService.Instance.IsHighContrast ? 0 : .4;
                var layer = new Grid { IsHitTestVisible = false, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                layer.SetBinding(FrameworkElement.WidthProperty, new Binding(nameof(FrameworkElement.ActualWidth)) { Source = surface });
                layer.SetBinding(FrameworkElement.HeightProperty, new Binding(nameof(FrameworkElement.ActualHeight)) { Source = surface });
                layer.SetBinding(FrameworkElement.MarginProperty, new Binding(nameof(FrameworkElement.Margin)) { Source = surface });
                layer.Children.Add(shadow);
                surface.Effect = null;
                root.Children.Insert(0, layer);
                dialog.UseLayoutRounding = true;
                dialog.SnapsToDevicePixels = true;
            }
            Queue();
        }
        private void Resized(object sender, SizeChangedEventArgs e) => Queue();
        private void Queue() => dialog.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(Update));
        private void Set(DependencyObject target, DependencyProperty property, object value)
        {
            if (changed.Add((target, property)))
            {
                object original = target.GetValue(property);
                restore.Add(() => target.SetCurrentValue(property, original));
            }
            target.SetCurrentValue(property, value);
        }
        private void Update()
        {
            if (disposed || !dialog.IsLoaded || host == null) return;
            bool needed = host.ActualWidth < 500 || host.ActualHeight < 400;
            if (!needed && compact)
            {
                foreach (var undo in restore) undo();
                restore.Clear(); changed.Clear(); compact = false;
                if (dialog.Template.FindName("DialogRootGrid", dialog) is Grid normalRoot && normalRoot.Children.OfType<Border>().FirstOrDefault() is Border normalSurface)
                {
                    // Restore live sizing, not the pixel snapshot from the old small host.
                    if (dialog.Content is Views.GearGallery)
                    {
                        var margin = dialog.DialogMargin;
                        normalSurface.Width = normalSurface.MaxWidth = Math.Max(0, Math.Min(Math.Min(740, dialog.DialogMaxWidth), host.ActualWidth - margin.Left - margin.Right));
                        normalSurface.Height = normalSurface.MaxHeight = Math.Max(0, Math.Min(750, host.ActualHeight - margin.Top - margin.Bottom));
                    }
                    else
                    {
                        normalSurface.Width = normalSurface.Height = double.NaN;
                        normalSurface.SetBinding(FrameworkElement.MaxWidthProperty, new Binding(nameof(ContentDialog.DialogWidth)) { Source = dialog });
                        normalSurface.SetBinding(FrameworkElement.MaxHeightProperty, new Binding(nameof(ContentDialog.DialogHeight)) { Source = dialog });
                    }
                }
            }
            if (!needed) return;
            if (dialog.Template.FindName("PART_ContentScroll", dialog) is not ScrollViewer body
                || VisualTreeHelper.GetParent(body) is not Grid contentGrid
                || dialog.Template.FindName("DialogRootGrid", dialog) is not Grid root
                || root.Children.OfType<Border>().FirstOrDefault() is not Border surface) return;
            if (!compact)
            {
                compact = true;
                Set(dialog, ContentDialog.DialogMarginProperty, new Thickness(8));
                Set(dialog, Control.PaddingProperty, new Thickness(12, 8, 12, 8));
                Set(contentGrid, FrameworkElement.MarginProperty, new Thickness(12, 8, 12, 0));
                foreach (ContentPresenter title in contentGrid.Children.OfType<ContentPresenter>())
                    Set(title, FrameworkElement.MarginProperty, new Thickness(0));
                Set(body, FrameworkElement.MarginProperty, new Thickness(0, 8, 0, 8));
                Set(body, Control.PaddingProperty, new Thickness(0));
                foreach (string name in new[] { "PrimaryButton", "SecondaryButton", "CloseButton" })
                {
                    if (dialog.Template.FindName(name, dialog) is not Wpf.Ui.Controls.Button button) continue;
                    // An absent secondary action must not reserve a third of the footer.
                    if (button.Visibility != Visibility.Visible)
                    {
                        if (VisualTreeHelper.GetParent(button) is Grid unusedActions)
                            Set(unusedActions.ColumnDefinitions[Grid.GetColumn(button)], ColumnDefinition.WidthProperty, new GridLength(0));
                        continue;
                    }
                    Set(button, FrameworkElement.MinWidthProperty, 0d);
                    Set(button, Control.PaddingProperty, new Thickness(12, 8, 12, 8));
                    var text = new FrameworkElementFactory(typeof(System.Windows.Controls.TextBlock));
                    text.SetBinding(System.Windows.Controls.TextBlock.TextProperty, new Binding());
                    text.SetValue(System.Windows.Controls.TextBlock.TextWrappingProperty, TextWrapping.Wrap);
                    text.SetValue(System.Windows.Controls.TextBlock.TextAlignmentProperty, TextAlignment.Center);
                    Set(button, ContentControl.ContentTemplateProperty, new DataTemplate { VisualTree = text });
                    if (VisualTreeHelper.GetParent(button) is Grid actions)
                    {
                        Set(actions, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
                        Set(actions.ColumnDefinitions[Grid.GetColumn(button)], ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
                    }
                }
                Set(surface, FrameworkElement.WidthProperty, double.NaN);
                Set(surface, FrameworkElement.HeightProperty, double.NaN);
                Set(surface, FrameworkElement.MaxWidthProperty, Math.Max(0, host.ActualWidth - 16));
                Set(surface, FrameworkElement.MaxHeightProperty, Math.Max(0, host.ActualHeight - 16));
            }
            double width = Math.Max(0, Math.Min(dialog.DialogMaxWidth, host.ActualWidth - 16));
            double height = Math.Max(0, Math.Min(dialog.DialogMaxHeight, host.ActualHeight - 16));
            surface.SetCurrentValue(FrameworkElement.WidthProperty, width);
            surface.SetCurrentValue(FrameworkElement.MaxWidthProperty, width);
            surface.SetCurrentValue(FrameworkElement.HeightProperty, height);
            surface.SetCurrentValue(FrameworkElement.MaxHeightProperty, height);
        }
        public void Dispose()
        {
            disposed = true;
            dialog.Loaded -= Loaded;
            if (host != null) host.SizeChanged -= Resized;
        }
    }
}
