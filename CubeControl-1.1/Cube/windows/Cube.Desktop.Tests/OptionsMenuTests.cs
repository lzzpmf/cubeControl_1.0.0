using System.IO;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cube.Desktop;
using Cube.Desktop.Appearance;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;
using Cube.Protocol;
using Wpf.Ui.Appearance;

internal static class OptionsMenuTests
{
    private static IEnumerable<T> Find<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T item) yield return item;
            foreach (var nested in Find<T>(child)) yield return nested;
        }
    }

    public static async Task Run()
    {
        string temporary = Path.Combine(Path.GetTempPath(), "cube-options-" + Guid.NewGuid().ToString("N"));
        string folder = Path.GetFullPath(TestPaths.Report("C:/CubeControl-Build/reports/options-menu")); Directory.CreateDirectory(folder);
        int initialZoom = ZoomService.Instance.Percent;
        ZoomService.Instance.Preview(100);
        var initialTheme = ThemeService.Instance.Mode;
        var initialFinish = ThemeService.Instance.Finish;
        var initialLanguage = LanguageService.Instance.Language;
        using var fake = new FakeConnection();
        using var session = new CubeSession(fake); await session.Identify();
        using var model = new EditorViewModel(temporary);
        await model.Execute(() => model.AttachSession(session));
        var window = new MainWindow(model, initializeDevices: false) { ShowInTaskbar = false };
        window.SelectZoom = ZoomService.Instance.Preview;
        window.SelectControlFinish = ThemeService.Instance.PreviewFinish;
        window.Show(); await Task.Delay(150); while (model.Busy) await Task.Delay(30);
        var button = (Wpf.Ui.Controls.Button)window.FindName("MoreOptionsButton");
        var header = (Grid)window.FindName("HeaderGrid");
        var peer = UIElementAutomationPeer.CreatePeerForElement(button)!;
        async Task Settle() { window.UpdateLayout(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout(); }
        void Check(bool valid, string name) { if (!valid) throw new Exception(name); Console.WriteLine("PASS " + name); }
        void Capture(FrameworkElement visual, string name)
        {
            var bitmap = new RenderTargetBitmap((int)Math.Ceiling(visual.ActualWidth), (int)Math.Ceiling(visual.ActualHeight), 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual); var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
            using var output = File.Create(Path.Combine(folder, name + ".png")); png.Save(output);
        }
        async Task<ContextMenu> Open()
        {
            // Allow native popup close animations and input capture to finish between invocations.
            await Task.Delay(700); await Settle();
            button.BringIntoView(); button.Focus();
            ((IInvokeProvider)peer.GetPattern(PatternInterface.Invoke)!).Invoke();
            await Task.Delay(400); await Settle();
            return button.ContextMenu;
        }
        try
        {
            foreach (int stage in new[] { 0, 5 })
            {
                model.SelectedStage = stage; await Settle();
                var catalog = (Wpf.Ui.Controls.CardAction)window.FindName("EquipmentPickerCard");
                Check(catalog.Icon is Wpf.Ui.Controls.SymbolIcon { Symbol: Wpf.Ui.Controls.SymbolRegular.Library24 }, "amp and cabinet catalogs share the library icon");
            }
            foreach (var theme in new[] { ApplicationTheme.Light, ApplicationTheme.Dark, ApplicationTheme.HighContrast })
            foreach (string language in new[] { "pt-BR", "en-US" })
            {
                ApplicationThemeManager.Apply(theme); LanguageService.Instance.Preview(language);
                window.Width = 1280; window.Height = 720; await Settle();
                Check(peer.GetName() == LanguageService.T("Mais opções"), "localized accessible ellipsis name " + language);
                Check(!Find<Wpf.Ui.Controls.Button>(header).Any(b => b.Content?.ToString() is "Tema" or "Idioma" or "Theme" or "Language"), "standalone preference buttons removed");
                var menu = await Open();
                var menuSurface = (Border)menu.Template.FindName("Border", menu);
                Check(menuSurface.Effect == null && TextOptions.GetTextFormattingMode(menuSurface) == TextFormattingMode.Display, "menu text is rendered outside the shadow effect");
                Check(VisualTreeHelper.GetParent(menuSurface) is Grid layers && layers.Children.OfType<Border>().Any(b => b.Name == "PopupShadowLayer" && b.Child == null && !b.IsHitTestVisible), "menu shadow is a separate noninteractive background");
                Check(menu.IsOpen && menu.Items.Count == 5 && menu.Items[3] is Separator, "ellipsis opens theme, language, finish and zoom preferences");
                var themeItem = (MenuItem)menu.Items[0]; var languageItem = (MenuItem)menu.Items[1];
                var finishItem = (MenuItem)menu.Items[2];
                Check(finishItem.Header?.ToString() == LanguageService.T("Acabamento dos controles") && finishItem.Items.Count == 11, "localized finish submenu contains all eleven finishes");
                Check(finishItem.Items.OfType<MenuItem>().Count(i => i.Tag is true) == 1, "exactly one finish is highlighted");
                Check(themeItem.Header?.ToString() == LanguageService.T("Tema") && languageItem.Header?.ToString() == LanguageService.T("Idioma"), "menu labels follow current language");
                Check(themeItem.Items.OfType<MenuItem>().Count(i => i.Tag is true) == 1 && languageItem.Items.OfType<MenuItem>().Count(i => i.Tag is true) == 1, "exactly one theme and one language are highlighted");
                Check(themeItem.Items.OfType<MenuItem>().Concat(languageItem.Items.OfType<MenuItem>()).Concat(finishItem.Items.OfType<MenuItem>()).All(i => !i.IsCheckable && !i.IsChecked), "preferences have no checkboxes");
                Check(themeItem.Items.OfType<MenuItem>().Concat(languageItem.Items.OfType<MenuItem>()).Concat(finishItem.Items.OfType<MenuItem>()).Where(i => i.Tag is true).All(i => i.FontWeight == FontWeights.SemiBold), "active preferences use visible emphasis");
                Check(model.Ir.Slot == 0, "destination defaults to slot 1");
                Check(menu.LayoutTransform is ScaleTransform { ScaleX: .9, ScaleY: .9 }, "menu keeps existing 90 percent density");
                Capture(window, $"header-{theme}-{language}");
                Capture((FrameworkElement)PresentationSource.FromVisual(menu)!.RootVisual, $"menu-{theme}-{language}");
                await Task.Delay(300); themeItem.Focus();
                ((IExpandCollapseProvider)UIElementAutomationPeer.CreatePeerForElement(themeItem)!.GetPattern(PatternInterface.ExpandCollapse)!).Expand();
                var first = (MenuItem)themeItem.Items[0];
                for (int attempt = 0; PresentationSource.FromVisual(first) == null && attempt < 20; attempt++)
                {
                    themeItem.IsSubmenuOpen = true; await Task.Delay(50); await Settle();
                }
                Check(PresentationSource.FromVisual(first) != null, "theme submenu opens through accessibility");
                var popupRoot = (FrameworkElement)PresentationSource.FromVisual(first)!.RootVisual;
                var submenuRight = popupRoot.PointToScreen(new Point(popupRoot.ActualWidth, 0)).X;
                Check(submenuRight <= window.PointToScreen(new Point(window.ActualWidth, 0)).X + 1, "submenu stays inside window right edge");
                Capture(popupRoot, $"theme-submenu-{theme}-{language}");
                themeItem.IsSubmenuOpen = false;
                finishItem.IsSubmenuOpen = true;
                await Task.Delay(200); await Settle();
                var firstFinish = (MenuItem)finishItem.Items[0];
                Check(PresentationSource.FromVisual(firstFinish) != null, "finish submenu opens through keyboard navigation");
                var finishRoot = (FrameworkElement)PresentationSource.FromVisual(firstFinish)!.RootVisual;
                Check(finishRoot.PointToScreen(new Point(finishRoot.ActualWidth, 0)).X <= window.PointToScreen(new Point(window.ActualWidth, 0)).X + 1, "finish submenu stays inside window right edge");
                Capture(finishRoot, $"finish-submenu-{theme}-{language}");
                finishItem.IsSubmenuOpen = false; menu.IsOpen = false; await Settle();
            Check(button.IsKeyboardFocused, "closing menu returns focus to ellipsis");
            }
            model.SelectedStage = 0;
            foreach (var mode in new[] { Cube.Desktop.Appearance.ThemeMode.Light, Cube.Desktop.Appearance.ThemeMode.Dark })
            foreach (var finish in Enum.GetValues<ControlFinish>())
            foreach (var (width, zoom) in new[] { (640, 100), (1280, 150) })
            {
                ThemeService.Instance.Preview(mode); ThemeService.Instance.PreviewFinish(finish);
                window.Width = width; ZoomService.Instance.Preview(zoom);
                await Settle();
                Capture(window, $"controls-{mode}-{finish}-{width}-{zoom}");
            }
            ZoomService.Instance.Preview(100);
            // Preview updates selections without changing the user's preference files.
            ThemeService.Instance.Preview(Cube.Desktop.Appearance.ThemeMode.Light);
            LanguageService.Instance.Preview("en-US");
            var finishSelection = await Open();
            var finishParent = (MenuItem)finishSelection.Items[2];
            finishParent.IsSubmenuOpen = true; await Task.Delay(200); await Settle();
            var skull = (MenuItem)finishParent.Items[10];
            ((IInvokeProvider)UIElementAutomationPeer.CreatePeerForElement(skull)!.GetPattern(PatternInterface.Invoke)!).Invoke();
            await Task.Delay(200); await Settle();
            Check(ThemeService.Instance.Finish == ControlFinish.ChromeSkullBlue, "new archive finish menu action updates the existing controls");
            finishSelection.IsOpen = false;
            var reopened = await Open();
            Check(((MenuItem)((MenuItem)reopened.Items[2]).Items[10]).Tag is true, "reopening refreshes selected archive finish");
            Check(((MenuItem)((MenuItem)reopened.Items[0]).Items[1]).Tag is true, "reopening refreshes selected theme");
            Check(((MenuItem)reopened.Items[1]).Items.OfType<MenuItem>().Single(i => i.Tag is true).Header?.ToString() == "English", "reopening refreshes selected language");
            reopened.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(reopened), 0, Key.Escape) { RoutedEvent = Keyboard.KeyDownEvent });
            await Settle(); Check(!reopened.IsOpen, "Escape dismisses the native menu");

            foreach (int width in new[] { 640, 920, 1920 })
            {
                window.Width = width; await Settle();
                var bounds = button.TransformToAncestor(header).TransformBounds(new Rect(button.RenderSize));
                Check(bounds.Left >= 0 && bounds.Right <= header.ActualWidth + 1 && button.ActualWidth * .9 >= 32, "ellipsis remains reachable at " + width);
                var menu = await Open(); Capture(window, "header-width-" + width);
                var menuRoot = (FrameworkElement)menu.Template.FindName("Border", menu);
                double alignmentError = menuRoot.PointToScreen(new Point(menuRoot.ActualWidth, 0)).X - button.PointToScreen(new Point(button.ActualWidth, 0)).X;
                double gap = menuRoot.PointToScreen(new Point()).Y - button.PointToScreen(new Point(0, button.ActualHeight)).Y;
                Check(gap >= 0 && gap <= 12, "menu stays attached below button (gap " + gap + ")");
                Check(Math.Abs(alignmentError) <= 2, "menu surface aligns with ellipsis at " + width + " (error " + alignmentError + ")");
                Check(menuRoot.PointToScreen(new Point(menuRoot.ActualWidth, 0)).X <= window.PointToScreen(new Point(window.ActualWidth, 0)).X + 1, "options menu stays inside window right edge at " + width);
                Capture((FrameworkElement)PresentationSource.FromVisual(menu)!.RootVisual, "menu-width-" + width);
                menu.IsOpen = false;
            }
            string zoomPath = Path.Combine(temporary, "zoom-test.json");
            Check(ZoomSettings.Load(zoomPath) == 100, "zoom defaults to existing app density");
            foreach (int percent in new[] { 50, 100, 150, 200 })
            { ZoomSettings.Save(zoomPath, percent); Check(ZoomSettings.Load(zoomPath) == percent, "zoom preference survives reload " + percent); }
            File.WriteAllText(zoomPath, "{broken"); Check(ZoomSettings.Load(zoomPath) == 100, "invalid zoom preference recovers");
            window.Width = 1280;
            var zoomMenu = await Open();
            var zoomRow = (MenuItem)zoomMenu.Items[4];
            var zoomButtons = Find<Wpf.Ui.Controls.Button>(zoomRow).ToArray();
            var less = zoomButtons.Single(b => b.Name == "ZoomOutButton");
            var more = zoomButtons.Single(b => b.Name == "ZoomInButton");
            var reset = zoomButtons.Single(b => b.Name == "ZoomResetButton");
            var label = Find<TextBlock>(zoomRow).Single(t => t.Name == "ZoomPercent");
            zoomRow.Focus(); await Settle();
            Check(less.IsKeyboardFocused, "keyboard navigation enters zoom controls");
            void Invoke(Wpf.Ui.Controls.Button control) => ((IInvokeProvider)UIElementAutomationPeer.CreatePeerForElement(control)!.GetPattern(PatternInterface.Invoke)!).Invoke();
            Invoke(more); await Task.Delay(250); await Settle();
            Check(ZoomService.Instance.Percent == 110 && label.Text == "110%" && zoomMenu.IsOpen, "zoom plus updates without closing menu");
            Check(((ScaleTransform)((Grid)window.FindName("ClientArea")).LayoutTransform).ScaleX == .99, "zoom changes client scale once");
            Invoke(less); await Task.Delay(250); await Settle();
            Check(ZoomService.Instance.Percent == 100 && !reset.IsEnabled, "zoom minus and default state");
            ZoomService.Instance.Preview(200); await Settle(); Check(!more.IsEnabled && less.IsEnabled, "zoom upper limit disables plus");
            Invoke(reset); await Task.Delay(250); await Settle(); Check(ZoomService.Instance.Percent == 100, "reset restores 100 percent");
            ZoomService.Instance.Preview(50); await Settle(); Check(!less.IsEnabled && more.IsEnabled, "zoom lower limit disables minus");
            Invoke(reset); await Task.Delay(250); await Settle();
            Capture((FrameworkElement)PresentationSource.FromVisual(zoomMenu)!.RootVisual, "zoom-controls");
            zoomMenu.IsOpen = false;
            var service = ThemeService.Instance;
            var transparencyReader = service.ReadWindowsTransparency;
            try
            {
                foreach (var theme in new[] { Cube.Desktop.Appearance.ThemeMode.Light, Cube.Desktop.Appearance.ThemeMode.Dark })
                {
                    service.Preview(theme);
                    service.ReadWindowsTransparency = () => false; service.RefreshBackdrop();
                    Check(!service.UsesBackdrop && ((SolidColorBrush)window.Background).Color.A == 255, "Windows transparency off uses opaque backdrop in " + theme);
                    service.ReadWindowsTransparency = () => true; service.RefreshBackdrop();
                    bool supported = Wpf.Ui.Controls.WindowBackdrop.IsSupported(Wpf.Ui.Controls.WindowBackdropType.Mica) && !SystemParameters.HighContrast;
                    Check(service.UsesBackdrop == supported, "Windows transparency on follows Mica support in " + theme);
                    if (supported) Check(((SolidColorBrush)window.Background).Color.A == 0, "Mica is visible behind root content");
                }
                ApplicationThemeManager.Apply(ApplicationTheme.HighContrast);
                Check(!service.UsesBackdrop && ((SolidColorBrush)window.Background).Color.A == 255, "high contrast always uses solid background");
            }
            finally { service.ReadWindowsTransparency = transparencyReader; service.Preview(initialTheme); }
            Check(fake.Writes == 0, "options navigation, themes and language previews send no MIDI");
        }
        finally
        {
            ZoomService.Instance.Preview(initialZoom);
            if (button.ContextMenu != null) button.ContextMenu.IsOpen = false;
            ThemeService.Instance.PreviewFinish(initialFinish); ThemeService.Instance.Preview(initialTheme); LanguageService.Instance.Preview(initialLanguage);
            window.Close(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            string resolved = Path.GetFullPath(temporary);
            if (resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) && Path.GetFileName(resolved).StartsWith("cube-options-")) Directory.Delete(resolved, true);
        }
    }
}
