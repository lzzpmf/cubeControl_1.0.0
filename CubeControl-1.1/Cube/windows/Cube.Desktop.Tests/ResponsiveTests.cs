using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cube.Desktop;
using Cube.Desktop.Appearance;
using Cube.Desktop.Controls;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;
using Cube.Protocol;

internal static class ResponsiveTests
{
    private static IEnumerable<T> Find<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match) yield return match;
            foreach (var nested in Find<T>(child)) yield return nested;
        }
    }

    private static void Check(bool valid, string description)
    {
        if (!valid) throw new Exception(description);
    }

    public static async Task Run()
    {
        string temporary = Path.Combine(Path.GetTempPath(), "cube-responsive-" + Guid.NewGuid().ToString("N"));
        bool compactOnly = Environment.GetCommandLineArgs().Contains("--zoom-compact-only");
        bool dialogsOnly = Environment.GetCommandLineArgs().Contains("--zoom-dialogs-only");
        bool zoomOnly = compactOnly || dialogsOnly || Environment.GetCommandLineArgs().Contains("--zoom-only");
        int originalZoom = ZoomService.Instance.Percent;
        bool volumeOnly = Environment.GetCommandLineArgs().Contains("--volume-only");
        string folder = Path.GetFullPath(volumeOnly ? "C:/CubeControl-Build/reports/volume-3.3.21" : "C:/CubeControl-Build/reports/responsive-3.3.19");
        Directory.CreateDirectory(folder);
        using var fake = new FakeConnection();
        fake.Bank[0] = 2; fake.Bank[1] = 4; fake.Bank[2] = 8; fake.Bank[8] = 8;
        using var session = new CubeSession(fake); await session.Identify();
        using var model = new EditorViewModel(temporary);
        await model.Execute(() => model.AttachSession(session));
        var window = new MainWindow(model) { ShowInTaskbar = false };
        window.Show();
        await Task.Delay(150);
        while (model.Busy) await Task.Delay(30);
        var workspace = (Grid)window.FindName("Workspace");
        var outer = (ScrollViewer)window.FindName("WorkspaceScroll");
        var tabs = (TabControl)window.FindName("Tabs");
        var chain = (ListBox)window.FindName("EffectChain");
        async Task Settle()
        {
            window.UpdateLayout(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout();
            foreach (var button in Find<Wpf.Ui.Controls.Button>(window).Where(b => b.IsVisible && b.Content is string { Length: > 0 }))
            {
                Check(button.ActualHeight >= 39.5, $"Command button is vertically compressed: {button.Content}, height {button.ActualHeight}");
                Check(button.Padding.Top >= 8 && button.Padding.Bottom >= 8, $"Command text needs vertical breathing room: {button.Content}");
                Check(button.VerticalContentAlignment == VerticalAlignment.Center, $"Command text must be centered: {button.Content}");
            }
        }
        void Capture(string name)
        {
            var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(window);
            var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
            using var file = File.Create(Path.Combine(folder, name + ".png")); png.Save(file);
        }
        try
        {
            if (volumeOnly)
            {
                model.Ask = _ => Task.FromResult(true);
                var knob = Find<Knob>((FrameworkElement)window.FindName("OutputGroup")).Single();
                foreach (string language in new[] { "pt-BR", "en-US" })
                foreach (int width in new[] { 640, 920, 1100, 1200, 1400 })
                {
                    LanguageService.Instance.Preview(language);
                    window.Width = width; window.Height = 940;
                    model.SelectedIndex = 0;
                    knob.Value = 0; await model.Flush(); await Settle();
                    outer.ScrollToTop(); await Settle();
                    var origin = knob.TranslatePoint(new Point(), window);
                    void Stable(string step)
                    {
                        var position = knob.TranslatePoint(new Point(), window);
                        Check((position - origin).Length < .5, $"Volume moved {position - origin} at {language}/{width}/{step}");
                    }
                    for (int i = 0; i < 12; i++)
                    {
                        knob.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, 120) { RoutedEvent = Mouse.MouseWheelEvent });
                        await Settle(); Stable("wheel/pending");
                        await model.Flush(); await Settle(); Stable("wheel/confirmed");
                    }
                    foreach (Key key in new[] { Key.End, Key.Left, Key.Home, Key.Right })
                    {
                        knob.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(window), 0, key) { RoutedEvent = Keyboard.KeyDownEvent });
                        await Settle(); Stable("keyboard");
                        await model.Flush(); await Settle(); Stable("keyboard/confirmed");
                    }
                    // Status text is presentation data and must never relocate a drag target.
                    foreach (string status in new[] { "Enviando…", "Aplicado ao aparelho · não gravado", "Estado do preset\nMensagem adicional" })
                    {
                        model.Current.State = status; await Settle(); Stable("status text");
                    }
                    Check(knob.Value == 1 && fake.Bank[5] == 1, "Volume binding must preserve the real 0-127 range and preset offset");
                    // Drive the same coordinate-based adjustment used by mouse drag,
                    // without moving the user's system pointer.
                    knob.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.MouseDownEvent });
                    try
                    {
                        Check(knob.IsMouseCaptured, "Dragging must capture the knob");
                        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                        var start = (Point)typeof(Knob).GetField("start", flags)!.GetValue(knob)!;
                        var adjust = typeof(Knob).GetMethod("Adjust", flags)!;
                        var target = new Point(start.X + 20, start.Y - 20);
                        adjust.Invoke(knob, new object[] { target });
                        await model.Flush(); await Settle(); Stable("drag");
                        double dragged = knob.Value;
                        model.Current.State = "Estado do preset\nMensagem adicional";
                        await Settle(); Stable("drag/status text");
                        adjust.Invoke(knob, new object[] { target });
                        Check(knob.Value == dragged && dragged > 1, "Unchanged drag coordinates must preserve volume after text changes");
                    }
                    finally { knob.ReleaseMouseCapture(); }
                    model.Current.State = "Aplicado ao aparelho · não gravado";
                    await Settle(); Capture($"{language}-{width}");
                    Console.WriteLine($"PASS stable volume {language}/{width}: wheel, keyboard, simulated drag, pending/confirmed and status text");
                }
                return;
            }
            foreach (int zoom in dialogsOnly ? Array.Empty<int>() : compactOnly ? new[] { 200 } : zoomOnly ? new[] { 50, 100, 150, 200 } : new[] { 100 })
            foreach (string language in new[] { "pt-BR", "en-US" })
            foreach (var (width, height) in new[] { (1400, 940), (640, 480), (920, 740), (1024, 768), (1100, 850), (1920, 1080), (640, 940), (1400, 940) })
            {
                ZoomService.Instance.Preview(zoom);
                LanguageService.Instance.Preview(language);
                window.Width = width; window.Height = height;
                await Settle();
                for (int tab = 0; tab < 3; tab++)
                {
                    tabs.SelectedIndex = tab; outer.ScrollToTop(); await Settle();
                    var page = (ScrollViewer)window.FindName(new[] { "EditorScroll", "IrScroll", "HelpScroll" }[tab]);
                    page.ScrollToTop(); await Settle();
                    bool compact = outer.VerticalScrollBarVisibility == ScrollBarVisibility.Auto;
                    var footer = (FrameworkElement)window.FindName("FooterActions");
                    var pageContent = (StackPanel)page.Content;
                    Check(footer.Parent == pageContent && pageContent.Children[^1] == footer, "Footer follows active page content");
                    var preceding = (FrameworkElement)pageContent.Children[pageContent.Children.Count - 2];
                    double gap = footer.TranslatePoint(new Point(), pageContent).Y - preceding.TranslatePoint(new Point(0, preceding.ActualHeight), pageContent).Y;
                    Check(gap <= 32, "Footer must not drift away from content in tall windows");
                    Check(double.IsNaN(workspace.Height), "Workspace must size to content, never a fixed height");
                    Check(workspace.ActualWidth <= 1440.5, "Wide content must remain bounded");
                    Check(outer.ExtentWidth <= outer.ViewportWidth + 1, "Workspace horizontal overflow");
                    Check(page.ExtentWidth <= page.ViewportWidth + 1, "Page horizontal overflow");
                    Check(!compact || page.ScrollableHeight < 1, "Compact layout has competing vertical scroll owners");
                    var header = (Grid)window.FindName("EditHeaderGrid");
                    foreach (string name in new[] { "PresetGroup", "OutputGroup", "SaveGroup" })
                    {
                        var group = (FrameworkElement)window.FindName(name);
                        var bounds = group.TransformToAncestor(header).TransformBounds(new Rect(group.RenderSize));
                        Check(bounds.Left >= -1 && bounds.Right <= header.ActualWidth + 1, $"{name} exceeds header at {width}");
                    }
                    if (tab == 0)
                    {
                        for (int i = 0; i < chain.Items.Count; i++)
                        {
                            var item = (ListBoxItem)chain.ItemContainerGenerator.ContainerFromIndex(i);
                            var bounds = item.TransformToAncestor(chain).TransformBounds(new Rect(item.RenderSize));
                            Check(bounds.Left >= -1 && bounds.Right <= chain.ActualWidth + 1 && bounds.Bottom <= chain.ActualHeight + 1, $"Chain item {i} clipped at {width}");
                        }
                        foreach (int stage in Enumerable.Range(0, 6))
                        {
                            model.SelectedStage = stage; await Settle();
                            foreach (var dial in Find<Knob>(page))
                            {
                                var bounds = dial.TransformToAncestor(page).TransformBounds(new Rect(dial.RenderSize));
                                Check(bounds.Left >= -1 && bounds.Right <= page.ActualWidth + 1, $"Dial clipped at {width}, stage {stage}");
                            }
                        }
                        model.SelectedStage = 0; page.ScrollToTop(); outer.ScrollToTop(); await Settle();
                    }
                    if (zoom == 200 && width == 640)
                    {
                        foreach (var expander in Find<Wpf.Ui.Controls.CardExpander>(page)) expander.IsExpanded = true;
                        foreach (var expander in Find<Expander>(page)) expander.IsExpanded = true;
                        await Settle();
                        foreach (var action in Find<System.Windows.Controls.Primitives.ButtonBase>(page).Where(b => b.IsVisible && b.ActualWidth > 0))
                        {
                            var bounds = action.TransformToAncestor(page).TransformBounds(new Rect(action.RenderSize));
                            Check(bounds.Left >= -1 && bounds.Right <= page.ActualWidth + 1, $"Action exceeds page: {action.GetType().Name}/{(action as ContentControl)?.Content} at zoom {zoom}, tab {tab}: {bounds}, page {page.ActualWidth}");
                        }
                    }
                    Capture($"{language}-{width}x{height}-zoom{zoom}-tab{tab}");
                    var owner = compact ? outer : page;
                    Check(owner.ActualHeight >= 60, "Fixed navigation must leave a useful content viewport");
                    if (tab == 0 && owner.ScrollableHeight > 0)
                    {
                        var source = (UIElement)window.FindName("SavePresetsButton");
                        source.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, -120) { RoutedEvent = Mouse.PreviewMouseWheelEvent });
                        await Task.Delay(260); await Settle();
                        Check(owner.VerticalOffset > 0, "Pedalboard wheel down must reach active scroll owner");
                        double offset = owner.VerticalOffset;
                        source.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, 120) { RoutedEvent = Mouse.PreviewMouseWheelEvent });
                        await Task.Delay(260); await Settle();
                        Check(owner.VerticalOffset < offset, "Pedalboard wheel up must reach active scroll owner");
                        var knobWheel = new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, -120) { RoutedEvent = Mouse.PreviewMouseWheelEvent };
                        ((UIElement)window.FindName("PresetVolumeKnob")).RaiseEvent(knobWheel);
                        Check(!knobWheel.Handled, "Page must leave knob wheel adjustment to its control");
                    }
                    var fixedHeader = (FrameworkElement)window.FindName("HeaderGrid");
                    var headerBefore = fixedHeader.TransformToAncestor(window).TransformBounds(new Rect(fixedHeader.RenderSize));
                    owner.ScrollToEnd(); await Settle();
                    var headerAfter = fixedHeader.TransformToAncestor(window).TransformBounds(new Rect(fixedHeader.RenderSize));
                    Check(headerBefore == headerAfter && headerAfter.Top >= 0 && headerAfter.Bottom <= window.ActualHeight, "Navigation must stay fixed and visible when content scrolls");
                    Check(Math.Abs(owner.VerticalOffset - owner.ScrollableHeight) < 1, "Cannot reach end of content");
                    if (compact) Capture($"{language}-{width}x{height}-tab{tab}-end");
                }
                Console.WriteLine($"PASS resize {language} {width}x{height} zoom {zoom}: all pages, six effects, header, chain, scroll reachability");
            }
            ZoomService.Instance.Preview(100);
            tabs.SelectedIndex = 0;
            foreach (int dialogZoom in zoomOnly ? new[] { 100, 200 } : new[] { 100 })
            foreach (var (width, height) in new[] { (640, 480), (920, 740), (1400, 940) })
            {
                ZoomService.Instance.Preview(dialogZoom);
                window.Width = width; window.Height = height; await Settle();
                foreach (var content in new FrameworkElement[] {
                    new Cube.Desktop.Views.GearGallery(model.Amps, model.SelectedAmp, true),
                    new Cube.Desktop.Views.MatchSettings(model.CreateMatchRows()),
                    new Cube.Desktop.Views.ConnectionDetails { DataContext = model } })
                {
                    var host = (Wpf.Ui.Controls.ContentDialogHost)window.FindName("DialogHost");
                    var dialog = new Wpf.Ui.Controls.ContentDialog(host) { Title = "Responsive layout", Content = content, DialogMaxWidth = 900,
                        PrimaryButtonText = "Salvar associações", CloseButtonText = "Cancelar" };
                    if (content is Cube.Desktop.Views.MatchSettings) { dialog.DialogMaxWidth = 680; dialog.DialogMaxHeight = 900; }
                    var shown = dialog.ShowLocalizedAsync(); await Task.Delay(200); await Settle();
                    var body = Find<ScrollViewer>(dialog).First(s => s.Name == "PART_ContentScroll");
                    Check(body.ViewportHeight >= 48 && body.ViewportWidth >= 180, $"Dialog content has no usable viewport at {width}, zoom {dialogZoom}: {body.ViewportWidth}x{body.ViewportHeight}");
                    if (content is Cube.Desktop.Views.MatchSettings && width == 1400 && dialogZoom == 100)
                        Check(body.ScrollableHeight < 1 && body.ScrollableWidth < 1, "Association rows must fit without scrolling on desktop");
                    Check(content.ActualWidth <= body.ViewportWidth + 1, $"{content.GetType().Name} exceeds dialog at {width}");
                    foreach (string name in new[] { "PrimaryButton", "SecondaryButton", "CloseButton" })
                    {
                        if (dialog.Template.FindName(name, dialog) is FrameworkElement { IsVisible: true } action)
                        {
                            var bounds = action.TransformToAncestor(host).TransformBounds(new Rect(action.RenderSize));
                            Check(bounds.Left >= 0 && bounds.Right <= host.ActualWidth + 1 && bounds.Bottom <= host.ActualHeight + 1, "Dialog action is clipped: " + name);
                        }
                    }
                    var actions = new[] { "PrimaryButton", "SecondaryButton", "CloseButton" }.Select(name => dialog.Template.FindName(name, dialog)).OfType<Wpf.Ui.Controls.Button>().Where(b => b.IsVisible).ToArray();
                    Check(actions.Max(b => b.ActualHeight) - actions.Min(b => b.ActualHeight) < 1, "Dialog buttons must have equal heights, including wrapped labels");
                    Capture($"dialog-{content.GetType().Name}-{width}-zoom{dialogZoom}");
                    if (dialogZoom == 200 && width == 640) { body.ScrollToEnd(); await Settle(); Capture($"dialog-{content.GetType().Name}-{width}-zoom{dialogZoom}-end"); }
                    if (dialogZoom == 200 && width == 640)
                    {
                        ZoomService.Instance.Preview(100); window.Width = 1280; window.Height = 720;
                        await Task.Delay(100); await Settle();
                        Check(body.ViewportHeight >= 48 && content.ActualWidth <= body.ViewportWidth + 1, $"Open {content.GetType().Name} dialog failed to return to normal layout: viewport {body.ViewportWidth}x{body.ViewportHeight}, content {content.ActualWidth}");
                        ZoomService.Instance.Preview(200); window.Width = width; window.Height = height;
                        await Task.Delay(100); await Settle();
                        Check(body.ViewportHeight >= 48 && content.ActualWidth <= body.ViewportWidth + 1, "Open dialog failed to return to compact layout");
                    }
                    dialog.Hide(); await shown;
                }
                Console.WriteLine($"PASS dialogs fit {width}x{height} zoom {dialogZoom}");
            }
            ZoomService.Instance.Preview(100);
            Check(fake.Writes == 0, "Resizing must not send MIDI edits");
            Console.WriteLine("PASS no MIDI edits during resize or navigation");
            foreach (var size in new[] { (1280, 720), (1920, 1080) })
            {
                window.Width = size.Item1; window.Height = size.Item2; tabs.SelectedIndex = 0;
                await Settle();
                model.Current.Volume.Value = model.Current.Volume.Value == 0 ? 1 : 0;
                await Settle();
                var save = (UIElement)window.FindName("SavePresetsButton");
                save.Focus(); outer.ScrollToTop(); ((ScrollViewer)window.FindName("EditorScroll")).ScrollToTop(); await Settle();
                var owner = outer.VerticalScrollBarVisibility == ScrollBarVisibility.Auto ? outer : (ScrollViewer)window.FindName("EditorScroll");
                double offset = owner.VerticalOffset;
                model.Save.Execute(null);
                while (model.Busy) { await Task.Delay(20); await Settle(); Check(Math.Abs(owner.VerticalOffset - offset) < 1, "Saving must not scroll the page while busy"); }
                await Settle();
                Check(model.ErrorMessage.Length == 0 && Math.Abs(owner.VerticalOffset - offset) < 1, "Saving must retain scroll position after completion");
                Console.WriteLine("PASS save retains viewport " + size);
            }
        }
        finally
        {
            ZoomService.Instance.Preview(originalZoom);
            window.Close(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            if (Directory.Exists(temporary)) Directory.Delete(temporary, true);
        }
    }
}
