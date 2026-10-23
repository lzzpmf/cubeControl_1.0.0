using System.Collections.Concurrent;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Cube.Desktop.Services;

public enum BackingPlaybackState { Empty, Loading, Ready, Playing, Paused, Ended, Error }
public sealed record BackingPlaybackSnapshot(BackingPlaybackState State, double Position, double Duration, string? OutputId, int Generation, long Repetitions = 0, Exception? Error = null, string? Source = null);

public sealed class BackingAudioEngine : IDisposable
{
    private readonly AudioPeakAccumulator outputPeak = new(), trackPeak = new(), clickPeak = new();
    public float ConsumeOutputPeak() => outputPeak.Consume();
    internal float ConsumeTrackPeak() => trackPeak.Consume();
    internal float ConsumeClickPeak() => clickPeak.Consume();
    private readonly BlockingCollection<Action> commands = new();
    private readonly Task worker;
    private volatile BackingPlaybackSnapshot snapshot = new(BackingPlaybackState.Empty, 0, 0, null, 0);
    private volatile bool disposed;
    private MMDeviceEnumerator? enumerator;
    private MMDevice? device;
    private WasapiPlayer? output;
    private BackingSessionAudio? processing;
    private BackingStemSource[] stems = [];
    private bool recordingClock;
    private double? recordingStopPosition;
    private ISampleProvider? provider, monitorProvider;
    private MetronomeSampleProvider? independentClick;
    private float[] monitorBlock = [], clickBlock = [];
    private bool transportRunning;
    private bool HasAuxiliary => monitorProvider != null || independentClick != null;
    public Task SetMonitor(string endpoint, ISampleProvider? value) => Invoke(() =>
    {
        monitorProvider = value;
        EnsureAuxiliary(endpoint);
    });
    public Task SetIndependentClick(string endpoint, MetronomeSampleProvider? value) => Invoke(() =>
    {
        independentClick = value;
        EnsureAuxiliary(endpoint);
    });
    private void EnsureAuxiliary(string endpoint)
    {
        if (processing != null) processing.KeepAlive = recordingClock || HasAuxiliary;
        if (HasAuxiliary && output == null) { outputId = endpoint; path ??= ""; Rebuild(snapshot.Position, false); }
        if (HasAuxiliary) { eof = false; Fill(); output?.Play(); }
        else if (!transportRunning) output?.Pause();
    }
    private AudioQueue? queue;
    private string? path, outputId;
    private double speed = 1, pitch, start, loopStart, loopEnd, volume = .3;
    private bool looping, muted, eof;
    private float gain, masterGain = 1;
    private double masterVolume = 1;
    private readonly BackingSignalMeter masterMeter = new(), clickSignal = new();
    internal (float Peak, float Rms) ConsumeClickSignal() => clickSignal.Consume();
    private volatile Dictionary<Guid, BackingSignalMeter> stemMeters = new();
    internal Dictionary<Guid, BackingSignalMeter> StemMeters => stemMeters;
    internal (float Peak, float Rms) ConsumeMasterMeter() => masterMeter.Consume();
    public Task SetMasterVolume(double value) => Invoke(() => masterVolume = Math.Clamp(value, 0, BackingMixMath.MaximumGain));
    private float[] block = [];
    private byte[] bytes = [];
    private int generation;
    private MetronomeSettings metronome = new();
    private long renderedFrames, countInFrames, countInRemaining;
    public bool IsCountingIn { get; private set; }
    public int MetronomeBeat { get; private set; }
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
    public Task Open(string file, string endpoint, double position, double rate, double semitones, bool loop, double a, double b, double level, bool mute, BackingStemSource[]? session = null) => Invoke(() =>
    {
        stems = session ?? [new(Guid.NewGuid(), file)];
        path = file; outputId = endpoint; speed = rate; pitch = semitones; looping = loop; loopStart = a; loopEnd = b; volume = level; muted = mute;
        generation++; Rebuild(position, false);
    });
    public Task ConfigureMetronome(MetronomeSettings settings) => Invoke(() =>
    {
        MetronomeClick.Prepare();
        if (!settings.Valid) throw new ArgumentException("Invalid metronome settings.");
        if (metronome == settings) return;
        var previous = metronome;
        metronome = settings;
        if (!recordingClock && output != null && (previous.Enabled != settings.Enabled || previous.Bpm != settings.Bpm
            || previous.Beats != settings.Beats || previous.EighthNotes != settings.EighthNotes || previous.Offset != settings.Offset))
        { UpdateClock(); Rebuild(snapshot.Position, snapshot.State == BackingPlaybackState.Playing); }
    });
    public Task Play() => Play(false);
    public Task Play(bool countIn) => Invoke(() =>
    {
        if (output == null) return;
        bool ended = snapshot.State == BackingPlaybackState.Ended;
        double position = ended ? looping ? loopStart : 0 : snapshot.Position;
        bool prelude = countIn && metronome.Enabled && metronome.CountInBars > 0;
        if (HasAuxiliary || ended || prelude) { Rebuild(position, true, prelude); return; }
        transportRunning = true; Fill(); output.Play(); Publish(BackingPlaybackState.Playing, snapshot.Position);
    });
    public Task Pause() => Invoke(() => { if (output == null) return; UpdateClock(); transportRunning = false; if (HasAuxiliary) Rebuild(snapshot.Position, false); else output.Pause(); Publish(BackingPlaybackState.Paused, snapshot.Position); });
    public Task Seek(double position) => Invoke(() => Rebuild(position, snapshot.State == BackingPlaybackState.Playing));
    public Task Configure(double rate, double semitones, bool loop, double a, double b) => Invoke(() =>
    {
        if (speed == rate && pitch == semitones && looping == loop && (!loop || loopStart == a && loopEnd == b)) return;
        UpdateClock(); double position = snapshot.Position; bool resume = snapshot.State == BackingPlaybackState.Playing;
        speed = rate; pitch = semitones; looping = loop; loopStart = a; loopEnd = b;
        if (path != null && output != null) Rebuild(position, resume);
    });
    public Task SetStemMix(BackingStemSource[] sources) => Invoke(() => { stems = sources; processing?.SetMix(sources); });
    public Task SetRecordingClock(bool enabled, double? stopPosition = null) => Invoke(() =>
    {
        recordingClock = enabled;
        recordingStopPosition = enabled ? stopPosition : null;
        if (processing != null) processing.KeepAlive = enabled;
    });
    public Task RefreshSession(BackingStemSource[] sources) => Invoke(() =>
    {
        UpdateClock(); stems = sources;
        if (path != null && output != null) Rebuild(snapshot.Position, snapshot.State == BackingPlaybackState.Playing);
    });
    public async Task<(double Position, long Timestamp)> CaptureClock()
    {
        (double Position, long Timestamp) result = default;
        await Invoke(() => { UpdateClock(); result = (snapshot.Position, System.Diagnostics.Stopwatch.GetTimestamp()); });
        return result;
    }
    internal async Task<BackingCaptureWindow> RecordingWindow(double sourceStart, double? sourceEnd, double latencyMs)
    {
        BackingCaptureWindow result = new(double.PositiveInfinity, double.PositiveInfinity);
        await Invoke(() =>
        {
            double now = System.Diagnostics.Stopwatch.GetTimestamp() / (double)System.Diagnostics.Stopwatch.Frequency;
            double elapsed = output == null ? 0 : output.GetPosition() / (double)output.OutputWaveFormat.AverageBytesPerSecond;
            double first = now - elapsed + countInFrames / (double)(processing?.WaveFormat.SampleRate ?? 48000) - latencyMs / 1000;
            result = new(first, sourceEnd is double end ? first + (end - sourceStart) / speed : double.PositiveInfinity);
        });
        return result;
    }
    public Task SetVolume(double level, bool mute) => Invoke(() => { volume = Math.Clamp(level, 0, BackingMixMath.MaximumGain); muted = mute; });
    public Task Stop() => Invoke(() => { if (path != null && output != null) Rebuild(looping ? loopStart : 0, false); });
    public Task ReleaseOutput() => Invoke(() => { UpdateClock(); CloseAudio(); Publish(BackingPlaybackState.Paused, snapshot.Position); });
    internal Task ChangeOutput(string endpoint) => Invoke(() =>
    {
        if (outputId == endpoint && output != null) return;
        outputId = endpoint;
        // Read the clock on the audio worker before disposing the old endpoint.
        // Preserve transport intent here so consecutive changes remain serialized.
        if (path == null || output == null) return;
        UpdateClock();
        var state = snapshot.State;
        Rebuild(snapshot.Position, state == BackingPlaybackState.Playing);
        if (state == BackingPlaybackState.Paused) Publish(state, snapshot.Position);
    });
    public Task Clear() => Invoke(() => { generation++; CloseAudio(); path = null; snapshot = new(BackingPlaybackState.Empty, 0, 0, null, generation); Changed?.Invoke(snapshot); });
    private void Rebuild(double position, bool resume, bool countIn = false)
    {
        if (path == null || outputId == null) return;
        CloseAudio(); transportRunning = resume; Publish(BackingPlaybackState.Loading, position);
        enumerator ??= new MMDeviceEnumerator(); device = enumerator.GetDevice(outputId);
        if (device.State != DeviceState.Active) throw new InvalidOperationException("The selected output is unavailable.");
        if (!recordingClock && !HasAuxiliary && stems.Length > 0 && !stems.Any(s => System.IO.File.Exists(s.Path))) throw new System.IO.FileNotFoundException("No session audio is available.");
        processing = new(stems, position, looping ? loopEnd : double.PositiveInfinity, speed, pitch, looping, loopStart) { KeepAlive = recordingClock || HasAuxiliary };
        provider = processing; stemMeters = processing.GetMeters();
        start = position; eof = false; gain = 0;
        renderedFrames = 0;
        countInFrames = countIn ? (long)Math.Round(metronome.CountInBars * metronome.Beats * metronome.Interval / speed * processing.WaveFormat.SampleRate) : 0;
        countInRemaining = countInFrames; IsCountingIn = countInFrames > 0;
        snapshot = snapshot with { Repetitions = 0 };
        int samples = Math.Max(processing.WaveFormat.Channels, processing.WaveFormat.SampleRate / 100 * processing.WaveFormat.Channels);
        block = new float[samples]; bytes = new byte[samples * sizeof(float)];
        queue = new(processing.WaveFormat, Math.Max(bytes.Length * 2, processing.WaveFormat.AverageBytesPerSecond / 4));
        output = new WasapiPlayerBuilder().WithDevice(device).WithSharedMode().WithEventSync().WithLatency(100).Build();
        var current = output;
        output.PlaybackStopped += (_, args) =>
        {
            if (disposed) return;
            try { commands.Add(() => { if (output != current) return; if (args.Exception != null) Fail(args.Exception); else if (eof) Publish(BackingPlaybackState.Ended, recordingStopPosition ?? processing!.Duration); }); }
            catch (InvalidOperationException) { }
        };
        output.Init(queue); Fill();
        Publish(BackingPlaybackState.Ready, position);
        if (resume) { output.Play(); Publish(BackingPlaybackState.Playing, position); }
        else if (HasAuxiliary) output.Play();
    }
    private void Fill()
    {
        if (queue == null || provider == null || eof) return;
        while (queue.FreeBytes >= bytes.Length)
        {
            bool prelude = countInRemaining > 0;
            int count;
            if (prelude) { count = (int)Math.Min(block.Length, countInRemaining * provider.WaveFormat.Channels); Array.Clear(block); }
            else if (HasAuxiliary && !transportRunning) { count = block.Length; Array.Clear(block); }
            else
            {
                int wanted = block.Length;
                if (recordingClock && recordingStopPosition is double stop)
                {
                    long remaining = (long)Math.Round((stop - start) / speed * provider.WaveFormat.SampleRate) - (renderedFrames - countInFrames);
                    wanted = (int)Math.Min(wanted, Math.Max(0, remaining) * provider.WaveFormat.Channels);
                }
                count = wanted == 0 ? 0 : provider.Read(block.AsSpan(0, wanted));
            }
            if (count == 0) { eof = true; queue.Complete(); break; }
            if (monitorBlock.Length < count) { monitorBlock = new float[count]; clickBlock = new float[count]; }
            Array.Clear(monitorBlock, 0, count); Array.Clear(clickBlock, 0, count);
            monitorProvider?.Read(monitorBlock.AsSpan(0, count)); independentClick?.Read(clickBlock, 0, count);
            float target = muted ? 0 : (float)volume;
            float step = 1f / Math.Max(1, provider.WaveFormat.SampleRate * .005f);
            int channels = provider.WaveFormat.Channels;
            float trackMaximum = 0, clickMaximum = 0;
            for (int i = 0; i < count; i += channels)
            {
                gain += Math.Clamp(target - gain, -step, step);
                masterGain += Math.Clamp((float)masterVolume - masterGain, -step, step);
                double sourceTime = prelude ? metronome.Offset + renderedFrames * speed / provider.WaveFormat.SampleRate
                    : MetronomeClick.SourcePosition(renderedFrames - countInFrames, provider.WaveFormat.SampleRate, start, speed, looping, loopStart, loopEnd);
                float click = ((prelude || !metronome.CountInOnly) && (transportRunning || !HasAuxiliary) ? MetronomeClick.Sample(metronome, sourceTime, speed) : 0) + clickBlock[i];
                clickMaximum = Math.Max(clickMaximum, Math.Abs(click));
                clickSignal.Add(click, click);
                for (int c = 0; c < channels; c++)
                {
                    float trackSample = block[i + c] * gain;
                    trackMaximum = Math.Max(trackMaximum, Math.Abs(trackSample));
                    float mixed = trackSample + click + monitorBlock[i + c];
                    block[i + c] = mixed * masterGain;
                }
                masterMeter.Add(block[i], block[i + 1]);
                renderedFrames++;
            }
            if (prelude) countInRemaining -= count / channels;
            float peak = 0;
            for (int i = 0; i < count; i++) peak = Math.Max(peak, Math.Abs(block[i]));

            outputPeak.Add(peak); trackPeak.Add(trackMaximum); clickPeak.Add(clickMaximum);
            Buffer.BlockCopy(block, 0, bytes, 0, count * sizeof(float));
            queue.Write(bytes, count * sizeof(float));
        }
    }
    private void UpdateClock()
    {
        if (HasAuxiliary && !transportRunning) return;
        if (output == null || processing == null || snapshot.State is not (BackingPlaybackState.Playing or BackingPlaybackState.Paused)) return;
        double elapsed = output.GetPosition() / (double)output.OutputWaveFormat.AverageBytesPerSecond;
        double preludeDuration = countInFrames / (double)processing.WaveFormat.SampleRate;
        IsCountingIn = elapsed < preludeDuration;
        MetronomeBeat = metronome.Enabled ? metronome.BeatAt(IsCountingIn ? metronome.Offset + elapsed * speed : snapshot.Position) : 0;
        elapsed = Math.Max(0, elapsed - preludeDuration);
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
        snapshot = snapshot with { Position = recordingClock ? sourcePosition : Math.Min(processing.Duration, sourcePosition), Repetitions = repetitions };
        if (!IsCountingIn) MetronomeBeat = metronome.Enabled ? metronome.BeatAt(snapshot.Position) : 0;
        if (repetitions > previousRepetitions) LoopCompleted?.Invoke(generation, repetitions);
        if (HasAuxiliary && transportRunning && !recordingClock && !looping && sourcePosition >= processing.Duration)
        { transportRunning = false; Publish(BackingPlaybackState.Ended, processing.Duration); }
    }
    private void Work()
    {
        try
        {
            while (!commands.IsCompleted)
            {
                if (commands.TryTake(out var command, snapshot.State == BackingPlaybackState.Playing || HasAuxiliary ? 4 : 100)) command();
                if (snapshot.State == BackingPlaybackState.Playing || HasAuxiliary)
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
        outputPeak.Consume(); trackPeak.Consume(); clickPeak.Consume();
        IsCountingIn = false; MetronomeBeat = 0;
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
