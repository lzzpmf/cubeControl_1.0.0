using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Cube.Desktop;
using Cube.Desktop.Controls;
using Cube.Desktop.ViewModels;
using Cube.Protocol;

internal static class ScrollProfileTests
{
    public static async Task Run()
    {
        using var fake = new FakeConnection(); using var session = new CubeSession(fake); await session.Identify();
        using var model = new EditorViewModel(Path.Combine(Path.GetTempPath(), "cube-scroll-profile-" + Guid.NewGuid().ToString("N")));
        await model.Execute(() => model.AttachSession(session));
        var window = new MainWindow(model, initializeDevices: false) { Width = 1280, Height = 720, ShowInTaskbar = false };
        window.Show(); await Task.Delay(300); window.UpdateLayout();
        var scroll = (ScrollViewer)window.FindName("WorkspaceScroll");
        var intervals = new List<double>();
        TimeSpan? previous = null;
        void Frame(object? sender, EventArgs args)
        {
            if (!SmoothScroll.IsRunning(scroll) || args is not RenderingEventArgs frame) { previous = null; return; }
            if (previous == frame.RenderingTime) return;
            if (previous is { } last) intervals.Add((frame.RenderingTime - last).TotalMilliseconds);
            previous = frame.RenderingTime;
        }
        CompositionTarget.Rendering += Frame;
        try
        {
            var time = Stopwatch.StartNew();
            while (time.ElapsedMilliseconds < 3000)
            {
                bool down = (int)(time.ElapsedMilliseconds / 600) % 2 == 0;
                SmoothScroll.Wheel(scroll, down ? -120 : 120);
                await Task.Delay(45);
            }
            await Task.Delay(300);
            if (SmoothScroll.IsRunning(scroll) || fake.Writes != 0) throw new Exception("Scroll profile left work active or sent MIDI");
            intervals.Sort();
            double Percentile(double p) => intervals.Count == 0 ? 0 : intervals[(int)((intervals.Count - 1) * p)];
            var report = new { Width = window.ActualWidth, Height = window.ActualHeight, RenderTier = RenderCapability.Tier >> 16,
                MotionAllowed = SurfaceMotion.Allowed, Samples = intervals.Count, MedianFrameMilliseconds = Percentile(.5),
                P95FrameMilliseconds = Percentile(.95), Note = "WPF Rendering callback intervals in this run, not display-present FPS or a hardware guarantee." };
            Directory.CreateDirectory(TestPaths.Report("C:/CubeControl-Build/reports/interaction"));
            File.WriteAllText(TestPaths.Report("C:/CubeControl-Build/reports/interaction/scroll-profile.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine(JsonSerializer.Serialize(report));
        }
        finally { CompositionTarget.Rendering -= Frame; window.Close(); }
    }
}
