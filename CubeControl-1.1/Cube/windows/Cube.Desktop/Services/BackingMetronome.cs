using NAudio.Wave;

namespace Cube.Desktop.Services;

public sealed record MetronomeSettings(bool Enabled = false, double Bpm = 120, int Beats = 4,
    bool EighthNotes = false, bool Accent = true, double Volume = 1, double Offset = 0, int CountInBars = 0, string Sound = "Click", MetronomeSoundBuffer? CustomSound = null)
{
    public double Interval => 60 / Bpm / (EighthNotes ? 2 : 1);
    public bool Valid => double.IsFinite(Bpm) && Bpm >= 20 && Bpm <= 300 && Beats is 2 or 3 or 4 or 6
        && double.IsFinite(Offset) && double.IsFinite(Volume) && Volume >= 0 && Volume <= BackingMixMath.MaximumGain && CountInBars is >= 0 and <= 2;
    public int BeatAt(double position)
    {
        if (!Valid) return 0;
        long beat = (long)Math.Floor((position - Offset) / Interval + 1e-8);
        return (int)((beat % Beats + Beats) % Beats) + 1;
    }
}

/// <summary>Stateless synthesis on the source timeline: no timer, rounded beat accumulation or drift.</summary>
public static class MetronomeClick
{
    // Normalize each timbre and accent independently, once, outside per-sample synthesis.
    private static readonly Dictionary<(string Sound, bool Accent), MetronomeSoundBuffer> sounds =
        new[] { "Click", "Bell", "Ping", "Woodblock", "Hi-hat" }
            .SelectMany(sound => new[] { false, true }.Select(accent => (sound, accent)))
            .ToDictionary(key => key, key => CreateSound(key.sound, key.accent));

    public static void Prepare() => _ = sounds.Count;

    public static float Sample(MetronomeSettings settings, double sourceSeconds, double speed = 1)
    {
        if (!settings.Enabled || !settings.Valid) return 0;
        double beat = (sourceSeconds - settings.Offset) / settings.Interval;
        long index = (long)Math.Floor(beat + 1e-8);
        double age = Math.Max(0, (beat - index) * settings.Interval / speed);
        bool accent = settings.Accent && (index % settings.Beats + settings.Beats) % settings.Beats == 0;
        if (settings.CustomSound is { } custom)
        {
            double interval = settings.Interval / speed;
            float sample = 0;
            for (int previous = 0; age + previous * interval < custom.Duration; previous++)
            {
                sample += custom.Sample(age + previous * interval);

            }
            return Math.Clamp(sample, -1, 1) * (float)settings.Volume;
        }
        string sound = settings.Sound is "Bell" or "Ping" or "Woodblock" or "Hi-hat" ? settings.Sound : "Click";
        return sounds[(sound, accent)].Sample(age) * (float)settings.Volume;
    }

    private static MetronomeSoundBuffer CreateSound(string sound, bool accent)
    {
        double duration = sound switch { "Bell" => .18, "Ping" => .12, "Hi-hat" => .045, _ => .025 };
        var samples = new float[(int)Math.Ceiling(duration * 48000) + 1];
        double frequency = accent ? 1800 : 1100;
        for (int i = 0; i < samples.Length; i++)
        {
            double age = i / 48000d;
            double signal = sound switch
            {
                "Bell" => Math.Sin(2 * Math.PI * frequency * age) + .35 * Math.Sin(2 * Math.PI * frequency * 2.76 * age),
                "Ping" => Math.Sin(2 * Math.PI * (accent ? 2400 : 1600) * age),
                "Woodblock" => Math.Sin(2 * Math.PI * (accent ? 800 : 600) * age) * Math.Cos(2 * Math.PI * 170 * age),
                "Hi-hat" => (Math.Sin(2 * Math.PI * 7319 * age) + Math.Sin(2 * Math.PI * 10427 * age) + Math.Sin(2 * Math.PI * 13781 * age)) / 3,
                _ => Math.Sin(2 * Math.PI * frequency * age)
            };
            samples[i] = (float)(signal * Math.Exp(-age * (sound is "Bell" or "Ping" ? 35 : 180))
                * Math.Min(1, age / .001) * Math.Clamp((duration - age) / .002, 0, 1));
        }
        return MetronomeSoundBuffer.Normalized(samples, 48000);
    }

