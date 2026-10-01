namespace Cube.Protocol;

// Per-session parser for standard length-prefixed envelopes, not an assumed USB codec.
public sealed class PacketParser(int maximumPayload = 65536)
{
    private readonly List<byte> buffer = [];
    private readonly int limit = maximumPayload is >= 8 and <= 0xFFFFFF
        ? maximumPayload : throw new ArgumentOutOfRangeException(nameof(maximumPayload));
    public int InvalidFrames { get; private set; }
    public void Reset() => buffer.Clear();

    public IReadOnlyList<CubePacket> Feed(ReadOnlySpan<byte> bytes)
    {
        List<CubePacket> result = [];
        foreach (var b in bytes)
        {
            buffer.Add(b);
            while (buffer.Count >= 2)
            {
                if (buffer[0] != 0 || buffer[1] != 0x59) { buffer.RemoveAt(0); continue; }
                if (buffer.Count < 6) break;
                int length = buffer[3] | buffer[4] << 8 | buffer[5] << 16;
                if (length > limit) { InvalidFrames++; buffer.RemoveAt(0); continue; }
                if (buffer.Count < length + 7) break;
                var payload = buffer.GetRange(6, length).ToArray();
                if (CubePacket.Checksum(payload) != buffer[length + 6])
                { InvalidFrames++; buffer.RemoveAt(0); continue; }
                result.Add(new(buffer[2], payload));
                buffer.RemoveRange(0, length + 7);
            }
        }
        return result;
    }
}
