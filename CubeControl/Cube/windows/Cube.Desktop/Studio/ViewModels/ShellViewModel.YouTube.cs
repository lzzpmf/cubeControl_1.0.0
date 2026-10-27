using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Cube.Studio.Audio;

namespace Cube.Desktop.Studio.ViewModels;

public sealed partial class ShellViewModel
{
    private PlaybackResultViewModel? _previewResult, _hoverResult;
    private CancellationTokenSource? _previewRequest;
    private double _hoverSince;
    public ObservableCollection<PlaybackResultViewModel> Results { get; } = [];
    public string SearchText { get => Downloader.YouTubeQuery; set { StopPreview(); Downloader.YouTubeQuery = value; OnPropertyChanged(); } }
    public string ResultSummary => Downloader.YouTubeSearching ? T("Buscando faixas…") : $"{Results.Count} {T("resultados")}";
    public string NoResultsMessage => Results.Count > 0 ? "" : T(Downloader.YouTubeStatus.Length > 0 ? Downloader.YouTubeStatus : "Busque uma backing para começar");
    public bool HasResults => Results.Count > 0;
    public PlaybackResultViewModel? PreviewResult => _previewResult;
    public double PreviewProgress => _previewResult?.PreviewProgress ?? 0;
    public string PreviewStatus => _previewResult?.WaveformStatus ?? "";
    public string DownloadStatusText => T(Downloader.YouTubeDownloadStatus);

