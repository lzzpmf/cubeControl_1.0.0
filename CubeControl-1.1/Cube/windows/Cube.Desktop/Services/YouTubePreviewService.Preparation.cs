using System.Diagnostics;
using System.IO;
using System.Text.Json;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Cube.Desktop.Services;

internal sealed partial class YouTubePreviewService
{
    private const int MaximumPcmBytes = 30 * 44100 * 2 * sizeof(short);
    private sealed record PreparedPreview(byte[] Pcm, YouTubePreviewFrame Frame);
    private readonly Dictionary<string, PreparedPreview> prepared = [];
    private readonly LinkedList<string> preparedOrder = new();
    internal int PreparedCacheCount => prepared.Count;
    internal Func<YouTubeResult, string, CancellationToken, Task<byte[]>>? PreparePcmForTest { get; set; }

    /// <summary>Decodes at most 30 seconds into bounded RAM. This path never creates an audio endpoint.</summary>
    public Task<YouTubePreviewFrame> PrepareAsync(YouTubeResult video, CancellationToken cancellation)
        => Task.Run(async () =>
        {
            await gate.WaitAsync(cancellation);
            try
            {
                cancellation.ThrowIfCancellationRequested();
                if (TryPrepared(video.Id, out var previous)) return CopyFrame(previous.Frame);
                bool cached = cache.TryGet(video.Id, out string metadata);
                for (int attempt = 0; ; attempt++)
                {
                    if (!cached)
                    {
                        using var resolveTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
                        resolveTimeout.CancelAfter(TimeSpan.FromSeconds(20));
                        try { metadata = await (Resolve?.Invoke(video, resolveTimeout.Token) ?? new YouTubeAudioService(tools).ResolvePreviewAsync(video, resolveTimeout.Token)); }
                        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested) { throw new TimeoutException("Preview resolution timed out"); }
                        cache.Store(video.Id, metadata);
                    }
                    try
                    {
                        byte[] pcm = await (PreparePcmForTest?.Invoke(video, metadata, cancellation) ?? DecodeSilently(metadata, cancellation));
                        cancellation.ThrowIfCancellationRequested();
                        int length = Math.Min(MaximumPcmBytes, pcm.Length) / 4 * 4;
                        if (length == 0) throw new IOException("Preview decoding failed");
                        if (length != pcm.Length) pcm = pcm[..length];
                        var peaks = new YouTubePreviewPeaks(); peaks.Append(pcm);
                        var frame = peaks.Snapshot(length / 176400d, 0);
                        StorePrepared(video.Id, new(pcm, frame));
                        return CopyFrame(frame);
                    }
                    catch (Exception) when (cached && attempt == 0 && !cancellation.IsCancellationRequested)
                    { cache.Remove(video.Id); cached = false; }
                }
            }
            finally { gate.Release(); }
        }, cancellation);

    private bool TryPrepared(string id, out PreparedPreview preview)
    {
        if (prepared.TryGetValue(id, out preview!)) { preparedOrder.Remove(id); preparedOrder.AddLast(id); return true; }
        return false;
    }
    private void StorePrepared(string id, PreparedPreview preview)
    {
        prepared[id] = preview; preparedOrder.Remove(id); preparedOrder.AddLast(id);
        // Three 30-second stereo PCM16 excerpts occupy at most 15.9 MB.
        while (preparedOrder.Count > 3) { prepared.Remove(preparedOrder.First!.Value); preparedOrder.RemoveFirst(); }
    }
    private static YouTubePreviewFrame CopyFrame(YouTubePreviewFrame frame) => frame with { Peaks = (float[])frame.Peaks.Clone(), Position = 0 };
    private ProcessStartInfo CreateDecodeInfo(string metadata)
    {
        using var json = JsonDocument.Parse(metadata);
        string url = json.RootElement.GetProperty("url").GetString() ?? throw new IOException("Missing stream");
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != "https") throw new IOException("Invalid stream");
        var args = new List<string> { "-hide_banner", "-loglevel", "error", "-nostdin", "-rw_timeout", "10000000" };
        if (json.RootElement.TryGetProperty("http_headers", out var headers))
        {
            string value = string.Concat(headers.EnumerateObject().Where(h => h.Value.ValueKind == JsonValueKind.String)
                .Select(h => h.Name.Replace("\r", "").Replace("\n", "") + ": " + h.Value.GetString()!.Replace("\r", "").Replace("\n", "") + "\r\n"));
            args.AddRange(["-headers", value]);
        }
        args.AddRange(["-i", url, "-t", "30", "-vn", "-ac", "2", "-ar", "44100", "-f", "s16le", "pipe:1"]);
        return MediaProcess.CreateStartInfo(Path.Combine(tools, "ffmpeg.exe"), args);
    }
    private async Task<byte[]> DecodeSilently(string metadata, CancellationToken cancellation)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        timeout.CancelAfter(TimeSpan.FromSeconds(25));
        var token = timeout.Token;
        using var process = new Process { StartInfo = CreateDecodeInfo(metadata) };
        token.ThrowIfCancellationRequested(); process.Start();
        using var registration = token.Register(() => MediaProcess.Kill(process));
        var errors = process.StandardError.ReadToEndAsync();
        try
        {
            using var pcm = new MemoryStream(MaximumPcmBytes);
            byte[] block = new byte[32768];
            while (pcm.Length < MaximumPcmBytes)
            {
                int count = await process.StandardOutput.BaseStream.ReadAsync(block.AsMemory(0, (int)Math.Min(block.Length, MaximumPcmBytes - pcm.Length)), token);
                if (count == 0) break;
                pcm.Write(block, 0, count);
            }
            bool reachedLimit = pcm.Length == MaximumPcmBytes;
            if (reachedLimit) MediaProcess.Kill(process);
            await process.WaitForExitAsync(token); await errors;
            token.ThrowIfCancellationRequested();
            if ((!reachedLimit && process.ExitCode != 0) || pcm.Length < 4) throw new IOException("Preview decoding failed");
            return pcm.ToArray();
        }
        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested) { throw new TimeoutException("Preview preparation timed out"); }
        finally { MediaProcess.Kill(process); await process.WaitForExitAsync(CancellationToken.None); await errors; }
    }
    private async Task PlayPrepared(PreparedPreview preview, string outputId, Func<float> volume, Func<CancellationToken, Task> beforeStart,
        Action started, CancellationToken cancellation, Action<YouTubePreviewFrame>? progress)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        timeout.CancelAfter(TimeSpan.FromSeconds(preview.Frame.Duration + 10));
        var token = timeout.Token;
        try
        {
            await beforeStart(token).WaitAsync(token); token.ThrowIfCancellationRequested();
            using var enumerator = new MMDeviceEnumerator(); using var device = enumerator.GetDevice(outputId);
            using var output = new WasapiPlayerBuilder().WithDevice(device).WithSharedMode().WithLatency(80).Build();
            using var bytes = new MemoryStream(preview.Pcm, writable: false);
            using var wave = new RawSourceWaveStream(bytes, new WaveFormat(44100, 16, 2));
            var gain = new PreviewSamples(wave.ToSampleProvider(), volume, token);
            output.Init(new SampleToWaveProvider(gain));
            var stopped = new TaskCompletionSource<Exception?>(TaskCreationOptions.RunContinuationsAsynchronously);
            output.PlaybackStopped += (_, e) => stopped.TrySetResult(e.Exception);
            try
            {
                token.ThrowIfCancellationRequested();
                output.Play(); started(); var clock = Stopwatch.StartNew();
                while (!stopped.Task.IsCompleted)
                {
                    progress?.Invoke(CopyFrame(preview.Frame) with { Position = Math.Clamp(clock.Elapsed.TotalSeconds - .08, 0, preview.Frame.Duration) });
                    await Task.WhenAny(stopped.Task, Task.Delay(100, token)).WaitAsync(token);
                }
                if (await stopped.Task is { } error) throw error;
                progress?.Invoke(CopyFrame(preview.Frame) with { Position = preview.Frame.Duration });
            }
            finally { output.Stop(); }
        }
        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested) { throw new TimeoutException("Preview playback timed out"); }
    }
}
