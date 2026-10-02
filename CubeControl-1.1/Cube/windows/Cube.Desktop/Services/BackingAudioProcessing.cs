using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using NAudio.Wave;

namespace Cube.Desktop.Services;

internal sealed class BackingDspHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private BackingDspHandle() : base(true) { }
    protected override bool ReleaseHandle() { Native.Destroy(handle); return true; }
    internal static class Native
    {
        private const string Library = "CubeBackingDsp";
        [DllImport(Library, EntryPoint = "backing_create", CallingConvention = CallingConvention.Cdecl)]
        internal static extern BackingDspHandle Create(int channels, int sampleRate);
        [DllImport(Library, EntryPoint = "backing_destroy", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Destroy(IntPtr handle);
        [DllImport(Library, EntryPoint = "backing_prepare", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Prepare(BackingDspHandle handle, double speed, double semitones);
        [DllImport(Library, EntryPoint = "backing_seek", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Seek(BackingDspHandle handle, float[] input, int frames);
        [DllImport(Library, EntryPoint = "backing_process", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Process(BackingDspHandle handle, float[] input, int inputFrames, [Out] float[] output, int outputFrames);
    }
}

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
    public void Dispose() => reader.Dispose();
}

internal sealed class BackingAudioProcessing : ISampleProvider, IDisposable
{
    private readonly BackingDecoder decoder;
    private BackingDspHandle? dsp;
    private readonly double speed;
    private readonly double pitch;
    private readonly bool looping;
    private readonly long loopStart, end;
    private long inputFrame, outputFrame, outputLength;
    private double inputRemainder;
    private float[] input = [], output = [];
    private int buffered, offset;
    private readonly int channels, rate;
    public WaveFormat WaveFormat => decoder.Format;
    public double Duration => decoder.Duration;
    public BackingAudioProcessing(string path, double start, double end, double speed, double pitch, bool looping = false, double loopStart = 0)
    {
        decoder = new(path);
        try
        {
            channels = WaveFormat.Channels; rate = WaveFormat.SampleRate;
            this.speed = Math.Clamp(speed, .5, 1.5); this.pitch = Math.Clamp(pitch, -6, 6);
            this.looping = looping;
            this.end = (long)Math.Round(Math.Min(end, Duration) * rate);
            this.loopStart = (long)Math.Round(Math.Max(0, loopStart) * rate);
            if (this.end <= 0 || (looping && this.end - this.loopStart < rate / 10)) throw new InvalidDataException("Invalid audio range.");
            if (this.speed != 1 || this.pitch != 0)
            {
                dsp = BackingDspHandle.Native.Create(channels, rate);
                if (dsp.IsInvalid) throw new InvalidOperationException("DSP initialization failed.");
            }
            Begin((long)Math.Round(Math.Clamp(start, 0, this.end / (double)rate) * rate));
        }
        catch { Dispose(); throw; }
    }
    private void Begin(long frame)
    {
        decoder.Seek(frame); inputFrame = frame; outputFrame = 0;
        outputLength = (long)Math.Round((end - frame) / speed);
        inputRemainder = 0; buffered = offset = 0;
        if (dsp != null)
        {
            int context = BackingDspHandle.Native.Prepare(dsp, speed, pitch);
            if (context < 0) throw new InvalidOperationException("DSP configuration failed.");
            ReadInput(context);
            if (BackingDspHandle.Native.Seek(dsp, input, context) != 0) throw new InvalidOperationException("DSP seek failed.");
        }
    }
    private void ReadInput(int frames)
    {
        int count = checked(frames * channels);
        if (input.Length < count) input = new float[count];
        Array.Clear(input, 0, count);
        int done = 0;
        while (done < frames)
        {
            if (inputFrame >= end)
            {
                if (!looping) break;
                inputFrame = loopStart; decoder.Seek(loopStart);
            }
            int want = (int)Math.Min(frames - done, end - inputFrame);
            int got = decoder.Read(input, done * channels, want * channels) / channels;
            if (got == 0)
            {
                if (inputFrame < end - rate / 20) throw new EndOfStreamException("Audio ended before the declared duration.");
                break;
            }
            done += got; inputFrame += got;
        }
    }
    public int Read(float[] buffer, int bufferOffset, int count)
        => Read(buffer.AsSpan(bufferOffset, count));
    public int Read(Span<float> buffer)
    {
        int count = buffer.Length - buffer.Length % channels;
        int written = 0;
        while (written < count)
        {
            if (offset == buffered)
            {
                if (outputFrame >= outputLength)
                {
                    if (!looping) break;
                    Begin(loopStart);
                }
                int frames = (int)Math.Min(1024, outputLength - outputFrame);
                if (frames <= 0) break;
                if (output.Length < frames * channels) output = new float[frames * channels];
                if (dsp == null) { ReadInput(frames); Array.Copy(input, output, frames * channels); }
                else
                {
                    double exact = frames * speed + inputRemainder;
                    int inputFrames = (int)Math.Floor(exact); inputRemainder = exact - inputFrames;
                    ReadInput(inputFrames);
                    if (BackingDspHandle.Native.Process(dsp, input, inputFrames, output, frames) != 0) throw new InvalidOperationException("DSP processing failed.");
                }
                if (looping)
                {
                    double fade = Math.Max(1, rate * .002);
                    for (int f = 0; f < frames; f++)
                    {
                        float gain = (float)Math.Min(1, Math.Min((outputFrame + f) / fade, (outputLength - outputFrame - f - 1) / fade));
                        for (int c = 0; c < channels; c++) output[f * channels + c] *= Math.Max(0, gain);
                    }
                }
                outputFrame += frames; buffered = frames * channels; offset = 0;
            }
            int copy = Math.Min(count - written, buffered - offset);
            output.AsSpan(offset, copy).CopyTo(buffer[written..]);
            written += copy; offset += copy;
        }
        return written;
    }
    public void Dispose() { dsp?.Dispose(); decoder.Dispose(); }

    public static Task<(double Duration, float[] Peaks)> Analyze(string path, CancellationToken cancellation) => Task.Run(() =>
    {
        using var source = new BackingDecoder(path);
        int channels = source.Format.Channels, rate = source.Format.SampleRate;
        // A single envelope is capped as well as the aggregate cache.
        int bins = (int)Math.Clamp(Math.Ceiling(source.Duration * 100), 1, 8 * 1024 * 1024);
        var peaks = new float[bins]; var block = new float[4096 * channels];
        long frame = 0;
        int read;
        while ((read = source.Read(block)) > 0)
        {
            cancellation.ThrowIfCancellationRequested();
            for (int i = 0; i < read; i += channels, frame++)
            {
                float peak = 0;
                for (int c = 0; c < channels; c++) peak = Math.Max(peak, Math.Abs(block[i + c]));
                int bin = (int)Math.Min(bins - 1, frame * bins / Math.Max(1, source.Duration * rate));
                peaks[bin] = Math.Max(peaks[bin], float.IsFinite(peak) ? peak : 0);
            }
        }
        return (frame / (double)rate, peaks);
    }, cancellation);

    public static Task Export(string source, string destination, double start, double end, double speed, double pitch, bool processed, CancellationToken cancellation) => Task.Run(() =>
    {
        string temporary = Path.Combine(Path.GetDirectoryName(destination)!, "." + Path.GetFileName(destination) + "." + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            using (var audio = new BackingAudioProcessing(source, start, end, processed ? speed : 1, processed ? pitch : 0))
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

internal sealed class BackingPeakGuard : ISampleProvider
{
    private readonly ISampleProvider source;
    private readonly float[] ring, block;
    private readonly int channels, window;
    private readonly long[] peakFrames;
    private readonly float[] peakValues;
    private int peakHead, peakCount;
    private long inserted, removed;
    private int blockCount, blockOffset;
    private float gain = 1;
    private readonly float recovery;
    private bool ended;
    public WaveFormat WaveFormat => source.WaveFormat;
    public BackingPeakGuard(ISampleProvider source)
    {
        this.source = source; channels = WaveFormat.Channels;
        window = Math.Max(1, (int)Math.Ceiling(WaveFormat.SampleRate * .005));
        ring = new float[(window + 1) * channels]; block = new float[1024 * channels];
        peakFrames = new long[window + 2]; peakValues = new float[window + 2];
        recovery = (float)(1 - Math.Exp(-1 / (.05 * WaveFormat.SampleRate)));
    }
    private bool Push()
    {
        if (ended) return false;
        if (blockOffset == blockCount) { blockCount = source.Read(block); blockOffset = 0; }
        if (blockCount == 0) { ended = true; return false; }
        int index = (int)(inserted % (window + 1)) * channels; float peak = 0;
        for (int c = 0; c < channels; c++)
        {
            float value = block[blockOffset++];
            if (!float.IsFinite(value)) throw new InvalidDataException("Non-finite DSP sample.");
            ring[index + c] = value; peak = Math.Max(peak, Math.Abs(value));
        }
        while (peakCount > 0 && peakFrames[peakHead] < removed) { peakHead = (peakHead + 1) % peakFrames.Length; peakCount--; }
        while (peakCount > 0 && peakValues[(peakHead + peakCount - 1) % peakFrames.Length] <= peak) peakCount--;
        int tail = (peakHead + peakCount++) % peakFrames.Length;
        peakFrames[tail] = inserted++; peakValues[tail] = peak; return true;
    }
    public int Read(float[] buffer, int offset, int count)
        => Read(buffer.AsSpan(offset, count));
    public int Read(Span<float> buffer)
    {
        int done = 0, count = buffer.Length - buffer.Length % channels;
        while (done < count)
        {
            while (inserted - removed <= window && Push()) { }
            if (inserted == removed) break;
            while (peakCount > 0 && peakFrames[peakHead] < removed) { peakHead = (peakHead + 1) % peakFrames.Length; peakCount--; }
            float peak = peakCount > 0 ? peakValues[peakHead] : 0;
            float target = peak > 1 ? 1 / peak : 1;
            gain = Math.Min(target, gain + (1 - gain) * recovery);
            int index = (int)(removed++ % (window + 1)) * channels;
            for (int c = 0; c < channels; c++) buffer[done++] = ring[index + c] * gain;
        }
        return done;
    }
}
