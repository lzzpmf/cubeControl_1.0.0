using System.Diagnostics;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Cube.Studio.Audio;

// Single producer (render worker), single consumer (device callback).
internal sealed class AudioRing : ISampleProvider
{
    private sealed class Storage(int samples)
    {
        internal readonly float[] Samples = new float[samples];
        internal long Write, Read;
    }
    private Storage storage;
    private readonly int capacity;
    private readonly StereoPeakGuard peakGuard;
    internal MetronomeClock? Metronome { get; set; }
    internal volatile bool MediaEnabled = true;
    internal float Peak;
    internal long Underruns;
    internal int Count { get { var current = Volatile.Read(ref storage); return (int)(Volatile.Read(ref current.Write) - Volatile.Read(ref current.Read)); } }
    internal int Free => capacity - Count;
    public WaveFormat WaveFormat { get; }
    internal AudioRing(int sampleRate, int frames)
    { WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2); capacity = frames * 2; storage = new(capacity); peakGuard = new(sampleRate); }
    internal void Write(ReadOnlySpan<float> source)
    {
        var current = Volatile.Read(ref storage); var samples = current.Samples;
        long w = current.Write; int count = Math.Min(source.Length, samples.Length - (int)(w - Volatile.Read(ref current.Read)));
        int index = (int)(w % samples.Length), first = Math.Min(count, samples.Length - index);
        source[..first].CopyTo(samples.AsSpan(index)); source.Slice(first, count - first).CopyTo(samples);
        Volatile.Write(ref current.Write, w + count);
    }
    public int Read(Span<float> destination)
    {
        if (MediaEnabled)
        {
            var current = Volatile.Read(ref storage); var samples = current.Samples;
            long r = current.Read; int count = (int)Math.Min(destination.Length, Volatile.Read(ref current.Write) - r);
            int index = (int)(r % samples.Length), first = Math.Min(count, samples.Length - index);
            samples.AsSpan(index, first).CopyTo(destination); samples.AsSpan(0, count - first).CopyTo(destination[first..]);
            if (count < destination.Length) { destination[count..].Clear(); Interlocked.Increment(ref Underruns); }
            Volatile.Write(ref current.Read, r + count);
        }
        else destination.Clear();
        var metronome = Metronome;
        bool click = metronome?.Settings.Enabled == true;
        if (click) metronome!.Add(destination, WaveFormat.SampleRate);
        float peak = 0;
        for (int i = 0; i + 1 < destination.Length; i += 2)
        {
            if (click) peakGuard.Process(ref destination[i], ref destination[i + 1]);
            peak = Math.Max(peak, Math.Max(Math.Abs(destination[i]), Math.Abs(destination[i + 1])));
        }
        Volatile.Write(ref Peak, peak); return destination.Length;
    }
    // Swap on the producer thread. A callback already reading the old buffer finishes safely;
    // the metronome continues on its independent clock without stopping the device.
    internal void Clear() { Volatile.Write(ref storage, new(capacity)); Interlocked.Exchange(ref Underruns, 0); }
}

internal sealed class CapturePacket(int samples)
{
    internal readonly float[] Samples = new float[samples];
    internal int Frames;
    internal double Time;
    internal long DeviceFrame;
    internal bool Discontinuity;
    internal double OutputSeconds = double.NaN;
}

// Packet storage is allocated before capture starts. Consumers release slots after disk writes.
internal sealed class CaptureQueue
{
    private readonly CapturePacket[] packets;
    private readonly int channels, firstChannel;
    private long write, read;
    internal volatile bool Enabled;
    internal int Overflow;
    internal readonly int SampleRate;
    internal int Channels => channels;
    internal CaptureQueue(int rate, int firstChannel, int channels)
    {
        SampleRate = rate; this.channels = channels; this.firstChannel = firstChannel;
        packets = Enumerable.Range(0, 64).Select(_ => new CapturePacket(Math.Max(8192, rate / 4) * channels)).ToArray();
    }
    private CapturePacket? Acquire(int frames)
    {
        if (!Enabled) return null;
        if (write - Volatile.Read(ref read) >= packets.Length || frames * channels > packets[0].Samples.Length)
        { Interlocked.Exchange(ref Overflow, 1); return null; }
        return packets[write % packets.Length];
    }
    internal void Native(ReadOnlySpan<byte> data, WaveFormat format, AudioClientBufferFlags flags, long position, long qpc)
    {
        int frames = data.Length / format.BlockAlign;
        var packet = Acquire(frames); if (packet == null) return;
        bool silent = (flags & AudioClientBufferFlags.Silent) != 0;
        bool floating = format.Encoding == WaveFormatEncoding.IeeeFloat || format is WaveFormatExtensible ext && ext.SubFormat == new Guid("00000003-0000-0010-8000-00aa00389b71");
        int width = format.BitsPerSample / 8;
        for (int f = 0; f < frames; f++) for (int c = 0; c < channels; c++)
        {
            var bytes = data.Slice(f * format.BlockAlign + (firstChannel + c) * width, width);
            float value = silent ? 0 : floating && width == 4 ? BitConverter.ToSingle(bytes) : width switch
            {
                2 => System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(bytes) / 32768f,
                3 => ((bytes[0] | bytes[1] << 8 | bytes[2] << 16) << 8 >> 8) / 8388608f,
                4 => System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(bytes) / 2147483648f,
                _ => 0
            };
            packet.Samples[f * channels + c] = float.IsFinite(value) ? value : 0;
        }
        packet.Frames = frames; packet.DeviceFrame = (long)position;
        packet.Time = qpc > 0 && (flags & AudioClientBufferFlags.TimestampError) == 0 ? qpc / 1e7 : ClockSeconds - frames / (double)format.SampleRate;
        packet.Discontinuity = (flags & AudioClientBufferFlags.DataDiscontinuity) != 0;
        packet.OutputSeconds = double.NaN;
        Volatile.Write(ref write, write + 1);
    }
    internal void Asio(in AsioProcessBuffers buffers, double outputSeconds)
    {
        var packet = Acquire(buffers.Frames); if (packet == null) return;
        for (int c = 0; c < channels; c++)
        {
            var input = buffers.GetInput(c);
            for (int f = 0; f < buffers.Frames; f++) packet.Samples[f * channels + c] = float.IsFinite(input[f]) ? input[f] : 0;
        }
        packet.Frames = buffers.Frames; packet.DeviceFrame = buffers.SamplePosition;
        packet.Time = buffers.SystemTimeNanoseconds > 0 ? buffers.SystemTimeNanoseconds / 1e9 : ClockSeconds;
        packet.OutputSeconds = outputSeconds;
        packet.Discontinuity = false; Volatile.Write(ref write, write + 1);
    }
    internal CapturePacket? Peek() => read < Volatile.Read(ref write) ? packets[read % packets.Length] : null;
    internal void Release() => Volatile.Write(ref read, read + 1);
    internal void Discard() { while (Peek() != null) Release(); Overflow = 0; }
    internal static double ClockSeconds => Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency;
}