    private void ResultsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        StopPreview();
        var previous = Results.ToDictionary(r => r.Video.Id);
        Results.Clear();
        foreach (var video in Downloader.PresentedYouTubeResults)
            Results.Add(previous.TryGetValue(video.Id, out var item) ? item : new PlaybackResultViewModel(video));
        RefreshYouTubeResults();
    }
    private void DownloaderChanged(object? sender, PropertyChangedEventArgs e)
    {
        RefreshYouTubeResults();
        foreach (var command in _commands) command.RaiseCanExecuteChanged();
    }
    private void RefreshYouTubeResults()
    {
        foreach (var result in Results)
        {
            result.IsDownloading = Downloader.YouTubeDownloadId == result.Video.Id;
            result.DownloadStatus = Downloader.YouTubeDownloadMessageId == result.Video.Id ? T(Downloader.YouTubeDownloadStatus)
                : Downloader.IsYouTubeDownloaded(result.Video.Id) ? T("Áudio pronto.") : "";
            result.DownloadProgress = result.IsDownloading ? Downloader.YouTubePercent : 0;
            result.Refresh();
        }
        foreach (var name in new[] { nameof(ResultSummary), nameof(NoResultsMessage), nameof(HasResults), nameof(PreviewResult), nameof(PreviewProgress), nameof(PreviewStatus), nameof(DownloadStatusText) }) OnPropertyChanged(name);
    }
    public void PreviewHover(PlaybackResultViewModel result, bool entered)
    {
        if (entered)
        {
            if (!CanPreviewPlayback || _previewResult == result) return;
            StopPreview(); _hoverResult = result; _hoverSince = _tapClock.Elapsed.TotalSeconds;
        }
        else
        {
            if (_hoverResult == result) _hoverResult = null;
            if (_previewResult == result) StopPreview();
        }
        UpdateUiCadence();
    }
    private void TogglePreview(PlaybackResultViewModel result)
    {
        if (!CanPreviewPlayback) return;
        if (_previewResult == result) StopPreview(); else StartPreview(result);
    }
    private void StartPreview(PlaybackResultViewModel result)
    {
        if (!CanPreviewPlayback || !PageVisible) return;
        StopPreview();
        var request = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
        _previewRequest = request; _previewResult = result;
        result.IsPreparingWaveform = true; result.WaveformError = null; result.PreviewPosition = 0; result.Refresh();
        Run(() => PlaySessionPreviewAsync(result, request));
    }
    private async Task PlaySessionPreviewAsync(PlaybackResultViewModel result, CancellationTokenSource request)
    {
        using (request)
        {
            try
            {
                // The displayed waveform and the played PCM always come from this same decode.
                var prepared = await Downloader.PrepareSessionPreviewAsync(result.Video, request.Token);
                request.Token.ThrowIfCancellationRequested();
                if (_previewRequest != request || !PageVisible) return;
                result.Peaks = prepared.Waveform.Peaks; result.PreviewDuration = prepared.Waveform.Duration;
                result.HasFullWaveform = false; result.IsPreparingWaveform = false; result.Refresh();
                await ExecuteTransportAsync(async () =>
                {
                    request.Token.ThrowIfCancellationRequested();
                    if (_previewRequest != request || IsRecording || !PageVisible) return;
                    await PauseTransportCoreAsync();
                    if (BeforePlayback != null) await BeforePlayback();
                    await EnsureAudioAsync();
                    request.Token.ThrowIfCancellationRequested();
                    await _audio.PlayPreviewAsync(new StudioPreviewAudio(prepared.Pcm16, 44100, 2,
                        Session.BackingMix.Muted ? -61 : Session.BackingMix.VolumeDb), request.Token);
                    _metronomeEnabled = false; OnPropertyChanged(nameof(MetronomeEnabled));
                    _state = _audio.State;
                    result.IsPreviewing = true; result.Refresh();
                });
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (_previewRequest == request)
                {
                    result.WaveformError = T("Prévia indisponível") + ": " + ex.Message;
                    StatusMessage = result.WaveformError;
                    StopPreview();
                }
            }
            finally { result.IsPreparingWaveform = false; result.Refresh(); }
        }
    }
    public void StopPreview()
    {
        _hoverResult = null;
        var request = _previewRequest; _previewRequest = null;
        // A finished preparation has already disposed its CTS; playback still needs stopping.
        if (request != null) { try { request.Cancel(); } catch (ObjectDisposedException) { } }
        bool hadPreview = _previewResult != null || _state.IsPreviewPlaying;
        if (_previewResult is { } result)
        {
            result.IsPreparingWaveform = result.IsPreviewing = result.IsPinned = false; result.Refresh();
        }
        _previewResult = null;
        Downloader.StopYouTubePreview();
        if (hadPreview && !_disposed) Run(() => _audio.StopPreviewAsync(_lifetime.Token));
        OnPropertyChanged(nameof(PreviewResult)); OnPropertyChanged(nameof(PreviewProgress));
    }
    private void UpdateSessionPreview()
    {
        if (_previewResult is not { } result) return;
        if (_state.IsPreviewPlaying)
        {
            if (!result.IsPreviewing) { result.IsPreviewing = true; result.Refresh(); }
            result.UpdatePreviewPosition(_state.PreviewPosition);
            OnPropertyChanged(nameof(PreviewProgress));
        }
        else if (result.IsPreviewing) StopPreview();
    }
    internal async Task AddPlaybackAsync(PlaybackResultViewModel result)
    {
        if (!CanReplaceBacking) return;
        StopPreview();
        long intent = BeginBackingIntent(), generation = _sessionGeneration;
        if (Downloader.YouTubeDownloading && Downloader.YouTubeDownloadId != result.Video.Id)
            Downloader.CancelYouTubeDownload.Execute(null);
        while (Downloader.YouTubeDownloading && Downloader.GetDownloadedPath(result.Video.Id) == null)
        {
            await Task.Delay(50, _lifetime.Token);
            if (intent != _backingIntent || generation != _sessionGeneration || IsRecording || _closePending || _disposed) return;
        }
        if (intent != _backingIntent || generation != _sessionGeneration || IsRecording || _closePending || _disposed) return;
        string? path = await Downloader.GetOrDownloadForStudioAsync(result.Video);
        if (path == null || intent != _backingIntent || generation != _sessionGeneration || IsRecording || _closePending || _disposed) return;
        await LoadBackingAsync(path, intent, generation, discardWithoutPrompt: true);
        if (_media.TryGetValue(path, out var info))
        {
            result.Peaks = info.Peaks; result.HasFullWaveform = true; result.PreviewDuration = info.Duration;
            result.Bpm = info.Bpm; result.Key = info.Key; result.Refresh();
        }
    }
}
