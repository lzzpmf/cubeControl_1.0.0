using System.IO;
using Cube.Desktop.Services;
using Cube.Desktop.ViewModels;

internal static class YouTubeTests
{
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); }
    private sealed class Fake : IYouTubeAudioService
    {
        public List<(string Query, TaskCompletionSource<IReadOnlyList<YouTubeResult>> Result)> Calls = [];
        public Task<IReadOnlyList<YouTubeResult>> SearchAsync(string query, CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<IReadOnlyList<YouTubeResult>>(); Calls.Add((query, completion)); return completion.Task;
        }
        public Task DownloadAsync(YouTubeResult video, string destination, IProgress<YouTubeProgress> progress, CancellationToken cancellation) => Task.CompletedTask;
    }
    public static async Task Run()
    {
        string folder = Path.Combine(Path.GetTempPath(), "cube-youtube-tests-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        try
        {
            using var model = new BackingTracksViewModel(folder);
            var fake = new Fake(); model.YouTubeService = fake;
            model.Search = "ab"; await Task.Delay(650); Check(fake.Calls.Count == 0, "short query stays local");
            model.Search = "blues"; await Task.Delay(200); model.Search = "jazz"; await Task.Delay(650);
            Check(fake.Calls.Count == 1 && fake.Calls[0].Query == "jazz", "600ms debounce cancels previous query");
            model.Search = "rock"; await Task.Delay(650);
            var video = new YouTubeResult("abcdefghijk", "Track", "Channel", 60, null);
            fake.Calls[1].Result.SetResult([video]); await Task.Delay(30);
            fake.Calls[0].Result.SetResult([]); await Task.Delay(30);
            Check(model.YouTubeResults.Count == 1, "stale response cannot replace current results");
            model.Search = ""; Check(model.YouTubeResults.Count == 0 && !model.ShowYouTube, "clearing search removes online results");
            model.Search = "offline"; await Task.Delay(650); fake.Calls[2].Result.SetException(new IOException("offline")); await Task.Delay(30);
            Check(!model.YouTubeSearching && model.YouTubeStatus.Length > 0, "offline search ends with a recoverable message");
            model.Search = "";
            foreach (string tool in new[] { "yt-dlp", "ffmpeg", "ffprobe", "deno" }) File.WriteAllText(Path.Combine(folder, tool + ".exe"), "fixture");
            var service = new YouTubeAudioService(folder);
            service.ProcessRunner = (_, _, _, _) => Task.FromResult("{\"entries\":[{\"id\":\"abcdefghijk\",\"title\":\"Unknown duration\",\"duration\":null},{\"id\":\"12345678901\",\"live_status\":\"is_live\"}]}");
            var parsed = await service.SearchAsync("fixture", CancellationToken.None);
            Check(parsed.Count == 1 && parsed[0].Duration == 0, "search handles missing duration and excludes live streams");
            string destination = Path.Combine(folder, "test & ' ç.mp3"); File.WriteAllText(destination, "original");
            bool fail = true, cancel = false;
            service.ProcessRunner = (exe, args, token, progress) =>
            {
                var arguments = args.ToArray();
                if (exe.EndsWith("ffprobe.exe")) return Task.FromResult("{\"streams\":[{\"codec_name\":\"mp3\",\"bit_rate\":\"320000\"}],\"format\":{\"duration\":\"10\"}}");
                Check(arguments.Contains("320K") && arguments.Contains("bestaudio/best") && arguments.Contains("--ignore-config"), "best source and 320K conversion isolated from user config");
                string pattern = arguments[Array.IndexOf(arguments, "-o") + 1];
                File.WriteAllText(pattern.Replace("%(ext)s", "mp3"), "converted");
                if (cancel) throw new OperationCanceledException();
                if (fail) throw new IOException("conversion failed");
                return Task.FromResult("");
            };
            try { await service.DownloadAsync(video, destination, new Progress<YouTubeProgress>(), CancellationToken.None); } catch (IOException) { }
            Check(File.ReadAllText(destination) == "original" && !Directory.EnumerateDirectories(folder, ".cube-youtube-*").Any(), "failure preserves destination and cleans temporary files");
            fail = false; cancel = true;
            try { await service.DownloadAsync(video, destination, new Progress<YouTubeProgress>(), CancellationToken.None); } catch (OperationCanceledException) { }
            Check(File.ReadAllText(destination) == "original" && !Directory.EnumerateDirectories(folder, ".cube-youtube-*").Any(), "cancellation preserves destination and cleans temporary files");
            cancel = false; await service.DownloadAsync(video, destination, new Progress<YouTubeProgress>(), CancellationToken.None);
            Check(File.ReadAllText(destination) == "converted", "successful validation replaces destination with special characters");
            model.YouTubeService = service;
            await model.DownloadYouTubeTo(video, destination); await model.DownloadYouTubeTo(video, destination);
            Check(model.Tracks.Count == 1 && !model.IsPlaying, "download imports once without autoplay");
            using var editor = new EditorViewModel(Path.Combine(folder, "ui"));
            var window = new Cube.Desktop.MainWindow(editor, initializeDevices: false) { Width = 1280, Height = 900 };
            window.Show();
            try
            {
                ((System.Windows.Controls.TabControl)window.FindName("Tabs")).SelectedIndex = 2;
                var uiFake = new Fake(); window.BackingTracks.YouTubeService = uiFake;
                window.BackingTracks.Search = "blues"; await Task.Delay(650);
                uiFake.Calls[0].Result.SetResult([video with { Title = "Backing track with a long title for responsive layout verification", Channel = "Test channel" }]);
                ((System.Windows.Controls.Expander)window.FindName("BackingPlaylistToggle")).IsExpanded = true;
                string reports = "C:/CubeControl-Build/youtube-validation/reports"; Directory.CreateDirectory(reports);
                foreach (var theme in new[] { "Dark", "Light", "HighContrast" })
                    foreach (int width in new[] { 1280, 640 })
                    {
                        if (theme == "HighContrast") Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.HighContrast);
                        else Cube.Desktop.Appearance.ThemeService.Instance.Preview(Enum.Parse<Cube.Desktop.Appearance.ThemeMode>(theme));
                        window.Width = width;
                        await Task.Delay(150); window.UpdateLayout();
                        ((System.Windows.Controls.ScrollViewer)window.FindName("BackingTracksScroll")).ScrollToEnd(); await Task.Delay(100);
                        var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                        bitmap.Render(window); var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder(); encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                        using var image = File.Create(Path.Combine(reports, $"youtube-{theme}-{width}.png")); encoder.Save(image);
                        Check(window.BackingTracks.YouTubeResults.Count == 1, $"results survive {theme} at {width} DIPs");
                    }
            }
            finally { window.Close(); }
        }
        finally { Directory.Delete(folder, true); }
    }
}
