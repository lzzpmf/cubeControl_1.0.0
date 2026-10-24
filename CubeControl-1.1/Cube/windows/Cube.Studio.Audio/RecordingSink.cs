using System.Text.Json;
using NAudio.Wave;

namespace Cube.Studio.Audio;

internal sealed record RecordingJournal(Guid TrackId, string Path, double Start, double RecordedSpeed,
    double RecordedPitch, int SampleRate, int Channels, long LoopPass, double ClockRatio = 1);

internal sealed class RecordingSink : IDisposable
{
    private readonly StudioRecordingRequest request;
    private readonly StudioSnapshot snapshot;
    private readonly int rate, channels;
    private readonly double anchorPosition, correction;
    private double anchorTime, anchorOutput, outputOrigin, outputClockRatio = 1, firstClockTime = double.NaN, firstClockOutput;
    private readonly string directory;
    private readonly List<StudioRecordedSegment> complete = [];
    private readonly List<float> peaks = [];
    private WaveFileWriter? writer;
    private string path = "", journal = "";
    private long segmentFrames, pass = -1, lastDeviceFrame = -1, firstDeviceFrame = -1;
    private double segmentStart, firstPacketTime, lastPacketTime;
    private int peakCount, peakStride = 1;
    private float peak;
    private bool finished;
    private readonly float[] silence = new float[8192];
    private readonly Func<string, WaveFormat, WaveFileWriter> createWriter;
    internal float InputPeak { get; private set; }

    internal RecordingSink(StudioRecordingRequest request, StudioSnapshot snapshot, int rate, int channels,
        double anchorTime, double anchorPosition, double correction, string defaultDirectory,
        Func<string, WaveFormat, WaveFileWriter>? createWriter = null)
    {
        this.request = request; this.snapshot = snapshot; this.rate = rate; this.channels = channels;
        this.anchorTime = anchorTime; this.anchorPosition = anchorPosition; this.correction = correction;
        this.createWriter = createWriter ?? ((path, format) => new(path, format, new WaveFileWriterOptions { EnableRf64 = true }));
        directory = request.Directory ?? defaultDirectory; Directory.CreateDirectory(directory);
    }

    // Called on the worker after output starts and before consuming queued packets.
    // WASAPI QPC is correlated with the render clock; ASIO supplies the shared output frame clock directly.
    internal void UpdateClock(double outputSeconds, double monotonicSeconds, double origin)
    {
        if (double.IsNaN(firstClockTime)) { firstClockTime = monotonicSeconds; firstClockOutput = outputSeconds; }
        double elapsed = monotonicSeconds - firstClockTime;
        if (elapsed >= 1)
        {
            double ratio = (outputSeconds - firstClockOutput) / elapsed;
            if (ratio is >= .98 and <= 1.02) outputClockRatio = ratio;
        }
        anchorTime = monotonicSeconds; anchorOutput = outputSeconds - origin; outputOrigin = origin;
    }

