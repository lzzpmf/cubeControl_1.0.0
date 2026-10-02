using System.Collections.Concurrent;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Cube.Desktop.Services;

public enum BackingPlaybackState { Empty, Loading, Ready, Playing, Paused, Ended, Error }
public sealed record BackingPlaybackSnapshot(BackingPlaybackState State, double Position, double Duration, string? OutputId, int Generation, long Repetitions = 0, Exception? Error = null, string? Source = null);

public sealed class BackingAudioEngine : IDisposable
{
    private readonly BlockingCollection<Action> commands = new();
    private readonly Task worker;
    private volatile BackingPlaybackSnapshot snapshot = new(BackingPlaybackState.Empty, 0, 0, null, 0);
    private volatile bool disposed;
    private MMDeviceEnumerator? enumerator;
    private MMDevice? device;
    private WasapiPlayer? output;
    private BackingAudioProcessing? processing;
    private ISampleProvider? provider;
    private AudioQueue? queue;
    private string? path, outputId;
    private double speed = 1, pitch, start, loopStart, loopEnd, volume = .3;
    private bool looping, muted, eof;
    private float gain;
    private float[] block = [];
    private byte[] bytes = [];
    private int generation;
    public BackingPlaybackSnapshot Snapshot => snapshot;
    public event Action<BackingPlaybackSnapshot>? Changed;
    public event Action<int, long>? LoopCompleted;

