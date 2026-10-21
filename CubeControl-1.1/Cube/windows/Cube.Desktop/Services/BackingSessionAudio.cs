using System.IO;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Cube.Desktop.Services;

internal sealed class BackingExportSourceException(string path, Exception inner)
    : IOException("Export source is unavailable: " + path, inner)
{
    public string SourcePath { get; } = path;
}

public sealed record BackingStemSource(Guid Id, string Path, double Offset = 0, double RecordedSpeed = 1,
    double RecordedPitch = 0, bool Muted = false, bool Solo = false, double Volume = 1, double Pan = 0, bool LegacyMono = true,
    Guid TrackId = default, double SourceStart = 0, double ClipLength = double.PositiveInfinity,
    double EnvelopeStart = 0, double EnvelopeLength = double.PositiveInfinity, double FadeIn = 0, double FadeOut = 0,
    bool EqualPowerIn = false, bool EqualPowerOut = false,
    double LayerStart = 0, double LayerLength = double.PositiveInfinity, double LayerFadeIn = 0, double LayerFadeOut = 0,
    bool LayerPowerIn = false, bool LayerPowerOut = false, Cube.Desktop.ViewModels.BackingClipEnvelope[]? Envelopes = null);

// One frame clock for every stem. Decoders always advance, including muted stems.
internal sealed class BackingSessionAudio : ISampleProvider, IDisposable
{
    private sealed class Voice(BackingStemSource source, double length)
    {
        public BackingStemSource Source = source;
        public double Length = length;
        public BackingAudioProcessing? Processing;
        public ISampleProvider? Provider;
        public float Gain, Left = 1, Right = 1;
        public bool Mono;
        public readonly BackingSignalMeter Meter = new();
    }
    private readonly List<Voice> voices = [];
    private readonly Dictionary<Guid, BackingSignalMeter> meters = [];
    private readonly Dictionary<Guid, float[]> sums = [];
    private static Guid Track(BackingStemSource s) => s.TrackId == Guid.Empty ? s.Id : s.TrackId;
    private readonly double speed, pitch, end, loopStart;
    private readonly bool looping, offline;
    private double segmentStart;
    private long frame;
    private bool anySolo;
    private float[] scratch = [];
    public bool KeepAlive { get; set; }
    public double Duration { get; }
    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);

    public BackingSessionAudio(IEnumerable<BackingStemSource> sources, double start, double end,
        double speed, double pitch, bool looping = false, double loopStart = 0, bool offline = false)
    {
        this.speed = speed; this.pitch = pitch; this.looping = looping; this.loopStart = loopStart; this.offline = offline;
        try
        {
            var inputs = sources.ToArray(); anySolo = inputs.Any(s => s.Solo);
            foreach (var source in inputs)
            {
                if (!File.Exists(source.Path))
                {
                    if (offline) throw new BackingExportSourceException(source.Path, new FileNotFoundException(null, source.Path));
                    continue;
                }
                if (!double.IsFinite(source.Offset) || !double.IsFinite(source.RecordedSpeed) || source.RecordedSpeed is < .49 or > 1.53
                    || !double.IsFinite(source.RecordedPitch) || Math.Abs(source.RecordedPitch) > 6)
                    throw new InvalidDataException("Invalid stem timing.");
                using var decoder = new BackingDecoder(source.Path);
                double length = Math.Min(source.ClipLength, Math.Max(0, decoder.Duration - source.SourceStart) * source.RecordedSpeed);
                voices.Add(new(source, length) { Mono = decoder.Format.Channels == 1 });
                meters.TryAdd(Track(source), new());
            }
            Duration = voices.Select(v => v.Source.Offset + v.Length).DefaultIfEmpty(0).Max();
            // Offline export preserves the selected interval, including intentional silence.
            this.end = double.IsFinite(end) ? offline ? end : Math.Min(end, Duration) : Duration;
            Begin(start);
        }
        catch { Dispose(); throw; }
    }
    private void Begin(double position)
    {
        segmentStart = position; frame = 0;
        foreach (var voice in voices)
        {
            voice.Processing?.Dispose(); voice.Processing = null; voice.Provider = null;
            double elapsed = Math.Max(0, position - voice.Source.Offset);
            double sourceStart = voice.Source.SourceStart + elapsed / voice.Source.RecordedSpeed;
            if (elapsed >= voice.Length) continue;
            voice.Processing = new(voice.Source.Path, sourceStart, voice.Source.SourceStart + voice.Length / voice.Source.RecordedSpeed,
                speed / voice.Source.RecordedSpeed, pitch - voice.Source.RecordedPitch, relativeParameters: true, offline: offline);
            ISampleProvider provider = voice.Processing;
            voice.Mono = provider.WaveFormat.Channels == 1;
            if (provider.WaveFormat.Channels == 1) provider = new MonoToStereoSampleProvider(provider);
            if (provider.WaveFormat.Channels != 2) throw new InvalidDataException("Session audio requires mono or stereo files.");
            if (provider.WaveFormat.SampleRate != 48000) provider = new WdlResamplingSampleProvider(provider, 48000);
            voice.Provider = provider;
        }
    }
    public void SetMix(IEnumerable<BackingStemSource> sources)
    {
        var values = sources.GroupBy(s => Track(s)).ToDictionary(g => g.Key, g => g.First()); anySolo = values.Values.Any(s => s.Solo);
        foreach (var voice in voices) if (values.TryGetValue(Track(voice.Source), out var value))
            voice.Source = voice.Source with { Volume = value.Volume, Pan = value.Pan, Muted = value.Muted, Solo = value.Solo };
    }
    public int Read(float[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
    public int Read(Span<float> buffer)
    {
        int written = 0, wanted = buffer.Length / 2;
        while (written < wanted)
        {
            double now = segmentStart + frame * speed / 48000;
            long remaining = (long)Math.Round((end - now) * 48000 / speed);
            if (remaining <= 0 && (!KeepAlive || looping))
            {
                if (!looping || end <= loopStart) break;
                Begin(loopStart); continue;
            }
            int frames = (int)Math.Min(wanted - written, KeepAlive && !looping ? wanted - written : remaining);
            var target = buffer.Slice(written * 2, frames * 2); target.Clear();
            if (scratch.Length < target.Length) scratch = new float[target.Length];
            bool solo = anySolo;
            foreach (var id in meters.Keys)
            {
                if (!sums.TryGetValue(id, out var sum) || sum.Length < target.Length) sums[id] = sum = new float[target.Length];
                sum.AsSpan(0, target.Length).Clear();
            }
            foreach (var voice in voices)
            {
                int skip = (int)Math.Clamp(Math.Round((voice.Source.Offset - now) * 48000 / speed), 0, frames);
                if (voice.Provider == null || skip == frames) continue;
                int read = voice.Provider.Read(scratch.AsSpan(0, (frames - skip) * 2));
                var sum = sums[Track(voice.Source)];
                for (int i = 0; i < read; i += 2)
                {
                    double t = now + (skip + i / 2) * speed / 48000;
                    var s = voice.Source;
                    double envelope = Cube.Desktop.ViewModels.BackingClipEdit.Envelope(t - s.EnvelopeStart, s.EnvelopeLength, s.FadeIn, s.FadeOut, s.EqualPowerIn, s.EqualPowerOut)
                        * Cube.Desktop.ViewModels.BackingClipEdit.Envelope(t - s.LayerStart, s.LayerLength, s.LayerFadeIn, s.LayerFadeOut, s.LayerPowerIn, s.LayerPowerOut);
                    foreach (var curve in s.Envelopes ?? []) envelope *= Cube.Desktop.ViewModels.BackingClipEdit.Envelope(t - curve.Position, curve.Duration, curve.FadeIn, curve.FadeOut, curve.PowerIn, curve.PowerOut);
                    sum[skip * 2 + i] += scratch[i] * (float)envelope;
                    sum[skip * 2 + i + 1] += scratch[i + 1] * (float)envelope;
                }
            }
            foreach (var group in voices.GroupBy(v => Track(v.Source)))
            {
                var voice = group.First(); var source = voice.Source;
                float desired = source.Muted || solo && !source.Solo ? 0 : (float)Math.Clamp(source.Volume, 0, BackingMixMath.MaximumGain);
                var pan = BackingMixMath.Pan(source.Pan, group.All(v => v.Mono), source.LegacyMono);
                var sum = sums[group.Key]; var meter = meters[group.Key];
                for (int i = 0; i < target.Length; i += 2)
                {
                    voice.Gain += Math.Clamp(desired - voice.Gain, -1f / 240, 1f / 240);
                    voice.Left += Math.Clamp(pan.Left - voice.Left, -1f / 240, 1f / 240);
                    voice.Right += Math.Clamp(pan.Right - voice.Right, -1f / 240, 1f / 240);
                    float left = sum[i] * voice.Gain * voice.Left, right = sum[i + 1] * voice.Gain * voice.Right;
                    meter.Add(left, right); target[i] += left; target[i + 1] += right;
                }
            }
            frame += frames; written += frames;
        }
        return written * 2;
    }
    internal Dictionary<Guid, BackingSignalMeter> GetMeters() => meters;
    public void Dispose() { foreach (var voice in voices) voice.Processing?.Dispose(); }

    public static Task Export(BackingStemSource[] sources, string destination, double start, double end,
        double speed, double pitch, double volume, CancellationToken cancellation) => Task.Run(() =>
    {
        cancellation.ThrowIfCancellationRequested();
        if (!double.IsFinite(start) || !double.IsFinite(end) || start < 0 || end <= start
            || !double.IsFinite(speed) || speed <= 0 || !double.IsFinite(pitch) || !double.IsFinite(volume))
            throw new ArgumentException("Invalid export range or parameters.");
        if (sources.Any(s => string.Equals(System.IO.Path.GetFullPath(s.Path), System.IO.Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase)))
            throw new IOException("Export cannot overwrite source audio.");
        bool anySolo = sources.Any(s => s.Solo);
        var required = sources.Where(s => s.ClipLength > 0 && s.Offset < end && s.Offset + s.ClipLength > start
            && !s.Muted && (!anySolo || s.Solo) && s.Volume > 0).ToArray();
        if (sources.Length == 0) throw new InvalidDataException("No session audio is available for export.");
        // Validate every contributing clip/layer before creating the temporary output.
        // Do not use File.Exists alone: access and sharing failures must also block export.
        foreach (string path in required.Select(s => s.Path).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            cancellation.ThrowIfCancellationRequested();
            try { using var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            { throw new BackingExportSourceException(path, ex); }
        }
        string temporary = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var session = new BackingSessionAudio(required, start, end, speed, pitch, offline: true))
            using (var writer = new WaveFileWriter(temporary, new WaveFormat(48000, 16, 2)))
            {
                var samples = new float[4096]; var pcm = new byte[samples.Length * 2];
                int read;
                while ((read = session.Read(samples, 0, samples.Length)) > 0)
                {
                    cancellation.ThrowIfCancellationRequested();
                    for (int i = 0; i < read; i++)
                    {
                        short sample = (short)Math.Round(Math.Clamp(samples[i] * volume, -1, 1) * short.MaxValue);
                        pcm[i * 2] = (byte)sample; pcm[i * 2 + 1] = (byte)(sample >> 8);
                    }
                    writer.Write(pcm, 0, read * 2);
                }
            }
            cancellation.ThrowIfCancellationRequested(); File.Move(temporary, destination, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }, cancellation);
}
