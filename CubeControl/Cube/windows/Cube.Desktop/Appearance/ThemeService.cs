using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using Microsoft.Win32;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Cube.Desktop.Appearance;

public enum ThemeMode { System, Light, Dark, Graphite, MidnightBlue, Forest, Wine, VintageAmber, Sand, Ice }
public enum ControlFinish
{
    Official, BrushedSilver, RetroIvory, GraphiteCopper,
    BlackIvory, IndustrialBlack, TexturedSteel, MarconiBlack, VintageBrass, IvoryGold, ChromeSkullBlue,
    Vasco, Flamengo, Fluminense, Botafogo, Skeleton, Cartoon, Anime, Boho, Papercut,
    ReceiverSilverBlue, ReceiverChampagne, TurntableWalnut, TurntableDirectDrive,
    MiniSystemTitanium, MiniSystemBlackRed
}

public sealed class ThemeSettings
{
    public int Version { get; set; } = 1;
    public string Mode { get; set; } = "System";
    public string Finish { get; set; } = "Official";
    public static ControlFinish LoadFinish(string path)
    {
        try
        {
            var settings = JsonSerializer.Deserialize<ThemeSettings>(File.ReadAllText(path));
            if (settings?.Version == 1 && Enum.TryParse<ControlFinish>(settings.Finish, out var finish) && Enum.IsDefined(finish)) return finish;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { }
        return ControlFinish.Official;
    }
    public static ThemeMode Load(string path)
    {
        try
        {
            var settings = JsonSerializer.Deserialize<ThemeSettings>(File.ReadAllText(path));
            if (settings?.Version == 1 && Enum.TryParse<ThemeMode>(settings.Mode, out var mode) && Enum.IsDefined(mode)) return mode;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { }
        return ThemeMode.System;
    }
    public static void Save(string path, ThemeMode mode) => Save(path, mode, LoadFinish(path));
    public static void Save(string path, ThemeMode mode, ControlFinish finish)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        if (!Enum.IsDefined(finish)) throw new ArgumentOutOfRangeException(nameof(finish));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path + ".tmp", JsonSerializer.Serialize(new ThemeSettings { Mode = mode.ToString(), Finish = finish.ToString() }));
        File.Move(path + ".tmp", path, true);
    }
}

