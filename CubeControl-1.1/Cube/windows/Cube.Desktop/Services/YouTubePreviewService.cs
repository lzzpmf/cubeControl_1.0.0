using System.Diagnostics;
using System.IO;
using System.Text.Json;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Cube.Desktop.Services;

internal interface IYouTubePreviewService
{
    Task PlayAsync(YouTubeResult video, string outputId, Func<float> volume, Func<CancellationToken, Task> beforeStart, Action started, CancellationToken cancellation);
}

// One worker owns the decoder and audio endpoint until both have been released.
// PCM is streamed through a bounded buffer; no preview files are created.
internal sealed class YouTubePreviewService(string? toolsDirectory = null) : IYouTubePreviewService
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string tools = toolsDirectory ?? Path.Combine(AppContext.BaseDirectory, "Tools", "YouTube");
    public Task PlayAsync(YouTubeResult video, string outputId, Func<float> volume, Func<CancellationToken, Task> beforeStart, Action started, CancellationToken cancellation)
        => Task.Run(async () =>
        {
            await gate.WaitAsync(cancellation);
            try { await Play(video, outputId, volume, beforeStart, started, cancellation); }
            finally { gate.Release(); }
        }, cancellation);

    private async Task Play(YouTubeResult video, string outputId, Func<float> volume, Func<CancellationToken, Task> beforeStart, Action started, CancellationToken cancellation)
    {
        using var preparation = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        preparation.CancelAfter(TimeSpan.FromSeconds(20));
        var token = preparation.Token;
        try
        {
            var metadata = await new YouTubeAudioService(tools).ResolvePreviewAsync(video, token);
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
            var info = new ProcessStartInfo(Path.Combine(tools, "ffmpeg.exe"))
            { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
            foreach (string arg in args) info.ArgumentList.Add(arg);
            using var process = new Process { StartInfo = info };
            token.ThrowIfCancellationRequested();
            process.Start();
            using var registration = token.Register(() => Kill(process));
            var errors = process.StandardError.ReadToEndAsync();
            try
            {
                using var enumerator = new MMDeviceEnumerator();
                using var device = enumerator.GetDevice(outputId);
                using var output = new WasapiPlayerBuilder().WithDevice(device).WithSharedMode().WithLatency(80).Build();
                var buffer = new BufferedWaveProvider(new WaveFormat(44100, 16, 2), TimeSpan.FromSeconds(2));
                var gain = new PreviewSamples(buffer.ToSampleProvider(), volume, token);
                output.Init(new SampleToWaveProvider(gain));
                var stopped = new TaskCompletionSource<Exception?>(TaskCreationOptions.RunContinuationsAsynchronously);
                output.PlaybackStopped += (_, e) => stopped.TrySetResult(e.Exception);
                byte[] block = new byte[8192];
                bool playing = false;
                long total = 0;
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
                        buffer.AddSamples(block.AsSpan(0, count)); total += count;
                        if (!playing && buffer.BufferedDuration.TotalMilliseconds >= 200)
                        {
                            token.ThrowIfCancellationRequested();
                            await beforeStart(token).WaitAsync(token);
                            token.ThrowIfCancellationRequested();
                            output.Play(); playing = true;
                            preparation.CancelAfter(TimeSpan.FromSeconds(35));
                            started();
                        }
                    }
                    await process.WaitForExitAsync(token);
                    await errors;
                    if (process.ExitCode != 0 || total == 0) throw new IOException("Preview decoding failed");
                    if (!playing)
                    {
                        await beforeStart(token).WaitAsync(token);
                        token.ThrowIfCancellationRequested(); output.Play(); started();
                        preparation.CancelAfter(TimeSpan.FromSeconds(35));
                    }
                    while (buffer.BufferedBytes > 0)
                    {
                        if (stopped.Task.IsCompleted) throw await stopped.Task ?? new IOException("Preview output stopped");
                        await Task.Delay(20, token);
                    }
                    await Task.Delay(100, token);
                }
                finally { output.Stop(); }
            }
            finally
            {
                Kill(process);
                await process.WaitForExitAsync(CancellationToken.None);
                await errors;
            }
        }
        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
        { throw new TimeoutException("Preview preparation or playback timed out"); }
    }

    private static void Kill(Process process)
    {
        try { if (!process.HasExited) process.Kill(true); }
        catch (InvalidOperationException) { }
        catch (System.ComponentModel.Win32Exception) { }
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