    public static double SourcePosition(long frame, int sampleRate, double start, double speed,
        bool looping, double loopStart, double loopEnd)
    {
        if (!looping) return start + frame * speed / sampleRate;
        long first = (long)Math.Round((loopEnd - start) * sampleRate / speed);
        long cycle = Math.Max(1, (long)Math.Round((loopEnd - loopStart) * sampleRate / speed));
        return frame < first ? start + frame * speed / sampleRate : loopStart + (frame - first) % cycle * speed / sampleRate;
    }
}

public sealed class MetronomeSampleProvider : ISampleProvider
{
    private MetronomeSettings settings;
    private long frames;
    public double Seconds => Interlocked.Read(ref frames) / 48000d;
    private int muted;
    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);
    public MetronomeSampleProvider(MetronomeSettings settings) { MetronomeClick.Prepare(); this.settings = settings; }
    public void Configure(MetronomeSettings value, bool mute) { Volatile.Write(ref settings, value); Volatile.Write(ref muted, mute ? 1 : 0); }
    public int Read(float[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
    public int Read(Span<float> buffer)
    {
        var current = Volatile.Read(ref settings);
        bool silent = Volatile.Read(ref muted) != 0;
        int count = buffer.Length - buffer.Length % 2;
        for (int i = 0; i < count; i += 2)
        {
            float click = silent ? 0 : MetronomeClick.Sample(current, frames / 48000d);
            buffer[i] = buffer[i + 1] = click; frames++;
        }
        return count;
    }
}

public sealed class MetronomeTapTempo
{
    private readonly Queue<double> intervals = new();
    private double? previous;
    public double? Tap(double seconds)
    {
        if (!double.IsFinite(seconds)) return null;
        double interval = previous.HasValue ? seconds - previous.Value : 0;
        previous = seconds;
        if (interval <= 0 || interval > 2) { intervals.Clear(); return null; }
        intervals.Enqueue(interval); if (intervals.Count > 5) intervals.Dequeue();
        return Math.Clamp(60 / intervals.Average(), 20, 300);
    }
}

/// <summary>Immutable decoded mono click; loading never runs in the audio callback.</summary>
public sealed class MetronomeSoundBuffer
{
    private readonly float[] samples;
    private readonly int sampleRate;
    private MetronomeSoundBuffer(float[] samples, int rate) { this.samples = samples; sampleRate = rate; }
    internal static MetronomeSoundBuffer Normalized(float[] samples, int rate)
    {
        float peak = 0;
        foreach (float sample in samples) peak = Math.Max(peak, Math.Abs(sample));
        if (peak > 0)
            for (int i = 0; i < samples.Length; i++) samples[i] /= peak;
        return new(samples, rate);
    }
    public double Duration => samples.Length / (double)sampleRate;
    public float Sample(double age)
    {
        double position = age * sampleRate;
        if (position < 0 || position >= samples.Length - 1) return 0;
        int index = (int)position;
        return samples[index] + (samples[index + 1] - samples[index]) * (float)(position - index);
    }
    public static MetronomeSoundBuffer Load(string path)
    {
        using var reader = new WaveFileReader(path);
        var format = reader.WaveFormat;
        if (format.Channels is < 1 or > 2 || reader.TotalTime.TotalSeconds is <= 0 or > 2 ||
            format.Encoding is not (WaveFormatEncoding.Pcm or WaveFormatEncoding.IeeeFloat))
            throw new System.IO.InvalidDataException("WAV must be mono/stereo PCM or float, up to two seconds.");
        var source = reader.ToSampleProvider();
        var raw = new float[checked((int)(reader.Length / format.BlockAlign) * format.Channels)];
        int count = 0, read;
        while (count < raw.Length && (read = source.Read(raw.AsSpan(count))) > 0) count += read;
        var mono = new float[count / format.Channels];
        if (mono.Length < 2) throw new System.IO.InvalidDataException("Empty WAV.");
        for (int i = 0; i < mono.Length; i++)
        {
            float value = 0;
            for (int c = 0; c < format.Channels; c++) value += raw[i * format.Channels + c] / format.Channels;
            if (!float.IsFinite(value)) throw new System.IO.InvalidDataException("Invalid WAV samples.");
            mono[i] = value * (float)Math.Min(1, Math.Min(i, mono.Length - 1 - i) / Math.Max(1d, format.SampleRate * .002));
        }
        return Normalized(mono, format.SampleRate);
    }
}
