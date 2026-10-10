using System.IO;
using Cube.Desktop.Services;
using Cube.Desktop.ViewModels;

internal static class YouTubeDownloadTests
{
    private static void Check(bool value, string message)
    { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); }
    private sealed class Fake : IYouTubeAudioService
    {
        public List<string> Destinations { get; } = [];
        public bool Wait, Fail;
        public IProgress<YouTubeProgress>? LastProgress;
        public IReadOnlyList<YouTubeResult> Results = [new("abcdefghijk", "Online", "Channel", 30, null)];
        public Task<IReadOnlyList<YouTubeResult>> SearchAsync(string query, CancellationToken cancellation)
            => Task.FromResult(Results);
        public async Task DownloadAsync(YouTubeResult video, string destination, IProgress<YouTubeProgress> progress, CancellationToken cancellation)
        {
            LastProgress = progress;
            Destinations.Add(destination);
            if (Fail) throw new IOException("fixture failure");
            if (Wait) await Task.Delay(Timeout.Infinite, cancellation);
            await File.WriteAllTextAsync(destination, "download fixture", cancellation);
        }
    }
    private static async Task RunPresentation(string root)
    {
        using var model = new BackingTracksViewModel(Path.Combine(root, "download-presentation"));
        var first = new YouTubeResult("abcdefghijk", "First", "Channel", 30, null);
        var second = first with { Id = "12345678901", Title = "Second" };
        var fake = new Fake { Wait = true, Results = [second, first] };
        model.YouTubeService = fake;
        model.YouTubeResults.Add(second); model.YouTubeResults.Add(first);
        var task = model.DownloadYouTubeTo(first, Path.Combine(root, "presentation.mp3"));
        Check(model.PresentedYouTubeResults.Count == 2 && model.PresentedYouTubeResults[1].Id == first.Id, "active download retains its original position");
        Check(model.YouTubeResults[0] == second, "presentation does not reorder search results");
        model.YouTubeQuery = "another search"; await Task.Delay(700);
        Check(model.PresentedYouTubeResults.Count == 2 && model.PresentedYouTubeResults[1].Id == first.Id, "new results preserve search order without duplicates");
        fake.Results = []; model.YouTubeQuery = "empty search"; await Task.Delay(700);
        Check(model.YouTubeResults.Count == 0 && model.PresentedYouTubeResults.Single().Id == first.Id, "empty results retain active download");
        model.YouTubeQuery = "";
        Check(model.ShowYouTube && model.PresentedYouTubeResults.Single().Id == first.Id, "clearing search keeps active row visible");
        var lateProgress = fake.LastProgress!;
        model.CancelYouTubeDownload.Execute(null); await task;
        string terminal = model.YouTubeDownloadStatus;
        lateProgress.Report(new("late progress", 90)); await Task.Delay(30);
        Check(model.YouTubeDownloadId == null && model.YouTubeDownloadMessageId == first.Id && model.YouTubeDownloadStatus == terminal && model.ShowYouTube, "cancellation keeps terminal identity and ignores late callbacks");
        model.YouTubeQuery = "x";
        Check(model.PresentedYouTubeResults.Count == 0 && model.YouTubeDownloadMessageId == null && !model.ShowYouTube, "next query releases completed pinned row");
        fake.Wait = false; fake.Fail = true;
        await model.DownloadYouTubeTo(first, Path.Combine(root, "failure.mp3"));
        Check(!model.YouTubeDownloading && model.YouTubeDownloadMessageId == first.Id && model.YouTubeDownloadStatus.Length > 0, "failure remains attached to source row");
        fake.Fail = false;
        await model.DownloadYouTubeTo(second, Path.Combine(root, "success.mp3"));
        Check(model.PresentedYouTubeResults.Single().Id == second.Id && model.YouTubeDownloadMessageId == second.Id && model.Tracks.Count == 1, "new download replaces old terminal row and success imports track");
        var card = new Cube.Desktop.Controls.YouTubeResultCard { Model = model, DataContext = second };
        Check(card.IsDownloaded && card.DownloadIcon == Wpf.Ui.Controls.SymbolRegular.CheckmarkCircle24, "successful download uses completion icon");
        Check(!model.IsYouTubeDownloaded(first.Id), "failed or cancelled download is not marked complete");
        string success = model.YouTubeDownloadStatus;
        model.SelectYouTubeDownloadFolder = _ => Path.Combine(root, "nonexistent-folder");
        model.ChangeYouTubeDownloadFolder.Execute(null);
        Check(model.YouTubeFolderStatus.Length > 0 && model.YouTubeDownloadStatus == success, "folder failure has separate feedback without overwriting completed row");
        model.PageVisible = true; model.SelectedOutput = null;
        await model.PreviewYouTube(second, true);
        Check(model.PresentedYouTubeResults.Single().Id == second.Id && model.YouTubeDownloadMessageId == null, "preview clears final message without removing its pinned card");
    }

    internal static async Task Run(string root)
    {
        await RunPresentation(root);
        string data = Path.Combine(root, "settings"), first = Path.Combine(root, "downloads"), second = Path.Combine(root, "other-downloads");
        Directory.CreateDirectory(first); Directory.CreateDirectory(second);
        var video = new YouTubeResult("abcdefghijk", "Track: title", "Channel", 30, null);
        var fake = new Fake(); int selections = 0;
        using (var model = new BackingTracksViewModel(data))
        {
            model.YouTubeService = fake;
            model.SelectYouTubeDownloadFolder = _ => { selections++; return null; };
            await model.DownloadYouTube(video);
            Check(fake.Destinations.Count == 0 && model.YouTubeDownloadFolder.Length == 0, "cancelled initial picker never downloads");
            model.SelectYouTubeDownloadFolder = _ => { selections++; return first; };
            await model.DownloadYouTube(video); await model.DownloadYouTube(video);
            Check(selections == 2 && fake.Destinations.Count == 2, "only the first download asks for a folder");
            Check(Path.GetFileName(fake.Destinations[0]) == "Track_ title.mp3" && Path.GetFileName(fake.Destinations[1]) == "Track_ title (2).mp3", "duplicate names use a suffix and preserve earlier download");
            Check(model.Tracks.Count == 2 && !model.IsPlaying, "completed downloads import without autoplay");
            model.SelectYouTubeDownloadFolder = _ => null;
            model.ChangeYouTubeDownloadFolder.Execute(null);
            Check(model.YouTubeDownloadFolder == first, "cancelling folder change retains destination");
            fake.Wait = true;
            var download = model.DownloadYouTube(video);
            Check(model.YouTubeDownloading && !model.ChangeYouTubeDownloadFolder.CanExecute(null), "folder change disabled while downloading");
            model.CancelYouTubeDownload.Execute(null); await download;
            Check(!File.Exists(fake.Destinations[^1]) && model.Tracks.Count == 2, "cancelled download does not import or leave a file");
            fake.Wait = false;
            model.Search = "does not match"; model.YouTubeQuery = "online"; await Task.Delay(700);
            Check(model.FilteredTracks.IsEmpty && model.YouTubeResults.Count == 1, "online search does not change playlist filter");
            model.Search = "";
            Check(!model.FilteredTracks.IsEmpty && model.YouTubeResults.Count == 1, "playlist filter does not clear YouTube results");
        }
        using (var model = new BackingTracksViewModel(data))
        {
            model.YouTubeService = fake;
            model.SelectYouTubeDownloadFolder = _ => throw new Exception("Unexpected folder picker");
            Check(model.YouTubeDownloadFolder == first, "folder survives model restart");
            Check(model.IsYouTubeDownloaded(video.Id), "completed download survives model restart");
            await model.DownloadYouTube(video);
            Check(Path.GetFileName(fake.Destinations[^1]) == "Track_ title (3).mp3", "restart downloads directly to remembered folder");
            model.SelectYouTubeDownloadFolder = _ => second;
            model.ChangeYouTubeDownloadFolder.Execute(null);
            await model.DownloadYouTube(video with { Title = "CON" });
            Check(Path.GetDirectoryName(fake.Destinations[^1]) == second && Path.GetFileName(fake.Destinations[^1]) == "_CON.mp3", "changed folder and Windows reserved names are usable");
            File.Delete(fake.Destinations[^1]); Directory.Delete(second);
            int calls = fake.Destinations.Count;
            await model.DownloadYouTube(video);
            Check(fake.Destinations.Count == calls && model.YouTubeDownloadStatus.Contains("Alterar pasta"), "missing folder reports recovery instead of silently changing destination");
        }
        using (var model = new BackingTracksViewModel(data))
            Check(model.YouTubeDownloadFolder == second, "changed folder remains persisted even when unavailable");
        File.WriteAllText(Path.Combine(data, "youtube-downloads-v1.json"), "invalid");
        using (var model = new BackingTracksViewModel(data))
            Check(model.YouTubeDownloadFolder.Length == 0, "malformed settings safely return to first-folder selection");
    }
}
