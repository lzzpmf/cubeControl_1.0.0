using System.IO;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Cube.Desktop.Services;

public sealed record BackingStemSource(Guid Id, string Path, double Offset = 0, double RecordedSpeed = 1,
    double RecordedPitch = 0, bool Muted = false, bool Solo = false, double Volume = 1);

// One frame clock for every stem. Decoders always advance, including muted stems.
internal sealed class BackingSessionAudio : ISampleProvider, IDisposable
{
    private sealed class Voice(BackingStemSource source, double length)
    {
        public BackingStemSource Source = source;
        public double Length = length;
        public BackingAudioProcessing? Processing;
        public ISampleProvider? Provider;
        public float Gain;
    }
    private readonly List<Voice> voices = [];
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
                if (!File.Exists(source.Path)) continue;
                if (!double.IsFinite(source.Offset) || !double.IsFinite(source.RecordedSpeed) || source.RecordedSpeed is < .5 or > 1.5
                    || !double.IsFinite(source.RecordedPitch) || Math.Abs(source.RecordedPitch) > 6)
                    throw new InvalidDataException("Invalid stem timing.");
                using var decoder = new BackingDecoder(source.Path);
                voices.Add(new(source, decoder.Duration));
            }
            Duration = voices.Select(v => v.Source.Offset + v.Length * v.Source.RecordedSpeed).DefaultIfEmpty(0).Max();
            this.end = double.IsFinite(end) ? Math.Min(end, Duration) : Duration;
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
            double sourceStart = Math.Max(0, (position - voice.Source.Offset) / voice.Source.RecordedSpeed);
            if (sourceStart >= voice.Length) continue;
            voice.Processing = new(voice.Source.Path, sourceStart, voice.Length,
                speed / voice.Source.RecordedSpeed, pitch - voice.Source.RecordedPitch, relativeParameters: true, offline: offline);
            ISampleProvider provider = voice.Processing;
            if (provider.WaveFormat.Channels == 1) provider = new MonoToStereoSampleProvider(provider);
            if (provider.WaveFormat.Channels != 2) throw new InvalidDataException("Session audio requires mono or stereo files.");
            if (provider.WaveFormat.SampleRate != 48000) provider = new WdlResamplingSampleProvider(provider, 48000);
            voice.Provider = provider;
        }
    }
    public void SetMix(IEnumerable<BackingStemSource> sources)
    {
        var values = sources.ToDictionary(s => s.Id); anySolo = values.Values.Any(s => s.Solo);
        foreach (var voice in voices) if (values.TryGetValue(voice.Source.Id, out var value)) voice.Source = value;
    }
    public int Read(float[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
    public int Read(Span<float> buffer)
    {
        int written = 0, wanted = buffer.Length / 2;
        while (written < wanted)
        {
            double now = segmentStart + frame * speed / 48000;
            long remaining = (long)Math.Round((end - now) * 48000 / speed);
            if (remaining <= 0 && !KeepAlive)
            {
                if (!looping || end <= loopStart) break;
                Begin(loopStart); continue;
            }
            int frames = (int)Math.Min(wanted - written, KeepAlive ? wanted - written : remaining);
            var target = buffer.Slice(written * 2, frames * 2); target.Clear();
            if (scratch.Length < target.Length) scratch = new float[target.Length];
            bool solo = anySolo;
            foreach (var voice in voices)
            {
                int skip = (int)Math.Clamp(Math.Round((voice.Source.Offset - now) * 48000 / speed), 0, frames);
                if (voice.Provider == null || skip == frames) continue;
                int read = voice.Provider.Read(scratch.AsSpan(0, (frames - skip) * 2));
                float desired = voice.Source.Muted || solo && !voice.Source.Solo ? 0 : (float)Math.Clamp(voice.Source.Volume, 0, 1);
                for (int i = 0; i < read; i += 2)
                {
                    voice.Gain += Math.Clamp(desired - voice.Gain, -1f / 240, 1f / 240);
                    target[skip * 2 + i] += scratch[i] * voice.Gain;
                    target[skip * 2 + i + 1] += scratch[i + 1] * voice.Gain;
                }
            }
            frame += frames; written += frames;
        }
        return written * 2;
    }
    public void Dispose() { foreach (var voice in voices) voice.Processing?.Dispose(); }

    public static Task Export(BackingStemSource[] sources, string destination, double start, double end,
        double speed, double pitch, double volume, CancellationToken cancellation) => Task.Run(() =>
    {
        if (!sources.Any(s => File.Exists(s.Path))) throw new FileNotFoundException("No session audio is available for export.");
        string temporary = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var session = new BackingSessionAudio(sources, start, end, speed, pitch, offline: true))
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
