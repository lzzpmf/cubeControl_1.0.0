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
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr handle, uint message, IntPtr wParam, string lParam);
    public static async Task Run()
    {
        int passed = 0;
        void Check(bool condition, string label) { if (!condition) throw new Exception(label); Console.WriteLine("PASS " + label); passed++; }
        string folder = Path.Combine(Path.GetTempPath(), "cube-theme-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder); string path = Path.Combine(folder, "appearance-v1.json");
        var service = Cube.Desktop.Appearance.ThemeService.Instance; var previous = service.Mode;
        var originalWindowsReader = service.ReadWindowsLightTheme;
        bool windowsLight = true;
        service.ReadWindowsLightTheme = () => windowsLight;
        Window? window = null;
        try
        {
            Check(ThemeSettings.Load(path) == ThemeMode.System, "new installations follow the system");
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
            foreach (string key in new[] { "CubeSelected", "CubeInset", "CubeFocus", "CubeAccent" })
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
            double Contrast(string foreground, string background)
            {
                Color fg = ((SolidColorBrush)Cube.Desktop.Appearance.ThemeService.Brush(foreground)).Color;
                Color bg = ((SolidColorBrush)Cube.Desktop.Appearance.ThemeService.Brush(background)).Color;
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
                foreach (var pair in new[] { ("CubeText", "TextFillColorPrimaryBrush"), ("Muted", "TextFillColorSecondaryBrush"),
                    ("CubeDisabledText", "TextFillColorDisabledBrush"), ("CubeWarning", "SystemFillColorCautionBrush") })
                    Check(ReferenceEquals(Cube.Desktop.Appearance.ThemeService.Brush(pair.Item1), Cube.Desktop.Appearance.ThemeService.Brush(pair.Item2)), mode + " semantic resource " + pair.Item1);
                foreach (string surface in new[] { "CubeCanvas", "CubeCard", "CubeInset", "CubeWell", "CubeFloatingSurface" })
                    foreach (string label in new[] { "CubeText", "Muted", "CubeWarning" })
                        Check(Contrast(label, surface) >= 4.5, mode + " text contrast >= 4.5: " + label + "/" + surface);
            }
            ApplicationThemeManager.Apply(ApplicationTheme.HighContrast);
            Check(((SolidColorBrush)Application.Current.Resources["ButtonBorderBrushDisabled"]).Color == SystemColors.WindowTextColor,
                "high contrast restores system-colored control outlines");
            Check((Thickness)Application.Current.Resources["CubeSurfaceBorderThickness"] == new Thickness(1), "high contrast cards have visible outlines");
            Check(ReferenceEquals(Application.Current.Resources["CubeDisabledText"], SystemColors.GrayTextBrush), "high contrast disabled text uses Windows palette");
            service.Preview(ThemeMode.Dark);
            Check(((SolidColorBrush)Application.Current.Resources["ButtonBorderBrushDisabled"]).Color.A == 0, "leaving high contrast restores normal borderless appearance");
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
            service.Detach(); window?.Close(); service.ReadWindowsLightTheme = originalWindowsReader; service.Preview(previous);
            string resolved = Path.GetFullPath(folder);
            if (!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("cube-theme-tests-")) throw new IOException("Unexpected test directory");
            Directory.Delete(resolved, true);
        }
        Console.WriteLine($"{passed} theme checks passed.");
    }
}
