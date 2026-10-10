using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cube.Desktop;
using Cube.Desktop.Appearance;
using Cube.Desktop.Localization;
using Cube.Desktop.Services;
using Cube.Desktop.ViewModels;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using ThemeMode = Cube.Desktop.Appearance.ThemeMode;

internal static class MetronomeTests
{
    private static void Check(bool condition, string label) { if (!condition) throw new Exception(label); Console.WriteLine("PASS " + label); }
    public static async Task Run()
    {
        var settings = new MetronomeSettings(true, 137, 4);
        foreach (var (beats, eighth) in new[] { (2, false), (3, false), (4, false), (6, true) })
        {
            var meter = settings with { Beats = beats, EighthNotes = eighth };
            for (int beat = 0; beat < 100000; beat += 997)
            {
                double position = beat * meter.Interval;
                CheckBeat(meter.BeatAt(position) == beat % beats + 1);
                CheckBeat(Math.Abs(MetronomeClick.Sample(meter, position + .001)) > .01);
                CheckBeat(MetronomeClick.Sample(meter, position + .03) == 0);
            }
            Check(true, $"{beats}/{(eighth ? 8 : 4)} precise clicks without cumulative drift");
        }
        Check(MetronomeClick.Sample(settings with { Enabled = false }, .001) == 0, "disabled click is silent");
        Check(MetronomeClick.Sample(settings with { Volume = 0 }, .001) == 0, "zero click volume is silent");
        Check(MetronomeClick.Sample(settings with { Bpm = double.NaN }, .001) == 0, "invalid tempo is silent");
        Check(settings.BeatAt(settings.Interval * 4 + .01) == 1, "bar accent repeats");
        Check(MetronomeClick.Sample(settings, .001) != MetronomeClick.Sample(settings with { Accent = false }, .001), "accent has distinct sound");
        Check(Math.Abs(MetronomeClick.Sample(settings with { Offset = 1 }, 1.001) - MetronomeClick.Sample(settings, .001)) < 1e-5, "first beat offset shifts grid");
        Check(Math.Abs(MetronomeClick.SourcePosition(24000, 48000, .25, 1, true, .2, .75) - .2) < 1e-8, "loop boundary maps back to A");
        Check(Math.Abs(MetronomeClick.SourcePosition(48000, 48000, 1, .75, false, 0, 0) - 1.75) < 1e-8, "speed maps samples to source position");
        var taps = new MetronomeTapTempo();
        Check(taps.Tap(0) == null && taps.Tap(.5) == 120 && taps.Tap(1) == 120, "tap tempo averages intervals");
        for (int i = 1; i <= 6; i++) taps.Tap(1 + i * .6);
        Check(Math.Abs(taps.Tap(5.2)!.Value - 100) < .001, "tap retains last five intervals");
        Check(taps.Tap(8) == null && Math.Abs(taps.Tap(8.5)!.Value - 120) < .001, "tap timeout resets history");
        var synth = new MetronomeSampleProvider(settings);
        var samples = new float[96000]; synth.Read(samples); synth.Configure(settings, true); synth.Read(samples);
        Check(samples.All(x => x == 0), "independent mute clears click audio");
        var timbres = new[] { "Click", "Bell", "Ping", "Woodblock", "Hi-hat" }.Select(sound =>
            Enumerable.Range(0, 1200).Select(frame => MetronomeClick.Sample(settings with { Sound = sound }, frame / 48000d)).ToArray()).ToArray();
        Check(timbres.All(sound => sound.All(float.IsFinite) && sound.Any(x => Math.Abs(x) > .001)), "all five timbres produce finite audible samples");
        for (int i = 0; i < timbres.Length; i++) for (int j = i + 1; j < timbres.Length; j++)
            Check(!timbres[i].SequenceEqual(timbres[j]), "timbres are distinct");
        string folder = Path.Combine(Path.GetTempPath(), "cube-metronome-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        try
        {
            string source = Path.Combine(folder, "fixture.wav");
            using (var writer = new WaveFileWriter(source, WaveFormat.CreateIeeeFloatWaveFormat(48000, 1))) writer.WriteSamples(new float[144000], 0, 144000);
            string clickFile = Path.Combine(folder, "custom.wav");
            using (var writer = new WaveFileWriter(clickFile, WaveFormat.CreateIeeeFloatWaveFormat(48000, 2)))
                writer.WriteSamples(Enumerable.Range(0, 9600).Select(i => (float)(.5 * Math.Sin(i / 2 * 2 * Math.PI * 700 / 48000))).ToArray(), 0, 9600);
            var custom = MetronomeSoundBuffer.Load(clickFile);
            Check(custom.Sample(.001) != 0 && custom.Sample(3) == 0, "custom WAV decoded with finite duration");
            string tooLong = Path.Combine(folder, "long.wav");
            using (var writer = new WaveFileWriter(tooLong, WaveFormat.CreateIeeeFloatWaveFormat(48000, 1))) writer.WriteSamples(new float[144000], 0, 144000);
            bool rejected = false; try { MetronomeSoundBuffer.Load(tooLong); } catch (InvalidDataException) { rejected = true; }
            Check(rejected, "WAV longer than two seconds rejected");
            string invalid = Path.Combine(folder, "invalid.wav"); File.WriteAllText(invalid, "invalid");
            rejected = false; try { MetronomeSoundBuffer.Load(invalid); } catch { rejected = true; }
            Check(rejected, "invalid WAV rejected");
            using (var model = new BackingTracksViewModel(folder))
            {
                Check(!model.FollowTrack && !model.MetronomeEnabled && model.MetronomeBpm == 120 && model.MetronomeVolume == 30, "fresh defaults and no autoplay");
                await model.ImportMetronomeSound(clickFile);
                Check(model.CustomMetronomeFiles.Count == 1 && model.MetronomeSound != clickFile && File.Exists(model.MetronomeSound), "import copies and selects WAV");
                string selectedSound = model.MetronomeSound;
                await model.ImportMetronomeSound(tooLong);
                Check(model.MetronomeSound == selectedSound && model.CustomMetronomeFiles.Count == 1, "failed import retains selection");
                model.MetronomeBpm = 140; model.MetronomeMeter = "6/8"; model.CountInBars = 2;
                model.AddPaths([source]); await Wait(() => model.Duration > 0);
                model.FollowTrack = true; model.MetronomeBpm = 100; model.FirstBeatOffset = .75;
                model.Position = 1; model.MarkFirstBeat.Execute(null); Check(model.FirstBeatOffset == 1, "mark current position as first beat");
            }
            using (var restored = new BackingTracksViewModel(folder))
            {
                Check(restored.FollowTrack && !restored.MetronomeEnabled && restored.MetronomeMeter == "6/8" && restored.CountInBars == 2, "preferences restore without autoplay or playlist restore");
                Check(restored.CustomMetronomeFiles.Count == 1 && restored.MetronomeSound == restored.CustomMetronomeFiles[0], "custom sound preferences restore");
                restored.AddPaths([source]); await Wait(() => restored.Duration > 0);
                Check(restored.Bpm == 100 && restored.FirstBeatOffset == 1, "manual BPM and alignment restore per source");
                restored.FollowTrack = false; Check(restored.MetronomeBpm == 140, "independent tempo remains separate");
            }
            string savedCustom = Directory.GetFiles(Path.Combine(folder, "metronome-sounds"))[0]; File.Delete(savedCustom);
            using (var missing = new BackingTracksViewModel(folder)) Check(missing.MetronomeSound == "Click", "missing saved WAV falls back to Click");
            string legacy = Path.Combine(folder, "legacy"); Directory.CreateDirectory(legacy);
            File.WriteAllText(Path.Combine(legacy, "backing-metronome-v1.json"), "{\"Version\":1,\"Bpm\":135}");
            using (var old = new BackingTracksViewModel(legacy)) Check(old.MetronomeSound == "Click" && old.MetronomeBpm == 135, "legacy preferences retain tempo and default to Click");
            await Audio(source, folder);
            await Render(folder);
        }
        finally { Directory.Delete(folder, true); }
    }
    private static void CheckBeat(bool condition) { if (!condition) throw new Exception("Metronome timing drift"); }
    private static async Task Wait(Func<bool> predicate) { for (int i = 0; i < 150 && !predicate(); i++) await Task.Delay(40); Check(predicate(), "asynchronous operation completed"); }
    private static async Task Audio(string source, string folder)
    {
        string? endpoint = await Task.Run(() => { using var devices = new MMDeviceEnumerator(); foreach (var device in devices.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)) { using (device) return device.ID; } return null; });
        if (endpoint == null) { Console.WriteLine("NOT VALIDATED: no WASAPI endpoint"); return; }
        using var engine = new BackingAudioEngine();
        await engine.ConfigureMetronome(new(true, 300, 2, CountInBars: 1));
        await engine.Open(source, endpoint, 0, 1, 0, false, 0, 3, 0, true);
        await engine.Play(true); await Task.Delay(180);
        Check(engine.IsCountingIn && engine.Snapshot.Position == 0, "count-in holds source position using output clock");
        await engine.Pause(); double position = engine.Snapshot.Position; await Task.Delay(100);
        Check(engine.Snapshot.Position == position, "pause retains count-in position");
        await engine.Play(); await Task.Delay(550);
        Check(!engine.IsCountingIn && engine.Snapshot.Position > 0, "resume completes count-in without repeating");
        await engine.Seek(1); Check(!engine.IsCountingIn && engine.Snapshot.Position == 1, "seek cancels count-in and aligns source");
        await engine.Stop(); await engine.Play(true); await Task.Delay(80); await engine.Stop();
        Check(!engine.IsCountingIn && engine.Snapshot.State == BackingPlaybackState.Ready, "stop cancels count-in");
        await engine.Configure(.75, 0, true, .2, .3); await engine.Seek(.2); await engine.Play(); await Task.Delay(600);
        Check(engine.Snapshot.Repetitions >= 2 && !engine.IsCountingIn, "speed and A/B loops do not repeat count-in");
        await engine.ReleaseOutput(); Check(!engine.IsCountingIn && engine.Snapshot.OutputId == null, "release cancels audio/count-in");
        if (Environment.GetCommandLineArgs().Contains("--metronome-loopback")) await Loopback(endpoint, folder);
        using var independent = new BackingTracksViewModel(Path.Combine(folder, "independent")) { IsMuted = true, SelectedOutput = new(endpoint, "Fixture") };
        independent.ToggleMetronome.Execute(null); await Wait(() => independent.MetronomeEnabled); await Task.Delay(180);
        Check(independent.Tracks.Count == 0 && independent.MetronomeBeat > 0, "independent WASAPI plays without a track");
        independent.FollowTrack = true; Check(!independent.MetronomeEnabled, "mode switch stops metronome");
        independent.ToggleMetronome.Execute(null); await Task.Delay(50); Check(!independent.MetronomeEnabled, "follow without track requires manual BPM");
        independent.FollowTrack = false; independent.ToggleMetronome.Execute(null); await Wait(() => independent.MetronomeEnabled);
        independent.SelectedOutput = null; Check(!independent.MetronomeEnabled, "output loss stops independent metronome");
        using var follow = new BackingTracksViewModel(Path.Combine(folder, "follow")) { IsMuted = true, SelectedOutput = new(endpoint, "Fixture"), FollowTrack = true };
        follow.AddPaths([source]); await Wait(() => follow.Duration > 0);
        follow.MetronomeBpm = 300; follow.MetronomeMeter = "2/4"; follow.CountInBars = 1;
        follow.ToggleMetronome.Execute(null); await Wait(() => follow.MetronomeEnabled);
        follow.PlaySelected(); await Wait(() => follow.IsPlaying); await Task.Delay(100);
        Check(follow.Position == 0 && follow.MetronomeBeat > 0, "player command runs configured count-in");
        await Task.Delay(550); Check(follow.Position > 0, "player starts source after count-in");
        follow.PlayPause.Execute(null); await Wait(() => follow.State == BackingPlaybackState.Paused);
        double paused = follow.Position;
        follow.PlayPause.Execute(null); await Wait(() => follow.IsPlaying); await Task.Delay(150);
        Check(follow.Position > paused, "player resumes without a second count-in");
        follow.Speed = 75; await Task.Delay(200);
        Check(follow.MetronomeTempoLabel.StartsWith("225"), "follow mode uses source BPM times playback speed");
        follow.Stop.Execute(null); await Wait(() => follow.State == BackingPlaybackState.Ready);
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        follow.BeforePlayback = () => ready.Task;
        follow.PlaySelected(); await Wait(() => follow.IsLoading);
        follow.Stop.Execute(null); ready.SetResult(); await Task.Delay(150);
        Check(!follow.IsPlaying && !follow.IsLoading, "stop cancels pending start before count-in");
    }
    private static async Task Loopback(string endpoint, string folder)
    {
        string reference = Path.Combine(folder, "reference.wav");
        var samples = new float[96000];
        for (int i = 0; i < samples.Length; i++)
        {
            double time = i / 48000d;
            double phase = (time - .25) % .5;
            samples[i] = time >= .25 && phase is >= 0 and < .025 ? (float)(.3 * Math.Sin(2 * Math.PI * 440 * time)) : 0;
        }
        using (var writer = new WaveFileWriter(reference, WaveFormat.CreateIeeeFloatWaveFormat(48000, 1))) writer.WriteSamples(samples, 0, samples.Length);
        using var engine = new BackingAudioEngine();
        await engine.ConfigureMetronome(new(true, 120, 4, Accent: false, Volume: .15, Offset: .25));
        await engine.Open(reference, endpoint, 0, 1, 0, false, 0, 2, .3, false);
        using var capture = new BackingRecording(endpoint, Path.Combine(folder, "loopback.wav"), true);
        await engine.Play(); await Task.Delay(2300);
        Check(engine.Snapshot.State == BackingPlaybackState.Ended, "metronome stops at natural track end");
        Check(await capture.Stop() == null, "loopback capture finishes successfully");
        using var reader = new BackingDecoder(capture.TemporaryPath);
        var recorded = new float[(int)Math.Ceiling(reader.Duration * reader.Format.SampleRate) * reader.Format.Channels];
        int count = reader.Read(recorded);
        int channels = reader.Format.Channels, rate = reader.Format.SampleRate, window = rate / 100;
        var clicks = new List<double>(); var markers = new List<double>();
        for (int offset = 0; offset + window * channels <= count; offset += window * channels)
        {
            double Energy(double frequency)
            {
                double real = 0, imaginary = 0;
                for (int j = 0; j < window; j++) { double angle = 2 * Math.PI * frequency * j / rate; real += recorded[offset + j * channels] * Math.Cos(angle); imaginary += recorded[offset + j * channels] * Math.Sin(angle); }
                return 2 * Math.Sqrt(real * real + imaginary * imaginary) / window;
            }
            double time = offset / (double)(rate * channels);
            if (Energy(1100) > .003 && (clicks.Count == 0 || time - clicks[^1] > .1)) clicks.Add(time);
            if (Energy(440) > .012 && (markers.Count == 0 || time - markers[^1] > .1)) markers.Add(time);
        }
        Check(clicks.Count >= 3 && markers.Count >= 3, "real loopback contains click and reference track");
        Check(clicks.Zip(clicks.Skip(1)).All(pair => Math.Abs(pair.Second - pair.First - .5) < .03), "real output click intervals match 120 BPM");
        Check(markers.All(marker => clicks.Any(click => Math.Abs(click - marker) < .03)), "clicks align with reference track first-beat offset");
        string reports = "C:/CubeControl-Build/metronome-validation/reports"; Directory.CreateDirectory(reports);
        File.Copy(capture.TemporaryPath, Path.Combine(reports, "metronome-reference-loopback.wav"), true);
    }
    private static async Task Render(string folder)
    {
        ThemeService.Instance.ReadWindowsTransparency = () => false;
        using var model = new EditorViewModel(Path.Combine(folder, "render"));
        var window = new MainWindow(model); window.Show();
        string reports = "C:/CubeControl-Build/metronome-validation/reports"; Directory.CreateDirectory(reports);
        try
        {
            ((TabControl)window.FindName("Tabs")).SelectedIndex = 2;
            foreach (var theme in new[] { "Dark", "Light", "System", "HighContrast" })
            foreach (string language in new[] { "pt-BR", "en-US" })
            foreach (var (width, scale) in new[] { (1280, 1d), (640, 1d), (1280, 1.5), (640, 2d) })
            {
                if (theme == "HighContrast") Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.HighContrast);
                else ThemeService.Instance.Preview(Enum.Parse<ThemeMode>(theme));
                LanguageService.Instance.Select(language);
                window.Width = width; window.Height = 900;
                ((FrameworkElement)window.FindName("ClientArea")).LayoutTransform = new ScaleTransform(scale, scale);
                await Task.Delay(150); window.UpdateLayout();
                var panel = (FrameworkElement)window.FindName("BackingTransportRack");
                var metronomePanel = (FrameworkElement)window.FindName("BackingMetronomePanel");
                Check(Descendants(panel).Contains(metronomePanel), "metronome belongs to player");
                var faders = Descendants(panel).OfType<Cube.Desktop.Controls.BackingFader>().ToArray();
                Check(faders.Length == 3 && faders.All(f => f.Orientation == Orientation.Vertical), "player mixer contains three vertical faders");
                Check(panel.ActualWidth > 0 && panel.ActualHeight > 0, $"panel without track {theme}/{language}/{width}/{scale}");
                foreach (var control in Descendants(panel).OfType<FrameworkElement>().Where(x => x is ButtonBase or ComboBox or TextBox or Cube.Desktop.Controls.Knob))
                {
                    var bounds = control.TransformToAncestor(panel).TransformBounds(new Rect(0, 0, control.ActualWidth, control.ActualHeight));
                    CheckBeat(bounds.Left >= -1 && bounds.Right <= panel.ActualWidth + 1);
                }
                var tap = Descendants(panel).OfType<Button>().First(x => x.Command == window.BackingTracks.TapMetronome);
                tap.Focus();
                var menuKey = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(window), 0, Key.Apps) { RoutedEvent = Keyboard.PreviewKeyDownEvent };
                tap.RaiseEvent(menuKey); Check(menuKey.Handled, "keyboard opens metronome menu");
                var menu = (ContextMenu?)typeof(MainWindow).GetField("backingOpenMenu", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(window);
                Check(menu is { IsOpen: true }, "metronome menu is open");
                menu!.IsOpen = false;
                var key = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(window), 0, Key.Space) { RoutedEvent = Keyboard.PreviewKeyDownEvent };
                tap.RaiseEvent(key); Check(!key.Handled, "metronome buttons retain Space instead of triggering track transport");
                var bitmap = new RenderTargetBitmap((int)Math.Ceiling(panel.ActualWidth), (int)Math.Ceiling(panel.ActualHeight), 96, 96, PixelFormats.Pbgra32);
                var visual = new DrawingVisual(); using (var context = visual.RenderOpen()) context.DrawRectangle(new VisualBrush(panel), null, new Rect(0, 0, panel.ActualWidth, panel.ActualHeight));
                bitmap.Render(visual);
                var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap)); using var file = File.Create(Path.Combine(reports, $"metronome-{theme}-{language}-{width}-{scale}.png")); png.Save(file);
            }
        }
        finally { window.Close(); LanguageService.Instance.Select("pt-BR"); ThemeService.Instance.Preview(ThemeMode.Dark); }
    }
    private static IEnumerable<DependencyObject> Descendants(DependencyObject parent)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i); yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }
}
