using System.IO;
using Cube.Desktop.Services;
using NAudio.Wave;

internal static class MultitrackCaptureTests
{
    public static async Task Run()
    {
        string folder = Path.Combine(Path.GetTempPath(), "cube-capture-boundaries-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            // Known signed PCM samples, independent of the capture decoder.
            foreach (var (bits, packet) in new[]
            {
                (16, new byte[] { 0, 128, 0, 64 }),
                (24, new byte[] { 0, 0, 128, 0, 0, 64 }),
                (32, new byte[] { 0, 0, 0, 128, 0, 0, 0, 64 })
            })
            {
                string path = Path.Combine(folder, bits + ".wav");
                var route = new BackingCaptureRoute(Guid.NewGuid(), 0, 2, path, false, 0, 0);
                using (var capture = new BackingMultitrackCapture(new WaveFormat(44100, bits, 2), [route]))
                {
                    capture.ProcessPacket(packet);
                    Check(capture.Snapshot(route.Id).Seconds == 1d / 44100, "PCM frame duration at " + bits + " bits");
                    Check(await capture.Stop() == null && await capture.Stop() == null, "stop is idempotent at " + bits + " bits");
                    try { capture.ProcessPacket(packet); throw new Exception("closed capture accepted a packet"); }
                    catch (ObjectDisposedException) { Console.WriteLine("PASS closed capture rejects packets"); }
                }
                using var reader = new WaveFileReader(path);
                byte[] samples = new byte[8]; reader.ReadExactly(samples);
                Check(reader.WaveFormat.SampleRate == 44100 && reader.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat && reader.Length == 8,
                    "PCM capture produces a finalized float WAV at " + bits + " bits");
                Check(BitConverter.ToSingle(samples, 0) == -1 && BitConverter.ToSingle(samples, 4) == .5f,
                    "signed PCM conversion preserves channel order at " + bits + " bits");
            }

            var monitorOnly = new BackingCaptureRoute(Guid.NewGuid(), 0, 1, null, true, 1, -1);
            using var monitored = new BackingMultitrackCapture(WaveFormat.CreateIeeeFloatWaveFormat(48000, 2), [monitorOnly]);
            float[] input = new float[9600];
            for (int i = 0; i < input.Length; i += 2) { input[i] = .25f; input[i + 1] = -.75f; }
            byte[] bytes = new byte[input.Length * 4]; Buffer.BlockCopy(input, 0, bytes, 0, bytes.Length);
            monitored.ProcessPacket(bytes);
            var first = monitored.ConsumeMeters(monitorOnly.Id);
            Check(first.Input.Peak == .25f && first.Monitor.Peak > 0, "monitor-only route measures the selected channel");
            // During capture only mix controls change; routing and file identity stay fixed.
            monitored.SetRoutes([monitorOnly with { FirstChannel = 1, Channels = 2, Destination = Path.Combine(folder, "unexpected.wav"), Muted = true }]);
            monitored.ProcessPacket(bytes);
            var muted = monitored.ConsumeMeters(monitorOnly.Id);
            Check(muted.Input.Peak == .25f && !File.Exists(Path.Combine(folder, "unexpected.wav")), "live mix edits cannot reroute input or create a writer");
            monitored.ProcessPacket(bytes);
            Check(monitored.ConsumeMeters(monitorOnly.Id).Monitor.Peak == 0, "muted monitoring settles to silence while input continues");
            Check(Math.Abs(monitored.Snapshot(monitorOnly.Id).Seconds - .3) < .000001, "muting monitoring does not stop the capture clock");
            var failure = new IOException("device disconnected");
            Check(ReferenceEquals(await monitored.Stop(failure), failure) && ReferenceEquals(await monitored.Stop(), failure), "repeated stop preserves the original failure");

            foreach (var invalid in new[] { monitorOnly with { FirstChannel = -1 }, monitorOnly with { FirstChannel = 2 }, monitorOnly with { Channels = 0 }, monitorOnly with { Channels = 3 }, monitorOnly with { FirstChannel = 1, Channels = 2 } })
            {
                try { using var capture = new BackingMultitrackCapture(new WaveFormat(48000, 16, 2), [invalid]); throw new Exception("invalid channel route accepted"); }
                catch (InvalidDataException) { Console.WriteLine("PASS invalid capture route rejected"); }
            }
        }
        finally { Directory.Delete(folder, true); }
    }

    private static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
        Console.WriteLine("PASS " + message);
    }
}
