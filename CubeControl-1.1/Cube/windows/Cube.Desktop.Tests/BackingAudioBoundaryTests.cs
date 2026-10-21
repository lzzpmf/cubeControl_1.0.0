using Cube.Desktop.Services;

internal static class BackingAudioBoundaryTests
{
    public static Task Run()
    {
        foreach (double db in new[] { -60d, -24, -6, 0, 3, 6 })
            Near(BackingMixMath.ToDb(BackingMixMath.FromDb(db)), db, "gain round trip at " + db + " dB");
        Check(BackingMixMath.FromDb(-100) == 0 && BackingMixMath.ToDb(0) == -61, "silence has its own fader position");
        Near(BackingMixMath.FromDb(20), BackingMixMath.MaximumGain, "gain is limited to +6 dB");
        Near(BackingMixMath.ToDb(10), 6, "overload display is limited to +6 dB");
        foreach (double pan in new[] { -1d, -.5, 0, .5, 1 })
        {
            var mono = BackingMixMath.Pan(pan, true);
            Near(mono.Left * mono.Left + mono.Right * mono.Right, 1, "mono pan preserves power at " + pan);
        }
        var stereo = BackingMixMath.Pan(0, false);
        Check(stereo == (1f, 1f), "stereo center preserves both channels");
        var legacy = BackingMixMath.Pan(0, true, true);
        Near(legacy.Left, 1, "legacy mono center preserves left level");
        Near(legacy.Right, 1, "legacy mono center preserves right level");
        Check(BackingMixMath.Pan(-2, false) == (1f, 0f) && BackingMixMath.Pan(2, false) == (0f, 1f), "pan clamps outside the endpoints");

        foreach (int rate in new[] { 44100, 48000, 96000 })
        {
            var monitor = new BackingMonitorBuffer(rate);
            Check(monitor.WaveFormat.SampleRate == 48000 && monitor.WaveFormat.Channels == 2, "monitor output format at " + rate);
            float[] destination = Enumerable.Repeat(9f, 100).ToArray();
            Check(monitor.Read(destination, 2, 96) == 96 && destination[0] == 9 && destination[1] == 9 && destination[^2] == 9 && destination[^1] == 9,
                "empty read respects destination offset and count at " + rate);
            Check(destination.AsSpan(2, 96).ToArray().All(x => x == 0), "underrun returns silence at " + rate);
            // Overflow must discard old audio, preserving the newest stereo frames.
            monitor.Write(Enumerable.Repeat(.9f, rate).ToArray());
            float[] recent = new float[rate];
            for (int i = 0; i < recent.Length; i += 2) { recent[i] = .25f; recent[i + 1] = -.5f; }
            monitor.Write(recent);
            Check(monitor.BufferedFrames <= rate / 2, "overflow keeps memory bounded at " + rate);
            int bufferedBefore = monitor.BufferedFrames;
            var output = new float[960]; monitor.Read(output);
            Check(Math.Abs(bufferedBefore - monitor.BufferedFrames - 480d * rate / 48000) <= 2,
                "monitor consumes capture frames at the input sample rate " + rate);
            Check(output.Where((_, i) => i % 2 == 0).All(x => Math.Abs(x - .25f) < .0001) &&
                output.Where((_, i) => i % 2 == 1).All(x => Math.Abs(x + .5f) < .0001), "overflow preserves newest samples and stereo order at " + rate);
            // Drain past the available audio, then prove a new packet resumes playback.
            monitor.Read(new float[96000]);
            Array.Fill(output, 9f); monitor.Read(output);
            Check(output.All(x => x == 0), "drained monitor does not repeat stale audio at " + rate);
            monitor.Write(recent); monitor.Read(output);
            Check(output.Skip(4).Where((_, i) => i % 2 == 0).All(x => Math.Abs(x - .25f) < .0001), "monitor resumes after underrun at " + rate);
        }

        var meter = new BackingSignalMeter(1000);
        for (int i = 0; i < 300; i++) meter.Add(-1, 0);
        var reading = meter.Consume();
        Near(reading.Peak, 1, "peak includes negative samples");
        Near(reading.Rms, Math.Sqrt(.5), "RMS includes both channels");
        var repeated = meter.Consume();
        Check(repeated.Peak == 0 && repeated.Rms == reading.Rms, "consuming peak preserves RMS history");
        for (int i = 0; i < 300; i++) meter.Add(0, 0);
        Check(meter.Consume() == (0f, 0f), "a full silent window clears RMS history");
        return Task.CompletedTask;
    }

    private static void Near(double actual, double expected, string message) => Check(Math.Abs(actual - expected) < .00001, message);
    private static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
        Console.WriteLine("PASS " + message);
    }
}
