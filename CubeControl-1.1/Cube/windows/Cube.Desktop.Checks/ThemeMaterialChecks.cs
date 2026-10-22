using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Cube.Desktop.Appearance;
using Wpf.Ui.Appearance;

internal static class ThemeMaterialChecks
{
    internal static void Run()
    {
        var app = new Cube.Desktop.App();
        app.InitializeComponent();
        var theme = ThemeService.Instance;
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var apply = typeof(ThemeService).GetMethod("ApplySurfaceMaterials", flags)!;
        var change = typeof(ThemeService).GetMethod("OnThemeChanged", flags)!;
        typeof(ThemeService).GetProperty("ReadWindowsTransparency", flags)!.SetValue(theme, (Func<bool>)(() => false));
        string[] keys = ["CubeItemRest", "CubeItemHover", "CubeSelected", "CubeCard", "CubeInset", "CubeWell", "CubeComboField", "BackingCard"];
        Color ColorOf(string key) => ((SolidColorBrush)app.Resources[key]).Color;
        foreach (var mode in new[] { ApplicationTheme.Dark, ApplicationTheme.Light })
        {
            change.Invoke(theme, [mode, Colors.Blue]);
            var text = ThemeService.Brush("CubeText");
            apply.Invoke(theme, [true]);
            var first = keys.ToDictionary(k => k, ColorOf);
            foreach (var key in new[] { "CubeCard", "CubeInset", "CubeWell" })
            {
                var color = first[key];
                bool dark = color.R < 128 && color.G < 128 && color.B < 128;
                if (dark != (mode == ApplicationTheme.Dark))
                    throw new Exception($"{key} has the wrong base color in {mode}: {color}");
            }
            if (first.Values.Any(c => c.A is 0 or 255)) throw new Exception("Missing translucent surface");
            apply.Invoke(theme, [true]);
            if (keys.Any(k => ColorOf(k) != first[k])) throw new Exception("Opacity compounded on refresh");
            apply.Invoke(theme, [false]);
            if (keys.Any(k => ColorOf(k).A != 255)) throw new Exception("Opaque fallback missing");
            apply.Invoke(theme, [true]);
            if (keys.Any(k => ColorOf(k) != first[k]) || !ReferenceEquals(text, ThemeService.Brush("CubeText")))
                throw new Exception("Palette or foreground changed unexpectedly");
            Console.WriteLine($"PASS: {mode}, transparency on/off/on, stable alpha and unchanged text");
        }
        change.Invoke(theme, [ApplicationTheme.HighContrast, Colors.Blue]);
        apply.Invoke(theme, [true]);
        if (keys.Any(k => ColorOf(k).A != 255) || ColorOf("CubeSelected") != SystemColors.HighlightColor)
            throw new Exception("High contrast fallback missing");
        Console.WriteLine("PASS: high contrast uses opaque system surfaces");
        app.Shutdown();
    }
}
