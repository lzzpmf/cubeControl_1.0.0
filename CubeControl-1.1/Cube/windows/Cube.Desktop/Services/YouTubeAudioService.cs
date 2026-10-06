using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace Cube.Desktop.Services;

public sealed record YouTubeResult(string Id, string Title, string Channel, double Duration, string? Thumbnail)
{
    public string Url => "https://www.youtube.com/watch?v=" + Id;
    public string DurationLabel => TimeSpan.FromSeconds(Duration).ToString(Duration >= 3600 ? @"h\:mm\:ss" : @"m\:ss");
}
public sealed record YouTubeProgress(string Stage, double Percent);
public interface IYouTubeAudioService
{
    Task<IReadOnlyList<YouTubeResult>> SearchAsync(string query, CancellationToken cancellation);
    Task DownloadAsync(YouTubeResult video, string destination, IProgress<YouTubeProgress> progress, CancellationToken cancellation);
}

public sealed class YouTubeAudioService(string? toolsDirectory = null) : IYouTubeAudioService
{
    internal Func<string, IEnumerable<string>, CancellationToken, Action<string>?, Task<string>> ProcessRunner { get; set; } = Run;
    private readonly string tools = toolsDirectory ?? Path.Combine(AppContext.BaseDirectory, "Tools", "YouTube");
    private string Tool(string name)
    {
        string path = Path.Combine(tools, name + ".exe");
        if (!File.Exists(path)) throw new FileNotFoundException("Ferramentas do YouTube ausentes. Reinstale o aplicativo.");
        return path;
    }
    private IEnumerable<string> Common() => new[] { "--ignore-config", "--no-plugin-dirs", "--no-remote-components", "--no-update", "--no-playlist", "--no-warnings", "--socket-timeout", "20", "--retries", "2", "--js-runtimes", "deno:" + Tool("deno"), "--ffmpeg-location", tools };
    public async Task<IReadOnlyList<YouTubeResult>> SearchAsync(string query, CancellationToken cancellation)
    {
        var json = await ProcessRunner(Tool("yt-dlp"), Common().Concat(new[] { "--flat-playlist", "--dump-single-json", "--", "ytsearch10:" + query }), cancellation, null);
        using var document = JsonDocument.Parse(json);
        var results = new List<YouTubeResult>();
        foreach (var item in document.RootElement.GetProperty("entries").EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object || Text(item, "live_status") is "is_live" or "is_upcoming" or "post_live") continue;
            string id = Text(item, "id");
            if (!System.Text.RegularExpressions.Regex.IsMatch(id, "^[A-Za-z0-9_-]{11}$")) continue;
            string? thumbnail = null;
            if (item.TryGetProperty("thumbnails", out var thumbs) && thumbs.ValueKind == JsonValueKind.Array)
                thumbnail = thumbs.EnumerateArray().Select(t => Text(t, "url")).FirstOrDefault(u => u.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
            results.Add(new(id, Text(item, "title"), Text(item, "channel") is { Length: > 0 } channel ? channel : Text(item, "uploader"),
                item.TryGetProperty("duration", out var duration) && duration.ValueKind == JsonValueKind.Number && duration.TryGetDouble(out var seconds) ? Math.Max(0, seconds) : 0, thumbnail));
        }
        return results.Take(10).ToArray();
    }
    private static string Text(JsonElement element, string name) => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString()! : "";
    public async Task DownloadAsync(YouTubeResult video, string destination, IProgress<YouTubeProgress> progress, CancellationToken cancellation)
    {
        Tool("ffmpeg"); Tool("ffprobe");
        string parent = Path.GetDirectoryName(Path.GetFullPath(destination))!;
        string temporary = Path.Combine(parent, ".cube-youtube-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporary);
        try
        {
            progress.Report(new("Baixando áudio…", 0));
            await ProcessRunner(Tool("yt-dlp"), Common().Concat(new[] { "--no-simulate", "--match-filters", "!is_live & !is_upcoming", "-f", "bestaudio/best", "-x", "--audio-format", "mp3", "--audio-quality", "320K", "--newline", "--progress", "--progress-template", "download:PROGRESS:%(progress._percent_str)s", "--print", "before_dl:DOWNLOAD", "--print", "post_process:CONVERT", "-o", Path.Combine(temporary, "audio.%(ext)s"), "--", video.Url }), cancellation, line =>
            {
                if (line.StartsWith("PROGRESS:") && double.TryParse(line[9..].Trim().TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out var percent)) progress.Report(new("Baixando áudio…", percent));
                if (line.Contains("ExtractAudio") || line == "CONVERT") progress.Report(new("Convertendo para MP3…", 100));
            });
            string file = Path.Combine(temporary, "audio.mp3");
            if (!File.Exists(file) || new FileInfo(file).Length == 0) throw new IOException("Não foi possível converter o áudio para MP3.");
            var metadata = await ProcessRunner(Tool("ffprobe"), new[] { "-v", "error", "-select_streams", "a:0", "-show_entries", "stream=codec_name,bit_rate:format=duration", "-of", "json", file }, cancellation, null);
            using var probe = JsonDocument.Parse(metadata);
            var stream = probe.RootElement.GetProperty("streams")[0];
            if (Text(stream, "codec_name") != "mp3" || !int.TryParse(Text(stream, "bit_rate"), out int bitrate) || bitrate != 320000 || !double.TryParse(Text(probe.RootElement.GetProperty("format"), "duration"), CultureInfo.InvariantCulture, out double length) || length <= 0)
                throw new IOException("Não foi possível converter o áudio para MP3.");
            cancellation.ThrowIfCancellationRequested();
            if (File.Exists(destination)) File.Replace(file, destination, null); else File.Move(file, destination);
        }
        finally { try { Directory.Delete(temporary, true); } catch (IOException) { } catch (UnauthorizedAccessException) { } }
    }
    private static async Task<string> Run(string executable, IEnumerable<string> arguments, CancellationToken cancellation, Action<string>? output = null)
    {
        var info = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true, StandardOutputEncoding = System.Text.Encoding.UTF8, StandardErrorEncoding = System.Text.Encoding.UTF8 };
        foreach (string argument in arguments) info.ArgumentList.Add(argument);
        using var process = new Process { StartInfo = info };
        cancellation.ThrowIfCancellationRequested(); process.Start();
        using var registration = cancellation.Register(() => { try { if (!process.HasExited) process.Kill(true); } catch (InvalidOperationException) { } catch (System.ComponentModel.Win32Exception) { } });
        var stdout = new System.Text.StringBuilder();
        async Task Read(StreamReader reader, bool collect)
        {
            while (await reader.ReadLineAsync(cancellation) is { } line)
            {
                if (collect) stdout.AppendLine(line);
                output?.Invoke(line);
            }
        }
        try { await Task.WhenAll(Read(process.StandardOutput, true), Read(process.StandardError, false), process.WaitForExitAsync(cancellation)); }
        finally { if (!process.HasExited) { process.Kill(true); await process.WaitForExitAsync(CancellationToken.None); } }
        cancellation.ThrowIfCancellationRequested();
        if (process.ExitCode != 0) throw new IOException("YouTube indisponível ou vídeo restrito. Verifique a internet e tente novamente.");
        return stdout.ToString();
    }
}
