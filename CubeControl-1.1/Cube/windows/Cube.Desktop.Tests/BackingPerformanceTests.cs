using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Cube.Desktop;
using Cube.Desktop.ViewModels;

internal static class BackingPerformanceTests
{
    public static async Task Run()
    {
        Cube.Desktop.Appearance.ThemeService.Instance.Initialize();
        using var model = new EditorViewModel(Path.Combine(Path.GetTempPath(), "cube-backing-profile-" + Guid.NewGuid().ToString("N")));
        var window = new MainWindow(model) { Width = 1280, Height = 900 };
        window.Show();
        try
        {
            var tabs = (TabControl)window.FindName("Tabs");
            string? source = Environment.GetEnvironmentVariable("CUBE_BACKING_PROFILE_AUDIO");
            if (source == null || !File.Exists(source)) throw new InvalidOperationException("Set CUBE_BACKING_PROFILE_AUDIO to a local MP3/WAV.");
            if (Environment.GetCommandLineArgs().Contains("--backing-preview"))
            {
                tabs.SelectedIndex = 2;
                window.BackingTracks.AddPaths([source]);
                var closed = new TaskCompletionSource();
                window.Closed += (_, _) => closed.TrySetResult();
                var wait = Stopwatch.StartNew();
                while (!closed.Task.IsCompleted && window.BackingTracks.Duration <= 0 && wait.Elapsed < TimeSpan.FromSeconds(30)) await Task.Delay(100);
                await Task.Delay(1000); // Let entrance animations finish before the preview capture.
                if (!closed.Task.IsCompleted)
                {
                    window.UpdateLayout();
                    var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                    bitmap.Render(window);
                    var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                    encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                    string reports = "C:/CubeControl-Build/backing-validation/reports";
                    Directory.CreateDirectory(reports);
                    using var image = File.Create(Path.Combine(reports, "backing-real-track.png"));
                    encoder.Save(image);
                }
                Console.WriteLine("Backing preview opened with the selected local track; close its window to exit.");
                await closed.Task;
                return;
            }
            async Task Measure(string label)
            {
                await Task.Delay(1000);
                int layouts = 0;
                void Layout(object? sender, EventArgs e) => layouts++;
                window.LayoutUpdated += Layout;
                using var process = Process.GetCurrentProcess();
                var cpu = process.TotalProcessorTime;
                long allocated = GC.GetTotalAllocatedBytes(true);
                var watch = Stopwatch.StartNew();
                await Task.Delay(4000);
                window.LayoutUpdated -= Layout;
                process.Refresh();
                Console.WriteLine($"PROFILE {label}: CPU={(process.TotalProcessorTime-cpu).TotalMilliseconds:0}ms / {watch.ElapsedMilliseconds}ms; allocated={(GC.GetTotalAllocatedBytes(true)-allocated)/1024:0}KB; private={process.PrivateMemorySize64/1024/1024}MB; layouts={layouts}");
            }
            tabs.SelectedIndex = 0; await Measure("editor idle");
            tabs.SelectedIndex = 2; await Measure("backing empty");
            var analysis = Stopwatch.StartNew();
            window.BackingTracks.AddPaths([source]);
            while (window.BackingTracks.Duration <= 0 && analysis.Elapsed < TimeSpan.FromSeconds(30)) await Task.Delay(50);
            if (window.BackingTracks.Duration <= 0) throw new Exception("Audio analysis did not complete.");
            Console.WriteLine($"PROFILE analysis={analysis.ElapsedMilliseconds}ms; duration={window.BackingTracks.Duration:0.0}s; peaks={window.BackingTracks.Peaks.Length * sizeof(float)} bytes");
            await Measure("backing loaded idle");
            window.BackingTracks.SelectedOutput = window.BackingTracks.Outputs.FirstOrDefault();
            if (window.BackingTracks.SelectedOutput != null)
            {
                window.BackingTracks.IsMuted = true;
                window.BackingTracks.PlaySelected();
                await Measure("backing playing");
                if (!window.BackingTracks.IsPlaying) throw new Exception("Playback did not stay active during measurement: " + window.BackingTracks.Status);
                var player = (StackPanel)window.FindName("BackingPlayerPanel");
                var waveforms = new[] { (Cube.Desktop.Controls.BackingWaveform)window.FindName("BackingMainWave"), (Cube.Desktop.Controls.BackingWaveform)window.FindName("BackingOverview") };
                foreach (var waveform in waveforms) waveform.Visibility = Visibility.Hidden;
                await Measure("backing playing without waveform");
                foreach (var waveform in waveforms) waveform.Visibility = Visibility.Visible;
                tabs.SelectedIndex = 0; await Measure("editor with mini-player");
                tabs.SelectedIndex = 2;
                window.BackingTracks.PlayPause.Execute(null);
                await Measure("backing paused");
            }
            else throw new InvalidOperationException("No audio output available for playback profiling.");
        }
        finally { window.Close(); }
    }
}
