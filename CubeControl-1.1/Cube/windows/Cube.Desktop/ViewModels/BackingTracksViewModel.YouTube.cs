using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Cube.Desktop.Services;
using Cube.Desktop.Localization;
using Microsoft.Win32;

namespace Cube.Desktop.ViewModels;

public sealed partial class BackingTracksViewModel
{
    internal IYouTubeAudioService YouTubeService { get; set; } = new YouTubeAudioService();
    private CancellationTokenSource? youtubeSearch, youtubeDownload;
    private string youtubeStatus = "", youtubeDownloadStatus = "";
    private bool youtubeSearching, youtubeDownloading;
    private double youtubePercent;
    private bool previewOnHover;
    public bool PreviewOnHover
    {
        get => previewOnHover;
        set
        {
            if (!Set(ref previewOnHover, value)) return;
            if (!value) StopYouTubePreview();
            try { SaveYouTubeDownloadSettings(YouTubeDownloadFolder); }
            catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException) { Log(ex); }
        }
    }
    public ObservableCollection<YouTubeResult> YouTubeResults { get; } = [];
    public ObservableCollection<YouTubeResult> PresentedYouTubeResults { get; } = [];
    private YouTubeResult? pinnedDownload;
    private string? downloadMessageId;
    private string youtubeFolderStatus = "";
    public string? YouTubeDownloadMessageId { get => downloadMessageId; private set => Set(ref downloadMessageId, value); }
    public string YouTubeFolderStatus { get => youtubeFolderStatus; private set => Set(ref youtubeFolderStatus, value); }
    private void RefreshYouTubePresentation()
    {
        var desired = (pinnedDownload == null ? YouTubeResults : YouTubeResults.Concat(new[] { pinnedDownload }))
            .DistinctBy(v => v.Id).ToArray();
        for (int i = PresentedYouTubeResults.Count - 1; i >= 0; i--)
            if (!desired.Any(v => v.Id == PresentedYouTubeResults[i].Id)) PresentedYouTubeResults.RemoveAt(i);
        for (int i = 0; i < desired.Length; i++)
        {
            int existing = -1;
            for (int j = i; j < PresentedYouTubeResults.Count; j++)
                if (PresentedYouTubeResults[j].Id == desired[i].Id) { existing = j; break; }
            if (existing < 0) PresentedYouTubeResults.Insert(i, desired[i]);
            else if (existing != i) PresentedYouTubeResults.Move(existing, i);
        }
        Notify(nameof(ShowYouTube));
    }
    private void ClearCompletedYouTubeDownload(bool keepRow = false)
    {
        if (YouTubeDownloading) return;
        if (!keepRow) pinnedDownload = null;
        YouTubeDownloadMessageId = null; YouTubeDownloadStatus = "";
        RefreshYouTubePresentation();
    }
    private string youtubeQuery = "";
    public string YouTubeQuery { get => youtubeQuery; set { if (Set(ref youtubeQuery, value)) SearchYouTube(); } }
    private bool appendBackingTrack = true;
    public bool AppendBackingTrack { get => appendBackingTrack; set { if (Set(ref appendBackingTrack, value)) SearchYouTube(); } }
    public bool ShowYouTube => YouTubeQuery.Trim().Length >= 3 || pinnedDownload != null;
    public bool YouTubeSearching { get => youtubeSearching; private set => Set(ref youtubeSearching, value); }
    public bool YouTubeDownloading { get => youtubeDownloading; private set { Set(ref youtubeDownloading, value); CommandManager.InvalidateRequerySuggested(); } }
    public string YouTubeStatus { get => youtubeStatus; private set => Set(ref youtubeStatus, value); }
    public string YouTubeDownloadStatus { get => youtubeDownloadStatus; private set => Set(ref youtubeDownloadStatus, value); }
    public double YouTubePercent { get => youtubePercent; private set => Set(ref youtubePercent, value); }
    public ICommand CancelYouTubeDownload => new UiCommand(() => youtubeDownload?.Cancel(), () => YouTubeDownloading);
    private async void SearchYouTube()
    {
        if (disposed) return;
        StopYouTubePreview();
        ClearCompletedYouTubeDownload();
        youtubeSearch?.Cancel();
        var request = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        youtubeSearch = request;
        var token = request.Token;
        string query = YouTubeQuery.Trim();
        Notify(nameof(ShowYouTube)); YouTubeResults.Clear(); YouTubeStatus = ""; YouTubeSearching = false;
        try
        {
            if (query.Length < 3) return;
            await Task.Delay(600, token);
            YouTubeSearching = true; YouTubeStatus = "Buscando faixas…";
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
            timeout.CancelAfter(TimeSpan.FromSeconds(45));
            var results = await YouTubeService.SearchAsync(AppendBackingTrack ? query + " backing track" : query, timeout.Token);
            if (token.IsCancellationRequested || disposed) return;
            foreach (var result in results) YouTubeResults.Add(result);
            YouTubeStatus = results.Count == 0 ? "Nenhuma faixa encontrada." : "";
        }
        catch (OperationCanceledException) { if (!token.IsCancellationRequested && !disposed) YouTubeStatus = "A busca demorou demais. Tente novamente."; }
        catch (Exception ex) { if (!token.IsCancellationRequested && !disposed) YouTubeStatus = YouTubeError(ex); }
        finally { if (youtubeSearch == request) { youtubeSearch = null; if (!disposed) YouTubeSearching = false; } request.Dispose(); }
    }
    private static string YouTubeError(Exception ex) => ex switch
    {
        DirectoryNotFoundException => "Pasta de downloads indisponível. Use Alterar pasta.",
        FileNotFoundException => "Ferramentas de áudio ausentes. Reinstale o aplicativo.",
        UnauthorizedAccessException => "Sem permissão para salvar o MP3. Escolha outra pasta.",
        IOException io when (io.HResult & 0xffff) is 39 or 112 => "Espaço insuficiente para salvar o MP3.",
        IOException io when io.Message == "Não foi possível converter o áudio para MP3." => io.Message,
        _ => "Faixa indisponível ou acesso restrito. Verifique a internet e tente novamente."
    };
    public async Task DownloadYouTube(YouTubeResult result)
    {
        if (YouTubeDownloading || choosingDownloadFolder || disposed) return;
        StopYouTubePreview();
        ClearCompletedYouTubeDownload(); YouTubeFolderStatus = "";
        if (YouTubeDownloadFolder.Length == 0 && !ChooseDownloadFolder()) return;
        try { VerifyDownloadFolder(YouTubeDownloadFolder); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { YouTubeFolderStatus = YouTubeDownloadStatus = "Pasta de downloads indisponível. Use Alterar pasta."; return; }
        await DownloadYouTubeTo(result, AvailableDownloadPath(YouTubeDownloadFolder, result.Title, result.Id));
    }
    internal async Task DownloadYouTubeTo(YouTubeResult result, string destination)
    {
        if (YouTubeDownloading || disposed) return;
        using var request = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        StopYouTubePreview();
        ClearCompletedYouTubeDownload(); YouTubeFolderStatus = "";
        pinnedDownload = result; YouTubeDownloadMessageId = result.Id;
        YouTubeDownloadId = result.Id;
        YouTubeDownloadTitle = result.Title;
        youtubeDownload = request; YouTubeDownloading = true; YouTubePercent = 0; YouTubeDownloadStatus = "Baixando áudio…";
        RefreshYouTubePresentation();
        try
        {
            await YouTubeService.DownloadAsync(result, destination, new Progress<YouTubeProgress>(p =>
            { if (!disposed && youtubeDownload == request && !request.IsCancellationRequested) { YouTubeDownloadStatus = p.Stage; YouTubePercent = Math.Clamp(p.Percent, 0, 100); } }), request.Token);
            request.Token.ThrowIfCancellationRequested();
            if (!disposed) { RememberYouTubeDownload(result.Id, destination); YouTubeDownloadStatus = "MP3 salvo."; }
        }
        catch (OperationCanceledException) { if (!disposed) YouTubeDownloadStatus = "Download cancelado."; }
        catch (Exception ex) { if (!disposed) YouTubeDownloadStatus = YouTubeError(ex); }
        finally { YouTubeDownloadId = null; youtubeDownload = null; if (!disposed) YouTubeDownloading = false; }
    }
}
