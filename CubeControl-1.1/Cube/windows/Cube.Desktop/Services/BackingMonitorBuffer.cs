using NAudio.Wave;

namespace Cube.Desktop.Services;

// Playback clock consumes capture frames independently. A small adaptive resampling
// correction keeps monitoring bounded when the two hardware clocks differ.
internal sealed class BackingMonitorBuffer(int sampleRate) : ISampleProvider
{
    private readonly object sync = new();
    private readonly float[] ring = new float[Math.Max(4, sampleRate / 2) * 2];
    private int read, write, frames;
    private double fraction, correction;
    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);
    internal int BufferedFrames { get { lock (sync) return frames; } }
    public void Write(ReadOnlySpan<float> samples)
    {
        lock (sync)
        {
            for (int i = 0; i + 1 < samples.Length; i += 2)
            {
                if (frames == ring.Length / 2) { read = (read + 2) % ring.Length; frames--; }
                ring[write] = samples[i]; ring[write + 1] = samples[i + 1]; write = (write + 2) % ring.Length; frames++;
            }
        }
    }
    public int Read(float[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
    public int Read(Span<float> buffer)
    {
        lock (sync)
        {
            buffer.Clear();
            double target = sampleRate * .04;
            double desired = Math.Clamp((frames - target) / sampleRate * .02, -.005, .005);
            correction += (desired - correction) * .01;
            double step = sampleRate / 48000d * (1 + correction);
            for (int i = 0; i + 1 < buffer.Length && frames >= 2; i += 2)
            {
                int next = (read + 2) % ring.Length;
                buffer[i] = (float)(ring[read] + (ring[next] - ring[read]) * fraction);
                buffer[i + 1] = (float)(ring[read + 1] + (ring[next + 1] - ring[read + 1]) * fraction);
                fraction += step;
                int consumed = Math.Min(frames - 1, (int)fraction);
                read = (read + consumed * 2) % ring.Length; frames -= consumed; fraction -= consumed;
                if (fraction >= 1) fraction = 0;
            }
            return buffer.Length;
        }
    }
}
