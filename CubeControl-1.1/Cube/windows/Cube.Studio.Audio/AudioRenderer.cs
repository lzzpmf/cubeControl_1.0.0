using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Cube.Studio.Audio;

internal sealed class AudioRenderer : IDisposable
{
    private sealed class Voice(StudioTrack track, StudioClip clip, PreparedMedia media)
    {
        internal StudioTrack Track = track;
        internal readonly StudioClip Clip = clip;
        internal readonly PreparedMedia Media = media;
        internal DspAudio? Audio;
        internal ISampleProvider? Provider;
        internal double Gain = AudioRenderer.Gain(track.VolumeDb);
        internal double Pan = track.Pan;
        internal MemorySamples? LoopAudio;
        internal double LoopAudioStart;
        internal readonly StudioClip[] Peers = track.Clips.Where(other => other.Start < clip.Start + clip.Duration && other.Start + other.Duration > clip.Start).ToArray();
    }
    private StudioSnapshot snapshot;
    private readonly Voice[] voices;
    private readonly int rate;
    private readonly double end;
    private readonly bool loop;
    private readonly MetronomeClock? offlineMetronome;
    private bool anySolo;
    private readonly float[] scratch = new float[2048];
    private double segmentStart;
    private long frame;
    private readonly StereoPeakGuard peakGuard;
    internal double Position => segmentStart + frame * snapshot.Speed / rate;
    internal long LoopPass { get; private set; }

    internal void UpdateMix(StudioSnapshot value)
    {
        snapshot = value;
        ConfigureOfflineMetronome();
        anySolo = value.Tracks.Any(t => t.Solo);
        foreach (var voice in voices)
            if (value.Tracks.FirstOrDefault(t => t.Id == voice.Track.Id) is { } track) voice.Track = track;
    }

    internal AudioRenderer(StudioSnapshot snapshot, IReadOnlyDictionary<string, PreparedMedia> media,
        double start, double end, int sampleRate, bool loop, bool metronome)
    {
        this.snapshot = snapshot; rate = sampleRate; this.end = end; this.loop = loop;
        if (metronome) { offlineMetronome = new(); ConfigureOfflineMetronome(); }
        peakGuard = new(sampleRate);
        anySolo = snapshot.Tracks.Any(t => t.Solo);
        voices = snapshot.Tracks.SelectMany(track => track.Clips.Select(clip => new Voice(track, clip, media[clip.Path]))).ToArray();
        foreach (var voice in voices) if (voice.Track.Muted || anySolo && !voice.Track.Solo) voice.Gain = 0;
        try { PrepareShortLoop(); Begin(start); } catch { Dispose(); throw; }
    }

    private void PrepareShortLoop()
    {
        // Short loops must not repeatedly open files or rebuild FFT state on the render worker.
        // Cache each voice before pan, fades and gain, allowing live mixing to keep working.
        if (!loop || end - snapshot.LoopStart > 2) return;
        long budget = 8 * 1024 * 1024;
        foreach (var voice in voices)
        {
            double start = Math.Max(snapshot.LoopStart, voice.Clip.Start), finish = Math.Min(end, voice.Clip.Start + voice.Clip.Duration);
            int count = (int)Math.Max(0, Math.Round((finish - start) * rate / snapshot.Speed)) * 2;
            if (count <= 0 || count > budget) continue;
            budget -= count;
            var clip = voice.Clip; double sourceRate = clip.RecordedSpeed * clip.ClockRatio;
            using var source = new DspAudio(voice.Media.PcmPath, clip.SourceOffset + (start - clip.Start) / sourceRate,
                clip.SourceOffset + (finish - clip.Start) / sourceRate, snapshot.Speed / sourceRate, snapshot.Semitones - clip.RecordedPitch);
            ISampleProvider provider = source;
            if (provider.WaveFormat.Channels == 1) provider = new MonoToStereoSampleProvider(provider);
            if (provider.WaveFormat.SampleRate != rate) provider = new WdlResamplingSampleProvider(provider, rate);
            var samples = new float[count]; int copied = 0;
            while (copied < count) { int read = provider.Read(samples.AsSpan(copied)); if (read <= 0) break; copied += read; }
            voice.LoopAudio = new(samples, rate); voice.LoopAudioStart = start;
        }
    }

