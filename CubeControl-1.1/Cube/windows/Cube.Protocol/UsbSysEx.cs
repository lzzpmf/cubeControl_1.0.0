namespace Cube.Protocol;

// Cube Baby USB packs the entire 00 59 envelope as a little-endian bitstream of septets.
public static class UsbSysEx
{
    public static byte[] Encode(CubePacket packet)
    {
        List<byte> result = [0xF0];
        uint accumulator = 0;
        int bits = 0;
        foreach (byte b in packet.Encode())
        {
            accumulator |= (uint)b << bits;
            bits += 8;
            while (bits >= 7) { result.Add((byte)(accumulator & 127)); accumulator >>= 7; bits -= 7; }
        }
        if (bits > 0) result.Add((byte)(accumulator & 127)); // Retain zero padding.
        result.Add(0xF7);
        return result.ToArray();
    }

    public static CubePacket Decode(ReadOnlySpan<byte> message)
    {
        if (message.Length < 4 || message[0] != 0xF0 || message[^1] != 0xF7)
            throw new FormatException("Mensagem SysEx incompleta.");
        List<byte> clear = [];
        uint accumulator = 0;
        int bits = 0;
        foreach (byte b in message[1..^1])
        {
            if (b > 127) throw new FormatException("Byte inválido dentro do SysEx.");
            accumulator |= (uint)b << bits;
            bits += 7;
            if (bits >= 8) { clear.Add((byte)(accumulator & 255)); accumulator >>= 8; bits -= 8; }
        }
        if (accumulator != 0 || clear.Count < 7 || clear[0] != 0 || clear[1] != 0x59)
            throw new FormatException("Envelope Cube Baby inválido.");
        var bytes = clear.ToArray();
        int size = CubePacket.Read24(bytes.AsSpan(3));
        if (size + 7 != bytes.Length || message.Length != 2 + (bytes.Length * 8 + 6) / 7)
            throw new FormatException("Tamanho do envelope inválido.");
        byte[] payload = bytes[6..^1];
        if (CubePacket.Checksum(payload) != bytes[^1]) throw new FormatException("Checksum inválido.");
        return new(bytes[2], payload);
    }
}

public sealed class SysExAssembler
{
    private readonly List<byte> buffer = [];
    public void Reset() => buffer.Clear();
    public IReadOnlyList<byte[]> Feed(ReadOnlySpan<byte> bytes)
    {
        List<byte[]> messages = [];
        foreach (byte b in bytes)
        {
            if (b >= 0xF8) continue; // Real-time MIDI may be interleaved.
            if (b == 0xF0) { buffer.Clear(); buffer.Add(b); continue; }
            if (buffer.Count == 0) continue;
            if (b == 0xF7) { buffer.Add(b); messages.Add(buffer.ToArray()); buffer.Clear(); }
            else if (b >= 0x80 || buffer.Count >= 131072) buffer.Clear();
            else buffer.Add(b);
        }
        return messages;
    }
}
