using System.Text.Json;

namespace Cube.Desktop.Services;

internal sealed record YouTubePreviewFrame(float[] Peaks, double Duration, double DecodedDuration, double Position);

// Fixed 50 ms buckets of stereo PCM16 at 44.1 kHz, independent of output gain.
internal sealed class YouTubePreviewPeaks
{
    private readonly float[] peaks = Enumerable.Repeat(float.NaN, 600).ToArray();
    private long samples;
    private int lowByte = -1;
    public void Append(ReadOnlySpan<byte> bytes)
    {
        lock (peaks)
        {
            foreach (byte value in bytes)
            {
                if (lowByte < 0) { lowByte = value; continue; }
                short sample = unchecked((short)(lowByte | value << 8)); lowByte = -1;
                int bin = (int)(samples++ / 4410);
                if (bin >= peaks.Length) continue;
                float amplitude = Math.Abs((int)sample) / 32768f;
                peaks[bin] = float.IsNaN(peaks[bin]) ? amplitude : Math.Max(peaks[bin], amplitude);
            }
        }
    }
    public YouTubePreviewFrame Snapshot(double duration, double position)
    {
        lock (peaks)
        {
            int count = Math.Clamp((int)Math.Ceiling(duration / .05), 1, 600);
            return new(peaks[..count], duration, Math.Min(duration, samples / 88200d), Math.Clamp(position, 0, duration));
        }
    }
}

// Accessed by the single preview worker. Never persists signed URLs or headers.
internal sealed class YouTubePreviewCache
{
    private readonly Dictionary<string, (string Json, DateTimeOffset Expires, long Used)> entries = [];
    private long sequence;
    internal Func<DateTimeOffset> Now { get; set; } = () => DateTimeOffset.UtcNow;
    internal int Count => entries.Count;
    internal bool TryGet(string id, out string json)
    {
        if (entries.TryGetValue(id, out var item) && item.Expires > Now())
        {
            entries[id] = item with { Used = ++sequence }; json = item.Json; return true;
        }
        entries.Remove(id); json = ""; return false;
    }
    internal void Remove(string id) => entries.Remove(id);
    internal void Store(string id, string json)
    {
        using var document = JsonDocument.Parse(json);
        var url = new Uri(document.RootElement.GetProperty("url").GetString()!);
        if (url.Scheme != "https") throw new System.IO.IOException("Invalid stream");
        var expires = Now().AddMinutes(5);
        foreach (string part in url.Query.TrimStart('?').Split('&'))
        {
            var pair = part.Split('=', 2);
            if (pair.Length == 2 && pair[0] == "expire" && long.TryParse(pair[1], out long seconds))
            {
                var signedExpiry = DateTimeOffset.FromUnixTimeSeconds(seconds).AddSeconds(-10);
                if (signedExpiry < expires) expires = signedExpiry;
            }
        }
        entries[id] = (json, expires, ++sequence);
        if (entries.Count > 20) entries.Remove(entries.MinBy(e => e.Value.Used).Key);
    }
}
