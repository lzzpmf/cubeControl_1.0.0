using Cube.Desktop.Services;

namespace Cube.Desktop.ViewModels;

public sealed partial class BackingTracksViewModel
{
    internal IYouTubePreviewService YouTubePreviewService { get; set; } = new YouTubePreviewService();
    private CancellationTokenSource? youtubePreview;
    private Task? previewWorker;
    private TaskCompletionSource? previewPromotion;
    private readonly Dictionary<string, YouTubePreviewFrame> previewWaves = [];
    private readonly LinkedList<string> previewWaveOrder = new();
    internal YouTubePreviewFrame? GetYouTubeWave(string? id) => id != null && previewWaves.TryGetValue(id, out var frame) ? frame : null;
    internal Action<string, double>? PreviewTiming { get; set; }
    private void UpdatePreviewWave(string id, YouTubePreviewFrame frame)
    {
        // Retain already decoded peaks when replaying the same excerpt.
        if (previewWaves.TryGetValue(id, out var previous) && previous.Peaks.Length == frame.Peaks.Length)
            for (int i = 0; i < frame.Peaks.Length; i++)
                if (float.IsNaN(frame.Peaks[i])) frame.Peaks[i] = previous.Peaks[i];
        previewWaves[id] = frame;
        previewWaveOrder.Remove(id); previewWaveOrder.AddLast(id);
        if (previewWaveOrder.Count > 20)
        { previewWaves.Remove(previewWaveOrder.First!.Value); previewWaveOrder.RemoveFirst(); }
        Notify("YouTubeWave");
    }
    private string? youtubePreviewId, youtubeDownloadId;
    private string youtubeDownloadTitle = "";
    private string youtubePreviewStatus = "";
    private bool youtubePreviewActive;
    private float previewGain = .3f;
    public string? YouTubePreviewId { get => youtubePreviewId; private set => Set(ref youtubePreviewId, value); }
    public string? YouTubeDownloadId { get => youtubeDownloadId; private set => Set(ref youtubeDownloadId, value); }
    public string YouTubeDownloadTitle { get => youtubeDownloadTitle; private set => Set(ref youtubeDownloadTitle, value); }
    public string YouTubePreviewStatus { get => youtubePreviewStatus; private set => Set(ref youtubePreviewStatus, value); }
    public bool YouTubePreviewActive { get => youtubePreviewActive; private set => Set(ref youtubePreviewActive, value); }
    internal void UpdateYouTubePreviewVolume() => Volatile.Write(ref previewGain, IsMuted ? 0 : (float)(Volume / 100));

    public void StopYouTubePreview(string? videoId = null)
    {
        if (videoId != null && YouTubePreviewId != videoId) return;
        var request = youtubePreview; youtubePreview = null;
        request?.Cancel();
        YouTubePreviewActive = false; YouTubePreviewStatus = ""; YouTubePreviewId = null;
    }

    private async Task StopYouTubePreviewAndWait()
    {
        var worker = previewWorker;
        StopYouTubePreview();
        if (worker != null)
            try { await worker; } catch (Exception ex) { if (ex is not OperationCanceledException) Log(ex); }
    }

    public async Task PreviewYouTube(YouTubeResult video, bool immediate = false)
    {
        if (immediate && youtubePreview != null && YouTubePreviewId == video.Id)
        { previewPromotion?.TrySetResult(); return; }
        StopYouTubePreview();
        if (disposed || !PageVisible || YouTubeDownloading) return;
        ClearCompletedYouTubeDownload(keepRow: true);
        YouTubePreviewId = video.Id;
        if (SelectedOutput == null) { YouTubePreviewStatus = "Escolha a saída de áudio"; return; }
        using var request = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        youtubePreview = request;
        var promotion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        previewPromotion = promotion;
        var dwell = System.Diagnostics.Stopwatch.StartNew();
        Task? worker = null;
        try
        {
            if (!immediate) await Task.WhenAny(Task.Delay(250, request.Token), promotion.Task).WaitAsync(request.Token);
            PreviewTiming?.Invoke("hover", dwell.Elapsed.TotalMilliseconds);
            request.Token.ThrowIfCancellationRequested();
            if (IsLoading || !PageVisible || SelectedOutput == null) return;
            YouTubePreviewActive = true;
            YouTubePreviewStatus = "Preparando prévia…";
            UpdateYouTubePreviewVolume();
            worker = YouTubePreviewService.PlayAsync(video, SelectedOutput.Id, () => Volatile.Read(ref previewGain),
                token => dispatcher.InvokeAsync(async () =>
                {
                    token.ThrowIfCancellationRequested();
                    if (youtubePreview != request || disposed || IsLoading) throw new OperationCanceledException(token);
                    if (BeforePlayback != null) await BeforePlayback();
                    token.ThrowIfCancellationRequested();
                    if (IsPlaying)
                    {
                        await engine.Pause();
                        token.ThrowIfCancellationRequested();
                        UpdatePosition();
                        State = BackingPlaybackState.Paused;
                        if (SelectedTrack != null) SelectedTrack.State = "Pausada";
                        positionTimer.Stop();
                    }
                }, System.Windows.Threading.DispatcherPriority.Normal, token).Task.Unwrap(),
                () => Dispatch(() =>
                {
                    if (youtubePreview == request && !request.IsCancellationRequested)
                        YouTubePreviewStatus = "Pré-escuta · até 30 s";
                }), request.Token, frame => Dispatch(() =>
                {
                    if (youtubePreview == request && !request.IsCancellationRequested)
                        UpdatePreviewWave(video.Id, frame);
                }));
            previewWorker = worker;
            backgroundWork.Add(worker);
            await worker;
            if (youtubePreview == request && !disposed) YouTubePreviewStatus = "Prévia concluída";
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (youtubePreview == request && !disposed)
                YouTubePreviewStatus = ex is TimeoutException ? "A prévia demorou demais. Tente novamente." : "Prévia indisponível. Tente novamente.";
        }
        finally
        {
            if (worker != null) backgroundWork.Remove(worker);
            if (previewWorker == worker) previewWorker = null;
            if (youtubePreview == request) { previewPromotion = null; youtubePreview = null; YouTubePreviewActive = false; }
        }
    }
}