public sealed class ThemeService
{
    public static ThemeService Instance { get; } = new();
    private readonly string settingsPath = Path.Combine(Cube.Desktop.Appearance.AppDataPaths.LocalApplicationData, "CubeControl", "appearance-v1.json");
    private Window? window;
    private bool watching;
    private HwndSource? iconSource;
    private readonly ThemePaletteResources paletteResources = new();
    private ThemePalette? ActivePalette => IsHighContrast ? null : ThemeCatalog.Get(Mode).Palette;
    internal Func<bool> ReadWindowsHighContrast { get; set; } = () => SystemParameters.HighContrast;
    internal Func<bool> ReadWindowsLightTheme { get; set; } = () =>
        Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 1) is not int value || value != 0;
    internal Func<bool> ReadWindowsTransparency { get; set; } = () =>
        Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 1) is not int value || value != 0;
    internal bool UsesBackdrop { get; private set; }
    private bool applyingBackdrop;
    internal void RefreshBackdrop()
    {
        if (applyingBackdrop) return;
        applyingBackdrop = true;
        try
        {
            bool enabled = !IsHighContrast && !ReadWindowsHighContrast() && ReadWindowsTransparency()
                && WindowBackdrop.IsSupported(WindowBackdropType.Mica);
            UsesBackdrop = enabled;
            ApplySurfaceMaterials(enabled);
            var resources = Application.Current.Resources;
            resources["CubeWindowBackground"] = enabled ? Brushes.Transparent : Brush("CubeCanvas");
            // Retain Mica, but tint it for custom palettes so the wallpaper cannot wash out their colors.
            Color? tint = ActivePalette?.Canvas;
            if (enabled && tint is Color color)
            {
                color.A = ThemePalette.WindowTintAlpha;
                resources["CubeCanvasSurface"] = ThemePaletteResources.Brush(color);
            }
            else resources["CubeCanvasSurface"] = resources["CubeWindowBackground"];
            if (window is FluentWindow fluent)
            {
                // Keep the existing chrome object: replacing it after load in WPF UI
                // can detach its Freezable inheritance context during a theme change.
                var chrome = System.Windows.Shell.WindowChrome.GetWindowChrome(fluent);
                if (chrome is { IsFrozen: false }) chrome.GlassFrameThickness = enabled ? new Thickness(-1) : new Thickness(0.00001);
                // Theme changes can replace the native background independently of this property.
                WindowBackgroundManager.UpdateBackground(fluent, ApplicationThemeManager.GetAppTheme(), enabled ? WindowBackdropType.Mica : WindowBackdropType.None);
                fluent.SetResourceReference(Window.BackgroundProperty, "CubeWindowBackground");
            }
        }
        finally { applyingBackdrop = false; }
    }
    // Rebuild from palette sources on every change; never multiply the previous opacity.
    // Only backgrounds are translucent. Text, icons and selection markers stay opaque.
    internal void ApplySurfaceMaterials(bool enabled)
    {
        var resources = Application.Current.Resources;
        if (IsHighContrast || ReadWindowsHighContrast())
        {
            foreach (string key in new[] { "CubeItemRest", "CubeItemHover", "CubeComboField", "CubeComboFieldHover",
                "BackingCard", "CubeCard", "CubeInset", "CubeWell" }) resources[key] = SystemColors.WindowBrush;
            resources["CubeSelected"] = resources["CubeSelectedBrush"] = SystemColors.HighlightBrush;
            resources["CubeLibraryItemBackground"] = SystemColors.WindowBrush;
            return;
        }
        void Surface(string key, Color color, byte alpha)
        {
            color.A = enabled ? alpha : (byte)255;
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            resources[key] = brush;
        }
        foreach (var (key, alpha) in new (string, byte)[] {
            ("CubeItemRest", 90), ("CubeItemHover", 135), ("CubeSelectedBrush", 180),
            ("CubeComboField", 140), ("CubeComboFieldHover", 170), ("BackingCard", 120),
            ("CubeCard", 100), ("CubeInset", 80), ("CubeWell", 110) })
            Surface(key, ActivePalette is { } palette && palette.Colors.TryGetValue(key, out var color)
                ? color : (Color)resources[key + (IsLight ? ".Light" : ".Dark")], alpha);
        resources["CubeSelected"] = resources["CubeSelectedBrush"];
        resources["CubeLibraryItemBackground"] = IsLight ? resources["CubeItemRest"] : Brushes.Transparent;
    }

    private void RefreshWindowIcon()
    {
        bool light = SystemParameters.HighContrast
            ? SystemColors.WindowColor.R * 0.299 + SystemColors.WindowColor.G * 0.587 + SystemColors.WindowColor.B * 0.114 >= 128
            : ReadWindowsLightTheme();
        Application.Current.Resources["CubeWindowIcon"] = light ? BlackIcon : WhiteIcon;
    }
    private IntPtr WindowThemeMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        // Keep observing the shell theme even when the app uses a manual theme.
        if (message is 0x001A or 0x031A)
        {
            if (IsHighContrast != ReadWindowsHighContrast()) Apply();
            else { RefreshWindowIcon(); RefreshBackdrop(); }
        } // WM_SETTINGCHANGE / WM_THEMECHANGED
        return IntPtr.Zero;
    }
    private int revision;
    private static readonly ImageSource BlackIcon = LoadIcon("cubeBabyBlack.png");
    private static readonly ImageSource WhiteIcon = LoadIcon("cubeBabyWhite.png");
    private static ImageSource LoadIcon(string name)
    {
        var image = new BitmapImage(new Uri($"pack://application:,,,/CubeControl;component/Assets/Images/Icons/{name}"));
        image.Freeze();
        return image;
    }
    public ThemeMode Mode { get; private set; }
    public ControlFinish Finish { get; private set; }
    public bool IsLight { get; private set; }
    public bool IsHighContrast { get; private set; }
    public event Action? Changed;

    private ThemeService()
    {
        Mode = ThemeSettings.Load(settingsPath);
        Finish = ThemeSettings.LoadFinish(settingsPath);
        ApplicationThemeManager.Changed += OnThemeChanged;
        SystemParameters.StaticPropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(SystemParameters.HighContrast) || Application.Current?.Dispatcher is not { } dispatcher) return;
            if (dispatcher.CheckAccess()) Apply();
            else dispatcher.BeginInvoke(Apply);
        };
    }
    public void Initialize() => Apply();
    public void Attach(Window target)
    {
        if (iconSource != null) iconSource.RemoveHook(WindowThemeMessage);
        window = target;
        iconSource = HwndSource.FromHwnd(new WindowInteropHelper(target).Handle);
        iconSource?.AddHook(WindowThemeMessage);
        // Attach after Loaded so Watch/UnWatch never leave deferred handlers behind.
        Apply();
        target.Activated += (_, _) => { if (Mode == ThemeMode.System && !watching) Apply(); };
    }
    public void Detach()
    {
        StopWatching();
        iconSource?.RemoveHook(WindowThemeMessage);
        iconSource = null;
        window = null;
    }
    private void StopWatching()
    {
        if (watching && window?.IsLoaded == true) SystemThemeWatcher.UnWatch(window);
        watching = false;
    }
    public void Select(ThemeMode mode)
    {
        ThemeSettings.Save(settingsPath, mode, Finish);
        Preview(mode);
    }
    public void SelectFinish(ControlFinish finish)
    {
        ThemeSettings.Save(settingsPath, Mode, finish);
        PreviewFinish(finish);
    }
    internal void PreviewFinish(ControlFinish finish)
    {
        if (!Enum.IsDefined(finish)) throw new ArgumentOutOfRangeException(nameof(finish));
        Finish = finish;
        OnThemeChanged(ApplicationThemeManager.GetAppTheme(), default);
    }
    internal void Preview(ThemeMode mode)
    {
        if (!Enum.IsDefined(mode)) throw new ArgumentOutOfRangeException(nameof(mode));
        Mode = mode; Apply();
    }
    private void Apply()
    {
        StopWatching();
        // The library also derives its accent during Apply; remove our overrides first.
        paletteResources.Reset(Application.Current.Resources);
        if (ReadWindowsHighContrast())
            ApplicationThemeManager.Apply(ApplicationTheme.HighContrast, WindowBackdropType.None, true);
        else if (Mode == ThemeMode.System)
        {
            ApplicationThemeManager.ApplySystemTheme();
            if (window?.IsLoaded == true) { SystemThemeWatcher.Watch(window, WindowBackdropType.None, true); watching = true; }
        }
        else ApplicationThemeManager.Apply(ThemeCatalog.Get(Mode).IsLight ? ApplicationTheme.Light : ApplicationTheme.Dark, WindowBackdropType.None, true);
        OnThemeChanged(ApplicationThemeManager.GetAppTheme(), default);
    }
    private void OnThemeChanged(ApplicationTheme theme, Color accent)
    {
        IsHighContrast = ReadWindowsHighContrast() || theme == ApplicationTheme.HighContrast;
        IsLight = !IsHighContrast && (Mode == ThemeMode.System ? theme == ApplicationTheme.Light : ThemeCatalog.Get(Mode).IsLight);
        var resources = Application.Current.Resources;
        paletteResources.Reset(resources);
        resources["CubeSurfaceBorderThickness"] = new Thickness(IsLight && !IsHighContrast ? 1 : 0);
        resources["CubeSurfaceBorderBrush"] = IsLight && !IsHighContrast
            ? new SolidColorBrush(Color.FromRgb(226, 226, 226)) : Brushes.Transparent;
        bool lightBackground = IsHighContrast
            ? SystemColors.WindowColor.R * 0.299 + SystemColors.WindowColor.G * 0.587 + SystemColors.WindowColor.B * 0.114 >= 128
            : IsLight;
        resources["CubeAppIcon"] = lightBackground ? BlackIcon : WhiteIcon;
        RefreshWindowIcon();
        void Set(string key, Brush highContrast)
            => resources[key] = IsHighContrast ? highContrast : new SolidColorBrush((Color)resources[key + (IsLight ? ".Light" : ".Dark")]);
        resources["CubeFloatingShadowOpacity"] = IsHighContrast ? 0d : IsLight ? .22d : .48d;
        Set("BackingCard", SystemColors.WindowBrush);
        Set("BackingBorder", SystemColors.WindowTextBrush);
        Set("BackingSecondary", SystemColors.WindowTextBrush);
        Set("BackingPlayhead", SystemColors.HighlightBrush);
        Set("BackingPlayed", SystemColors.HighlightBrush);
        Set("BackingFuture", SystemColors.WindowTextBrush);
        Set("CubePhaserGraph", SystemColors.WindowTextBrush);
        Set("CubePurple", SystemColors.WindowTextBrush);
        Set("CubeAmpAccent", SystemColors.WindowTextBrush);
        Set("CubeAmpBandA", SystemColors.WindowTextBrush);
        Set("CubeAmpBandB", SystemColors.WindowTextBrush);
        Set("CubeCapacitorAccent", SystemColors.WindowTextBrush);
        Set("CubeCapacitorDisplay", SystemColors.WindowBrush);
        Set("CubeKnobBase", SystemColors.WindowBrush);
        Set("CubeKnobTop", SystemColors.WindowBrush);
        Set("CubeKnobBottom", SystemColors.WindowBrush);
        Set("CubeKnobTrack", SystemColors.WindowTextBrush);
        Set("CubeComboField", SystemColors.WindowBrush);
        Set("CubeComboFieldHover", SystemColors.WindowBrush);
        Set("CubePopupSurface", SystemColors.WindowBrush);
        Set("CubePopupHover", SystemColors.HighlightBrush);
        Set("CubePopupSelected", SystemColors.HighlightBrush);
        Set("CubePopupBorder", SystemColors.WindowTextBrush);
        resources["CubePopupHoverText"] = IsHighContrast ? SystemColors.HighlightTextBrush : Brush("TextFillColorPrimaryBrush");
        resources["CubePopupShadowOpacity"] = IsHighContrast ? 0d : .135d;
        Set("CubeItemRest", SystemColors.WindowBrush);
        Set("CubeItemHover", SystemColors.WindowBrush);
        Set("CubeSelectedBrush", SystemColors.HighlightBrush);
        // Keep the library palette intact; only adapt the app's own semantic keys.
        foreach (var alias in new[] { "CubeCanvas", "CubeText", "CubeCard", "CubeFloatingSurface", "CubeInset", "CubeWell", "CubeBorder", "CubeSelected", "CubeSelectedText", "CubeFocus", "CubeAccent", "Muted", "CubeDisabledText", "CubeWarning" })
        {
            string source = (string)resources["CubePalette." + alias];
            resources[alias] = Brush(source);
        }
        if (ActivePalette is { } palette) paletteResources.Apply(resources, palette);
        if (IsHighContrast)
        {
            foreach (string key in new[] { "CubeCanvas", "CubeCard", "CubeFloatingSurface", "CubeInset", "CubeWell" }) resources[key] = SystemColors.WindowBrush;
            foreach (string key in new[] { "CubeText", "Muted", "CubeWarning", "CubeBorder" }) resources[key] = SystemColors.WindowTextBrush;
            resources["CubeSelected"] = resources["CubeFocus"] = resources["CubeAccent"] = SystemColors.HighlightBrush;
            resources["CubeSelectedText"] = SystemColors.HighlightTextBrush;
            resources["CubeDisabledText"] = SystemColors.GrayTextBrush;
        }
        resources["BackingMuteActive"] = IsHighContrast ? SystemColors.WindowTextBrush : new SolidColorBrush(IsLight ? Color.FromRgb(164, 76, 12) : Color.FromRgb(255, 169, 82));
        resources["BackingSoloActive"] = IsHighContrast ? SystemColors.WindowTextBrush : new SolidColorBrush(IsLight ? Color.FromRgb(128, 98, 0) : Color.FromRgb(255, 219, 77));
        resources["CubeSelectionIndicator"] = IsHighContrast ? SystemColors.HighlightTextBrush : resources["CubeFocus"];
        ControlArtwork.Apply(resources, Finish, IsLight, IsHighContrast);
        RefreshBackdrop();
        resources["CubeThemeRevision"] = ++revision;
        Changed?.Invoke();
    }
    public static Brush Brush(string key) => Application.Current.TryFindResource(key) as Brush ?? Brushes.Gray;
    public static string ModuleColor(string color)
    {
        if (Instance.IsHighContrast) return SystemColors.WindowTextColor.ToString();
        if (!Instance.IsLight) return color;
        return color.ToUpperInvariant() switch
        {
            "#D5AD7D" => "#82551F", "#80B8A8" => "#226E59", "#D9A071" => "#965622", "#82ACD2" => "#2D638B", "#B49ACF" => "#765394", "#C5B77F" => "#726021", _ => "#53626D"
        };
    }
}
