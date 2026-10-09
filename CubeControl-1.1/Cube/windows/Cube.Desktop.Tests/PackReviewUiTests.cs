using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cube.Desktop;
using Cube.Desktop.Appearance;
using Cube.Desktop.Controls;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;
using Cube.Protocol;
using ThemeMode = Cube.Desktop.Appearance.ThemeMode;

internal static class PackReviewUiTests
{
    private static IEnumerable<T> Find<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T item) yield return item;
            foreach (var nested in Find<T>(child)) yield return nested;
        }
    }
    public static async Task Run()
    {
        using var model = new EditorViewModel(Path.Combine(Path.GetTempPath(), "cube-review-ui-" + Guid.NewGuid().ToString("N")));
        using var fake = new FakeConnection(); using var session = new CubeSession(fake); await session.Identify();
        await model.Execute(() => model.AttachSession(session)); model.Ask = _ => Task.FromResult(false);
        var oldWindow = Application.Current.MainWindow;
        var theme = ThemeService.Instance.Mode; var language = LanguageService.Instance.Language; var zoom = ZoomService.Instance.Percent;
        var window = new MainWindow(model, initializeDevices: false) { Width = 1200, Height = 850, ShowInTaskbar = false };
        Application.Current.MainWindow = window; window.Show();
        ((TabControl)window.FindName("Tabs")).SelectedIndex = 1;
        string folder = "C:/CubeControl-Build/reports/ir-pack-flow"; Directory.CreateDirectory(folder);
        try
        {
            foreach (var (code, percent, mode, width, highContrast) in new[] {
                ("pt-BR", 100, ThemeMode.Dark, 1200, false), ("en-US", 100, ThemeMode.Light, 1200, false),
                ("pt-BR", 50, ThemeMode.Dark, 640, false), ("en-US", 200, ThemeMode.Light, 640, false),
                ("pt-BR", 100, ThemeMode.Dark, 1200, true) })
            {
                LanguageService.Instance.Preview(code); ZoomService.Instance.Preview(percent); ThemeService.Instance.Preview(mode);
                if (highContrast) Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.HighContrast);
                window.Width = width; window.Height = width == 640 ? 480 : 850;
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout();
                var pending = model.Execute(() => (Task)typeof(EditorViewModel).GetMethod("ApplyIrPackAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(model, null)!);
                var host = (Wpf.Ui.Controls.ContentDialogHost)window.FindName("DialogHost");
                Wpf.Ui.Controls.ContentDialog? dialog = null;
                for (int attempt = 0; attempt < 150 && dialog == null && !pending.IsCompleted; attempt++)
                {
                    await Task.Delay(50); window.UpdateLayout();
                    dialog = Find<Wpf.Ui.Controls.ContentDialog>(host).FirstOrDefault();
                }
                if (dialog == null) throw new Exception("Pack review did not open: " + model.ErrorMessage);
                await Task.Delay(400); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout();
                var content = Find<IrPackReview>(dialog).Single();
                var review = (IrPackReviewViewModel)content.DataContext;
                var grid = Find<IrPackGridPanel>(content).Single();
                if (grid.ActualWidth >= 260 && grid.Children.Cast<UIElement>().Any(c => c.RenderSize.Width < 260))
                    throw new Exception("Review compressed cabinet cards below their minimum width");
                var body = Find<ScrollViewer>(dialog).Single(s => s.Name == "PART_ContentScroll");
                if (body.ViewportHeight < 48 || body.ScrollableWidth > 1 || content.ActualWidth > body.ViewportWidth + 1)
                    throw new Exception($"Review viewport clipped at {code}/{percent}: {body.ViewportWidth} x {body.ViewportHeight}");
                var knobs = Find<Knob>(content).ToArray();
                if (knobs.Length != 8 || knobs.Any(k => !k.IsEnabled)) throw new Exception("Review must allow adjusting all eight IRs during processing lock");
                var first = Find<CheckBox>(content).First(); first.Focus();
                if (!first.IsKeyboardFocusWithin) throw new Exception("Review selection cannot receive keyboard focus");
                first.SetCurrentValue(CheckBox.IsCheckedProperty, false);
                if (review.Slots[0].Selected) throw new Exception("Review checkbox did not update the snapshot");
                knobs[1].SetCurrentValue(Knob.ValueProperty, 23d);
                if (review.Slots[1].Distance != 23 || model.PackEditor.Slots[1].Entry!.Distance != 50)
                    throw new Exception("Review knob mutated the pack or failed to update its snapshot");
                foreach (string name in new[] { "PrimaryButton", "CloseButton" })
                {
                    var button = (FrameworkElement)dialog.Template.FindName(name, dialog);
                    var bounds = button.TransformToAncestor(host).TransformBounds(new Rect(button.RenderSize));
                    if (!button.IsEnabled || bounds.Left < 0 || bounds.Right > host.ActualWidth + 1 || bounds.Bottom > host.ActualHeight + 1)
                        throw new Exception("Review action clipped: " + name);
                }
                void Capture(string suffix)
                {
                    var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(window); var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
                    string target = Path.Combine(folder, $"review-{code}-{percent}-{mode}-{highContrast}-{suffix}.png");
                    using var output = File.Create(target); png.Save(output); Console.WriteLine("SCREENSHOT " + target);
                }
                body.ScrollToTop(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout();
                Capture("start"); body.ScrollToEnd(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout(); Capture("end");
                dialog.Hide(); await pending;
                if (fake.Writes != 0 || model.PackEditor.Dirty) throw new Exception("Closing review wrote device state or changed the pack");
                Console.WriteLine($"PASS pack review viewport, keyboard focus, snapshot controls and cancellation at {code}/{percent}/{mode}/{highContrast}");
            }
        }
        finally
        {
            window.Close(); Application.Current.MainWindow = oldWindow;
            ThemeService.Instance.Preview(theme); LanguageService.Instance.Preview(language); ZoomService.Instance.Preview(zoom);
        }
    }
}
