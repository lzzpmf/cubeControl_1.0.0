using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using Cube.Desktop;
using Cube.Desktop.Controls;
using Cube.Desktop.ViewModels;
using NAudio.Wave;

internal static class BackingResponsivenessTests
{
    public static async Task Run(bool includeDevice = true)
    {
        string folder = Path.Combine(Path.GetTempPath(), "cube-responsiveness-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string source = Path.Combine(folder, "practice.wav");
            using (var writer = new WaveFileWriter(source, new WaveFormat(8000, 16, 1)))
            {
                var samples = new float[8000];
                for (int second = 0; second < 300; second++)
                {
                    for (int i = 0; i < samples.Length; i++) { double t = second + i / 8000d; samples[i] = (float)(Math.Sin(2 * Math.PI * 220 * t) * (.2 + .6 * Math.Abs(Math.Sin(t * 1.4)))); }
                    writer.WriteSamples(samples, 0, samples.Length);
                }
            }
            using var editor = new EditorViewModel(folder);
            var window = new MainWindow(editor, initializeDevices: false) { Width = 1500, Height = 950 };
            window.Show();
            try
            {
                var tabs = (TabControl)window.FindName("Tabs"); tabs.SelectedIndex = 2;
                var model = window.BackingTracks; model.AddPaths([source]);
                for (int i = 0; i < 200 && model.Duration == 0; i++) await Task.Delay(20);
                if (model.Duration != 300) throw new Exception("Profile fixture failed to load.");
                var wave = (BackingWaveform)window.FindName("BackingMainWave");
                model.SelectRange(30, 60); model.SectionName = "Solo"; model.Position = 80; model.AddSection.Execute(null);
                await Task.Delay(600);
                async Task Measure(string label, int count, Action<int> action)
                {
                    await Task.Delay(300);
                    int builds = wave.WaveCacheBuildCount, renders = wave.RenderCount, layouts = 0;
                    long buildTime = wave.WaveCacheBuildTicks;
                    void Layout(object? sender, EventArgs e) => layouts++;
                    window.LayoutUpdated += Layout;
                    using var process = Process.GetCurrentProcess(); var cpu = process.TotalProcessorTime;
                    long allocated = GC.GetTotalAllocatedBytes(true); var elapsed = Stopwatch.StartNew();
                    var timings = new List<double>();
                    for (int i = 0; i < count; i++)
                    {
                        var work = Stopwatch.StartNew(); action(i); window.UpdateLayout();
                        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                        timings.Add(work.Elapsed.TotalMilliseconds);
                        await Task.Delay(20);
                    }
                    window.LayoutUpdated -= Layout;
                    timings.Sort(); process.Refresh();
                    int built = wave.WaveCacheBuildCount - builds;
                    double cacheMilliseconds = (wave.WaveCacheBuildTicks - buildTime) * 1000d / Stopwatch.Frequency;
                    if ((label is "seek" or "tabs" or "click" or "playback" or "seek-playing" or "idle" or "menus-playing") && built != 0)
                        throw new Exception($"{label} unnecessarily rebuilt the wave cache {built} times.");
                    if (built > 0 && cacheMilliseconds / built > 250)
                        throw new Exception($"{label}: waveform bitmap construction blocks the UI ({cacheMilliseconds / built:0} ms per image).");
                    Console.WriteLine($"RESP {label}: wall={elapsed.ElapsedMilliseconds}ms CPU={(process.TotalProcessorTime-cpu).TotalMilliseconds:0}ms alloc={(GC.GetTotalAllocatedBytes(true)-allocated)/1024}KB p95={timings[(int)((count-1)*.95)]:0.0}ms max={timings[^1]:0.0}ms renders={wave.RenderCount-renders} cache={wave.WaveCacheBuildCount-builds} cacheMs={(wave.WaveCacheBuildTicks-buildTime)*1000d/Stopwatch.Frequency:0.0} layouts={layouts}");
                }
                await Measure("seek", 60, i => model.Position = i * 3);
                await Measure("click", 20, _ =>
                {
                    wave.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = UIElement.MouseLeftButtonDownEvent });
                    wave.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = UIElement.MouseLeftButtonUpEvent });
                });
                await Measure("tabs", 20, i => tabs.SelectedIndex = i % 2 == 0 ? 0 : 2);
                await Measure("resize", 24, i => window.Width = 1280 + i % 12 * 40);
                await Measure("maximize", 8, i => window.WindowState = i % 2 == 0 ? WindowState.Maximized : WindowState.Normal);
                await Measure("minimize", 8, i => window.WindowState = i % 2 == 0 ? WindowState.Minimized : WindowState.Normal);
                model.Zoom = 4;
                await Measure("pan", 40, i => model.ViewportStart = i * 4);
                model.Zoom = 1;
                await Measure("idle", 50, _ => { });
                if (includeDevice)
                {
                await model.Initialize(); model.SelectedOutput = model.Outputs.FirstOrDefault();
                if (model.SelectedOutput == null) Console.WriteLine("NOT VALIDATED: no WASAPI output for muted playback profiling.");
                else
                {
                    model.IsMuted = true; model.Position = 0; model.PlaySelected();
                    for (int i = 0; i < 100 && !model.IsPlaying; i++) await Task.Delay(20);
                    if (!model.IsPlaying) throw new Exception("Muted playback failed: " + model.Status);
                    string otherSource = Path.Combine(folder, "other.wav"); File.Copy(source, otherSource);
                    model.AddPaths([otherSource]); var activeTrack = model.SelectedTrack; var otherTrack = model.Tracks.Last();
                    await Measure("menus-playing", 8, _ =>
                    {
                        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                        typeof(MainWindow).GetMethod("OpenBackingMenu", flags)!.Invoke(window, new object?[] { wave, "track", otherTrack, null, null, false });
                        var menu = (ContextMenu)typeof(MainWindow).GetField("backingOpenMenu", flags)!.GetValue(window)!;
                        if (model.SelectedTrack != activeTrack || !model.IsPlaying) throw new Exception("Opening another track menu interrupted playback.");
                        menu.IsOpen = false;
                    });
                    model.RemoveTrackItem(otherTrack);
                    if (model.SelectedTrack != activeTrack || !model.IsPlaying) throw new Exception("Removing another track interrupted playback.");
                    await Measure("playback", 120, _ => { });
                    await Measure("seek-playing", 12, i => model.Position = 20 + i * 5);
                    await Task.Delay(300);
                    if (!model.IsPlaying) throw new Exception("Playback interrupted during interaction: " + model.Status);
                    model.Stop.Execute(null);
                }
                }
                else Console.WriteLine("EXCLUDED audio-device: muted WASAPI responsiveness profile");
                Console.WriteLine("PASS waveform responsiveness and cache regression checks");
            }
            finally
            {
                var closed = new TaskCompletionSource();
                window.Closed += (_, _) => closed.TrySetResult();
                window.Close();
                await closed.Task.WaitAsync(TimeSpan.FromSeconds(10));
            }
        }
        finally { Directory.Delete(folder, true); }
    }
}
