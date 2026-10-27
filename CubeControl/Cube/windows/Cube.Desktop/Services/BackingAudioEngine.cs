using System.Collections.Concurrent;
using NAudio.CoreAudioApi;
using NAudio.Wave;
namespace Cube.Desktop.Services;
public enum BackingPlaybackState { Empty, Loading, Ready, Playing, Paused, Ended, Error }
public sealed record BackingPlaybackSnapshot(BackingPlaybackState State, double Position, double Duration, string? OutputId, int Generation, long Repetitions = 0, Exception? Error = null, string? Source = null);
// Raw IR audition playback. No session, recording or native DSP is initialized.
public sealed class BackingAudioEngine : IDisposable
{
    private readonly BlockingCollection<Action> commands = new();
    private readonly Task worker;
    private volatile BackingPlaybackSnapshot snapshot = new(BackingPlaybackState.Empty, 0, 0, null, 0);
    private volatile bool disposed;
    private IWavePlayer? output;
    private MMDeviceEnumerator? enumerator;
    private MMDevice? device;
    private RawAudio? audio;
    private string? path, outputId;
    private bool looping, muted;
    private double volume = 1;
    private int generation;
    public BackingPlaybackSnapshot Snapshot => snapshot;
    public BackingAudioEngine() => worker = Task.Factory.StartNew(Work, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    private Task Invoke(Action action)
    {
        if (disposed) return Task.CompletedTask;
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        try { commands.Add(() => { try { action(); completion.SetResult(); } catch (Exception ex) { CloseAudio(); snapshot = snapshot with { State = BackingPlaybackState.Error, Error = ex }; completion.SetException(ex); } }); }
        catch (InvalidOperationException) { completion.TrySetResult(); }
        return completion.Task;
    }
    public Task Open(string file, string endpoint, bool loop, double level) => Invoke(() =>
    {
        CloseAudio(); path = file; outputId = endpoint; looping = loop; volume = Math.Clamp(level, 0, 1); muted = false; generation++;
        Build();
    });
    private void Build()
    {
        enumerator ??= new MMDeviceEnumerator(); device = enumerator.GetDevice(outputId!);
        if (device.State != DeviceState.Active) throw new InvalidOperationException("The selected output is unavailable.");
        audio = new RawAudio(path!) { Looping = looping, Gain = muted ? 0 : (float)volume };
        output = new WasapiPlayerBuilder().WithDevice(device).WithSharedMode().WithEventSync().WithLatency(100).Build();
        var current = output;
        output.PlaybackStopped += (_, args) =>
        {
            if (disposed) return;
            _ = Invoke(() => { if (current != output) return; snapshot = snapshot with { State = args.Exception == null ? BackingPlaybackState.Ended : BackingPlaybackState.Error, Error = args.Exception, Position = audio?.Duration ?? 0 }; });
        };
        output.Init(audio.ToWaveProvider());
        snapshot = new(BackingPlaybackState.Ready, 0, audio.Duration, outputId, generation, Source: path);
    }
    public Task Play() => Invoke(() =>
    {
        if (snapshot.State == BackingPlaybackState.Ended) { CloseAudio(); Build(); }
        if (output == null) return;
        output.Play(); snapshot = snapshot with { State = BackingPlaybackState.Playing };
    });
    public Task Pause() => Invoke(() => { if (output == null) return; output.Pause(); UpdatePosition(); snapshot = snapshot with { State = BackingPlaybackState.Paused }; });
    public Task Clear() => Invoke(() => { CloseAudio(); path = outputId = null; snapshot = new(BackingPlaybackState.Empty, 0, 0, null, ++generation); });
    public Task Configure(bool loop) => Invoke(() => { looping = loop; if (audio != null) audio.Looping = loop; });
    public Task SetVolume(double level, bool mute) => Invoke(() => { volume = level; muted = mute; if (audio != null) audio.Gain = mute ? 0 : (float)Math.Clamp(level, 0, 1); });
    public float ConsumeOutputPeak() => audio?.Peak.Consume() ?? 0;
    private void UpdatePosition()
    {
        if (output is not IWavePosition position || audio == null) return;
        double seconds = position.GetPosition() / (double)audio.WaveFormat.AverageBytesPerSecond;
        snapshot = snapshot with { Position = looping && audio.Duration > 0 ? seconds % audio.Duration : Math.Min(seconds, audio.Duration) };
    }
    private void Work()
    {
        try
        {
            while (!commands.IsCompleted)
            {
                if (commands.TryTake(out var command, snapshot.State == BackingPlaybackState.Playing ? 40 : Timeout.Infinite)) command();
                if (snapshot.State == BackingPlaybackState.Playing)
                    try { UpdatePosition(); } catch (Exception ex) { CloseAudio(); snapshot = snapshot with { State = BackingPlaybackState.Error, Error = ex }; }
            }
        }
        finally { CloseAudio(); enumerator?.Dispose(); }
    }
    private void CloseAudio()
    {
        var previous = output; output = null;
        try { previous?.Stop(); } finally { previous?.Dispose(); audio?.Dispose(); audio = null; device?.Dispose(); device = null; }
    }
    public void Dispose()
    {
        if (disposed) return; disposed = true; commands.CompleteAdding(); worker.GetAwaiter().GetResult(); commands.Dispose();
    }
    private sealed class RawAudio : ISampleProvider, IDisposable
    {
        private readonly BackingDecoder decoder;
        public WaveFormat WaveFormat => decoder.Format;
        public double Duration => decoder.Duration;
        public volatile bool Looping;
        public volatile float Gain = 1;
        public readonly AudioPeakAccumulator Peak = new();
        public RawAudio(string file) => decoder = new(file);
        public int Read(Span<float> buffer)
        {
            int total = 0;
            while (total < buffer.Length)
            {
                var block = buffer[total..];
                int count = decoder.Read(block);
                if (count == 0)
                {
                    if (!Looping || Duration <= 0) break;
                    decoder.Seek(0); count = decoder.Read(block); if (count == 0) break;
                }
                float peak = 0, gain = Gain;
                for (int i = 0; i < count; i++) { float value = block[i] * gain; block[i] = value; peak = Math.Max(peak, Math.Abs(value)); }
                Peak.Add(peak); total += count;
            }
            return total;
        }
        public void Dispose() => decoder.Dispose();
    }
}
