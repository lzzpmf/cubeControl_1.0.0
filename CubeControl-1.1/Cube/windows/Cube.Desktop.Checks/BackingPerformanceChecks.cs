using System.Diagnostics;
using System.IO;
using Cube.Desktop.Services;
using Cube.Desktop.ViewModels;
using NAudio.Wave;

internal static class BackingPerformanceChecks
{
    public static void Run()
    {
        string root = Path.Combine(Path.GetTempPath(), "CubeControl-Backing-Performance", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root); string source = Path.Combine(root, "source.wav");
        using (var writer = new WaveFileWriter(source, new WaveFormat(48000, 16, 1)))
        {
            var second = Enumerable.Range(0, 48000).Select(i => (float)(.015 * Math.Sin(i * 2 * Math.PI * 220 / 48000))).ToArray();
            for (int i = 0; i < 75; i++) writer.WriteSamples(second, 0, second.Length);
        }
        var sources = new List<BackingStemSource>();
        for (int track = 0; track < 8; track++)
        {
            int clips = track < 4 ? 13 : 12;
            var stem = new BackingStem { Id = Guid.NewGuid(), Source = source, Volume = 100, LegacyMono = false };
            for (int clip = 0; clip < clips; clip++)
            {
                double duration = 900d / clips;
                var take = new BackingClipTake(Guid.NewGuid(), "Synthetic", [new(Guid.NewGuid(), source, 0, 0, duration)]);
                sources.AddRange(new BackingClip(Guid.NewGuid(), "Clip", clip * duration, duration, [take], take.Id).Sources(stem));
            }
        }
        var process = Process.GetCurrentProcess(); var cpu = process.TotalProcessorTime; var watch = Stopwatch.StartNew();
        Console.WriteLine("Offline benchmark: 8 tracks, 15 minutes, 100 clips. No live-device/underrun claim.");
        string destination = Path.Combine(root, "mix.wav");
        BackingSessionAudio.Export(sources.ToArray(), destination, 0, 900, 1, 0, 1, CancellationToken.None).GetAwaiter().GetResult();
        watch.Stop();
        using var reader = new WaveFileReader(destination);
        if (Math.Abs(reader.TotalTime.TotalSeconds - 900) > 1d / 48000) throw new Exception("Benchmark output duration mismatch.");
        Console.WriteLine($"PASS: 900 seconds exported; wall={watch.Elapsed.TotalSeconds:0.00}s; cpu={(process.TotalProcessorTime - cpu).TotalSeconds:0.00}s; peakWorkingSet={process.PeakWorkingSet64 / 1048576d:0.0} MiB");
        Console.WriteLine("Artifacts: " + root);
    }
}
