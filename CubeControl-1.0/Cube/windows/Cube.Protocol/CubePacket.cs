using System.Buffers.Binary;

namespace Cube.Protocol;

// Shared Cube envelope. USB carries this through UsbSysEx's 7-bit packing.
public sealed record CubePacket(byte Command, byte[] Payload)
{
    public byte[] Encode()
    {
        if (Payload.Length > 0xFFFFFF) throw new ArgumentOutOfRangeException(nameof(Payload));
        var bytes = new byte[Payload.Length + 7];
        bytes[1] = 0x59;
        bytes[2] = Command;
        Write24(bytes.AsSpan(3), Payload.Length);
        Payload.CopyTo(bytes, 6);
        bytes[^1] = Checksum(Payload);
        return bytes;
    }

    public static CubePacket Identify() => new(0x11, []);
    public static CubePacket Midi(ReadOnlySpan<byte> message) => new(0x15, message.ToArray());

    internal static CubePacket EraseIrSector(uint address)
    {
        if (address < 0x69000 || address > 0x70000 || address % 4096 != 0) throw new ArgumentOutOfRangeException(nameof(address));
        var payload = new byte[5];
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(1), address);
        return new(0x21, payload);
    }

    internal static CubePacket Register(uint address, uint value)
    {
        if (address is not (0x764 or 0x768)) throw new ArgumentOutOfRangeException(nameof(address));
        var payload = new byte[9]; payload[0] = 4;
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(1), address);
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(5), value);
        return new(0x24, payload);
    }

    public static CubePacket Read(byte region, uint address, int length)
        => new(0x23, MemoryPayload(region, address, length));

    public static CubePacket Write(byte region, uint address, ReadOnlySpan<byte> data)
    {
        var payload = new byte[8 + data.Length];
        MemoryPayload(region, address, data.Length).CopyTo(payload, 0);
        data.CopyTo(payload.AsSpan(8));
        return new(0x22, payload);
    }

    private static byte[] MemoryPayload(byte region, uint address, int length)
    {
        if (region > 7) throw new ArgumentOutOfRangeException(nameof(region));
        if (length < 0 || length > 0xFFFFFF) throw new ArgumentOutOfRangeException(nameof(length));
        if ((ulong)address + (ulong)length > (ulong)uint.MaxValue + 1)
            throw new ArgumentOutOfRangeException(nameof(address));
        var payload = new byte[8];
        payload[0] = region;
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(1), address);
        Write24(payload.AsSpan(5), length);
        return payload;
    }

    public static byte Checksum(ReadOnlySpan<byte> bytes)
    {
        var sum = 0;
        foreach (var b in bytes) sum = (sum + b) & 255;
        return (byte)(sum ^ 255);
    }

    internal static int Read24(ReadOnlySpan<byte> bytes) => bytes[0] | bytes[1] << 8 | bytes[2] << 16;
    private static void Write24(Span<byte> bytes, int value)
    {
        bytes[0] = (byte)value;
        bytes[1] = (byte)(value >> 8);
        bytes[2] = (byte)(value >> 16);
    }
}
