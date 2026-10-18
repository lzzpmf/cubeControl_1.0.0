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
        await PackCompactUiTests.Run();
        using var model = new EditorViewModel(Path.Combine(Path.GetTempPath(), "cube-review-ui-" + Guid.NewGuid().ToString("N")));
        using var fake = new FakeConnection(); using var session = new CubeSession(fake); await session.Identify();
        await model.Execute(() => model.AttachSession(session)); model.Ask = _ => Task.FromResult(false);
        var oldWindow = Application.Current.MainWindow;
        var theme = ThemeService.Instance.Mode; var language = LanguageService.Instance.Language; var zoom = ZoomService.Instance.Percent;
        var window = new MainWindow(model, initializeDevices: false) { Width = 1200, Height = 850, ShowInTaskbar = false };
        Application.Current.MainWindow = window; window.Show();
        ((TabControl)window.FindName("Tabs")).SelectedIndex = 1;
        ((TabControl)window.FindName("IrWorkspaceTabs")).SelectedIndex = 1;
        string folder = TestPaths.Report("C:/CubeControl-Build/reports/ir-pack-flow"); Directory.CreateDirectory(folder);
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
                if (Find<IrPackReview>(dialog).Any() || Find<Knob>(dialog).Any())
                    throw new Exception("Apply confirmation must keep editing on the pack cards");
                if (dialog.Content is not TextBlock message || string.IsNullOrWhiteSpace(message.Text))
                    throw new Exception("Apply confirmation must describe the destination slots");
                var body = Find<ScrollViewer>(dialog).Single(s => s.Name == "PART_ContentScroll");
                if (body.ViewportHeight < 20 || body.ScrollableWidth > 1)
                    throw new Exception($"Confirmation viewport clipped at {code}/{percent}");
                var close = (Control)dialog.Template.FindName("CloseButton", dialog);
                close.Focus();
                if (!close.IsKeyboardFocusWithin) throw new Exception("Confirmation cannot receive keyboard focus");
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
                Console.WriteLine($"PASS pack review viewport, keyboard focus, confirmation controls and cancellation at {code}/{percent}/{mode}/{highContrast}");
            }
        }
        finally
        {
            window.Close(); Application.Current.MainWindow = oldWindow;
            ThemeService.Instance.Preview(theme); LanguageService.Instance.Preview(language); ZoomService.Instance.Preview(zoom);
        }
    }
}
