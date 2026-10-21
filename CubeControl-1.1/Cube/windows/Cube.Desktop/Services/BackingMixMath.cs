namespace Cube.Desktop.Services;

public static class BackingMixMath
{
    public const double MaximumGain = 1.9952623149688795;
    public static double ToDb(double gain) => gain <= 0 ? -61 : Math.Clamp(20 * Math.Log10(gain), -60, 6);
    public static double FromDb(double db) => db <= -61 ? 0 : Math.Pow(10, Math.Clamp(db, -60, 6) / 20);
    public static (float Left, float Right) Pan(double pan, bool mono, bool legacyCenter = false)
    {
        pan = Math.Clamp(pan, -1, 1);
        if (!mono) return ((float)Math.Min(1, 1 - pan), (float)Math.Min(1, 1 + pan));
        double compensation = legacyCenter ? Math.Sqrt(2) : 1;
        return ((float)(Math.Cos((pan + 1) * Math.PI / 4) * compensation),
            (float)(Math.Sin((pan + 1) * Math.PI / 4) * compensation));
    }
}

// A bounded, sample-clock RMS window. Readers never touch the audio ring.
internal sealed class BackingSignalMeter(int sampleRate = 48000)
{
    private readonly double[] squares = new double[Math.Max(1, sampleRate * 3 / 10)];
    private int cursor;
    private double sum;
    private readonly AudioPeakAccumulator peak = new();
    private float rms;
    public void Add(float left, float right)
    {
        double square = ((double)left * left + (double)right * right) / 2;
        sum += square - squares[cursor]; squares[cursor] = square;
        cursor = (cursor + 1) % squares.Length;
        Volatile.Write(ref rms, (float)Math.Sqrt(Math.Max(0, sum / squares.Length)));
        peak.Add(Math.Max(Math.Abs(left), Math.Abs(right)));
    }
    public (float Peak, float Rms) Consume() => (peak.Consume(), Volatile.Read(ref rms));
}
