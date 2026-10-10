using Cube.Desktop.Services;

internal static class YouTubePreviewDataTests
{
    private static void Check(bool value, string message)
    { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); }
    internal static async Task Run()
    {
        var peaks = new YouTubePreviewPeaks();
        byte[] pcm = new byte[4410 * 2 * 2];
        // First bucket is silence; second contains a negative full-scale sample.
        pcm[4410 * 2 + 1] = 128;
        peaks.Append(pcm.AsSpan(0, 17)); peaks.Append(pcm.AsSpan(17));
        var frame = peaks.Snapshot(30, .08);
        Check(frame.Peaks[0] == 0 && frame.Peaks[1] == 1 && float.IsNaN(frame.Peaks[2]), "PCM peaks preserve silence, full scale and unknown regions across odd byte blocks");
        Check(Math.Abs(frame.DecodedDuration - .1) < .0001 && frame.Position == .08, "decoded duration is distinct from playback position");
        Check(peaks.Snapshot(.075, 1).Peaks.Length == 2 && peaks.Snapshot(.075, 1).Position == .075, "short excerpts and positions are bounded");
        var now = DateTimeOffset.FromUnixTimeSeconds(1900000000);
        var cache = new YouTubePreviewCache { Now = () => now };
        string Json(long expiry) => "{\"url\":\"https://example.test/audio?expire=" + expiry + "\",\"http_headers\":{\"Origin\":\"https://example.test\"}}";
        string metadata = Json(now.AddHours(1).ToUnixTimeSeconds());
        cache.Store("a", metadata);
        Check(cache.TryGet("a", out var restored) && restored == metadata, "stream cache preserves URL and headers");
        now = now.AddMinutes(5);
        Check(!cache.TryGet("a", out _), "stream cache expires after five minutes");
        cache.Store("short", Json(now.AddSeconds(20).ToUnixTimeSeconds())); now = now.AddSeconds(11);
        Check(!cache.TryGet("short", out _), "signed expiry takes precedence with safety margin");
        for (int i = 0; i < 20; i++) cache.Store(i.ToString(), metadata);
        cache.TryGet("0", out _); cache.Store("20", metadata);
        Check(cache.Count == 20 && cache.TryGet("0", out _) && !cache.TryGet("1", out _), "cache evicts least recently used entry at twenty items");

        int resolves = 0, decodes = 0;
        bool fail = false, failAfterStart = false;
        var service = new YouTubePreviewService
        {
            Resolve = (_, _) => { resolves++; return Task.FromResult(metadata); },
            DecodeForTest = (_, started, token) =>
            {
                decodes++;
                if (failAfterStart) { started(); throw new System.IO.IOException("after start"); }
                if (fail) { fail = false; throw new System.IO.IOException("expired URL"); }
                started(); return Task.CompletedTask;
            }
        };
        var video = new YouTubeResult("abcdefghijk", "Fixture", "Channel", 60, null);
        Task Play(CancellationToken token = default) => service.PlayAsync(video, "fixture", () => 0, _ => Task.CompletedTask, () => { }, token);
        await Play(); await Play();
        Check(resolves == 1 && decodes == 2, "repeated preview reuses resolved metadata");
        fail = true; await Play();
        Check(resolves == 2 && decodes == 4, "cached decode failure resolves once and retries before playback");
        failAfterStart = true;
        try { await Play(); throw new Exception("expected decode error"); } catch (System.IO.IOException) { }
        Check(resolves == 2 && decodes == 5, "failure after playback starts is never retried");
        using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
        try { await Play(cancelled.Token); throw new Exception("expected cancellation"); } catch (OperationCanceledException) { }
        Check(decodes == 5, "cancelled request never starts decoder");
    }
}
