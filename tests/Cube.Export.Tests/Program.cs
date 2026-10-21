using System.IO;
using System.Reflection;
using Cube.Desktop.Services;
using NAudio.Wave;

internal static class Program
{
    private static readonly string Root = Path.Combine(Path.GetTempPath(), "CubeExportTests-" + Guid.NewGuid());
    private static readonly MethodInfo ExportMethod = typeof(BackingStemSource).Assembly
        .GetType("Cube.Desktop.Services.BackingSessionAudio")!.GetMethod("Export")!;
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static void Export(BackingStemSource[] sources, string destination, double end = .4, CancellationToken token = default) =>
        ((Task)ExportMethod.Invoke(null, [sources, destination, 0d, end, 1d, 0d, 1d, token])!).GetAwaiter().GetResult();
    private static string Wave(string name)
    {
        string path = Path.Combine(Root, name);
        using var writer = new WaveFileWriter(path, new WaveFormat(48000, 16, 1));
        for (int i = 0; i < 9600; i++) writer.WriteSample((float)(.1 * Math.Sin(2 * Math.PI * 440 * i / 48000)));
        return path;
    }
    private static double Peak(string path)
    {
        using var reader = new WaveFileReader(path);
        var provider = reader.ToSampleProvider();
        var samples = new float[4096]; double peak = 0; int count;
        while ((count = provider.Read(samples.AsSpan())) > 0)
            for (int i = 0; i < count; i++) peak = Math.Max(peak, Math.Abs(samples[i]));
        return peak;
    }
    private static void FailsWithoutReplacing(BackingStemSource[] sources, string label, CancellationToken token = default)
    {
        string destination = Path.Combine(Root, label + ".wav"); byte[] original = [7, 8, 9]; File.WriteAllBytes(destination, original);
        bool failed = false;
        try { Export(sources, destination, token: token); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or OperationCanceledException)
        {
            failed = true;
            if (label is "missing" or "layer" or "locked")
                Check(ex.GetType().Name == "BackingExportSourceException" && ex.Message.Contains(sources.Last().Path), "Missing actionable source error");
        }
        Check(failed, label + " unexpectedly succeeded");
        Check(File.ReadAllBytes(destination).SequenceEqual(original), label + " overwrote the destination");
        Check(!Directory.EnumerateFiles(Root, label + ".wav.*.tmp").Any(), label + " left temporary output");
    }
    private static void Main()
    {
        Directory.CreateDirectory(Root);
        var first = new BackingStemSource(Guid.NewGuid(), Wave("first.wav"), ClipLength: .2);
        var second = new BackingStemSource(Guid.NewGuid(), Wave("second.wav"), ClipLength: .2);
        var missing = new BackingStemSource(Guid.NewGuid(), Path.Combine(Root, "absent.wav"), ClipLength: .2);
        string single = Path.Combine(Root, "single.wav"), mix = Path.Combine(Root, "mix.wav");
        Export([first], single); Export([first, second], mix);
        Check(Peak(mix) > Peak(single) * 1.9, "Mix omitted a source");
        using (var reader = new WaveFileReader(mix)) Check(Math.Abs(reader.TotalTime.TotalSeconds - .4) < .001, "Export truncated trailing silence");
        FailsWithoutReplacing([first, missing], "missing");
        FailsWithoutReplacing([first, missing with { TrackId = first.Id, SourceStart = .01 }], "layer");
        using (var locked = File.Open(second.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            FailsWithoutReplacing([first, second], "locked");
        foreach (var excluded in new[] { missing with { Muted = true }, missing with { Volume = 0 }, missing with { Offset = 2 }, missing with { ClipLength = 0 } })
        {
            Export([first, excluded], mix);
            Check(Math.Abs(Peak(mix) - Peak(single)) < .0001, "Excluded source changed the mix");
        }
        Export([first with { Solo = true }, missing], mix);
        Check(Math.Abs(Peak(mix) - Peak(single)) < .0001, "Solo export changed");
        Export([first with { Muted = true }], mix);
        Check(Peak(mix) == 0, "Muted export is not silent");
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        FailsWithoutReplacing([first], "canceled", canceled.Token);
        string corrupt = Path.Combine(Root, "corrupt.wav"); File.WriteAllText(corrupt, "invalid WAV");
        FailsWithoutReplacing([first, new(Guid.NewGuid(), corrupt, ClipLength: .2)], "corrupt");
        byte[] input = File.ReadAllBytes(first.Path);
        try { Export([first], first.Path); throw new Exception("Source overwrite permitted"); } catch (IOException) { }
        Check(File.ReadAllBytes(first.Path).SequenceEqual(input), "Source audio modified");
        Console.WriteLine("PASS: complete mix, missing stem/layer, locked/corrupt files, mute/solo/range filtering, duration, cancellation, destination/source preservation.");
        Console.WriteLine("Isolated test data: " + Root);
    }
}
