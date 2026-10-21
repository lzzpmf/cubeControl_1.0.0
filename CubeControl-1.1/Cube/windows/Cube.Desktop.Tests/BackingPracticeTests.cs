using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cube.Desktop;
using Cube.Desktop.Controls;
using Cube.Desktop.ViewModels;
using NAudio.Wave;

internal static class BackingPracticeTests
{
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); Console.WriteLine("PASS " + message); }
    public static async Task Run()
    {
        string folder = Path.Combine(Path.GetTempPath(), "cube-practice-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        Cube.Desktop.Appearance.ThemeService.Instance.ReadWindowsTransparency = () => false;
        Cube.Desktop.Appearance.ThemeService.Instance.Preview(Cube.Desktop.Appearance.ThemeMode.Dark);
        try
        {
            await BackingMusicTests.Run(folder);
            string source = Path.Combine(folder, "Manhattan — prática.wav");
            using (var writer = new WaveFileWriter(source, WaveFormat.CreateIeeeFloatWaveFormat(8000, 1)))
            {
                var samples = new float[8000];
                for (int second = 0; second < 120; second++)
                {
                    for (int i = 0; i < samples.Length; i++) { double t = second + i / 8000d; samples[i] = (float)(Math.Sin(2 * Math.PI * 220 * t) * (.12 + .7 * Math.Pow(Math.Abs(Math.Sin(t * 1.2)), 3))); }
                    writer.WriteSamples(samples, 0, samples.Length);
                }
            }
            var meter = new Cube.Desktop.Services.BackingLevelMeter(new MeterFixture());
            float[] metered = new float[4];
            Check(meter.Read(metered) == 4 && metered.SequenceEqual(new[] { .2f, -.8f, .4f, -.1f }), "level metering preserves audio samples");
            Check(Math.Abs(meter.ConsumePeak() - .8f) < .0001 && meter.ConsumePeak() == 0, "level meter reports the strongest channel and resets between polls");
            var envelope = new Cube.Desktop.Services.BackingMeterEnvelope();
            var peak = envelope.Update(.5f, true, 10);
            Check(Math.Abs(peak.LevelDb + 6.0206) < .001, "meter converts sample amplitude to dBFS");
            var falling = envelope.Update(0, true, 10.25);
            Check(Math.Abs(falling.LevelDb - (peak.LevelDb - 6)) < .001 && falling.PeakDb == peak.PeakDb,
                "meter falls at 24 dB per second while peak is held");
            Check(envelope.Update(1.2f, true, 11).Clipping && envelope.Update(0, true, 11.9).Clipping,
                "clipping uses unclamped samples and remains visible for one second");
            Check(!envelope.Update(0, true, 12.01).Clipping, "clip hold expires with elapsed time");
            Check(envelope.Update(0, false, 12.02) == Cube.Desktop.Services.BackingMeterReading.Silent,
                "stopping source clears level, peak and clipping");
            using var editor = new EditorViewModel(folder);
            var window = new MainWindow(editor, initializeDevices: false);
            window.Show();
            try
            {
                ((TabControl)window.FindName("Tabs")).SelectedIndex = 2;
                var model = window.BackingTracks;
                model.AddPaths([source]);
                for (int i = 0; i < 200 && model.Duration == 0; i++) await Task.Delay(20);
                Check(model.Duration == 120, "practice fixture analysed");
                var playbackState = typeof(BackingTracksViewModel).GetProperty(nameof(model.State))!;
                model.Zoom = 4; model.Position = 23;
                playbackState.SetValue(model, Cube.Desktop.Services.BackingPlaybackState.Playing);
                Check(model.ViewportStart == 0, "zoomed playback retains current page while playhead is inside it");
                model.Position = 25;
                Check(model.ViewportStart > 0 && model.Position > model.ViewportStart && model.Position < model.ViewportStart + model.ViewportDuration, "zoomed playback advances viewport before leaving visible page");
                double pageStart = model.ViewportStart; model.Position = 26;
                Check(model.ViewportStart == pageStart, "playback does not rebuild viewport on every tick");
                model.Position = 5;
                Check(model.ViewportStart == 0, "backward seek or loop return follows playhead");
                model.Position = 119;
                Check(model.ViewportStart == 90, "follow clamps viewport at track end");
                model.Zoom = 8;
                Check(model.Position >= model.ViewportStart && model.Position <= model.ViewportStart + model.ViewportDuration, "changing zoom during playback keeps playhead visible");
                playbackState.SetValue(model, Cube.Desktop.Services.BackingPlaybackState.Paused);
                model.ViewportStart = 30; model.Position = 5;
                Check(model.ViewportStart == 30, "paused waveform permits manual navigation");
                model.Zoom = 1; playbackState.SetValue(model, Cube.Desktop.Services.BackingPlaybackState.Playing); model.Position = 80;
                Check(model.ViewportStart == 0, "full-track view does not scroll");
                playbackState.SetValue(model, Cube.Desktop.Services.BackingPlaybackState.Ready); model.Position = 0;
                model.OriginalKey = "Em"; model.Pitch = -1; model.Bpm = 120; model.Speed = 75;
                Check(model.CurrentKey == "D#m" && model.CurrentBpm.StartsWith("90 BPM"), "original key and BPM transpose independently with pitch and speed");
                Check(BackingTracksViewModel.TransposeKey("C", -1) == "B" && BackingTracksViewModel.TransposeKey("Bm", 1) == "Cm", "transposition wraps both octave boundaries");
                Check(BackingTracksViewModel.TransposeKey("Não definido", 3) == "Não definido", "unknown key is not guessed");
                Check(model.SelectedTrack!.MetadataLabel.Contains("120 BPM") && model.SelectedTrack.MetadataLabel.Contains("Em"), "playlist metadata reflects manual BPM and key");
                model.ZoomIn.Execute(null); Check(model.Zoom == 2, "zoom plus doubles magnification");
                model.ZoomOut.Execute(null); Check(model.Zoom == 1, "zoom minus restores magnification");
                model.Zoom = 8; model.ViewportStart = 60; model.FitWaveform.Execute(null);
                Check(model.Zoom == 1 && model.ViewportStart == 0, "fit to screen restores the full track");
                model.Zoom = 4; model.ViewportStart = 119;
                Check(model.ViewportDuration == 30 && model.ViewportStart == 90, "zoomed viewport is clamped to the track end");
                model.Zoom = 1;
                Check(model.ViewportStart == 0, "zooming out resets an out-of-range viewport");
                model.Zoom = double.NaN; model.ViewportStart = double.PositiveInfinity;
                Check(model.Zoom == 1 && model.ViewportStart == 0, "invalid viewport input is ignored");
                model.SelectRange(10, 20); model.SelectRange(40, 60);
                Check(model.RangeStart == 40 && model.RangeEnd == 60, "drag selection can replace a range beyond the previous B atomically");
                model.SelectRange(70, 60); model.SelectRange(50, 50.01);
                Check(model.RangeStart == 40 && model.RangeEnd == 60, "inverted and too-short drag selections are rejected");
                model.Position = 15; model.SectionName = "Solo"; model.AddSection.Execute(null);
                model.Position = 0; model.SectionName = "Intro"; model.AddSection.Execute(null);
                Check(model.Sections!.Count == 2 && model.Sections[0].Name == "Intro", "named sections are chronologically ordered");
                model.SelectedSection = model.Sections[1]; model.GoToSection.Execute(null);
                Check(model.Position == 15, "section navigation seeks to its marker");
                model.RemoveSection.Execute(null); Check(model.Sections.Count == 1 && model.SelectedSection == null, "removing a section clears its selection");
                var track = model.SelectedTrack;
                model.Search = "not in playlist"; await Task.Delay(50);
                Check(model.FilteredTracks.IsEmpty && model.SelectedTrack == track && model.Position == 15, "filtering the selected track out does not stop or change it");
                Check(string.IsNullOrEmpty(track!.Tuning) && track.Preset == 0, "track starts without tuning or preset associations");
                track.Tuning = "Drop D";
                model.Search = "Drop D"; Check(!model.FilteredTracks.IsEmpty, "playlist search includes explicitly assigned tuning"); model.Search = "";
                Check(((ListBox)window.FindName("BackingTrackList")).SelectedItem == track, "clearing search restores the active row highlight");
                var wave = (BackingWaveform)window.FindName("BackingMainWave");
                void Key(UIElement target, System.Windows.Input.Key key)
                {
                    target.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(window), 0, key) { RoutedEvent = Keyboard.PreviewKeyDownEvent });
                }
                model.ResetRange.Execute(null); model.Position = 20; Key(wave, System.Windows.Input.Key.OemOpenBrackets);
                model.Position = 30; Key(wave, System.Windows.Input.Key.OemCloseBrackets);
                Check(model.RangeStart != 20 || model.RangeEnd != 30, "bracket shortcuts no longer configure loop"); model.SelectRange(20, 30);
                Key(wave, System.Windows.Input.Key.L); Check(model.IsLooping, "L enables the selected loop"); Key(wave, System.Windows.Input.Key.L);
                model.Speed = 100; Key(wave, System.Windows.Input.Key.Down); Check(model.Speed == 95, "global down shortcut decreases speed by five percent");
                ((TabControl)window.FindName("Tabs")).SelectedIndex = 0;
                Key(window, System.Windows.Input.Key.Up); Check(model.Speed == 100, "practice shortcuts also work outside the backing page");
                ((TabControl)window.FindName("Tabs")).SelectedIndex = 2;
                model.Speed = 95;
                ((FrameworkElement)window.FindName("BackingLibraryLayout")).Visibility = Visibility.Visible; window.UpdateLayout();
                var search = Find<TextBox>((DependencyObject)window.FindName("BackingPlaylistPanel")).First();
                Key(search, System.Windows.Input.Key.Up); Check(model.Speed == 95, "text fields retain their editing keys");
                await Task.Delay(80); window.UpdateLayout();
                Check(((FrameworkElement)window.FindName("BackingPlaylistPanel")).IsVisible, "playlist remains expanded");
                model.LinkedPreset = 2; Check(model.PresetHint.Contains("B") && model.PresetHint.Contains("footswitch"), "preset association explicitly requires physical selection");
                Check(!model.IsMonitoring && model.MonitorVolume == 0 && !model.ToggleMonitor.CanExecute(null), "monitor is off and silent without explicitly selected endpoints");
                model.MonitorVolume = 200; Check(model.MonitorVolume == 100, "monitor volume is independently clamped"); model.MonitorVolume = 0;
                model.Position = 24; model.SelectRange(18, 36); model.IsLooping = true; model.Zoom = 2; model.ViewportStart = 0;
                model.IsLooping = false; model.Position = 40; model.SectionName = "Solo"; model.AddSection.Execute(null); model.Position = 24; model.IsLooping = true;
                await Task.Delay(100); window.UpdateLayout();
                model.SelectedSection = model.Sections!.First(section => section.Name == "Solo");
                model.IsLooping = false;
                model.GoToSection.Execute(null);
                Check(model.Position == 40, "section navigation seeks to the existing ruler marker");
                model.Position = 24; model.IsLooping = true;
                string reports = TestPaths.Report("C:/CubeControl-Build/backing-ui-review/compact-analysis"); Directory.CreateDirectory(reports);
                foreach (var expander in Find<Expander>((DependencyObject)window.FindName("BackingRacks"))) expander.IsExpanded = false;
                foreach (var (theme, language) in new[] { ("Dark", "pt-BR"), ("Light", "en-US"), ("HighContrast", "en-US") })
                {
                if (theme == "HighContrast") Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.HighContrast);
                else Cube.Desktop.Appearance.ThemeService.Instance.Preview(theme == "Dark" ? Cube.Desktop.Appearance.ThemeMode.Dark : Cube.Desktop.Appearance.ThemeMode.Light);
                Cube.Desktop.Localization.LanguageService.Instance.Preview(language);
                foreach (var (width, height, scale) in new[] { (1800, 1000, 1d), (1280, 720, 1d), (640, 720, 1d), (1800, 1000, 1.5), (1280, 720, 1.5), (640, 720, 1.5), (1800, 1000, 2d), (1280, 720, 2d), (640, 720, 2d) })
                {
                    window.Width = width; window.Height = height;
                    ((FrameworkElement)window.FindName("ClientArea")).LayoutTransform = new ScaleTransform(scale, scale);
                    await Task.Delay(500); window.UpdateLayout();
                    BackingUiReviewTests.CheckTransport(window);
                    Check(((FrameworkElement)window.FindName("Workspace")).ActualWidth <= 1441, "workspace is bounded on ultrawide screens");
                    if (width == 640 && scale == 1) Check(((FrameworkElement)window.FindName("BackingPlaylistPanel")).IsVisible, "small windows keep the playlist open");
                    var player = (FrameworkElement)window.FindName("BackingPlayerPanel");
                    var transport = (FrameworkElement)window.FindName("BackingTransportRack");
                    var transportHost = (ContentControl)window.FindName("BackingTransportHost");
                    Check(transportHost.IsVisible && transportHost.Parent == window.FindName("BackingPage"), "transport remains outside the scrollable player");
                    Check(!transport.IsVisible || transport.ActualWidth <= ((FrameworkElement)window.FindName("BackingPage")).ActualWidth + 1, "transport fits the player at every scale");
                    foreach (string name in new[] { "BackingAudioPanel" })
                        Check(!((FrameworkElement)window.FindName(name)).IsVisible || ((FrameworkElement)window.FindName(name)).ActualWidth <= ((FrameworkElement)window.FindName("BackingPage")).ActualWidth + 1, $"{name} fits at {width}, scale {scale}");
                    foreach (string name in new[] { "BackingTransportRack", "BackingArrangement", "BackingAudioPanel" })
                    {
                        var section = (FrameworkElement)window.FindName(name);
                        if (!section.IsVisible) continue;
                        foreach (var control in Find<FrameworkElement>(section).Where(c => c.IsVisible && (c is System.Windows.Controls.Primitives.ButtonBase || c is Knob || c is Slider || c is Wpf.Ui.Controls.ToggleSwitch)))
                        {
                            var bounds = control.TransformToAncestor(section).TransformBounds(new Rect(control.RenderSize));
                            for (DependencyObject? node = VisualTreeHelper.GetParent(control); node != null && node != section; node = VisualTreeHelper.GetParent(node))
                                if (node is System.Windows.Controls.ScrollContentPresenter viewport) bounds.Intersect(viewport.TransformToAncestor(section).TransformBounds(new Rect(viewport.RenderSize)));
                            Check(bounds.IsEmpty || bounds.Left >= -1 && bounds.Right <= section.ActualWidth + 1, $"{name} contains {control.GetType().Name} at {width}/{scale}");
                        }
                        var drawing = new DrawingVisual();
                        using (var dc = drawing.RenderOpen()) dc.DrawRectangle(new VisualBrush(section), null, new Rect(section.RenderSize));
                        var detail = new RenderTargetBitmap((int)Math.Ceiling(section.ActualWidth), (int)Math.Ceiling(section.ActualHeight), 96, 96, PixelFormats.Pbgra32); detail.Render(drawing);
                        var detailEncoder = new PngBitmapEncoder(); detailEncoder.Frames.Add(BitmapFrame.Create(detail));
                        using var detailStream = File.Create(Path.Combine(reports, $"{name}-{theme}-{width}-{scale}.png")); detailEncoder.Save(detailStream);
                    }
                    Check(((FrameworkElement)window.FindName("BackingPlayButton")).ActualWidth >= 36 && wave.ActualHeight >= 64 && Math.Abs(((FrameworkElement)window.FindName("BackingOverview")).ActualHeight - 30) < 1, $"compact targets: play={((FrameworkElement)window.FindName("BackingPlayButton")).ActualWidth}, wave={wave.ActualHeight}, overview={((FrameworkElement)window.FindName("BackingOverview")).ActualHeight}");
                    var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32); bitmap.Render(window);
                    var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    using var stream = File.Create(Path.Combine(reports, $"practice-{theme}-{language}-{width}-{scale}.png")); encoder.Save(stream);
                }
                }
                await BackingUiReviewTests.Run(window, source, reports);
            }
            finally { var closed = new TaskCompletionSource(); window.Closed += (_, _) => closed.TrySetResult(); window.Close(); await closed.Task; }
        }
        finally { Directory.Delete(folder, true); }
    }
    private sealed class MeterFixture : ISampleProvider
    {
        public WaveFormat WaveFormat => WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);
        public int Read(Span<float> buffer) { new float[] { .2f, -.8f, .4f, -.1f }.CopyTo(buffer); return 4; }
    }
    private static IEnumerable<T> Find<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        { var child = VisualTreeHelper.GetChild(root, i); if (child is T match) yield return match; foreach (var nested in Find<T>(child)) yield return nested; }
    }
}
