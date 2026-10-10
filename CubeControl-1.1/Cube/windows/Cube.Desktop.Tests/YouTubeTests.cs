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
            await YouTubePreviewTests.Run(folder);
            await YouTubeDownloadTests.Run(folder);
            using var model = new BackingTracksViewModel(folder);
            var fake = new Fake(); model.YouTubeService = fake;
            Check(model.AppendBackingTrack, "backing track option starts enabled");
            model.YouTubeQuery = "ab"; await Task.Delay(650); Check(fake.Calls.Count == 0, "short query stays local");
            model.YouTubeQuery = "blues"; await Task.Delay(200); model.YouTubeQuery = "jazz"; await Task.Delay(650);
            Check(fake.Calls.Count == 1 && fake.Calls[0].Query == "jazz backing track", "600ms debounce cancels previous query");
            model.YouTubeQuery = "rock"; await Task.Delay(650);
            var video = new YouTubeResult("abcdefghijk", "Track", "Channel", 60, null);
            fake.Calls[1].Result.SetResult([video]); await Task.Delay(30);
            fake.Calls[0].Result.SetResult([]); await Task.Delay(30);
            Check(model.YouTubeResults.Count == 1, "stale response cannot replace current results");
            Check(model.YouTubeQuery == "rock", "suffix does not change user text");
            model.AppendBackingTrack = false; await Task.Delay(650);
            Check(fake.Calls[2].Query == "rock", "disabled option sends original term");
            model.AppendBackingTrack = true; await Task.Delay(650);
            Check(fake.Calls[3].Query == "rock backing track", "toggle repeats current query with suffix");
            fake.Calls[3].Result.SetResult([video]); await Task.Delay(30);
            fake.Calls[2].Result.SetResult([]); await Task.Delay(30);
            Check(model.YouTubeResults.Count == 1, "response from old toggle state cannot replace current tracks");
            using (var fresh = new BackingTracksViewModel(Path.Combine(folder, "fresh")))
                Check(fresh.AppendBackingTrack, "new session resets option to enabled");
            model.YouTubeQuery = ""; Check(model.YouTubeResults.Count == 0 && !model.ShowYouTube, "clearing search removes online results");
            model.YouTubeQuery = "offline"; await Task.Delay(650); fake.Calls[4].Result.SetException(new IOException("offline")); await Task.Delay(30);
            Check(!model.YouTubeSearching && model.YouTubeStatus.Length > 0, "offline search ends with a recoverable message");
            model.YouTubeQuery = "";
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
            cancel = false;
            try { await service.DownloadAsync(video, destination, new Progress<YouTubeProgress>(), CancellationToken.None); throw new Exception("overwrote destination"); } catch (IOException) { }
            Check(File.ReadAllText(destination) == "original", "successful conversion never overwrites an existing destination");
            File.Delete(destination);
            await service.DownloadAsync(video, destination, new Progress<YouTubeProgress>(), CancellationToken.None);
            Check(File.ReadAllText(destination) == "converted", "successful validation saves destination with special characters");
            File.Delete(destination);
            model.YouTubeService = service;
            await model.DownloadYouTubeTo(video, destination); await model.DownloadYouTubeTo(video, destination);
            Check(model.Tracks.Count == 1 && !model.IsPlaying, "download imports once without autoplay");
            using var editor = new EditorViewModel(Path.Combine(folder, "ui"));
            var window = new Cube.Desktop.MainWindow(editor, initializeDevices: false) { Width = 1280, Height = 900 };
            window.Show();
            try
            {
                ((System.Windows.Controls.TabControl)window.FindName("Tabs")).SelectedIndex = 2;
                for (int i = 0; i < 12; i++) window.BackingTracks.Tracks.Add(new BackingTrack(Path.Combine(folder, $"Practice track {i + 1:00}.mp3")) { Length = 180 });
                var uiFake = new Fake(); window.BackingTracks.YouTubeService = uiFake;
                window.BackingTracks.YouTubeQuery = "blues"; await Task.Delay(650);
                uiFake.Calls[0].Result.SetResult(Enumerable.Range(0, 10).Select(i => video with { Id = $"testvideo{i:00}", Title = $"{i + 1}. Backing track with a long title for responsive layout verification", Channel = "Test channel" }).ToArray());
                await Task.Delay(30);
                await YouTubePreviewTests.PrepareWaveFixture(window.BackingTracks);
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
                        bitmap.Render(window);
                        // Window/Mica render targets contain transparency: composite on the actual canvas.
                        var composition = new System.Windows.Media.DrawingVisual();
                        using (var drawing = composition.RenderOpen())
                        {
                            drawing.DrawRectangle((System.Windows.Media.Brush)window.FindResource("CubeCanvas"), null, new System.Windows.Rect(0, 0, window.ActualWidth, window.ActualHeight));
                            drawing.DrawImage(bitmap, new System.Windows.Rect(0, 0, window.ActualWidth, window.ActualHeight));
                        }
                        bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                        bitmap.Render(composition); var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder(); encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                        using var image = File.Create(Path.Combine(reports, $"youtube-{theme}-{width}.png")); encoder.Save(image);
                        Check(window.BackingTracks.YouTubeResults.Count == 10, $"results survive {theme} at {width} DIPs");
                    }
                await CheckTrackList(window);
                await YouTubePreviewTests.RunUi(window);
            }
            finally { window.Close(); }
        }
        finally { Directory.Delete(folder, true); }
    }
    private static IEnumerable<T> Children<T>(System.Windows.DependencyObject root) where T : System.Windows.DependencyObject
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            if (child is T match) yield return match;
            foreach (var descendant in Children<T>(child)) yield return descendant;
        }
    }
    private static async Task CheckTrackList(Cube.Desktop.MainWindow window)
    {
        var zoomService = Cube.Desktop.Appearance.ZoomService.Instance;
        var languageService = Cube.Desktop.Localization.LanguageService.Instance;
        int originalZoom = zoomService.Percent;
        string originalLanguage = languageService.Language;
        try
        {
            foreach (string language in new[] { "pt-BR", "en-US" })
            foreach (int zoom in new[] { 50, 100, 200 })
            foreach (int width in new[] { 640, 1280 })
            {
                zoomService.Preview(zoom); languageService.Preview(language); window.Width = width;
                await Task.Delay(100); window.UpdateLayout();
                var library = (System.Windows.Controls.Grid)window.FindName("BackingLibraryLayout");
                var playlist = (System.Windows.Controls.Border)window.FindName("BackingPlaylistPanel");
                var youtube = (System.Windows.Controls.Border)window.FindName("BackingYouTubePanel");
                var list = (System.Windows.Controls.ListBox)window.FindName("BackingTrackList");
                var resultsScroll = (System.Windows.Controls.ScrollViewer)window.FindName("BackingYouTubeScroll");
                bool columns = library.ActualWidth >= 860;
                var playlistBounds = playlist.TransformToAncestor(library).TransformBounds(new System.Windows.Rect(playlist.RenderSize));
                var youtubeBounds = youtube.TransformToAncestor(library).TransformBounds(new System.Windows.Rect(youtube.RenderSize));
                Check(playlist.IsVisible && (columns ? Math.Abs(playlistBounds.Top - youtubeBounds.Top) < 1 && youtubeBounds.Left >= playlistBounds.Right + 15 : youtubeBounds.Top >= playlistBounds.Bottom + 15), "playlist stays open and cards adapt without overlap");
                Check(columns ? list.ActualHeight <= 481 && resultsScroll.ActualHeight <= 481 : double.IsPositiveInfinity(list.MaxHeight) && resultsScroll.VerticalScrollBarVisibility == System.Windows.Controls.ScrollBarVisibility.Disabled, "wide lists scroll internally; compact layout uses page scrolling");
                Check(!columns || Math.Abs(playlist.ActualWidth / (playlist.ActualWidth + youtube.ActualWidth) - .45) < .005, "wide panels allocate 45 percent to playlist");
                Check(Cube.Desktop.Localization.LanguageService.T("Passe o mouse ou clique para pr\u00e9-ouvir at\u00e9 30 s") == (language == "en-US" ? "Hover or click to preview up to 30 s" : "Passe o mouse ou clique para pr\u00e9-ouvir at\u00e9 30 s"), "preview guidance is translated");
                Check(list.Items.Count == 12 && window.BackingTracks.Search.Length == 0, "YouTube query preserves all playlist rows");
                Check(Cube.Desktop.Localization.LanguageService.T("Buscando faixas\u2026") == (language == "en-US" ? "Searching tracks\u2026" : "Buscando faixas\u2026"), "search progress is translated");
                Check(!Children<System.Windows.Controls.TextBlock>(youtube).Any(t => t.Text.Contains("YouTube") || t.Text.Contains("v\u00eddeo")), "search presentation uses musical language");
                var toggle = (System.Windows.Controls.Primitives.ToggleButton)window.FindName("BackingTrackSearchToggle");
                var search = (Wpf.Ui.Controls.AutoSuggestBox)window.FindName("BackingYouTubeSearch");
                var toggleBounds = toggle.TransformToAncestor(search.Parent as System.Windows.Media.Visual).TransformBounds(new System.Windows.Rect(toggle.RenderSize));
                Check(toggleBounds.Left >= 0 && toggleBounds.Right <= search.ActualWidth + 1 && toggle.ActualHeight >= 32, "search toggle stays within field");
                toggle.Focus();
                Check(toggle.IsKeyboardFocused && new System.Windows.Automation.Peers.ToggleButtonAutomationPeer(toggle).GetPattern(System.Windows.Automation.Peers.PatternInterface.Toggle) != null, "search option exposes keyboard focus and toggle state");
                var mosaic = (System.Windows.Controls.ItemsControl)window.FindName("YouTubeMosaic");
                var cards = Children<Cube.Desktop.Controls.YouTubeResultCard>(mosaic).ToArray();
                Check(cards.Length == 10 && cards.All(c => c.ActualWidth <= mosaic.ActualWidth + 1 && c.ActualWidth > 0), $"mosaic fits {width}/{zoom}%/{language}");
                bool contained = true;
                double previousBottom = 0;
                foreach (var card in cards)
                {
                    var position = card.TransformToAncestor(mosaic).Transform(new System.Windows.Point());
                    contained &= position.X >= -1 && position.X + card.ActualWidth <= mosaic.ActualWidth + 1;
                    contained &= position.Y >= previousBottom - 1;
                    previousBottom = position.Y + card.ActualHeight;
                    Check(!Children<System.Windows.Controls.Image>(card).Any(), "track rows have no video artwork");
                    var wave = Children<Cube.Desktop.Controls.BackingWaveform>(card).Single();
                    Check(wave.ActualHeight == 30 && !wave.IsHitTestVisible && !wave.Focusable, "preview waveform stays compact and read-only");
                    foreach (string action in new[] { "PreviewButton", "DownloadButton" })
                    {
                        var control = (System.Windows.FrameworkElement)card.FindName(action);
                        var bounds = control.TransformToAncestor(card).TransformBounds(new System.Windows.Rect(control.RenderSize));
                        Check(bounds.Left >= 0 && bounds.Right <= card.ActualWidth + 1 && bounds.Bottom <= card.ActualHeight + 1, "preview and download actions are contained");
                    }
                }
                Check(contained, "track rows fit full width in a single vertical list");
                var button = (Wpf.Ui.Controls.Button)cards[0].FindName("DownloadButton");
                button.Focus(); await Task.Delay(20);
                Check(button.IsKeyboardFocused && button.ActualWidth >= 40 && button.ActualHeight >= 40 && button.IsVisible, "keyboard reaches always visible download action");
                if (language == "en-US" && zoom == 200 && width == 640)
                {
                    cards[0].BringIntoView(); await Task.Delay(100);
                    var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                    bitmap.Render(window);
                    var composition = new System.Windows.Media.DrawingVisual();
                    using (var drawing = composition.RenderOpen())
                    {
                        drawing.DrawRectangle((System.Windows.Media.Brush)window.FindResource("CubeCanvas"), null, new System.Windows.Rect(window.RenderSize));
                        drawing.DrawImage(bitmap, new System.Windows.Rect(window.RenderSize));
                    }
                    bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                    bitmap.Render(composition);
                    var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder(); encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                    using var screenshot = File.Create("C:/CubeControl-Build/youtube-validation/reports/youtube-English-200.png"); encoder.Save(screenshot);
                }
                var preview = (Wpf.Ui.Controls.Button)cards[0].FindName("PreviewButton");
                Check(preview.ToolTip?.ToString() == (language == "en-US" ? "Preview audio" : "Pré-ouvir"), "preview action is translated");
            }
        }
        finally { zoomService.Preview(originalZoom); languageService.Preview(originalLanguage); }
    }

}
