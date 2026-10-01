using System.IO;
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

internal static class EffectVisualizationTests
{
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static byte[] Pixels(FrameworkElement element, string? file = null)
    {
        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(element.ActualWidth), (int)Math.Ceiling(element.ActualHeight), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(element);
        if (file != null)
        {
            var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(file); png.Save(stream);
        }
        byte[] bytes = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
        bitmap.CopyPixels(bytes, bitmap.PixelWidth * 4, 0); return bytes;
    }
    public static async Task Run()
    {
        string folder = Path.GetFullPath("C:/CubeControl-Build/reports/effect-visualizations"); Directory.CreateDirectory(folder);
        var theme = ThemeService.Instance; var previousTheme = theme.Mode;
        string language = LanguageService.Instance.Language;
        using var fake = new FakeConnection();
        fake.Bank[8] = 8;
        using var session = new CubeSession(fake); await session.Identify();
        using var model = new EditorViewModel(Path.Combine(Path.GetTempPath(), "cube-effects-" + Guid.NewGuid().ToString("N")));
        await model.Execute(() => model.AttachSession(session));
        var window = new MainWindow(model) { ShowInTaskbar = false, Width = 1280, Height = 900 };
        window.Show(); await Task.Delay(150);
        while (model.Busy) await Task.Delay(30);
        var diagram = (EffectVisualization)window.FindName("EffectDiagram");
        var band = (Grid)window.FindName("EffectVisualizationBand");
        async Task Settle() { window.UpdateLayout(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout(); }
        try
        {
            int writes = model.WriteCommandCount;
            foreach (int stage in new[] { 1, 2 })
            {
                model.SelectedStage = stage; await Settle(); byte[]? before = null;
                for (int position = 1; position <= 7; position++)
                {
                    var dial = stage == 1 ? model.Current.Musical.ChorusDial : model.Current.Musical.PhaserDial;
                    dial.Value = position; await Settle();
                    Check(diagram.Position == position && !diagram.Active && !diagram.IsAnimating, "Prepared modulation binding or inactive motion failed");
                    var pixels = Pixels(diagram);
                    Check(before == null || !pixels.SequenceEqual(before), $"Adjacent modulation positions must look different: {stage}/{position}"); before = pixels;
                }
            }
            Check(model.WriteCommandCount == writes, "Illustration/prepared positions caused MIDI writes");
            model.Current.Musical.ChorusEnabled = true; model.SelectedStage = 1; await model.Flush(); await Settle();
            Check(diagram.Active, "Chorus activation binding");
            model.Current.Musical.PhaserEnabled = true; await model.Flush(); await Settle();
            Check(!diagram.Active && !diagram.IsAnimating, "Chorus must stop when Phaser activates");
            model.SelectedStage = 2; await Settle();
            if (SystemParameters.ClientAreaAnimation && !SystemParameters.HighContrast) Check(diagram.IsAnimating, "Visible active effect should animate");
            window.WindowState = WindowState.Minimized; await Settle(); Check(!diagram.IsAnimating, "Minimized window retained animation");
            window.WindowState = WindowState.Normal;
            model.SelectedStage = 0; await Settle(); Check(!band.IsVisible && !diagram.IsAnimating, "Amp retained visualization");
            model.SelectedStage = 4; model.Current.Musical.ReverbEnabled = false; await model.Flush(); await Settle();
            model.Current.Musical.ReverbDial.Value = 1; await Settle(); var lowReverb = Pixels(diagram);
            model.Current.Musical.ReverbDial.Value = 15; await Settle(); Check(diagram.Mix == 15 && !Pixels(diagram).SequenceEqual(lowReverb), "Reverb mix response");
            model.SelectedStage = 3; model.Current.Musical.DelayEnabled = false;
            model.Current.Musical.DelayMixDial.Value = 0; await Settle(); var dry = Pixels(diagram);
            model.Current.Parameters[6].Value = 31; model.Current.Parameters[4].Value = 127; await model.Flush(); await Settle();
            Check(Pixels(diagram).SequenceEqual(dry), "Zero delay mix must show only direct reference");
            model.Current.Musical.DelayMixDial.Value = 118; await Settle(); var wet = Pixels(diagram);
            Check(!wet.SequenceEqual(dry), "Delay wet signal missing");
            model.Current.Parameters[6].Value = 0; await model.Flush(); await Settle();
            Check(!Pixels(diagram).SequenceEqual(wet), "Delay time must change spacing");
            wet = Pixels(diagram); model.Current.Parameters[4].Value = 0; await model.Flush(); await Settle();
            Check(!Pixels(diagram).SequenceEqual(wet), "Delay feedback must change repetitions");
            model.Undo.Execute(null); while (model.Busy) await Task.Delay(10); await Settle();
            Check(diagram.Feedback == 127, "Undo did not restore visualization feedback");
            model.SelectedIndex = 1; await Settle(); Check(diagram.Time == model.Current.Parameters[6].Value, "Preset switch retains stale input");
            model.SelectedIndex = 0; await Settle();
            writes = model.WriteCommandCount;
            foreach (var mode in new[] { ThemeMode.Light, ThemeMode.Dark })
            foreach (string locale in new[] { "pt-BR", "en-US" })
            foreach (int width in new[] { 640, 1024, 1280, 1920 })
            {
                theme.Preview(mode); LanguageService.Instance.Preview(locale); window.Width = width;
                foreach (int stage in new[] { 1, 2, 3, 4 })
                {
                    model.SelectedStage = stage; await Settle();
                    band.BringIntoView(); await Settle();
                    Check(band.IsVisible && Math.Abs(band.ActualHeight - 144) < .1 && diagram.ActualWidth > 32, "Responsive effect band geometry");
                    Check(band.ActualWidth <= window.ActualWidth, "Effect band horizontal overflow");
                    if (width == 1280 && locale == "pt-BR")
                    {
                        await Task.Delay(200); await Settle();
                        Pixels(window, Path.Combine(folder, $"{mode}-{stage}.png"));
                    }
                }
            }
            await model.Flush(); Check(model.WriteCommandCount == writes, "Theme, navigation or rendering sent MIDI");
            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.HighContrast);
            model.SelectedStage = 2; await Settle();
            Check(theme.IsHighContrast && !diagram.IsAnimating, "High contrast should use static system colors");
            band.BringIntoView(); await Task.Delay(200); await Settle();
            Pixels(window, Path.Combine(folder, "high-contrast.png"));
            theme.Preview(ThemeMode.Dark);
            Pixels(window, Path.Combine(folder, "window.png"));
            Console.WriteLine("PASS effect visualizations: seven positions, prepared state, activation, exclusive modulation, zero mix, delay controls, reverb mix, preset switch, lifecycle, themes, locales, four widths and no additional MIDI.");
        }
        finally
        {
            // MainWindow closes asynchronously and asks about dirty presets, even with a fake device.
            model.Ask = _ => Task.FromResult(true);
            window.Close();
            for (int i = 0; i < 100 && window.IsVisible; i++) await Task.Delay(20);
            Check(!window.IsVisible, "Test window did not finish closing");
            theme.Preview(previousTheme); LanguageService.Instance.Preview(language);
        }
        Check(!diagram.IsAnimating, "Closed window retained animation");
    }
}
