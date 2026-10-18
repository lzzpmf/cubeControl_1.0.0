using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.IO;
using System.Windows.Media.Imaging;
using Cube.Desktop.Controls;
using Cube.Desktop.ViewModels;
using Cube.Protocol;

internal static class PresentationTests
{
    public static async Task RenderFixture()
    {
        string temporary = Path.Combine(Path.GetTempPath(), "cube-render-" + Guid.NewGuid().ToString("N"));
        using var fake = new FakeConnection();
        fake.Bank[0] = 2; fake.Bank[1] = 4; fake.Bank[2] = 8; fake.Bank[5] = 33;
        fake.Bank[8] = 8; fake.Bank[6] = 16; fake.Bank[4] = 54; fake.Bank[7] = 51;
        using var session = new CubeSession(fake); await session.Identify();
        using var model = new EditorViewModel(temporary);
        await model.Execute(() => model.AttachSession(session));
        var window = new Cube.Desktop.MainWindow(model, false);
        window.Show();
        await Task.Delay(150);
        while (model.Busy) await Task.Delay(30);
        var tabs = (TabControl)window.FindName("Tabs");
        if (tabs.SelectedIndex != 0 || tabs.SelectedContent == null) throw new Exception("Pedalboard must be visible without clicking navigation after connection.");
        Console.WriteLine("PASS pedalboard content selected automatically after connection");
        string folder = Path.GetFullPath(TestPaths.Report("C:/CubeControl-Build/reports/ux-3.3.1/fixture")); Directory.CreateDirectory(folder);
        var busyGate = new TaskCompletionSource();
        var busyOperation = model.Execute(() => busyGate.Task);
        try
        {
            window.UpdateLayout();
            var overlay = (Border)window.FindName("BusyOverlay");
            var message = (TextBlock)window.FindName("BusyMessage");
            if (!overlay.IsVisible) throw new Exception("Busy notice must appear during an operation.");
            var center = overlay.TranslatePoint(new Point(overlay.ActualWidth / 2, overlay.ActualHeight / 2), window);
            if (Math.Abs(center.X - window.ActualWidth / 2) > 2 || Math.Abs(center.Y - (window.ActualHeight + 36) / 2) > 2)
                throw new Exception("Busy notice must be centered in the client content.");
            var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(window);
            var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
            using (var stream = File.Create(Path.Combine(folder, "busy-centered.png"))) png.Save(stream);
            string firstMessage = message.Text;
            await Task.Delay(4200);
            if (message.Text == firstMessage) throw new Exception("Busy messages must rotate during long operations.");
        }
        finally { busyGate.SetResult(); await busyOperation; }
        if (((Border)window.FindName("BusyOverlay")).Visibility != Visibility.Collapsed) throw new Exception("Busy notice must disappear after completion.");
        Console.WriteLine("PASS busy notice centers, rotates messages and disappears after completion");
        IEnumerable<T> Find<T>(DependencyObject root) where T : DependencyObject
        {
            for (int childIndex = 0; childIndex < VisualTreeHelper.GetChildrenCount(root); childIndex++)
            {
                var child = VisualTreeHelper.GetChild(root, childIndex);
                if (child is T item) yield return item;
                foreach (var nested in Find<T>(child)) yield return nested;
            }
        }
        foreach (int stage in new[] { 0, 5 })
        {
            model.SelectedStage = stage; window.UpdateLayout();
            var art = Find<GearArt>(window).First();
            ((Wpf.Ui.Controls.Button)window.FindName("EquipmentImageButton")).RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            await Task.Delay(250); window.UpdateLayout();
            var picker = Find<Cube.Desktop.Views.GearGallery>(window).Single();
            var filter = (ListBox)picker.FindName("Filter");
            if ((filter.Visibility == Visibility.Visible) != (stage == 0)) throw new Exception("Amp filter must be preserved and cabinet filter hidden.");
            var dialog = Find<Wpf.Ui.Controls.ContentDialog>(window).Single();
            var close = Find<Wpf.Ui.Controls.Button>(dialog).Single(b => b.IsVisible && b.ToolTip?.ToString() == Cube.Desktop.Localization.LanguageService.T("Fechar") && b.Icon is Wpf.Ui.Controls.SymbolIcon icon && icon.Symbol == Wpf.Ui.Controls.SymbolRegular.Dismiss24);
            close.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            await Task.Delay(250);
            if (Find<Cube.Desktop.Views.GearGallery>(window).Any()) throw new Exception("X must close the equipment picker.");
            ((Wpf.Ui.Controls.Button)window.FindName("EquipmentImageButton")).RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            await Task.Delay(250); window.UpdateLayout();
            picker = Find<Cube.Desktop.Views.GearGallery>(window).Single();
            var items = (ListBox)picker.FindName("Items");
            var selected = (ListBoxItem)items.ItemContainerGenerator.ContainerFromItem(items.SelectedItem);
            items.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Control.MouseDoubleClickEvent, Source = selected });
            await Task.Delay(250);
            if (Find<Cube.Desktop.Views.GearGallery>(window).Any()) throw new Exception("Double-click must apply and close the picker.");
        }
        Console.WriteLine("PASS equipment image links, amp filter, X and double-click for both pickers");
        await model.Execute(() => Task.CompletedTask);
        int writesAfterPicker = fake.Writes;
        var chain = (ListBox)window.FindName("EffectChain");
        model.SelectedStage = 2;
        void ChainWheel(int delta) => chain.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, delta) { RoutedEvent = UIElement.PreviewMouseWheelEvent });
        ChainWheel(120);
        if (model.SelectedStage != 1) throw new Exception("Wheel up must select previous card.");
        ChainWheel(-60);
        if (model.SelectedStage != 1) throw new Exception("Partial wheel must accumulate.");
        ChainWheel(-60);
        if (model.SelectedStage != 2) throw new Exception("Wheel down must select next card.");
        ChainWheel(-1200);
        if (model.SelectedStage != 5) throw new Exception("Chain must stop at last card.");
        ChainWheel(1200);
        if (model.SelectedStage != 0) throw new Exception("Chain must stop at first card.");
        await model.Flush();
        if (fake.Writes != writesAfterPicker) throw new Exception("Chain wheel sent MIDI edits.");
        Console.WriteLine("PASS chain wheel selects previous/up and next/down, accumulates deltas, clamps and sends no MIDI.");

        for (int i = 0; i < model.Stages.Length + 2; i++)
        {
            tabs.SelectedIndex = i < model.Stages.Length ? 0 : i - model.Stages.Length + 1;
            if (i < model.Stages.Length) model.SelectedStage = i;
            window.UpdateLayout(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(window);
            var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(Path.Combine(folder, $"connected-{i}.png")); png.Save(stream);
        }
        if (fake.Writes != writesAfterPicker) throw new Exception("Rendering sent MIDI edits.");
        model.SelectedStage = 0; tabs.SelectedIndex = 0; window.UpdateLayout();
        var mainArt = Find<GearArt>(window).First();
        int oldAmp = model.SelectedAmp.Value;
        mainArt.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, -60) { RoutedEvent = UIElement.PreviewMouseWheelEvent });
        if (model.SelectedAmp.Value != oldAmp) throw new Exception("Partial wheel must not select an amp yet.");
        mainArt.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, -60) { RoutedEvent = UIElement.PreviewMouseWheelEvent });
        if (model.SelectedAmp.Value != oldAmp + 1) throw new Exception("Main amp image wheel must change equipment.");
        await model.Execute(() => Task.CompletedTask);
        model.SelectedIndex = 1;
        var saveButton = (Wpf.Ui.Controls.Button)window.FindName("SavePresetsButton");
        foreach (int tabIndex in new[] { 0, 1, 2 })
        {
            tabs.SelectedIndex = tabIndex; window.UpdateLayout();
            if (saveButton.Appearance != Wpf.Ui.Controls.ControlAppearance.Primary || !model.Dirty)
                throw new Exception("Dirty bank must remain highlighted across presets and tabs.");
        }
        model.Save.Execute(null);
        while (model.Busy) await Task.Delay(30);
        window.UpdateLayout();
        if (model.Dirty || model.Save.CanExecute(null) || saveButton.Appearance != Wpf.Ui.Controls.ControlAppearance.Primary) throw new Exception("Confirmed save must disable saving while preserving the primary action style.");
        tabs.SelectedIndex = 0; model.SelectedStage = 5; window.UpdateLayout();
        mainArt = Find<GearArt>(window).First();
        int oldCabinet = model.SelectedCabinet.Value;
        mainArt.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, -120) { RoutedEvent = UIElement.PreviewMouseWheelEvent });
        if (model.SelectedCabinet.Value != oldCabinet + 1) throw new Exception("Main cabinet image wheel must change equipment.");
        await model.Execute(() => Task.CompletedTask);
        Console.WriteLine("PASS main image wheel and bank-wide save emphasis independent of tab and preset");
        foreach (var mode in new[] { Cube.Desktop.Appearance.ThemeMode.Light, Cube.Desktop.Appearance.ThemeMode.Dark })
        foreach (double width in new[] { 640d, 920d, 1400d })
        foreach (double scale in new[] { 1d, 1.5d, 2d })
        {
            Cube.Desktop.Appearance.ThemeService.Instance.Preview(mode);
            window.Width = width; window.Height = 940;
            ((FrameworkElement)window.FindName("Workspace")).LayoutTransform = new ScaleTransform(scale, scale);
            window.UpdateLayout(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            var header = (Grid)window.FindName("HeaderGrid");
            var connection = (Grid)window.FindName("ConnectionGroup");
            if (Grid.GetRow(connection) != 0 || Grid.GetRow((FrameworkElement)window.FindName("NavigationHeader")) != (header.ActualWidth < 1000 ? 1 : 0)) throw new Exception("Header breakpoint mismatch.");
            var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32); bitmap.Render(window);
            var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(Path.Combine(folder, $"header-{mode}-{width}-{scale}.png")); png.Save(stream);
        }
        Console.WriteLine("PASS header layout matrix at 640/920/1400 and 100/150/200 percent");
        model.Save.Execute(null); while (model.Busy) await Task.Delay(30);
        window.Close(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        Console.WriteLine("PASS eight connected screens rendered using fake MIDI, without edits");
        string resolved = Path.GetFullPath(temporary);
        if (resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) && Path.GetFileName(resolved).StartsWith("cube-render-")) Directory.Delete(resolved, true);
    }
    public static async Task Run()
    {
        int checks = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); Console.WriteLine("PASS " + name); checks++; }
        double value = 3;
        int writes = 0;
        var dial = new DialViewModel("Test", "", 1, 7, () => value, v => { value = v; writes++; });
        var number = new ParameterNumberBox { Minimum = 1, Maximum = 7, DataContext = dial };
        number.SetBinding(Wpf.Ui.Controls.NumberBox.ValueProperty, new Binding("Value") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        var button = new Button { Content = "Next" };
        var panel = new StackPanel(); panel.Children.Add(number); panel.Children.Add(button);
        var window = new Window { Content = panel, Width = 320, Height = 200, ShowInTaskbar = false };
        window.Show();
        try
        {
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            number.Focus(); number.Text = "6";
            Check(value == 3 && writes == 0, "numeric draft never writes before commit");
            void Key(Key key) => number.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(window), 0, key) { RoutedEvent = Keyboard.PreviewKeyUpEvent });
            Key(System.Windows.Input.Key.Enter);
            Check(value == 6 && writes == 1, "Enter commits one valid whole-number edit");
            foreach (string invalid in new[] { "", "abc", "99", "0", "2.5", "NaN" })
            {
                number.Text = invalid; Key(System.Windows.Input.Key.Enter);
                Check(value == 6 && writes == 1 && number.InputError.Length > 0, "invalid hardware input is rejected: " + invalid);
            }
            number.Text = "2"; Key(System.Windows.Input.Key.Escape);
            Check(value == 6 && number.Text == "6" && writes == 1, "Escape cancels draft without MIDI change");
            number.Text = "4"; button.Focus(); await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            Check(value == 4 && writes == 2, $"focus loss commits valid draft once (value={value}, writes={writes}, text={number.Text}, focused={number.IsFocused})");
            Check(number.Template != null && number.ActualWidth > 0, "number input resolves native WPF UI template");
            var chain = new StagePanel();
            for (int i = 0; i < 6; i++) chain.Children.Add(new Border { Height = 60 });
            chain.Measure(new Size(700, double.PositiveInfinity)); chain.Arrange(new Rect(0, 0, 700, chain.DesiredSize.Height));
            Check(chain.DesiredSize.Height == 120 && VisualTreeHelper.GetOffset(chain.Children[3]).Y == 60, "compact signal chain wraps in signal order");
            var adaptive = new AdaptivePanel { FirstShare = .5 };
            adaptive.Children.Add(new Border { Height = 100 }); adaptive.Children.Add(new Border { Height = 120 });
            adaptive.Measure(new Size(600, double.PositiveInfinity));
            Check(adaptive.DesiredSize.Height == 220, "IR panels stack instead of clipping in compact layout");
            adaptive.Measure(new Size(1000, double.PositiveInfinity));
            Check(adaptive.DesiredSize.Height == 120, "IR panels share a row when space permits");
            adaptive.ControlsFirstWhenStacked = true;
            adaptive.Measure(new Size(600, double.PositiveInfinity)); adaptive.Arrange(new Rect(0, 0, 600, adaptive.DesiredSize.Height));
            Check(VisualTreeHelper.GetOffset(adaptive.Children[1]).Y == 0 && KeyboardNavigation.GetTabIndex(adaptive.Children[1]) == 0, "narrow editor prioritizes sound controls in layout and tab order");
            var ir = new IrViewModel { PreviewActive = true };
            Check(ir.PreviewLabel.Contains("não confirmada"), "interrupted IR preview is not presented as confirmed playback");
            ir.PreviewConfirmed = true;
            Check(ir.PreviewLabel.Contains("confirmada em RAM"), "confirmed IR preview stays distinct from slot persistence");
        }
        finally { window.Close(); }
        Console.WriteLine($"{checks} presentation checks passed.");
    }
}