    public BackingAudioEngine() => worker = Task.Factory.StartNew(Work, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    private Task Invoke(Action action)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        if (disposed) return Task.CompletedTask;
        try { commands.Add(() => { try { action(); completion.SetResult(); } catch (Exception ex) { Fail(ex); completion.SetException(ex); } }); }
        catch (InvalidOperationException) { completion.TrySetResult(); }
        return completion.Task;
    }
    private void Publish(BackingPlaybackState state, double position, Exception? error = null)
    {
        snapshot = new(state, position, processing?.Duration ?? snapshot.Duration, device?.ID, generation, snapshot.Repetitions, error, path);
        Changed?.Invoke(snapshot);
    }
    public Task Open(string file, string endpoint, double position, double rate, double semitones, bool loop, double a, double b, double level, bool mute) => Invoke(() =>
    {
        path = file; outputId = endpoint; speed = rate; pitch = semitones; looping = loop; loopStart = a; loopEnd = b; volume = level; muted = mute;
        generation++; Rebuild(position, false);
    });
    public Task Play() => Invoke(() =>
    {
        if (output == null) return;
        if (snapshot.State == BackingPlaybackState.Ended) Rebuild(looping ? loopStart : 0, false);
        Fill(); output!.Play(); Publish(BackingPlaybackState.Playing, snapshot.Position);
    });
    public Task Pause() => Invoke(() => { if (output == null) return; UpdateClock(); output.Pause(); Publish(BackingPlaybackState.Paused, snapshot.Position); });
    public Task Seek(double position) => Invoke(() => Rebuild(position, snapshot.State == BackingPlaybackState.Playing));
    public Task Configure(double rate, double semitones, bool loop, double a, double b) => Invoke(() =>
    {
        if (speed == rate && pitch == semitones && looping == loop && (!loop || loopStart == a && loopEnd == b)) return;
        UpdateClock(); double position = snapshot.Position; bool resume = snapshot.State == BackingPlaybackState.Playing;
        speed = rate; pitch = semitones; looping = loop; loopStart = a; loopEnd = b;
        if (path != null && output != null) Rebuild(position, resume);
    });
    public Task SetVolume(double level, bool mute) => Invoke(() => { volume = Math.Clamp(level, 0, 1); muted = mute; });
    public Task Stop() => Invoke(() => { if (path != null && output != null) Rebuild(looping ? loopStart : 0, false); });
    public Task ReleaseOutput() => Invoke(() => { UpdateClock(); CloseAudio(); Publish(BackingPlaybackState.Paused, snapshot.Position); });
    public Task Clear() => Invoke(() => { generation++; CloseAudio(); path = null; snapshot = new(BackingPlaybackState.Empty, 0, 0, null, generation); Changed?.Invoke(snapshot); });
    private void Rebuild(double position, bool resume)
    {
        if (path == null || outputId == null) return;
        CloseAudio(); Publish(BackingPlaybackState.Loading, position);
        enumerator ??= new MMDeviceEnumerator(); device = enumerator.GetDevice(outputId);
        if (device.State != DeviceState.Active) throw new InvalidOperationException("The selected output is unavailable.");
        using (var decoder = new BackingDecoder(path))
        {
            double duration = decoder.Duration;
            position = looping ? Math.Clamp(position, loopStart, Math.Max(loopStart, loopEnd - 1d / decoder.Format.SampleRate)) : Math.Clamp(position, 0, duration);
            processing = new(path, position, looping ? loopEnd : duration, speed, pitch, looping, loopStart);
        }
        provider = speed != 1 || pitch != 0 ? new BackingPeakGuard(processing) : processing;
        start = position; eof = false; gain = 0;
        snapshot = snapshot with { Repetitions = 0 };
        int samples = Math.Max(processing.WaveFormat.Channels, processing.WaveFormat.SampleRate / 100 * processing.WaveFormat.Channels);
        block = new float[samples]; bytes = new byte[samples * sizeof(float)];
        queue = new(processing.WaveFormat, Math.Max(bytes.Length * 2, processing.WaveFormat.AverageBytesPerSecond / 4));
        output = new WasapiPlayerBuilder().WithDevice(device).WithSharedMode().WithEventSync().WithLatency(100).Build();
        var current = output;
        output.PlaybackStopped += (_, args) =>
        {
            if (disposed) return;
            try { commands.Add(() => { if (output != current) return; if (args.Exception != null) Fail(args.Exception); else if (eof) Publish(BackingPlaybackState.Ended, processing!.Duration); }); }
            catch (InvalidOperationException) { }
        };
        output.Init(queue); Fill();
        Publish(BackingPlaybackState.Ready, position);
        if (resume) { output.Play(); Publish(BackingPlaybackState.Playing, position); }
    }
    private void Fill()
    {
        if (queue == null || provider == null || eof) return;
        while (queue.FreeBytes >= bytes.Length)
        {
            int count = provider.Read(block);
            if (count == 0) { eof = true; queue.Complete(); break; }
            float target = muted ? 0 : (float)volume;
            float step = 1f / Math.Max(1, provider.WaveFormat.SampleRate * .005f);
            int channels = provider.WaveFormat.Channels;
            for (int i = 0; i < count; i += channels)
            {
                gain += Math.Clamp(target - gain, -step, step);
                for (int c = 0; c < channels; c++) block[i + c] *= gain;
            }
            Buffer.BlockCopy(block, 0, bytes, 0, count * sizeof(float));
            queue.Write(bytes, count * sizeof(float));
        }
    }
    private void UpdateClock()
    {
        if (output == null || processing == null || snapshot.State is not (BackingPlaybackState.Playing or BackingPlaybackState.Paused)) return;
        double elapsed = output.GetPosition() / (double)output.OutputWaveFormat.AverageBytesPerSecond;
        double sourcePosition = start + elapsed * speed;
        long repetitions = 0;
        if (looping && sourcePosition >= loopEnd)
        {
            double rate = processing.WaveFormat.SampleRate;
            double firstDuration = Math.Round((loopEnd - start) * rate / speed) / rate;
            double cycleDuration = Math.Max(1 / rate, Math.Round((loopEnd - loopStart) * rate / speed) / rate);
            if (elapsed >= firstDuration)
            {
                repetitions = 1 + (long)((elapsed - firstDuration) / cycleDuration);
                sourcePosition = loopStart + (elapsed - firstDuration) % cycleDuration / cycleDuration * (loopEnd - loopStart);
            }
        }
        long previousRepetitions = snapshot.Repetitions;
        snapshot = snapshot with { Position = Math.Min(processing.Duration, sourcePosition), Repetitions = repetitions };
        if (repetitions > previousRepetitions) LoopCompleted?.Invoke(generation, repetitions);
    }
    private void Work()
    {
        try
        {
            while (!commands.IsCompleted)
            {
                if (commands.TryTake(out var command, snapshot.State == BackingPlaybackState.Playing ? 4 : 100)) command();
                if (snapshot.State == BackingPlaybackState.Playing)
                {
                    try
                    {
                        Fill(); UpdateClock();
                        if (queue?.Underrun == true) throw new InvalidOperationException("Audio buffer underrun.");
                    }
                    catch (Exception ex) { Fail(ex); }
                }
            }
        }
        finally { CloseAudio(); enumerator?.Dispose(); }
    }
    private void Fail(Exception error) { double position = snapshot.Position; CloseAudio(); Publish(BackingPlaybackState.Error, position, error); }
    private void CloseAudio()
    {
        var previous = output; output = null;
        try { previous?.Stop(); } finally { previous?.Dispose(); processing?.Dispose(); processing = null; device?.Dispose(); device = null; queue = null; provider = null; }
    }
    public void Dispose()
    {
        if (disposed) return;
        disposed = true; commands.CompleteAdding(); worker.GetAwaiter().GetResult(); commands.Dispose();
    }
    private sealed class AudioQueue(WaveFormat format, int capacity) : IWaveProvider
    {
        private readonly byte[] ring = new byte[capacity];
        private readonly object sync = new();
        private int read, write, length;
        private bool complete;
        private volatile bool underrun;
        public bool Underrun => underrun;
        public WaveFormat WaveFormat => format;
        public int FreeBytes { get { lock (sync) return capacity - length; } }
        public void Complete() { lock (sync) complete = true; }
        public void Write(byte[] data, int count)
        {
            lock (sync)
            {
                if (count > capacity - length) throw new InvalidOperationException("Audio queue overflow.");
                int first = Math.Min(count, capacity - write);
                Buffer.BlockCopy(data, 0, ring, write, first); Buffer.BlockCopy(data, first, ring, 0, count - first);
                write = (write + count) % capacity; length += count;
            }
        }
        public int Read(byte[] buffer, int offset, int count)
            => Read(buffer.AsSpan(offset, count));
        public int Read(Span<byte> buffer)
        {
            lock (sync)
            {
                int count = buffer.Length;
                int take = Math.Min(count, length), first = Math.Min(take, capacity - read);
                ring.AsSpan(read, first).CopyTo(buffer); ring.AsSpan(0, take - first).CopyTo(buffer[first..]);
                read = (read + take) % capacity; length -= take;
                if (take < count && !complete) { underrun = true; buffer[take..].Clear(); return count; }
                return take;
            }
        }
    }
}
