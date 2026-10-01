using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cube.Desktop;
using Cube.Desktop.ViewModels;
using Cube.Protocol;

internal static class ProgressPresentationTests
{
    public static async Task Run()
    {
        string path = Path.Combine(Path.GetTempPath(), "cube-progress-" + Guid.NewGuid().ToString("N"));
        using var model = new EditorViewModel(path);
        var window = new MainWindow(model) { ShowInTaskbar = false, Width = 640, Height = 720 };
        window.Show();
        void Check(bool value, string name) { if (!value) throw new Exception(name); Console.WriteLine("PASS " + name); }
        void Set(string name, object value) => typeof(EditorViewModel).GetProperty(name)!.SetValue(model, value);
        async Task Settle() { await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout(); }
        IEnumerable<T> Find<T>(DependencyObject root) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T result) yield return result;
                foreach (var nested in Find<T>(child)) yield return nested;
            }
        }
        async Task Verify(string name, int rings, int bars)
        {
            await Settle(); await Task.Delay(250); await Settle();
            Check(Find<Wpf.Ui.Controls.ProgressRing>(window).Count(c => c.IsVisible) == rings, name + " ring visibility");
            Check(Find<ProgressBar>(window).Count(c => c.IsVisible) == bars, name + " bar visibility");
            Check(Find<ProgressBar>(window).Where(c => c.IsVisible).All(c => c.IsIndeterminate && c.Orientation == Orientation.Horizontal && c.Height == 4), name + " uses standard horizontal infinite bar");
            Check(Find<ProgressBar>(window).Where(c => !c.IsVisible).All(c => !c.IsIndeterminate), name + " hidden bars stop animating");
            foreach (var bar in Find<ProgressBar>(window).Where(c => c.IsVisible))
                Check(bar.Template.FindName("PART_GlowRect", bar) != null, name + " has animation element");
            string folder = Path.GetFullPath("C:/CubeControl-Build/reports/progress"); Directory.CreateDirectory(folder);
            var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(window); var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
            using var file = File.Create(Path.Combine(folder, name + ".png")); png.Save(file);
        }
        try
        {
            await Verify("idle", 0, 0);
            await model.Execute(async () =>
            {
                await Verify("unknown-duration", 0, 1);
                Set(nameof(model.SearchingPorts), true);
                await Verify("port-search", 0, 1);
                Check(Find<ProgressBar>(window).Single(c => c.IsVisible).IsIndeterminate, "port search does not invent progress");
                Set(nameof(model.SearchingPorts), false);
                Set(nameof(model.ReadingInventory), true); Set(nameof(model.InventoryReadCount), 3);
                await Verify("inventory", 0, 1);
                Check(Find<ProgressBar>(window).Single(c => c.IsVisible).IsIndeterminate && model.InventoryReadCount == 3, "inventory animates while retaining real count");
                foreach (int zoom in new[] { 100, 140, 150, 200 })
                {
                    Cube.Desktop.Appearance.ZoomService.Instance.Preview(zoom);
                    await Verify("inventory-zoom-" + zoom, 0, 1);
                    var overlay = (Border)window.FindName("BusyOverlay");
                    var message = (TextBlock)window.FindName("BusyMessage");
                    Check(overlay.Effect == null && ((Border)window.FindName("BusyShadow")).Effect != null,
                        "shadow stays separate from text and animated progress at zoom " + zoom);
                    var bounds = overlay.TransformToAncestor(window).TransformBounds(new Rect(overlay.RenderSize));
                    Check(bounds.Left >= 0 && bounds.Right <= window.ActualWidth && bounds.Top >= 0 && bounds.Bottom <= window.ActualHeight,
                        "busy surface fits viewport at zoom " + zoom);
                    for (DependencyObject? parent = message; parent != overlay; parent = VisualTreeHelper.GetParent(parent!))
                        Check(parent is not UIElement element || element.Effect == null, "text has no rasterizing effect ancestor");
                }
                Cube.Desktop.Appearance.ZoomService.Instance.Preview(100);
                Set(nameof(model.ReadingInventory), false);
                model.Ir.TransferActive = true; model.Ir.Report(new("Gravando", 1024, 4096));
                await Verify("transfer", 0, 1);
                Check(Find<ProgressBar>(window).Single(c => c.IsVisible).IsIndeterminate && model.Ir.Progress == 25, "transfer uses standard infinite bar while retaining actual byte progress");
                model.Ir.Report(new("Aguardando", 0, 0));
                await Verify("unknown-transfer", 0, 1);
            });
            Check(model.ErrorMessage.Length == 0, "all operation-state checks completed without a swallowed exception: " + model.ErrorMessage);
            await Verify("completed", 0, 0);
            await model.Execute(() => throw new Exception("Test failure"));
            await Verify("failed", 0, 0);
        }
        finally
        {
            window.Close();
            if (Directory.Exists(path)) Directory.Delete(path, true);
        }
    }
}
