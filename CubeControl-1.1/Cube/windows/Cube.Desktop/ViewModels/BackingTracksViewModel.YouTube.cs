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
    public bool ShowYouTube => Search.Trim().Length >= 3;
    public bool YouTubeSearching { get => youtubeSearching; private set => Set(ref youtubeSearching, value); }
    public bool YouTubeDownloading { get => youtubeDownloading; private set { Set(ref youtubeDownloading, value); CommandManager.InvalidateRequerySuggested(); } }
    public string YouTubeStatus { get => youtubeStatus; private set => Set(ref youtubeStatus, value); }
    public string YouTubeDownloadStatus { get => youtubeDownloadStatus; private set => Set(ref youtubeDownloadStatus, value); }
    public double YouTubePercent { get => youtubePercent; private set => Set(ref youtubePercent, value); }
    public ICommand CancelYouTubeDownload => new UiCommand(() => youtubeDownload?.Cancel(), () => YouTubeDownloading);
    private async void SearchYouTube()
    {
        youtubeSearch?.Cancel();
        var request = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        youtubeSearch = request;
        var token = request.Token;
        string query = Search.Trim();
        Notify(nameof(ShowYouTube)); YouTubeResults.Clear(); YouTubeStatus = ""; YouTubeSearching = false;
        try
        {
            if (query.Length < 3) return;
            await Task.Delay(600, token);
            YouTubeSearching = true; YouTubeStatus = "Buscando no YouTube…";
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
            timeout.CancelAfter(TimeSpan.FromSeconds(45));
            var results = await YouTubeService.SearchAsync(query, timeout.Token);
            if (token.IsCancellationRequested || disposed) return;
            foreach (var result in results) YouTubeResults.Add(result);
            YouTubeStatus = results.Count == 0 ? "Nenhum vídeo encontrado." : "";
        }
        catch (OperationCanceledException) { if (!token.IsCancellationRequested && !disposed) YouTubeStatus = "A busca demorou demais. Tente novamente."; }
        catch (Exception ex) { if (!token.IsCancellationRequested && !disposed) YouTubeStatus = YouTubeError(ex); }
        finally { if (youtubeSearch == request) { youtubeSearch = null; if (!disposed) YouTubeSearching = false; } request.Dispose(); }
    }
    private static string YouTubeError(Exception ex) => ex switch
    {
        FileNotFoundException => "Ferramentas do YouTube ausentes. Reinstale o aplicativo.",
        UnauthorizedAccessException => "Sem permissão para salvar o MP3. Escolha outra pasta.",
        IOException io when (io.HResult & 0xffff) is 39 or 112 => "Espaço insuficiente para salvar o MP3.",
        IOException io when io.Message == "Não foi possível converter o áudio para MP3." => io.Message,
        _ => "YouTube indisponível ou vídeo restrito. Verifique a internet e tente novamente."
    };
    public async Task DownloadYouTube(YouTubeResult result)
    {
        if (YouTubeDownloading || disposed) return;
        string name = string.Concat(result.Title.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)).Trim().TrimEnd('.');
        if (name.Length > 120) name = name[..120];
        var dialog = new SaveFileDialog { FileName = (name.Length == 0 ? result.Id : name) + ".mp3", Filter = "MP3|*.mp3", DefaultExt = ".mp3", AddExtension = true, OverwritePrompt = true };
        if (dialog.ShowDialog() != true) return;
        await DownloadYouTubeTo(result, dialog.FileName);
    }
    internal async Task DownloadYouTubeTo(YouTubeResult result, string destination)
    {
        if (YouTubeDownloading || disposed) return;
        using var request = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        youtubeDownload = request; YouTubeDownloading = true; YouTubePercent = 0; YouTubeDownloadStatus = "Baixando áudio…";
        try
        {
            await YouTubeService.DownloadAsync(result, destination, new Progress<YouTubeProgress>(p =>
            { if (!disposed && youtubeDownload == request) { YouTubeDownloadStatus = p.Stage; YouTubePercent = Math.Clamp(p.Percent, 0, 100); } }), request.Token);
            if (!disposed) { AddPaths([destination]); YouTubeDownloadStatus = "MP3 salvo e adicionado à playlist."; }
        }
        catch (OperationCanceledException) { if (!disposed) YouTubeDownloadStatus = "Download cancelado."; }
        catch (Exception ex) { if (!disposed) YouTubeDownloadStatus = YouTubeError(ex); }
        finally { youtubeDownload = null; if (!disposed) YouTubeDownloading = false; }
    }
}