    private void Begin(double position)
    {
        segmentStart = position; frame = 0;
        foreach (var voice in voices)
        {
            voice.Audio?.Dispose(); voice.Audio = null; voice.Provider = null;
            var clip = voice.Clip;
            if (position >= clip.Start + clip.Duration) continue;
            if (voice.LoopAudio != null && position >= snapshot.LoopStart)
            {
                voice.LoopAudio.Seek((int)Math.Max(0, Math.Round((Math.Max(position, clip.Start) - voice.LoopAudioStart) * rate / snapshot.Speed)) * 2);
                voice.Provider = voice.LoopAudio; continue;
            }
            double sourceRate = clip.RecordedSpeed * clip.ClockRatio;
            double elapsed = Math.Max(0, position - clip.Start);
            voice.Audio = new(voice.Media.PcmPath, clip.SourceOffset + elapsed / sourceRate,
                clip.SourceOffset + clip.Duration / sourceRate, snapshot.Speed / sourceRate, snapshot.Semitones - clip.RecordedPitch);
            ISampleProvider provider = voice.Audio;
            if (provider.WaveFormat.Channels == 1) provider = new MonoToStereoSampleProvider(provider);
            if (provider.WaveFormat.SampleRate != rate) provider = new WdlResamplingSampleProvider(provider, rate);
            voice.Provider = provider;
        }
    }

    internal int Read(Span<float> output)
    {
        int written = 0, wanted = output.Length / 2;
        while (written < wanted)
        {
            double now = Position;
            long remaining = (long)Math.Round((end - now) * rate / snapshot.Speed);
            if (remaining <= 0)
            {
                if (!loop || end - snapshot.LoopStart < .001) break;
                LoopPass++; Begin(snapshot.LoopStart); continue;
            }
            int frames = (int)Math.Min(Math.Min(wanted - written, 1024), remaining);
            var target = output.Slice(written * 2, frames * 2); target.Clear();
            foreach (var voice in voices)
            {
                var clip = voice.Clip; var track = voice.Track;
                int skip = (int)Math.Clamp(Math.Round((clip.Start - now) * rate / snapshot.Speed), 0, frames);
                if (voice.Provider == null || skip >= frames) continue;
                int requested = (int)Math.Min(frames - skip, Math.Max(0, Math.Round((clip.Start + clip.Duration - now) * rate / snapshot.Speed) - skip));
                if (requested <= 0) continue;
                int read = voice.Provider.Read(scratch.AsSpan(0, requested * 2));
                double desiredGain = track.Muted || anySolo && !track.Solo ? 0 : Gain(track.VolumeDb);
                for (int i = 0; i + 1 < read; i += 2)
                {
                    voice.Gain += Math.Clamp(desiredGain - voice.Gain, -1d / 240, 1d / 240);
                    voice.Pan += Math.Clamp(track.Pan - voice.Pan, -1d / 240, 1d / 240);
                    var pan = Pan(voice.Pan, voice.Media.Channels == 1);
                    double time = now + (skip + i / 2) * snapshot.Speed / rate;
                    double envelope = Envelope(voice.Peers, clip, time);
                    target[skip * 2 + i] += (float)(scratch[i] * voice.Gain * pan.Left * envelope);
                    target[skip * 2 + i + 1] += (float)(scratch[i + 1] * voice.Gain * pan.Right * envelope);
                }
            }
            offlineMetronome?.Add(target, rate);
            for (int i = 0; i < frames; i++)
            {
                float left = target[i * 2], right = target[i * 2 + 1];
                peakGuard.Process(ref left, ref right);
                target[i * 2] = left; target[i * 2 + 1] = right;
            }
            frame += frames; written += frames;
        }
        return written * 2;
    }

