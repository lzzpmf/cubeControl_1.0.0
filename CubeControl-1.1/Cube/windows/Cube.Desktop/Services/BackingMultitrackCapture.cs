using System.IO;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Cube.Desktop.Services;

internal sealed record BackingCaptureRoute(Guid Id, int FirstChannel, int Channels, string? Destination,
    bool Monitor, double Gain, double Pan, bool Muted = false);

// One WASAPI endpoint and one capture clock for all armed/monitored routes.
internal sealed class BackingMultitrackCapture : IDisposable
{
    private sealed class Sink(BackingCaptureRoute route, WaveFileWriter? writer, int sampleRate)
    {
        public BackingCaptureRoute Route = route;
        public WaveFileWriter? Writer = writer;
        public float[] Samples = [];
        public readonly List<float> Peaks = [];
        public long Frames;
        public float Peak;
        public int PeakFrames, PeakStride = 1;
        public readonly BackingSignalMeter InputMeter = new(sampleRate), MonitorMeter = new(sampleRate);
        public float Gain, Left = 1, Right = 1;
    }
    private readonly object sync = new();
    private readonly MMDevice? device;
    private readonly WaveFormat format;
    private readonly WasapiRecorder? capture;
    private readonly List<Sink> sinks = [];
    private BackingMonitorBuffer monitor = null!;
    private readonly TaskCompletionSource<Exception?> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool closed;
    private int disposed;
    private float[] input = [], mixed = [];
    private Exception? failure;
    public Task<Exception?> Completion => completion.Task;
    public double FirstCaptureSeconds { get; private set; } = double.NaN;
    private long capturedFrames;
    public double ClockRatio { get; private set; } = 1;
    public int SampleRate => format.SampleRate;
    public ISampleProvider MonitorProvider { get; private set; } = null!;
    public readonly AudioPeakAccumulator InputPeak = new();
    public BackingMultitrackCapture(string endpoint, BackingCaptureRoute[] routes)
    {
        using var enumerator = new MMDeviceEnumerator();
        device = enumerator.GetDevice(endpoint);
        try
        {
            capture = new WasapiRecorderBuilder().WithDevice(device).Build();
            format = capture.WaveFormat;
            Prepare(routes);
            capture.DataAvailable += (data, flags, position, qpc) =>
            {
                try
                {
                    lock (sync)
                    {
                        if (closed) return;
                        if (!double.IsNaN(FirstCaptureSeconds) && (flags & AudioClientBufferFlags.DataDiscontinuity) != 0)
                            throw new IOException("Capture discontinuity; recoverable takes retained.");
                        if (double.IsNaN(FirstCaptureSeconds)) FirstCaptureSeconds = qpc > 0 && (flags & AudioClientBufferFlags.TimestampError) == 0
                            ? qpc / 10000000d : System.Diagnostics.Stopwatch.GetTimestamp() / (double)System.Diagnostics.Stopwatch.Frequency - data.Length / (double)capture.WaveFormat.AverageBytesPerSecond;
                        if (capturedFrames >= SampleRate && qpc > 0 && (flags & AudioClientBufferFlags.TimestampError) == 0)
                        {
                            double ratio = (qpc / 10000000d - FirstCaptureSeconds) / (capturedFrames / (double)SampleRate);
                            if (ratio is >= .98 and <= 1.02) ClockRatio = ratio;
                        }
                        Process(data, (flags & AudioClientBufferFlags.Silent) != 0);
                    }
                }
                catch (Exception ex) { failure = ex; try { capture.StopRecording(); } catch { Finish(ex); } }
            };
            capture.RecordingStopped += (_, e) => Finish(e.Exception);
        }
        catch { foreach (var sink in sinks) sink.Writer?.Dispose(); capture?.Dispose(); device.Dispose(); throw; }
    }
    internal BackingMultitrackCapture(WaveFormat format, BackingCaptureRoute[] routes)
    {
        this.format = format;
        try { Prepare(routes); } catch { foreach (var sink in sinks) sink.Writer?.Dispose(); throw; }
    }
    private void Prepare(BackingCaptureRoute[] routes)
    {
            foreach (var route in routes)
            {
                if (route.FirstChannel < 0 || route.Channels is < 1 or > 2 || route.FirstChannel + route.Channels > format.Channels)
                    throw new InvalidDataException("The selected input channels are unavailable.");
                WaveFileWriter? writer = route.Destination == null ? null : new(route.Destination, WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, route.Channels));
                sinks.Add(new(route, writer, SampleRate));
            }
            monitor = new(SampleRate); MonitorProvider = monitor;
    }
    internal void ProcessPacket(ReadOnlySpan<byte> data, bool silent = false)
    {
        lock (sync) { if (closed) throw new ObjectDisposedException(nameof(BackingMultitrackCapture)); Process(data, silent); }
    }
    public void Start() => capture?.StartRecording();
    public void SetRoutes(BackingCaptureRoute[] routes)
    {
        lock (sync) foreach (var sink in sinks)
            if (routes.FirstOrDefault(r => r.Id == sink.Route.Id) is { } route)
                sink.Route = sink.Route with { Monitor = route.Monitor, Gain = route.Gain, Pan = route.Pan, Muted = route.Muted };
    }
    private void Process(ReadOnlySpan<byte> data, bool silent)
    {
        int frames = data.Length / format.BlockAlign, samples = frames * format.Channels;
        if (input.Length < samples) input = new float[samples];
        bool floating = format.Encoding == WaveFormatEncoding.IeeeFloat || format is WaveFormatExtensible ext && ext.SubFormat == new Guid("00000003-0000-0010-8000-00aa00389b71");
        int width = format.BitsPerSample / 8;
        for (int i = 0; i < samples; i++)
        {
            var bytes = data.Slice(i * width, width);
            input[i] = silent ? 0 : floating && width == 4 ? BitConverter.ToSingle(bytes) : width switch
            {
                2 => System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(bytes) / 32768f,
                3 => ((bytes[0] | bytes[1] << 8 | bytes[2] << 16) << 8 >> 8) / 8388608f,
                4 => System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(bytes) / 2147483648f,
                _ => throw new InvalidDataException("Unsupported capture format.")
            };
            InputPeak.Add(Math.Abs(input[i]));
        }
        if (mixed.Length < frames * 2) mixed = new float[frames * 2];
        Array.Clear(mixed, 0, frames * 2);
        foreach (var sink in sinks)
        {
            var route = sink.Route;
            int count = frames * route.Channels;
            if (sink.Samples.Length < count) sink.Samples = new float[count];
            var pan = BackingMixMath.Pan(route.Pan, route.Channels == 1);
            for (int f = 0; f < frames; f++)
            {
                float left = input[f * format.Channels + route.FirstChannel];
                float right = route.Channels == 2 ? input[f * format.Channels + route.FirstChannel + 1] : left;
                sink.Samples[f * route.Channels] = left;
                if (route.Channels == 2) sink.Samples[f * 2 + 1] = right;
                sink.Peak = Math.Max(sink.Peak, Math.Max(Math.Abs(left), Math.Abs(right)));
                if (++sink.PeakFrames >= Math.Max(1, SampleRate / 100) * sink.PeakStride)
                {
                    sink.Peaks.Add(sink.Peak); sink.Peak = 0; sink.PeakFrames = 0;
                    if (sink.Peaks.Count >= 32768) { for (int j = 0; j < sink.Peaks.Count / 2; j++) sink.Peaks[j] = Math.Max(sink.Peaks[j * 2], sink.Peaks[j * 2 + 1]); sink.Peaks.RemoveRange(sink.Peaks.Count / 2, sink.Peaks.Count / 2); sink.PeakStride *= 2; }
                }
                sink.InputMeter.Add(left, right);
                float desiredGain = route.Monitor && !route.Muted ? (float)route.Gain : 0;
                sink.Gain += Math.Clamp(desiredGain - sink.Gain, -1f / 240, 1f / 240);
                sink.Left += Math.Clamp(pan.Left - sink.Left, -1f / 240, 1f / 240);
                sink.Right += Math.Clamp(pan.Right - sink.Right, -1f / 240, 1f / 240);
                float monitorLeft = left * sink.Left * sink.Gain, monitorRight = right * sink.Right * sink.Gain;
                sink.MonitorMeter.Add(monitorLeft, monitorRight);
                if (route.Monitor && !route.Muted)
                {
                    mixed[f * 2] += monitorLeft;
                    mixed[f * 2 + 1] += monitorRight;
                }
            }
            sink.Writer?.WriteSamples(sink.Samples, 0, count);
            sink.Frames += frames;
        }
        capturedFrames += frames;
        monitor.Write(mixed.AsSpan(0, frames * 2));
    }
    public ((float Peak, float Rms) Input, (float Peak, float Rms) Monitor) ConsumeMeters(Guid id)
    {
        lock (sync) { var sink = sinks.FirstOrDefault(s => s.Route.Id == id); return sink == null ? default : (sink.InputMeter.Consume(), sink.MonitorMeter.Consume()); }
    }
    public (double Seconds, float[] Peaks) Snapshot(Guid id)
    {
        lock (sync) { var sink = sinks.First(s => s.Route.Id == id); return (sink.Frames / (double)SampleRate, sink.Peaks.ToArray()); }
    }
    public Task<Exception?> Stop(Exception? error = null)
    {
        failure ??= error;
        try { if (!completion.Task.IsCompleted) { if (capture != null) capture.StopRecording(); else Finish(error); } } catch (Exception ex) { Finish(ex); }
        return completion.Task;
    }
    private void Finish(Exception? error)
    {
        lock (sync)
        {
            if (closed) return; closed = true; failure ??= error;
            foreach (var sink in sinks) try { sink.Writer?.Dispose(); sink.Writer = null; } catch (Exception ex) { failure ??= ex; }
            completion.TrySetResult(failure);
        }
    }
    public void Dispose() { if (Interlocked.Exchange(ref disposed, 1) != 0) return; try { capture?.Dispose(); } finally { Finish(null); device?.Dispose(); } }
}
