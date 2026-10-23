using System.IO;
using System.Windows;
using System.Windows.Threading;
using Cube.Desktop.Services;
using Cube.Desktop.ViewModels;
using NAudio.Wave;

internal static class YouTubeDownloaderChecks
{
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        Console.WriteLine("PASS: " + message);
    }

    public static void Run()
    {
        var app = new Cube.Desktop.App { ShutdownMode = ShutdownMode.OnExplicitShutdown }; app.InitializeComponent();
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
        var task = Tests(); var frame = new DispatcherFrame();
        task.ContinueWith(_ => app.Dispatcher.BeginInvoke(new Action(() => frame.Continue = false)));
        Dispatcher.PushFrame(frame); task.GetAwaiter().GetResult(); app.Shutdown();
    }

    private static async Task Until(Func<bool> predicate)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (!predicate()) await Task.Delay(10, timeout.Token);
    }

    private static async Task Tests()
    {
        string root = Path.Combine(Path.GetTempPath(), "CubeControl-YouTube-Checks", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Window? host = null;
        try
        {
            string legacy = Path.Combine(root, "old.cubeproject");
            File.WriteAllText(legacy, "existing project");
            var service = new FakeYouTube();
            using var vm = new BackingTracksViewModel(root) { YouTubeService = service, PageVisible = true };
            var view = new Cube.Desktop.Views.BackingTracksView { DataContext = vm };
            host = new Window { Content = view, Width = 1000, Height = 700, ShowActivated = false, ShowInTaskbar = false, Opacity = 0 };
            host.Show();
            Assert(view.Content != null && view.BackingTracksScroll != null, "downloader view initializes its content");
            Assert(typeof(BackingTracksViewModel).GetProperty("SessionStems") == null && typeof(BackingTracksViewModel).GetProperty("InsertDownloadedAudio") == null, "downloader exposes no studio or insertion commands");
            vm.YouTubeQuery = "ab";
            await Task.Delay(650);
            Assert(service.Queries.Count == 0, "short queries do not search");
            vm.YouTubeQuery = "blues";
            await Until(() => service.Queries.Count == 1 && !vm.YouTubeSearching);
            Assert(service.Queries[0] == "blues backing track" && vm.YouTubeResults.Count == 1, "automatic search appends backing track");
            vm.AppendBackingTrack = false;
            await Until(() => service.Queries.Count == 2 && !vm.YouTubeSearching);
            Assert(service.Queries[1] == "blues", "filter changes repeat search without suffix");
            vm.YouTubeQuery = "stale"; vm.YouTubeQuery = "latest";
            await Until(() => service.Queries.Count == 3 && !vm.YouTubeSearching);
            Assert(service.Queries[2] == "latest", "debounce cancels stale searches");
            foreach (double width in new[] { 480d, 1000d })
            {
                view.Measure(new Size(width, 700)); view.Arrange(new Rect(0, 0, width, 700)); view.UpdateLayout();
                Assert(view.BackingTracksScroll.ScrollableWidth == 0, "downloader fits page width " + width);
            }

            vm.SelectYouTubeDownloadFolder = _ => root;
            await vm.DownloadYouTube(service.Video);
            string file = Path.Combine(root, "Test.mp3");
            Assert(File.Exists(file) && vm.IsYouTubeDownloaded(service.Video.Id) && vm.YouTubeDownloadStatus == "MP3 salvo.", "MP3 downloads and completion tracking work without a session");
            await vm.DownloadYouTube(service.Video);
            Assert(File.Exists(Path.Combine(root, "Test (2).mp3")), "repeated download preserves existing files");
            vm.PreviewOnHover = true;
            using (var restored = new BackingTracksViewModel(root))
                Assert(restored.YouTubeDownloadFolder == root && restored.PreviewOnHover && restored.IsYouTubeDownloaded(service.Video.Id), "existing download settings and completed files persist");

            service.BlockDownload = true;
            var download = vm.DownloadYouTube(service.Video);
            await Until(() => vm.YouTubeDownloading);
            vm.CancelYouTubeDownload.Execute(null); await download;
            Assert(!vm.YouTubeDownloading && vm.YouTubeDownloadStatus == "Download cancelado.", "canceling a download releases the busy state");
            service.BlockDownload = false;
            service.DownloadError = new FileNotFoundException();
            await vm.DownloadYouTube(service.Video);
            Assert(vm.YouTubeDownloadStatus.Contains("Reinstale"), "missing tools produce actionable feedback");
            service.DownloadError = new IOException("network");
            await vm.DownloadYouTube(service.Video);
            Assert(vm.YouTubeDownloadStatus.Contains("internet"), "network errors produce actionable feedback");
            service.DownloadError = null;
            string missing = Path.Combine(root, "missing"); Directory.CreateDirectory(missing);
            vm.SelectYouTubeDownloadFolder = _ => missing; vm.ChangeYouTubeDownloadFolder.Execute(null);
            Directory.Delete(missing);
            await vm.DownloadYouTube(service.Video);
            Assert(vm.YouTubeFolderStatus.Contains("Alterar pasta"), "unavailable download folder is reported before network work");

            var preview = new FakePreview(); vm.YouTubePreviewService = preview;
            vm.SelectedOutput = new BackingOutput("fake", "Test output");
            int before = 0; vm.BeforePlayback = () => { before++; return Task.CompletedTask; };
            var playing = vm.PreviewYouTube(service.Video, immediate: true);
            await Until(() => preview.Started);
            Assert(before == 1 && vm.YouTubePreviewActive, "preview coordinates with other audio before starting");
            vm.Volume = 45; Assert(Math.Abs(preview.Gain!() - .45f) < .001, "preview volume updates live");
            vm.IsMuted = true; Assert(preview.Gain() == 0, "preview mute updates live");
            await vm.StopForIrAudition(); await playing;
            Assert(preview.Canceled && !vm.YouTubePreviewActive, "IR audition waits for preview output release");
            preview.Reset(); vm.IsMuted = false;
            playing = vm.PreviewYouTube(service.Video, immediate: true); await Until(() => preview.Started);
            vm.PageVisible = false; await playing;
            Assert(preview.Canceled, "leaving backing tracks stops preview");
            vm.PageVisible = true; preview.Reset();
            playing = vm.PreviewYouTube(service.Video, immediate: true); await Until(() => preview.Started);
            vm.SelectedOutput = null; await playing;
            Assert(preview.Canceled, "changing output stops preview");
            vm.SelectedOutput = new BackingOutput("fake", "Test output"); preview.Reset();
            playing = vm.PreviewYouTube(service.Video, immediate: true); await Until(() => preview.Started);
            vm.Dispose(); await playing;
            Assert(preview.Canceled, "closing downloader cancels active preview");
            Assert(File.ReadAllText(legacy) == "existing project", "old project files remain untouched");
            Assert(!File.Exists(Path.Combine(root, "backing-devices.json")), "downloader does not initialize legacy project settings");

            using var engine = new BackingAudioEngine();
            Assert(engine.Snapshot.State == BackingPlaybackState.Empty, "raw IR audio starts without studio or native DSP");
            await engine.Clear();
            RawAudioChecks(root);
            await ServiceChecks(root);
        }
        finally { host?.Close(); Directory.Delete(root, recursive: true); }
    }

    private static async Task ServiceChecks(string root)
    {
        string tools = Path.Combine(root, "tools"); Directory.CreateDirectory(tools);
        foreach (string tool in new[] { "yt-dlp", "ffmpeg", "ffprobe", "deno" })
            File.WriteAllText(Path.Combine(tools, tool + ".exe"), "test placeholder");
        var service = new YouTubeAudioService(tools);
        service.ProcessRunner = (executable, arguments, token, progress) =>
        {
            var args = arguments.ToArray();
            if (args.Contains("--flat-playlist"))
                return Task.FromResult("""{"entries":[{"id":"abcdefghijk","title":"Track","duration":60},{"id":"invalid"},{"id":"bcdefghijkl","live_status":"is_live"}]}""");
            if (Path.GetFileName(executable) == "ffprobe.exe")
                return Task.FromResult("""{"streams":[{"codec_name":"mp3","bit_rate":"320000"}],"format":{"duration":"60"}}""");
            string output = args[Array.IndexOf(args, "-o") + 1].Replace("%(ext)s", "mp3");
            File.WriteAllText(output, "encoded MP3"); progress?.Invoke("PROGRESS:50%");
            return Task.FromResult("");
        };
        var videos = await service.SearchAsync("blues", CancellationToken.None);
        Assert(videos.Count == 1 && videos[0].Id == "abcdefghijk", "real search parser excludes live and invalid results");
        string destination = Path.Combine(root, "converted.mp3");
        await service.DownloadAsync(videos[0], destination, new Progress<YouTubeProgress>(), CancellationToken.None);
        Assert(File.ReadAllText(destination) == "encoded MP3" && !Directory.EnumerateDirectories(root, ".cube-youtube-*").Any(), "download service validates conversion and removes temporary files");
        try
        {
            await service.DownloadAsync(videos[0], destination, new Progress<YouTubeProgress>(), CancellationToken.None);
            throw new Exception("Existing audio was overwritten");
        }
        catch (IOException) { }
        Assert(File.ReadAllText(destination) == "encoded MP3", "download service never overwrites an existing destination");
        service.ProcessRunner = (_, _, token, _) => Task.FromCanceled<string>(token);
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        try
        {
            await service.DownloadAsync(videos[0], Path.Combine(root, "canceled.mp3"), new Progress<YouTubeProgress>(), canceled.Token);
            throw new Exception("Cancellation ignored");
        }
        catch (OperationCanceledException) { }
        Assert(!File.Exists(Path.Combine(root, "canceled.mp3")) && !Directory.EnumerateDirectories(root, ".cube-youtube-*").Any(), "canceled conversion leaves no destination or temporary directory");
    }

    private static void RawAudioChecks(string root)
    {
        string path = Path.Combine(root, "raw.wav");
        using (var writer = new WaveFileWriter(path, new WaveFormat(48000, 16, 1)))
            writer.WriteSamples(Enumerable.Repeat(.25f, 480).ToArray(), 0, 480);
        var type = typeof(BackingAudioEngine).GetNestedType("RawAudio", System.Reflection.BindingFlags.NonPublic)!;
        using var audio = (IDisposable)Activator.CreateInstance(type, path)!;
        var provider = (ISampleProvider)audio;
        float[] samples = new float[960];
        int count = provider.Read(samples);
        Assert(count == 480 && Math.Abs(samples[0] - .25f) < .001, "IR playback preserves raw decoded audio and ends at EOF");
        using var loop = (IDisposable)Activator.CreateInstance(type, path)!;
        type.GetField("Looping")!.SetValue(loop, true); type.GetField("Gain")!.SetValue(loop, .5f);
        count = ((ISampleProvider)loop).Read(samples);
        Assert(count == 960 && samples.All(v => Math.Abs(v - .125f) < .001), "IR playback loops and applies volume without native DSP");
    }

    private sealed class FakeYouTube : IYouTubeAudioService
    {
        public readonly YouTubeResult Video = new("abcdefghijk", "Test", "Channel", 60, null);
        public readonly List<string> Queries = [];
        public bool BlockDownload;
        public Exception? DownloadError;
        public Task<IReadOnlyList<YouTubeResult>> SearchAsync(string query, CancellationToken cancellation)
        { Queries.Add(query); return Task.FromResult<IReadOnlyList<YouTubeResult>>([Video]); }
        public async Task DownloadAsync(YouTubeResult video, string destination, IProgress<YouTubeProgress> progress, CancellationToken cancellation)
        {
            if (DownloadError != null) throw DownloadError;
            if (BlockDownload) await Task.Delay(Timeout.Infinite, cancellation);
            progress.Report(new("Downloading", 50));
            await File.WriteAllTextAsync(destination, "MP3", cancellation);
        }
    }

    private sealed class FakePreview : IYouTubePreviewService
    {
        public bool Started, Canceled;
        public Func<float>? Gain;
        public void Reset() { Started = Canceled = false; }
        public async Task PlayAsync(YouTubeResult video, string outputId, Func<float> volume, Func<CancellationToken, Task> beforeStart, Action started, CancellationToken cancellation, Action<YouTubePreviewFrame>? progress = null)
        {
            await beforeStart(cancellation); Gain = volume; Started = true; started();
            try { await Task.Delay(Timeout.Infinite, cancellation); }
            catch (OperationCanceledException) { Canceled = true; throw; }
        }
    }
}