    internal static double Gain(double db) => db <= -61 ? 0 : Math.Pow(10, Math.Clamp(db, -60, 6) / 20);
    private static (double Left, double Right) Pan(double value, bool mono)
    {
        value = Math.Clamp(value, -1, 1);
        return mono ? (Math.Cos((value + 1) * Math.PI / 4), Math.Sin((value + 1) * Math.PI / 4))
            : (Math.Min(1, 1 - value), Math.Min(1, 1 + value));
    }

    private static double Envelope(StudioClip[] peers, StudioClip clip, double time)
    {
        double relative = time - clip.Start, gain = 1;
        if (clip.FadeIn > 0) gain *= Math.Clamp(relative / clip.FadeIn, 0, 1);
        if (clip.FadeOut > 0) gain *= Math.Clamp((clip.Duration - relative) / clip.FadeOut, 0, 1);
        if (peers.Length < 2) return gain;
        double weight = OverlapWeight(peers, clip, time), sumSquares = 0;
        foreach (var candidate in peers)
            if (time >= candidate.Start && time < candidate.Start + candidate.Duration)
            { double value = OverlapWeight(peers, candidate, time); sumSquares += value * value; }
        return gain * (sumSquares > 1e-20 ? weight / Math.Sqrt(sumSquares) : 0);
    }

    private static double OverlapWeight(StudioClip[] peers, StudioClip clip, double time)
    {
        double weight = 1, clipEnd = clip.Start + clip.Duration;
        foreach (var other in peers)
        {
            if (other.Id == clip.Id) continue;
            double otherEnd = other.Start + other.Duration;
            double start = Math.Max(clip.Start, other.Start), end = Math.Min(clipEnd, otherEnd);
            if (end <= start || time < start || time >= end) continue;
            if (clip.Start == other.Start && clipEnd == otherEnd) continue;
            double fraction = Math.Clamp((time - start) / (end - start), 0, 1);
            bool contains = clip.Start <= other.Start && clipEnd >= otherEnd;
            bool contained = other.Start <= clip.Start && otherEnd >= clipEnd;
            if ((contains || contained) && clip.Start != other.Start && clipEnd != otherEnd)
                weight *= contains ? Math.Abs(Math.Cos(fraction * Math.PI)) : Math.Sin(fraction * Math.PI);
            else
            {
                bool outgoing = clip.Start < other.Start || clip.Start == other.Start && clipEnd < otherEnd;
                weight *= outgoing ? Math.Cos(fraction * Math.PI / 2) : Math.Sin(fraction * Math.PI / 2);
            }
        }
        return weight;
    }

    private void ConfigureOfflineMetronome() => offlineMetronome?.Configure(new()
    { Enabled = snapshot.MetronomeEnabled, Bpm = snapshot.Bpm, Meter = snapshot.Meter, Sound = snapshot.MetronomeSound, VolumeDb = snapshot.MetronomeVolumeDb }, false);
    public void Dispose() { foreach (var voice in voices) voice.Audio?.Dispose(); }
}

internal sealed class MemorySamples(float[] samples, int sampleRate) : ISampleProvider
{
    private int position;
    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2);
    internal void Seek(int sample) => position = Math.Clamp(sample, 0, samples.Length);
    public int Read(Span<float> destination)
    {
        int count = Math.Min(destination.Length, samples.Length - position);
        samples.AsSpan(position, count).CopyTo(destination); position += count; return count;
    }
}

// A stereo-linked soft knee preserves the image and releases gain over 50 ms.
// It adds no lookahead delay, so live and offline reference timelines remain identical.
internal sealed class StereoPeakGuard(int sampleRate)
{
    private readonly double release = Math.Exp(-1d / (.05 * sampleRate));
    private double gain = 1;
    internal void Process(ref float left, ref float right)
    {
        if (!float.IsFinite(left)) left = 0;
        if (!float.IsFinite(right)) right = 0;
        double peak = Math.Max(Math.Abs(left), Math.Abs(right));
        double desired = peak <= .9 ? 1 : (.9 + .1 * Math.Tanh((peak - .9) / .1)) / peak;
        gain = desired < gain ? desired : desired + (gain - desired) * release;
        left = (float)(left * gain); right = (float)(right * gain);
    }
}
