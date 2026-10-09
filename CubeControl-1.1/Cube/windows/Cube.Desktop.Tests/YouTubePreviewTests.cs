using Cube.Desktop.Services;
using Cube.Desktop.ViewModels;

internal static class YouTubePreviewTests
{
    private static void Check(bool value, string message)
    { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); }
    private sealed class Fake : IYouTubePreviewService
    {
        internal sealed record Call(string Id, string Output, Func<float> Volume, Func<CancellationToken, Task> BeforeStart, Action Started, CancellationToken Token, TaskCompletionSource Completion);
        public List<Call> Calls { get; } = [];
        public Task PlayAsync(YouTubeResult video, string outputId, Func<float> volume, Func<CancellationToken, Task> beforeStart, Action started, CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            cancellation.Register(() => completion.TrySetCanceled(cancellation));
            Calls.Add(new(video.Id, outputId, volume, beforeStart, started, cancellation, completion));
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
        pending = model.PreviewYouTube(first); await Task.Delay(900);
        Check(fake.Calls.Count == 1 && model.YouTubePreviewActive, "stable hover starts after 800ms");
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
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(45));
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
            }
            finally { model.StopYouTubePreview(); await preview; }
        }
        finally { System.IO.Directory.Delete(folder, true); }
        Console.WriteLine("NOT VALIDATED: audible listening; integration output was muted.");
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
        model.YouTubeService = new Download();
        var download = model.DownloadYouTubeTo(video, System.IO.Path.Combine(System.IO.Path.GetTempPath(), "cube-cancelled-preview-test.mp3"));
        await Task.Delay(60); window.UpdateLayout();
        var progress = (System.Windows.Controls.ProgressBar)window.FindName("YouTubeDownloadProgress");
        Check(progress.IsVisible && progress.IsIndeterminate && progress.Height == 4, "YouTube download uses shared infinite progress inside its card");
        var panel = (System.Windows.FrameworkElement)window.FindName("BackingYouTubePanel");
        panel.BringIntoView(new System.Windows.Rect(0, 0, panel.ActualWidth, 260));
        await Task.Delay(100);
        var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
        bitmap.Render(window);
        var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder(); encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
        using (var image = System.IO.File.Create("C:/CubeControl-Build/youtube-validation/reports/youtube-download.png")) encoder.Save(image);
        model.CancelYouTubeDownload.Execute(null); await download; await Task.Delay(30);
        Check(!progress.IsVisible && !progress.IsIndeterminate, "download indicator stops animating after cancellation");
        pending = model.PreviewYouTube(video, true);
        ((System.Windows.FrameworkElement)window.FindName("BackingYouTubePanel")).Visibility = System.Windows.Visibility.Collapsed;
        await pending.WaitAsync(TimeSpan.FromSeconds(2));
        Check(fake.Calls[^1].Token.IsCancellationRequested, "hiding results cancels preview");
    }
}
