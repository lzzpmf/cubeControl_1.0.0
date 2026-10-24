using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Cube.Desktop.Studio.ViewModels;

public sealed partial class ShellViewModel
{
    private PlaybackResultViewModel? _previewResult, _hoverResult, _preparingResult;
    private CancellationTokenSource? _wavePreparation;
    private double _hoverSince;
    public ObservableCollection<PlaybackResultViewModel> Results { get; } = [];
    public string SearchText { get => Downloader.YouTubeQuery; set { StopPreview(); Downloader.YouTubeQuery = value; OnPropertyChanged(); } }
    public string ResultSummary => Downloader.YouTubeSearching ? T("Buscando faixas…") : $"{Results.Count} {T("resultados")}";
    public string NoResultsMessage => T(Downloader.YouTubeStatus.Length > 0 ? Downloader.YouTubeStatus : Results.Count == 0 ? "Busque uma backing para começar" : "");
    public bool HasResults => Results.Count > 0;
    public PlaybackResultViewModel? PreviewResult => _previewResult;
    public double PreviewProgress => _previewResult?.PreviewProgress ?? 0;
    public string PreviewStatus => T(Downloader.YouTubePreviewStatus);
    public string DownloadStatusText => T(Downloader.YouTubeDownloadStatus);
    private void ResultsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CancelWaveformPreparation();
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
            result.IsPreviewing = Downloader.YouTubePreviewId == result.Video.Id && Downloader.YouTubePreviewActive;
            if (Downloader.GetYouTubeWave(result.Video.Id) is { } wave)
            {
                result.PreviewPosition = wave.Position;
                if (!result.HasFullWaveform) { result.Peaks = wave.Peaks; result.PreviewDuration = wave.Duration; }
            }
            result.IsDownloading = Downloader.YouTubeDownloadId == result.Video.Id;
            result.DownloadStatus = Downloader.YouTubeDownloadMessageId == result.Video.Id ? T(Downloader.YouTubeDownloadStatus) : Downloader.IsYouTubeDownloaded(result.Video.Id) ? T("MP3 salvo.") : "";
            result.DownloadProgress = result.IsDownloading ? Downloader.YouTubePercent : 0;
            result.Refresh();
        }
        foreach (var name in new[] { nameof(ResultSummary), nameof(NoResultsMessage), nameof(HasResults), nameof(PreviewResult), nameof(PreviewProgress), nameof(PreviewStatus), nameof(DownloadStatusText) }) OnPropertyChanged(name);
    }
    public void PreviewHover(PlaybackResultViewModel result, bool entered)
    {
        if (entered)
        {
            if (!CanPreviewPlayback || result.IsPreviewing) return;
            CancelWaveformPreparation(); _hoverResult = result; _hoverSince = _tapClock.Elapsed.TotalSeconds;
        }
        else
        {
            if (_hoverResult == result) _hoverResult = null;
            if (_preparingResult == result) CancelWaveformPreparation();
        }
    }
    private async Task PrepareWaveformAsync(PlaybackResultViewModel result)
    {
        if (!CanPreviewPlayback || !PageVisible || !Results.Contains(result)) return;
        bool downloaded = Downloader.GetDownloadedPath(result.Video.Id) is { };
        if (result.Peaks.Length > 0 && (!downloaded || result.HasFullWaveform)) return;
        CancelWaveformPreparation();
        using var preparation = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
        _wavePreparation = preparation; _preparingResult = result;
        result.IsPreparingWaveform = true; result.WaveformError = null; result.Refresh();
        try
        {
            if (Downloader.GetDownloadedPath(result.Video.Id) is { } path)
            {
                var media = await _audio.AnalyzeAsync(path, preparation.Token);
                preparation.Token.ThrowIfCancellationRequested();
                if (!Results.Contains(result) || !PageVisible) return;
                result.Peaks = media.Peaks; result.PreviewDuration = media.Duration;
                result.Bpm = media.Bpm; result.Key = media.Key; result.HasFullWaveform = true;
            }
            else
            {
                var wave = await Downloader.PrepareYouTubePreviewAsync(result.Video, preparation.Token);
                preparation.Token.ThrowIfCancellationRequested();
                if (!Results.Contains(result) || !PageVisible) return;
                result.Peaks = wave.Peaks; result.PreviewDuration = wave.Duration;
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception) { if (!preparation.IsCancellationRequested) result.WaveformError = T("Prévia indisponível"); }
        finally
        {
            if (_wavePreparation == preparation) { _wavePreparation = null; _preparingResult = null; }
            result.IsPreparingWaveform = false; result.Refresh();
        }
    }
    private void CancelWaveformPreparation() { _hoverResult = null; _wavePreparation?.Cancel(); }
    private void TogglePreview(PlaybackResultViewModel result)
    {
        if (!CanPreviewPlayback) return;
        if (_previewResult == result && Downloader.YouTubePreviewActive) { StopPreview(); return; }
        StartPreview(result);
    }
    private void StartPreview(PlaybackResultViewModel result)
    {
        if (!CanPreviewPlayback || !PageVisible) return;
        StopPreview(); _previewResult = result; result.IsPinned = true; result.PreviewPosition = 0; result.Refresh();
        Run(() => Downloader.PreviewYouTube(result.Video, immediate: true));
    }
    public void StopPreview()
    {
        CancelWaveformPreparation(); Downloader.StopYouTubePreview();
        if (_previewResult is { } result) { result.IsPreviewing = result.IsPinned = false; result.Refresh(); }
        _previewResult = null;
        OnPropertyChanged(nameof(PreviewResult)); OnPropertyChanged(nameof(PreviewProgress));
    }
    internal async Task AddPlaybackAsync(PlaybackResultViewModel result)
    {
        if (!CanReplaceBacking) return;
        long intent = BeginBackingIntent(), generation = _sessionGeneration;
        // One downloader is retained, with its folder/history/cancellation. A newer
        // backing choice waits for that download, while invalidating the older load intent.
        while (Downloader.YouTubeDownloading && Downloader.GetDownloadedPath(result.Video.Id) == null)
        {
            await Task.Delay(100, _lifetime.Token);
            if (intent != _backingIntent || generation != _sessionGeneration || IsRecording || _closePending || _disposed) return;
        }
        if (intent != _backingIntent || generation != _sessionGeneration || IsRecording || _closePending || _disposed) return;
        string? path = await Downloader.GetOrDownloadForStudioAsync(result.Video);
        if (path == null || intent != _backingIntent || generation != _sessionGeneration || IsRecording || _closePending || _disposed) return;
        await LoadBackingAsync(path, intent, generation);
        if (_media.TryGetValue(path, out var info))
        { result.Peaks = info.Peaks; result.HasFullWaveform = true; result.PreviewDuration = info.Duration; result.Bpm = info.Bpm; result.Key = info.Key; result.Refresh(); }
    }
}
