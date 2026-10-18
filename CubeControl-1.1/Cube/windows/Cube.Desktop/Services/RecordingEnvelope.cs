using System.Buffers.Binary;
using NAudio.Wave;

namespace Cube.Desktop.Services;

// Bounded, incremental envelope; compaction preserves peaks, never raw audio.
internal sealed class RecordingEnvelope(WaveFormat format)
{
    private readonly object sync = new();
    private readonly List<float> peaks = [];
    private int window = Math.Max(1, format.SampleRate / 100), inWindow;
    private float peak;
    public void Write(ReadOnlySpan<byte> data, bool silent)
    {
        lock (sync)
        {
            int bytes = format.BitsPerSample / 8;
            bool floating = format.Encoding == WaveFormatEncoding.IeeeFloat ||
                format is WaveFormatExtensible extensible && extensible.SubFormat == new Guid("00000003-0000-0010-8000-00aa00389b71");
            for (int frame = 0; frame < data.Length; frame += format.BlockAlign)
            {
                if (!silent) for (int channel = 0; channel < format.Channels; channel++)
                {
                    var sample = data.Slice(frame + channel * bytes, bytes);
                    float value = floating && bytes == 4 ? BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(sample)) : bytes switch
                    {
                        1 => (sample[0] - 128) / 128f,
                        2 => BinaryPrimitives.ReadInt16LittleEndian(sample) / 32768f,
                        3 => ((sample[0] | sample[1] << 8 | sample[2] << 16) << 8 >> 8) / 8388608f,
                        4 => BinaryPrimitives.ReadInt32LittleEndian(sample) / 2147483648f,
                        _ => 0
                    };
                    if (float.IsFinite(value)) peak = Math.Max(peak, Math.Abs(value));
                }
                if (++inWindow < window) continue;
                peaks.Add(peak); peak = 0; inWindow = 0;
                if (peaks.Count < 32768) continue;
                for (int i = 0; i < peaks.Count / 2; i++) peaks[i] = Math.Max(peaks[i * 2], peaks[i * 2 + 1]);
                peaks.RemoveRange(peaks.Count / 2, peaks.Count / 2); window *= 2;
            }
        }
    }
    public float[] Snapshot() { lock (sync) return inWindow > 0 ? [.. peaks, peak] : [.. peaks]; }
}
