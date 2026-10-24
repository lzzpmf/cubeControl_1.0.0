using System.IO;
using Microsoft.Win32;
using Cube.Studio.Audio;
using Cube.Studio.Core;

namespace Cube.Desktop.Studio.ViewModels;

public sealed partial class ShellViewModel
{
    private readonly Dictionary<string, StudioMediaInfo> _media = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? _backingPreparation;
    public event EventHandler? PeaksChanged;
    public bool HasMedia(ClipModel clip) => clip.ActiveSource.SourcePath is { } path && _media.ContainsKey(path);
    public float GetPeak(ClipModel clip, double sourceSeconds, double spanSeconds)
    {
        if (clip.ActiveSource.SourcePath is not { } path || !_media.TryGetValue(path, out var media) || media.Peaks.Length == 0 || media.Duration <= 0) return 0;
        int start = Math.Clamp((int)(sourceSeconds / media.Duration * media.Peaks.Length), 0, media.Peaks.Length - 1);
        int end = Math.Clamp((int)Math.Ceiling((sourceSeconds + Math.Max(0, spanSeconds)) / media.Duration * media.Peaks.Length), start + 1, media.Peaks.Length);
        float peak = 0;
        for (int i = start; i < end; i++) if (float.IsFinite(media.Peaks[i])) peak = Math.Max(peak, media.Peaks[i]);
        return peak;
    }
    private void ImportFiles()
    {
        var dialog = new OpenFileDialog { Title = T("Importar backing"), Multiselect = false, Filter = T("Áudio") + "|*.wav;*.mp3;*.flac;*.ogg;*.m4a;*.aac;*.aif;*.aiff" };
        if (dialog.ShowDialog(Owner) == true) ImportPaths([dialog.FileName]);
    }
    public void ImportPaths(IEnumerable<string> paths)
    {
        if (!CanReplaceBacking || paths.FirstOrDefault() is not { } path) return;
        long intent = BeginBackingIntent();
        Run(() => LoadBackingAsync(path, intent, _sessionGeneration));
    }
    private long BeginBackingIntent()
    {
        _backingPreparation?.Cancel();
        return ++_backingIntent;
    }
    internal async Task LoadBackingAsync(string path, long intent, long generation)
    {
        if (IsRecording || IsBusy || _closePending || intent != _backingIntent || generation != _sessionGeneration || _disposed) return;
        using var preparation = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
        _backingPreparation = preparation;
        try
        {
            StatusMessage = T("Analisando áudio: ") + Path.GetFileName(path);
            var media = await _audio.AnalyzeAsync(Path.GetFullPath(path), preparation.Token);
            if (media.Duration < ClipModel.MinimumDuration) throw new InvalidDataException(T("O áudio é curto demais."));
            preparation.Token.ThrowIfCancellationRequested();
            if (intent != _backingIntent || generation != _sessionGeneration || !CanReplaceBacking || _disposed) return;
            if (!await ConfirmDiscardAsync()) return;
            // A dialog or an analysis can finish after the musician chose another backing.
            if (intent != _backingIntent || generation != _sessionGeneration || !CanReplaceBacking || _disposed) return;
            await ExecuteTransportAsync(async () =>
            {
                if (intent != _backingIntent || generation != _sessionGeneration || IsRecording || IsBusy || _closePending || _disposed) return;
                await StopTransportCoreAsync();
                var source = new PracticeSource { Name = Path.GetFileNameWithoutExtension(media.Path), SourcePath = media.Path,
                    SourceSampleRate = media.SampleRate, SourceChannels = media.Channels,
                    SourceFrameCount = (long)Math.Round(media.Duration * media.SampleRate) };
                Editor.SetBacking(source, clearAttempts: true); ++_sessionGeneration;
                _media.Clear(); _media[media.Path] = media;
                SyncPresentation(); await FlushSnapshotAsync();
                if (!_preserveRecordings) CleanupOwnedRecordings();
                _discardApproved = false; StatusMessage = "Backing pronta para tocar";
            });
        }
        finally { if (_backingPreparation == preparation) _backingPreparation = null; RefreshTransport(); }
    }
    internal StudioSnapshot BuildSnapshot(bool includeBacking = true)
    {
        var backing = Session.Backing;
        var attempt = IsRecording ? null : SelectedAttempt;
        return new StudioSnapshot
        {
            PlaybackRate = Session.SpeedPercent / 100, Semitones = Session.Semitones,
            Duration = Math.Max(IsRecording ? Math.Min(86400, Session.Position + .15) : 0,
                Math.Max(Session.LoopEnabled ? Session.LoopEnd : 0, Math.Max(backing?.End ?? 0, attempt?.End ?? 0))),
            LoopEnabled = Session.LoopEnabled, LoopStart = Session.LoopStart, LoopEnd = Session.LoopEnd,
            MetronomeEnabled = false,
            Tracks = new[]
            {
                new StudioTrack { Id = Project.Tracks[0].Id, VolumeDb = Session.BackingMix.VolumeDb, Muted = Session.BackingMix.Muted,
                    Clips = includeBacking && backing != null ? [AudioClip(backing)] : [] },
                new StudioTrack { Id = Project.Tracks[1].Id, VolumeDb = Session.AttemptMix.VolumeDb, Muted = Session.AttemptMix.Muted,
                    Clips = attempt != null ? [AudioClip(attempt)] : [] }
            }
        };
    }
    private static StudioClip AudioClip(PracticeSource source) => new()
    {
        Id = source.Id, Path = source.SourcePath!, Start = source.AudibleStart, Duration = source.Duration,
        SourceOffset = source.TrimStartFrame / (double)source.SourceSampleRate,
        RecordedSpeed = source.RecordedSpeed, RecordedPitch = source.RecordedSemitones, ClockRatio = source.ClockRatio
    };
    private void QueueSnapshot() { if (!_disposed) { _renderDirty = true; _snapshotRetryAfter = 0; } }
    private async Task FlushSnapshotAsync()
    {
        await _snapshotGate.WaitAsync(_lifetime.Token);
        try
        {
            if (!_renderDirty || _disposed) return;
            var snapshot = BuildSnapshot(); _renderDirty = false;
            _snapshotWorker = _audio.ApplySnapshotAsync(snapshot, _lifetime.Token);
            try { await _snapshotWorker; }
            catch { _renderDirty = true; _snapshotRetryAfter = _tapClock.Elapsed.TotalSeconds + 2; _snapshotWorker = Task.CompletedTask; throw; }
        }
        finally { _snapshotGate.Release(); }
    }
}
