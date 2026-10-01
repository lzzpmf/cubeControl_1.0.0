using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cube.Desktop;
using Cube.Desktop.Controls;
using Cube.Desktop.ViewModels;
using Cube.Protocol;

internal static class GearCarouselTests
{
    public static async Task Run()
    {
        void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
        using var fake = new FakeConnection();
        using var session = new CubeSession(fake); await session.Identify();
        using var model = new EditorViewModel(Path.Combine(Path.GetTempPath(), "cube-carousel-" + Guid.NewGuid().ToString("N")));
        await model.Execute(() => model.AttachSession(session));
        var window = new MainWindow(model) { ShowInTaskbar = false, Width = 1280, Height = 900 };
        window.Show(); await Task.Delay(150); while (model.Busy) await Task.Delay(20);
        var art = (GearArt)window.FindName("FocusedArt");
        var image = (GearImageButton)window.FindName("EquipmentImageButton");
        async Task Settle() { window.UpdateLayout(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout(); }
        void Wheel(int delta) => art.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, delta) { RoutedEvent = UIElement.PreviewMouseWheelEvent });
        try
        {
            foreach (int stage in new[] { 0, 5 })
            {
                model.SelectedStage = stage; await Settle();
                var hint = (StackPanel)image.Template.FindName("CarouselHint", image);
                Check(hint.IsVisible && hint.HorizontalAlignment == HorizontalAlignment.Right && !hint.IsHitTestVisible, "Scroll indicators should be visible at right without intercepting interaction");
                Check(hint.Children.OfType<Wpf.Ui.Controls.SymbolIcon>().Select(i => i.Symbol).SequenceEqual(new[] { Wpf.Ui.Controls.SymbolRegular.ChevronUp24, Wpf.Ui.Controls.SymbolRegular.ChevronDown24 }), "Both directional chevrons required");
                var size = art.RenderSize;
                int start = model.FocusedGear.Value;
                Wheel(-60); Check(model.FocusedGear.Value == start && !art.IsScrolling, "Fractional wheel must wait without animation");
                Wheel(-60); await Settle();
                Check(model.FocusedGear.Value == start + 1, "Wheel down selects next item immediately");
                if (SurfaceMotion.Allowed && !Cube.Desktop.Appearance.ThemeService.Instance.IsHighContrast)
                    Check(art.IsScrolling && art.ScrollDirection == 1, "Next image must enter from below");
                Wheel(120); await Settle(); Check(model.FocusedGear.Value == start, "Rapid reverse must select previous item immediately");
                if (SurfaceMotion.Allowed && !Cube.Desktop.Appearance.ThemeService.Instance.IsHighContrast)
                    Check(art.IsScrolling && art.ScrollDirection == -1, "Previous image must enter from above");
                await Task.Delay(280); await Settle();
                Check(!art.IsScrolling && art.RenderSize == size, "Animation must clean up and preserve dimensions");
                Wheel(120); await Settle(); Check(model.FocusedGear.Value == start && !art.IsScrolling, "Endpoint must not animate or change item");
                Wheel(-120); model.SelectedStage = 1; await Settle();
                Check(!art.IsScrolling && !hint.IsVisible, "Effects must not retain carousel animation or hints");
                await model.Flush(); int writes = fake.Writes; await Task.Delay(250);
                Check(fake.Writes == writes, "Animation frames must not generate MIDI writes");
            }
            model.SelectedStage = 0; await Settle(); image.BringIntoView(); await Task.Delay(250); await Settle();
            string folder = Path.GetFullPath("C:/CubeControl-Build/reports/gear-carousel"); Directory.CreateDirectory(folder);
            var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32); bitmap.Render(window);
            var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
            using (var stream = File.Create(Path.Combine(folder, "chevrons.png"))) png.Save(stream);
            Console.WriteLine("PASS carousel: paired chevrons, partial wheel, direction, immediate selection, rapid reversal, bounds, stable layout, cleanup, effects excluded and no animation MIDI writes.");
        }
        finally
        {
            model.Ask = _ => Task.FromResult(true); window.Close();
            for (int i = 0; i < 100 && window.IsVisible; i++) await Task.Delay(20);
        }
    }
}
