using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cube.Desktop;
using Cube.Desktop.Controls;
using Cube.Desktop.ViewModels;

internal static class BackingUiReviewTests
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); }
    private static T Named<T>(MainWindow window, string name) => (T)window.FindName(name);
    private static IEnumerable<T> Find<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        { var child = VisualTreeHelper.GetChild(root, i); if (child is T result) yield return result; foreach (var nested in Find<T>(child)) yield return nested; }
    }
    private static object? Invoke(MainWindow window, string method, params object?[] args) => typeof(MainWindow).GetMethod(method, Private)!.Invoke(window, args);
    private static void Key(MainWindow window, UIElement target, Key key) => target.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(window), 0, key) { RoutedEvent = Keyboard.PreviewKeyDownEvent });
    private static void Click(ButtonBase button)
    {
        if (button.Command is { } command && command.CanExecute(button.CommandParameter)) command.Execute(button.CommandParameter);
        else button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
    }
    private static void Capture(FrameworkElement visual, string path)
    {
        visual.UpdateLayout();
        var drawing = new DrawingVisual();
        using (var dc = drawing.RenderOpen()) dc.DrawRectangle(new VisualBrush(visual), null, new Rect(visual.RenderSize));
        var bitmap = new RenderTargetBitmap(Math.Max(1, (int)Math.Ceiling(visual.ActualWidth)), Math.Max(1, (int)Math.Ceiling(visual.ActualHeight)), 96, 96, PixelFormats.Pbgra32);
        if (visual is Window) bitmap.Render(visual); else bitmap.Render(drawing); var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path); encoder.Save(stream);
    }
    public static void CheckTransport(MainWindow window)
    {
        var panel = Named<BackingTransportPanel>(window, "BackingTransportButtons");
        var buttons = Find<ButtonBase>(panel).ToArray();
        Check(buttons.Length == 7, "all transport actions remain reachable");
        var bounds = buttons.Select(b => b.TransformToAncestor(panel).TransformBounds(new Rect(b.RenderSize))).ToArray();
        var row = panel.ActualWidth >= 344 ? Enumerable.Range(0, 7).ToArray() : new[] { 1, 2, 3, 4 };
        var centers = row.Select(i => bounds[i].Top + bounds[i].Height / 2).ToArray();
        Check(centers.Max() - centers.Min() <= 1.1, "transport centers align independently of icon size");
        for (int i = 0; i < bounds.Length; i++)
        {
            Check(bounds[i].Left >= -1 && bounds[i].Right <= panel.ActualWidth + 1 && bounds[i].Bottom <= panel.ActualHeight + 1, "transport button stays within panel");
            for (int j = i + 1; j < bounds.Length; j++) Check(!bounds[i].IntersectsWith(bounds[j]), "transport buttons do not overlap");
        }
    }
    public static async Task Run(MainWindow window, string source, string reports)
    {
        var model = window.BackingTracks;
        var client = Named<FrameworkElement>(window, "ClientArea");
        var scroll = Named<ScrollViewer>(window, "BackingTracksScroll");
        var audio = Named<FrameworkElement>(window, "BackingAudioPanel");
        var wave = Named<BackingWaveform>(window, "BackingMainWave");
        Cube.Desktop.Appearance.ThemeService.Instance.Preview(Cube.Desktop.Appearance.ThemeMode.Dark);
        Cube.Desktop.Localization.LanguageService.Instance.Preview("pt-BR");
        client.LayoutTransform = Transform.Identity;
        window.Width = 1280; window.Height = 720;
        Check(audio.IsVisible, "audio selectors remain open");
        await Task.Delay(250); window.UpdateLayout();
        var current = model.SelectedTrack!;
        model.IsLooping = false; model.Position = 24;
        string otherPath = Path.Combine(Path.GetDirectoryName(source)!, "Uma faixa com nome bastante longo para conferir a leitura e o menu da playlist.wav");
        File.Copy(source, otherPath);
        model.AddPaths([otherPath]); var other = model.Tracks.Last();
        for (int i = 0; i < 5; i++) model.AddSectionAt(current, 45 + i * 10, "Seção de prática " + (i + 1));
        await Task.Delay(200);
        var speedText = Find<TextBlock>(Named<FrameworkElement>(window, "BackingTransportRack")).Any(t => t.Text.Contains('%') && t.Text.Contains('▾'));
        Check(speedText, "speed presents both percentage and menu affordance");

        ContextMenu OpenMenu(FrameworkElement owner, string kind, BackingTrack? track = null)
        {
            Invoke(window, "OpenBackingMenu", owner, kind, track, null, null, false);
            return (ContextMenu)typeof(MainWindow).GetField("backingOpenMenu", Private)!.GetValue(window)!;
        }
        var owner = Named<FrameworkElement>(window, "BackingTrackHeader");
        int cache = wave.WaveCacheBuildCount;
        var menu = OpenMenu(owner, "track", other); await Task.Delay(100);
        Check(model.SelectedTrack == current && model.Position == 24, "opening another track menu preserves selection and position");
        Capture(menu, Path.Combine(reports, "menu-track.png")); menu.IsOpen = false;
        Check(wave.WaveCacheBuildCount == cache, "context menu leaves waveform cache intact");
        Key(window, owner, System.Windows.Input.Key.Apps); await Task.Delay(60);
        menu = (ContextMenu)typeof(MainWindow).GetField("backingOpenMenu", Private)!.GetValue(window)!;
        Check(menu != null && menu.IsOpen, "keyboard menu key opens track options"); menu!.IsOpen = false;
        Key(window, wave, System.Windows.Input.Key.Apps); await Task.Delay(60);
        menu = (ContextMenu)typeof(MainWindow).GetField("backingOpenMenu", Private)!.GetValue(window)!;
        Check(menu.Items.OfType<MenuItem>().Any(i => i.Header.ToString() == "Adicionar seção"), "waveform keyboard menu exposes section creation");
        Capture(menu, Path.Combine(reports, "menu-wave.png")); menu.IsOpen = false;
        await Task.Delay(150); window.UpdateLayout();
        var list = Named<ListBox>(window, "BackingTrackList");
        var row = (ListBoxItem)list.ItemContainerGenerator.ContainerFromItem(other);
        row.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right) { RoutedEvent = Mouse.PreviewMouseDownEvent });
        menu = (ContextMenu)typeof(MainWindow).GetField("backingOpenMenu", Private)!.GetValue(window)!;
        Check(menu.IsOpen && model.SelectedTrack == current && model.Position == 24, "right-click handler does not select the other track"); menu.IsOpen = false;
        Check(!Find<Wpf.Ui.Controls.Button>(row).Any(), "playlist has no redundant options button");
        model.UpdateTrackMetadata(other, 140, "Am", "Drop D", 3);
        Check(model.SelectedTrack == current && model.Position == 24 && other.Bpm == 140 && other.Preset == 3, "editing another track preserves playback context");
        model.MoveTrack(other, 0); Check(model.SelectedTrack == current && model.Position == 24, "reordering another track preserves playback context");
        model.RemoveTrackItem(other); Check(model.SelectedTrack == current && model.Position == 24, "removing another track preserves playback context");
        model.AddPaths([otherPath]);

        foreach (var (width, height, scale) in new[] { (1280, 720, 1d), (640, 720, 1d), (640, 720, 2d) })
        {
            window.Width = width; window.Height = height; client.LayoutTransform = new ScaleTransform(scale, scale);
            await Task.Delay(250); window.UpdateLayout();
            Check(Named<FrameworkElement>(window, "BackingPlaylistPanel").IsVisible, "playlist stays open");
            Check(audio.IsVisible, "audio selectors remain open");
            foreach (var control in Find<FrameworkElement>(audio).Where(c => c is ButtonBase or Slider or ComboBox))
            {
                var box = control.TransformToAncestor(audio).TransformBounds(new Rect(control.RenderSize));
                Check(box.Left >= -1 && box.Right <= audio.ActualWidth + 1, "audio controls fit the panel");
            }
            Capture(audio, Path.Combine(reports, $"audio-{width}-{scale}.png"));
            Capture(window, Path.Combine(reports, $"state-{width}-{scale}.png"));
            Invoke(window, "EditBackingTrack", current); await Task.Delay(250); window.UpdateLayout();
            var dialog = Find<Wpf.Ui.Controls.ContentDialog>(window).First(d => d.IsVisible);
            Check(dialog.ActualWidth <= Named<FrameworkElement>(window, "DialogHost").ActualWidth + 1, "metadata dialog fits available width");
            Capture(window, Path.Combine(reports, $"metadata-{width}-{scale}.png"));
            var bpm = Find<TextBox>(dialog).First(); bpm.Text = "abc";
            Check(!dialog.IsPrimaryButtonEnabled, "invalid BPM prevents saving"); bpm.Text = "126";
            Check(dialog.IsPrimaryButtonEnabled, "valid BPM enables saving");
            if (width == 1280)
            {
                double beforePosition = model.Position, beforeZoom = model.Zoom;
                Click((ButtonBase)dialog.Template.FindName("PrimaryButton", dialog)); await Task.Delay(250);
                Check(current.Bpm == 126 && model.Position == beforePosition && model.Zoom == beforeZoom, "metadata save preserves viewport and position");
            }
            else { dialog.Hide(); await Task.Delay(100); }
        }
        client.LayoutTransform = Transform.Identity; window.Width = 1280; window.Height = 1000;
        Check(audio.IsVisible, "audio selectors remain open");
        await Task.Delay(250);
        var dock = Named<ContentControl>(window, "BackingTransportHost");
        Check(dock.IsVisible, "transport is present in tall windows");
        window.Height = 480; await Task.Delay(250);
        Check(dock.IsVisible && dock.Parent == window.FindName("BackingRacks"), "short windows keep transport in the scrolling player");
        window.Height = 1000; await Task.Delay(250);
        Check(dock.IsVisible, "height-only resize preserves transport");
        window.Height = 720; await Task.Delay(250);
        scroll.ScrollToTop(); await Task.Delay(80);
        Capture(window, Path.Combine(reports, "final-desktop.png"));
        model.ResetRange.Execute(null); model.RangeStart = 10;
        Check(!model.CanLoop, "one loop endpoint keeps looping unavailable"); model.RangeEnd = 20;
        Check(model.CanLoop, "complete range enables looping");
        model.Speed = 100; model.Pitch = 0; model.RaisePitch.Execute(null); model.LowerPitch.Execute(null);
        Check(model.Pitch == 0, "pitch controls preserve semitone increments");
        Invoke(window, "BackingMuteClick", window, new RoutedEventArgs()); Check(model.IsMuted, "mute control updates state");
        Invoke(window, "BackingMuteClick", window, new RoutedEventArgs());
        var volume = Find<BackingFader>(Named<FrameworkElement>(window, "BackingTransportRack")).First();
        void Wheel(UIElement target, int delta)
        {
            var args = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, delta) { RoutedEvent = Mouse.PreviewMouseWheelEvent };
            target.RaiseEvent(args);
            if (!args.Handled) { args.RoutedEvent = Mouse.MouseWheelEvent; target.RaiseEvent(args); }
            Check(args.Handled, "musical wheel event is consumed by its control");
        }
        double scrollBefore = scroll.VerticalOffset;
        model.IsLooping = false; model.Position = 24;
        Wheel(wave, 60); Check(model.Position == 24, "high-resolution wheel accumulates partial seek detents");
        Wheel(wave, 60); Check(model.Position == 29, "wave wheel advances five seconds");
        Wheel(wave, -120); Check(model.Position == 24, "wave wheel seeks backward");
        model.IsLooping = true; model.Position = 19; Wheel(wave, 120);
        Check(model.Position <= 20, "wheel respects active loop boundaries"); model.IsLooping = false;
        foreach (var fader in Find<BackingFader>(Named<FrameworkElement>(window, "BackingTransportRack")))
        {
            fader.Value = 30; Wheel(fader, 60); Wheel(fader, 60); Check(fader.Value == 31, "fader wheel raises volume by one percent");
            Wheel(fader, -120); Check(fader.Value == 30, "fader wheel lowers volume");
            fader.Value = 100; Wheel(fader, 120); Check(fader.Value == 100, "fader wheel clamps maximum");
            fader.Value = 0; Wheel(fader, -120); Check(fader.Value == 0, "fader wheel clamps minimum");
        }
        Check(scroll.VerticalOffset == scrollBefore, "wheel on musical controls does not scroll the page");
        model.Volume = 30;
        double speed = model.Speed, position = model.Position;
        Key(window, volume, System.Windows.Input.Key.Right);
        Check(model.Speed == speed && model.Position == position, "fader navigation is not intercepted by playback shortcuts");
        double beforeVolume = model.Volume;
        volume.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(window), 0, System.Windows.Input.Key.Right) { RoutedEvent = Keyboard.KeyDownEvent });
        Check(model.Volume > beforeVolume && model.Position == position, "fader arrow key changes volume without seeking");
        Invoke(window, "AddBackingSection", current, 17d); await Task.Delay(180);
        var sectionDialog = Find<Wpf.Ui.Controls.ContentDialog>(window).First(d => d.IsVisible);
        Find<TextBox>(sectionDialog).First().Text = "Trecho novo";
        Capture(window, Path.Combine(reports, "section-dialog.png"));
        Click((ButtonBase)sectionDialog.Template.FindName("PrimaryButton", sectionDialog)); await Task.Delay(350);
        Check(current.Sections.Any(s => s.Name == "Trecho novo" && s.Position == 17), "section dialog commits the requested position without seeking");

        // Presentation fixture for an in-flight export; actual file export is exercised separately below.
        using var cancel = new CancellationTokenSource();
        typeof(BackingTracksViewModel).GetField("exportCancellation", Private)!.SetValue(model, cancel);
        typeof(BackingTracksViewModel).GetProperty("IsExporting")!.SetValue(model, true);
        CommandManager.InvalidateRequerySuggested();
        await Task.Delay(100); scroll.ScrollToEnd(); await Task.Delay(100);
        var cancelButton = Find<Wpf.Ui.Controls.Button>(Named<FrameworkElement>(window, "BackingPage")).Single(b => b.Command == model.CancelExport);
        var cancelBounds = cancelButton.TransformToAncestor(window).TransformBounds(new Rect(cancelButton.RenderSize));
        Check(cancelButton.IsVisible && cancelButton.IsEnabled && cancelBounds.Bottom <= window.ActualHeight && model.CancelExport.CanExecute(null), "export cancellation remains visible and enabled while scrolled");
        Capture(window, Path.Combine(reports, "export-state.png"));
        model.CancelExport.Execute(null); Check(cancel.IsCancellationRequested, "cancel export reaches cancellation source");
        typeof(BackingTracksViewModel).GetField("exportCancellation", Private)!.SetValue(model, null);
        typeof(BackingTracksViewModel).GetProperty("IsExporting")!.SetValue(model, false);
        string exported = Path.Combine(Path.GetDirectoryName(source)!, "review-export.wav");
        await Cube.Desktop.Services.BackingAudioProcessing.Export(source, exported, 1, 2, 1, 0, false, CancellationToken.None);
        Check(new FileInfo(exported).Length > 44, "original WAV export completes");
        foreach (var track in model.Tracks.ToArray()) model.RemoveTrackItem(track);
        scroll.ScrollToTop(); await Task.Delay(180);
        Capture(window, Path.Combine(reports, "empty.png"));
        Check(!model.PlayPause.CanExecute(null) && !model.CanLoop, "empty state disables unavailable playback actions");
    }
}
