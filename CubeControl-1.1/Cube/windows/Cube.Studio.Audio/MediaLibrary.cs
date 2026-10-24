using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NAudio.Wave;

namespace Cube.Studio.Audio;

internal sealed record PreparedMedia(string OriginalPath, string PcmPath, double Duration, int Channels, int SampleRate,
    int SourceSampleRate = 48000, int SourceChannels = 2);

internal sealed class MediaLibrary(string dataDirectory, string? toolsDirectory)
{
    private readonly ConcurrentDictionary<string, PreparedMedia> prepared = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim preparation = new(1, 1);
    private readonly string cacheDirectory = Path.Combine(dataDirectory, "StudioCache");
    internal string Tool(string name)
    {
        string path = Path.Combine(toolsDirectory ?? Path.Combine(AppContext.BaseDirectory, "Tools", "YouTube"), name + ".exe");
        return File.Exists(path) ? path : throw new FileNotFoundException("Ferramentas de áudio ausentes. Reinstale o pacote completo do CubeControl.", path);
    }

    internal async Task<PreparedMedia> Prepare(string path, CancellationToken cancellation)
    {
        path = Path.GetFullPath(path);
        var info = new FileInfo(path);
        if (!info.Exists) throw new FileNotFoundException("Arquivo de áudio ausente: " + info.Name, path);
        string identity = path + "|" + info.Length + "|" + info.LastWriteTimeUtc.Ticks;
        if (prepared.TryGetValue(identity, out var found) && File.Exists(found.PcmPath)) return found;
        await preparation.WaitAsync(cancellation).ConfigureAwait(false);
        try
        {
            if (prepared.TryGetValue(identity, out found) && File.Exists(found.PcmPath)) return found;
            Directory.CreateDirectory(cacheDirectory);
            string key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
            string destination = Path.Combine(cacheDirectory, key + ".wav");
            string metadata = await MediaProcess.Run(Tool("ffprobe"), ["-v", "error", "-select_streams", "a:0", "-show_entries", "stream=channels,sample_rate:format=duration", "-of", "json", path], cancellation).ConfigureAwait(false);
            using var document = JsonDocument.Parse(metadata);
            if (!document.RootElement.TryGetProperty("streams", out var streams) || streams.GetArrayLength() == 0)
                throw new InvalidDataException("O arquivo não contém áudio.");
            int sourceChannels = streams[0].GetProperty("channels").GetInt32();
            int sourceRate = int.Parse(streams[0].GetProperty("sample_rate").GetString()!, CultureInfo.InvariantCulture);
            int channels = Math.Clamp(sourceChannels, 1, 2);
            if (!File.Exists(destination))
            {
                string temporary = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
                try
                {
                    await MediaProcess.Run(Tool("ffmpeg"), ["-hide_banner", "-loglevel", "error", "-nostdin", "-i", path,
                        "-map", "0:a:0", "-vn", "-ar", "48000", "-ac", channels.ToString(CultureInfo.InvariantCulture),
                        "-c:a", "pcm_f32le", "-rf64", "auto", "-f", "wav", temporary], cancellation).ConfigureAwait(false);
                    cancellation.ThrowIfCancellationRequested();
                    File.Move(temporary, destination, overwrite: false);
                }
                finally { if (File.Exists(temporary)) File.Delete(temporary); }
            }
            using var reader = new WaveFileReader(destination);
            double duration = reader.TotalTime.TotalSeconds;
            if (!double.IsFinite(duration) || duration <= 0 || duration > 86400)
                throw new InvalidDataException("Duração do áudio fora do limite de 24 horas.");
            found = new(path, destination, duration, reader.WaveFormat.Channels, reader.WaveFormat.SampleRate, sourceRate, sourceChannels);
            prepared[identity] = found;
            return found;
        }
        finally { preparation.Release(); }
    }

    internal async Task<StudioMediaInfo> Analyze(string path, CancellationToken cancellation)
    {
        var media = await Prepare(path, cancellation).ConfigureAwait(false);
        return await Task.Run(() =>
        {
            var peaks = ReadPeaks(media.PcmPath, cancellation);
            var music = MusicAnalysis.Analyze(media.PcmPath, cancellation);
            return new StudioMediaInfo(media.OriginalPath, media.Duration, media.SourceSampleRate, media.SourceChannels,
                peaks, music.Bpm > 0 && music.TempoConfidence >= .25 ? music.Bpm : null,
                music.KeyConfidence >= .25 && music.Key != "Não definido" ? music.Key : null,
                music.TempoConfidence, music.KeyConfidence);
        }, cancellation).ConfigureAwait(false);
    }

    internal static float[] ReadPeaks(string path, CancellationToken cancellation)
    {
        using var decoder = new StudioDecoder(path);
        int channels = decoder.Format.Channels;
        long total = Math.Max(1, (long)Math.Round(decoder.Duration * decoder.Format.SampleRate));
        int bins = (int)Math.Clamp(Math.Ceiling(decoder.Duration * 100), 1, 65536);
        var peaks = new float[bins]; var samples = new float[4096 * channels]; long frame = 0;
        int count;
        while ((count = decoder.Read(samples)) > 0)
        {
            cancellation.ThrowIfCancellationRequested();
            for (int i = 0; i + channels <= count; i += channels, frame++)
            {
                float peak = 0;
                for (int c = 0; c < channels; c++) if (float.IsFinite(samples[i + c])) peak = Math.Max(peak, Math.Abs(samples[i + c]));
                int bin = (int)Math.Min(bins - 1, frame * bins / total);
                peaks[bin] = Math.Max(peaks[bin], peak);
            }
        }
        return peaks;
    }
}

internal static class MediaProcess
{
    internal static ProcessStartInfo StartInfo(string executable, IEnumerable<string> arguments)
    {
        var info = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true, RedirectStandardInput = true };
        foreach (var argument in arguments) info.ArgumentList.Add(argument);
        return info;
    }
    internal static void Kill(Process process) { try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { } }
    internal static async Task<string> Run(string executable, IEnumerable<string> arguments, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        using var process = new Process { StartInfo = StartInfo(executable, arguments) };
        process.Start(); process.StandardInput.Close();
        using var registration = cancellation.Register(() => Kill(process));
        var stdout = process.StandardOutput.ReadToEndAsync(cancellation);
        var stderr = process.StandardError.ReadToEndAsync(cancellation);
        try
        {
            await process.WaitForExitAsync(cancellation).ConfigureAwait(false);
            string result = await stdout.ConfigureAwait(false), error = await stderr.ConfigureAwait(false);
            if (process.ExitCode != 0) throw new IOException("Não foi possível processar o áudio. " + error[..Math.Min(error.Length, 500)]);
            return result;
        }
        finally
        {
            Kill(process);
            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }
}
