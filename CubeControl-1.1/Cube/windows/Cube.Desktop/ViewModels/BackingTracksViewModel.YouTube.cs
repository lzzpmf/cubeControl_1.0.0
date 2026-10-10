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
    public ObservableCollection<YouTubeResult> YouTubeResults { get; } = [];
    private string youtubeQuery = "";
    public string YouTubeQuery { get => youtubeQuery; set { if (Set(ref youtubeQuery, value)) SearchYouTube(); } }
    private bool appendBackingTrack = true;
    public bool AppendBackingTrack { get => appendBackingTrack; set { if (Set(ref appendBackingTrack, value)) SearchYouTube(); } }
    public bool ShowYouTube => YouTubeQuery.Trim().Length >= 3;
    public bool YouTubeSearching { get => youtubeSearching; private set => Set(ref youtubeSearching, value); }
    public bool YouTubeDownloading { get => youtubeDownloading; private set { Set(ref youtubeDownloading, value); CommandManager.InvalidateRequerySuggested(); } }
    public string YouTubeStatus { get => youtubeStatus; private set => Set(ref youtubeStatus, value); }
    public string YouTubeDownloadStatus { get => youtubeDownloadStatus; private set => Set(ref youtubeDownloadStatus, value); }
    public double YouTubePercent { get => youtubePercent; private set => Set(ref youtubePercent, value); }
    public ICommand CancelYouTubeDownload => new UiCommand(() => youtubeDownload?.Cancel(), () => YouTubeDownloading);
    private async void SearchYouTube()
    {
        StopYouTubePreview();
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
        if (YouTubeDownloadFolder.Length == 0 && !ChooseDownloadFolder()) return;
        try { VerifyDownloadFolder(YouTubeDownloadFolder); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { YouTubeDownloadStatus = "Pasta de downloads indisponível. Use Alterar pasta."; return; }
        await DownloadYouTubeTo(result, AvailableDownloadPath(YouTubeDownloadFolder, result.Title, result.Id));
    }
    internal async Task DownloadYouTubeTo(YouTubeResult result, string destination)
    {
        if (YouTubeDownloading || disposed) return;
        using var request = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        StopYouTubePreview();
        YouTubeDownloadId = result.Id;
        YouTubeDownloadTitle = result.Title;
        youtubeDownload = request; YouTubeDownloading = true; YouTubePercent = 0; YouTubeDownloadStatus = "Baixando áudio…";
        try
        {
            await YouTubeService.DownloadAsync(result, destination, new Progress<YouTubeProgress>(p =>
            { if (!disposed && youtubeDownload == request) { YouTubeDownloadStatus = p.Stage; YouTubePercent = Math.Clamp(p.Percent, 0, 100); } }), request.Token);
            if (!disposed) { AddPaths([destination]); YouTubeDownloadStatus = "MP3 salvo e adicionado à playlist."; }
        }
        catch (OperationCanceledException) { if (!disposed) YouTubeDownloadStatus = "Download cancelado."; }
        catch (Exception ex) { if (!disposed) YouTubeDownloadStatus = YouTubeError(ex); }
        finally { YouTubeDownloadId = null; youtubeDownload = null; if (!disposed) YouTubeDownloading = false; }
    }
}
