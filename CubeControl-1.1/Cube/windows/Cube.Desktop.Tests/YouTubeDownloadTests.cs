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
        public bool Wait;
        public Task<IReadOnlyList<YouTubeResult>> SearchAsync(string query, CancellationToken cancellation)
            => Task.FromResult<IReadOnlyList<YouTubeResult>>([new("abcdefghijk", "Online", "Channel", 30, null)]);
        public async Task DownloadAsync(YouTubeResult video, string destination, IProgress<YouTubeProgress> progress, CancellationToken cancellation)
        {
            Destinations.Add(destination);
            if (Wait) await Task.Delay(Timeout.Infinite, cancellation);
            await File.WriteAllTextAsync(destination, "download fixture", cancellation);
        }
    }
    internal static async Task Run(string root)
    {
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
