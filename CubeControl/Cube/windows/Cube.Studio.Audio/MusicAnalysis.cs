using System.Numerics;

namespace Cube.Studio.Audio;

internal sealed record MusicResult(double Bpm, string Key, double TempoConfidence, double KeyConfidence);

// Offline estimates only: spectral flux/pulse consensus and detuning-corrected
// chroma/profile correlation. Confidence is a heuristic, not a calibrated probability.
internal static class MusicAnalysis
{
    private const int Rate = 11025, Frame = 4096, Hop = 1024;
    private static readonly double[] Major = [6.35, 2.23, 3.48, 2.33, 4.38, 4.09, 2.52, 5.19, 2.39, 3.66, 2.29, 2.88];
    private static readonly double[] Minor = [6.33, 2.68, 3.52, 5.38, 2.60, 3.53, 2.54, 4.75, 3.98, 2.69, 3.34, 3.17];
    private static readonly string[] Notes = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];

    internal const int AlgorithmVersion = 2;
    private const string UnknownKey = "Não definido";
    private sealed record TempoCandidate(double Bpm, double Score);
    private sealed record TonalEvidence(double[] Scores);

    internal static MusicResult Analyze(string path, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        using var decoder = new StudioDecoder(path);
        if (!double.IsFinite(decoder.Duration) || decoder.Duration < 3) return new(0, UnknownKey, 0, 0);
        int segments = (int)Math.Min(5, Math.Ceiling(decoder.Duration / 20));
        double seconds = Math.Min(20, decoder.Duration / segments);
        var tempos = new List<TempoCandidate[]>();
        var keys = new List<TonalEvidence>();
        for (int part = 0; part < segments; part++)
        {
            token.ThrowIfCancellationRequested();
            double start = segments == 1 ? 0 : (decoder.Duration - seconds) * part / (segments - 1);
            var signal = ReadSegment(decoder, start, seconds, token);
            if (signal.Length < Rate * 3) continue;
            double mean = signal.Average(x => (double)x);
            double rms = Math.Sqrt(signal.Average(x => (x - mean) * (x - mean)));
            if (rms < 1e-5) continue;
            // Normalize analysis level, not playback. DC and gain must not decide the result.
            for (int i = 0; i < signal.Length; i++) signal[i] = (float)((signal[i] - mean) / rms * .1);
            var tempo = Tempo(signal, token);
            if (tempo.Length > 0) tempos.Add(tempo);
            var tonal = Chroma(signal, token);
            if (tonal != null) keys.Add(tonal);
        }
        var (bpm, tempoConfidence) = CombineTempos(tempos);
        var (key, keyConfidence) = CombineKeys(keys);
        return new(bpm, key, tempoConfidence, keyConfidence);
    }

    private static float[] ReadSegment(StudioDecoder decoder, double start, double seconds, CancellationToken token)
    {
        int channels = decoder.Format.Channels, sourceRate = decoder.Format.SampleRate;
        long first = (long)(start * sourceRate);
        int frames = checked((int)Math.Ceiling(seconds * sourceRate));
        var energy = new double[channels];
        var block = new float[4096 * channels];
        // Select once over the whole segment: block-by-block switching creates false onsets.
        decoder.Seek(first);
        int remaining = frames;
        while (remaining > 0)
        {
            token.ThrowIfCancellationRequested();
            int read = decoder.Read(block, 0, Math.Min(remaining, 4096) * channels);
            if (read == 0) break;
            for (int i = 0; i < read; i++) if (float.IsFinite(block[i])) energy[i % channels] += (double)block[i] * block[i];
            remaining -= read / channels;
        }
        int channel = Array.IndexOf(energy, energy.Max());
        decoder.Seek(first);
        var mono = new float[frames];
        int written = 0;
        while (written < frames)
        {
            token.ThrowIfCancellationRequested();
            int read = decoder.Read(block, 0, Math.Min(frames - written, 4096) * channels);
            if (read == 0) break;
            for (int i = 0; i + channels <= read; i += channels)
                mono[written++] = float.IsFinite(block[i + channel]) ? block[i + channel] : 0;
        }
        if (written != mono.Length) Array.Resize(ref mono, written);
        return Resample(mono, sourceRate, token);
    }

    private static float[] Resample(float[] source, int sourceRate, CancellationToken token)
    {
        if (sourceRate == Rate) return source;
        // Polyphase Blackman-windowed sinc. The cutoff is below the smaller Nyquist
        // frequency; kernel width grows with downsampling ratio to retain rejection.
        const int phases = 256;
        double ratio = sourceRate / (double)Rate;
        double cutoff = .45 / Math.Max(1, ratio);
        int radius = (int)Math.Ceiling(16 * Math.Max(1, ratio));
        int taps = radius * 2 + 1;
        var kernels = new double[phases][];
        for (int phase = 0; phase < phases; phase++)
        {
            token.ThrowIfCancellationRequested();
            var kernel = kernels[phase] = new double[taps];
            double sum = 0;
            for (int j = -radius; j <= radius; j++)
            {
                double distance = j - phase / (double)phases;
                double x = 2 * cutoff * distance;
                double sinc = Math.Abs(x) < 1e-12 ? 1 : Math.Sin(Math.PI * x) / (Math.PI * x);
                double window = Math.Abs(distance) <= radius ? .42 + .5 * Math.Cos(Math.PI * distance / radius) + .08 * Math.Cos(2 * Math.PI * distance / radius) : 0;
                sum += kernel[j + radius] = 2 * cutoff * sinc * window;
            }
            for (int j = 0; j < taps; j++) kernel[j] /= sum;
        }
        var result = new float[(int)(source.Length / ratio)];
        for (int i = 0; i < result.Length; i++)
        {
            if ((i & 1023) == 0) token.ThrowIfCancellationRequested();
            double position = i * ratio;
            int center = (int)position, phase = Math.Min(phases - 1, (int)((position - center) * phases));
            var kernel = kernels[phase]; double value = 0;
            int low = Math.Max(0, radius - center), high = Math.Min(taps, source.Length + radius - center);
            for (int j = low; j < high; j++) value += source[center + j - radius] * kernel[j];
            result[i] = (float)value;
        }
        return result;
    }

    private static TempoCandidate[] Tempo(float[] signal, CancellationToken token)
    {
        const int frame = 1024, hop = 110;
        int count = 1 + (signal.Length - frame) / hop;
        if (count < 250) return [];
        var data = new Complex[frame]; var previous = new double[frame / 2];
        var flux = new double[count];
        for (int t = 0; t < count; t++)
        {
            token.ThrowIfCancellationRequested();
            for (int j = 0; j < frame; j++) data[j] = signal[t * hop + j] * (.5 - .5 * Math.Cos(2 * Math.PI * j / (frame - 1)));
            Fft(data);
            for (int bin = 3; bin < previous.Length; bin++)
            {
                double value = Math.Log(1 + 10 * data[bin].Magnitude);
                // The first frame has no predecessor and cannot establish a beat.
                if (t > 0) flux[t] += Math.Max(0, value - previous[bin]);
                previous[bin] = value;
            }
        }
        var sums = new double[count + 1]; var squares = new double[count + 1];
        for (int i = 0; i < count; i++) { sums[i + 1] = sums[i] + flux[i]; squares[i + 1] = squares[i] + flux[i] * flux[i]; }
        if (squares[count] < 1e-8) return [];
        var onset = new double[count];
        for (int i = 0; i < count; i++)
        {
            int left = Math.Max(0, i - 50), right = Math.Min(count, i + 51);
            double mean = (sums[right] - sums[left]) / (right - left);
            double scale = Math.Sqrt((squares[right] - squares[left]) / (right - left));
            onset[i] = Math.Max(0, flux[i] - mean) / Math.Max(1e-8, scale);
        }
        var events = new List<(int Time, double Weight)>();
        for (int i = 2; i < count - 2; i++)
            if (onset[i] > .5 && onset[i] > onset[i - 1] && onset[i] >= onset[i + 1])
            {
                if (events.Count > 0 && i - events[^1].Time < 5)
                { if (onset[i] > events[^1].Weight) events[^1] = (i, onset[i]); }
                else events.Add((i, onset[i]));
            }
        if (events.Count < 4) return [];
        double average = onset.Average();
        var smooth = new double[count];
        for (int i = 1; i < count - 1; i++) smooth[i] = .25 * onset[i - 1] + .5 * onset[i] + .25 * onset[i + 1] - average;
        double onsetRate = Rate / (double)hop;
        int lowLag = (int)Math.Floor(60 * onsetRate / 240), highLag = (int)Math.Ceiling(60 * onsetRate / 40);
        var scores = new double[highLag + 2];
        for (int lag = lowLag - 1; lag <= highLag + 1; lag++)
        {
            token.ThrowIfCancellationRequested();
            double dot = 0, left = 0, right = 0;
            for (int i = lag; i < count; i++) { dot += smooth[i] * smooth[i - lag]; left += smooth[i] * smooth[i]; right += smooth[i - lag] * smooth[i - lag]; }
            scores[lag] = dot / (Math.Sqrt(left * right) + 1e-15);
        }
        var candidates = new List<TempoCandidate>();
        for (int lag = lowLag; lag <= highLag; lag++)
        {
            if (scores[lag] < .25 || scores[lag] < scores[lag - 1] || scores[lag] < scores[lag + 1]) continue;
            double denominator = scores[lag - 1] - 2 * scores[lag] + scores[lag + 1];
            double offset = Math.Abs(denominator) < 1e-12 ? 0 : Math.Clamp(.5 * (scores[lag - 1] - scores[lag + 1]) / denominator, -.5, .5);
            double period = lag + offset, bpm = 60 * onsetRate / period;
            if (bpm < 39.5 || bpm > 240.5) continue;
            double pulse = PulseFit(events, period, token);
            candidates.Add(new(Math.Clamp(bpm, 40, 240), .65 * scores[lag] + .35 * pulse));
        }
        return candidates.OrderByDescending(c => c.Score).ToArray();
    }

    private static double PulseFit(List<(int Time, double Weight)> events, double period, CancellationToken token)
    {
        // Reward both explained onsets and occupied beats. Half tempo skips events;
        // double tempo invents empty beats. Neither is preferred just for being faster.
        double total = events.Sum(e => e.Weight), best = 0;
        double expected = 1 + (events[^1].Time - events[0].Time) / period;
        foreach (var anchor in events.OrderByDescending(e => e.Weight).Take(32))
        {
            token.ThrowIfCancellationRequested();
            double covered = 0; var beats = new HashSet<int>();
            foreach (var onset in events)
            {
                double beat = (onset.Time - anchor.Time) / period;
                int nearest = (int)Math.Round(beat);
                if (Math.Abs(beat - nearest) * period > Math.Min(period * .15, 2 + period * .03)) continue;
                covered += onset.Weight; beats.Add(nearest);
            }
            best = Math.Max(best, Math.Sqrt(covered / total * Math.Min(1, beats.Count / expected)));
        }
        return best;
    }

    private static bool NearTempo(double a, double b) => Math.Abs(a - b) <= Math.Max(1.5, b * .025);
    private static (double, double) CombineTempos(List<TempoCandidate[]> segments)
    {
        if (segments.Count == 0) return (0, 0);
        var seeds = segments.SelectMany(s => s).OrderByDescending(c => c.Score).ToArray();
        var ranked = new List<(double Bpm, double Score, double Support)>();
        foreach (var seed in seeds)
        {
            if (ranked.Any(c => NearTempo(c.Bpm, seed.Bpm))) continue;
            var matches = segments.Select(s => s.Where(c => NearTempo(c.Bpm, seed.Bpm)).MaxBy(c => c.Score)).ToArray();
            double sum = matches.Sum(c => c?.Score ?? 0);
            if (sum <= 0) continue;
            double support = matches.Where((c, i) => c != null && c.Score >= segments[i][0].Score * .88).Count() / (double)segments.Count;
            ranked.Add((matches.Sum(c => c == null ? 0 : c.Bpm * c.Score) / sum, sum / segments.Count, support));
        }
        var ordered = ranked.OrderByDescending(c => c.Score).ToArray();
        if (ordered.Length == 0) return (0, 0);
        var best = ordered[0];
        double margin = best.Score - (ordered.Length > 1 ? ordered[1].Score : 0);
        if (best.Score < .45 || best.Support < .6 || margin < .06) return (0, 0);
        // A strong incompatible section indicates changing tempo, not a reliable global BPM.
        if (segments.Any(s => s[0].Score >= .6 && !NearTempo(s[0].Bpm, best.Bpm)
            && !NearTempo(s[0].Bpm * 2, best.Bpm) && !NearTempo(s[0].Bpm / 2, best.Bpm))) return (0, 0);
        return (Math.Round(best.Bpm, 1), Math.Clamp(best.Score * best.Support * Math.Min(1, margin / .2), 0, 1));
    }

    private static TonalEvidence? Chroma(float[] signal, CancellationToken token)
    {
        var data = new Complex[Frame]; var magnitude = new double[Frame / 2];
        var frames = new List<List<(double Note, double Weight)>>();
        double tuneX = 0, tuneY = 0, tuneWeight = 0;
        for (int start = 0; start + Frame <= signal.Length; start += Hop)
        {
            token.ThrowIfCancellationRequested();
            for (int i = 0; i < Frame; i++) data[i] = signal[start + i] * (.5 - .5 * Math.Cos(2 * Math.PI * i / (Frame - 1)));
            Fft(data);
            for (int i = 0; i < magnitude.Length; i++) magnitude[i] = data[i].Magnitude;
            double maximum = magnitude.Max(); if (maximum < .01) continue;
            var prefix = new double[magnitude.Length + 1];
            for (int i = 0; i < magnitude.Length; i++) prefix[i + 1] = prefix[i] + magnitude[i];
            var peaks = new List<(double Note, double Weight)>();
            for (int i = Math.Max(2, (int)(25d * Frame / Rate)); i < Math.Min(magnitude.Length - 1, (int)(3500d * Frame / Rate)); i++)
            {
                if (magnitude[i] < maximum * .02 || magnitude[i] <= magnitude[i - 1] || magnitude[i] < magnitude[i + 1]) continue;
                double a = Math.Log(magnitude[i - 1] + 1e-15), b = Math.Log(magnitude[i] + 1e-15), c = Math.Log(magnitude[i + 1] + 1e-15);
                double denominator = a - 2 * b + c;
                double offset = Math.Abs(denominator) < 1e-12 ? 0 : Math.Clamp(.5 * (a - c) / denominator, -.5, .5);
                double note = 69 + 12 * Math.Log2((i + offset) * Rate / Frame / 440);
                int radius = Math.Max(8, i / 12), low = Math.Max(0, i - radius), high = Math.Min(magnitude.Length, i + radius + 1);
                double envelope = (prefix[high] - prefix[low]) / (high - low);
                double weight = Math.Min(8, magnitude[i] / (envelope + maximum * .01)) * Math.Sqrt(magnitude[i] / maximum);
                peaks.Add((note, weight));
            }
            if (peaks.Count == 0) continue;
            double sum = peaks.Sum(p => p.Weight);
            for (int i = 0; i < peaks.Count; i++)
            {
                var peak = peaks[i]; peaks[i] = (peak.Note, peak.Weight / sum);
                tuneX += Math.Cos(2 * Math.PI * peak.Note) * peak.Weight / sum;
                tuneY += Math.Sin(2 * Math.PI * peak.Note) * peak.Weight / sum;
            }
            tuneWeight++; frames.Add(peaks);
        }
        if (frames.Count < 4) return null;
        double concentration = Math.Sqrt(tuneX * tuneX + tuneY * tuneY) / tuneWeight;
        if (concentration < .2) return null;
        double detuning = Math.Atan2(tuneY, tuneX) / (2 * Math.PI);
        var hpcp = new double[36];
        foreach (var frame in frames)
        {
            token.ThrowIfCancellationRequested();
            foreach (var peak in frame)
            {
                double bin = ((peak.Note - detuning) * 3 % 36 + 36) % 36;
                int index = (int)bin; double fraction = bin - index;
                hpcp[index] += peak.Weight * (1 - fraction);
                hpcp[(index + 1) % 36] += peak.Weight * fraction;
            }
        }
        var chroma = new double[12];
        for (int i = 0; i < 12; i++) chroma[i] = hpcp[3 * i] + .5 * (hpcp[(3 * i + 35) % 36] + hpcp[(3 * i + 1) % 36]);
        if (chroma.Count(x => x > chroma.Max() * .1) < 3) return null;
        var scores = new double[24];
        for (int mode = 0; mode < 2; mode++) for (int tonic = 0; tonic < 12; tonic++)
        {
            var profile = mode == 0 ? Major : Minor;
            double a = chroma.Average(), b = profile.Average(), dot = 0, aa = 0, bb = 0;
            for (int i = 0; i < 12; i++) { double x = chroma[(i + tonic) % 12] - a, y = profile[i] - b; dot += x * y; aa += x * x; bb += y * y; }
            scores[mode * 12 + tonic] = dot / (Math.Sqrt(aa * bb) + 1e-15);
        }
        return scores.Max() >= .45 ? new(scores) : null;
    }

    private static (string, double) CombineKeys(List<TonalEvidence> segments)
    {
        if (segments.Count == 0) return (UnknownKey, 0);
        var ranked = Enumerable.Range(0, 24).Select(key => (Key: key, Score: segments.Average(s => s.Scores[key])))
            .OrderByDescending(c => c.Score).ToArray();
        var best = ranked[0]; double margin = best.Score - ranked[1].Score;
        int agreeing = 0;
        foreach (var segment in segments)
        {
            var local = segment.Scores.OrderByDescending(s => s).ToArray();
            if (segment.Scores[best.Key] >= local[0] - .04) agreeing++;
            // Strong evidence for another key must not be hidden by averaging.
            if (local[0] >= .6 && local[0] - local[1] >= .08 && segment.Scores[best.Key] < local[0] - .08) return (UnknownKey, 0);
        }
        double support = agreeing / (double)segments.Count;
        if (best.Score < .55 || margin < .04 || support < .6) return (UnknownKey, 0);
        return (Notes[best.Key % 12] + (best.Key >= 12 ? "m" : ""), Math.Clamp(margin * 4 * support, 0, 1));
    }
    private static void Fft(Complex[] data)
    {
        for (int i = 1, j = 0; i < data.Length; i++)
        { int bit = data.Length >> 1; for (; (j & bit) != 0; bit >>= 1) j ^= bit; j ^= bit; if (i < j) (data[i], data[j]) = (data[j], data[i]); }
        for (int length = 2; length <= data.Length; length <<= 1)
        {
            var factor = Complex.FromPolarCoordinates(1, -2 * Math.PI / length);
            for (int start = 0; start < data.Length; start += length)
            { Complex w = 1; for (int i = 0; i < length / 2; i++) { var even = data[start + i]; var odd = data[start + i + length / 2] * w; data[start + i] = even + odd; data[start + i + length / 2] = even - odd; w *= factor; } }
        }
    }
}