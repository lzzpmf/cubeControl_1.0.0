using Cube.Desktop.Services;
using Cube.Desktop.ViewModels;

internal static class YouTubePreviewTests
{
    private static void Check(bool value, string message)
    { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); }
    private sealed class Fake : IYouTubePreviewService
    {
        internal sealed record Call(string Id, string Output, Func<float> Volume, Func<CancellationToken, Task> BeforeStart, Action Started, CancellationToken Token, TaskCompletionSource Completion, Action<YouTubePreviewFrame>? Progress);
        public List<Call> Calls { get; } = [];
        public Task PlayAsync(YouTubeResult video, string outputId, Func<float> volume, Func<CancellationToken, Task> beforeStart, Action started, CancellationToken cancellation, Action<YouTubePreviewFrame>? progress = null)
        {
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            cancellation.Register(() => completion.TrySetCanceled(cancellation));
            Calls.Add(new(video.Id, outputId, volume, beforeStart, started, cancellation, completion, progress));
            return completion.Task;
        }
    }
    private sealed class Download : IYouTubeAudioService
    {
        public Task<IReadOnlyList<YouTubeResult>> SearchAsync(string query, CancellationToken cancellation) => Task.FromResult<IReadOnlyList<YouTubeResult>>([]);
        public Task DownloadAsync(YouTubeResult video, string destination, IProgress<YouTubeProgress> progress, CancellationToken cancellation)
            => Task.Delay(Timeout.Infinite, cancellation);
    }
    internal static async Task Run(string folder)
    {
        await YouTubePreviewDataTests.Run();
        using var model = new BackingTracksViewModel(folder) { PageVisible = true, SelectedOutput = new("fixture", "Test output") };
        var fake = new Fake(); model.YouTubePreviewService = fake;
        int irStops = 0;
        model.BeforePlayback = () => { irStops++; return Task.CompletedTask; };
        var first = new YouTubeResult("abcdefghijk", "First", "Channel", 60, null);
        var second = first with { Id = "12345678901", Title = "Second" };
        var pending = model.PreviewYouTube(first);
        await Task.Delay(200); Check(fake.Calls.Count == 0, "hover waits before resolving audio");
        model.StopYouTubePreview(); await pending;
        Check(fake.Calls.Count == 0, "leaving during dwell never resolves audio");
        pending = model.PreviewYouTube(first);
        await model.PreviewYouTube(first, true); await Task.Delay(40);
        Check(fake.Calls.Count == 1 && !fake.Calls[0].Token.IsCancellationRequested, "click promotes pending hover without restarting preparation");
        fake.Calls[0].Progress?.Invoke(new([.25f, .5f], 30, .1, .05)); await Task.Delay(30);
        Check(model.GetYouTubeWave(first.Id)?.Peaks[1] == .5f, "preview receives real peaks even before output gain");
        var oldProgress = fake.Calls[0].Progress;
        model.StopYouTubePreview(); await pending;
        oldProgress?.Invoke(new([1f], 30, 1, 1)); await Task.Delay(30);
        Check(model.GetYouTubeWave(first.Id)?.Peaks.Length == 2, "cancelled waveform callback cannot overwrite session cache");
        fake.Calls.Clear();
        pending = model.PreviewYouTube(first); await Task.Delay(350);
        Check(fake.Calls.Count == 1 && model.YouTubePreviewActive, "stable hover starts after 250ms");
        fake.Calls[0].Started(); await Task.Delay(30);
        Check(model.YouTubePreviewStatus.Contains("30 s"), "started preview has an in-card playing state");
        model.Volume = 42; Check(Math.Abs(fake.Calls[0].Volume() - .42f) < .001, "preview uses accompaniment volume");
        model.IsMuted = true; Check(fake.Calls[0].Volume() == 0, "preview follows mute immediately");
        model.IsMuted = false;
        var next = model.PreviewYouTube(second, true);
        await pending;
        Check(fake.Calls[0].Token.IsCancellationRequested && fake.Calls[1].Output == "fixture", "switching cards cancels old preview and uses selected output");
        fake.Calls[0].Started(); await Task.Delay(30);
        Check(model.YouTubePreviewId == second.Id && model.YouTubePreviewStatus == "Preparando prévia…", "late callback cannot replace new preview state");
        fake.Calls[1].Completion.SetResult(); await next;
        int calls = fake.Calls.Count; await Task.Delay(850);
        Check(fake.Calls.Count == calls && !model.YouTubePreviewActive && model.YouTubePreviewStatus == "Prévia concluída", "finished preview does not restart automatically");
        pending = model.PreviewYouTube(first, true);
        fake.Calls[^1].Completion.SetException(new TimeoutException()); await pending;
        Check(model.YouTubePreviewStatus.Contains("demorou"), "timeout remains recoverable in card");
        pending = model.PreviewYouTube(first, true);
        fake.Calls[^1].Completion.SetException(new System.IO.IOException()); await pending;
        Check(model.YouTubePreviewStatus.Contains("indisponível"), "stream failure remains recoverable in card");
        pending = model.PreviewYouTube(first, true); model.PageVisible = false; await pending;
        Check(fake.Calls[^1].Token.IsCancellationRequested && model.YouTubePreviewId == null, "leaving page cancels preview");
        model.PageVisible = true;
        pending = model.PreviewYouTube(first, true); model.SelectedOutput = new("other", "Other"); await pending;
        Check(fake.Calls[^1].Token.IsCancellationRequested, "switching endpoint cancels preview");
        pending = model.PreviewYouTube(first, true); model.YouTubeQuery = "ab"; await pending;
        Check(fake.Calls[^1].Token.IsCancellationRequested, "new search cancels preview");
        typeof(BackingTracksViewModel).GetProperty(nameof(model.State))!.SetValue(model, BackingPlaybackState.Playing);
        pending = model.PreviewYouTube(first, true);
        Check(model.IsPlaying, "preparing preview leaves backing track playing");
        fake.Calls[^1].Completion.SetException(new System.IO.IOException()); await pending;
        Check(model.IsPlaying, "failure before ready preserves backing playback");
        Check(irStops == 0, "failed preparation does not interrupt IR audition");
        pending = model.PreviewYouTube(first, true);
        await fake.Calls[^1].BeforeStart(fake.Calls[^1].Token);
        Check(model.State == BackingPlaybackState.Paused, "ready preview pauses local playback");
        Check(irStops == 1, "ready preview preserves coordination with IR audition");
        fake.Calls[^1].Started(); fake.Calls[^1].Completion.SetResult(); await pending;
        Check(model.State == BackingPlaybackState.Paused, "completed preview does not resume local playback");
        pending = model.PreviewYouTube(first, true); model.StopYouTubePreview(); await pending;
        Check(model.State == BackingPlaybackState.Paused, "leaving preview keeps local playback paused");
        pending = model.PreviewYouTube(first, true);
        var late = fake.Calls[^1]; model.StopYouTubePreview(); await pending;
        try { await late.BeforeStart(late.Token); throw new Exception("late ready accepted"); } catch (OperationCanceledException) { }
        Check(model.Tracks.Count == 0 && !model.IsMonitoring, "previews never import tracks or start guitar monitoring");
        pending = model.PreviewYouTube(first, true);
        model.SelectedTrack = new BackingTrack(System.IO.Path.Combine(folder, "missing-test-track.wav"));
        model.PlaySelected(); await pending;
        Check(fake.Calls[^1].Token.IsCancellationRequested, "starting local playback cancels preview before opening local audio");
        model.SelectedTrack = null;
        model.YouTubeService = new Download();
        pending = model.PreviewYouTube(first, true);
        var download = model.DownloadYouTubeTo(second, System.IO.Path.Combine(folder, "cancelled.mp3"));
        await pending;
        Check(fake.Calls[^1].Token.IsCancellationRequested && model.YouTubeDownloadId == second.Id && model.YouTubeDownloadTitle == second.Title, "download cancels preview and exposes active video identity");
        calls = fake.Calls.Count; await model.PreviewYouTube(first, true);
        Check(fake.Calls.Count == calls, "hover cannot restart preview during download");
        model.CancelYouTubeDownload.Execute(null); await download;
        pending = model.PreviewYouTube(first, true); model.Dispose(); await pending;
        Check(fake.Calls[^1].Token.IsCancellationRequested, "dispose cancels active work");
    }

    // Opt-in integration check: real network, decoder and WASAPI, with muted output.
    internal static async Task RunLive()
    {
        using var enumerator = new NAudio.CoreAudioApi.MMDeviceEnumerator();
        using var device = enumerator.GetDefaultAudioEndpoint(NAudio.CoreAudioApi.DataFlow.Render, NAudio.CoreAudioApi.Role.Multimedia);
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(100));
        var results = await new YouTubeAudioService().SearchAsync("blues guitar backing track", deadline.Token);
        Check(results.Count > 0, "live YouTube search returns results");
        string folder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "cube-preview-live-" + Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(folder);
        try
        {
            string source = System.IO.Path.Combine(folder, "silent-local.wav");
            using (var writer = new NAudio.Wave.WaveFileWriter(source, NAudio.Wave.WaveFormat.CreateIeeeFloatWaveFormat(44100, 1)))
            {
                var silence = new float[44100];
                for (int i = 0; i < 60; i++) writer.WriteSamples(silence, 0, silence.Length);
            }
            using var model = new BackingTracksViewModel(folder) { PageVisible = true, IsMuted = true, SelectedOutput = new(device.ID, device.FriendlyName) };
            ((YouTubePreviewService)model.YouTubePreviewService).Timing = (stage, milliseconds) => Console.WriteLine($"TIMING {stage}: {milliseconds:F0} ms");
            ((YouTubePreviewService)model.YouTubePreviewService).Diagnostic = Console.WriteLine;
            model.PreviewTiming = (stage, milliseconds) => Console.WriteLine($"TIMING {stage}: {milliseconds:F0} ms");
            model.AddPaths([source]);
            while (model.Duration <= 0) await Task.Delay(30, deadline.Token);
            model.Position = 7; model.PlaySelected();
            while (!model.IsPlaying) await Task.Delay(30, deadline.Token);
            var preview = model.PreviewYouTube(results[0], true);
            try
            {
                while (!model.YouTubePreviewStatus.Contains("30 s") && !preview.IsCompleted) await Task.Delay(30, deadline.Token);
                if (preview.IsCompleted) throw new Exception("Real preview failed: " + model.YouTubePreviewStatus);
                Check(model.State == BackingPlaybackState.Paused && model.Position >= 7, "real preview pauses local WAV and preserves position");
                double pausedAt = model.Position;
                await Task.Delay(1500, deadline.Token);
                model.StopYouTubePreview(); await preview;
                Check(!model.IsPlaying && Math.Abs(model.Position - pausedAt) < .05, "real preview cancellation keeps local WAV paused at same position");
                Check(model.GetYouTubeWave(results[0].Id)?.DecodedDuration > 0, "live muted preview produces real waveform");
                Console.WriteLine("REPEAT PREVIEW (cached resolution)");
                preview = model.PreviewYouTube(results[0], true);
                while (!model.YouTubePreviewStatus.Contains("30 s") && !preview.IsCompleted) await Task.Delay(30, deadline.Token);
                Check(!preview.IsCompleted, "cached real preview starts");
                await Task.Delay(500, deadline.Token);
                model.StopYouTubePreview(); await preview;
            }
            finally { model.StopYouTubePreview(); await preview; }
        }
        finally { System.IO.Directory.Delete(folder, true); }
        Console.WriteLine("NOT VALIDATED: audible listening; integration output was muted.");
    }

    internal static async Task PrepareWaveFixture(BackingTracksViewModel model)
    {
        var fake = new Fake(); model.YouTubePreviewService = fake;
        model.SelectedOutput = new("fixture", "Test output");
        var video = model.YouTubeResults[0];
        var pending = model.PreviewYouTube(video, true);
        var pcm = new byte[44100 * 4 * 8];
        for (int i = 0; i < pcm.Length / 2; i++)
        {
            double t = i / 88200d;
            short sample = (short)(Math.Sin(t * 440 * Math.PI * 2) * (.15 + .7 * Math.Abs(Math.Sin(t * 3))) * 32767);
            pcm[2 * i] = (byte)sample; pcm[2 * i + 1] = (byte)(sample >> 8);
        }
        var peaks = new YouTubePreviewPeaks(); peaks.Append(pcm);
        fake.Calls[^1].Progress?.Invoke(peaks.Snapshot(30, 4)); await Task.Delay(30);
        model.StopYouTubePreview(); await pending;
    }

    private static IEnumerable<T> Descendants<T>(System.Windows.DependencyObject root) where T : System.Windows.DependencyObject
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            if (child is T match) yield return match;
            foreach (var nested in Descendants<T>(child)) yield return nested;
        }
    }
    private static async Task CaptureDownloadCards(Cube.Desktop.MainWindow window, Cube.Desktop.Controls.YouTubeResultCard card, string state)
    {
        var theme = Cube.Desktop.Appearance.ThemeService.Instance;
        var zoom = Cube.Desktop.Appearance.ZoomService.Instance;
        var language = Cube.Desktop.Localization.LanguageService.Instance;
        int previousZoom = zoom.Percent;
        string previousLanguage = language.Language;
        double previousWidth = window.Width;
        var previousTheme = theme.Mode;
        bool highContrast = theme.IsHighContrast;
        try
        {
            foreach (string mode in new[] { "Dark", "Light", "HighContrast" })
            foreach (string locale in new[] { "pt-BR", "en-US" })
            foreach (int scale in new[] { 50, 100, 200 })
            {
                if (mode == "HighContrast") Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.HighContrast);
                else theme.Preview(Enum.Parse<Cube.Desktop.Appearance.ThemeMode>(mode));
                language.Preview(locale); zoom.Preview(scale); window.Width = scale == 200 ? 640 : 1280;
                await Task.Delay(50); window.UpdateLayout();
                var actionButton = (Wpf.Ui.Controls.Button)card.FindName("DownloadButton");
                Check(actionButton.ToolTip?.ToString() == (locale == "en-US" ? (card.IsDownloading ? "Cancel download" : "Download MP3") : card.DownloadLabel), "download action follows current language");
                if (card.IsDownloading && locale == "en-US")
                    Check(Descendants<System.Windows.Controls.TextBlock>(card).Any(t => t.Text == "Downloading audio\u2026"), "download message follows current language");
                foreach (string action in new[] { "PreviewButton", "DownloadButton" })
                {
                    var button = (System.Windows.Controls.Control)card.FindName(action);
                    var clip = System.Windows.Controls.Primitives.LayoutInformation.GetLayoutClip(button);
                    Check(button.Margin == new System.Windows.Thickness(0) && button.ActualWidth == 40 && button.ActualHeight == 40 && (clip == null || clip.Bounds.Contains(new System.Windows.Rect(button.RenderSize))), "button has full forty-DIP face without layout clipping");
                }
                var drawing = new System.Windows.Media.DrawingVisual();
                using (var dc = drawing.RenderOpen())
                {
                    dc.DrawRectangle((System.Windows.Media.Brush)window.FindResource("CubeCanvas"), null, new System.Windows.Rect(card.RenderSize));
                    dc.DrawRectangle(new System.Windows.Media.VisualBrush(card), null, new System.Windows.Rect(card.RenderSize));
                }
                double factor = scale / 100d;
                var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap((int)Math.Ceiling(card.ActualWidth * factor), (int)Math.Ceiling(card.ActualHeight * factor), 96 * factor, 96 * factor, System.Windows.Media.PixelFormats.Pbgra32);
                bitmap.Render(drawing);
                var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder(); encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                using var file = System.IO.File.Create($"C:/CubeControl-Build/youtube-validation/reports/{state}-card-{mode}-{locale}-{scale}.png"); encoder.Save(file);
            }
        }
        finally
        {
            zoom.Preview(previousZoom); language.Preview(previousLanguage); window.Width = previousWidth;
            theme.Preview(previousTheme);
            if (highContrast) Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.HighContrast);
            await Task.Delay(50); window.UpdateLayout();
        }
    }

    internal static async Task RunUi(Cube.Desktop.MainWindow window)
    {
        var model = window.BackingTracks;
        var fake = new Fake(); model.YouTubePreviewService = fake;
        model.SelectedOutput = new("fixture", "Test output");
        var video = model.YouTubeResults[0];
        var pending = model.PreviewYouTube(video, true);
        var escape = new System.Windows.Input.KeyEventArgs(System.Windows.Input.Keyboard.PrimaryDevice,
            System.Windows.PresentationSource.FromVisual(window), 0, System.Windows.Input.Key.Escape)
        { RoutedEvent = System.Windows.Input.Keyboard.PreviewKeyDownEvent };
        window.RaiseEvent(escape); await pending;
        Check(escape.Handled && fake.Calls[^1].Token.IsCancellationRequested, "Escape cancels preview without invoking transport shortcuts");
        var card = Descendants<Cube.Desktop.Controls.YouTubeResultCard>(window).Single(c => (c.DataContext as YouTubeResult)?.Id == video.Id);
        double restingHeight = card.ActualHeight;
        await CaptureDownloadCards(window, card, "waveform");
        model.YouTubeService = new Download();
        var download = model.DownloadYouTubeTo(video, System.IO.Path.Combine(System.IO.Path.GetTempPath(), "cube-cancelled-preview-test.mp3"));
        await Task.Delay(60); window.UpdateLayout();
        var progress = (System.Windows.Controls.ProgressBar)card.FindName("DownloadProgress");
        Check(progress.IsVisible && progress.IsIndeterminate && progress.Height == 4, "YouTube download uses shared infinite progress inside its card");
        Check(window.FindName("YouTubeDownloadProgress") == null && card.ActualHeight == restingHeight, "download stays inside existing row without adding a panel or changing its height");
        Check(card.CanDownload && card.IsDownloading && card.DownloadIcon == Wpf.Ui.Controls.SymbolRegular.Dismiss24, "active download action remains enabled as cancel");
        Check(Descendants<Cube.Desktop.Controls.YouTubeResultCard>(window).Where(c => c != card).All(c => !c.CanDownload && c.DownloadProgressVisibility == System.Windows.Visibility.Collapsed), "other rows neither animate nor allow another download");
        await CaptureDownloadCards(window, card, "download");
        var panel = (System.Windows.FrameworkElement)window.FindName("BackingYouTubePanel");
        panel.BringIntoView(new System.Windows.Rect(0, 0, panel.ActualWidth, 260));
        await Task.Delay(100);
        var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
        bitmap.Render(window);
        var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder(); encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
        using (var image = System.IO.File.Create("C:/CubeControl-Build/youtube-validation/reports/youtube-download.png")) encoder.Save(image);
        ((Wpf.Ui.Controls.Button)card.FindName("DownloadButton")).RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        await download; await Task.Delay(30);
        Check(!progress.IsVisible && !progress.IsIndeterminate, "download indicator stops animating after cancellation");
        Check(card.ActualHeight == restingHeight && card.HasDownloadMessage && card.StatusText == model.YouTubeDownloadStatus, "terminal message stays inside same-size card");
        await CaptureDownloadCards(window, card, "cancelled");
        pending = model.PreviewYouTube(video, true);
        ((System.Windows.FrameworkElement)window.FindName("BackingYouTubePanel")).Visibility = System.Windows.Visibility.Collapsed;
        await pending.WaitAsync(TimeSpan.FromSeconds(2));
        Check(fake.Calls[^1].Token.IsCancellationRequested, "hiding results cancels preview");
    }
}
