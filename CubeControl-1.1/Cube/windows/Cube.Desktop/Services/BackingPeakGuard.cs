using System.IO;
using NAudio.Wave;

namespace Cube.Desktop.Services;

internal sealed class BackingPeakGuard : ISampleProvider
{
    private readonly ISampleProvider source;
    private readonly float[] ring, block;
    private readonly int channels, window;
    private readonly long[] peakFrames;
    private readonly float[] peakValues;
    private int peakHead, peakCount;
    private long inserted, removed;
    private int blockCount, blockOffset;
    private float gain = 1;
    private readonly float recovery;
    private bool ended;
    public WaveFormat WaveFormat => source.WaveFormat;
    public BackingPeakGuard(ISampleProvider source)
    {
        this.source = source; channels = WaveFormat.Channels;
        window = Math.Max(1, (int)Math.Ceiling(WaveFormat.SampleRate * .005));
        ring = new float[(window + 1) * channels]; block = new float[1024 * channels];
        peakFrames = new long[window + 2]; peakValues = new float[window + 2];
        recovery = (float)(1 - Math.Exp(-1 / (.05 * WaveFormat.SampleRate)));
    }
    private bool Push()
    {
        if (ended) return false;
        if (blockOffset == blockCount) { blockCount = source.Read(block); blockOffset = 0; }
        if (blockCount == 0) { ended = true; return false; }
        int index = (int)(inserted % (window + 1)) * channels; float peak = 0;
        for (int c = 0; c < channels; c++)
        {
            float value = block[blockOffset++];
            if (!float.IsFinite(value)) throw new InvalidDataException("Non-finite DSP sample.");
            ring[index + c] = value; peak = Math.Max(peak, Math.Abs(value));
        }
        while (peakCount > 0 && peakFrames[peakHead] < removed) { peakHead = (peakHead + 1) % peakFrames.Length; peakCount--; }
        while (peakCount > 0 && peakValues[(peakHead + peakCount - 1) % peakFrames.Length] <= peak) peakCount--;
        int tail = (peakHead + peakCount++) % peakFrames.Length;
        peakFrames[tail] = inserted++; peakValues[tail] = peak; return true;
    }
    public int Read(float[] buffer, int offset, int count)
        => Read(buffer.AsSpan(offset, count));
    public int Read(Span<float> buffer)
    {
        int done = 0, count = buffer.Length - buffer.Length % channels;
        while (done < count)
        {
            while (inserted - removed <= window && Push()) { }
            if (inserted == removed) break;
            while (peakCount > 0 && peakFrames[peakHead] < removed) { peakHead = (peakHead + 1) % peakFrames.Length; peakCount--; }
            float peak = peakCount > 0 ? peakValues[peakHead] : 0;
            float target = peak > 1 ? 1 / peak : 1;
            gain = Math.Min(target, gain + (1 - gain) * recovery);
            int index = (int)(removed++ % (window + 1)) * channels;
            for (int c = 0; c < channels; c++) buffer[done++] = ring[index + c] * gain;
        }
        return done;
    }
}
