namespace Cube.Desktop.Services;

public sealed record BackingMeterReading(double LevelDb, double PeakDb, bool Clipping)
{
    public static BackingMeterReading Silent { get; } = new(-60, -60, false);
}

// Presentation ballistics only; the audio stream is never modified.
internal sealed class BackingMeterEnvelope
{
    private double lastTime, heldUntil, clipUntil;
    private BackingMeterReading reading = BackingMeterReading.Silent;
    internal BackingMeterReading Update(float amplitude, bool active, double now)
    {
        double elapsed = Math.Max(0, now - lastTime);
        lastTime = now;
        if (!active)
        {
            heldUntil = clipUntil = 0;
            return reading = BackingMeterReading.Silent;
        }
        double db = amplitude > 0 && float.IsFinite(amplitude) ? Math.Max(-60, 20 * Math.Log10(amplitude)) : -60;
        double level = Math.Max(db, reading.LevelDb - 24 * elapsed);
        double peak = reading.PeakDb;
        if (db >= peak) { peak = db; heldUntil = now + 1; }
        else if (now > heldUntil) peak = Math.Max(level, peak - 24 * Math.Min(elapsed, now - heldUntil));
        if (amplitude >= 1) clipUntil = now + 1;
        return reading = new(level, peak, now < clipUntil);
    }
}
