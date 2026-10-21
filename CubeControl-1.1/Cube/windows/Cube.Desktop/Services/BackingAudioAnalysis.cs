
namespace Cube.Desktop.Services;

internal static class BackingAudioAnalysis
{
    public static Task<(double Duration, float[] Peaks)> Analyze(string path, CancellationToken cancellation, int maximumBins = 8 * 1024 * 1024) => Task.Run(() =>
    {
        using var source = new BackingDecoder(path);
        int channels = source.Format.Channels, rate = source.Format.SampleRate;
        // A single envelope is capped as well as the aggregate cache.
        double duration = source.Duration;
        double totalFrames = Math.Max(1, duration * rate);
        int bins = (int)Math.Clamp(Math.Ceiling(duration * 100), 1, maximumBins);
        var peaks = new float[bins]; var block = new float[4096 * channels];
        long frame = 0;
        int read;
        while ((read = source.Read(block)) > 0)
        {
            cancellation.ThrowIfCancellationRequested();
            for (int i = 0; i < read; i += channels, frame++)
            {
                float peak = 0;
                for (int c = 0; c < channels; c++) peak = Math.Max(peak, Math.Abs(block[i + c]));
                int bin = (int)Math.Min(bins - 1, frame * bins / totalFrames);
                peaks[bin] = Math.Max(peaks[bin], float.IsFinite(peak) ? peak : 0);
            }
        }
        return (frame / (double)rate, peaks);
    }, cancellation);

}
