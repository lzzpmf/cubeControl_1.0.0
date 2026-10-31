using System.Windows;
using System.Windows.Media;

namespace Cube.Desktop.Appearance;

/// <summary>App-local adaptations for stock WPF UI controls; never edits the library dictionaries.</summary>
internal sealed class ThemePaletteResources
{
    private readonly Dictionary<string, object?> originals = new();

    internal void Reset(ResourceDictionary resources)
    {
        foreach (var (key, original) in originals)
            if (original == null) resources.Remove(key);
            else resources[key] = original;
        originals.Clear();
    }

    internal static SolidColorBrush Brush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    internal void Apply(ResourceDictionary resources, ThemePalette palette)
    {
        foreach (var (key, color) in palette.Colors) resources[key] = Brush(color);
        resources["CubeSurfaceBorderBrush"] = Brush(palette.Colors["CubeBorder"]);
        resources["CubeSurfaceBorderThickness"] = new Thickness(0);

        void Map(Color color, params string[] keys)
        {
            foreach (string key in keys)
            {
                if (!resources.Contains(key)) continue;
                // Save only locally declared values; merged theme resources must resolve afresh.
                originals.TryAdd(key, resources.Keys.Cast<object>().Contains(key) ? resources[key] : null);
                resources[key] = Brush(color);
            }
        }
        var c = palette.Colors;
        Map(palette.Canvas, "ApplicationBackgroundBrush", "SolidBackgroundFillColorBaseBrush");
        Map(c["CubeCard"], "CardBackgroundFillColorDefaultBrush", "CardBackgroundFillColorSecondaryBrush",
            "SolidBackgroundFillColorSecondaryBrush", "SolidBackgroundFillColorTertiaryBrush",
            "ContentDialogBackground", "MessageBoxBackground", "ToolTipBackground", "FlyoutBackground", "ButtonBackground");
        Map(c["CubeInset"], "LayerFillColorDefaultBrush", "ControlFillColorDefaultBrush", "ControlFillColorInputActiveBrush",
            "TextControlBackground", "TextControlBackgroundFocused");
        Map(c["CubeItemHover"], "ControlFillColorSecondaryBrush", "ButtonBackgroundPointerOver", "TextControlBackgroundPointerOver");
        Map(c["CubeSelected"], "ControlFillColorTertiaryBrush", "ButtonBackgroundPressed");
        Map(palette.Text, "TextFillColorPrimaryBrush", "ContentDialogForeground", "MessageBoxForeground", "ToolTipForeground",
            "ComboBoxForeground", "ComboBoxDropDownGlyphForeground", "TextControlForeground", "TextControlForegroundFocused",
            "ButtonForeground", "ButtonForegroundPointerOver", "ButtonForegroundPressed", "RadioButtonForeground", "ListViewItemForeground");
        Map(c["Muted"], "TextFillColorSecondaryBrush", "TextFillColorTertiaryBrush", "TextControlPlaceholderForeground");
        Map(c["CubeDisabledText"], "TextFillColorDisabledBrush");
        Map(c["CubeBorder"], "ControlStrokeColorDefaultBrush", "ControlStrokeColorSecondaryBrush",
            "CardStrokeColorDefaultBrush", "ContentDialogBorderBrush", "ContentDialogSeparatorBorderBrush",
            "ToolTipBorderBrush", "FlyoutBorderBrush", "ButtonBorderBrush", "ButtonBorderBrushPointerOver", "TextControlBorderBrush", "MessageBoxSeparatorBorderBrush", "CardBorderBrush");
        Map(c["CubeFocus"], "TextControlFocusedBorderBrush");
        Map(palette.Accent, "AccentFillColorDefaultBrush",
            "AccentTextFillColorPrimaryBrush", "AccentTextFillColorSecondaryBrush", "AccentTextFillColorTertiaryBrush",
            "AccentButtonBackground");
        Color onAccent = ThemePalette.Contrast(System.Windows.Media.Colors.Black, palette.Accent)
            >= ThemePalette.Contrast(System.Windows.Media.Colors.White, palette.Accent)
            ? System.Windows.Media.Colors.Black : System.Windows.Media.Colors.White;
        Map(onAccent, "TextOnAccentFillColorPrimaryBrush", "TextOnAccentFillColorSecondaryBrush",
            "AccentButtonForeground", "AccentButtonForegroundPointerOver", "AccentButtonForegroundPressed");
        Color awayFromText = onAccent == System.Windows.Media.Colors.Black ? System.Windows.Media.Colors.White : System.Windows.Media.Colors.Black;
        Map(ThemePalette.Mix(palette.Accent, awayFromText, .06), "AccentFillColorSecondaryBrush", "AccentButtonBackgroundPointerOver");
        Map(ThemePalette.Mix(palette.Accent, awayFromText, .12), "AccentFillColorTertiaryBrush", "AccentButtonBackgroundPressed");
    }
}
