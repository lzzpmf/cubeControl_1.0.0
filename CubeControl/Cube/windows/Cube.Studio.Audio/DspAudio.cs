using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using NAudio.Wave;

namespace Cube.Studio.Audio;

internal sealed class DspHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private DspHandle() : base(true) { }
    protected override bool ReleaseHandle() { Destroy(handle); return true; }
    private const string Library = "CubeStudioDsp";
    [DllImport(Library, EntryPoint = "backing_create", CallingConvention = CallingConvention.Cdecl)]
    internal static extern DspHandle Create(int channels, int sampleRate);
    [DllImport(Library, EntryPoint = "backing_destroy", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Destroy(IntPtr handle);
    [DllImport(Library, EntryPoint = "backing_prepare", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Prepare(DspHandle handle, double speed, double pitch);
    [DllImport(Library, EntryPoint = "backing_seek", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Seek(DspHandle handle, float[] samples, int frames);
    [DllImport(Library, EntryPoint = "backing_process", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Process(DspHandle handle, float[] input, int inputFrames, [Out] float[] output, int outputFrames);
    [DllImport(Library, EntryPoint = "backing_flush_speed", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int Flush(DspHandle handle, [Out] float[] output, int frames, double speed);
}

// Runs only on a render worker, never on a hardware callback.
internal sealed class DspAudio : ISampleProvider, IDisposable
{
    private readonly StudioDecoder decoder;
    private readonly DspHandle? dsp;
    private readonly int channels;
    private readonly double speed;
    private readonly long end, length;
    private long sourceFrame, rendered;
    private long regularLength;
    private float[] input = [], output = [];
    private int available, consumed;
    private double remainder;
    public WaveFormat WaveFormat => decoder.Format;

    internal DspAudio(string path, double start, double endSeconds, double speed, double pitch)
    {
        if (!double.IsFinite(speed) || speed is < .009 or > 101 || !double.IsFinite(pitch) || Math.Abs(pitch) > 24)
            throw new ArgumentOutOfRangeException(nameof(speed), "Ajustes de áudio fora dos limites da sessão.");
        decoder = new(path);
        try
        {
            this.speed = speed; channels = WaveFormat.Channels;
            sourceFrame = (long)Math.Round(Math.Max(0, start) * WaveFormat.SampleRate);
            end = (long)Math.Round(Math.Min(decoder.Duration, endSeconds) * WaveFormat.SampleRate);
            length = Math.Max(0, (long)Math.Round((end - sourceFrame) / speed));
            decoder.Seek(sourceFrame);
            if (Math.Abs(speed - 1) > 1e-9 || Math.Abs(pitch) > 1e-9)
            {
                dsp = DspHandle.Create(channels, WaveFormat.SampleRate);
                if (dsp.IsInvalid) throw new InvalidOperationException("Não foi possível preparar o processamento de áudio.");
                int context = DspHandle.Prepare(dsp, speed, pitch);
                if (context < 0) throw new InvalidOperationException("Configuração de áudio inválida.");
                ReadInput(context);
                if (DspHandle.Seek(dsp, input, context) != 0) throw new IOException("Falha ao posicionar o áudio.");
                regularLength = Math.Max(0, (long)Math.Round((end - sourceFrame) / speed));
            }
        }
        catch { Dispose(); throw; }
    }

    private void ReadInput(int frames)
    {
        int count = checked(frames * channels);
        if (input.Length < count) input = new float[count];
        input.AsSpan(0, count).Clear();
        int wanted = (int)Math.Min(frames, Math.Max(0, end - sourceFrame));
        int done = 0;
        while (done < wanted)
        {
            int read = decoder.Read(input, done * channels, (wanted - done) * channels) / channels;
            if (read == 0) break;
            done += read;
        }
        sourceFrame += done;
    }

    public int Read(Span<float> samples)
    {
        int written = 0, wanted = samples.Length - samples.Length % channels;
        while (written < wanted)
        {
            if (available == consumed)
            {
                int frames = (int)Math.Min(1024, length - rendered);
                if (frames <= 0) break;
                bool flush = dsp != null && rendered >= regularLength;
                if (dsp != null) frames = (int)Math.Min(int.MaxValue / channels, flush ? length - rendered : Math.Min(frames, regularLength - rendered));
                if (output.Length < frames * channels) output = new float[frames * channels];
                if (dsp == null) { ReadInput(frames); input.AsSpan(0, frames * channels).CopyTo(output); }
                else if (flush)
                {
                    if (DspHandle.Flush(dsp, output, frames, speed) != 0) throw new IOException("Falha ao concluir o processamento de áudio.");
                }
                else
                {
                    double exact = frames * speed + remainder;
                    int inputFrames = (int)Math.Floor(exact); remainder = exact - inputFrames;
                    ReadInput(inputFrames);
                    if (DspHandle.Process(dsp, input, inputFrames, output, frames) != 0) throw new IOException("Falha no processamento de áudio.");
                }
                rendered += frames; consumed = 0; available = frames * channels;
            }
            int count = Math.Min(wanted - written, available - consumed);
            output.AsSpan(consumed, count).CopyTo(samples[written..]);
            consumed += count; written += count;
        }
        return written;
    }
    public void Dispose() { dsp?.Dispose(); decoder.Dispose(); }
}
