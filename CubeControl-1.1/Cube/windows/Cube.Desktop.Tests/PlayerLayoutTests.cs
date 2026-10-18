using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cube.Desktop;
using Cube.Desktop.Appearance;
using Cube.Desktop.Controls;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;
using ThemeMode = Cube.Desktop.Appearance.ThemeMode;

internal static class PlayerLayoutTests
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static IEnumerable<T> Find<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match) yield return match;
            foreach (var nested in Find<T>(child)) yield return nested;
        }
    }

    private static Rect Bounds(FrameworkElement child, Visual parent) =>
        child.TransformToAncestor(parent).TransformBounds(new Rect(child.RenderSize));

    private static void Contained(FrameworkElement child, FrameworkElement parent, string scenario)
    {
        var bounds = Bounds(child, parent);
        Check(bounds.Left >= -.5 && bounds.Top >= -.5 && bounds.Right <= parent.ActualWidth + .5 && bounds.Bottom <= parent.ActualHeight + .5,
            $"{scenario}: {child.GetType().Name}/{child.Name} outside parent: {bounds} in {parent.RenderSize}");
    }

    private static void Disjoint(IEnumerable<FrameworkElement> children, FrameworkElement parent, string scenario)
    {
        var bounds = children.Select(c => Bounds(c, parent)).ToArray();
        for (int i = 0; i < bounds.Length; i++)
        for (int j = i + 1; j < bounds.Length; j++)
        {
            var overlap = Rect.Intersect(bounds[i], bounds[j]);
            Check(overlap.IsEmpty || overlap.Width < .5 || overlap.Height < .5, $"{scenario}: controls overlap: {bounds[i]} and {bounds[j]}");
        }
    }

    public static async Task Run()
    {
        // Exercise both sides of the transport breakpoint, including its second row.
        foreach (double width in new[] { 200d, 391d, 392d, 500d })
        {
            var transport = new BackingTransportPanel();
            for (int i = 0; i < 8; i++) transport.Children.Add(new Border { Width = i == 2 ? 56 : 40, Height = 72 });
            transport.Measure(new Size(width, double.PositiveInfinity));
            transport.Arrange(new Rect(0, 0, width, transport.DesiredSize.Height));
            foreach (FrameworkElement child in transport.Children) Contained(child, transport, $"transport/{width}");
            Disjoint(transport.Children.Cast<FrameworkElement>(), transport, $"transport/{width}");
        }

        string folder = Path.Combine(Path.GetTempPath(), "cube-player-layout-" + Guid.NewGuid().ToString("N"));
        string reports = TestPaths.Report("C:/CubeControl-Build/reports/player-layout");
        Directory.CreateDirectory(reports);
        int originalZoom = ZoomService.Instance.Percent;
        string originalLanguage = LanguageService.Instance.Language;
        var originalTheme = ThemeService.Instance.Mode;
        using var model = new EditorViewModel(folder);
        var window = new MainWindow(model, initializeDevices: false) { ShowInTaskbar = false, Height = 900 };
        window.Show();
        try
        {
            ((TabControl)window.FindName("Tabs")).SelectedIndex = 2;
            var mixerModel = window.BackingTracks;
            mixerModel.FollowTrack = false;
            void WheelTempo(string name, int delta)
            {
                var wheel = new System.Windows.Input.MouseWheelEventArgs(System.Windows.Input.Mouse.PrimaryDevice, 0, delta)
                { RoutedEvent = System.Windows.Input.Mouse.PreviewMouseWheelEvent };
                ((FrameworkElement)window.FindName(name)).RaiseEvent(wheel);
                Check(wheel.Handled, "tempo wheel must not scroll the page");
            }
            var tempoTrack = new BackingTrack(Path.Combine(folder, "wheel-test.wav"));
            mixerModel.SelectedTrack = tempoTrack;
            WheelTempo("BackingSpeedOptions", 120);
            WheelTempo("BackingPitchOptions", -120);
            Check(mixerModel.Speed == 101 && mixerModel.Pitch == -1, "wheel changes speed by 1 percent and pitch by 1 semitone independently");
            WheelTempo("BackingSpeedOptions", -120);
            WheelTempo("BackingPitchOptions", 120);
            Check(mixerModel.Speed == 100 && mixerModel.Pitch == 0, "tempo wheel reverses direction");
            WheelTempo("BackingSpeedOptions", 60);
            WheelTempo("BackingPitchOptions", -60);
            Check(mixerModel.Speed == 100 && mixerModel.Pitch == 0, "partial tempo notches wait");
            WheelTempo("BackingSpeedOptions", 60);
            WheelTempo("BackingPitchOptions", -60);
            Check(mixerModel.Speed == 101 && mixerModel.Pitch == -1, "tempo wheel remainders are independent");
            mixerModel.Speed = 150; mixerModel.Pitch = 6;
            WheelTempo("BackingSpeedOptions", 120); WheelTempo("BackingPitchOptions", 120);
            Check(mixerModel.Speed == 150 && mixerModel.Pitch == 6, "tempo upper limits");
            mixerModel.Speed = 50; mixerModel.Pitch = -6;
            WheelTempo("BackingSpeedOptions", -120); WheelTempo("BackingPitchOptions", -120);
            Check(mixerModel.Speed == 50 && mixerModel.Pitch == -6, "tempo lower limits");
            mixerModel.SelectedTrack = null;
            var bpmButton = (FrameworkElement)window.FindName("BackingBpmButton");
            void WheelBpm(int delta)
            {
                var wheel = new System.Windows.Input.MouseWheelEventArgs(System.Windows.Input.Mouse.PrimaryDevice, 0, delta)
                { RoutedEvent = System.Windows.Input.Mouse.PreviewMouseWheelEvent };
                bpmButton.RaiseEvent(wheel);
                Check(wheel.Handled, "BPM wheel must not scroll the page");
            }
            mixerModel.MetronomeBpm = 120;
            WheelBpm(120);
            Check(mixerModel.MetronomeBpm == 121, "wheel up increases BPM by one");
            WheelBpm(-120);
            Check(mixerModel.MetronomeBpm == 120, "wheel down decreases BPM by one");
            WheelBpm(60);
            Check(mixerModel.MetronomeBpm == 120, "partial wheel notch waits for remainder");
            WheelBpm(60);
            Check(mixerModel.MetronomeBpm == 121, "partial wheel deltas accumulate to one BPM");
            mixerModel.MetronomeBpm = 300;
            WheelBpm(120);
            Check(mixerModel.MetronomeBpm == 300, "BPM upper limit");
            mixerModel.MetronomeBpm = 20;
            WheelBpm(-120);
            Check(mixerModel.MetronomeBpm == 20, "BPM lower limit");
            mixerModel.MetronomeBpm = 120;
            mixerModel.Volume = 19; mixerModel.MonitorVolume = 42; mixerModel.MetronomeVolume = 65;
            mixerModel.IsMuted = true; mixerModel.MonitorMuted = true; mixerModel.MetronomeMuted = true;
            Check(mixerModel.Volume == 19 && mixerModel.MonitorVolume == 42 && mixerModel.MetronomeVolume == 65, "channel mutes preserve all gains");
            mixerModel.IsMuted = false;
            Check(mixerModel.MonitorMuted && mixerModel.MetronomeMuted, "track mute is independent from guitar and click");
            mixerModel.ResetMixerVolumes();
            Check(mixerModel.Volume == 30 && mixerModel.MonitorVolume == 0 && mixerModel.MetronomeVolume == 100 && mixerModel.MonitorMuted && mixerModel.MetronomeMuted, "reset restores defaults and preserves mute");
            mixerModel.MonitorMuted = false; mixerModel.MetronomeMuted = false;
            var configureClick = typeof(BackingTracksViewModel).GetMethod("MetronomeConfiguration", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            mixerModel.IsMuted = true;
            var clickSettings = (Cube.Desktop.Services.MetronomeSettings)configureClick.Invoke(mixerModel, new object[] { true })!;
            Check(clickSettings.Volume == 1, "track mute preserves independent click gain");
            mixerModel.MetronomeMuted = true;
            clickSettings = (Cube.Desktop.Services.MetronomeSettings)configureClick.Invoke(mixerModel, new object[] { false })!;
            Check(clickSettings.Volume == 0 && mixerModel.MetronomeVolume == 100, "click mute silences synchronized click without changing fader");
            mixerModel.IsMuted = false; mixerModel.MetronomeMuted = false;


            foreach (var theme in new[] { ThemeMode.Dark, ThemeMode.Light })
            foreach (string language in new[] { "pt-BR", "en-US" })
            foreach (int width in new[] { 1600, 640 })
            foreach (int zoom in new[] { 50, 100, 200 })
            {
                ThemeService.Instance.Preview(theme);
                LanguageService.Instance.Preview(language);
                ZoomService.Instance.Preview(zoom);
                window.Width = width;
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                window.UpdateLayout();
                string scenario = $"{theme}-{language}-{width}-{zoom}";
                var query = (FrameworkElement)window.FindName("BackingYouTubeSearch");
                var append = (FrameworkElement)window.FindName("BackingTrackSearchToggle");
                var searchRow = (FrameworkElement)append.Parent;
                Contained(query, searchRow, scenario); Contained(append, searchRow, scenario);
                Check(query.TranslatePoint(new Point(query.ActualWidth, 0), searchRow).X <= append.TranslatePoint(new Point(), searchRow).X,
                    scenario + ": backing-track toggle stays outside the search field");
                if (width == 1600 && zoom == 100 && language == "pt-BR")
                {
                    var searchToggle = (ToggleButton)window.FindName("BackingTrackSearchToggle");
                    bool originalChecked = mixerModel.AppendBackingTrack;
                    foreach (bool enabled in new[] { false, true })
                    {
                        mixerModel.AppendBackingTrack = enabled;
                        window.UpdateLayout();
                        var search = (FrameworkElement)searchToggle.Parent;
                        var capture = new RenderTargetBitmap((int)Math.Ceiling(search.ActualWidth), (int)Math.Ceiling(search.ActualHeight), 96, 96, PixelFormats.Pbgra32);
                        var drawing = new DrawingVisual();
                        using (var dc = drawing.RenderOpen())
                        {
                            dc.DrawRectangle((Brush)window.FindResource("CubeCanvas"), null, new Rect(search.RenderSize));
                            dc.DrawRectangle(new VisualBrush(search), null, new Rect(search.RenderSize));
                        }
                        capture.Render(drawing);
                        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(capture));
                        using var file = File.Create(Path.Combine(reports, $"search-{theme}-{enabled}.png")); encoder.Save(file);
                    }
                    mixerModel.AppendBackingTrack = originalChecked;
                }
                var rack = (FrameworkElement)window.FindName("BackingTransportRack");
                var layout = Find<BackingCommandLayout>(rack).Single();
                var groups = layout.Children.Cast<FrameworkElement>().ToArray();
                Check(groups.Length == 3, scenario + ": three functional zones");
                foreach (var group in groups) Contained(group, rack, scenario);
                Disjoint(groups, rack, scenario);
                double alignmentTolerance = Math.Max(1, 1 / (.9 * zoom / 100d)) + .1; // Allow a rounded DIP or one rendered pixel.
                foreach (var row in groups.GroupBy(g => Math.Round(Bounds(g, rack).Top, 1)))
                {
                    var cells = row.OrderBy(g => Bounds(g, rack).Left).ToArray();
                    var gaps = cells.Zip(cells.Skip(1), (left, right) => Bounds(right, rack).Left - Bounds(left, rack).Right).ToArray();
                    Check(gaps.All(g => g >= 16 - alignmentTolerance && Math.Abs(g - gaps[0]) <= alignmentTolerance), scenario + ": equal group spacing " + string.Join(",", gaps));
                }
                foreach (var button in Find<Button>(rack).Where(b => b.Content is BackingButtonFace))
                {
                    Check(button.BorderThickness == new Thickness(0), scenario + ": no outer artwork border");
                    Check(button.FocusVisualStyle != null, scenario + ": keyboard focus retained");
                }

                var controls = Find<FrameworkElement>(rack).Where(c => c.IsVisible && c is Button or Knob or BackingFader).ToArray();
                foreach (var control in controls) Contained(control, rack, scenario);
                Disjoint(controls, rack, scenario);
                var mixer = (FrameworkElement)window.FindName("BackingVolumesPanel");
                Check(mixer.Visibility == Visibility.Collapsed, scenario + ": mixer initially collapsed");
                mixer.Visibility = Visibility.Visible;
                window.UpdateLayout();
                var faders = Find<BackingFader>(mixer).ToArray();
                Check(faders.Length == 3 && faders.All(f => f.Orientation == Orientation.Horizontal), scenario + ": horizontal mixer");
                foreach (var fader in faders) Contained(fader, mixer, scenario);
                Disjoint(faders, mixer, scenario);
                {
                    window.UpdateLayout();
                    foreach (var control in Find<FrameworkElement>(mixer).Where(c => c.IsVisible && c is Button or TextBox or BackingFader)) Contained(control, mixer, scenario);
                    var mixerBitmap = new RenderTargetBitmap((int)Math.Ceiling(mixer.ActualWidth), (int)Math.Ceiling(mixer.ActualHeight), 96, 96, PixelFormats.Pbgra32);
                    var mixerVisual = new DrawingVisual();
                    using (var dc = mixerVisual.RenderOpen())
                    {
                        dc.DrawRectangle((Brush)window.FindResource("CubeCanvas"), null, new Rect(mixer.RenderSize));
                        dc.DrawRectangle(new VisualBrush(mixer), null, new Rect(mixer.RenderSize));
                    }
                    mixerBitmap.Render(mixerVisual);
                    var mixerPng = new PngBitmapEncoder(); mixerPng.Frames.Add(BitmapFrame.Create(mixerBitmap));
                    using var mixerFile = File.Create(Path.Combine(reports, $"mixer-{scenario}.png")); mixerPng.Save(mixerFile);
                }
                mixer.Visibility = Visibility.Collapsed;
                window.UpdateLayout();

                var bitmap = new RenderTargetBitmap((int)Math.Ceiling(rack.ActualWidth), (int)Math.Ceiling(rack.ActualHeight), 96, 96, PixelFormats.Pbgra32);
                var visual = new DrawingVisual();
                using (var dc = visual.RenderOpen())
                {
                    dc.DrawRectangle((Brush)window.FindResource("CubeCanvas"), null, new Rect(rack.RenderSize));
                    dc.DrawRectangle(new VisualBrush(rack) { ViewboxUnits = BrushMappingMode.Absolute, Viewbox = new Rect(rack.RenderSize) }, null, new Rect(rack.RenderSize));
                }
                bitmap.Render(visual);
                var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
                using (var file = File.Create(Path.Combine(reports, scenario + ".png"))) png.Save(file);
                Console.WriteLine($"PASS player layout {scenario}: {rack.RenderSize}");
            }
        }
        finally
        {
            window.Close();
            ZoomService.Instance.Preview(originalZoom);
            LanguageService.Instance.Preview(originalLanguage);
            ThemeService.Instance.Preview(originalTheme);
        }
        Console.WriteLine("PASS player transport containment and render matrix: " + reports);
    }
}
