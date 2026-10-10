using System.IO;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Cube.Desktop.Services;

// Each session owns its endpoint and file. Loopback records the selected output's mix.
public sealed class BackingRecording : IDisposable
{
    private readonly MMDevice device;
    private readonly WasapiRecorder capture;
    private readonly BackingRecordingWriter writer;
    private readonly object sync = new();
    private readonly TaskCompletionSource<Exception?> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private Exception? failure;
    private bool closed, disposed;
    public string TemporaryPath { get; }
    public Task<Exception?> Completion => completion.Task;
    public double Seconds => writer.Seconds;

    public BackingRecording(string endpoint, string destination, bool loopback = false)
    {
        TemporaryPath = destination + "." + Guid.NewGuid().ToString("N") + ".recording.wav";
        using var enumerator = new MMDeviceEnumerator();
        device = enumerator.GetDevice(endpoint);
        try
        {
            var builder = new WasapiRecorderBuilder().WithDevice(device);
            if (loopback) builder.WithLoopbackCapture();
            capture = builder.Build();
            try { writer = new BackingRecordingWriter(TemporaryPath, capture.WaveFormat); }
            catch { capture.Dispose(); throw; }
        }
        catch { device.Dispose(); throw; }
        capture.DataAvailable += (data, flags, _, _) =>
        {
            try
            {
                lock (sync)
                {
                    if (closed || failure != null) return;
                    writer.Write(data, (flags & AudioClientBufferFlags.Silent) != 0);
                }
            }
            catch (Exception ex)
            {
                failure = ex;
                try { capture.StopRecording(); } catch (Exception stopError) { Finish(stopError); }
            }
        };
        capture.RecordingStopped += (_, e) => Finish(e.Exception);
        try { capture.StartRecording(); }
        catch { Dispose(); throw; }
    }

    private void Finish(Exception? error)
    {
        lock (sync)
        {
            if (closed) return;
            closed = true;
            try { writer.Dispose(); } catch (Exception ex) { failure ??= ex; }
            completion.TrySetResult(failure ?? error);
        }
    }

    public Task<Exception?> Stop(Exception? error = null)
    {
        if (error != null) failure ??= error;
        if (!completion.Task.IsCompleted)
            try { capture.StopRecording(); } catch (Exception ex) { Finish(ex); }
        return completion.Task;
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        try { capture.Dispose(); } catch (Exception ex) { failure ??= ex; }
        finally
        {
            Finish(null);
            try { device.Dispose(); } catch { /* The WAV is already finalized. */ }
        }
    }
}

internal sealed class BackingRecordingWriter(string path, WaveFormat format) : IDisposable
{
    private readonly WaveFileWriter writer = new(path, format);
    private long bytes;
    public double Seconds => Interlocked.Read(ref bytes) / (double)format.AverageBytesPerSecond;
    public void Write(ReadOnlySpan<byte> data, bool silent)
    {
        if (bytes + data.Length > uint.MaxValue - 4096L) throw new IOException("WAV size limit reached.");
        if (data.Length % format.BlockAlign != 0) throw new InvalidDataException("Incomplete audio frame.");
        if (silent)
        {
            Span<byte> silence = stackalloc byte[4096]; silence.Clear();
            for (int remaining = data.Length; remaining > 0; remaining -= Math.Min(remaining, silence.Length))
                writer.Write(silence[..Math.Min(remaining, silence.Length)]);
        }
        else writer.Write(data);
        Interlocked.Add(ref bytes, data.Length);
    }
    public void Dispose() => writer.Dispose();
}
