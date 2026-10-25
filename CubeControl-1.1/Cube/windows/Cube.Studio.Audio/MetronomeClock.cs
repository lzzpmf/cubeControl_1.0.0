namespace Cube.Studio.Audio;

// Phase belongs to the device output stream, independent of every media renderer/seek/loop.
// The hardware callback only reads an immutable control record and processes preallocated samples.
internal sealed class MetronomeClock
{
    // Normalize the synthesized source to 0 dBFS before the user's gain.
    // Built once outside the audio callback; keep the existing output peak guard
    // for positive gain and sums with backing audio.
    private static readonly double[] sourcePeaks = [
        SourcePeak(2400, 160), SourcePeak(1700, 160),
        SourcePeak(1000, 160), SourcePeak(700, 160),
        SourcePeak(1600, 160), SourcePeak(1100, 160),
        SourcePeak(1800, 90), SourcePeak(1200, 90)];

    private static double SourcePeak(double frequency, double decay)
    {
        double peak = 0;
        for (int i = 0; i <= 11520; i++)
        {
            double age = i / 192000d;
            peak = Math.Max(peak, Math.Abs(Math.Sin(age * frequency * 2 * Math.PI)
                * Math.Min(1, age / .0007) * Math.Exp(-age * decay)));
        }
        return peak;
    }

    private sealed record Control(StudioMetronomeSettings Settings, long Reset);
    private Control control = new(new(), 0);
    private long appliedReset = -1, beat;
    private double phase;
    private bool pulse;
    internal MetronomeClock() => _ = sourcePeaks.Length;

    internal bool IsPulse => Volatile.Read(ref pulse);
    internal StudioMetronomeSettings Settings => Volatile.Read(ref control).Settings;

    internal void Configure(StudioMetronomeSettings settings, bool resetPhase)
    {
        var current = Volatile.Read(ref control);
        long reset = current.Reset + (resetPhase || settings.Enabled && !current.Settings.Enabled ? 1 : 0);
        Volatile.Write(ref control, new(settings, reset));
    }

    internal void Add(Span<float> stereo, int sampleRate)
    {
        var command = Volatile.Read(ref control); var settings = command.Settings;
        if (!settings.Enabled) return;
        if (appliedReset != command.Reset) { phase = 0; beat = 0; appliedReset = command.Reset; }
        bool eighths = settings.Meter == "6/8";
        double increment = settings.Bpm / (60 * sampleRate) * (eighths ? 2 : 1);
        double period = 60 / settings.Bpm / (eighths ? 2 : 1);
        int beats = settings.Meter == "3/4" ? 3 : eighths ? 6 : 4;
        double volume = AudioRenderer.Gain(settings.VolumeDb);
        int soundIndex = settings.Sound switch { "Madeira" => 2, "Bloco" => 4, "Beep" => 6, _ => 0 };
        for (int i = 0; i + 1 < stereo.Length; i += 2)
        {
            double age = phase * period;
            if (age <= .06)
            {
                bool accent = beat % beats == 0;
                double frequency = settings.Sound switch
                {
                    "Madeira" => accent ? 1000 : 700, "Bloco" => accent ? 1600 : 1100,
                    "Beep" => accent ? 1800 : 1200, _ => accent ? 2400 : 1700
                };
                double envelope = Math.Min(1, age / .0007) * Math.Exp(-age * (settings.Sound == "Beep" ? 90 : 160));
                double normalized = Math.Clamp(Math.Sin(age * frequency * 2 * Math.PI) * envelope
                    / sourcePeaks[soundIndex + (accent ? 0 : 1)], -1, 1);
                float click = (float)(volume * normalized);
                stereo[i] += click; stereo[i + 1] += click;
            }
            phase += increment;
            if (phase >= 1) { long elapsed = (long)phase; beat += elapsed; phase -= elapsed; }
        }
        Volatile.Write(ref pulse, phase * period < .08);
    }
}