    internal void Process(CapturePacket packet)
    {
        if (finished) return;
        if (packet.Discontinuity && firstDeviceFrame >= 0) throw new IOException("A entrada de áudio foi interrompida; a tomada parcial foi preservada.");
        double outputSeconds = double.IsFinite(packet.OutputSeconds) ? packet.OutputSeconds - outputOrigin
            : anchorOutput + (packet.Time - anchorTime) * outputClockRatio;
        if (firstDeviceFrame < 0) { firstDeviceFrame = packet.DeviceFrame; firstPacketTime = outputSeconds; }
        lastDeviceFrame = packet.DeviceFrame; lastPacketTime = outputSeconds;
        InputPeak = 0;
        for (int i = 0; i < packet.Frames * channels; i++) InputPeak = Math.Max(InputPeak, Math.Abs(packet.Samples[i]));
        int f = 0;
        while (f < packet.Frames)
        {
            double linear = anchorPosition + (outputSeconds + f / (double)rate * ClockRatio + correction) * snapshot.Speed;
            if (linear < anchorPosition)
            {
                f += Math.Max(1, (int)Math.Min(packet.Frames - f, Math.Ceiling((anchorPosition - linear) * rate / (snapshot.Speed * ClockRatio))));
                continue;
            }
            long currentPass = 0;
            double position = linear;
            double boundary = double.PositiveInfinity;
            if (snapshot.LoopEnabled && snapshot.LoopEnd > snapshot.LoopStart)
            {
                if (linear >= snapshot.LoopEnd)
                {
                    double length = snapshot.LoopEnd - snapshot.LoopStart;
                    currentPass = 1 + (long)Math.Floor((linear - snapshot.LoopEnd) / length);
                    position = snapshot.LoopStart + (linear - snapshot.LoopEnd) % length;
                }
                boundary = snapshot.LoopEnd;
            }
            if (currentPass != pass)
            {
                CloseSegment(false);
                pass = currentPass; segmentStart = currentPass == 0 ? anchorPosition : snapshot.LoopStart;
                path = Path.Combine(directory, "take-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N") + ".wav");
                journal = path + ".recording.json";
                File.WriteAllText(journal, JsonSerializer.Serialize(new RecordingJournal(request.TrackId, path, segmentStart,
                    snapshot.Speed, snapshot.Semitones, rate, channels, pass)));
                writer = createWriter(path, WaveFormat.CreateIeeeFloatWaveFormat(rate, channels));
                // Preserve the requested take origin. Device startup or a positive calibration
                // offset becomes leading silence; a negative offset was trimmed above.
                int padding = checked((int)Math.Max(0, Math.Round((position - segmentStart) * rate / (snapshot.Speed * ClockRatio))));
                while (padding > 0)
                {
                    int part = Math.Min(padding, silence.Length / channels);
                    WriteFrames(silence, 0, part); padding -= part;
                }
            }
            int count = (int)Math.Min(packet.Frames - f, double.IsFinite(boundary) ? Math.Max(1, Math.Ceiling((boundary - position) * rate / (snapshot.Speed * ClockRatio) - 1e-8)) : packet.Frames - f);
            WriteFrames(packet.Samples, f, count); f += count;
        }
    }

    private void WriteFrames(float[] samples, int offset, int count)
    {
        writer!.WriteSamples(samples, offset * channels, count * channels);
        for (int j = offset; j < offset + count; j++)
            {
                for (int c = 0; c < channels; c++) peak = Math.Max(peak, Math.Abs(samples[j * channels + c]));
                if (++peakCount >= Math.Max(1, rate / 100) * peakStride)
                {
                    peaks.Add(peak); peak = 0; peakCount = 0;
                    if (peaks.Count >= 32768)
                    {
                        int half = peaks.Count / 2;
                        for (int k = 0; k < half; k++) peaks[k] = Math.Max(peaks[k * 2], peaks[k * 2 + 1]);
                        peaks.RemoveRange(half, peaks.Count - half); peakStride *= 2;
                    }
                }
            }
        segmentFrames += count;
    }

    private double ClockRatio
    {
        get
        {
            if (lastDeviceFrame - firstDeviceFrame < rate) return 1;
            double ratio = (lastPacketTime - firstPacketTime) / ((lastDeviceFrame - firstDeviceFrame) / (double)rate);
            return double.IsFinite(ratio) && ratio is >= .98 and <= 1.02 ? ratio : 1;
        }
    }
    private void CloseSegment(bool incomplete)
    {
        if (writer == null) return;
        writer.Dispose(); writer = null;
        if (peakCount > 0) peaks.Add(peak);
        double sourceDuration = segmentFrames / (double)rate;
        if (segmentFrames > 0)
            complete.Add(new(request.TrackId, path, segmentStart, sourceDuration * snapshot.Speed * ClockRatio,
                sourceDuration, snapshot.Speed, snapshot.Semitones, ClockRatio, pass, incomplete, peaks.ToArray()));
        try
        {
            string temporary = journal + ".tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(new RecordingJournal(request.TrackId, path, segmentStart,
                snapshot.Speed, snapshot.Semitones, rate, channels, pass, ClockRatio)));
            File.Move(temporary, journal, true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { System.Diagnostics.Debug.WriteLine(ex); }
        // The session owner acknowledges this journal only after an atomic project save.
        // Stopping successfully must still be recoverable if the app closes before Save.
        segmentFrames = 0; peaks.Clear(); peak = 0; peakCount = 0; peakStride = 1;
    }
    internal IReadOnlyList<StudioRecordedSegment> Finish(bool incomplete)
    {
        if (!finished) { CloseSegment(incomplete); finished = true; }
        return complete.ToArray();
    }
    public void Dispose() { if (!finished) Finish(true); }
}

