using NAudio.Wave;

namespace Cube.Desktop.Services;

// Peak envelope only; samples pass through unchanged and are never retained.
internal sealed class BackingLevelMeter(ISampleProvider source) : ISampleProvider
{
    private float peak;
    public WaveFormat WaveFormat => source.WaveFormat;
    public float ConsumePeak() => Interlocked.Exchange(ref peak, 0);
    public int Read(Span<float> buffer)
    {
        int count = source.Read(buffer); float current = 0;
        for (int i = 0; i < count; i++) current = Math.Max(current, Math.Abs(buffer[i]));
        Volatile.Write(ref peak, Math.Max(Volatile.Read(ref peak), current));
        return count;
    }
}
