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

public enum ThemeMode { System, Light, Dark }
public enum ControlFinish
{
    Official, BrushedSilver, RetroIvory, GraphiteCopper,
    BlackIvory, IndustrialBlack, TexturedSteel, MarconiBlack, VintageBrass, IvoryGold, ChromeSkullBlue
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
    private readonly string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CubeControl", "appearance-v1.json");
    private Window? window;
    private bool watching;
    private HwndSource? iconSource;
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
            bool enabled = !IsHighContrast && !SystemParameters.HighContrast && ReadWindowsTransparency()
                && WindowBackdrop.IsSupported(WindowBackdropType.Mica);
            UsesBackdrop = enabled;
            Application.Current.Resources["CubeWindowBackground"] = enabled ? Brushes.Transparent : Brush("CubeCanvas");
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
        if (message is 0x001A or 0x031A) { RefreshWindowIcon(); RefreshBackdrop(); } // WM_SETTINGCHANGE / WM_THEMECHANGED
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
        Mode = mode; Apply();
    }
    private void Apply()
    {
        StopWatching();
        if (Mode == ThemeMode.System)
        {
            ApplicationThemeManager.ApplySystemTheme();
            if (window?.IsLoaded == true) { SystemThemeWatcher.Watch(window, WindowBackdropType.None, true); watching = true; }
        }
        else ApplicationThemeManager.Apply(Mode == ThemeMode.Light ? ApplicationTheme.Light : ApplicationTheme.Dark, WindowBackdropType.None, true);
        OnThemeChanged(ApplicationThemeManager.GetAppTheme(), default);
    }
    private void OnThemeChanged(ApplicationTheme theme, Color accent)
    {
        IsHighContrast = theme == ApplicationTheme.HighContrast;
        IsLight = theme == ApplicationTheme.Light;
        var resources = Application.Current.Resources;
        // Keep native WPF UI button templates and state transitions. Only the light
        // secondary palette needs stronger separation from the near-white cards.
        foreach (string key in new[] { "ButtonBackground", "ButtonBackgroundPointerOver", "ButtonBackgroundPressed", "ButtonForeground", "ButtonForegroundPressed" })
            resources.Remove(key);
        if (IsLight)
        {
            resources["ButtonBackground"] = new SolidColorBrush(Color.FromRgb(226, 226, 226));
            resources["ButtonBackgroundPointerOver"] = new SolidColorBrush(Color.FromRgb(211, 211, 211));
            resources["ButtonBackgroundPressed"] = new SolidColorBrush(Color.FromRgb(196, 196, 196));
        }
        resources["CubeSurfaceBorderThickness"] = new Thickness(IsHighContrast ? 1 : 0);
        Brush controlBorder = IsHighContrast ? SystemColors.WindowTextBrush : Brushes.Transparent;
        static IEnumerable<string> BorderResources(ResourceDictionary dictionary)
        {
            foreach (var merged in dictionary.MergedDictionaries)
                foreach (string key in BorderResources(merged)) yield return key;
            foreach (string key in dictionary.Keys.OfType<string>())
                if (key.Contains("BorderBrush", StringComparison.Ordinal)) yield return key;
        }
        foreach (string key in BorderResources(resources).Distinct().ToArray()) resources[key] = controlBorder;
        // Normal themes use fills; high contrast retains visible system-colored outlines.
        foreach (string key in new[] { "ControlElevationBorderBrush", "AccentControlElevationBorderBrush", "ButtonBorderBrushDisabled", "ButtonBorderBrushPressed", "CardBorderBrush", "CardBorderBrushDisabled", "CardBorderBrushPressed", "ComboBoxBorderBrushDisabled", "ComboBoxBorderBrushFocused", "ComboBoxDropDownBorderBrush", "TextControlBorderBrush", "TextControlBorderBrushPointerOver", "TextControlBorderBrushFocused", "TextControlBorderBrushDisabled", "ControlStrokeColorDefaultBrush", "ControlStrokeColorSecondaryBrush", "ControlStrongStrokeColorDefaultBrush", "ControlStrongStrokeColorDisabledBrush" })
            resources[key] = controlBorder;
        bool lightBackground = IsHighContrast
            ? SystemColors.WindowColor.R * 0.299 + SystemColors.WindowColor.G * 0.587 + SystemColors.WindowColor.B * 0.114 >= 128
            : IsLight;
        resources["CubeAppIcon"] = lightBackground ? BlackIcon : WhiteIcon;
        RefreshWindowIcon();
        void Set(string key, string dark, string light, Brush? highContrast = null)
            => resources[key] = IsHighContrast && highContrast != null ? highContrast : new SolidColorBrush((Color)ColorConverter.ConvertFromString(IsLight ? light : dark));
        Set("CubeCanvas", "#202020", "#F3F3F3", SystemColors.WindowBrush);
        resources["CubeText"] = IsHighContrast ? SystemColors.WindowTextBrush : Brush("TextFillColorPrimaryBrush");
        Set("CubeCard", "#2B2B2B", "#FBFBFB", SystemColors.WindowBrush);
        Set("CubeFloatingSurface", "#333333", "#FFFFFF", SystemColors.WindowBrush);
        resources["CubeFloatingShadowOpacity"] = IsHighContrast ? 0d : IsLight ? .22d : .48d;
        Set("CubeInset", "#252525", "#F5F5F5", SystemColors.WindowBrush);
        Set("CubeWell", "#1C1C1C", "#EFEFEF", SystemColors.WindowBrush);
        Set("CubeBorder", "#404040", "#E0E0E0", SystemColors.WindowTextBrush);
        Set("CubeSelected", "#3B3B3B", "#E9E9E9", SystemColors.HighlightBrush);
        Set("CubeSelectedText", "#FFFFFF", "#1B1B1B", SystemColors.HighlightTextBrush);
        Set("CubeFocus", "#FFFFFF", "#1A1A1A", SystemColors.HighlightBrush);
        Set("BackingCard", "#292D31", "#FFFFFF", SystemColors.WindowBrush);
        Set("BackingBorder", "#4C545C", "#BBC3CB", SystemColors.WindowTextBrush);
        Set("BackingSecondary", "#D4DAE0", "#3C4650", SystemColors.WindowTextBrush);
        Set("BackingPlayhead", "#FFD280", "#815000", SystemColors.HighlightBrush);
        Set("BackingPlayed", "#80B8A8", "#226E59", SystemColors.HighlightBrush);
        Set("BackingFuture", "#82ACD2", "#2D638B", SystemColors.WindowTextBrush);
        Set("CubeAccent", "#BDBDBD", "#626262", SystemColors.HighlightBrush);
        resources["Muted"] = IsHighContrast ? SystemColors.WindowTextBrush : Brush("TextFillColorSecondaryBrush");
        resources["CubeDisabledText"] = IsHighContrast ? SystemColors.GrayTextBrush : Brush("TextFillColorDisabledBrush");
        resources["CubeWarning"] = IsHighContrast ? SystemColors.WindowTextBrush : Brush("SystemFillColorCautionBrush");
        Set("CubePhaserGraph", "#D9A071", "#965622", SystemColors.WindowTextBrush);
        Set("CubePurple", "#B4A4D6", "#67508F", SystemColors.WindowTextBrush);
        Set("CubeKnobBase", "#202020", "#DCE3E8", SystemColors.WindowBrush);
        Set("CubeKnobTop", "#555555", "#FFFFFF", SystemColors.WindowBrush);
        Set("CubeKnobBottom", "#292929", "#C6D0D8", SystemColors.WindowBrush);
        Set("CubeKnobTrack", "#666666", "#8798A3", SystemColors.WindowTextBrush);
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