public static class StudioRecordingRecovery
{
    public static IReadOnlyList<string> Find(string directory) => Directory.Exists(directory)
        ? Directory.GetFiles(directory, "*.recording.json", SearchOption.TopDirectoryOnly) : [];

    public static StudioRecordedSegment Recover(string journalPath)
    {
        var journal = JsonSerializer.Deserialize<RecordingJournal>(File.ReadAllText(journalPath)) ?? throw new InvalidDataException("Registro de gravação inválido.");
        string parent = Path.GetFullPath(Path.GetDirectoryName(journalPath)!);
        string source = Path.GetFullPath(journal.Path);
        if (!string.Equals(Path.GetDirectoryName(source), parent, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("A gravação não pertence à pasta de recuperação.");
        string destination = Path.Combine(parent, Path.GetFileNameWithoutExtension(source) + "-recovered-" + Guid.NewGuid().ToString("N") + ".wav");
        RepairCopy(source, destination);
        using var reader = new WaveFileReader(destination);
        double seconds = reader.TotalTime.TotalSeconds;
        double ratio = double.IsFinite(journal.ClockRatio) && journal.ClockRatio is >= .5 and <= 2 ? journal.ClockRatio : 1;
        return new(journal.TrackId, destination, journal.Start, seconds * journal.RecordedSpeed * ratio, seconds,
            journal.RecordedSpeed, journal.RecordedPitch, ratio, journal.LoopPass, true, MediaLibrary.ReadPeaks(destination, CancellationToken.None));
    }

    private static void RepairCopy(string source, string destination)
    {
        File.Copy(source, destination, overwrite: false);
        using var stream = new FileStream(destination, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        using var reader = new BinaryReader(stream, System.Text.Encoding.ASCII, true);
        using var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, true);
        if (stream.Length < 44) throw new InvalidDataException("WAV inválido.");
        uint signature = reader.ReadUInt32();
        if (signature is not (0x46464952 or 0x34364652)) throw new InvalidDataException("WAV inválido.");
        stream.Position = 8; if (reader.ReadUInt32() != 0x45564157) throw new InvalidDataException("WAV inválido.");
        long fact = -1, rf64 = -1; int align = 0;
        while (stream.Position + 8 <= stream.Length)
        {
            uint type = reader.ReadUInt32(), length = reader.ReadUInt32(); long content = stream.Position;
            if (type == 0x20746D66 && length >= 16) { stream.Position = content + 12; align = reader.ReadUInt16(); }
            if (type == 0x74636166 && length >= 4) fact = content;
            if ((type == 0x34367364 || type == 0x4B4E554A && content == 20) && length >= 28) rf64 = content;
            if (type == 0x61746164)
            {
                if (align <= 0) throw new InvalidDataException("Formato WAV ausente.");
                long bytes = (stream.Length - content) / align * align;
                if (bytes <= 0) throw new InvalidDataException("Tomada sem dados recuperáveis.");
                bool large = content + bytes - 8 > uint.MaxValue || signature == 0x34364652;
                if (large && rf64 < 0) throw new InvalidDataException("A tomada longa não reservou um cabeçalho RF64.");
                stream.Position = content - 4; writer.Write(large ? uint.MaxValue : (uint)bytes);
                stream.Position = 4; writer.Write(large ? uint.MaxValue : checked((uint)(content + bytes - 8)));
                if (large)
                {
                    stream.Position = 0; writer.Write(0x34364652U);
                    stream.Position = rf64 - 8; writer.Write(0x34367364U); writer.Write(28U);
                    writer.Write((ulong)(content + bytes - 8)); writer.Write((ulong)bytes); writer.Write((ulong)(bytes / align)); writer.Write(0U);
                }
                if (fact >= 0) { stream.Position = fact; writer.Write((uint)Math.Min(uint.MaxValue, bytes / align)); }
                stream.SetLength(content + bytes); return;
            }
            if (length > stream.Length - content) break;
            stream.Position = content + length + (length & 1);
        }
        throw new InvalidDataException("Dados de áudio ausentes.");
    }
}
