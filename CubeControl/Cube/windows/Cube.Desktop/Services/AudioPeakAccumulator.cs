namespace Cube.Desktop.Services;

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
