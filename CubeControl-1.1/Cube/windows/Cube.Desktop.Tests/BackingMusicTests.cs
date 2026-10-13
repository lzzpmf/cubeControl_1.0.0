using System.IO;
using Cube.Desktop.Services;
using Cube.Desktop.ViewModels;
using NAudio.Wave;

internal static class BackingMusicTests
{
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); }
    public static async Task Run(string folder)
    {
        const int rate = 22050;
        string Write(string name, Func<double, double> sample, int sampleRate = rate, int channels = 1, int duration = 16)
        {
            string path = Path.Combine(folder, name + ".wav");
            using var writer = new WaveFileWriter(path, WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels));
            var block = new float[sampleRate * channels];
            for (int second = 0; second < duration; second++)
            {
                for (int i = 0; i < sampleRate; i++) for (int channel = 0; channel < channels; channel++)
                    block[i * channels + channel] = (float)sample(second + i / (double)sampleRate) * (channel == 0 ? 1 : -1);
                writer.WriteSamples(block, 0, block.Length);
            }
            return path;
        }
        string? tempoFile = null;
        foreach (int bpm in new[] { 40, 60, 90, 120, 180, 240 })
        {
            string file = Write("pulse-" + bpm, t => { double phase = t % (60d / bpm); return phase < .06 ? Math.Sin(2 * Math.PI * 700 * t) * .8 * Math.Exp(-phase * 60) : 0; });
            var result = await Task.Run(() => BackingMusicAnalysis.Analyze(file, CancellationToken.None));
            Console.WriteLine($"MUSIC {bpm}: {result}");
            Check(Math.Abs(result.Bpm - bpm) < 2, "pulse tempo " + bpm + " avoids half/double octave error"); tempoFile = file;
        }
        foreach (bool minor in new[] { false, true })
        foreach (int cents in new[] { -40, 0, 30 })
        {
            int[][] chords = minor ? [[57, 60, 64], [62, 65, 69], [64, 68, 71], [57, 60, 64]] : [[60, 64, 67], [65, 69, 72], [67, 71, 74], [60, 64, 67]];
            string file = Write((minor ? "minor" : "major") + cents, t => chords[Math.Min(3, (int)t / 4)].Sum(note => .15 * Math.Sin(2 * Math.PI * 440 * Math.Pow(2, (note - 69d + cents / 100d) / 12) * t)));
            var result = await Task.Run(() => BackingMusicAnalysis.Analyze(file, CancellationToken.None));
            Console.WriteLine($"MUSIC key: {result}");
            Check(result.Key == (minor ? "Am" : "C"), "tonal progression identifies " + (minor ? "A minor" : "C major"));
        }
        var random = new Random(456);
        foreach (int sampleRate in new[] { 8000, 44100, 48000 })
        {
            string file = Write("stereo-" + sampleRate, t => { double phase = t % .5; return phase < .06 ? Math.Sin(2 * Math.PI * 700 * t) * .8 * Math.Exp(-phase * 60) : 0; }, sampleRate, 2);
            var result = await Task.Run(() => BackingMusicAnalysis.Analyze(file, CancellationToken.None));
            Check(Math.Abs(result.Bpm - 120) < 2, "resampling and opposite stereo phase preserve tempo at " + sampleRate);
        }
        string changing = Write("changing-tempo", t => { double phase = t % (60d / (t < 20 ? 80 : 130)); return phase < .06 ? Math.Sin(2 * Math.PI * 700 * t) * .8 * Math.Exp(-phase * 60) : 0; }, duration: 40);
        var changingResult = await Task.Run(() => BackingMusicAnalysis.Analyze(changing, CancellationToken.None));
        Check(changingResult.Bpm == 0, "incompatible sections do not fabricate a global tempo");
        foreach (bool noise in new[] { false, true })
        {
            string file = Write(noise ? "noise" : "silence", _ => noise ? random.NextDouble() * .2 - .1 : 0);
            var result = await Task.Run(() => BackingMusicAnalysis.Analyze(file, CancellationToken.None));
            Check(result.Bpm == 0 && result.Key == "Não definido", "non-musical input does not fabricate BPM or key");
        }
        using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
        try { BackingMusicAnalysis.Analyze(tempoFile!, cancelled.Token); throw new Exception("analysis ignored cancellation"); }
        catch (OperationCanceledException) { Console.WriteLine("PASS analysis cancellation"); }
        using var model = new BackingTracksViewModel(folder);
        model.AddPaths([tempoFile!]); var track = model.SelectedTrack!;
        Check(track.Tuning.Length == 0 && track.Preset == 0 && !track.MetadataLabel.EndsWith(" · "), "new track has no tuning or preset association");
        model.UpdateTrackMetadata(track, 133, "Dm", "Drop D", 2);
        await model.MusicAnalysisTask;
        Check(track.Bpm == 133 && track.OriginalKey == "Dm", "manual correction wins over running analysis");
        model.ReanalyzeMusic(track); await model.MusicAnalysisTask;
        Check(track.Bpm == 133 && track.OriginalKey == "Dm", "explicit reanalysis preserves manual corrections");
        Check(track.Tuning == "Drop D" && track.Preset == 2, "analysis preserves explicit tuning and preset associations");
        model.UpdateTrackMetadata(track, 133, "Dm", "", 0);
        Check(track.Tuning.Length == 0 && track.Preset == 0, "manual associations can be cleared");
        int analyses = model.MusicAnalysisCount;
        model.SelectedTrack = null; model.SelectedTrack = track; await model.MusicAnalysisTask;
        Check(track.Bpm == 133 && model.MusicAnalysisCount == analyses, "selection changes reuse cached analysis and corrections");
        File.SetLastWriteTimeUtc(tempoFile!, DateTime.UtcNow.AddSeconds(3));
        model.SelectedTrack = null; model.SelectedTrack = track; await model.MusicAnalysisTask;
        Check(model.MusicAnalysisCount == analyses + 1, "file identity change invalidates musical analysis cache");
        model.RemoveTrackItem(track); model.ApplyMusicResult(track, new(100, "C", 1, 1));
        Check(track.Bpm == 133, "late result cannot update a removed track");
    }
}
