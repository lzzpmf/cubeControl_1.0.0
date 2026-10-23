using System.IO;
using NAudio.Wave;

namespace Cube.Desktop.Services;

internal sealed class BackingDecoder : IDisposable
{
    private readonly WaveStream reader;
    private readonly ISampleProvider samples;
    public WaveFormat Format => samples.WaveFormat;
    public double Duration => reader.TotalTime.TotalSeconds;
    public BackingDecoder(string path)
    {
        if (Path.GetExtension(path).Equals(".wav", StringComparison.OrdinalIgnoreCase))
        {
            var wav = new WaveFileReader(path);
            if (wav.WaveFormat.Encoding is WaveFormatEncoding.Pcm or WaveFormatEncoding.IeeeFloat or WaveFormatEncoding.Extensible) reader = wav;
            else { wav.Dispose(); reader = new MediaFoundationReader(path, new() { RequestFloatOutput = true }); }
        }
        else reader = new MediaFoundationReader(path, new() { RequestFloatOutput = true });
        try { samples = reader.ToSampleProvider(); }
        catch { reader.Dispose(); throw; }
    }
    public void Seek(long frame) => reader.Position = Math.Clamp(frame * reader.WaveFormat.BlockAlign, 0, reader.Length);
    public int Read(float[] buffer, int offset, int count) => samples.Read(buffer.AsSpan(offset, count));
    public int Read(float[] buffer) => samples.Read(buffer);
    public int Read(Span<float> buffer) => samples.Read(buffer);
    public void Dispose() => reader.Dispose();
}

