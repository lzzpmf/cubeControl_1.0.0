using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cube.Desktop;
using Cube.Desktop.Appearance;
using Cube.Desktop.Controls;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;
using Cube.Protocol;
using Wpf.Ui.Appearance;

internal static class UiAuditTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        Console.WriteLine("PASS " + message);
    }

    private static IEnumerable<T> Find<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match) yield return match;
            foreach (var nested in Find<T>(child)) yield return nested;
        }
    }

    private static void Disabled(Action action)
    {
        try { action(); throw new Exception("Disabled automation must reject editing"); }
        catch (ElementNotEnabledException) { }
    }

    public static async Task Run()
    {
        string temporary = Path.Combine(Path.GetTempPath(), "cube-ui-audit-" + Guid.NewGuid().ToString("N"));
        string folder = Path.GetFullPath("C:/CubeControl-Build/reports/wpf-ui-audit/after");
        Directory.CreateDirectory(folder);
        var previousTheme = ThemeService.Instance.Mode;
        string previousLanguage = LanguageService.Instance.Language;
        using var fake = new FakeConnection();
        fake.Bank[0] = 2; fake.Bank[1] = 4; fake.Bank[2] = 8; fake.Bank[8] = 8;
        using var session = new CubeSession(fake); await session.Identify();
        using var model = new EditorViewModel(temporary);
        await model.Execute(() => model.AttachSession(session));
        model.Ask = _ => Task.FromResult(true);
        var window = new MainWindow(model) { ShowInTaskbar = false };
        window.Show();
        await Task.Delay(200); while (model.Busy) await Task.Delay(30);
        var tabs = (TabControl)window.FindName("Tabs");
        var outer = (ScrollViewer)window.FindName("WorkspaceScroll");
        var host = (Wpf.Ui.Controls.ContentDialogHost)window.FindName("DialogHost");
        async Task Settle()
        {
            window.UpdateLayout(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout();
        }
        void Capture(string name, double dpi = 1)
        {
            var bitmap = new RenderTargetBitmap((int)Math.Ceiling(window.ActualWidth * dpi), (int)Math.Ceiling(window.ActualHeight * dpi), 96 * dpi, 96 * dpi, PixelFormats.Pbgra32);
            bitmap.Render(window);
            var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var file = File.Create(Path.Combine(folder, name + ".png")); encoder.Save(file);
        }
        try
        {
            tabs.SelectedIndex = 1; await Settle();
            var scene = (MicrophoneDistance)window.FindName("IrMicrophone");
            var thumb = Find<Thumb>(scene).Single();
            var peer = UIElementAutomationPeer.CreatePeerForElement(thumb)!;
            var range = (IRangeValueProvider)peer.GetPattern(PatternInterface.RangeValue)!;
            foreach (var (language, name) in new[] { ("pt-BR", "Distância do microfone"), ("en-US", "Microphone distance") })
            {
                LanguageService.Instance.Preview(language); await Settle();
                Check(peer.GetName() == name && peer.GetAutomationControlType() == AutomationControlType.Slider, "microphone localized UIA name and slider role " + language);
            }
            Check(range.Minimum == 0 && range.Maximum == 100 && range.SmallChange == 1, "microphone exposes real normalized limits without invented physical units");
            range.SetValue(63);
            Check(model.Ir.Distance == 63 && fake.Writes == 0, "accessible distance prepares locally when preview is unavailable");
            scene.IsEnabled = false; Disabled(() => range.SetValue(40)); scene.ClearValue(UIElement.IsEnabledProperty);
            Check(range.Value == 63, "disabled microphone rejects accessibility edits");
            thumb.BringIntoView(); thumb.Focus(); await Settle();
            var focusFrame = (Border)thumb.Template.FindName("FocusFrame", thumb);
            Check(thumb.IsKeyboardFocused && focusFrame.Background != null && focusFrame.Background != Brushes.Transparent && focusFrame.BorderThickness == new Thickness(0), "microphone keyboard focus uses a fill without an outline");
            Capture("microphone-focus");

            var knob = new Knob { Minimum = 0, Maximum = 127, Value = 10 };
            int starts = 0, ends = 0;
            knob.AddHandler(Knob.GestureStartedEvent, new RoutedEventHandler((_, _) => starts++));
            knob.AddHandler(Knob.GestureEndedEvent, new RoutedEventHandler((_, _) => ends++));
            var knobRange = (IRangeValueProvider)UIElementAutomationPeer.CreatePeerForElement(knob)!.GetPattern(PatternInterface.RangeValue)!;
            knobRange.SetValue(32);
            Check(knob.Value == 32 && starts == 1 && ends == 1, "accessible knob change forms one complete undo gesture");
            knob.IsEnabled = false; Disabled(() => knobRange.SetValue(40));
            Check(knob.Value == 32 && starts == 1 && ends == 1, "disabled knob never starts an edit");

            tabs.SelectedIndex = 0; model.SelectedStage = 0; await Settle();
            var image = (GearImageButton)window.FindName("EquipmentImageButton");
            var art = (GearArt)window.FindName("FocusedArt");
            var imagePeer = UIElementAutomationPeer.CreatePeerForElement(image)!;
            Check(imagePeer.GetPattern(PatternInterface.Invoke) is IInvokeProvider && imagePeer.IsControlElement(), "equipment image exposes native button invocation");
            Check(art.ActualWidth > 100 && art.ActualHeight > 100, "native image button preserves visible artwork dimensions");
            image.BringIntoView(); image.Focus();
            ((IInvokeProvider)imagePeer.GetPattern(PatternInterface.Invoke)!).Invoke();
            await Task.Delay(200); await Settle();
            var invokedDialog = Find<Wpf.Ui.Controls.ContentDialog>(window).Single();
            Check(invokedDialog.Content is Cube.Desktop.Views.GearGallery, "UIA Invoke opens the real equipment picker");
            var cancelButton = Find<System.Windows.Controls.Button>(invokedDialog).Single(b => b.IsCancel);
            Check(!string.IsNullOrWhiteSpace(cancelButton.Content?.ToString()), "picker retains the native Escape/cancel button");
            ((IInvokeProvider)UIElementAutomationPeer.CreatePeerForElement(cancelButton)!.GetPattern(PatternInterface.Invoke)!).Invoke();
            await Task.Delay(150); await Settle();
            Check(!Find<Cube.Desktop.Views.GearGallery>(window).Any() && fake.Writes == 0, "native cancel closes without applying equipment");
            model.SelectedStage = 2; await Settle();
            Check(imagePeer.IsControlElement() && image.IsTabStop && image.IsHitTestVisible && ContextMenuService.GetIsEnabled(image), "effect image supports keyboard customization and context menu");
            model.SelectedStage = 0; await Settle();
            var quick = (Wpf.Ui.Controls.CardAction)window.FindName("EquipmentPickerCard");
            var defaultStyle = (Style)Application.Current.FindResource(typeof(Wpf.Ui.Controls.CardAction));
            var reference = new Wpf.Ui.Controls.CardAction { Style = defaultStyle };
            Check(ReferenceEquals(quick.Template, reference.Template) && !quick.IsChevronVisible, "quick action uses library template without replacing its states");

            image.BringIntoView(); image.Focus(); await Settle();
            var gallery = new Cube.Desktop.Views.GearGallery(model.Amps, model.SelectedAmp, true);
            var dialog = new Wpf.Ui.Controls.ContentDialog(host) { Title = "Gallery audit", Content = gallery, PrimaryButtonText = "Aplicar seleção", CloseButtonText = "Cancelar", DialogMaxWidth = 900 };
            var shown = dialog.ShowLocalizedAsync(); await Task.Delay(200); await Settle();
            var surface = GalleryDialogDismissal.Surface(dialog)!;
            Check(surface != null, "WPF UI 4.3 gallery surface contract resolves");
            Check(!GalleryDialogDismissal.IsOutside(dialog, surface!.TranslatePoint(new Point(surface.ActualWidth / 2, surface.ActualHeight / 2), dialog)), "gallery content clicks do not dismiss");
            Check(GalleryDialogDismissal.IsOutside(dialog, surface.TranslatePoint(new Point(-5, -5), dialog)), "gallery backdrop is distinct from its content");
            dialog.Hide(); await shown; await Settle();
            Check(image.IsKeyboardFocused, "localized dialog returns focus to its invoking image button");
            Check(fake.Writes == 0, "canceling gallery and accessibility inspection send no MIDI");

            async Task InspectNotice()
            {
                await Task.Delay(150);
                try
                {
                    await window.ShowNoticeAsync("Duplicate");
                    var notices = Application.Current.Windows.OfType<Wpf.Ui.Controls.MessageBox>().ToArray();
                    Check(notices.Length == 1 && notices[0].Owner == window, $"notice is owned, themed and guarded against duplicate opening (count={notices.Length})");
                }
                finally
                {
                    foreach (var notice in Application.Current.Windows.OfType<Wpf.Ui.Controls.MessageBox>().ToArray())
                        ((Window)notice).Close();
                }
            }
            var inspection = InspectNotice();
            await window.ShowNoticeAsync("Aguarde a operação terminar antes de fechar.");
            await inspection;

            // Layout uses DIPs; rasterize at each DPI without applying a second layout reduction.
            foreach (var theme in new[] { ApplicationTheme.Light, ApplicationTheme.Dark, ApplicationTheme.HighContrast })
            foreach (string language in new[] { "pt-BR", "en-US" })
            {
                ApplicationThemeManager.Apply(theme); LanguageService.Instance.Preview(language);
                foreach (var (width, height) in new[] { (640, 480), (920, 740), (1280, 720), (1920, 1080) })
                {
                    window.Width = width; window.Height = height; await Settle();
                    for (int screen = 0; screen < 8; screen++)
                    {
                        tabs.SelectedIndex = screen < 6 ? 0 : screen - 5;
                        if (screen < 6) model.SelectedStage = screen;
                        await Settle();
                        var page = (ScrollViewer)window.FindName(new[] { "EditorScroll", "IrScroll", "HelpScroll" }[tabs.SelectedIndex]);
                        if (outer.ExtentWidth > outer.ViewportWidth + 1 || page.ExtentWidth > page.ViewportWidth + 1)
                            throw new Exception($"Horizontal clipping {theme}/{language}/{width}/{screen}");
                        outer.ScrollToEnd(); page.ScrollToEnd(); await Settle();
                        if (Math.Abs(outer.VerticalOffset - outer.ScrollableHeight) > 1 || Math.Abs(page.VerticalOffset - page.ScrollableHeight) > 1)
                            throw new Exception("Content end is unreachable");
                        foreach (double dpi in new[] { 1d, 1.25, 1.5, 2 })
                            Capture($"{theme}-{language}-{width}-screen{screen}-dpi{(int)(dpi * 100)}", dpi);
                    }
                    foreach (var content in new FrameworkElement[] {
                        new Cube.Desktop.Views.GearGallery(model.Amps, model.SelectedAmp, true),
                        new Cube.Desktop.Views.GearGallery(model.Cabinets, model.SelectedCabinet, false),
                        new Cube.Desktop.Views.MatchSettings(model.CreateMatchRows()),
                        new Cube.Desktop.Views.ConnectionDetails { DataContext = model } })
                    {
                        dialog = new Wpf.Ui.Controls.ContentDialog(host) { Title = "CubeControl", Content = content, CloseButtonText = "Cancelar", DialogMaxWidth = 900 };
                        shown = dialog.ShowLocalizedAsync(); await Task.Delay(40); await Settle();
                        if (content is Cube.Desktop.Views.ConnectionDetails ports)
                        {
                            foreach (string port in new[] { "InputPort", "OutputPort" })
                            {
                                var combo = (ComboBox)ports.FindName(port);
                                if (AutomationProperties.GetLabeledBy(combo) == null || string.IsNullOrWhiteSpace(UIElementAutomationPeer.CreatePeerForElement(combo)!.GetName()))
                                    throw new Exception("USB selector has no accessible label");
                            }
                        }
                        if (content is Cube.Desktop.Views.MatchSettings)
                            foreach (var combo in Find<ComboBox>(content))
                                if (AutomationProperties.GetLabeledBy(combo) == null || string.IsNullOrWhiteSpace(AutomationProperties.GetName(combo)))
                                    throw new Exception("Cabinet association has no amplifier label");
                        var body = Find<ScrollViewer>(dialog).Single(s => s.Name == "PART_ContentScroll");
                        if (content.ActualWidth > body.ViewportWidth + 1) throw new Exception("Dialog overflow " + content.GetType().Name);
                        string kind = content is Cube.Desktop.Views.GearGallery picker ? (picker.Selected?.Kind ?? "gallery") : content.GetType().Name;
                        Capture($"{theme}-{language}-{width}-dialog-{kind}");
                        body.ScrollToEnd(); await Settle();
                        if (Math.Abs(body.VerticalOffset - body.ScrollableHeight) > 1) throw new Exception("Dialog end is unreachable");
                        dialog.Hide(); await shown;
                    }
                    Console.WriteLine($"PASS visual matrix {theme}/{language}/{width}: eight screens at 100/125/150/200 percent raster DPI and four dialogs");
                }
            }
            Check(fake.Writes == 0, "entire visual and accessibility audit leaves MIDI untouched");

        }
        finally
        {
            ThemeService.Instance.Preview(previousTheme); LanguageService.Instance.Preview(previousLanguage);
            window.Close(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var resolved = Path.GetFullPath(temporary);
            if (resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) && Path.GetFileName(resolved).StartsWith("cube-ui-audit-")) Directory.Delete(resolved, true);
        }
    }
}
