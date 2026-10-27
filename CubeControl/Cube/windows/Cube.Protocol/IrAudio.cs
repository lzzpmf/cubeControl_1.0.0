using System.Buffers.Binary;

namespace Cube.Protocol;

public sealed record IrAudio(int SampleRate, int Channels, int Bits, string Format, float[] Samples)
{
    public double Duration => (double)Samples.Length / SampleRate;
    public int ResampledLength => Math.Max(1, (int)Math.Round(Samples.Length * 48000d / SampleRate));
    public string Conversion => $"Mono · 48 kHz · 512 amostras · " +
        (ResampledLength > 512 ? $"{ResampledLength - 512} amostras finais descartadas" : $"{512 - ResampledLength} zeros adicionados") + " · sem normalização";

    public static IrAudio Decode(byte[] bytes)
    {
        if (bytes.Length < 44 || bytes.Length > 32 * 1024 * 1024 || !bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8) || !bytes.AsSpan(8, 4).SequenceEqual("WAVE"u8))
            throw new FormatException("Selecione um WAV RIFF válido de até 32 MB.");
        long end = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(4)) + 8L;
        if (end > bytes.Length || end < 44) throw new FormatException("WAV truncado.");
        byte[]? fmt = null, data = null;
        for (long pos = 12; pos + 8 <= end;)
        {
            int offset = (int)pos; uint size = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset + 4));
            if (pos + 8 + size > end) throw new FormatException("Bloco WAV truncado.");
            if (bytes.AsSpan(offset, 4).SequenceEqual("fmt "u8)) fmt = bytes.AsSpan(offset + 8, (int)size).ToArray();
            if (bytes.AsSpan(offset, 4).SequenceEqual("data"u8)) data = bytes.AsSpan(offset + 8, (int)size).ToArray();
            pos += 8 + size + (size & 1);
        }
        if (fmt == null || fmt.Length < 16 || data == null || data.Length == 0) throw new FormatException("WAV sem formato ou amostras.");
        int format = BinaryPrimitives.ReadUInt16LittleEndian(fmt);
        int channels = BinaryPrimitives.ReadUInt16LittleEndian(fmt.AsSpan(2));
        uint rawRate = BinaryPrimitives.ReadUInt32LittleEndian(fmt.AsSpan(4));
        if (rawRate is < 8000 or > 384000) throw new FormatException("Frequência WAV fora do intervalo de 8 a 384 kHz.");
        int rate = (int)rawRate;
        int align = BinaryPrimitives.ReadUInt16LittleEndian(fmt.AsSpan(12));
        int bits = BinaryPrimitives.ReadUInt16LittleEndian(fmt.AsSpan(14));
        if (format == 0xFFFE)
        {
            if (fmt.Length < 40 || BinaryPrimitives.ReadUInt16LittleEndian(fmt.AsSpan(16)) < 22 ||
                !fmt.AsSpan(26, 14).SequenceEqual(new byte[] { 0, 0, 0, 0, 0x10, 0, 0x80, 0, 0, 0xAA, 0, 0x38, 0x9B, 0x71 }))
                throw new FormatException("Subformato WAV extensível não suportado.");
            format = BinaryPrimitives.ReadUInt16LittleEndian(fmt.AsSpan(24));
            int validBits = BinaryPrimitives.ReadUInt16LittleEndian(fmt.AsSpan(18));
            if (validBits == 0 || validBits > bits) throw new FormatException("Resolução WAV extensível inválida.");
        }
        if (channels is < 1 or > 2 || rate is < 8000 or > 384000 ||
            !(format == 1 && bits is 16 or 24 or 32 || format == 3 && bits == 32) || align != channels * bits / 8 || data.Length % align != 0)
            throw new FormatException("Use WAV mono/estéreo PCM 16/24/32 bits ou float 32 bits, entre 8 e 384 kHz.");
        var mono = new float[data.Length / align];
        int width = bits / 8;
        for (int frame = 0; frame < mono.Length; frame++)
        {
            double sum = 0;
            for (int channel = 0; channel < channels; channel++)
            {
                var sample = data.AsSpan(frame * align + channel * width, width);
                double value = format == 3 ? BinaryPrimitives.ReadSingleLittleEndian(sample) : bits switch
                {
                    16 => BinaryPrimitives.ReadInt16LittleEndian(sample) / 32768d,
                    24 => ((sample[0] | sample[1] << 8 | sample[2] << 16) << 8 >> 8) / 8388608d,
                    _ => BinaryPrimitives.ReadInt32LittleEndian(sample) / 2147483648d
                };
                if (!double.IsFinite(value)) throw new FormatException("WAV contém NaN ou infinito.");
                sum += value;
            }
            mono[frame] = (float)(sum / channels);
        }
        return new(rate, channels, bits, format == 3 ? "Float" : "PCM", mono);
    }

    public float[] Prepare()
    {
        var result = new float[512];
        for (int i = 0; i < Math.Min(512, ResampledLength); i++)
        {
            double position = i * SampleRate / 48000d;
            int lo = Math.Min(Samples.Length - 1, (int)position), hi = Math.Min(Samples.Length - 1, lo + 1);
            result[i] = (float)(Samples[lo] + ((double)Samples[hi] - Samples[lo]) * (position - lo));
        }
        return result;
    }

    // CubeSuite 0x4A3770: QDial::value / float 100 at VA 0x584580, written to 0x768.
    public static float DistanceValue(int percent) => percent is >= 0 and <= 100 ? percent / 100f : throw new ArgumentOutOfRangeException(nameof(percent));
    public byte[] LivePayload(int distance)
    {
        var data = new byte[2052]; BinaryPrimitives.WriteSingleLittleEndian(data, DistanceValue(distance));
        var samples = Prepare();
        for (int i = 0; i < samples.Length; i++) BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(4 + i * 4), samples[i]);
        return data;
    }
    public byte[] Sector(int distance)
    {
        var sector = new byte[4096]; // user IR header 0; zero-pad the complete sector, never leave an erased FF tail.
        LivePayload(distance).CopyTo(sector, 4);
        return sector;
    }
}
