using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cube.Desktop.Appearance;
using Cube.Desktop.Controls;
using Cube.Desktop.ViewModels;
using Wpf.Ui.Appearance;
using ThemeMode = Cube.Desktop.Appearance.ThemeMode;

internal static class ThemeTests
{
    private static async Task CheckPlayerFinishes(ThemeService service)
    {
        var actions = new[] { ("play", "▶"), ("pause", "Ⅱ"), ("stop", "■"), ("loop", "↻"),
            ("previous", "⏮"), ("back", "◀◀"), ("forward", "▶▶"), ("next", "⏭"),
            ("sound", "♪"), ("mute", "×"), ("pitch-down", "♭"), ("pitch-up", "♯") };
        var panel = new WrapPanel { Margin = new Thickness(16) };
        var faces = new List<BackingButtonFace>();
        foreach (var (art, glyph) in actions)
        {
            var face = new BackingButtonFace { Art = art, Glyph = glyph, Width = 56, Height = 56 };
            faces.Add(face);
            var group = new StackPanel { Width = 116, Height = 98 };
            group.Children.Add(face);
            var label = new TextBlock { Text = art, HorizontalAlignment = HorizontalAlignment.Center };
            label.SetResourceReference(TextBlock.ForegroundProperty, "CubeText");
            group.Children.Add(label); panel.Children.Add(group);
        }
        panel.Children.Add(new BackingButtonFace { Art = "loop", Glyph = "↻", IsActive = true, Width = 56, Height = 56, Margin = new Thickness(12) });
        panel.Children.Add(new Button { IsEnabled = false, Content = new BackingButtonFace { Art = "play", Glyph = "▶", Width = 56, Height = 56 }, Margin = new Thickness(12) });
        var window = new Window { Width = 540, Height = 520, Content = panel, ShowInTaskbar = false, Title = "Player finishes" };
        window.SetResourceReference(Window.BackgroundProperty, "CubeCanvas");
        window.Show();
        string folder = TestPaths.Report("C:/CubeControl-Build/reports/control-finishes");
        Directory.CreateDirectory(folder);
        byte[] Pixels(Visual visual, int width, int height)
        {
            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            var isolated = new DrawingVisual();
            using (var dc = isolated.RenderOpen()) dc.DrawRectangle(new VisualBrush(visual) { Stretch = Stretch.Fill }, null, new Rect(0, 0, width, height));
            bitmap.Render(isolated);
            var pixels = new byte[width * height * 4]; bitmap.CopyPixels(pixels, width * 4, 0); return pixels;
        }
        try
        {
            foreach (var mode in new[] { ThemeMode.Light, ThemeMode.Dark })
            {
                service.Preview(mode);
                byte[][]? previous = null;
                foreach (var finish in Enum.GetValues<ControlFinish>())
                {
                    panel.LayoutTransform = Transform.Identity;
                    window.Width = 540; window.Height = 520;
                    service.PreviewFinish(finish);
                    window.UpdateLayout(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                    var images = faces.Select(face => Pixels(face, 56, 56)).ToArray();
                    if (images.Any(p => !p.Any(b => b != 0)) || previous != null && images.Where((p, i) => p.SequenceEqual(previous[i])).Any())
                        throw new Exception($"Player faces did not render/update: {mode}/{finish}");
                    if (images.Select(Convert.ToBase64String).Distinct().Count() != actions.Length)
                        throw new Exception("Player actions must remain visually distinct: " + string.Join(",", actions.Where((a, i) => images.Where((p, j) => j != i && p.SequenceEqual(images[i])).Any()).Select(a => a.Item1)));
                    previous = images;
                    Console.WriteLine($"PASS all twelve existing player faces update and remain distinct in {mode}/{finish}");
                    foreach (double scale in new[] { 1d, 1.5d })
                    {
                        panel.LayoutTransform = new ScaleTransform(scale, scale);
                        window.Width = 540 * scale; window.Height = 520 * scale;
                        window.UpdateLayout(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                        var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32); bitmap.Render(window);
                        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                        using var output = File.Create(Path.Combine(folder, $"player-{mode}-{finish}-{scale * 100:0}.png")); encoder.Save(output);
                    }
                }
            }
            panel.LayoutTransform = Transform.Identity; window.Width = 540; window.Height = 520;
            var before = Pixels(faces[0], 56, 56);
            ApplicationThemeManager.Apply(ApplicationTheme.HighContrast);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            if (before.SequenceEqual(Pixels(faces[0], 56, 56))) throw new Exception("Player must switch to system glyphs in high contrast");
            Console.WriteLine("PASS player uses system glyphs in high contrast");
            service.Preview(ThemeMode.Dark);
        }
        finally { window.Close(); }
    }
    private static async Task CheckControlFinishes(ThemeService service, EditorViewModel model, Knob existingKnob)
    {
        void Check(bool valid, string label) { if (!valid) throw new Exception(label); Console.WriteLine("PASS " + label); }
        string folder = TestPaths.Report("C:/CubeControl-Build/reports/control-finishes");
        Directory.CreateDirectory(folder);
        var panel = new WrapPanel { Margin = new Thickness(20) };
        var dial = new Knob { Width = 94, Height = 94, Maximum = 127, Value = 64 };
        var disabled = new Knob { Width = 94, Height = 94, Maximum = 127, Value = 64, IsEnabled = false };
        var vertical = new Knob { Width = 72, Height = 160, Maximum = 100, Value = 50, IsFader = true };
        var horizontal = new Slider { Width = 140, Maximum = 100, Value = 50, VerticalAlignment = VerticalAlignment.Center };
        var on = new Wpf.Ui.Controls.ToggleSwitch { Content = "On", IsChecked = true };
        var off = new Wpf.Ui.Controls.ToggleSwitch { Content = "Off", IsChecked = false };
        var foot = new Border { Width = 62, Height = 62 };
        foot.SetResourceReference(Border.BackgroundProperty, "CubeControlFootswitch");
        foreach (var control in new FrameworkElement[] { dial, disabled, vertical, horizontal, on, off, foot })
        {
            control.Margin = new Thickness(8);
            panel.Children.Add(control);
        }
        foreach (double value in new[] { 0d, 64d, 127d })
            panel.Children.Add(new Knob { Width = 94, Height = 94, Maximum = 127, Value = value, Margin = new Thickness(8) });
        var preview = new Window { Width = 640, Height = 420, Content = panel, ShowInTaskbar = false, Title = "Control finishes" };
        preview.SetResourceReference(Window.BackgroundProperty, "CubeCanvas");
        preview.Show();
        try
        {
            foreach (var mode in new[] { ThemeMode.Light, ThemeMode.Dark })
            {
                service.Preview(mode);
                ImageSource? previousFace = null;
                foreach (var finish in Enum.GetValues<ControlFinish>())
                {
                    int revision = (int)Application.Current.Resources["CubeThemeRevision"];
                    service.PreviewFinish(finish);
                    await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                    var face = (ImageSource)Application.Current.Resources["CubeControlKnobFace"];
                    Check(face.Width > 0 && !ReferenceEquals(face, previousFace) && (int)Application.Current.Resources["CubeThemeRevision"] > revision, $"{mode}/{finish} replaces faces and invalidates existing controls");
                    previousFace = face;
                    var onFace = (Border)on.Template.FindName("SwitchFace", on);
                    var offFace = (Border)off.Template.FindName("SwitchFace", off);
                    Check(ReferenceEquals(onFace.Background, Application.Current.Resources["OfficialSwitchOn"]) && ReferenceEquals(offFace.Background, Application.Current.Resources["OfficialSwitchOff"]) && !ReferenceEquals(onFace.Background, offFace.Background), "existing switches update their distinct on/off faces");
                    Check(ReferenceEquals(foot.Background, Application.Current.Resources["CubeControlFootswitch"]), "existing footswitch updates its face");
                    Check(existingKnob.Value == 4 && dial.Value == 64 && vertical.Value == 50 && horizontal.Value == 50 && on.IsChecked == true && off.IsChecked == false && model.WriteCommandCount == 0, "finish selection preserves values and sends no MIDI");
                    var image = new RenderTargetBitmap(94, 94, 96, 96, PixelFormats.Pbgra32);
                    image.Render(dial);
                    var bytes = new byte[94 * 94 * 4]; image.CopyPixels(bytes, 94 * 4, 0);
                    Check(bytes.Any(b => b != 0), "existing knob renders after finish selection");
                    foreach (double scale in new[] { 1d, 1.5d })
                    {
                        panel.LayoutTransform = new ScaleTransform(scale, scale);
                        preview.Height = scale == 1 ? 540 : 860;
                        preview.UpdateLayout();
                        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                        var bitmap = new RenderTargetBitmap((int)preview.ActualWidth, (int)preview.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                        bitmap.Render(preview);
                        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                        using var output = File.Create(Path.Combine(folder, $"{mode}-{finish}-{scale * 100:0}.png")); encoder.Save(output);
                    }
                }
            }
            var savedFinish = service.Finish;
            ApplicationThemeManager.Apply(ApplicationTheme.HighContrast);
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(service.Finish == savedFinish && Application.Current.Resources["OfficialSwitchOn"] is DrawingBrush && Application.Current.Resources["CubeControlHorizontalFader"] is DrawingBrush && Application.Current.Resources["CubeControlFootswitch"] is DrawingBrush, "high contrast uses system drawings without losing finish");
            service.Preview(ThemeMode.Dark);
            Check(service.Finish == savedFinish && Application.Current.Resources["OfficialSwitchOn"] is ImageBrush, "leaving high contrast restores finish");
        }
        finally { preview.Close(); }
    }
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr handle, uint message, IntPtr wParam, string lParam);
    public static async Task Run()
    {
        int passed = 0;
        void Check(bool condition, string label) { if (!condition) throw new Exception(label); Console.WriteLine("PASS " + label); passed++; }
        string folder = Path.Combine(Path.GetTempPath(), "cube-theme-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder); string path = Path.Combine(folder, "appearance-v1.json");
        var service = Cube.Desktop.Appearance.ThemeService.Instance; var previous = service.Mode;
        var previousFinish = service.Finish;
        var originalWindowsReader = service.ReadWindowsLightTheme;
        bool windowsLight = true;
        service.ReadWindowsLightTheme = () => windowsLight;
        Window? window = null;
        try
        {
            Check(ThemeSettings.Load(path) == ThemeMode.System, "new installations follow the system");
            Check(ThemeSettings.LoadFinish(path) == ControlFinish.Official, "new installations use the official finish");
            File.WriteAllText(path, "{\"Version\":1,\"Mode\":\"Light\"}");
            Check(ThemeSettings.Load(path) == ThemeMode.Light && ThemeSettings.LoadFinish(path) == ControlFinish.Official, "old preferences keep theme and default to official finish");
            File.WriteAllText(path, "{\"Version\":1,\"Mode\":\"Dark\",\"Finish\":\"unknown\"}");
            Check(ThemeSettings.Load(path) == ThemeMode.Dark && ThemeSettings.LoadFinish(path) == ControlFinish.Official, "invalid finish preserves valid theme");
            foreach (var finish in Enum.GetValues<ControlFinish>())
            {
                ThemeSettings.Save(path, ThemeMode.Light, finish);
                Check(ThemeSettings.Load(path) == ThemeMode.Light && ThemeSettings.LoadFinish(path) == finish, $"{finish} survives restart");
                ThemeSettings.Save(path, ThemeMode.Dark);
                Check(ThemeSettings.LoadFinish(path) == finish, "saving theme preserves " + finish);
            }
            foreach (var mode in Enum.GetValues<ThemeMode>()) { ThemeSettings.Save(path, mode); Check(ThemeSettings.Load(path) == mode, $"{mode} preference survives restart"); }
            File.WriteAllText(path, "{broken"); Check(ThemeSettings.Load(path) == ThemeMode.System, "invalid preference falls back to system");
            File.WriteAllText(path, "{\"Version\":1,\"Mode\":\"999\"}"); Check(ThemeSettings.Load(path) == ThemeMode.System, "unknown theme is rejected");
            var panel = new StackPanel(); var text = new TextBlock { Text = "Theme" }; text.SetResourceReference(TextBlock.ForegroundProperty, "CubeText");
            var knob = new Knob { Value = 4, Maximum = 7, Width = 90, Height = 90 }; panel.Children.Add(text); panel.Children.Add(knob);
            window = new Window { Width = 200, Height = 200, Content = panel, ShowInTaskbar = false, WindowStartupLocation = WindowStartupLocation.CenterScreen };
            window.Show(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); service.Attach(window);
            window.SetResourceReference(Window.IconProperty, "CubeWindowIcon");
            bool HasIcon(string name) => window.Icon is BitmapImage image && image.UriSource.AbsoluteUri.EndsWith(name) && image.PixelWidth > 0;
            using var model = new EditorViewModel(Path.Combine(folder, "model"));
            service.Preview(ThemeMode.Light); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var light = ((SolidColorBrush)text.Foreground).Color;
            foreach (string key in new[] { "CubeSelected", "CubeInset" })
            {
                var color = ((SolidColorBrush)Application.Current.Resources[key]).Color;
                Check(color.R == color.G && color.G == color.B, key + " is neutral in light theme");
            }
            Check(HasIcon("cubeBabyBlack.png"), "light theme loads embedded black icon into the existing window");
            Check(ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light && light.R < 100, "light theme refreshes WPF UI and existing custom text");
            Check(Cube.Desktop.Appearance.ThemeService.ModuleColor("#D5AD7D") == "#82551F", "module accent becomes legible on light surfaces");
            service.Preview(ThemeMode.Dark); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(HasIcon("cubeBabyBlack.png"), "dark app keeps black taskbar icon on light Windows");
            Check(((BitmapImage)Application.Current.Resources["CubeAppIcon"]).UriSource.AbsoluteUri.EndsWith("cubeBabyWhite.png"), "dark app keeps its own white branding");
            windowsLight = false;
            SendMessage(new WindowInteropHelper(window).Handle, 0x001A, IntPtr.Zero, "ImmersiveColorSet");
            Check(HasIcon("cubeBabyWhite.png"), "Windows notification updates taskbar icon in manual app mode");
            service.Preview(ThemeMode.Light);
            Check(HasIcon("cubeBabyWhite.png"), "light app keeps white taskbar icon on dark Windows");
            Check(((BitmapImage)Application.Current.Resources["CubeAppIcon"]).UriSource.AbsoluteUri.EndsWith("cubeBabyBlack.png"), "light app keeps its own black branding");
            service.Preview(ThemeMode.Dark);
            Check(ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark && ((SolidColorBrush)text.Foreground).Color.R > 200, "dark theme refreshes existing resources");
            Check(knob.Value == 4 && model.WriteCommandCount == 0 && model.Current.Parameters[0].Value == 0, "theme switch preserves knob and preset values without MIDI writes");
            await CheckControlFinishes(service, model, knob);
            await CheckPlayerFinishes(service);
            double Contrast(string foreground, string background)
            {
                Color fg = ((SolidColorBrush)Cube.Desktop.Appearance.ThemeService.Brush(foreground)).Color;
                Color bg = ((SolidColorBrush)Cube.Desktop.Appearance.ThemeService.Brush(background)).Color;
                // WPF UI layers are translucent; composite over the window canvas first.
                var canvas = ((SolidColorBrush)Cube.Desktop.Appearance.ThemeService.Brush("CubeCanvas")).Color;
                double backgroundAlpha = bg.A / 255d;
                bg = Color.FromRgb((byte)Math.Round(bg.R * backgroundAlpha + canvas.R * (1 - backgroundAlpha)),
                    (byte)Math.Round(bg.G * backgroundAlpha + canvas.G * (1 - backgroundAlpha)),
                    (byte)Math.Round(bg.B * backgroundAlpha + canvas.B * (1 - backgroundAlpha)));
                double alpha = fg.A / 255d;
                double Linear(double channel) => channel <= .04045 ? channel / 12.92 : Math.Pow((channel + .055) / 1.055, 2.4);
                double Lum(double r, double g, double b) => .2126 * Linear(r / 255) + .7152 * Linear(g / 255) + .0722 * Linear(b / 255);
                double a = Lum(fg.R * alpha + bg.R * (1 - alpha), fg.G * alpha + bg.G * (1 - alpha), fg.B * alpha + bg.B * (1 - alpha));
                double b = Lum(bg.R, bg.G, bg.B);
                return (Math.Max(a, b) + .05) / (Math.Min(a, b) + .05);
            }
            foreach (var mode in new[] { ThemeMode.Light, ThemeMode.Dark })
            {
                service.Preview(mode);
                foreach (string key in new[] { "CubeCanvas", "CubeCard", "CubeInset", "CubeWell", "CubeFloatingSurface", "CubeBorder", "CubeFocus", "CubeAccent", "CubeSelected" })
                {
                    string source = (string)Application.Current.Resources["CubePalette." + key];
                    Check(Application.Current.TryFindResource(source) is Brush, mode + " resolves " + source);
                    Check(ReferenceEquals(Application.Current.Resources[key], Application.Current.TryFindResource(source)), mode + " semantic alias " + key);
                }
                foreach (var pair in new[] { ("CubeText", "TextFillColorPrimaryBrush"), ("Muted", "TextFillColorSecondaryBrush"),
                    ("CubeDisabledText", "TextFillColorDisabledBrush"), ("CubeWarning", "SystemFillColorCautionBrush") })
                    Check(ReferenceEquals(Cube.Desktop.Appearance.ThemeService.Brush(pair.Item1), Cube.Desktop.Appearance.ThemeService.Brush(pair.Item2)), mode + " semantic resource " + pair.Item1);
                foreach (string surface in new[] { "CubeCanvas", "CubeCard", "CubeInset", "CubeWell", "CubeFloatingSurface" })
                    foreach (string label in new[] { "CubeText", "Muted", "CubeWarning" })
                        Check(Contrast(label, surface) >= 4.5, mode + " text contrast >= 4.5: " + label + "/" + surface);
            }
            ApplicationThemeManager.Apply(ApplicationTheme.HighContrast);
            Check(ReferenceEquals(Application.Current.Resources["CubeBorder"], SystemColors.WindowTextBrush),
                "high contrast retains system-colored functional drawing strokes");
            Check((Thickness)Application.Current.Resources["CubeSurfaceBorderThickness"] == new Thickness(0), "high contrast surfaces remain borderless");
            Check(ReferenceEquals(Application.Current.Resources["CubeDisabledText"], SystemColors.GrayTextBrush), "high contrast disabled text uses Windows palette");
            service.Preview(ThemeMode.Dark);
            Check(!Application.Current.Resources.Keys.Cast<object>().Contains("ButtonBorderBrushDisabled"), "theme service leaves native control borders to WPF UI");
            // Notify only this test window; do not change global Windows personalization settings.
            SendMessage(new WindowInteropHelper(window).Handle, 0x001A, IntPtr.Zero, "ImmersiveColorSet");
            Check(ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark, "manual dark mode ignores system-change notifications");
            service.Preview(ThemeMode.System); var expected = ApplicationThemeManager.GetAppTheme();
            Check(HasIcon("cubeBabyWhite.png"), "taskbar follows Windows shell independently of Windows app theme");
            int before = (int)Application.Current.Resources["CubeThemeRevision"];
            SendMessage(new WindowInteropHelper(window).Handle, 0x001A, IntPtr.Zero, "ImmersiveColorSet");
            Check((int)Application.Current.Resources["CubeThemeRevision"] > before && ApplicationThemeManager.GetAppTheme() == expected, "system mode receives Windows theme notifications and refreshes custom palette");
            service.Preview(ThemeMode.Light);
            before = (int)Application.Current.Resources["CubeThemeRevision"];
            SendMessage(new WindowInteropHelper(window).Handle, 0x001A, IntPtr.Zero, "ImmersiveColorSet");
            Check((int)Application.Current.Resources["CubeThemeRevision"] == before && ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light, "leaving system mode removes the watcher");
        }
        finally
        {
            service.Detach(); window?.Close(); service.ReadWindowsLightTheme = originalWindowsReader; service.PreviewFinish(previousFinish); service.Preview(previous);
            string resolved = Path.GetFullPath(folder);
            if (!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("cube-theme-tests-")) throw new IOException("Unexpected test directory");
            Directory.Delete(resolved, true);
        }
        Console.WriteLine($"{passed} theme checks passed.");
    }
}
