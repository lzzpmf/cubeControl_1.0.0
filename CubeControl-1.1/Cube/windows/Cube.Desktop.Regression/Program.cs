using System.IO;
using Cube.Desktop.ViewModels;
using NAudio.Wave;

internal static class Program
{
    [STAThread]
    private static void Main() => RunAsync().GetAwaiter().GetResult();

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static async Task RunAsync()
    {
        int executions = 0;
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var command = new AsyncUiCommand(async () => { executions++; await release.Task; }, () => true,
            ex => throw new InvalidOperationException("Unexpected command failure", ex));
        Task first = command.ExecuteAsync();
        await command.ExecuteAsync();
        Assert(executions == 1 && !command.CanExecute(null), "Command allowed concurrent execution");
        release.SetResult();
        await first;
        Assert(command.CanExecute(null), "Command stayed disabled after completion");

        Exception? reported = null;
        var failure = new IOException("Expected failure");
        command = new(() => Task.FromException(failure), () => true, ex => reported = ex);
        await command.ExecuteAsync();
        Assert(ReferenceEquals(reported, failure) && command.CanExecute(null), "Command did not report/recover from failure");
        command = new(() => { executions++; return Task.CompletedTask; }, () => false, _ => { });
        await command.ExecuteAsync();
        Assert(executions == 1, "Disabled command executed");

        string directory = Path.Combine(Path.GetTempPath(), "cube-regression-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string source = Path.Combine(directory, "source.wav"), target = Path.Combine(directory, "export.wav");
            using (var writer = new WaveFileWriter(source, new WaveFormat(44100, 16, 2)))
                for (int i = 0; i < 44100; i++)
                {
                    float value = (float)(.5 * Math.Sin(2 * Math.PI * 440 * i / 44100));
                    writer.WriteSample(value); writer.WriteSample(value);
                }
            // Exercise the existing facade, including its internal collaborators.
            var type = typeof(EditorViewModel).Assembly.GetType("Cube.Desktop.Services.BackingAudioProcessing", true)!;
            var analysis = (Task<(double Duration, float[] Peaks)>)type.GetMethod("Analyze")!
                .Invoke(null, [source, CancellationToken.None, 1000])!;
            var result = await analysis;
            Assert(Math.Abs(result.Duration - 1) < .001 && result.Peaks.Length == 100, "Analysis duration/bin count changed");
            Assert(result.Peaks.All(peak => peak > .49f && peak <= .501f), "Analysis peak amplitude changed");

            foreach (bool processed in new[] { false, true })
            {
                await (Task)type.GetMethod("Export")!.Invoke(null,
                    [source, target, .2, .7, 1d, 0d, processed, CancellationToken.None])!;
                using var exported = new WaveFileReader(target);
                Assert(Math.Abs(exported.TotalTime.TotalSeconds - .5) < .001, "Exported range changed");
                Assert(exported.WaveFormat.Channels == 2 && exported.WaveFormat.BitsPerSample == 16, "Exported PCM format changed");
            }
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            byte[] original = File.ReadAllBytes(target);
            try
            {
                await (Task)type.GetMethod("Export")!.Invoke(null,
                    [source, target, 0d, 1d, 1d, 0d, false, cancelled.Token])!;
                throw new InvalidOperationException("Cancelled export completed");
            }
            catch (OperationCanceledException) { }
            Assert(original.SequenceEqual(File.ReadAllBytes(target)), "Cancelled export replaced destination");
            Assert(Directory.GetFiles(directory).Length == 2, "Export left temporary files");
        }
        finally { Directory.Delete(directory, recursive: true); }
        Console.WriteLine("PASS: commands, PCM analysis, raw/processed export, cancellation and cleanup");
    }
}
