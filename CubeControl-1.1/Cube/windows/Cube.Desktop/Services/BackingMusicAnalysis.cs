using System.Numerics;

namespace Cube.Desktop.Services;

internal sealed record BackingMusicResult(double Bpm, string Key, double TempoConfidence, double KeyConfidence);

// Estimates only: periodic onsets and chroma/profile correlation, independent of playback DSP.
internal static class BackingMusicAnalysis
{
    private const int Rate = 11025, Frame = 4096, Hop = 2048;
    private static readonly double[] Major = [6.35, 2.23, 3.48, 2.33, 4.38, 4.09, 2.52, 5.19, 2.39, 3.66, 2.29, 2.88];
    private static readonly double[] Minor = [6.33, 2.68, 3.52, 5.38, 2.60, 3.53, 2.54, 4.75, 3.98, 2.69, 3.34, 3.17];
    private static readonly string[] Notes = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];

    internal static BackingMusicResult Analyze(string path, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        using var decoder = new BackingDecoder(path);
        var chroma = new double[12]; var tempos = new List<(double Bpm, double Confidence)>();
        int segments = decoder.Duration > 30 ? 3 : 1;
        double seconds = Math.Min(30, decoder.Duration / segments);
        for (int part = 0; part < segments; part++)
        {
            token.ThrowIfCancellationRequested();
            double start = segments == 1 ? 0 : (decoder.Duration - seconds) * part / (segments - 1);
            decoder.Seek((long)(start * decoder.Format.SampleRate));
            int channels = decoder.Format.Channels, sourceRate = decoder.Format.SampleRate;
            var samples = new List<float>((int)(seconds * Rate) + 1);
            var block = new float[4096 * channels]; long frames = 0, limit = (long)(seconds * sourceRate);
            double sum = 0; int count = 0; long next = 1;
            while (frames < limit)
            {
                token.ThrowIfCancellationRequested();
                int read = decoder.Read(block, 0, (int)Math.Min(block.Length, (limit - frames) * channels));
                if (read == 0) break;
                int channel = 0; double strongest = 0;
                for (int c = 0; c < channels; c++)
                {
                    double energy = 0; for (int i = c; i < read; i += channels) energy += block[i] * block[i];
                    if (energy > strongest) { strongest = energy; channel = c; }
                }
                for (int i = 0; i + channels <= read; i += channels)
                {
                    // Dominant channel avoids phase cancellation and silent-channel bias.
                    sum += float.IsFinite(block[i + channel]) ? block[i + channel] : 0; count++; frames++;
                    if (frames * Rate >= next * sourceRate)
                    { float value = (float)(sum / count); while (frames * Rate >= next * sourceRate) { samples.Add(value); next++; } sum = 0; count = 0; }
                }
            }
            var signal = samples.ToArray();
            tempos.Add(Tempo(signal, token)); AccumulateChroma(signal, chroma, token);
        }
        var valid = tempos.Where(t => t.Confidence >= .28).OrderByDescending(t => t.Confidence).ToArray();
        double bpm = valid.Length == 0 ? 0 : valid[0].Bpm;
        double tempoConfidence = valid.Length == 0 ? 0 : valid[0].Confidence;
        if (valid.Length > 1 && valid.Any(t => Math.Abs(t.Bpm - bpm) > 4 && Math.Abs(t.Bpm * 2 - bpm) > 4 && Math.Abs(t.Bpm / 2 - bpm) > 4))
        { bpm = 0; tempoConfidence = 0; }
        var (key, confidence) = Key(chroma);
        return new(bpm, key, tempoConfidence, confidence);
    }
    private static (double, double) Tempo(float[] signal, CancellationToken token)
    {
        const int step = 110;
        int size = signal.Length / step; if (size < 300) return (0, 0);
        var onset = new double[size]; double previous = 0;
        for (int i = 0; i < size; i++)
        {
            double energy = 0;
            for (int j = 0; j < step; j++) energy += signal[i * step + j] * signal[i * step + j];
            energy = Math.Sqrt(energy / step); onset[i] = Math.Max(0, energy - previous); previous = energy;
        }
        // Smooth the 10 ms quantization so a fractional beat period does not
        // spuriously favor its integer-aligned double (for example 180 -> 90).
        var smooth = new double[size];
        for (int i = 1; i < size - 1; i++) smooth[i] = .25 * onset[i - 1] + .5 * onset[i] + .25 * onset[i + 1];
        onset = smooth;
        double mean = onset.Average();
        for (int i = 0; i < size; i++) onset[i] -= mean;
        double total = onset.Sum(x => x * x); if (total < 1e-7) return (0, 0);
        double best = 0; int lagBest = 0; var scores = new double[152];
        // Prefer the shortest convincing period when exact multiples tie.
        for (int lag = 24; lag <= 151; lag++)
        {
            token.ThrowIfCancellationRequested(); double value = 0, left = 0, right = 0;
            for (int i = lag; i < size; i++) { value += onset[i] * onset[i - lag]; left += onset[i] * onset[i]; right += onset[i - lag] * onset[i - lag]; }
            value /= Math.Sqrt(left * right) + 1e-15;
            scores[lag] = value;
            if (lag >= 25 && lag <= 150 && value > best) { best = value; lagBest = lag; }
        }
        if (best < .28) return (0, best);
        for (int lag = 25; lag <= 150; lag++)
            if (scores[lag] >= best * .9 && scores[lag] >= scores[lag - 1] && scores[lag] >= scores[lag + 1]) { lagBest = lag; break; }
        double denominator = scores[lagBest - 1] - 2 * scores[lagBest] + scores[lagBest + 1];
        double offset = Math.Abs(denominator) < 1e-12 ? 0 : Math.Clamp(.5 * (scores[lagBest - 1] - scores[lagBest + 1]) / denominator, -.5, .5);
        return (Math.Round(60d * Rate / step / (lagBest + offset), 1), scores[lagBest]);
    }
    private static void AccumulateChroma(float[] signal, double[] chroma, CancellationToken token)
    {
        var data = new Complex[Frame]; var magnitude = new double[Frame / 2];
        for (int start = 0; start + Frame <= signal.Length; start += Hop)
        {
            token.ThrowIfCancellationRequested();
            for (int i = 0; i < Frame; i++) data[i] = signal[start + i] * (.5 - .5 * Math.Cos(2 * Math.PI * i / (Frame - 1)));
            Fft(data);
            for (int i = 1; i < magnitude.Length; i++) magnitude[i] = data[i].Magnitude;
            double peak = magnitude.Max(); if (peak < .01) continue;
            for (int i = 20; i < Math.Min(magnitude.Length - 1, 1600); i++)
            {
                if (magnitude[i] < peak * .08 || magnitude[i] <= magnitude[i - 1] || magnitude[i] < magnitude[i + 1]) continue;
                double offset = .5 * (magnitude[i - 1] - magnitude[i + 1]) / (magnitude[i - 1] - 2 * magnitude[i] + magnitude[i + 1]);
                double note = 69 + 12 * Math.Log2((i + offset) * Rate / Frame / 440);
                int midi = (int)Math.Round(note);
                if (Math.Abs(note - midi) < .35) chroma[(midi % 12 + 12) % 12] += magnitude[i];
            }
        }
    }
    private static (string, double) Key(double[] chroma)
    {
        if (chroma.Sum() < .01 || chroma.Count(x => x > chroma.Max() * .1) < 3) return ("Não definido", 0);
        var scores = new List<(string Key, double Score)>();
        foreach (bool minor in new[] { false, true }) for (int tonic = 0; tonic < 12; tonic++)
        {
            var profile = minor ? Minor : Major; double a = chroma.Average(), b = profile.Average(), dot = 0, aa = 0, bb = 0;
            for (int i = 0; i < 12; i++) { double x = chroma[(i + tonic) % 12] - a, y = profile[i] - b; dot += x * y; aa += x * x; bb += y * y; }
            scores.Add((Notes[tonic] + (minor ? "m" : ""), dot / (Math.Sqrt(aa * bb) + 1e-15)));
        }
        var ordered = scores.OrderByDescending(x => x.Score).ToArray(); double margin = ordered[0].Score - ordered[1].Score;
        return ordered[0].Score >= .55 && margin >= .04 ? (ordered[0].Key, Math.Clamp(margin * 4, 0, 1)) : ("Não definido", 0);
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
