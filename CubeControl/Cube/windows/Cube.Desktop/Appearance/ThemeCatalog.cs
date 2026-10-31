using System.Windows.Media;

namespace Cube.Desktop.Appearance;

internal sealed record ThemeDefinition(ThemeMode Mode, string Label, bool IsLight, ThemePalette? Palette = null);

internal static class ThemeCatalog
{
    internal static IReadOnlyList<ThemeDefinition> All { get; } = Array.AsReadOnly(new[]
    {
        new ThemeDefinition(ThemeMode.System, "Sistema", false),
        new ThemeDefinition(ThemeMode.Light, "Claro", true),
        new ThemeDefinition(ThemeMode.Dark, "Escuro", false),
        Custom(ThemeMode.Graphite, "Grafite", false, "#191C21", "#EDF0F5", "#A9B9CE"),
        Custom(ThemeMode.MidnightBlue, "Azul Meia-Noite", false, "#101B2C", "#EAF2FF", "#79B7FF"),
        Custom(ThemeMode.Forest, "Floresta", false, "#15221E", "#ECF4EE", "#89C9A5"),
        Custom(ThemeMode.Wine, "Vinho", false, "#281820", "#FAEDF2", "#E5A0BC"),
        Custom(ThemeMode.VintageAmber, "Âmbar Vintage", false, "#261E16", "#FAF0DC", "#E4B66C"),
        Custom(ThemeMode.Sand, "Areia", true, "#F2EBDD", "#342E25", "#805B2D"),
        Custom(ThemeMode.Ice, "Gelo", true, "#EDF3F8", "#233548", "#28648E")
    });

    private static ThemeDefinition Custom(ThemeMode mode, string label, bool light, string canvas, string text, string accent)
        => new(mode, label, light, new ThemePalette(canvas, text, accent));

    internal static ThemeDefinition Get(ThemeMode mode) => All.FirstOrDefault(item => item.Mode == mode)
        ?? throw new ArgumentOutOfRangeException(nameof(mode));
}

/// <summary>Opaque palette sources. Translucent brushes are always rebuilt from these colors.</summary>
internal sealed class ThemePalette
{
    internal const byte WindowTintAlpha = 232;
    internal IReadOnlyDictionary<string, Color> Colors { get; }
    internal Color Canvas => Colors["CubeCanvas"];
    internal Color Text => Colors["CubeText"];
    internal Color Accent => Colors["CubeAccent"];

    internal ThemePalette(string canvasHex, string textHex, string accentHex)
    {
        Color canvas = Parse(canvasHex), text = Parse(textHex), accent = Parse(accentHex);
        Color card = Mix(canvas, text, .06), field = Mix(canvas, text, .04);
        Color hover = Mix(canvas, text, .10), selected = Mix(canvas, accent, .20);
        Color[] opaqueSurfaces = [canvas, card, field, hover, selected];
        // Include the extreme desktop colors under Mica and every translucent surface.
        var backgrounds = opaqueSurfaces.Concat(new[] { System.Windows.Media.Colors.Black, System.Windows.Media.Colors.White }
            .SelectMany(desktop => opaqueSurfaces.Select(surface =>
                Mix(Mix(desktop, canvas, WindowTintAlpha / 255d), surface, 90 / 255d)))).ToArray();
        Color muted = EnsureContrast(Mix(canvas, text, .65), text, backgrounds, 4.5);
        // Custom themes separate surfaces by fill contrast, without decorative outlines.
        Color border = System.Windows.Media.Colors.Transparent;
        Color focus = EnsureContrast(accent, text, backgrounds, 3);
        Colors = new Dictionary<string, Color>
        {
            ["CubeCanvas"] = canvas, ["CubeText"] = text, ["Muted"] = muted,
            ["CubeCard"] = card, ["CubeFloatingSurface"] = card,
            ["CubeInset"] = field, ["CubeWell"] = field,
            ["CubeItemRest"] = card, ["CubeItemHover"] = hover,
            ["CubeComboField"] = field, ["CubeComboFieldHover"] = hover,
            ["CubeSelected"] = selected, ["CubeSelectedBrush"] = selected, ["CubeSelectedText"] = text,
            ["CubeBorder"] = border, ["CubeFocus"] = focus, ["CubeAccent"] = accent,
            ["CubeDisabledText"] = Mix(canvas, text, .45),
            ["CubePopupSurface"] = card, ["CubePopupHover"] = hover,
            ["CubePopupSelected"] = selected, ["CubePopupBorder"] = border,
            ["CubePopupHoverText"] = text,
            ["BackingCard"] = card, ["BackingBorder"] = border, ["BackingSecondary"] = muted
        };
    }

    internal static Color Parse(string hex) => (Color)ColorConverter.ConvertFromString(hex);
    internal static Color Mix(Color background, Color foreground, double amount) => Color.FromRgb(
        (byte)Math.Round(background.R + (foreground.R - background.R) * amount),
        (byte)Math.Round(background.G + (foreground.G - background.G) * amount),
        (byte)Math.Round(background.B + (foreground.B - background.B) * amount));

    internal static double Contrast(Color a, Color b)
    {
        static double Channel(byte value) => value / 255d <= .04045 ? value / 255d / 12.92 : Math.Pow((value / 255d + .055) / 1.055, 2.4);
        static double Luminance(Color c) => .2126 * Channel(c.R) + .7152 * Channel(c.G) + .0722 * Channel(c.B);
        double first = Luminance(a), second = Luminance(b);
        return (Math.Max(first, second) + .05) / (Math.Min(first, second) + .05);
    }

    private static Color EnsureContrast(Color preferred, Color text, Color[] backgrounds, double minimum)
    {
        for (int step = 0; step <= 100; step++)
        {
            Color candidate = Mix(preferred, text, step / 100d);
            if (backgrounds.All(background => Contrast(candidate, background) >= minimum)) return candidate;
        }
        throw new InvalidOperationException("Theme palette cannot meet its contrast requirements.");
    }
}
