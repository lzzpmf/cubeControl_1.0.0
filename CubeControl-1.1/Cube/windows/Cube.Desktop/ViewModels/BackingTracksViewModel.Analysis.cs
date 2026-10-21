using System.IO;
using Cube.Desktop.Services;

namespace Cube.Desktop.ViewModels;

public sealed partial class BackingTracksViewModel
{
    internal int MusicAnalysisCount { get; private set; }
    private void MusicLanguageChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    { if (e.PropertyName == "Language") { foreach (var track in Tracks) track.RefreshMusic(); RefreshMetronome(); } }
    private readonly Dictionary<string, BackingMusicResult> musicCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<BackingTrack, string> musicFinished = [];
    private BackingTrack? musicActive;
    private CancellationTokenSource? musicCancellation;
    private bool musicRunning;
    internal Task MusicAnalysisTask { get; private set; } = Task.CompletedTask;
    private static string MusicIdentity(BackingTrack track)
    {
        var file = new FileInfo(track.Source);
        return $"music-v{BackingMusicAnalysis.AlgorithmVersion}|" + track.Source + "|" + (file.Exists ? $"{file.Length}|{file.LastWriteTimeUtc.Ticks}" : "missing");
    }
    private void ScheduleMusic()
    {
        if (disposed) return;
        foreach (var old in musicFinished.Keys.Where(t => !Tracks.Contains(t)).ToArray()) musicFinished.Remove(old);
        if (musicActive != null && (!Tracks.Contains(musicActive) || SelectedTrack != null && musicActive != SelectedTrack &&
            (!musicFinished.TryGetValue(SelectedTrack, out var done) || done != MusicIdentity(SelectedTrack)))) musicCancellation?.Cancel();
        if (!musicRunning) { MusicAnalysisTask = AnalyzeMusicQueue(); Start(MusicAnalysisTask); }
    }
    internal void ReanalyzeMusic(BackingTrack track)
    {
        if (!Tracks.Contains(track)) return;
        musicFinished.Remove(track); musicCache.Remove(MusicIdentity(track));
        if (musicActive == track) musicCancellation?.Cancel();
        ScheduleMusic();
    }
    private async Task AnalyzeMusicQueue()
    {
        musicRunning = true;
        try
        {
            // Yield before touching the collection: AddPaths may still be adding entries.
            await Task.Yield();
            while (!disposed)
            {
                var target = Tracks.OrderByDescending(t => t == SelectedTrack)
                    .FirstOrDefault(t => !musicFinished.TryGetValue(t, out var id) || id != MusicIdentity(t));
                if (target == null) break;
                string identity = MusicIdentity(target);
                musicActive = target;
                using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
                musicCancellation = cancellation; target.MusicPending = true; target.RefreshMusic(); RefreshPractice();
                try
                {
                    if (!musicCache.TryGetValue(identity, out var result))
                    {
                        MusicAnalysisCount++;
                        var work = Task.Run(() => BackingMusicAnalysis.Analyze(target.Source, cancellation.Token), cancellation.Token);
                        backgroundWork.Add(work);
                        try { result = await work; } finally { backgroundWork.Remove(work); }
                        if (musicCache.Count >= 256) musicCache.Remove(musicCache.Keys.First());
                        musicCache[identity] = result;
                    }
                    if (disposed || cancellation.IsCancellationRequested || !Tracks.Contains(target) || identity != MusicIdentity(target)) continue;
                    ApplyMusicResult(target, result); musicFinished[target] = identity;
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) { Log(ex); musicFinished[target] = identity; }
                finally
                {
                    target.MusicPending = false; target.RefreshMusic();
                    if (!disposed && target == SelectedTrack) RefreshPractice();
                    musicActive = null; musicCancellation = null;
                }
            }
        }
        finally { musicRunning = false; }
    }
    internal void ApplyMusicResult(BackingTrack target, BackingMusicResult result)
    {
        if (!Tracks.Contains(target)) return;
        if (!target.ManualBpm) target.Bpm = result.Bpm;
        if (!target.ManualKey) target.OriginalKey = result.Key;
        target.TempoConfidence = result.TempoConfidence; target.KeyConfidence = result.KeyConfidence;
        target.RefreshMusic(); if (target == SelectedTrack) RefreshPractice();
        PersistSession();
    }
}
