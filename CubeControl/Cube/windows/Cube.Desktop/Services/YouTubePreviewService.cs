using System.Diagnostics;
using System.IO;
using System.Text.Json;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Cube.Desktop.Services;

internal interface IYouTubePreviewService
{
    Task<YouTubePreparedAudio> PrepareAudioAsync(YouTubeResult video, CancellationToken cancellation);
    Task<YouTubePreviewFrame> PrepareAsync(YouTubeResult video, CancellationToken cancellation);
    Task PlayAsync(YouTubeResult video, string outputId, Func<float> volume, Func<CancellationToken, Task> beforeStart, Action started, CancellationToken cancellation, Action<YouTubePreviewFrame>? progress = null);
}

internal sealed record YouTubePreparedAudio(byte[] Pcm16, YouTubePreviewFrame Waveform);

// One worker owns the decoder and audio endpoint until both have been released.
// PCM is streamed through a bounded buffer; no preview files are created.
internal sealed partial class YouTubePreviewService(string? toolsDirectory = null) : IYouTubePreviewService
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string tools = toolsDirectory ?? Path.Combine(AppContext.BaseDirectory, "Tools", "YouTube");
    private readonly YouTubePreviewCache cache = new();
    internal Action<string, double>? Timing { get; set; }
    internal Action<string>? Diagnostic { get; set; }
    internal Func<YouTubeResult, CancellationToken, Task<string>>? Resolve { get; set; }
    internal Func<string, Action, CancellationToken, Task>? DecodeForTest { get; set; }
    public Task PlayAsync(YouTubeResult video, string outputId, Func<float> volume, Func<CancellationToken, Task> beforeStart, Action started, CancellationToken cancellation, Action<YouTubePreviewFrame>? progress = null)
        => Task.Run(async () =>
        {
            await gate.WaitAsync(cancellation);
            try
            {
                if (TryPrepared(video.Id, out var prepared))
                {
                    await PlayPrepared(prepared, outputId, volume, beforeStart, started, cancellation, progress);
                    return;
                }
                bool cached = cache.TryGet(video.Id, out string metadata);
                bool didStart = false;
                void OnStarted() { didStart = true; started(); }
                for (int attempt = 0; ; attempt++)
                {
                    var resolveTime = Stopwatch.StartNew();
                    if (!cached)
                    {
                        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
                        timeout.CancelAfter(TimeSpan.FromSeconds(20));
                        try { metadata = await (Resolve?.Invoke(video, timeout.Token) ?? new YouTubeAudioService(tools).ResolvePreviewAsync(video, timeout.Token)); }
                        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested) { throw new TimeoutException("Preview resolution timed out"); }
                        cache.Store(video.Id, metadata);
                    }
                    Timing?.Invoke(cached ? "resolve-cache" : "resolve", resolveTime.Elapsed.TotalMilliseconds);
                    try
                    {
                        if (DecodeForTest != null) await DecodeForTest(metadata, OnStarted, cancellation);
                        else await Play(video, metadata, outputId, volume, beforeStart, OnStarted, cancellation, progress);
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (!cancellation.IsCancellationRequested) Diagnostic?.Invoke($"preview decoder: {ex.GetType().Name}");
                        if (!cached || attempt != 0 || didStart || cancellation.IsCancellationRequested || ex is OperationCanceledException) throw;
                        cache.Remove(video.Id); cached = false;
                    }
                }
            }
            finally { gate.Release(); }
        }, cancellation);

    private async Task Play(YouTubeResult video, string metadata, string outputId, Func<float> volume, Func<CancellationToken, Task> beforeStart, Action started, CancellationToken cancellation, Action<YouTubePreviewFrame>? progress = null)
    {
        using var preparation = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        preparation.CancelAfter(TimeSpan.FromSeconds(20));
        var token = preparation.Token;
        try
        {
            var decodeTime = Stopwatch.StartNew();
            var info = CreateDecodeInfo(metadata);
            using var process = new Process { StartInfo = info };
            token.ThrowIfCancellationRequested();
            process.Start();
            using var registration = token.Register(() => MediaProcess.Kill(process));
            var errors = process.StandardError.ReadToEndAsync();
            try
            {
                MMDevice? device = null;
                WasapiPlayer? output = null;
                var buffer = new BufferedWaveProvider(new WaveFormat(44100, 16, 2), TimeSpan.FromSeconds(2));
                var gain = new PreviewSamples(buffer.ToSampleProvider(), volume, token);
                var stopped = new TaskCompletionSource<Exception?>(TaskCreationOptions.RunContinuationsAsynchronously);
                byte[] block = new byte[8192];
                bool playing = false;
                async Task StartPlayback()
                {
                    token.ThrowIfCancellationRequested();
                    await beforeStart(token).WaitAsync(token);
                    token.ThrowIfCancellationRequested();
                    // Decode first, then release the study's exclusive endpoint before
                    // opening this player. Init itself can claim the device.
                    using var enumerator = new MMDeviceEnumerator();
                    device = enumerator.GetDevice(outputId);
                    output = new WasapiPlayerBuilder().WithDevice(device).WithSharedMode().WithLatency(80).Build();
                    output.Init(new SampleToWaveProvider(gain));
                    output.PlaybackStopped += (_, e) => stopped.TrySetResult(e.Exception);
                    token.ThrowIfCancellationRequested();
                    output.Play();
                    playing = true;
                    preparation.CancelAfter(TimeSpan.FromSeconds(35));
                    Timing?.Invoke("output-start", decodeTime.Elapsed.TotalMilliseconds);
                    started();
                }
                long total = 0;
                var peaks = new YouTubePreviewPeaks();
                double duration = video.Duration > 0 ? Math.Min(30, video.Duration) : 30;
                using var reporting = CancellationTokenSource.CreateLinkedTokenSource(token);
                // Separate timer keeps the playhead moving even if the network read stalls.
                var reporter = Task.Run(async () =>
                {
                    try
                    {
                        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
                        while (await timer.WaitForNextTickAsync(reporting.Token))
                        {
                            double position = Math.Max(0, (Interlocked.Read(ref total) - buffer.BufferedBytes) / 176400d - .08);
                            progress?.Invoke(peaks.Snapshot(duration, position));
                        }
                    }
                    catch (OperationCanceledException) when (reporting.IsCancellationRequested) { }
                });
                try
                {
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();
                        if (stopped.Task.IsCompleted) throw await stopped.Task ?? new IOException("Preview output stopped");
                        if (buffer.BufferedDuration.TotalSeconds > 1)
                        { await Task.Delay(20, token); continue; }
                        int count = await process.StandardOutput.BaseStream.ReadAsync(block, token);
                        if (count == 0) break;
                        if (total == 0) Timing?.Invoke("first-pcm", decodeTime.Elapsed.TotalMilliseconds);
                        buffer.AddSamples(block.AsSpan(0, count)); Interlocked.Add(ref total, count);
                        if (!playing && buffer.BufferedDuration.TotalMilliseconds >= 200)
                            await StartPlayback();
                        peaks.Append(block.AsSpan(0, count));
                    }
                    await process.WaitForExitAsync(token);
                    string decoderErrors = await errors;
                    if (process.ExitCode != 0 || total == 0)
                    {
                        // Only report status codes, never signed URLs or HTTP headers.
                        var http = System.Text.RegularExpressions.Regex.Match(decoderErrors, @"(?:HTTP error|Server returned)\s+\d{3}");
                        Diagnostic?.Invoke($"FFmpeg exit={process.ExitCode}; bytes={total}; {http.Value}");
                        throw new IOException("Preview decoding failed");
                    }
                    if (!playing)
                        await StartPlayback();
                    while (buffer.BufferedBytes > 0)
                    {
                        if (stopped.Task.IsCompleted) throw await stopped.Task ?? new IOException("Preview output stopped");
                        await Task.Delay(20, token);
                    }
                    await Task.Delay(100, token);
                }
                finally
                {
                    reporting.Cancel();
                    try { await reporter; }
                    finally
                    {
                        try { output?.Stop(); }
                        finally { try { output?.Dispose(); } finally { device?.Dispose(); } }
                    }
                }
            }
            finally
            {
                MediaProcess.Kill(process);
                await process.WaitForExitAsync(CancellationToken.None);
                await errors;
            }
        }
        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
        { throw new TimeoutException("Preview preparation or playback timed out"); }
    }

    // Read volume on the audio callback, even while the network decoder is stalled.
    // Cancellation returns EOF immediately instead of playing the buffered remainder.
    private sealed class PreviewSamples(ISampleProvider source, Func<float> volume, CancellationToken cancellation) : ISampleProvider
    {
        public WaveFormat WaveFormat => source.WaveFormat;
        public int Read(Span<float> samples)
        {
            if (cancellation.IsCancellationRequested) return 0;
            int count = source.Read(samples);
            float gain = Math.Clamp(volume(), 0, 1);
            for (int i = 0; i < count; i++) samples[i] *= gain;
            return count;
        }
    }
}
