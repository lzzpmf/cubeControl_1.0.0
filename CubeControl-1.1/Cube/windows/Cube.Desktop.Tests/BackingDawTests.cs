using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cube.Desktop;
using Cube.Desktop.Appearance;
using Cube.Desktop.Controls;
using Cube.Desktop.Localization;
using Cube.Desktop.Services;
using Cube.Desktop.ViewModels;
using NAudio.Wave;
using ThemeMode = Cube.Desktop.Appearance.ThemeMode;

internal static class BackingDawTests
{
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); }
    public static async Task Run()
    {
        string folder = Path.Combine(Path.GetTempPath(), "cube-daw-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        string reports = TestPaths.Report("C:/CubeControl-Build/reports/backing-daw"); Directory.CreateDirectory(reports);
        string file = Path.Combine(folder, "Guitarra.wav");
        using (var writer = new WaveFileWriter(file, WaveFormat.CreateIeeeFloatWaveFormat(48000, 1))) writer.WriteSamples(Enumerable.Repeat(.25f, 48000).ToArray(), 0, 48000);
        var source = new BackingStemSource(Guid.NewGuid(), file, Volume: BackingMixMath.MaximumGain, Pan: 0, LegacyMono: false);
        using (var mix = new BackingSessionAudio([source], 0, 1, 1, 0))
        {
            var samples = new float[4800]; mix.Read(samples, 0, samples.Length);
            Check(Math.Abs(samples[^1] - .25 * BackingMixMath.MaximumGain / Math.Sqrt(2)) < .001, "+6 dB with equal-power mono center");
            mix.SetMix([source with { Pan = -1 }]); mix.Read(samples, 0, samples.Length);
            Check(Math.Abs(samples[^2] - .25 * BackingMixMath.MaximumGain) < .001 && Math.Abs(samples[^1]) < .001, "pan reaches left without cross-channel leakage");
        }
        using (var cycle = new BackingSessionAudio([source], 0, .1, 1, 0, true, 0) { KeepAlive = true })
        {
            var samples = new float[48000];
            Check(cycle.Read(samples, 0, samples.Length) == samples.Length && samples[^1] > .1, "monitor keep-alive preserves repeated loop audio");
        }
        var meter = new BackingSignalMeter(); for (int i = 0; i < 14400; i++) meter.Add(1.25f, 1.25f);
        var reading = meter.Consume(); Check(Math.Abs(reading.Rms - 1.25) < .001 && reading.Peak == 1.25f, "RMS window and unclipped peak retain overload");
        Check(meter.Consume().Peak == 0, "peak consumption clears only the peak accumulator");
        Check(BackingMixMath.FromDb(-61) == 0 && Math.Abs(BackingMixMath.ToDb(1)) < .0001, "silence and unity conversion");
        await CapturePackets(folder);
        using var editor = new EditorViewModel(folder);
        var window = new MainWindow(editor, initializeDevices: false) { ShowInTaskbar = false, Width = 1400, Height = 1000 };
        var originalTheme = ThemeService.Instance.Mode; string originalLanguage = LanguageService.Instance.Language;
        window.Show();
        try
        {
            ((TabControl)window.FindName("Tabs")).SelectedIndex = 2;
            ((FrameworkElement)window.FindName("BackingVolumesPanel")).Visibility = Visibility.Visible;
            var model = window.BackingTracks;
            var track = new BackingTrack(file); model.Tracks.Add(track); model.SelectedTrack = track;
            await model.AddSessionFiles([file]);
            var stem = model.SessionStems![0]; stem.Name = "Guitarra principal"; stem.Color = "#40BFA0"; stem.GainDb = 3; stem.Pan = -.3; stem.IsArmed = true;
            var first = stem.ActiveTake!;
            var second = first with { Id = Guid.NewGuid(), Offset = .2 }; stem.Takes.Add(second);
            bool coherentTakeNotification = false;
            stem.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(BackingStem.ActiveTake)) coherentTakeNotification = stem.Source == second.Source && stem.Offset == second.Offset; };
            stem.ActiveTake = second;
            Check(coherentTakeNotification, "take notification exposes the new source and timing together");
            Check(stem.Takes.Count == 2 && stem.Offset == .2, "take selection retains previous audio and changes timing");
            stem.CanEditRouting = false; stem.ActiveTake = first; stem.InputChannel = 3;
            Check(stem.ActiveTake == second && stem.InputChannel == 0, "take and routing edits are blocked during capture"); stem.CanEditRouting = true;
            var incomplete = second with { Id = Guid.NewGuid(), Incomplete = true }; stem.Takes.Add(incomplete); stem.ActiveTake = incomplete;
            Check(stem.ActiveTake == second, "incomplete take cannot replace active audio");
            model.SelectedInput = new("unavailable", "Unavailable", false);
            Check(!model.ToggleRecording.CanExecute(null), "missing device cannot start capture");
            await Task.Delay(400);
            foreach (var item in model.SessionStems) { item.Peaks = Enumerable.Range(0, 200).Select(i => (float)(.2 + .5 * Math.Abs(Math.Sin(i * .13)))).ToArray(); item.Length = 1; }
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            foreach (var scenario in new[] { (1400d, ThemeMode.Dark, "pt-BR"), (900d, ThemeMode.Light, "en-US") })
            {
                window.Width = scenario.Item1; ThemeService.Instance.Preview(scenario.Item2); LanguageService.Instance.Preview(scenario.Item3);
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); window.UpdateLayout();
                foreach (string name in new[] { "BackingStemList", "SessionMixerContent" })
                {
                    var element = (FrameworkElement)window.FindName(name);
                    Check(element.ActualWidth > 200 && element.ActualHeight > 0, name + " lays out at " + scenario.Item1);
                    var bitmap = new RenderTargetBitmap((int)Math.Ceiling(element.ActualWidth), (int)Math.Ceiling(element.ActualHeight), 96, 96, PixelFormats.Pbgra32);
                    var drawing = new DrawingVisual();
                    using (var dc = drawing.RenderOpen())
                    {
                        dc.DrawRectangle((Brush)window.FindResource("CubeCanvas"), null, new Rect(element.RenderSize));
                        dc.DrawRectangle(new VisualBrush(element), null, new Rect(element.RenderSize));
                    }
                    bitmap.Render(drawing); var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    using var output = File.Create(Path.Combine(reports, $"{name}-{scenario.Item1}.png")); encoder.Save(output);
                }
            }
            await VerifyLayout(window, reports);
        }
        finally { ThemeService.Instance.Preview(originalTheme); LanguageService.Instance.Preview(originalLanguage); window.Close(); }
        Console.WriteLine("RENDERS " + reports);
    }
    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match) yield return match;
            foreach (var nested in Descendants<T>(child)) yield return nested;
        }
    }
    private static async Task VerifyLayout(MainWindow window, string reports)
    {
        await PlayerLayoutTests.VerifyStudio(window, reports);
    }
    private static async Task CapturePackets(string folder)
    {
        string mono = Path.Combine(folder, "capture-mono.wav"), stereo = Path.Combine(folder, "capture-stereo.wav");
        var a = new BackingCaptureRoute(Guid.NewGuid(), 1, 1, mono, true, .1, -1);
        var b = new BackingCaptureRoute(Guid.NewGuid(), 0, 2, stereo, false, 0, 0);
        using (var capture = new BackingMultitrackCapture(WaveFormat.CreateIeeeFloatWaveFormat(48000, 4), [a, b]))
        {
            var samples = new float[4800 * 4];
            for (int i = 0; i < samples.Length; i += 4) { samples[i] = .1f; samples[i + 1] = .25f; samples[i + 2] = .5f; samples[i + 3] = -.5f; }
            byte[] packet = new byte[samples.Length * 4]; Buffer.BlockCopy(samples, 0, packet, 0, packet.Length);
            capture.ProcessPacket(packet); capture.ProcessPacket(packet, silent: true);
            Check(capture.Snapshot(a.Id).Seconds == .2 && capture.Snapshot(b.Id).Seconds == .2, "all routes share the same capture frame count");
            Check(capture.Snapshot(a.Id).Peaks.Any(x => x == .25f) && capture.Snapshot(a.Id).Peaks.Last() == 0, "recorded envelopes include samples and silence");
            Check(await capture.Stop() == null, "normal stop finalizes every writer");
        }
        using (var reader = new WaveFileReader(mono))
        {
            byte[] data = new byte[reader.Length]; reader.ReadExactly(data);
            Check(reader.WaveFormat.Channels == 1 && BitConverter.ToSingle(data, 0) == .25f, "mono extraction preserves raw input despite monitor gain and pan");
            Check(data.AsSpan(4800 * 4).ToArray().All(x => x == 0), "silent packets write actual silence");
        }
        using (var reader = new WaveFileReader(stereo))
        {
            byte[] data = new byte[8]; reader.ReadExactly(data);
            Check(reader.WaveFormat.Channels == 2 && BitConverter.ToSingle(data, 0) == .1f && BitConverter.ToSingle(data, 4) == .25f, "stereo extraction retains channel order and shared channels");
        }
        try { using var invalid = new BackingMultitrackCapture(WaveFormat.CreateIeeeFloatWaveFormat(48000, 2), [a with { FirstChannel = 2 }]); throw new Exception("invalid route accepted"); }
        catch (InvalidDataException) { Console.WriteLine("PASS unavailable channel rejected before capture"); }
        string recovered = Path.Combine(folder, "interrupted.wav");
        using (var interrupted = new BackingMultitrackCapture(WaveFormat.CreateIeeeFloatWaveFormat(48000, 2), [a with { Destination = recovered }]))
        {
            interrupted.ProcessPacket(new byte[480 * 2 * 4], silent: true);
            var failure = new IOException("Simulated device loss");
            Check(ReferenceEquals(await interrupted.Stop(failure), failure), "capture failure is retained for the whole pass");
        }
        using (var recovery = new WaveFileReader(recovered)) Check(recovery.Length > 0, "interrupted capture retains a readable WAV");
        var monitor = new BackingMonitorBuffer(48000);
        monitor.Write(Enumerable.Repeat(.25f, 4800).ToArray());
        float[] output = new float[960], packet480 = Enumerable.Repeat(.25f, 960).ToArray(), packet481 = Enumerable.Repeat(.25f, 962).ToArray();
        for (int i = 0; i < 6000; i++) { monitor.Write(i % 20 == 0 ? packet481 : packet480); monitor.Read(output); }
        Check(monitor.BufferedFrames is > 100 and < 4800 && output.All(x => Math.Abs(x - .25) < .001), "adaptive monitor remains bounded with capture clock drift over one minute");
    }
}
