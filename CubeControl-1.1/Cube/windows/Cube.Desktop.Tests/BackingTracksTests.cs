using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cube.Desktop;
using Cube.Desktop.Services;
using Cube.Desktop.ViewModels;
using NAudio.Wave;
using NAudio.CoreAudioApi;

internal static class BackingTracksTests
{
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); Console.WriteLine("PASS " + message); }
    public static async Task Run()
    {
        string folder = Path.Combine(Path.GetTempPath(), "cube-backing-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        try
        {
            string source = Path.Combine(folder, "Guitarra estéreo 長い nome.wav");
            WriteWave(source, 48000, 2, 2);
            var analysis = await BackingAudioProcessing.Analyze(source, CancellationToken.None);
            Check(Math.Abs(analysis.Duration - 2) < .001 && analysis.Peaks.Length == 200, "waveform duration and 100 bins/second");
            Check(analysis.Peaks.Max() > .45f, "opposite-phase stereo is not cancelled in waveform");
            foreach (int rate in new[] { 8000, 44100, 48000, 96000 }) foreach (int channels in new[] { 1, 2 }) foreach (int bits in new[] { 16, 24, 32 })
            {
                string formatPath = Path.Combine(folder, $"format-{rate}-{channels}-{bits}.wav");
                using (var writer = new WaveFileWriter(formatPath, bits == 32 ? WaveFormat.CreateIeeeFloatWaveFormat(rate, channels) : new WaveFormat(rate, bits, channels)))
                {
                    var silence = new byte[rate / 10 * writer.WaveFormat.BlockAlign]; writer.Write(silence, 0, silence.Length);
                }
                var result = await BackingAudioProcessing.Analyze(formatPath, CancellationToken.None);
                Check(Math.Abs(result.Duration - .1) < .001 && result.Peaks.All(p => p == 0), $"PCM{bits} {rate} Hz {channels} channel silence");
            }
            string corrupt = Path.Combine(folder, "corrupt.wav"); File.WriteAllText(corrupt, "not audio");
            try { await BackingAudioProcessing.Analyze(corrupt, CancellationToken.None); throw new Exception("Corrupt file accepted"); } catch (FormatException) { Console.WriteLine("PASS corrupt WAV rejected"); }
            string longPath = Path.Combine(folder, "long.wav");
            using (var writer = new WaveFileWriter(longPath, new WaveFormat(8000, 16, 1)))
            {
                var second = new byte[16000]; for (int i = 0; i < 3601; i++) writer.Write(second, 0, second.Length);
            }
            var longAnalysis = await BackingAudioProcessing.Analyze(longPath, CancellationToken.None);
            Check(longAnalysis.Duration == 3601 && longAnalysis.Peaks.Length * sizeof(float) < 2 * 1024 * 1024, "one-hour file is analysed with compact envelopes, without retaining PCM");
            using (var audio = new BackingAudioProcessing(source, .25, 1.25, 1, 0))
            {
                var samples = Drain(audio);
                Check(samples.Length == 48000 * 2, "neutral processing preserves exact range frame count");
                Check(samples.Where((_, i) => i % 2 == 0).Zip(samples.Where((_, i) => i % 2 == 1), (a, b) => Math.Abs(a + b)).Max() < .0001, "neutral processing preserves channel order and phase");
            }
            using (var loop = new BackingAudioProcessing(source, .2, .3, 1, 0, true, .2))
            {
                var samples = new float[48000 * 2]; Check(loop.Read(samples) == samples.Length, "100 ms loop fills one second without ending");
                Check(Math.Abs(samples[0]) < .0001 && Math.Abs(samples[9600]) < .0001, "loop ramps are applied at frame boundaries");
            }
            var guarded = Drain(new BackingPeakGuard(new PeaksProvider()));
            Check(guarded.Length == 2000 && guarded.Max(v => Math.Abs(v)) <= 1.000001, "linked lookahead guard preserves duration and prevents overflow");
            string export = Path.Combine(folder, "export.wav");
            await BackingAudioProcessing.Export(source, export, .25, 1.25, .5, 3, false, CancellationToken.None);
            using (var reader = new WaveFileReader(export)) Check(reader.WaveFormat.BitsPerSample == 16 && reader.WaveFormat.Channels == 2 && Math.Abs(reader.TotalTime.TotalSeconds - 1) < .001, "original export ignores playback speed and pitch");
            byte[] originalDestination = File.ReadAllBytes(export);
            using (var cancelled = new CancellationTokenSource())
            {
                cancelled.Cancel();
                try { await BackingAudioProcessing.Export(source, export, 0, 2, 1, 0, false, cancelled.Token); throw new Exception("Cancellation ignored"); } catch (OperationCanceledException) { }
            }
            Check(File.ReadAllBytes(export).SequenceEqual(originalDestination) && !Directory.EnumerateFiles(folder, "*.tmp").Any(), "cancelled export preserves destination and cleans temporary files");
            string settings = Path.Combine(folder, "backing-tracks-v1.json");
            File.WriteAllText(settings, JsonSerializer.Serialize(new { Version = 1, Sources = new[] { source }, SelectedSource = source, OutputId = (string?)null, Volume = 42, IsMuted = true }));
            using (var model = new BackingTracksViewModel(folder))
            {
                await Wait(() => model.Duration > 0);
                Check(model.Tracks.Count == 1 && model.Volume == 42 && model.IsMuted && !model.IsPlaying, "v1 migration restores preferences without autoplay");
                model.RangeStart = .5; model.RangeEnd = 1;
                model.Position = 1.5; Check(model.Position == 1.5, "loop off permits seek beyond B");
                model.RangeStart = .6; Check(model.Position == 1.5, "marking A does not move position with loop off");
                model.RangeEnd = .2; Check(model.RangeEnd == 1, "inverted A/B is rejected");
                model.IsLooping = true; Check(model.Position == .6, "enabling loop outside selection moves to A");
                model.IsLooping = false; Check(model.RangeStart == .6 && model.RangeEnd == 1, "disabling loop retains points");
                model.Speed = 75; model.Pitch = -1;
                for (int i = 0; i < 99; i++) { string path = Path.Combine(folder, i + ".wav"); File.WriteAllBytes(path, []); model.AddPaths([path]); }
                Check(model.Tracks.Count == 100, "playlist accepts 100 tracks without analysing all entries");
                Check(!model.ContinueSequence, "stop at end is the default");
            }
            using (var restored = new BackingTracksViewModel(folder))
            {
                await Wait(() => restored.Duration > 0);
                Check(restored.Tracks.Count == 100 && restored.Speed == 75 && restored.Pitch == -1 && restored.RangeStart == .6 && !restored.IsLooping && restored.Position == 0, "v2 restores per-track practice without loop or resume autoplay");
            }
            Check(BackingTracksViewModel.FormatTime(3661) == "01:01:01", "time labels handle tracks longer than one hour");
            await CheckSilentTransport(source);
            if (File.Exists(Path.Combine(AppContext.BaseDirectory, "CubeBackingDsp.dll"))) await CheckDsp(source, folder);
            else Console.WriteLine("NOT VALIDATED native DSP: CubeBackingDsp.dll is unavailable; pitch and processed export acceptance remains open.");
            await Render(source, folder);
        }
        finally { Directory.Delete(folder, true); }
    }
    private static async Task Wait(Func<bool> condition)
    {
        for (int i = 0; i < 200 && !condition(); i++) await Task.Delay(20);
        Check(condition(), "asynchronous analysis completes");
    }
    private static void WriteWave(string path, int rate, int channels, int seconds)
    {
        using var writer = new WaveFileWriter(path, WaveFormat.CreateIeeeFloatWaveFormat(rate, channels));
        var samples = new float[rate * channels];
        for (int frame = 0; frame < rate; frame++) for (int c = 0; c < channels; c++) samples[frame * channels + c] = (float)(.5 * Math.Sin(2 * Math.PI * 440 * frame / rate) * (c % 2 == 0 ? 1 : -1));
        for (int i = 0; i < seconds; i++) writer.WriteSamples(samples, 0, samples.Length);
    }
    private static float[] Drain(ISampleProvider source)
    {
        var result = new List<float>(); var buffer = new float[2048 * source.WaveFormat.Channels]; int count;
        while ((count = source.Read(buffer)) > 0) result.AddRange(buffer.Take(count));
        return result.ToArray();
    }
    private static async Task CheckDsp(string source, string folder)
    {
        foreach (double speed in new[] { .5, .75, 1, 1.25, 1.5 }) foreach (double pitch in new[] { -6d, -1, 0, 1, 6 })
        {
            string target = Path.Combine(folder, "processed.wav");
            await BackingAudioProcessing.Export(source, target, 0, 2, speed, pitch, true, CancellationToken.None);
            using var reader = new BackingDecoder(target); var sample = reader.Format; var buffer = new float[(int)(reader.Duration * sample.SampleRate + 1) * sample.Channels]; int read = reader.Read(buffer);
            Check(Math.Abs(reader.Duration - 2 / speed) < .01, $"processed duration at {speed:P0}, {pitch} semitones");
            int first = (int)(sample.SampleRate * .25), last = read / sample.Channels - first; var crossings = new List<double>();
            for (int f = first + 1; f < last; f++)
            {
                double previous = buffer[(f - 1) * sample.Channels], current = buffer[f * sample.Channels];
                if (previous < 0 && current >= 0) crossings.Add(f - 1 - previous / (current - previous));
            }
            double frequency = (crossings.Count - 1) * sample.SampleRate / (crossings[^1] - crossings[0]);
            Check(Math.Abs(1200 * Math.Log2(frequency / (440 * Math.Pow(2, pitch / 12)))) < 5, "processed pitch within 5 cents");
        }
    }
    private static async Task CheckSilentTransport(string source)
    {
        string? endpoint = await Task.Run(() =>
        {
            using var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            string? id = null;
            foreach (var device in devices) { using (device) id ??= device.ID; }
            return id;
        });
        if (endpoint == null) { Console.WriteLine("NOT VALIDATED WASAPI: no active endpoint."); return; }
        using var engine = new BackingAudioEngine();
        await engine.Open(source, endpoint, 0, 1, 0, false, 0, 2, 0, true);
        Check(engine.Snapshot.State == BackingPlaybackState.Ready, "WASAPI opens without autoplay (muted validation)");
        await engine.Play(); await Task.Delay(250);
        Check(engine.Snapshot.State == BackingPlaybackState.Playing && engine.Snapshot.Position > 0, "WASAPI clock advances from the output device");
        await engine.Pause(); double position = engine.Snapshot.Position; await Task.Delay(100);
        Check(engine.Snapshot.State == BackingPlaybackState.Paused && Math.Abs(engine.Snapshot.Position - position) < .01, "pause retains output position");
        await engine.Seek(1.5); Check(Math.Abs(engine.Snapshot.Position - 1.5) < .001, "seek clears pending audio and preserves requested position");
        await engine.Play(); await Task.Delay(800);
        Check(engine.Snapshot.State == BackingPlaybackState.Ended, "natural end is reported by the output, not UI timer");
        await engine.Configure(1, 0, true, .2, .3); await engine.Seek(.2); await engine.Play(); await Task.Delay(550);
        Check(engine.Snapshot.State == BackingPlaybackState.Playing && engine.Snapshot.Repetitions >= 3, "short loop survives several output cycles");
        await engine.ReleaseOutput(); Check(engine.Snapshot.State == BackingPlaybackState.Paused && engine.Snapshot.OutputId == null, "output release pauses without fallback");
    }
    private static async Task Render(string source, string folder)
    {
        using var model = new EditorViewModel(Path.Combine(folder, "ui"));
        var window = new MainWindow(model); window.Show();
        try
        {
            var tabs = (TabControl)window.FindName("Tabs"); tabs.SelectedIndex = 3;
            window.BackingTracks.AddPaths([source]); await Wait(() => window.BackingTracks.Duration > 0);
            string reports = "C:/CubeControl-Build/backing-validation/reports"; Directory.CreateDirectory(reports);
            foreach (var (width, height) in new[] { (1280, 720), (640, 480), (1800, 1000) })
            {
                window.Width = width; window.Height = height; await Task.Delay(150); window.UpdateLayout();
                var page = (FrameworkElement)window.FindName("BackingPage");
                Check(page.ActualWidth > 0, "backing page is visible at " + width);
                Check(((FrameworkElement)window.FindName("BackingMiniPlayer")).Visibility == Visibility.Collapsed, "mini-player is hidden on its own page");
                var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32); bitmap.Render(window);
                var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap)); using var stream = File.Create(Path.Combine(reports, $"backing-{width}.png")); encoder.Save(stream);
            }
            foreach (double scale in new[] { 1.5, 2.0 })
            {
                window.Width = 1280; window.Height = 720;
                ((FrameworkElement)window.FindName("ClientArea")).LayoutTransform = new ScaleTransform(scale, scale);
                await Task.Delay(100); window.UpdateLayout();
                var page = (FrameworkElement)window.FindName("BackingPage");
                Check(page.ActualWidth > 0, "backing layout supports local scale " + scale);
                var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32); bitmap.Render(window);
                var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap)); using var stream = File.Create(Path.Combine(reports, $"backing-scale-{scale}.png")); encoder.Save(stream);
            }
        }
        finally { window.Close(); }
    }
    private sealed class PeaksProvider : ISampleProvider
    {
        private int position;
        public WaveFormat WaveFormat => WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);
        public int Read(Span<float> buffer)
        {
            int count = Math.Min(buffer.Length, 2000 - position);
            for (int i = 0; i < count; i++) buffer[i] = (position + i) is >= 300 and < 310 ? 2 : .5f;
            position += count; return count;
        }
    }
}
