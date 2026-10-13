using NAudio.Wave;

namespace Cube.Desktop.Services;

// Peak envelope only; samples pass through unchanged and are never retained.
internal sealed class BackingLevelMeter(ISampleProvider source) : ISampleProvider
{
    private readonly AudioPeakAccumulator peak = new();
    public WaveFormat WaveFormat => source.WaveFormat;
    public float ConsumePeak() => peak.Consume();
    public int Read(Span<float> buffer)
    {
        int count = source.Read(buffer); float current = 0;
        for (int i = 0; i < count; i++) current = Math.Max(current, Math.Abs(buffer[i]));
        peak.Add(current);
        return count;
    }
}

// A compare/exchange retry prevents the UI reset from resurrecting an old peak.
internal sealed class AudioPeakAccumulator
{
    private float peak;
    public float Consume() => Interlocked.Exchange(ref peak, 0);
    public void Add(float value)
    {
        if (!float.IsFinite(value) || value <= 0) return;
        float previous = Volatile.Read(ref peak);
        while (value > previous)
        {
            float observed = Interlocked.CompareExchange(ref peak, value, previous);
            if (observed == previous) return;
            previous = observed;
        }
    }
}
