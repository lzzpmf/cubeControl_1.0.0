using System.IO;
using NAudio.Wave;

namespace Cube.Desktop.Services;

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
    public BackingAudioProcessing(string path, double start, double end, double speed, double pitch, bool looping = false, double loopStart = 0, bool relativeParameters = false, bool offline = false)
    {
        decoder = new(path);
        try
        {
            channels = WaveFormat.Channels; rate = WaveFormat.SampleRate;
            this.speed = Math.Clamp(speed, relativeParameters ? .32 : .5, relativeParameters ? 3.07 : 1.5); this.pitch = Math.Clamp(pitch, relativeParameters ? -12 : -6, relativeParameters ? 12 : 6);
            this.looping = looping;
            this.end = (long)Math.Round(Math.Min(end, Duration) * rate);
            this.loopStart = (long)Math.Round(Math.Max(0, loopStart) * rate);
            if (this.end <= 0 || (looping && this.end - this.loopStart < rate / 10)) throw new InvalidDataException("Invalid audio range.");
            if (this.speed != 1 || this.pitch != 0)
            {
                dsp = offline ? BackingDspHandle.Native.CreateOffline(channels, rate) : BackingDspHandle.Native.Create(channels, rate);
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

    public static Task<(double Duration, float[] Peaks)> Analyze(string path, CancellationToken cancellation, int maximumBins = 8 * 1024 * 1024)
        => BackingAudioAnalysis.Analyze(path, cancellation, maximumBins);

    public static Task Export(string source, string destination, double start, double end, double speed, double pitch, bool processed, CancellationToken cancellation)
        => BackingAudioExporter.Export(source, destination, start, end, speed, pitch, processed, cancellation);
}
