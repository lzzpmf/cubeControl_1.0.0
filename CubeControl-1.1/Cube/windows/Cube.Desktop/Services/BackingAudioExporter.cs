using System.IO;
using NAudio.Wave;

namespace Cube.Desktop.Services;

internal static class BackingAudioExporter
{
    public static Task Export(string source, string destination, double start, double end, double speed, double pitch, bool processed, CancellationToken cancellation) => Task.Run(() =>
    {
        string temporary = Path.Combine(Path.GetDirectoryName(destination)!, "." + Path.GetFileName(destination) + "." + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            using (var audio = new BackingAudioProcessing(source, start, end, processed ? speed : 1, processed ? pitch : 0, offline: true))
            {
                ISampleProvider provider = processed ? new BackingPeakGuard(audio) : audio;
                using var writer = new WaveFileWriter(temporary, new WaveFormat(audio.WaveFormat.SampleRate, 16, audio.WaveFormat.Channels));
                var samples = new float[4096 * audio.WaveFormat.Channels]; var pcm = new byte[samples.Length * 2];
                var random = new Random(0x43554245);
                int read;
                while ((read = provider.Read(samples)) > 0)
                {
                    cancellation.ThrowIfCancellationRequested();
                    for (int i = 0; i < read; i++)
                    {
                        double dither = random.NextDouble() - random.NextDouble();
                        short value = (short)Math.Clamp(Math.Round(samples[i] * 32768 + dither), short.MinValue, short.MaxValue);
                        pcm[2 * i] = (byte)value; pcm[2 * i + 1] = (byte)(value >> 8);
                    }
                    writer.Write(pcm, 0, read * 2);
                }
            }
            cancellation.ThrowIfCancellationRequested();
            File.Move(temporary, destination, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }, cancellation);
}
